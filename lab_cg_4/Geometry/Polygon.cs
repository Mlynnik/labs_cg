using System;
using System.Collections.Generic;
using System.Text;

namespace lab_cg_4.Geometry
{
    public class Polygon
    {
        public List<PointF> pts = new List<PointF>();
        public bool Selected = false;

        public void ApplyMatrix(Matrix3 M)
        {
            for (int i = 0; i < pts.Count; i++)
                pts[i] = M.Transform(pts[i]);
        }

        public PointF Center()
        {
            if (pts.Count == 0) return PointF.Empty;
            int n = pts.Count;
            if (n > 2 && pts[0] == pts[n - 1])
                n--;

            float sx = 0, sy = 0;
            for (int i = 0; i < n; i++)
            {
                sx += pts[i].X;
                sy += pts[i].Y;
            }

            return new PointF(sx / n, sy / n);
        }

        public bool HitTest(PointF p, float tolerance)
        {
            if (pts.Count == 0)
                return false;

            if (pts.Count == 1)
                return Geometry2D.Distance(p, pts[0]) <= tolerance;

            if (pts.Count == 2)
                return Geometry2D.DistancePointSegment(p, pts[0], pts[1]) <= tolerance;

            if (Geometry2D.PointInPolygon_RayCasting(p, pts))
                return true;

            for (int i = 0; i < pts.Count - 1; i++)
                if (Geometry2D.DistancePointSegment(p, pts[i], pts[i + 1]) <= tolerance)
                    return true;

            return false;
        }

        public void Draw(Graphics g)
        {
            Draw(g, false);
        }

        public void Draw(Graphics g, bool isCurrent)
        {
            if (pts.Count == 0)
                return;

            Color color = Selected ? Color.Red : (isCurrent ? Color.Gray : Color.Black);
            Pen pen = new Pen(color, 2);

            if (pts.Count == 1)
            {
                Brush brush = Selected ? Brushes.Red : (isCurrent ? Brushes.Gray : Brushes.Black);
                g.FillEllipse(brush, pts[0].X - 3, pts[0].Y - 3, 6, 6);
            }
            else if (pts.Count == 2)
            {
                g.DrawLine(pen, pts[0], pts[1]);
            }
            else
            {
                g.DrawLines(pen, pts.ToArray());
            }
        }
    }
}
