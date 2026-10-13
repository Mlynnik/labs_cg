using System.Drawing.Drawing2D;

namespace lab_cg_5
{
    public partial class Task3 : Form
    {
        private readonly List<PointF> points = new List<PointF>();
        private int selected = -1;
        private int dragging = -1;

        private const float HitRadius = 12f;

        public Task3()
        {
            InitializeComponent();

            canvas.Paint += Canvas_Paint;
            canvas.MouseDown += Canvas_MouseDown;
            canvas.MouseMove += Canvas_MouseMove;
            canvas.MouseUp += Canvas_MouseUp;

            btnDelete.Click += BtnDelete_Click;
            btnClear.Click += BtnClear_Click;
            KeyDown += Form_KeyDown;

            RefreshView();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            DeleteSelected();
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            points.Clear();
            selected = -1;
            dragging = -1;
            RefreshView();
        }

        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete)
                return;

            DeleteSelected();
            e.Handled = true;
        }

        private void Canvas_MouseDown(object sender, MouseEventArgs e)
        {
            int hit = HitTest(e.Location);

            if (e.Button == MouseButtons.Right)
            {
                if (hit >= 0)
                {
                    points.RemoveAt(hit);
                    selected = -1;
                    dragging = -1;
                    RefreshView();
                }

                return;
            }

            if (e.Button != MouseButtons.Left)
                return;

            if (hit >= 0)
            {
                selected = hit;
            }
            else
            {
                points.Add(ClampToCanvas(e.Location));
                selected = points.Count - 1;
            }

            dragging = selected;
            canvas.Capture = true;
            RefreshView();
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging < 0)
                return;

            points[dragging] = ClampToCanvas(e.Location);
            canvas.Invalidate();
        }

        private void Canvas_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = -1;
            canvas.Capture = false;
        }

        private void DeleteSelected()
        {
            if (selected < 0 || selected >= points.Count)
                return;

            points.RemoveAt(selected);
            selected = -1;
            dragging = -1;
            RefreshView();
        }

        private void RefreshView()
        {
            lblCount.Text = selected >= 0
                ? $"Точек: {points.Count}\nВыбрана: {selected + 1}"
                : $"Точек: {points.Count}";

            btnDelete.Enabled = selected >= 0;
            btnClear.Enabled = points.Count > 0;
            canvas.Invalidate();
        }

        private int HitTest(Point location)
        {
            int hit = -1;
            float best = HitRadius;

            for (int i = 0; i < points.Count; i++)
            {
                float dist = Distance(points[i], location);
                if (dist <= best)
                {
                    best = dist;
                    hit = i;
                }
            }

            return hit;
        }

        private PointF ClampToCanvas(Point location)
        {
            float x = location.X;
            float y = location.Y;
            float maxX = canvas.ClientSize.Width - 1;
            float maxY = canvas.ClientSize.Height - 1;
            if (maxX < 0) maxX = 0;
            if (maxY < 0) maxY = 0;
            if (x < 0) x = 0;
            if (y < 0) y = 0;
            if (x > maxX) x = maxX;
            if (y > maxY) y = maxY;
            return new PointF(x, y);
        }

        private void Canvas_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            if (points.Count == 0)
            {
                using (Font hintFont = new Font("Segoe UI", 12))
                {
                    g.DrawString("Кликните, чтобы поставить первую точку", hintFont, Brushes.Gray, 24, 24);
                }
                return;
            }

            if (points.Count >= 2)
            {
                PointF[] bezier = BuildBezierControls(points);

                using (Pen guide = new Pen(Color.LightGray, 1))
                {
                    guide.DashStyle = DashStyle.Dash;
                    g.DrawLines(guide, points.ToArray());
                }

                using (Pen curve = new Pen(Color.Black, 2))
                {
                    g.DrawBeziers(curve, bezier);
                }
            }

            using (Font indexFont = new Font("Segoe UI", 9))
            {
                for (int i = 0; i < points.Count; i++)
                {
                    bool active = i == selected;
                    float radius = active ? 8f : 6f;
                    PointF p = points[i];
                    RectangleF marker = new RectangleF(p.X - radius, p.Y - radius, radius * 2, radius * 2);

                    g.FillEllipse(active ? Brushes.Blue : Brushes.Red, marker);
                    using (Pen outline = new Pen(Color.Black, 1))
                    {
                        g.DrawEllipse(outline, marker);
                    }
                    g.DrawString((i + 1).ToString(), indexFont, Brushes.Black, p.X + 10, p.Y - 12);
                }
            }
        }

        private static PointF[] BuildBezierControls(List<PointF> anchors)
        {
            int last = anchors.Count - 1;
            float[] h = new float[last];

            for (int i = 0; i < last; i++)
            {
                float dx = anchors[i].X - anchors[i + 1].X;
                float dy = anchors[i].Y - anchors[i + 1].Y;
                float distance = (float)Math.Sqrt(dx * dx + dy * dy);
                h[i] = distance < 0.001f ? 0.001f : distance;
            }

            PointF[] second = new PointF[last + 1];

            if (last >= 2)
            {
                int unknown = last - 1;
                float[] lower = new float[unknown];
                float[] diag = new float[unknown];
                float[] upper = new float[unknown];
                PointF[] rhs = new PointF[unknown];

                for (int k = 0; k < unknown; k++)
                {
                    int i = k + 1;
                    lower[k] = h[i - 1];
                    diag[k] = 2f * (h[i - 1] + h[i]);
                    upper[k] = h[i];

                    PointF slopeNext = Scale(Sub(anchors[i + 1], anchors[i]), 1f / h[i]);
                    PointF slopePrev = Scale(Sub(anchors[i], anchors[i - 1]), 1f / h[i - 1]);
                    rhs[k] = Scale(Sub(slopeNext, slopePrev), 6f);
                }

                lower[0] = 0f;
                upper[unknown - 1] = 0f;

                PointF[] solved = SolveTridiagonal(lower, diag, upper, rhs);
                for (int k = 0; k < unknown; k++)
                    second[k + 1] = solved[k];
            }

            PointF[] controls = new PointF[1 + 3 * last];
            int cursor = 0;

            for (int i = 0; i < last; i++)
            {
                PointF slope = Scale(Sub(anchors[i + 1], anchors[i]), 1f / h[i]);
                PointF startTangent = Sub(slope, Scale(Add(Scale(second[i], 2f), second[i + 1]), h[i] / 6f));
                PointF endTangent = Add(slope, Scale(Add(second[i], Scale(second[i + 1], 2f)), h[i] / 6f));

                PointF b0 = anchors[i];
                PointF b1 = Add(anchors[i], Scale(startTangent, h[i] / 3f));
                PointF b2 = Sub(anchors[i + 1], Scale(endTangent, h[i] / 3f));
                PointF b3 = anchors[i + 1];

                if (i == 0)
                    controls[cursor++] = b0;

                controls[cursor++] = b1;
                controls[cursor++] = b2;
                controls[cursor++] = b3;
            }

            return controls;
        }

        private static PointF[] SolveTridiagonal(float[] lower, float[] diag, float[] upper, PointF[] rhs)
        {
            int n = diag.Length;
            float[] sweep = new float[n];
            PointF[] value = new PointF[n];

            sweep[0] = upper[0] / diag[0];
            value[0] = Scale(rhs[0], 1f / diag[0]);

            for (int i = 1; i < n; i++)
            {
                float denominator = diag[i] - lower[i] * sweep[i - 1];
                sweep[i] = upper[i] / denominator;
                value[i] = Scale(Sub(rhs[i], Scale(value[i - 1], lower[i])), 1f / denominator);
            }

            PointF[] solution = new PointF[n];
            solution[n - 1] = value[n - 1];

            for (int i = n - 2; i >= 0; i--)
                solution[i] = Sub(value[i], Scale(solution[i + 1], sweep[i]));

            return solution;
        }

        private static PointF Add(PointF a, PointF b)
        {
            return new PointF(a.X + b.X, a.Y + b.Y);
        }

        private static PointF Sub(PointF a, PointF b)
        {
            return new PointF(a.X - b.X, a.Y - b.Y);
        }

        private static PointF Scale(PointF a, float s)
        {
            return new PointF(a.X * s, a.Y * s);
        }

        private static float Distance(PointF a, Point b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        private class BufferedPanel : Panel
        {
            public BufferedPanel()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
            }
        }
    }
}
