using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace lab_cg_5
{
    public partial class Task1 : Form
    {
        TextBox txtIter;
        CheckBox chkRandom, chkTree;
        Button btnLoad;
        PictureBox pic;

        string axiom = "";
        double angle, startDir;
        readonly Dictionary<char, string> rules = new Dictionary<char, string>();

        string lastCommands = null;
        bool lastIsTree = false;
        readonly Random rnd = new Random();

        public Task1()
        {
            Text = "L-системы (1a / 1b)";
            Width = 900; Height = 700;

            pic = new PictureBox { Dock = DockStyle.Fill, BackColor = Color.White };

            var panel = new Panel { Dock = DockStyle.Top, Height = 40 };
            txtIter = new TextBox { Left = 10, Top = 8, Width = 40, Text = "5" };
            chkRandom = new CheckBox { Left = 60, Top = 8, Width = 110, Text = "Случайность" };
            chkTree = new CheckBox { Left = 175, Top = 8, Width = 110, Text = "Дерево (1b)" };
            btnLoad = new Button { Left = 290, Top = 6, Width = 180, Text = "Загрузить L-систему" };
            btnLoad.Click += BtnLoad_Click;
            panel.Controls.AddRange(new Control[] { txtIter, chkRandom, chkTree, btnLoad });

            Controls.Add(pic);
            Controls.Add(panel);

            var resizeTimer = new System.Windows.Forms.Timer { Interval = 100 };
            resizeTimer.Tick += (s, e) =>
            {
                resizeTimer.Stop();
                Redraw();
            };

            pic.Resize += (s, e) =>
            {
                resizeTimer.Stop();
                resizeTimer.Start();
            };

            txtIter.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (int.TryParse(txtIter.Text, out _))
                        RebuildAndRedraw();
                    e.SuppressKeyPress = true;
                }
                
            };

            chkRandom.CheckedChanged += (s, e) => Redraw();
            chkTree.CheckedChanged += (s, e) => RebuildAndRedraw();
        }

        void Redraw()
        {
            if (string.IsNullOrEmpty(lastCommands)) return;
            if (pic.Width <= 0 || pic.Height <= 0) return;

            if (lastIsTree) DrawTree(lastCommands);
            else DrawL(lastCommands);
        }

        void RebuildAndRedraw()
        {
            if (string.IsNullOrEmpty(axiom)) return;
            if (!int.TryParse(txtIter.Text, out int iter) || iter < 0 || iter > 10)
                return;

            string s = axiom;
            for (int i = 0; i < iter; i++) s = Rewrite(s);

            lastCommands = s;
            lastIsTree = chkTree.Checked;
            Redraw();
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog { Filter = "txt|*.txt" })
            {
                if (ofd.ShowDialog() != DialogResult.OK) return;
                var lines = File.ReadAllLines(ofd.FileName);
                var head = lines[0].Split(new[] { ' ', '\t' },
                                          StringSplitOptions.RemoveEmptyEntries);
                axiom = head[0];
                angle = double.Parse(head[1], CultureInfo.InvariantCulture);
                startDir = double.Parse(head[2], CultureInfo.InvariantCulture);

                rules.Clear();
                for (int i = 1; i < lines.Length; i++)
                {
                    var line = lines[i].Replace("→", "->").Trim();
                    if (line.Length == 0) continue;
                    var kv = line.Split(new[] { "->" }, StringSplitOptions.None);
                    if (kv.Length == 2) rules[kv[0].Trim()[0]] = kv[1].Trim();
                }
            }

            RebuildAndRedraw();
        }

        string Rewrite(string s)
        {
            var sb = new System.Text.StringBuilder(s.Length * 2);
            foreach (char c in s)
                sb.Append(rules.ContainsKey(c) ? rules[c] : c.ToString());
            return sb.ToString();
        }
        
        void DrawL(string commands)
        {
            var pts = new List<PointF>();
            double x = 0, y = 0, dir = startDir * Math.PI / 180;
            var stack = new Stack<double[]>();
            double step = 1;

            pts.Add(new PointF(0, 0));

            foreach (char c in commands)
            {
                switch (c)
                {
                    case 'F':
                    case 'G':
                        {
                            double d = dir + (chkRandom.Checked ? (rnd.NextDouble() - 0.5) * 0.35 : 0);
                            x += step * Math.Cos(d);
                            y += step * Math.Sin(d);
                            pts.Add(new PointF((float)x, (float)y));
                        }
                        break;
                    case 'f':
                        x += step * Math.Cos(dir);
                        y += step * Math.Sin(dir);
                        break;
                    case '+':
                        dir += (angle + (chkRandom.Checked ? (rnd.NextDouble() - 0.5) * 20 : 0))
                               * Math.PI / 180;
                        break;
                    case '-':
                        dir -= (angle + (chkRandom.Checked ? (rnd.NextDouble() - 0.5) * 20 : 0))
                               * Math.PI / 180;
                        break;
                    case '[': stack.Push(new[] { x, y, dir }); break;
                    case ']':
                        {
                            var st = stack.Pop();
                            x = st[0]; y = st[1]; dir = st[2];
                        }
                        break;
                }
            }

            RenderLines(pts);
        }

        void RenderLines(List<PointF> pts)
        {
            if (pts.Count < 2) return;
            var bmp = new Bitmap(pic.Width, pic.Height);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                float minX = pts[0].X, maxX = pts[0].X, minY = pts[0].Y, maxY = pts[0].Y;
                foreach (var p in pts)
                {
                    if (p.X < minX) minX = p.X; if (p.X > maxX) maxX = p.X;
                    if (p.Y < minY) minY = p.Y; if (p.Y > maxY) maxY = p.Y;
                }
                float w = Math.Max(maxX - minX, 0.001f);
                float h = Math.Max(maxY - minY, 0.001f);
                float margin = 20;
                float scale = Math.Min((pic.Width - 2 * margin) / w,
                                       (pic.Height - 2 * margin) / h);

                Func<PointF, PointF> tr = p => new PointF(
                    margin + (p.X - minX) * scale,
                    margin + (p.Y - minY) * scale);

                for (int i = 1; i < pts.Count; i++)
                    g.DrawLine(Pens.Black, tr(pts[i - 1]), tr(pts[i]));
            }
            var old = pic.Image; pic.Image = bmp; old?.Dispose();
        }

        void DrawTree(string commands)
        {
            var segs = new List<(PointF a, PointF b, int depth)>();
            double x = 0, y = 0, dir = startDir * Math.PI / 180;
            var stack = new Stack<double[]>();
            double step = 1;
            int depth = 0;

            foreach (char c in commands)
            {
                switch (c)
                {
                    case 'F':
                    case 'G':
                        {
                            double d = dir + (rnd.NextDouble() - 0.5) * 0.4; // случайный угол
                            double nx = x + step * Math.Cos(d);
                            double ny = y + step * Math.Sin(d);
                            segs.Add((new PointF((float)x, (float)y),
                                      new PointF((float)nx, (float)ny),
                                      depth));
                            x = nx; y = ny;
                        }
                        break;
                    case '+':
                        dir += (angle + (rnd.NextDouble() - 0.5) * 20) * Math.PI / 180;
                        break;
                    case '-':
                        dir -= (angle + (rnd.NextDouble() - 0.5) * 20) * Math.PI / 180;
                        break;
                    case '[': stack.Push(new[] { x, y, dir, depth }); depth++; break;
                    case ']':
                        {
                            var st = stack.Pop();
                            x = st[0]; y = st[1]; dir = st[2]; depth = (int)st[3];
                        }
                        break;
                }
            }

            RenderTree(segs);
        }

        void RenderTree(List<(PointF a, PointF b, int depth)> segs)
        {
            if (segs.Count == 0) return;
            var bmp = new Bitmap(pic.Width, pic.Height);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                int maxDepth = 1;
                foreach (var s in segs) if (s.depth > maxDepth) maxDepth = s.depth;

                float minX = segs[0].a.X, maxX = segs[0].a.X;
                float minY = segs[0].a.Y, maxY = segs[0].a.Y;
                foreach (var s in segs)
                {
                    minX = Math.Min(minX, Math.Min(s.a.X, s.b.X));
                    maxX = Math.Max(maxX, Math.Max(s.a.X, s.b.X));
                    minY = Math.Min(minY, Math.Min(s.a.Y, s.b.Y));
                    maxY = Math.Max(maxY, Math.Max(s.a.Y, s.b.Y));
                }
                float w = Math.Max(maxX - minX, 0.001f);
                float h = Math.Max(maxY - minY, 0.001f);
                float margin = 30;
                float scale = Math.Min((pic.Width - 2 * margin) / w,
                                       (pic.Height - 2 * margin) / h);

                Func<PointF, PointF> tr = p => new PointF(
                    margin + (p.X - minX) * scale,
                    margin + (p.Y - minY) * scale);

                Color brown = Color.SaddleBrown;   // основание
                Color green = Color.Green;         // ветви

                foreach (var s in segs)
                {
                    float t = (float)s.depth / maxDepth;   // 0 у корня, 1 у листьев
                    int r = (int)(brown.R + (green.R - brown.R) * t);
                    int gg = (int)(brown.G + (green.G - brown.G) * t);
                    int b = (int)(brown.B + (green.B - brown.B) * t);
                    float thickness = Math.Max(1f, 10f * (1f - t));

                    using (var pen = new Pen(Color.FromArgb(r, gg, b), thickness))
                    {
                        pen.StartCap = LineCap.Round;
                        pen.EndCap = LineCap.Round;
                        g.DrawLine(pen, tr(s.a), tr(s.b));
                    }
                }
            }
            var old = pic.Image; pic.Image = bmp; old?.Dispose();
        }
    }
}
