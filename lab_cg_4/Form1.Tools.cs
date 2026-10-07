using System.Drawing;
using System.Text;
using System.Windows.Forms;
using lab_cg_4.Geometry;

namespace lab_cg_4
{
    public partial class Form1
    {
        PointF? testPoint = null;

        PointF? edgeA = null;
        PointF? edgeB = null;

        bool drawingSecondEdge = false;
        bool hasSecondEdge = false;
        PointF secondEdgeStart;
        PointF secondEdgeEnd;

        PointF? intersectionPoint = null;

        partial void InitToolsUI(int sx)
        {
        }

        partial void HandleControlClick(PointF p, ref bool handled)
        {
            handled = true;
            testPoint = p;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Точка ({p.X:F1}; {p.Y:F1}):");

            if (polygons.Count == 0)
            {
                sb.Append("Нет полигонов на сцене.");
                lblInfo.Text = sb.ToString();
                canvas.Invalidate();
                return;
            }

            for (int i = 0; i < polygons.Count; i++)
            {
                List<PointF> pts = polygons[i].pts;

                if (pts.Count < 3)
                {
                    sb.AppendLine($"#{i + 1}: это точка или ребро");
                    continue;
                }

                bool convex = IsConvexPolygon(pts);
                bool inside;

                if (convex)
                    inside = PointInConvexPolygon(p, pts);
                else
                    inside = Geometry2D.PointInPolygon_RayCasting(p, pts);

                if (!inside && PointOnBoundary(p, pts, 3f))
                    inside = true;

                string type = convex ? "выпуклый" : "невыпуклый";
                sb.AppendLine($"#{i + 1} ({type}): {(inside ? "внутри" : "снаружи")}");
            }

            lblInfo.Text = sb.ToString();
            canvas.Invalidate();
        }

        partial void HandleShiftClick(PointF p, ref bool handled)
        {
            handled = true;
            testPoint = p;

            if (!Geometry2D.FindNearestEdge(polygons, p, 40f, out PointF a, out PointF b))
            {
                edgeA = null;
                edgeB = null;
                lblInfo.Text = "Кликните ближе к ребру.";
                canvas.Invalidate();
                return;
            }

            edgeA = a;
            edgeB = b;

            float xa = b.X - a.X;
            float ya = b.Y - a.Y;
            float xb = p.X - a.X;
            float yb = p.Y - a.Y;

            float value = xa * yb - ya * xb;

            string side;
            if (Math.Abs(value) < 1e-3f)
                side = "на прямой ребра";
            else if (value > 0)
                side = "слева от ребра";
            else
                side = "справа от ребра";

            lblInfo.Text =
                $"Точка ({p.X:F1}; {p.Y:F1}) — {side}.\n" +
                $"Ребро: ({a.X:F1}; {a.Y:F1}) -> ({b.X:F1}; {b.Y:F1}).";

            canvas.Invalidate();
        }

        partial void HandleAltMouseDown(MouseEventArgs e, ref bool handled)
        {
            if (!IsAltPressed() || e.Button != MouseButtons.Left)
                return;

            handled = true;

            if (!Geometry2D.FindNearestEdge(polygons, e.Location, 40f, out PointF a, out PointF b))
            {
                drawingSecondEdge = false;
                edgeA = null;
                edgeB = null;
                intersectionPoint = null;
                lblInfo.Text = "Начните Alt+drag рядом с ребром.";
                canvas.Invalidate();
                return;
            }

            edgeA = a;
            edgeB = b;
            drawingSecondEdge = true;
            hasSecondEdge = true;
            secondEdgeStart = e.Location;
            secondEdgeEnd = e.Location;
            intersectionPoint = null;

            lblInfo.Text = "Тяните второе ребро. Пересечение считается динамически.";
            canvas.Invalidate();
        }

        partial void HandleAltMouseMove(MouseEventArgs e, ref bool handled)
        {
            if (!drawingSecondEdge)
                return;

            handled = true;
            secondEdgeEnd = e.Location;
            UpdateIntersection();
            canvas.Invalidate();
        }

        partial void HandleAltMouseUp(MouseEventArgs e, ref bool handled)
        {
            if (!drawingSecondEdge)
                return;

            handled = true;
            drawingSecondEdge = false;
            secondEdgeEnd = e.Location;
            UpdateIntersection();
            suppressNextCanvasClick = true;

            if (intersectionPoint.HasValue)
            {
                PointF ip = intersectionPoint.Value;
                lblInfo.Text = $"Точка пересечения: ({ip.X:F1}; {ip.Y:F1}).";
            }
            else
            {
                lblInfo.Text = "Рёбра не пересекаются.";
            }

            canvas.Invalidate();
        }

