using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace lab_cg_3
{
    public partial class Task1 : Form
    {
        PictureBox canvas;
        ComboBox mode;
        Button btnClear, btnPattern, btnImage, btnBorder, btnFill;
        Label info;

        Bitmap bmp;
        Bitmap? pattern;
        Color borderColor = Color.Black;
        Color fillColor = Color.DodgerBlue;

        bool drawing;
        Point lastPt;
        const int BrushSize = 8;

        int GetMode()
        {
            return mode.SelectedIndex;
        }

        public Task1()
        {
            InitializeComponent();

            Text = "Задание 1";
            Size = new Size(1000, 650);
            StartPosition = FormStartPosition.CenterScreen;

            var panel = new Panel();
            panel.Dock = DockStyle.Right;
            panel.Width = 220;

            mode = new ComboBox();
            mode.DropDownStyle = ComboBoxStyle.DropDownList;
            mode.Location = new Point(10, 15);
            mode.Width = 190;
            mode.Items.Add("Рисование");
            mode.Items.Add("1а заливка цветом");
            mode.Items.Add("1б заливка рисунком");
            mode.Items.Add("1в обход границы");
            mode.SelectedIndex = 0;

            btnBorder = MkBtn("Цвет границы", 10, 55, borderColor);
            btnFill = MkBtn("Цвет заливки", 10, 95, fillColor);
            btnPattern = MkBtn("Рисунок (1б)", 10, 135, SystemColors.Control);
            btnImage = MkBtn("Картинка (1в)", 10, 175, SystemColors.Control);
            btnClear = MkBtn("Очистить", 10, 215, SystemColors.Control);

            info = new Label();
            info.Location = new Point(10, 270);
            info.Size = new Size(190, 120);
            info.Text = "Рисуем границу ЛКМ.\nЗаливка/обход — клик.";

            canvas = new PictureBox();
            canvas.Dock = DockStyle.Fill;
            canvas.BackColor = Color.White;
            canvas.BorderStyle = BorderStyle.FixedSingle;

            panel.Controls.Add(mode);
            panel.Controls.Add(btnBorder);
            panel.Controls.Add(btnFill);
            panel.Controls.Add(btnPattern);
            panel.Controls.Add(btnImage);
            panel.Controls.Add(btnClear);
            panel.Controls.Add(info);

            Controls.Add(canvas);
            Controls.Add(panel);

            bmp = new Bitmap(800, 600);
            ClearBmp();
            canvas.Image = bmp;

            btnBorder.Click += BtnBorder_Click;
            btnFill.Click += BtnFill_Click;
            btnPattern.Click += BtnPattern_Click;
            btnImage.Click += BtnImage_Click;
            btnClear.Click += BtnClear_Click;

            canvas.MouseDown += Canvas_MouseDown;
            canvas.MouseMove += Canvas_MouseMove;
            canvas.MouseUp += Canvas_MouseUp;
            canvas.MouseClick += Canvas_MouseClick;
            canvas.SizeChanged += Canvas_SizeChanged;
        }

        void BtnBorder_Click(object? sender, EventArgs e)
        {
            PickColor(ref borderColor, btnBorder);
        }

        void BtnFill_Click(object? sender, EventArgs e)
        {
            PickColor(ref fillColor, btnFill);
        }

        void BtnPattern_Click(object? sender, EventArgs e)
        {
            LoadPattern();
        }

        void BtnImage_Click(object? sender, EventArgs e)
        {
            LoadImage();
        }

        void BtnClear_Click(object? sender, EventArgs e)
        {
            ClearBmp();
            canvas.Invalidate();
        }

        void Canvas_MouseUp(object? sender, MouseEventArgs e)
        {
            drawing = false;
        }

        void Canvas_SizeChanged(object? sender, EventArgs e)
        {
            if (canvas.Width < 1 || canvas.Height < 1)
                return;

            Bitmap old = bmp;
            bmp = new Bitmap(canvas.Width, canvas.Height);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                g.DrawImage(old, 0, 0);
            }

            old.Dispose();
            canvas.Image = bmp;
        }

        Button MkBtn(string text, int x, int y, Color back)
        {
            Button button = new Button();
            button.Text = text;
            button.Location = new Point(x, y);
            button.Size = new Size(190, 32);
            button.BackColor = back;
            return button;
        }

        void PickColor(ref Color c, Button b)
        {
            ColorDialog d = new ColorDialog();
            if (d.ShowDialog() == DialogResult.OK)
            {
                c = d.Color;
                b.BackColor = c;
            }
            d.Dispose();
        }

        void ClearBmp()
        {
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
            }
        }

        void LoadPattern()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Img|*.png;*.bmp;*.jpg;*.jpeg";

            if (ofd.ShowDialog() != DialogResult.OK)
            {
                ofd.Dispose();
                return;
            }

            if (pattern != null)
                pattern.Dispose();

            pattern = new Bitmap(ofd.FileName);
            info.Text = "Рисунок " + pattern.Width + "x" + pattern.Height;
            ofd.Dispose();
        }

        void LoadImage()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Img|*.png;*.bmp;*.jpg;*.jpeg";

            if (ofd.ShowDialog() != DialogResult.OK)
            {
                ofd.Dispose();
                return;
            }

            Bitmap img = new Bitmap(ofd.FileName);
            ClearBmp();

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.DrawImage(img, 0, 0);
            }

            img.Dispose();
            ofd.Dispose();

            mode.SelectedIndex = 3;
            canvas.Invalidate();
        }

        void Canvas_MouseDown(object? sender, MouseEventArgs e)
        {
            if (GetMode() != 0 || e.Button != MouseButtons.Left)
                return;

            drawing = true;
            lastPt = e.Location;
            StampBrush(e.X, e.Y);
            canvas.Invalidate();
        }

        void Canvas_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!drawing || GetMode() != 0)
                return;

            DrawThickLine(lastPt.X, lastPt.Y, e.X, e.Y);
            lastPt = e.Location;
            canvas.Invalidate();
        }

        void StampBrush(int cx, int cy)
        {
            int r = BrushSize / 2;
            int r2 = r * r;
            for (int dy = -r; dy <= r; dy++)
            for (int dx = -r; dx <= r; dx++)
                if (dx * dx + dy * dy <= r2)
                    SetPix(cx + dx, cy + dy, borderColor);
        }

        void DrawThickLine(int x0, int y0, int x1, int y1)
        {
            int dx = Math.Abs(x1 - x0), dy = Math.Abs(y1 - y0);
            int steps = Math.Max(dx, dy);
            if (steps == 0)
            {
                StampBrush(x0, y0);
                return;
            }
            for (int i = 0; i <= steps; i++)
            {
                int x = x0 + (x1 - x0) * i / steps;
                int y = y0 + (y1 - y0) * i / steps;
                StampBrush(x, y);
            }
        }

        void Canvas_MouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || GetMode() == 0)
                return;

            if (e.X < 0 || e.Y < 0 || e.X >= bmp.Width || e.Y >= bmp.Height)
                return;

            if (GetMode() == 1)
            {
                FillSpans(e.X, e.Y, false);
            }
            else if (GetMode() == 2)
            {
                if (pattern == null)
                {
                    MessageBox.Show("Загрузите рисунок");
                    return;
                }
                FillSpans(e.X, e.Y, true);
            }
            else if (GetMode() == 3)
            {
                TraceBorder(e.X, e.Y);
            }

            canvas.Invalidate();
        }

        void SetPix(int x, int y, Color c)
        {
            if (x >= 0 && y >= 0 && x < bmp.Width && y < bmp.Height)
                bmp.SetPixel(x, y, c);
        }

        bool Eq(Color a, Color b)
        {
            return a.R == b.R && a.G == b.G && a.B == b.B;
        }

        void FillSpans(int x, int y, bool usePattern)
        {
            Color old = bmp.GetPixel(x, y);
            if (Eq(old, borderColor)) return;
            if (!usePattern && Eq(old, fillColor)) return;

            bool tile = pattern != null &&
                        (pattern.Width < bmp.Width || pattern.Height < bmp.Height);

            bool[,] visited = new bool[bmp.Width, bmp.Height];
            FillSpanRec(x, y, old, usePattern, tile, x, y, visited);
        }

        void FillSpanRec(int x, int y, Color old, bool usePat, bool tile,
                         int sx, int sy, bool[,] visited)
        {
            if (y < 0 || y >= bmp.Height) return;
            if (x < 0 || x >= bmp.Width) return;
            if (visited[x, y]) return;
            if (!CanFill(bmp.GetPixel(x, y), old, usePat)) return;

            int L = x;
            while (L > 0 && !visited[L - 1, y] && CanFill(bmp.GetPixel(L - 1, y), old, usePat))
                L--;
            int R = x;
            while (R < bmp.Width - 1 && !visited[R + 1, y] && CanFill(bmp.GetPixel(R + 1, y), old, usePat))
                R++;

            for (int i = L; i <= R; i++)
            {
                visited[i, y] = true;
                if (usePat && pattern != null)
                {
                    Color? pc = PatColor(i, y, sx, sy, tile);
                    if (pc.HasValue)
                        bmp.SetPixel(i, y, pc.Value);
                }
                else
                {
                    bmp.SetPixel(i, y, fillColor);
                }
            }

            for (int i = L; i <= R; i++)
            {
                FillSpanRec(i, y - 1, old, usePat, tile, sx, sy, visited);
                FillSpanRec(i, y + 1, old, usePat, tile, sx, sy, visited);
            }
        }

        bool CanFill(Color c, Color old, bool usePat)
        {
            if (Eq(c, borderColor)) return false;
            if (!usePat && Eq(c, fillColor)) return false;
            return Eq(c, old);
        }

        Color? PatColor(int x, int y, int sx, int sy, bool tile)
        {
            if (pattern == null) return null;
            if (tile)
            {
                int px = ((x % pattern.Width) + pattern.Width) % pattern.Width;
                int py = ((y % pattern.Height) + pattern.Height) % pattern.Height;
                return pattern.GetPixel(px, py);
            }
            int dx = x - sx, dy = y - sy;
            if (dx < 0 || dy < 0 || dx >= pattern.Width || dy >= pattern.Height)
                return null;
            return pattern.GetPixel(dx, dy);
        }

        void TraceBorder(int x, int y)
        {
            Color bc = bmp.GetPixel(x, y);
            if (Eq(bc, Color.White))
            {
                bool found = false;
                for (int r = 1; r <= 20 && !found; r++)
                for (int dx = -r; dx <= r && !found; dx++)
                for (int dy = -r; dy <= r && !found; dy++)
                {
                    int nx = x + dx, ny = y + dy;
                    if (nx < 0 || ny < 0 || nx >= bmp.Width || ny >= bmp.Height) continue;
                    if (Eq(bmp.GetPixel(nx, ny), borderColor))
                    {
                        x = nx; y = ny; bc = borderColor; found = true;
                    }
                }
                if (!found)
                {
                    MessageBox.Show("Кликните по границе");
                    return;
                }
            }

            var points = new List<Point>();
            int[] dx8 = { 1, 1, 0, -1, -1, -1, 0, 1 };
            int[] dy8 = { 0, 1, 1, 1, 0, -1, -1, -1 };

            int cx = x, cy = y, dir = 6;
            points.Add(new Point(cx, cy));

            for (int step = 0; step < bmp.Width * bmp.Height; step++)
            {
                bool ok = false;
                int look = (dir + 6) % 8;
                for (int k = 0; k < 8; k++)
                {
                    int nd = (look + k) % 8;
                    int nx = cx + dx8[nd], ny = cy + dy8[nd];
                    if (nx < 0 || ny < 0 || nx >= bmp.Width || ny >= bmp.Height) continue;
                    if (!Eq(bmp.GetPixel(nx, ny), bc)) continue;

                    cx = nx; cy = ny; dir = nd;
                    points.Add(new Point(cx, cy));
                    ok = true;
                    break;
                }
                if (!ok) break;
                if (cx == x && cy == y && points.Count > 2) break;
            }

            foreach (var p in points)
                SetPix(p.X, p.Y, Color.Red);

            info.Text = $"Граница: {points.Count} точек";
        }
    }
}
