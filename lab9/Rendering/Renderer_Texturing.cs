using lab6.Math3d;
using lab6.Model;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace lab6.Rendering
{
    public partial class Renderer
    {
        public int[] TexturePixels;
        public int TextureWidth;
        public int TextureHeight;

        public void SetTexture(Bitmap source)
        {
            using var converted = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(converted))
            {
                graphics.DrawImage(source, 0, 0, converted.Width, converted.Height);
            }

            var rect = new Rectangle(0, 0, converted.Width, converted.Height);
            var data = converted.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            try
            {
                var pixels = new int[converted.Width * converted.Height];

                for (int y = 0; y < converted.Height; y++)
                {
                    Marshal.Copy(IntPtr.Add(data.Scan0, y * data.Stride), pixels, y * converted.Width, converted.Width);
                }

                TexturePixels = pixels;
                TextureWidth = converted.Width;
                TextureHeight = converted.Height;
            }
            finally
            {
                converted.UnlockBits(data);
            }
        }

        public void MakeCheckerTexture()
        {
            using var bitmap = new Bitmap(256, 256);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        var color = (x + y) % 2 == 0
                            ? System.Drawing.Color.FromArgb(45, 105, 185)
                            : System.Drawing.Color.FromArgb(242, 194, 97);

                        using var brush = new SolidBrush(color);
                        graphics.FillRectangle(brush, x * 32, y * 32, 32, 32);
                    }
                }
            }

            SetTexture(bitmap);
        }

        private static (double u, double v)[] GetUVs(Polyhedron model, Face face)
        {
            int n = face.Vertices.Count;
            var uv = new (double u, double v)[n];
            bool hasUV = true;

            for (int k = 0; k < n; k++)
            {
                int textureIndex = face.Vertices[k].T;
                if (textureIndex < 0 || textureIndex >= model.TexCoords.Count) hasUV = false;
            }

            if (hasUV)
            {
                for (int k = 0; k < n; k++)
                {
                    var coordinate = model.TexCoords[face.Vertices[k].T];
                    uv[k] = (coordinate.U, coordinate.V);
                }
                return uv;
            }

            Vec3 origin = model.Vertices[face.Vertices[0].V];
            Vec3 uAxis = (model.Vertices[face.Vertices[1].V] - origin).Normalized();
            Vec3 vAxis = Vec3.Cross(face.Normal, uAxis).Normalized();

            double minU = double.PositiveInfinity;
            double minV = double.PositiveInfinity;
            double maxU = double.NegativeInfinity;
            double maxV = double.NegativeInfinity;

            for (int k = 0; k < n; k++)
            {
                Vec3 offset = model.Vertices[face.Vertices[k].V] - origin;
                uv[k] = (Vec3.Dot(offset, uAxis), Vec3.Dot(offset, vAxis));

                minU = Math.Min(minU, uv[k].u);
                maxU = Math.Max(maxU, uv[k].u);
                minV = Math.Min(minV, uv[k].v);
                maxV = Math.Max(maxV, uv[k].v);
            }

            double width = Math.Max(1e-9, maxU - minU);
            double height = Math.Max(1e-9, maxV - minV);

            for (int k = 0; k < n; k++)
            {
                uv[k] = ((uv[k].u - minU) / width, (uv[k].v - minV) / height);

                FaceVertex corner = face.Vertices[k];
                corner.T = model.TexCoords.Count;
                model.TexCoords.Add((uv[k].u, uv[k].v));
                face.Vertices[k] = corner;
            }

            return uv;
        }

        private int SampleTexture(double u, double v)
        {
            if (TexturePixels == null || TextureWidth < 1 || TextureHeight < 1)
                return unchecked((int)0xFFFFFFFF);

            u = Math.Clamp(u, 0, 1);
            v = Math.Clamp(v, 0, 1);

            double fx = u * (TextureWidth - 1);
            double fy = (1 - v) * (TextureHeight - 1);

            int x0 = (int)fx;
            int y0 = (int)fy;
            int x1 = Math.Min(x0 + 1, TextureWidth - 1);
            int y1 = Math.Min(y0 + 1, TextureHeight - 1);
            double tx = fx - x0;
            double ty = fy - y0;

            Vec3 a = Unpack(TexturePixels[y0 * TextureWidth + x0]);
            Vec3 b = Unpack(TexturePixels[y0 * TextureWidth + x1]);
            Vec3 c = Unpack(TexturePixels[y1 * TextureWidth + x0]);
            Vec3 d = Unpack(TexturePixels[y1 * TextureWidth + x1]);

            Vec3 upper = a * (1 - tx) + b * tx;
            Vec3 lower = c * (1 - tx) + d * tx;
            return Pack(upper * (1 - ty) + lower * ty);
        }

        // Интерполируем UV и рассчитываем освещение для каждого пикселя.
        private Vec3 ShadeTexturedPixel(ScreenVertex a, ScreenVertex b, ScreenVertex c, double w0, double w1, double w2)
        {
            double u = a.U * w0 + b.U * w1 + c.U * w2;
            double v = a.V * w0 + b.V * w1 + c.V * w2;
            Vec3 texel = Unpack(SampleTexture(u, v));

            Vec3 normal = (a.Normal * w0 + b.Normal * w1 + c.Normal * w2).Normalized();
            Vec3 position = a.World * w0 + b.World * w1 + c.World * w2;
            double intensity = Ambient + (1 - Ambient) * Diffuse(normal, position);
            return texel * intensity;
        }
    }
}
