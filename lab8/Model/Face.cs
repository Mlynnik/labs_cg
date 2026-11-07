using lab6.Math3d;
using System;
using System.Collections.Generic;
using System.Text;

namespace lab6.Model
{
    public class Face
    {
        public List<FaceVertex> Vertices = new List<FaceVertex>();
        public Vec3 Normal;  
        public Vec3 Color = new Vec3(0.85, 0.55, 0.30);

        public Face() 
        {
        
        }
        public Face(IEnumerable<int> v)
        { 
            foreach (var i in v) 
                Vertices.Add(new FaceVertex(i)); 
        }
    }
}
