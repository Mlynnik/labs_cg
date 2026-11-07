using lab6.Math3d;
using System;
using System.Collections.Generic;
using System.Text;

namespace lab6.Rendering
{
    public class Camera
    {
        public Vec3 Eye = new Vec3(0, 0, 5);
        public Vec3 Target = Vec3.Zero;
        public Vec3 Up = Vec3.UnitY;
        public double FovDeg = 60;
        public double Near = 0.1;
        public double Far = 1000;

        public Mat4 ViewMatrix => Mat4.LookAt(Eye, Target, Up);
        public Camera Clone() => new Camera
        {
            Eye = Eye,
            Target = Target,
            Up = Up,
            FovDeg = FovDeg,
            Near = Near,
            Far = Far
        };
    }
}
