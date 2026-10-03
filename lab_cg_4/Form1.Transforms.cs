using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using lab_cg_4.Geometry;

namespace lab_cg_4
{
    public partial class Form1
    {
        private Button btnTranslate;
        private Button btnRotateCenter;
        private Button btnRotatePoint;
        private Button btnScaleCenter;
        private Button btnScalePoint;

        private TextBox tbDx;
        private TextBox tbDy;
        private TextBox tbAngle;
        private TextBox tbScale;

        private enum WaitingAction
        {
            None,
            RotateAroundPoint,
            ScaleAroundPoint
        }

        private WaitingAction waitingForPointAction = WaitingAction.None;

        partial void InitTransformUI(int sx)
        {
            GroupBox moveGroup = new GroupBox
            {
                Left = sx,
                Top = 90,
                Width = 250,
                Height = 100,
                Text = "Перенос"
            };

            Label dxLabel = new Label { Left = 12, Top = 25, Width = 25, Text = "X:" };
            tbDx = new TextBox { Left = 35, Top = 21, Width = 75, Text = "10" };

            Label dyLabel = new Label { Left = 125, Top = 25, Width = 25, Text = "Y:" };
            tbDy = new TextBox { Left = 150, Top = 21, Width = 80, Text = "10" };

            btnTranslate = new Button
            {
                Left = 12,
                Top = 56,
                Width = 218,
                Height = 28,
                Text = "Выполнить перенос"
            };

            btnTranslate.Click += BtnTranslate_Click;

            moveGroup.Controls.Add(dxLabel);
            moveGroup.Controls.Add(tbDx);
            moveGroup.Controls.Add(dyLabel);
            moveGroup.Controls.Add(tbDy);
            moveGroup.Controls.Add(btnTranslate);
            Controls.Add(moveGroup);

            GroupBox rotationGroup = new GroupBox
            {
                Left = sx,
                Top = 195,
                Width = 250,
                Height = 130,
                Text = "Поворот"
            };

            Label angleLabel = new Label { Left = 12, Top = 25, Width = 55, Text = "Угол, °:" };
            tbAngle = new TextBox { Left = 70, Top = 21, Width = 160, Text = "45" };

            btnRotateCenter = new Button
            {
                Left = 12,
                Top = 53,
                Width = 218,
                Height = 27,
                Text = "Относительно центра"
            };

            btnRotatePoint = new Button
            {
                Left = 12,
                Top = 88,
                Width = 218,
                Height = 27,
                Text = "Относительно заданной точки"
            };

            btnRotateCenter.Click += BtnRotateCenter_Click;
            btnRotatePoint.Click += BtnRotatePoint_Click;

            rotationGroup.Controls.Add(angleLabel);
            rotationGroup.Controls.Add(tbAngle);
            rotationGroup.Controls.Add(btnRotateCenter);
            rotationGroup.Controls.Add(btnRotatePoint);
            Controls.Add(rotationGroup);

            GroupBox scaleGroup = new GroupBox
            {
                Left = sx,
                Top = 330,
                Width = 250,
                Height = 130,
                Text = "Масштабирование"
            };

            Label scaleLabel = new Label { Left = 12, Top = 25, Width = 95, Text = "Коэффициент:" };
            tbScale = new TextBox { Left = 110, Top = 21, Width = 120, Text = "1.5" };

            btnScaleCenter = new Button
            {
                Left = 12,
                Top = 53,
                Width = 218,
                Height = 27,
                Text = "Относительно центра"
            };

            btnScalePoint = new Button
            {
                Left = 12,
                Top = 88,
                Width = 218,
                Height = 27,
                Text = "Относительно заданной точки"
            };

            btnScaleCenter.Click += BtnScaleCenter_Click;
            btnScalePoint.Click += BtnScalePoint_Click;

            scaleGroup.Controls.Add(scaleLabel);
            scaleGroup.Controls.Add(tbScale);
            scaleGroup.Controls.Add(btnScaleCenter);
            scaleGroup.Controls.Add(btnScalePoint);
            Controls.Add(scaleGroup);

            scaleGroup.BringToFront();
        }

        private void BtnTranslate_Click(object sender, EventArgs e)
        {
            if (!HasSelectedPolygon())
                return;

            if (!ReadNumber(tbDx.Text, out float dx) || !ReadNumber(tbDy.Text, out float dy))
            {
                lblInfo.Text = "Некорректные значения X или Y.";
                return;
            }

            Matrix3 matrix = Matrix3.Translate(dx, dy);
            selected.ApplyMatrix(matrix);

            lblInfo.Text = $"Полигон смещён на ({dx}; {dy}).";
            canvas.Invalidate();
        }

