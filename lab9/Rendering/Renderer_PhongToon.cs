using lab6.Math3d;
using System;

namespace lab6.Rendering
{
    // Участник 2: шейдинг Фонга (интерполяция нормалей) и туншейдинг.
    public partial class Renderer
    {
        private Vec3 Toon(Vec3 normal, Vec3 position, Vec3 baseColor)
        {
            return baseColor;
        }

        private Vec3 ShadePhongToonPixel(ScreenVertex a, ScreenVertex b, ScreenVertex c, Vec3 baseColor,
            double w0, double w1, double w2)
        {
            return baseColor;
        }
    }
}
