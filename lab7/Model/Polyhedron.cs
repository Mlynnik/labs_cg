using lab6.Math3d;
using System;
using System.Collections.Generic;
using System.Text;

namespace lab6.Model
{
    public class Polyhedron
    {
        public string Name = "Object";
        public List<Vec3> Vertices = new List<Vec3>();
        public List<Vec3> Normals = new List<Vec3>();            
        public List<(double U, double V)> TexCoords = new List<(double, double)>(); 
        public List<Face> Faces = new List<Face>();

        public Vec3 Center
        {
            get
            {
                if (Vertices.Count == 0) return Vec3.Zero;
                Vec3 mn = Vertices[0], mx = Vertices[0];
                foreach (var v in Vertices)
                {
                    if (v.X < mn.X) 
                        mn.X = v.X; 
                    if (v.Y < mn.Y)
                        mn.Y = v.Y;
                    if (v.Z < mn.Z) 
                        mn.Z = v.Z;
                    if (v.X > mx.X)
                        mx.X = v.X; 
                    if (v.Y > mx.Y) 
                        mx.Y = v.Y;
                    if (v.Z > mx.Z) 
                        mx.Z = v.Z;
                }
                return (mn + mx) * 0.5;
            }
        }

        public void ComputeFaceNormals()
        {
            foreach (var f in Faces)
            {
                var a = Vertices[f.Vertices[0].V];
                var b = Vertices[f.Vertices[1].V];
                var c = Vertices[f.Vertices[2].V];
                f.Normal = Vec3.Cross(b - a, c - a).Normalized();
            }
        }

        public void EnsureOutwardNormals()
        {
            ComputeFaceNormals();
            var center = Center;
            foreach (var f in Faces)
                if (Vec3.Dot(f.Normal, Vertices[f.Vertices[0].V] - center) < 0)
                    f.Vertices.Reverse();
            ComputeFaceNormals();
        }

        public void ApplyMatrix(Mat4 m)
        {
            for (int i = 0; i < Vertices.Count; i++) 
                Vertices[i] = m.TransformPoint(Vertices[i]);
            ComputeFaceNormals();
        }
    }
}