        partial void DrawTools(Graphics g)
        {
            if (edgeA.HasValue && edgeB.HasValue)
            {
                using (Pen pen = new Pen(Color.Purple, 3))
                    g.DrawLine(pen, edgeA.Value, edgeB.Value);
            }

            if (hasSecondEdge)
            {
                using (Pen pen = new Pen(Color.Blue, 2))
                    g.DrawLine(pen, secondEdgeStart, secondEdgeEnd);
            }

            if (intersectionPoint.HasValue)
            {
                PointF ip = intersectionPoint.Value;
                g.FillEllipse(Brushes.Red, ip.X - 5, ip.Y - 5, 10, 10);
            }

            if (testPoint.HasValue)
            {
                PointF tp = testPoint.Value;
                g.FillEllipse(Brushes.LimeGreen, tp.X - 5, tp.Y - 5, 10, 10);
            }
        }

        partial void ClearTools()
        {
            testPoint = null;
            edgeA = null;
            edgeB = null;
            drawingSecondEdge = false;
            hasSecondEdge = false;
            secondEdgeStart = PointF.Empty;
            secondEdgeEnd = PointF.Empty;
            intersectionPoint = null;
        }

        private void UpdateIntersection()
        {
            intersectionPoint = null;

            if (!edgeA.HasValue || !edgeB.HasValue)
                return;

            PointF a = edgeA.Value;
            PointF b = edgeB.Value;
            PointF c = secondEdgeStart;
            PointF d = secondEdgeEnd;

            float abx = b.X - a.X;
            float aby = b.Y - a.Y;
            float cdx = d.X - c.X;
            float cdy = d.Y - c.Y;
            float acx = a.X - c.X;
            float acy = a.Y - c.Y;

            float den = abx * cdy - aby * cdx;
            if (Math.Abs(den) < 1e-6f)
                return;

            float t = (cdx * acy - cdy * acx) / den;
            float u = (abx * acy - aby * acx) / den;

            if (t < 0 || t > 1 || u < 0 || u > 1)
                return;

            intersectionPoint = new PointF(a.X + t * abx, a.Y + t * aby);
        }

        private bool IsAltPressed()
        {
            return altDown || (ModifierKeys & Keys.Alt) == Keys.Alt;
        }

        private static bool IsConvexPolygon(List<PointF> pts)
        {
            int n = CountVertices(pts);
            if (n < 3)
                return false;

            int sign = 0;

            for (int i = 0; i < n; i++)
            {
                PointF p0 = pts[i];
                PointF p1 = pts[(i + 1) % n];
                PointF p2 = pts[(i + 2) % n];

                float dx1 = p1.X - p0.X;
                float dy1 = p1.Y - p0.Y;
                float dx2 = p2.X - p1.X;
                float dy2 = p2.Y - p1.Y;

                float cross = dx1 * dy2 - dy1 * dx2;
                if (Math.Abs(cross) < 1e-3f)
                    continue;

                int s = cross > 0 ? 1 : -1;
                if (sign == 0)
                    sign = s;
                else if (sign != s)
                    return false;
            }

            return true;
        }

        private static bool PointInConvexPolygon(PointF p, List<PointF> pts)
        {
            int n = CountVertices(pts);
            if (n < 3)
                return false;

            int sign = 0;

            for (int i = 0; i < n; i++)
            {
                PointF a = pts[i];
                PointF b = pts[(i + 1) % n];

                float xa = b.X - a.X;
                float ya = b.Y - a.Y;
                float xb = p.X - a.X;
                float yb = p.Y - a.Y;

                float value = xa * yb - ya * xb;

                if (Math.Abs(value) < 1e-3f)
                    continue;

                int s = value > 0 ? 1 : -1;
                if (sign == 0)
                    sign = s;
                else if (sign != s)
                    return false;
            }

            return true;
        }

        private static bool PointOnBoundary(PointF p, List<PointF> pts, float tol)
        {
            for (int i = 0; i < pts.Count - 1; i++)
            {
                if (Geometry2D.DistancePointSegment(p, pts[i], pts[i + 1]) <= tol)
                    return true;
            }

            return false;
        }

        private static int CountVertices(List<PointF> pts)
        {
            int n = pts.Count;
            if (n > 1 && pts[0] == pts[n - 1])
                n--;
            return n;
        }
    }
}
