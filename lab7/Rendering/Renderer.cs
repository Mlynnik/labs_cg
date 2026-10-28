using lab6.Math3d;
using lab6.Model;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

namespace lab6.Rendering
{
    public enum ProjectionMode { Perspective, Axonometric }

    public class Renderer
    {
        public int W { get; private set; }
        public int H { get; private set; }
        public int[] Color;
        public Bitmap Bitmap;

        public ProjectionMode Projection = ProjectionMode.Perspective;
        public double CameraDistance = 5.0;   // расстояние от «камеры» до центра объекта

        public Renderer(int w, int h) { Resize(w, h); }

        public void Resize(int w, int h)
        {
            W = w; H = h;
            Color = new int[w * h];
            Bitmap?.Dispose();
            Bitmap = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        }

        public void Clear(int argb)
        {
            for (int i = 0; i < Color.Length; i++) Color[i] = argb;
        }

        // Проекция точки мира на экран
        public double SceneRadius = 1.0;   // половина «радиуса» сцены (max |p−center|)

        (double X, double Y) Project(Vec3 world)
        {
            double r = Math.Max(SceneRadius, 1e-6);

            if (Projection == ProjectionMode.Perspective)
            {
                // Авто-удаление камеры: d >= 4r, чтобы модель не «взорвалась» в проекции
                double d = Math.Max(CameraDistance, 4.0 * r);
                double f = Math.Min(W, H) * 0.30 * d / r;
                double denom = d - world.Z;
                if (Math.Abs(denom) < 0.05) denom = 0.05;
                return (W / 2.0 + world.X * f / denom,
                        H / 2.0 - world.Y * f / denom);
            }
            else
            {
                var iso = Mat4.RotationX(35.264 * Math.PI / 180)
                        * Mat4.RotationY(-45.0 * Math.PI / 180);
                var p = iso.TransformPoint(world);
                double f = Math.Min(W, H) * 0.40 / r;
                return (W / 2.0 + p.X * f,
                        H / 2.0 - p.Y * f);
            }
        }

        /// <summary>Каркасная отрисовка модели с применённой модельной матрицей.</summary>
        public void Render(Polyhedron model, Mat4 modelMatrix)
        {
            var screen = new (double X, double Y)[model.Vertices.Count];
            for (int i = 0; i < model.Vertices.Count; i++)
                screen[i] = Project(modelMatrix.TransformPoint(model.Vertices[i]));

            int lineColor = unchecked((int)0xFF1A237E);
            foreach (var f in model.Faces)
            {
                int c = f.Vertices.Count;
                for (int i = 0; i < c; i++)
                {
                    var a = screen[f.Vertices[i].V];
                    var b = screen[f.Vertices[(i + 1) % c].V];
                    DrawLine(a.X, a.Y, b.X, b.Y, lineColor);
                }
            }
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
            var data = Bitmap.LockBits(
                new Rectangle(0, 0, W, H),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb);
            Marshal.Copy(Color, 0, data.Scan0, Color.Length);
            Bitmap.UnlockBits(data);
        }
    }
}
