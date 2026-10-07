using lab_cg_4.Geometry;

namespace lab_cg_4
{
    public partial class Form1 : Form
    {
        Panel canvas;
        Button btnClear;
        Button btnFinish;
        Label lblInfo;

        List<Polygon> polygons = new List<Polygon>();
        Polygon current = null;
        Polygon selected = null;

        bool altDown = false;
        bool suppressNextCanvasClick = false;

        partial void InitTransformUI(int sx);
        partial void InitToolsUI(int sx);

        partial void HandleControlClick(PointF p, ref bool handled);
        partial void HandleShiftClick(PointF p, ref bool handled);
        partial void HandleTransformClick(PointF p, ref bool handled);

        partial void HandleAltMouseDown(MouseEventArgs e, ref bool handled);
        partial void HandleAltMouseMove(MouseEventArgs e, ref bool handled);
        partial void HandleAltMouseUp(MouseEventArgs e, ref bool handled);

        partial void DrawTools(Graphics g);
        partial void ClearTools();

        public Form1()
        {
            Text = "Polygon affine";
            Width = 1100;
            Height = 700;

            InitUI();

            DoubleBuffered = true;
            KeyPreview = true;
            KeyDown += MainForm_KeyDown;
            KeyUp += MainForm_KeyUp;
        }

        void InitUI()
        {
            canvas = new Panel
            {
                Left = 10,
                Top = 10,
                Width = 800,
                Height = 640,
                BackColor = Color.White
            };

            canvas.Paint += Canvas_Paint;
            canvas.MouseClick += Canvas_MouseClick;
            canvas.MouseDoubleClick += Canvas_MouseDoubleClick;
            canvas.MouseDown += Canvas_MouseDown;
            canvas.MouseMove += Canvas_MouseMove;
            canvas.MouseUp += Canvas_MouseUp;

            Controls.Add(canvas);

            int sx = 820;

            btnClear = new Button
            {
                Left = sx,
                Top = 10,
                Width = 220,
                Text = "Очистить сцену"
            };

            btnClear.Click += (s, e) =>
            {
                polygons.Clear();
                current = null;
                selected = null;
                ClearTools();
                canvas.Invalidate();
            };

            Controls.Add(btnClear);

            btnFinish = new Button
            {
                Left = sx,
                Top = 50,
                Width = 220,
                Text = "Завершить полигон (или двойной клик)"
            };

            btnFinish.Click += (s, e) => FinishCurrent();
            Controls.Add(btnFinish);

            lblInfo = new Label
            {
                Left = sx,
                Top = 500,
                Width = 350,
                Height = 200,
                Text =
                    "Управление:\n" +
                    "- ЛКМ: добавление вершин\n" +
                    "- Двойной клик: завершить\n" +
                    "- Клик внутри/рядом: выбрать полигон\n" +
                    "- Ctrl+Click: проверить принадлежность\n" +
                    "- Shift+Click: позиция относительно ребра\n" +
                    "- Alt+drag: нарисовать ребро и найти пересечение"
            };

            Controls.Add(lblInfo);

            InitTransformUI(sx);
            InitToolsUI(sx);
        }

        void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Menu)
                altDown = true;
        }

        void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Menu)
                altDown = false;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Menu || keyData == (Keys.Alt | Keys.Menu))
                return true;

            return base.ProcessCmdKey(ref msg, keyData);
        }

        void Canvas_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            FinishCurrent();
        }

        void FinishCurrent()
        {
            if (current == null)
                return;

            if (current.pts.Count == 1 || current.pts.Count == 2)
            {
                polygons.Add(current);
            }
            else if (current.pts.Count > 2)
            {
                if (current.pts[0] != current.pts.Last())
                    current.pts.Add(current.pts[0]);

                polygons.Add(current);
            }

            current = null;
            canvas.Invalidate();
        }

        void Canvas_MouseDown(object sender, MouseEventArgs e)
        {
            bool handled = false;
            HandleAltMouseDown(e, ref handled);
        }

        void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            bool handled = false;
            HandleAltMouseMove(e, ref handled);
        }

        void Canvas_MouseUp(object sender, MouseEventArgs e)
        {
            bool handled = false;
            HandleAltMouseUp(e, ref handled);
        }

        void Canvas_MouseClick(object sender, MouseEventArgs e)
        {
            if (suppressNextCanvasClick)
            {
                suppressNextCanvasClick = false;
                return;
            }

            if (altDown)
                return;

            PointF p = e.Location;

            if (ModifierKeys == Keys.Control)
            {
                bool handled = false;
                HandleControlClick(p, ref handled);
                return;
            }

            if (ModifierKeys == Keys.Shift)
            {
                bool handled = false;
                HandleShiftClick(p, ref handled);
                return;
            }

            bool transformHandled = false;
            HandleTransformClick(p, ref transformHandled);

            if (transformHandled)
                return;

            for (int i = polygons.Count - 1; i >= 0; i--)
            {
                var poly = polygons[i];

                if (poly.HitTest(p, 6f))
                {
                    selected = poly;

                    foreach (var q in polygons)
                        q.Selected = false;

                    poly.Selected = true;
                    canvas.Invalidate();
                    return;
                }
            }

            if (current == null)
                current = new Polygon();

            current.pts.Add(p);
            canvas.Invalidate();
        }

        void Canvas_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            foreach (var poly in polygons)
                poly.Draw(g);

            if (current != null)
                current.Draw(g, true);

            DrawTools(g);
        }
    }
}
