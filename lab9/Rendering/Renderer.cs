using lab6.Math3d;
using lab6.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace lab6.Rendering
{
    public enum ProjectionMode { Perspective, Axonometric }
    public enum RenderMode { Wireframe, Filled, FilledWireframe }
    public enum ShadingMode { Gouraud, PhongToon, Textured }


    public partial class Renderer
    {
        public int W { get; private set; }
        public int H { get; private set; }
        public int[] Color;
        public Bitmap Bitmap;

        private double[] _depth;

        public Camera Camera = new Camera();
        public ProjectionMode Projection = ProjectionMode.Perspective;
        public RenderMode Mode = RenderMode.FilledWireframe;
        public ShadingMode Shading = ShadingMode.Gouraud;

        public bool CullBackFaces = true;
        public bool UseZBuffer = true;
        public Mat4 SceneTransform = Mat4.Identity;
        public double OrthoHalfSize = 2.0;

        public Vec3 LightDir = new Vec3(0.4, 0.7, 0.6);
        public Vec3 LightPosition = new Vec3(4, 5, 7);
        public bool PositionalLight = true;
        public double Ambient = 0.18;
        public int ToonBands = 4;

        public int BackgroundColor = unchecked((int)0xFFF2F2F2);
        public int WireColor = unchecked((int)0xFF0D1B4A);

        public Renderer(int w, int h)
        {
            Resize(w, h);
        }

        public void Resize(int w, int h)
        {
            W = Math.Max(1, w);
            H = Math.Max(1, h);
            Color = new int[W * H];
            _depth = new double[W * H];

            Bitmap?.Dispose();
            Bitmap = new Bitmap(W, H, PixelFormat.Format32bppArgb);
        }

        public void Clear(int argb)
        {
            Array.Fill(Color, argb);
            Array.Fill(_depth, double.PositiveInfinity);
        }

        public void RenderScene(List<SceneObject> objects)
        {
            Mat4 view = Camera.ViewMatrix;
            double aspect = (double)W / H;
            Mat4 projection = Projection == ProjectionMode.Perspective
                ? Mat4.Perspective(Camera.FovDeg, aspect, Camera.Near, Camera.Far)
                : Mat4.Orthographic(OrthoHalfSize, aspect, Camera.Near, Camera.Far);

            Mat4 viewProjection = projection * view;
            Vec3 viewDir = (Camera.Target - Camera.Eye).Normalized();

            if (Mode != RenderMode.Wireframe)
            {
                foreach (SceneObject obj in objects) RenderObjectFill(obj, viewProjection, viewDir);
            }

            if (Mode != RenderMode.Filled)
            {
                foreach (SceneObject obj in objects) RenderObjectWire(obj, viewProjection, viewDir);
            }
        }

        // Все атрибуты, которые требуется интерполировать внутри треугольника.
        private struct ScreenVertex
        {
            public double X, Y, Z, InvW;
            public double U, V;
            public Vec3 Normal, World, Lit;
        }

        private bool ProjectToScreen(Vec3 world, Mat4 viewProjection, out ScreenVertex screen)
        {
            screen = default;
            var (cx, cy, cz, cw) = viewProjection.TransformPointH(world);
            if (cw <= 1e-6) return false;

            double x = cx / cw;
            double y = cy / cw;
            screen.X = (x + 1) * 0.5 * W;
            screen.Y = (1 - y) * 0.5 * H;
            screen.Z = cz / cw;
            screen.InvW = 1.0 / cw;

            return double.IsFinite(screen.X) && double.IsFinite(screen.Y) && double.IsFinite(screen.Z);
        }

        private static Vec3 FaceCentroid(Polyhedron model, Face face, Mat4 world)
        {
            Vec3 center = Vec3.Zero;
            foreach (FaceVertex fv in face.Vertices) center += world.TransformPoint(model.Vertices[fv.V]);
            return center * (1.0 / face.Vertices.Count);
        }

        private bool IsFrontFacing(Vec3 normal, Vec3 center, Vec3 viewDir)
        {
            if (Projection == ProjectionMode.Perspective) return Vec3.Dot(normal, Camera.Eye - center) > 0;
            return Vec3.Dot(normal, viewDir) < 0;
        }

        // Используем inverse-transpose, чтобы нормали оставались корректными при масштабе.
        private static Vec3 TransformNormal(Mat4 m, Vec3 normal)
        {
            double a = m.M[0, 0], b = m.M[0, 1], c = m.M[0, 2];
            double d = m.M[1, 0], e = m.M[1, 1], f = m.M[1, 2];
            double g = m.M[2, 0], h = m.M[2, 1], i = m.M[2, 2];
            double det = a * (e * i - f * h) - b * (d * i - f * g) + c * (d * h - e * g);

            if (Math.Abs(det) < 1e-12) return m.TransformDirection(normal).Normalized();

            return new Vec3(
                ((e * i - f * h) * normal.X + (f * g - d * i) * normal.Y + (d * h - e * g) * normal.Z) / det,
                ((c * h - b * i) * normal.X + (a * i - c * g) * normal.Y + (b * g - a * h) * normal.Z) / det,
                ((b * f - c * e) * normal.X + (c * d - a * f) * normal.Y + (a * e - b * d) * normal.Z) / det
            ).Normalized();
        }

        // Нормали углов: значения из OBJ имеют приоритет. Иначе усредняем смежные грани.
        // Порог 60° сохраняет острые ребра куба/тетраэдра.
        private static Vec3[][] BuildCornerNormals(Polyhedron model)
        {
            var incident = new List<int>[model.Vertices.Count];
            for (int i = 0; i < incident.Length; i++) incident[i] = new List<int>();

            for (int fi = 0; fi < model.Faces.Count; fi++)
            {
                foreach (FaceVertex fv in model.Faces[fi].Vertices)
                {
                    if (fv.V >= 0 && fv.V < incident.Length) incident[fv.V].Add(fi);
                }
            }

            var normals = new Vec3[model.Faces.Count][];
            const double creaseCos = 0.5;

            for (int fi = 0; fi < model.Faces.Count; fi++)
            {
                Face face = model.Faces[fi];
                normals[fi] = new Vec3[face.Vertices.Count];

                for (int k = 0; k < face.Vertices.Count; k++)
                {
                    FaceVertex fv = face.Vertices[k];
                    if (fv.N >= 0 && fv.N < model.Normals.Count)
                    {
                        normals[fi][k] = model.Normals[fv.N].Normalized();
                        continue;
                    }

                    Vec3 sum = Vec3.Zero;
                    foreach (int other in incident[fv.V])
                    {
                        Vec3 neighborNormal = model.Faces[other].Normal;
                        if (Vec3.Dot(face.Normal, neighborNormal) >= creaseCos) sum += neighborNormal;
                    }

                    normals[fi][k] = sum.Length > 1e-9 ? sum.Normalized() : face.Normal;
                }
            }

            return normals;
        }

        // Свет
        private Vec3 LightVector(Vec3 position)
        {
            return PositionalLight ? (LightPosition - position).Normalized() : LightDir.Normalized();
        }

        private double Diffuse(Vec3 normal, Vec3 position)
        {
            return Math.Max(0, Vec3.Dot(normal.Normalized(), LightVector(position)));
        }

        private static int Pack(Vec3 color)
        {
            int r = (int)Math.Clamp(color.X * 255, 0, 255);
            int g = (int)Math.Clamp(color.Y * 255, 0, 255);
            int b = (int)Math.Clamp(color.Z * 255, 0, 255);
            return unchecked((int)0xFF000000) | (r << 16) | (g << 8) | b;
        }

        private static Vec3 Unpack(int argb)
        {
            return new Vec3(((argb >> 16) & 255) / 255.0, ((argb >> 8) & 255) / 255.0, (argb & 255) / 255.0);
        }

        private void RenderObjectFill(SceneObject obj, Mat4 viewProjection, Vec3 viewDir)
        {
            if (obj.Model == null) return;

            Polyhedron model = obj.Model;
            Mat4 world = SceneTransform * obj.ModelMatrix;
            Vec3[][] cornerNormals = BuildCornerNormals(model);

            for (int fi = 0; fi < model.Faces.Count; fi++)
            {
                Face face = model.Faces[fi];
                int n = face.Vertices.Count;
                if (n < 3) continue;

                var uv = Shading == ShadingMode.Textured ? GetUVs(model, face) : null;

                Vec3 normal = TransformNormal(world, face.Normal);
                Vec3 center = FaceCentroid(model, face, world);
                if (CullBackFaces && !IsFrontFacing(normal, center, viewDir)) continue;
                var vertices = new ScreenVertex[n];
                bool projected = true;

                for (int k = 0; k < n; k++)
                {
                    Vec3 point = world.TransformPoint(model.Vertices[face.Vertices[k].V]);
                    if (!ProjectToScreen(point, viewProjection, out vertices[k]))
                    {
                        projected = false;
                        break;
                    }

                    vertices[k].World = point;
                    vertices[k].Normal = TransformNormal(world, cornerNormals[fi][k]);

                    if (Shading == ShadingMode.Gouraud)
                    {
                        vertices[k].Lit = Lambert(vertices[k].Normal, point, obj.BaseColor);
                    }

                    if (uv != null)
                    {
                        vertices[k].U = uv[k].u;
                        vertices[k].V = uv[k].v;
                    }
                }

                if (!projected) continue;

                for (int k = 1; k < n - 1; k++) RasterizeTriangle(vertices[0], vertices[k], vertices[k + 1], obj.BaseColor, vertices, k);
            }
        }

        // Z-буфер и барицентрическая растеризация
        private void RasterizeTriangle(ScreenVertex a, ScreenVertex b, ScreenVertex c, Vec3 baseColor, ScreenVertex[] face, int triangleIndex)
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
                for (int x = minX; x <= maxX; x++)
                {
                    double px = x + 0.5;
                    double py = y + 0.5;
                    double l0 = ((b.Y - c.Y) * (px - c.X) + (c.X - b.X) * (py - c.Y)) * invDet;
                    double l1 = ((c.Y - a.Y) * (px - c.X) + (a.X - c.X) * (py - c.Y)) * invDet;
                    double l2 = 1.0 - l0 - l1;
                    if (l0 < -1e-9 || l1 < -1e-9 || l2 < -1e-9) continue;

                    double z = l0 * a.Z + l1 * b.Z + l2 * c.Z;
                    if (z < -1 || z > 1) continue;

                    int index = y * W + x;
                    if (UseZBuffer && z >= _depth[index]) continue;
                    double w0 = l0 * a.InvW;
                    double w1 = l1 * b.InvW;
                    double w2 = l2 * c.InvW;
                    double sum = w0 + w1 + w2;
                    if (Math.Abs(sum) < 1e-15) continue;

                    w0 /= sum;
                    w1 /= sum;
                    w2 /= sum;

                    Vec3 pixelColor;
                    switch (Shading)
                    {
                        case ShadingMode.Gouraud:
                            pixelColor = ShadeGouraudPixel(a, b, c, face, triangleIndex, w0, w1, w2);
                            break;
                        case ShadingMode.PhongToon:
                            pixelColor = ShadePhongToonPixel(a, b, c, baseColor, w0, w1, w2);
                            break;
                        default:
                            pixelColor = ShadeTexturedPixel(a, b, c, w0, w1, w2);
                            break;
                    }

                    Color[index] = Pack(pixelColor);
                    _depth[index] = z;
                }
            }
        }

        private void RenderObjectWire(SceneObject obj, Mat4 viewProjection, Vec3 viewDir)
        {
            if (obj.Model == null) return;
            Mat4 world = SceneTransform * obj.ModelMatrix;

            foreach (Face face in obj.Model.Faces)
            {
                int n = face.Vertices.Count;
                if (n < 2) continue;

                Vec3 normal = TransformNormal(world, face.Normal);
                if (CullBackFaces && !IsFrontFacing(normal, FaceCentroid(obj.Model, face, world), viewDir)) continue;

                var vertices = new ScreenVertex[n];
                bool projected = true;

                for (int k = 0; k < n; k++)
                {
                    Vec3 point = world.TransformPoint(obj.Model.Vertices[face.Vertices[k].V]);
                    if (!ProjectToScreen(point, viewProjection, out vertices[k]))
                    {
                        projected = false;
                        break;
                    }
                }

                if (!projected) continue;

                for (int k = 0; k < n; k++) DrawLine(vertices[k].X, vertices[k].Y, vertices[(k + 1) % n].X, vertices[(k + 1) % n].Y, WireColor);
            }
        }

        private void DrawLine(double x0, double y0, double x1, double y1, int argb)
        {
            int ix0 = (int)Math.Round(x0);
            int iy0 = (int)Math.Round(y0);
            int ix1 = (int)Math.Round(x1);
            int iy1 = (int)Math.Round(y1);
            int dx = Math.Abs(ix1 - ix0);
            int sx = ix0 < ix1 ? 1 : -1;
            int dy = -Math.Abs(iy1 - iy0);
            int sy = iy0 < iy1 ? 1 : -1;
            int error = dx + dy;

            while (true)
            {
                if (ix0 >= 0 && iy0 >= 0 && ix0 < W && iy0 < H) Color[iy0 * W + ix0] = argb;
                if (ix0 == ix1 && iy0 == iy1) break;

                int twiceError = 2 * error;
                if (twiceError >= dy) { error += dy; ix0 += sx; }
                if (twiceError <= dx) { error += dx; iy0 += sy; }
            }
        }

        public void Present()
        {
            var rect = new Rectangle(0, 0, W, H);
            var data = Bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            try
            {
                Marshal.Copy(Color, 0, data.Scan0, Color.Length);
            }
            finally
            {
                Bitmap.UnlockBits(data);
            }
        }
    }
}
