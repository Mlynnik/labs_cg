using lab6.Math3d;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace lab6.Model
{
    public static class ObjIO
    {
        public static Polyhedron Load(string path)
        {
            var p = new Polyhedron { Name = Path.GetFileNameWithoutExtension(path) };
            foreach (var raw in File.ReadAllLines(path))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line[0] == '#')
                    continue;
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                    continue;

                switch (parts[0])
                {
                    case "o":
                        if (parts.Length > 1)
                            p.Name = parts[1];
                        break;
                    case "v":
                        p.Vertices.Add(new Vec3(D(parts[1]), D(parts[2]), D(parts[3])));
                        break;
                    case "vn":
                        p.Normals.Add(new Vec3(D(parts[1]), D(parts[2]), D(parts[3])));
                        break;
                    case "vt":
                        p.TexCoords.Add((D(parts[1]), D(parts[2])));
                        break;
                    case "f":
                        {
                            var face = new Face();
                            for (int i = 1; i < parts.Length; i++)
                                face.Vertices.Add(ParseFaceVertex(parts[i], p.Vertices.Count, p.Normals.Count, p.TexCoords.Count));
                            p.Faces.Add(face);
                            break;
                        }
                }
            }
            return p;
        }

        public static void Save(string path, Polyhedron p)
        {
            using var sw = new StreamWriter(path);
            sw.WriteLine("# Lab7 OBJ export");
            sw.WriteLine($"o {p.Name}");
            foreach (var v in p.Vertices)
                sw.WriteLine($"v {F(v.X)} {F(v.Y)} {F(v.Z)}");
            foreach (var n in p.Normals)
                sw.WriteLine($"vn {F(n.X)} {F(n.Y)} {F(n.Z)}");
            foreach (var (u, vv) in p.TexCoords)
                sw.WriteLine($"vt {F(u)} {F(vv)}");

            foreach (var f in p.Faces)
            {
                var sb = new StringBuilder("f");
                foreach (var fv in f.Vertices)
                {
                    sb.Append(' ').Append(fv.V + 1);
                    if (fv.T >= 0 || fv.N >= 0)
                    {
                        sb.Append('/');
                        if (fv.T >= 0)
                            sb.Append(fv.T + 1);
                        if (fv.N >= 0)
                            sb.Append('/').Append(fv.N + 1);
                    }
                }
                sw.WriteLine(sb.ToString());
            }
        }

        static string F(double v) => v.ToString("F6", CultureInfo.InvariantCulture);
        static double D(string s) => double.Parse(s, CultureInfo.InvariantCulture);

        static FaceVertex ParseFaceVertex(string s, int vCount, int nCount, int tCount)
        {
            var parts = s.Split('/');
            int v = ParseIndex(parts.Length > 0 ? parts[0] : null, vCount);
            int t = ParseIndex(parts.Length > 1 ? parts[1] : null, tCount);
            int n = ParseIndex(parts.Length > 2 ? parts[2] : null, nCount);
            return new FaceVertex(v, n, t);
        }

        static int ParseIndex(string s, int count)
        {
            if (string.IsNullOrEmpty(s))
                return -1;
            if (!int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v))
                return -1;
            if (v > 0)
                return v - 1;
            if (v < 0)
                return count + v;
            return -1;
        }
    }
}
