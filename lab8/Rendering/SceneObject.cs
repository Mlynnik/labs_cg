using lab6.Math3d;
using lab6.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace lab6.Rendering
{
    public class SceneObject
    {
        public string Name = "Object";
        public Polyhedron Model;
        public Mat4 ModelMatrix = Mat4.Identity;
        public Vec3 BaseColor = new Vec3(0.85, 0.55, 0.30);

        public SceneObject Clone() => new SceneObject
        {
            Name = Name,
            Model = Model,
            ModelMatrix = ModelMatrix,
            BaseColor = BaseColor
        };
    }
}
