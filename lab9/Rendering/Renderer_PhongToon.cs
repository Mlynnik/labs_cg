using lab6.Math3d;
using System;

namespace lab6.Rendering
{
    // Участник 2: шейдинг Фонга (интерполяция нормалей) и туншейдинг.
    public partial class Renderer
    {
        private Vec3 Toon(Vec3 normal, Vec3 position, Vec3 baseColor)
        {
            double diffuse = Diffuse(normal, position);
            int bands = Math.Max(2, ToonBands);
            double quantized = Math.Floor(diffuse * bands) / (bands - 1.0);
            quantized = Math.Clamp(quantized, 0, 1);
            return baseColor * (Ambient + (1 - Ambient) * quantized);
        }

        private Vec3 ShadePhongToonPixel(ScreenVertex a, ScreenVertex b, ScreenVertex c, Vec3 baseColor,
            double w0, double w1, double w2)
        {
            Vec3 normal = (a.Normal * w0 + b.Normal * w1 + c.Normal * w2).Normalized();
            Vec3 position = a.World * w0 + b.World * w1 + c.World * w2;
            return Toon(normal, position, baseColor);
        }
    }
}
