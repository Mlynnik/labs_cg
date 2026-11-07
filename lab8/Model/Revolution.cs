using lab6.Math3d;
using System;
using System.Collections.Generic;
using System.Text;

namespace lab6.Model
{
    public static class Revolution
    {
        /// <param name="generatrix">Точки образующей (полилиния).</param>
        /// <param name="axisDirection">Направление оси вращения.</param>
        /// <param name="divisions">Количество разбиений по 360°.</param>
        public static Polyhedron Build(IList<Vec3> generatrix, Vec3 axisDirection, int divisions)
        {
            if (generatrix == null || generatrix.Count < 2)
                throw new ArgumentException("Образующая должна содержать минимум 2 точки.");
            if (divisions < 3) 
                throw new ArgumentException("Разбиений должно быть ≥ 3.");
            if (divisions > 1000)
                throw new ArgumentException("Слишком много разбиений (макс. 1000).");

            axisDirection = axisDirection.Normalized();

            if (axisDirection.Length < 1e-9)
                throw new ArgumentException("Ось вращения не может быть нулевой.");

            var p = new Polyhedron { Name = "SurfaceOfRevolution" };
            int n = generatrix.Count;
            double step = 2.0 * Math.PI / divisions;

            for (int d = 0; d < divisions; d++)
            {
                var rot = Mat4.RotationAxis(axisDirection, d * step);
                for (int i = 0; i < n; i++)
                    p.Vertices.Add(rot.TransformPoint(generatrix[i]));
            }

            for (int d = 0; d < divisions; d++)
            {
                int next = (d + 1) % divisions;
                for (int i = 0; i < n - 1; i++)
                {
                    int v00 = d * n + i;
                    int v01 = d * n + i + 1;
                    int v10 = next * n + i;
                    int v11 = next * n + i + 1;

                    bool same0 = (p.Vertices[v00] - p.Vertices[v10]).Length < 1e-9;
                    bool same1 = (p.Vertices[v01] - p.Vertices[v11]).Length < 1e-9;

                    if (same0 && same1) 
                        continue;
                    if (same0)
                        p.Faces.Add(new Face(new[] { v00, v11, v01 }));
                    else if 
                        (same1) p.Faces.Add(new Face(new[] { v00, v10, v01 }));
                    else 
                        p.Faces.Add(new Face(new[] { v00, v10, v11, v01 }));
                }
            }

            p.ComputeFaceNormals();
            return p;
        }
    }
}
