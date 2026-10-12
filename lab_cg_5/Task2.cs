
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace lab_cg_5
{
    public partial class Task2 : Form
    {
        private List<List<PointF>> steps = new List<List<PointF>>();
        private int currentStep = 0;
        private int seed = 12345;

        public Task2()
        {
            InitializeComponent();
            GenerateMountains();
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
            StopAnimation();

            if (currentStep > 0)
            {
                currentStep--;
                UpdateStepInfo();
                canvas.Invalidate();
            }
        }

        private void Next_Click(object sender, EventArgs e)
        {
            StopAnimation();

            if (currentStep < steps.Count - 1)
            {
                currentStep++;
                UpdateStepInfo();
                canvas.Invalidate();
            }
        }

        private void All_Click(object sender, EventArgs e)
        {
            StopAnimation();

            if (steps.Count == 0)
                return;

            currentStep = steps.Count - 1;
            UpdateStepInfo();
            canvas.Invalidate();
        }

        private void Play_Click(object sender, EventArgs e)
        {
            if (animationTimer.Enabled)
            {
                StopAnimation();
                return;
            }

            if (steps.Count == 0)
                return;

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
                StopAnimation();
            }
        }

        private void StopAnimation()
        {
            animationTimer.Stop();
            btnPlay.Text = "Автовоспроизведение";
        }

        private void DisplayOptions_Changed(object sender, EventArgs e)
        {
            canvas.Invalidate();
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

                using (LinearGradientBrush brush = new LinearGradientBrush(new PointF(0, top), new PointF(0, bottom), Color.FromArgb(100, 125, 145), Color.FromArgb(35, 65, 80)))
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
                    g.FillEllipse(Brushes.OrangeRed, point.X - 3, point.Y - 3, 6, 6);
            }

            using (Font font = new Font("Segoe UI", 10, FontStyle.Bold))
                g.DrawString($"Итерация: {currentStep}", font, Brushes.DarkSlateGray, 45, 15);
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
