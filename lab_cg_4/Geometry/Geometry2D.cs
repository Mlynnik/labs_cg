using System;
using System.Collections.Generic;
using System.Text;

namespace lab_cg_4.Geometry
{
    public static class Geometry2D
    {
        public static float Cross(float ax, float ay, float bx, float by)
        {
            return ax * by - ay * bx;
        }

        public static float Distance(PointF a, PointF b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        public static float DistancePointSegment(PointF p, PointF a, PointF b)
        {
            float vx = b.X - a.X;
            float vy = b.Y - a.Y;
            float wx = p.X - a.X;
            float wy = p.Y - a.Y;

            float c1 = vx * wx + vy * wy;
            if (c1 <= 0)
                return Distance(p, a);

            float c2 = vx * vx + vy * vy;
            if (c2 <= c1)
                return Distance(p, b);

            float t = c1 / c2;
            var proj = new PointF(a.X + t * vx, a.Y + t * vy);
            return Distance(p, proj);
        }

        public static bool PointInPolygon_RayCasting(PointF p, List<PointF> poly)
        {
            if (poly == null || poly.Count < 3)
                return false;

            bool inside = false;

            for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
            {
                var pi = poly[i];
                var pj = poly[j];

                if (((pi.Y > p.Y) != (pj.Y > p.Y)) &&
                    (p.X < (pj.X - pi.X) * (p.Y - pi.Y) / (pj.Y - pi.Y) + pi.X))
                    inside = !inside;
            }

            return inside;
        }

        public static bool FindNearestEdge(
            IEnumerable<Polygon> polygons,
            PointF p,
            float maxDist,
            out PointF aOut,
            out PointF bOut)
        {
            float best = float.MaxValue;
            aOut = bOut = PointF.Empty;

            foreach (var poly in polygons)
            {
                for (int i = 0; i < poly.pts.Count - 1; i++)
                {
                    var a = poly.pts[i];
                    var b = poly.pts[i + 1];

                    float d = DistancePointSegment(p, a, b);

                    if (d < best)
                    {
                        best = d;
                        aOut = a;
                        bOut = b;
                    }
                }
            }

            return best <= maxDist;
        }

        public static bool SegmentsIntersect(
            PointF p1,
            PointF p2,
            PointF q1,
            PointF q2,
            out PointF ip)
        {
            ip = PointF.Empty;

            float x1 = p1.X, y1 = p1.Y, x2 = p2.X, y2 = p2.Y;
            float x3 = q1.X, y3 = q1.Y, x4 = q2.X, y4 = q2.Y;

            float denom = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
            if (Math.Abs(denom) < 1e-6)
                return false;

            float xi = ((x1 * y2 - y1 * x2) * (x3 - x4) -
                        (x1 - x2) * (x3 * y4 - y3 * x4)) / denom;

            float yi = ((x1 * y2 - y1 * x2) * (y3 - y4) -
                        (y1 - y2) * (x3 * y4 - y3 * x4)) / denom;

            ip = new PointF(xi, yi);

            if (xi < Math.Min(x1, x2) - 1e-6 || xi > Math.Max(x1, x2) + 1e-6)
                return false;
            if (xi < Math.Min(x3, x4) - 1e-6 || xi > Math.Max(x3, x4) + 1e-6)
                return false;
            if (yi < Math.Min(y1, y2) - 1e-6 || yi > Math.Max(y1, y2) + 1e-6)
                return false;
            if (yi < Math.Min(y3, y4) - 1e-6 || yi > Math.Max(y3, y4) + 1e-6)
                return false;

            return true;
        }
    }
}
