using lab6.Math3d;
using System;

namespace lab6.Rendering
{
    // Участник 1: освещение по Ламберту и интерполяция цветов по Гуро.
    public partial class Renderer
    {
        private Vec3 Lambert(Vec3 normal, Vec3 position, Vec3 baseColor)
        {
            double intensity = Ambient + (1 - Ambient) * Diffuse(normal, position);
            return baseColor * intensity;
        }

        private Vec3 ShadeGouraudPixel(ScreenVertex a, ScreenVertex b, ScreenVertex c, ScreenVertex[] face,
            int triangleIndex, double w0, double w1, double w2)
        {
            // Для треугольников интерполируем известные цвета трех вершин.
            if (face.Length != 4) return a.Lit * w0 + b.Lit * w1 + c.Lit * w2;

            // Для четырехугольника восстанавливаем (s, t) по двум треугольникам.
            // Первый: (0,1,2); второй: (0,2,3). Это дает непрерывный цвет на диагонали.
            double s = triangleIndex == 1 ? w1 + w2 : w1;
            double t = triangleIndex == 1 ? w2 : w1 + w2;
            s = Math.Clamp(s, 0, 1);
            t = Math.Clamp(t, 0, 1);

            // C(s,t) = C00(1-s)(1-t) + C10*s(1-t) + C11*s*t + C01(1-s)*t.
            return face[0].Lit * ((1 - s) * (1 - t)) +
                   face[1].Lit * (s * (1 - t)) +
                   face[2].Lit * (s * t) +
                   face[3].Lit * ((1 - s) * t);
        }
    }
}
