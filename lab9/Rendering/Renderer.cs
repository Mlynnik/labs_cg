using lab6.Math3d;
using lab6.Model;
using lab6.Rendering;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace lab6.Rendering
{
    public enum ProjectionMode { Perspective, Axonometric }
    public enum RenderMode { Wireframe, Filled, FilledWireframe }

    public class Renderer
    {
        public int W { get; private set; }
        public int H { get; private set; }
        public int[] Color;
        public Bitmap Bitmap;
        double[] _depth;

        // конфигурация сцены
        public Camera Camera = new Camera();
        public ProjectionMode Projection = ProjectionMode.Perspective;
        public RenderMode Mode = RenderMode.FilledWireframe;
        public bool CullBackFaces = true;
        public bool UseZBuffer = true;
        public Mat4 SceneTransform = Mat4.Identity;     // общая анимация (вращение сцены)
        public double OrthoHalfSize = 2.0;              // для аксонометрии

        // цвета / свет
        public Vec3 LightDir = new Vec3(0.4, 0.7, 0.6);
        public int BackgroundColor = unchecked((int)0xFFF2F2F2);
        public int WireColor = unchecked((int)0xFF0D1B4A);

        public Renderer(int w, int h) { Resize(w, h); }

        public void Resize(int w, int h)
        {
            W = w; H = h;
            Color = new int[w * h];
            _depth = new double[w * h];
            Bitmap?.Dispose();
            Bitmap = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        }

        public void Clear(int argb)
        {
            for (int i = 0; i < Color.Length; i++) Color[i] = argb;
            for (int i = 0; i < _depth.Length; i++) _depth[i] = double.PositiveInfinity;
        }

        // ============ ГЛАВНЫЙ ВХОД ============
        public void RenderScene(List<SceneObject> objects)
        {
            var view = Camera.ViewMatrix;
            double aspect = (double)W / H;
            var proj = Projection == ProjectionMode.Perspective
                ? Mat4.Perspective(Camera.FovDeg, aspect, Camera.Near, Camera.Far)
                : Mat4.Orthographic(OrthoHalfSize, aspect, Camera.Near, Camera.Far);
            var mvp = proj * view;
            var viewDir = (Camera.Target - Camera.Eye).Normalized();

            if (Mode != RenderMode.Wireframe)
                foreach (var obj in objects)
                    RenderObjectFill(obj, view, mvp, viewDir);

            if (Mode != RenderMode.Filled)
                foreach (var obj in objects)
                    RenderObjectWire(obj, view, mvp, viewDir);
        }

        // ============ ЗАЛИВКА (Z-буфер) ============
        void RenderObjectFill(SceneObject obj, Mat4 view, Mat4 mvp, Vec3 viewDir)
        {
            var world = SceneTransform * obj.ModelMatrix;
            foreach (var face in obj.Model.Faces)
            {
                int n = face.Vertices.Count;
                if (n < 3) continue;

                var worldNormal = world.TransformDirection(face.Normal).Normalized();
                var worldCentroid = FaceCentroid(obj.Model, face, world);

                if (CullBackFaces && !IsFrontFacing(worldNormal, worldCentroid, viewDir))
                    continue;

                var sv = new ScreenVertex[n];
                bool ok = true;
                for (int i = 0; i < n; i++)
                {
                    var w = world.TransformPoint(obj.Model.Vertices[face.Vertices[i].V]);
                    if (!ProjectToScreen(w, mvp, out sv[i])) { ok = false; break; }
                }
                if (!ok) continue;

                int color = ShadeColor(worldNormal, obj.BaseColor);
                for (int i = 1; i < n - 1; i++)
                    RasterizeTriangle(sv[0], sv[i], sv[i + 1], color);
            }
        }

        // КАРКАС
        void RenderObjectWire(SceneObject obj, Mat4 view, Mat4 mvp, Vec3 viewDir)
        {
            var world = SceneTransform * obj.ModelMatrix;
            foreach (var face in obj.Model.Faces)
            {
                int n = face.Vertices.Count;
                if (n < 2) continue;

                if (CullBackFaces)
                {
                    var nrm = world.TransformDirection(face.Normal).Normalized();
                    var c = FaceCentroid(obj.Model, face, world);
                    if (!IsFrontFacing(nrm, c, viewDir)) continue;
                }

                var sv = new ScreenVertex[n];
                bool ok = true;
                for (int i = 0; i < n; i++)
                {
                    var w = world.TransformPoint(obj.Model.Vertices[face.Vertices[i].V]);
                    if (!ProjectToScreen(w, mvp, out sv[i])) { ok = false; break; }
                }
                if (!ok) continue;

                for (int i = 0; i < n; i++)
                {
                    var a = sv[i]; var b = sv[(i + 1) % n];
                    DrawLine(a.X, a.Y, b.X, b.Y, WireColor);
                }
            }
        }

        // ОТСЕЧЕНИЕ
        bool IsFrontFacing(Vec3 worldNormal, Vec3 worldCentroid, Vec3 viewDir)
        {
            if (Projection == ProjectionMode.Perspective)
            {
                var toCam = Camera.Eye - worldCentroid;
                return Vec3.Dot(worldNormal, toCam) > 0;
            }
            // Параллельная: нормаль должна смотреть навстречу направлению обзора
            return Vec3.Dot(worldNormal, viewDir) < 0;
        }

        // ПРОЕКЦИЯ ВЕРШИНЫ
        struct ScreenVertex { public double X, Y, Z; }

        bool ProjectToScreen(Vec3 world, Mat4 mvp, out ScreenVertex sv)
        {
            sv = default;
            var (cx, cy, cz, cw) = mvp.TransformPointH(world);
            if (cw <= 1e-6) return false;                 // за камерой / на плоскости камеры
            double nx = cx / cw, ny = cy / cw, nz = cz / cw;
            sv.X = (nx + 1) * 0.5 * W;
            sv.Y = (1 - ny) * 0.5 * H;
            sv.Z = nz;                                     // NDC z ∈ [-1, 1], меньше = ближе
            return true;
        }

        // РАСТЕРИЗАЦИЯ ТРЕУГОЛЬНИКА
        void RasterizeTriangle(ScreenVertex a, ScreenVertex b, ScreenVertex c, int color)
        {
            double det = (b.Y - c.Y) * (a.X - c.X) + (c.X - b.X) * (a.Y - c.Y);
            if (Math.Abs(det) < 1e-9) return;
            double invDet = 1.0 / det;

            int minX = Math.Max(0, (int)Math.Floor(Math.Min(a.X, Math.Min(b.X, c.X))));
            int maxX = Math.Min(W - 1, (int)Math.Ceiling(Math.Max(a.X, Math.Max(b.X, c.X))));
            int minY = Math.Max(0, (int)Math.Floor(Math.Min(a.Y, Math.Min(b.Y, c.Y))));
            int maxY = Math.Min(H - 1, (int)Math.Ceiling(Math.Max(a.Y, Math.Max(b.Y, c.Y))));

            for (int y = minY; y <= maxY; y++)
            {
                double py = y + 0.5;
                for (int x = minX; x <= maxX; x++)
                {
                    double px = x + 0.5;
                    double l1 = ((b.Y - c.Y) * (px - c.X) + (c.X - b.X) * (py - c.Y)) * invDet;
                    double l2 = ((c.Y - a.Y) * (px - c.X) + (a.X - c.X) * (py - c.Y)) * invDet;
                    double l3 = 1 - l1 - l2;
                    if (l1 < 0 || l2 < 0 || l3 < 0) continue;

                    double z = l1 * a.Z + l2 * b.Z + l3 * c.Z;
                    int idx = y * W + x;
                    if (UseZBuffer ? (z < _depth[idx]) : true)
                    {
                        _depth[idx] = z;
                        Color[idx] = color;
                    }
                }
            }
        }

        // ОСВЕЩЕНИЕ
        int ShadeColor(Vec3 worldNormal, Vec3 baseColor)
        {
            var n = worldNormal.Normalized();
            var l = LightDir.Normalized();
            double d = Vec3.Dot(n, l);
            double diff = CullBackFaces ? Math.Max(0, d) : Math.Abs(d);
            double ambient = 0.35;
            double k = ambient + (1 - ambient) * diff;
            int r = (int)Math.Clamp(baseColor.X * 255 * k, 0, 255);
            int g = (int)Math.Clamp(baseColor.Y * 255 * k, 0, 255);
            int b = (int)Math.Clamp(baseColor.Z * 255 * k, 0, 255);
            return unchecked((int)0xFF000000) | (r << 16) | (g << 8) | b;
        }

        // УТИЛИТЫ
        static Vec3 FaceCentroid(Polyhedron m, Face f, Mat4 world)
        {
            var c = Vec3.Zero;
            foreach (var fv in f.Vertices) c += world.TransformPoint(m.Vertices[fv.V]);
            return c * (1.0 / f.Vertices.Count);
        }

        void DrawLine(double x0, double y0, double x1, double y1, int argb)
        {
            int ix0 = (int)Math.Round(x0), iy0 = (int)Math.Round(y0);
            int ix1 = (int)Math.Round(x1), iy1 = (int)Math.Round(y1);
            int dx = Math.Abs(ix1 - ix0), sx = ix0 < ix1 ? 1 : -1;
            int dy = -Math.Abs(iy1 - iy0), sy = iy0 < iy1 ? 1 : -1;
            int err = dx + dy;
            while (true)
            {
                if (ix0 >= 0 && iy0 >= 0 && ix0 < W && iy0 < H)
                    Color[iy0 * W + ix0] = argb;
                if (ix0 == ix1 && iy0 == iy1) break;
                int e2 = 2 * err;
                if (e2 >= dy) { err += dy; ix0 += sx; }
                if (e2 <= dx) { err += dx; iy0 += sy; }
            }
        }

        public void Present()
        {
            var data = Bitmap.LockBits(new Rectangle(0, 0, W, H),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(Color, 0, data.Scan0, Color.Length);
            Bitmap.UnlockBits(data);
        }
    }
}