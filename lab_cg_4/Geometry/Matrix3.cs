using System;
using System.Collections.Generic;
using System.Text;

namespace lab_cg_4.Geometry
{
    public class Matrix3
    {
        public float[,] m = new float[3, 3];

        public Matrix3()
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    m[i, j] = 0f;
        }

        public static Matrix3 Identity()
        {
            var M = new Matrix3();
            M.m[0, 0] = M.m[1, 1] = M.m[2, 2] = 1f;
            return M;
        }

        public static Matrix3 Translate(float dx, float dy)
        {
            var M = Identity();
            M.m[0, 2] = dx;
            M.m[1, 2] = dy;
            return M;
        }

        public static Matrix3 Rotate(float angleDeg)
        {
            var M = Identity();
            double a = angleDeg * Math.PI / 180.0;
            float c = (float)Math.Cos(a);
            float s = (float)Math.Sin(a);

            M.m[0, 0] = c;
            M.m[0, 1] = -s;
            M.m[1, 0] = s;
            M.m[1, 1] = c;
            return M;
        }

        public static Matrix3 Scale(float sx, float sy)
        {
            var M = Identity();
            M.m[0, 0] = sx;
            M.m[1, 1] = sy;
            return M;
        }

        public static Matrix3 operator *(Matrix3 A, Matrix3 B)
        {
            var R = new Matrix3();

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    float sum = 0;
                    for (int k = 0; k < 3; k++)
                        sum += A.m[i, k] * B.m[k, j];
                    R.m[i, j] = sum;
                }

            return R;
        }

        public PointF Transform(PointF p)
        {
            float x = m[0, 0] * p.X + m[0, 1] * p.Y + m[0, 2];
            float y = m[1, 0] * p.X + m[1, 1] * p.Y + m[1, 2];
            float w = m[2, 0] * p.X + m[2, 1] * p.Y + m[2, 2];

            if (Math.Abs(w - 1f) > 1e-6f && Math.Abs(w) > 1e-9f)
            {
                x /= w;
                y /= w;
            }

            return new PointF(x, y);
        }
    }
}
