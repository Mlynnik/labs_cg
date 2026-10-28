using lab6.Math3d;
using System;
using System.Collections.Generic;
using System.Text;

namespace lab6.Model
{
    public static class SurfacePlot
    {
        public static Polyhedron Build(Func<double, double, double> f, double x0, double x1, double y0, double y1, int nx, int ny)
        {
            if (nx < 1 || ny < 1) 
                throw new ArgumentException("Разбиения должны быть ≥ 1.");
            if (nx > 500 || ny > 500)
                throw new ArgumentException("Слишком много разбиений (макс. 500).");

            var p = new Polyhedron { Name = "SurfacePlot" };
            int NX = nx + 1, NY = ny + 1;

            for (int j = 0; j < NY; j++)
            {
                double y = y0 + (y1 - y0) * j / ny;
                for (int i = 0; i < NX; i++)
                {
                    double x = x0 + (x1 - x0) * i / nx;
                    p.Vertices.Add(new Vec3(x, y, f(x, y)));
                }
            }

            for (int j = 0; j < ny; j++)
            {
                for (int i = 0; i < nx; i++)
                {
                    int v00 = j * NX + i;
                    int v10 = j * NX + (i + 1);
                    int v11 = (j + 1) * NX + (i + 1);
                    int v01 = (j + 1) * NX + i;
                    p.Faces.Add(new Face(new[] { v00, v10, v11, v01 }));
                }
            }

            p.ComputeFaceNormals();
            return p;
        }
    }
}
