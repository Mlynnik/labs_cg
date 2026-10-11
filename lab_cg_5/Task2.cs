
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace lab_cg_5
{
    public partial class Task2 : Form
    {
        private Panel canvas;
        private Panel controlsPanel;
        private NumericUpDown numIterations;
        private NumericUpDown numRoughness;
        private NumericUpDown numDecay;
        private NumericUpDown numStartHeight;
        private NumericUpDown numSeed;
        private Button btnGenerate;
        private Button btnPrevious;
        private Button btnNext;
        private Button btnAll;
        private Button btnPlay;
        private CheckBox chkFill;
        private CheckBox chkGrid;
        private CheckBox chkPoints;
        private Label lblStep;
        private Label lblInfo;
        private System.Windows.Forms.Timer animationTimer; 

        private List<List<PointF>> steps = new List<List<PointF>>();
        private int currentStep = 0;
        private int seed = 12345;

        public Task2()
        {
            InitializeComponent();

            Text = "Midpoint Displacement — горный массив";
            Width = 1200;
            Height = 750;
            MinimumSize = new Size(950, 600);
            StartPosition = FormStartPosition.CenterScreen;

            Controls.Clear();
            InitializeUI();
            GenerateMountains();
        }

        private void InitializeUI()
        {
            canvas = new BufferedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(235, 245, 255)
            };

            canvas.Paint += Canvas_Paint;
            Controls.Add(canvas);

            controlsPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 280,
                BackColor = Color.FromArgb(245, 247, 250),
                AutoScroll = true
            };

            Controls.Add(controlsPanel);
            controlsPanel.BringToFront();

            int y = 20;

            AddTitle("Midpoint Displacement", ref y);

            AddLabel("Количество итераций", ref y);
            numIterations = AddNumeric(1, 11, 7, 0, 1, ref y);

            AddLabel("Шероховатость", ref y);
            numRoughness = AddNumeric(0, 2, 0.55m, 2, 0.05m, ref y);

            AddLabel("Уменьшение смещения", ref y);
            numDecay = AddNumeric(0.1m, 1, 0.55m, 2, 0.05m, ref y);

            AddLabel("Начальная высота (%)", ref y);
            numStartHeight = AddNumeric(10, 90, 50, 0, 5, ref y);

            AddLabel("Seed", ref y);
            numSeed = AddNumeric(0, 999999, seed, 0, 1, ref y);

            btnGenerate = AddButton("Построить", ref y, Generate_Click);
            btnPrevious = AddButton("Предыдущий шаг", ref y, Previous_Click);
            btnNext = AddButton("Следующий шаг", ref y, Next_Click);
            btnAll = AddButton("Все шаги", ref y, All_Click);
            btnPlay = AddButton("Автовоспроизведение", ref y, Play_Click);

            AddSpace(ref y, 8);

            chkFill = AddCheckBox("Заливка гор", true, ref y);
            chkGrid = AddCheckBox("Показывать сетку", true, ref y);
            chkPoints = AddCheckBox("Показывать вершины", false, ref y);

            chkFill.CheckedChanged += (s, e) => canvas.Invalidate();
            chkGrid.CheckedChanged += (s, e) => canvas.Invalidate();
            chkPoints.CheckedChanged += (s, e) => canvas.Invalidate();

            AddSpace(ref y, 15);

            lblStep = new Label
            {
                Left = 15,
                Top = y,
                Width = 245,
                Height = 45,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };

            controlsPanel.Controls.Add(lblStep);
            y += 50;

            lblInfo = new Label
            {
                Left = 15,
                Top = y,
                Width = 245,
                Height = 110,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.DimGray
            };

            controlsPanel.Controls.Add(lblInfo);

            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 650;
            animationTimer.Tick += AnimationTimer_Tick;
        }

        private void AddTitle(string text, ref int y)
        {
            Label label = new Label
            {
                Left = 15,
                Top = y,
                Width = 250,
                Height = 40,
                Text = text,
                Font = new Font("Segoe UI", 14, FontStyle.Bold)
            };

            controlsPanel.Controls.Add(label);
            y += 50;
        }

        private void AddLabel(string text, ref int y)
        {
            Label label = new Label
            {
                Left = 15,
                Top = y,
                Width = 245,
                Height = 23,
                Text = text,
                Font = new Font("Segoe UI", 9)
            };

            controlsPanel.Controls.Add(label);
            y += 25;
        }

        private NumericUpDown AddNumeric(decimal min, decimal max, decimal value, int decimals, decimal increment, ref int y)
        {
            NumericUpDown numeric = new NumericUpDown
            {
                Left = 15,
                Top = y,
                Width = 245,
                Height = 30,
                Minimum = min,
                Maximum = max,
                Value = value,
                DecimalPlaces = decimals,
                Increment = increment,
                Font = new Font("Segoe UI", 10)
            };

            controlsPanel.Controls.Add(numeric);
            y += 45;

            return numeric;
        }

        private Button AddButton(string text, ref int y, EventHandler handler)
        {
            Button button = new Button
            {
                Left = 15,
                Top = y,
                Width = 245,
                Height = 36,
                Text = text,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            button.FlatAppearance.BorderColor = Color.LightGray;
            button.Click += handler;

            controlsPanel.Controls.Add(button);
            y += 43;

            return button;
        }

        private CheckBox AddCheckBox(string text, bool value, ref int y)
        {
            CheckBox checkBox = new CheckBox
            {
                Left = 15,
                Top = y,
                Width = 245,
                Height = 28,
                Text = text,
                Checked = value,
                Font = new Font("Segoe UI", 9)
            };

            controlsPanel.Controls.Add(checkBox);
            y += 32;

            return checkBox;
        }

        private void AddSpace(ref int y, int size)
        {
            y += size;
        }

        private void GenerateMountains()
        {
            animationTimer.Stop();
            btnPlay.Text = "Автовоспроизведение";

            int iterations = (int)numIterations.Value;
            float roughness = (float)numRoughness.Value;
            float decay = (float)numDecay.Value;
            float startHeight = (float)numStartHeight.Value / 100f;
            seed = (int)numSeed.Value;

            Random random = new Random(seed);

            steps.Clear();

            List<PointF> points = new List<PointF>
            {
                new PointF(0f, startHeight),
                new PointF(1f, startHeight)
            };

            steps.Add(new List<PointF>(points));

            float displacement = roughness;

            for (int iteration = 0; iteration < iterations; iteration++)
            {
                List<PointF> newPoints = new List<PointF>();

                for (int i = 0; i < points.Count - 1; i++)
                {
                    PointF a = points[i];
                    PointF b = points[i + 1];

                    float midX = (a.X + b.X) / 2f;
                    float midY = (a.Y + b.Y) / 2f;
                    float offset = (float)(random.NextDouble() * 2 - 1) * displacement;

                    newPoints.Add(a);
                    newPoints.Add(new PointF(midX, midY + offset));
                }

                newPoints.Add(points[points.Count - 1]);

                points = newPoints;
                steps.Add(new List<PointF>(points));

                displacement *= decay;
            }

            currentStep = 0;
            UpdateStepInfo();
            canvas.Invalidate();
        }

        private void UpdateStepInfo()
        {
            if (steps.Count == 0)
                return;

            int count = steps[currentStep].Count;

            lblStep.Text = $"Шаг {currentStep} из {steps.Count - 1}";

            lblInfo.Text =
                $"Количество точек: {count}\n" +
                $"Количество отрезков: {count - 1}\n" +
                $"Seed: {seed}\n" +
                $"Шероховатость: {numRoughness.Value:F2}";

            btnPrevious.Enabled = currentStep > 0;
            btnNext.Enabled = currentStep < steps.Count - 1;
        }

        private void Generate_Click(object sender, EventArgs e)
        {
            GenerateMountains();
        }

        private void Previous_Click(object sender, EventArgs e)
        {
            if (currentStep > 0)
            {
                currentStep--;
                UpdateStepInfo();
                canvas.Invalidate();
            }
        }

        private void Next_Click(object sender, EventArgs e)
        {
            if (currentStep < steps.Count - 1)
            {
                currentStep++;
                UpdateStepInfo();
                canvas.Invalidate();
            }
        }

        private void All_Click(object sender, EventArgs e)
        {
            animationTimer.Stop();
            btnPlay.Text = "Автовоспроизведение";

            currentStep = steps.Count - 1;
            UpdateStepInfo();
            canvas.Invalidate();
        }

        private void Play_Click(object sender, EventArgs e)
        {
            if (animationTimer.Enabled)
            {
                animationTimer.Stop();
                btnPlay.Text = "Автовоспроизведение";
                return;
            }

            if (currentStep >= steps.Count - 1)
                currentStep = 0;

            animationTimer.Start();
            btnPlay.Text = "Остановить";
            UpdateStepInfo();
            canvas.Invalidate();
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (currentStep < steps.Count - 1)
            {
                currentStep++;
                UpdateStepInfo();
                canvas.Invalidate();
            }
            else
            {
                animationTimer.Stop();
                btnPlay.Text = "Автовоспроизведение";
            }
        }

        private void Canvas_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.FromArgb(235, 245, 255));

            if (steps.Count == 0 || canvas.Width < 50 || canvas.Height < 50)
                return;

            int width = canvas.ClientSize.Width;
            int height = canvas.ClientSize.Height;

            int margin = 35;
            float left = margin;
            float right = width - margin;
            float top = margin;
            float bottom = height - margin;

            if (chkGrid.Checked)
                DrawGrid(g, left, top, right, bottom);

            List<PointF> points = steps[currentStep];

            float minY = float.MaxValue;
            float maxY = float.MinValue;

            foreach (PointF point in steps[steps.Count - 1])
            {
                minY = Math.Min(minY, point.Y);
                maxY = Math.Max(maxY, point.Y);
            }

            float range = maxY - minY;

            if (range < 0.1f)
                range = 0.1f;

            float padding = range * 0.25f;
            minY -= padding;
            maxY += padding;
            range = maxY - minY;

            PointF[] screenPoints = new PointF[points.Count];

            for (int i = 0; i < points.Count; i++)
            {
                float x = left + points[i].X * (right - left);
                float y = bottom - (points[i].Y - minY) / range * (bottom - top);

                screenPoints[i] = new PointF(x, y);
            }

            if (chkFill.Checked && screenPoints.Length >= 2)
            {
                PointF[] fillPoints = new PointF[screenPoints.Length + 2];

                Array.Copy(screenPoints, fillPoints, screenPoints.Length);

                fillPoints[screenPoints.Length] = new PointF(right, bottom);
                fillPoints[screenPoints.Length + 1] = new PointF(left, bottom);

                using (LinearGradientBrush brush = new LinearGradientBrush(
                    new PointF(0, top),
                    new PointF(0, bottom),
                    Color.FromArgb(100, 125, 145),
                    Color.FromArgb(35, 65, 80)))
                {
                    g.FillPolygon(brush, fillPoints);
                }
            }

            using (Pen pen = new Pen(Color.FromArgb(25, 50, 65), 2.5f))
            {
                g.DrawLines(pen, screenPoints);
            }

            if (chkPoints.Checked)
            {
                foreach (PointF point in screenPoints)
                {
                    g.FillEllipse(Brushes.OrangeRed, point.X - 3, point.Y - 3, 6, 6);
                }
            }

            using (Font font = new Font("Segoe UI", 10, FontStyle.Bold))
            {
                g.DrawString($"Итерация: {currentStep}", font, Brushes.DarkSlateGray, 45, 15);
            }
        }

        private void DrawGrid(Graphics g, float left, float top, float right, float bottom)
        {
            using (Pen pen = new Pen(Color.FromArgb(205, 220, 230), 1))
            {
                pen.DashStyle = DashStyle.Dash;

                for (int i = 0; i <= 10; i++)
                {
                    float x = left + (right - left) * i / 10f;
                    g.DrawLine(pen, x, top, x, bottom);
                }

                for (int i = 0; i <= 8; i++)
                {
                    float y = top + (bottom - top) * i / 8f;
                    g.DrawLine(pen, left, y, right, y);
                }
            }
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