        private void BtnRotateCenter_Click(object sender, EventArgs e)
        {
            if (!HasSelectedPolygon())
                return;

            if (!ReadNumber(tbAngle.Text, out float angle))
            {
                lblInfo.Text = "Некорректное значение угла.";
                return;
            }

            PointF center = CalculatePolygonCenter(selected);
            Matrix3 matrix = CreateRotationAroundPointMatrix(center.X, center.Y, angle);
            selected.ApplyMatrix(matrix);

            lblInfo.Text = $"Поворот на {angle}° относительно центра.";
            canvas.Invalidate();
        }

        private void BtnRotatePoint_Click(object sender, EventArgs e)
        {
            if (!HasSelectedPolygon())
                return;

            if (!ReadNumber(tbAngle.Text, out float angle))
            {
                lblInfo.Text = "Некорректное значение угла.";
                return;
            }

            waitingForPointAction = WaitingAction.RotateAroundPoint;
            lblInfo.Text = "ЛКМ укажите точку, относительно которой выполнить поворот.";
        }

        private void BtnScaleCenter_Click(object sender, EventArgs e)
        {
            if (!HasSelectedPolygon())
                return;

            if (!ReadNumber(tbScale.Text, out float scale))
            {
                lblInfo.Text = "Некорректный коэффициент масштабирования.";
                return;
            }

            PointF center = CalculatePolygonCenter(selected);
            Matrix3 matrix = CreateScaleAroundPointMatrix(center.X, center.Y, scale, scale);
            selected.ApplyMatrix(matrix);

            lblInfo.Text = $"Масштабирование с коэффициентом {scale} относительно центра.";
            canvas.Invalidate();
        }

        private void BtnScalePoint_Click(object sender, EventArgs e)
        {
            if (!HasSelectedPolygon())
                return;

            if (!ReadNumber(tbScale.Text, out float scale))
            {
                lblInfo.Text = "Некорректный коэффициент масштабирования.";
                return;
            }

            waitingForPointAction = WaitingAction.ScaleAroundPoint;
            lblInfo.Text = "ЛКМ укажите точку, относительно которой выполнить масштабирование.";
        }

        partial void HandleTransformClick(PointF point, ref bool handled)
        {
            if (waitingForPointAction == WaitingAction.None)
                return;

            handled = true;

            if (!HasSelectedPolygon())
            {
                waitingForPointAction = WaitingAction.None;
                return;
            }

            if (waitingForPointAction == WaitingAction.RotateAroundPoint)
            {
                if (!ReadNumber(tbAngle.Text, out float angle))
                {
                    lblInfo.Text = "Некорректное значение угла.";
                    waitingForPointAction = WaitingAction.None;
                    return;
                }

                Matrix3 matrix = CreateRotationAroundPointMatrix(point.X, point.Y, angle);
                selected.ApplyMatrix(matrix);

                lblInfo.Text = $"Выполнен поворот на {angle}° относительно точки ({point.X:F1}; {point.Y:F1}).";
            }
            else if (waitingForPointAction == WaitingAction.ScaleAroundPoint)
            {
                if (!ReadNumber(tbScale.Text, out float scale))
                {
                    lblInfo.Text = "Некорректный коэффициент масштабирования.";
                    waitingForPointAction = WaitingAction.None;
                    return;
                }

                Matrix3 matrix = CreateScaleAroundPointMatrix(point.X, point.Y, scale, scale);
                selected.ApplyMatrix(matrix);

                lblInfo.Text = $"Масштабирование с коэффициентом {scale} относительно точки ({point.X:F1}; {point.Y:F1}).";
            }

            waitingForPointAction = WaitingAction.None;
            canvas.Invalidate();
        }

        private Matrix3 CreateRotationAroundPointMatrix(float x, float y, float angle)
        {
            Matrix3 moveToOrigin = Matrix3.Translate(-x, -y);
            Matrix3 rotation = Matrix3.Rotate(angle);
            Matrix3 moveBack = Matrix3.Translate(x, y);

            return moveBack * rotation * moveToOrigin;
        }

        private Matrix3 CreateScaleAroundPointMatrix(float x, float y, float sx, float sy)
        {
            Matrix3 moveToOrigin = Matrix3.Translate(-x, -y);
            Matrix3 scale = Matrix3.Scale(sx, sy);
            Matrix3 moveBack = Matrix3.Translate(x, y);

            return moveBack * scale * moveToOrigin;
        }

        private PointF CalculatePolygonCenter(Polygon polygon)
        {
            int count = polygon.pts.Count;

            if (count == 0)
                return PointF.Empty;

            if (count > 1 && polygon.pts[0] == polygon.pts[count - 1])
                count--;

            float sumX = 0;
            float sumY = 0;

            for (int i = 0; i < count; i++)
            {
                sumX += polygon.pts[i].X;
                sumY += polygon.pts[i].Y;
            }

            return new PointF(sumX / count, sumY / count);
        }

        private bool HasSelectedPolygon()
        {
            if (selected != null)
                return true;

            lblInfo.Text = "Сначала выберите полигон.";
            return false;
        }

        private bool ReadNumber(string text, out float value)
        {
            return float.TryParse(text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }
    }
}