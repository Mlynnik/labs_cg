using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace lab_cg_3
{
    public partial class Task2 : Form
    {
        private PictureBox pictureCanvas;
        private ComboBox comboAlgorithm;
        private Button buttonClear;
        private Label labelAlgorithm;
        private Label labelInfo;
        private Panel controlPanel;

        private Bitmap Canvas;
        private Point? FirstPoint = null;

        private List<(Point StartPoint, Point EndPoint, int Alg)> Lines = new List<(Point StartPoint, Point EndPoint, int Alg)>();

        public Task2()
        {
            InitializeComponent();

            Text = "Алгоритмы Брезенхема и Ву";
            Size = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(800, 500);

            controlPanel = new Panel();
            controlPanel.Dock = DockStyle.Right;
            controlPanel.Width = 230;
            controlPanel.Padding = new Padding(20);

            labelAlgorithm = new Label();
            labelAlgorithm.Text = "Алгоритм рисования:";
            labelAlgorithm.Location = new Point(20, 30);
            labelAlgorithm.Size = new Size(190, 25);

            comboAlgorithm = new ComboBox();
            comboAlgorithm.Location = new Point(20, 60);
            comboAlgorithm.Size = new Size(190, 30);
            comboAlgorithm.DropDownStyle = ComboBoxStyle.DropDownList;
            comboAlgorithm.Items.Add("Брезенхем");
            comboAlgorithm.Items.Add("Ву");
            comboAlgorithm.SelectedIndex = 0;

            buttonClear = new Button();
            buttonClear.Text = "Очистить";
            buttonClear.Location = new Point(20, 110);
            buttonClear.Size = new Size(190, 40);

            labelInfo = new Label();
            labelInfo.Text = "ЛКМ — выбрать начало и конец\nПКМ — отменить первую точку";
            labelInfo.Location = new Point(20, 180);
            labelInfo.Size = new Size(190, 60);

            pictureCanvas = new PictureBox();
            pictureCanvas.Dock = DockStyle.Fill;
            pictureCanvas.BackColor = Color.White;
            pictureCanvas.BorderStyle = BorderStyle.FixedSingle;
            pictureCanvas.SizeMode = PictureBoxSizeMode.Normal;

            controlPanel.Controls.Add(labelAlgorithm);
            controlPanel.Controls.Add(comboAlgorithm);
            controlPanel.Controls.Add(buttonClear);
            controlPanel.Controls.Add(labelInfo);

            Controls.Add(pictureCanvas);
            Controls.Add(controlPanel);

            Canvas = new Bitmap(pictureCanvas.Width, pictureCanvas.Height);
            pictureCanvas.Image = Canvas;

            ClearCanvas();

            pictureCanvas.MouseClick += PictureCanvas_MouseClick;
            pictureCanvas.SizeChanged += PictureCanvas_SizeChanged;
            buttonClear.Click += ButtonClear_Click;
        }

        private void ClearCanvas()
        {
            using (Graphics g = Graphics.FromImage(Canvas))
            {
                g.Clear(Color.White);
            }
        }

        private void PictureCanvas_SizeChanged(object sender, EventArgs e)
        {
            if (pictureCanvas.Width <= 0 || pictureCanvas.Height <= 0)
                return;

            if (Canvas != null)
                Canvas.Dispose();

            Canvas = new Bitmap(pictureCanvas.Width, pictureCanvas.Height);
            RedrawAll();
        }

        private void ButtonClear_Click(object sender, EventArgs e)
        {
            Lines.Clear();
            FirstPoint = null;
            RedrawAll();
        }

        private void PictureCanvas_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                FirstPoint = null;
                return;
            }

            if (e.Button != MouseButtons.Left)
                return;

            Point currentPoint = new Point(e.X, e.Y);

            if (FirstPoint == null)
            {
                FirstPoint = currentPoint;
                return;
            }

            Lines.Add((FirstPoint.Value, currentPoint, comboAlgorithm.SelectedIndex));
            FirstPoint = null;
            RedrawAll();
        }

        private void RedrawAll()
        {
            ClearCanvas();

            foreach (var line in Lines)
            {
                if (line.Alg == 0)
                    DrawBresenham(line.StartPoint.X, line.StartPoint.Y, line.EndPoint.X, line.EndPoint.Y);
                else
                    DrawWu(line.StartPoint.X, line.StartPoint.Y, line.EndPoint.X, line.EndPoint.Y);
            }

            pictureCanvas.Image = Canvas;
            pictureCanvas.Invalidate();
        }

        private void DrawBresenham(int x0, int y0, int x1, int y1)
        {
            int dx = Math.Abs(x1 - x0);
            int sx = x0 < x1 ? 1 : -1;

            int dy = -Math.Abs(y1 - y0);
            int sy = y0 < y1 ? 1 : -1;

            int error = dx + dy;

            while (true)
            {
                DrawPixel(x0, y0, Color.Black);

                if (x0 == x1 && y0 == y1)
                    break;

                int e2 = 2 * error;

                if (e2 >= dy)
                {
                    error += dy;
                    x0 += sx;
                }

                if (e2 <= dx)
                {
                    error += dx;
                    y0 += sy;
                }
            }
        }

        private void DrawWu(int x0, int y0, int x1, int y1)
        {
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);

            if (steep)
            {
                Swap(ref x0, ref y0);
                Swap(ref x1, ref y1);
            }

            if (x0 > x1)
            {
                Swap(ref x0, ref x1);
                Swap(ref y0, ref y1);
            }

            double dx = x1 - x0;
            double dy = y1 - y0;

            if (dx == 0)
            {
                if (steep)
                    DrawPixel(y0, x0, Color.Black);
                else
                    DrawPixel(x0, y0, Color.Black);

                return;
            }

            double gradient = dy / dx;
            double y = y0;

            for (int x = x0; x <= x1; x++)
            {
                int yInteger = (int)Math.Floor(y);
                double fraction = y - Math.Floor(y);

                double intensity1 = 1 - fraction;
                double intensity2 = fraction;

                if (steep)
                {
                    DrawWuPixel(yInteger, x, intensity1);
                    DrawWuPixel(yInteger + 1, x, intensity2);
                }
                else
                {
                    DrawWuPixel(x, yInteger, intensity1);
                    DrawWuPixel(x, yInteger + 1, intensity2);
                }

                y += gradient;
            }
        }

        private void DrawWuPixel(int x, int y, double intensity)
        {
            if (x < 0 || y < 0 || x >= Canvas.Width || y >= Canvas.Height)
                return;

            intensity = Math.Max(0, Math.Min(1, intensity));

            int value = 255 - (int)Math.Round(255 * intensity);
            Canvas.SetPixel(x, y, Color.FromArgb(value, value, value));
        }

        private void DrawPixel(int x, int y, Color color)
        {
            if (x < 0 || y < 0 || x >= Canvas.Width || y >= Canvas.Height)
                return;

            Canvas.SetPixel(x, y, color);
        }

        private void Swap(ref int a, ref int b)
        {
            int temp = a;
            a = b;
            b = temp;
        }
    }
}