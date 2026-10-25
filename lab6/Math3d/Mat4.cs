using System;
using System.Collections.Generic;
using System.Text;

namespace lab6.Math3d
{
    // Матрица 4x4 (row-major), точка — вектор-столбец.
    public struct Mat4
    {
        public readonly double[,] M;
        public Mat4(double[,] m) { M = m; }

        public static Mat4 Identity
        {
            get
            {
                var m = new double[4, 4];
                for (int i = 0; i < 4; i++) m[i, i] = 1;
                return new Mat4(m);
            }
        }

        public static Mat4 operator *(Mat4 a, Mat4 b)
        {
            var r = new double[4, 4];
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    double s = 0;
                    for (int k = 0; k < 4; k++) s += a.M[i, k] * b.M[k, j];
                    r[i, j] = s;
                }
            return new Mat4(r);
        }

        public Vec3 TransformPoint(Vec3 p)
        {
            double x = M[0, 0] * p.X + M[0, 1] * p.Y + M[0, 2] * p.Z + M[0, 3];
            double y = M[1, 0] * p.X + M[1, 1] * p.Y + M[1, 2] * p.Z + M[1, 3];
            double z = M[2, 0] * p.X + M[2, 1] * p.Y + M[2, 2] * p.Z + M[2, 3];
            double w = M[3, 0] * p.X + M[3, 1] * p.Y + M[3, 2] * p.Z + M[3, 3];
            if (System.Math.Abs(w) > 1e-12) { x /= w; y /= w; z /= w; }
            return new Vec3(x, y, z);
        }

        // Фабрики
        public static Mat4 Translation(double x, double y, double z)
        {
            var m = new double[4, 4]; m[0, 0] = m[1, 1] = m[2, 2] = m[3, 3] = 1;
            m[0, 3] = x; m[1, 3] = y; m[2, 3] = z;
            return new Mat4(m);
        }
        public static Mat4 Translation(Vec3 v) => Translation(v.X, v.Y, v.Z);

        public static Mat4 Scale(double sx, double sy, double sz)
        {
            var m = new double[4, 4]; m[0, 0] = sx; m[1, 1] = sy; m[2, 2] = sz; m[3, 3] = 1;
            return new Mat4(m);
        }

        public static Mat4 RotationX(double a)
        {
            double c = System.Math.Cos(a), s = System.Math.Sin(a);
            var m = new double[4, 4]; m[0, 0] = 1; m[3, 3] = 1;
            m[1, 1] = c; m[1, 2] = -s; m[2, 1] = s; m[2, 2] = c;
            return new Mat4(m);
        }
        public static Mat4 RotationY(double a)
        {
            double c = System.Math.Cos(a), s = System.Math.Sin(a);
            var m = new double[4, 4]; m[1, 1] = 1; m[3, 3] = 1;
            m[0, 0] = c; m[0, 2] = s; m[2, 0] = -s; m[2, 2] = c;
            return new Mat4(m);
        }
        public static Mat4 RotationZ(double a)
        {
            double c = System.Math.Cos(a), s = System.Math.Sin(a);
            var m = new double[4, 4]; m[2, 2] = 1; m[3, 3] = 1;
            m[0, 0] = c; m[0, 1] = -s; m[1, 0] = s; m[1, 1] = c;
            return new Mat4(m);
        }

        // Поворот вокруг оси (через начало координат) на угол — формула Родрига.
        public static Mat4 RotationAxis(Vec3 axis, double a)
        {
            axis = axis.Normalized();
            double c = System.Math.Cos(a), s = System.Math.Sin(a), t = 1 - c;
            double x = axis.X, y = axis.Y, z = axis.Z;
            var m = new double[4, 4];
            m[0, 0] = t * x * x + c; m[0, 1] = t * x * y - s * z; m[0, 2] = t * x * z + s * y;
            m[1, 0] = t * x * y + s * z; m[1, 1] = t * y * y + c; m[1, 2] = t * y * z - s * x;
            m[2, 0] = t * x * z - s * y; m[2, 1] = t * y * z + s * x; m[2, 2] = t * z * z + c;
            m[3, 3] = 1;
            return new Mat4(m);
        }

        // Поворот вокруг произвольной прямой (p1->p2) на угол a.
        public static Mat4 RotationAroundLine(Vec3 p1, Vec3 p2, double a)
            => Translation(p1) * RotationAxis(p2 - p1, a) * Translation(-p1);

        // Масштаб относительно точки.
        public static Mat4 ScaleAround(Vec3 center, double s)
            => Translation(center) * Scale(s, s, s) * Translation(-center);

        // Поворот вокруг оси через точку center, параллельной axisDir.
        public static Mat4 RotateAroundAxisThroughPoint(Vec3 center, Vec3 axisDir, double a)
            => Translation(center) * RotationAxis(axisDir, a) * Translation(-center);

        // Отражения относительно координатных плоскостей
        public static Mat4 ReflectionXY() => Scale(1, 1, -1);
        public static Mat4 ReflectionXZ() => Scale(1, -1, 1);
        public static Mat4 ReflectionYZ() => Scale(-1, 1, 1);
    }
}
