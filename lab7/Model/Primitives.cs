using lab6.Math3d;
using System;
using System.Collections.Generic;
using System.Text;

namespace lab6.Model
{
    public static class Primitives
    {
        static readonly double Phi = (1 + System.Math.Sqrt(5)) / 2;

        public static Polyhedron Cube()
        {
            var p = new Polyhedron { Name = "Cube" };

            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                    for (int k = 0; k < 2; k++)
                        p.Vertices.Add(new Vec3(2 * i - 1, 2 * j - 1, 2 * k - 1));

            int[,] F ={
                {0, 2, 3, 1},
                {4, 5, 7, 6},
                {0, 1, 5, 4},
                {2, 6, 7, 3},
                {0, 4, 6, 2},
                {1, 3, 7, 5},
            };
            for (int i = 0; i < 6; i++)
                p.Faces.Add(new Face(new[] { F[i, 0], F[i, 1], F[i, 2], F[i, 3] }));

            p.EnsureOutwardNormals();
            return p;
        }

        public static Polyhedron Tetrahedron()
        {
            var p = new Polyhedron { Name = "Tetrahedron" };
            p.Vertices.AddRange(new[]
            {
                new Vec3( 1,  1,  1),
                new Vec3( 1, -1, -1),
                new Vec3(-1,  1, -1),
                new Vec3(-1, -1,  1),
            });
            int[,] F =
            {
                {0, 1, 2},
                {0, 3, 1},
                {0, 2, 3},
                {1, 3, 2},
            };
            for (int i = 0; i < F.GetLength(0); i++)
                p.Faces.Add(new Face(new[] { F[i, 0], F[i, 1], F[i, 2] }));
            p.EnsureOutwardNormals();
            return p;
        }

        public static Polyhedron Octahedron()
        {
            var p = new Polyhedron { Name = "Octahedron" };
            p.Vertices.AddRange(new[]
            {
                new Vec3( 1,  0,  0), new Vec3(-1,  0,  0),
                new Vec3( 0,  1,  0), new Vec3( 0, -1,  0),
                new Vec3( 0,  0,  1), new Vec3( 0,  0, -1),
            });
            int[,] F =
            {
                {0, 2, 4}, {2, 1, 4}, {1, 3, 4}, {3, 0, 4},
                {2, 0, 5}, {1, 2, 5}, {3, 1, 5}, {0, 3, 5},
            };
            for (int i = 0; i < F.GetLength(0); i++)
                p.Faces.Add(new Face(new[] { F[i, 0], F[i, 1], F[i, 2] }));
            p.EnsureOutwardNormals();
            return p;
        }

        public static Polyhedron Icosahedron()
        {
            var p = new Polyhedron { Name = "Icosahedron" };
            p.Vertices.AddRange(new[]
            {
                new Vec3(-1,  Phi, 0), new Vec3( 1,  Phi, 0), new Vec3(-1, -Phi, 0), new Vec3( 1, -Phi, 0),
                new Vec3( 0, -1,  Phi), new Vec3( 0,  1,  Phi), new Vec3( 0, -1, -Phi), new Vec3( 0,  1, -Phi),
                new Vec3( Phi, 0, -1), new Vec3( Phi, 0,  1), new Vec3(-Phi, 0, -1), new Vec3(-Phi, 0,  1),
            });
            int[,] F =
            {
                {0,11,5},{0,5,1},{0,1,7},{0,7,10},{0,10,11},
                {1,5,9},{5,11,4},{11,10,2},{10,7,6},{7,1,8},
                {3,9,4},{3,4,2},{3,2,6},{3,6,8},{3,8,9},
                {4,9,5},{2,4,11},{6,2,10},{8,6,7},{9,8,1},
            };
            for (int i = 0; i < F.GetLength(0); i++)
                p.Faces.Add(new Face(new[] { F[i, 0], F[i, 1], F[i, 2] }));
            p.EnsureOutwardNormals();
            return p;
        }

        public static Polyhedron Dodecahedron()
        {
            var ico = Icosahedron();
            var dod = new Polyhedron { Name = "Dodecahedron" };

            foreach (var f in ico.Faces)
            {
                Vec3 c = Vec3.Zero;
                foreach (var fv in f.Vertices) c += ico.Vertices[fv.V];
                dod.Vertices.Add(c * (1.0 / f.Vertices.Count));
            }

            for (int vi = 0; vi < ico.Vertices.Count; vi++)
            {
                var around = new List<int>();
                for (int fi = 0; fi < ico.Faces.Count; fi++)
                    if (ico.Faces[fi].Vertices.Any(fv => fv.V == vi))
                        around.Add(fi);
                var center = ico.Vertices[vi];
                var axis = center.Normalized();
                var refDir = (dod.Vertices[around[0]] - center);
                refDir = (refDir - axis * Vec3.Dot(refDir, axis)).Normalized();
                var tan = Vec3.Cross(axis, refDir);
                around.Sort((a, b) =>
                {
                    var va = dod.Vertices[a] - center; va = va - axis * Vec3.Dot(va, axis);
                    var vb = dod.Vertices[b] - center; vb = vb - axis * Vec3.Dot(vb, axis);
                    double aa = System.Math.Atan2(Vec3.Dot(va, tan), Vec3.Dot(va, refDir));
                    double ab = System.Math.Atan2(Vec3.Dot(vb, tan), Vec3.Dot(vb, refDir));
                    return aa.CompareTo(ab);
                });

                dod.Faces.Add(new Face(around));
            }
            dod.EnsureOutwardNormals();
            return dod;
        }
    }
}