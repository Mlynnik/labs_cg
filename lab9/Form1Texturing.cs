using lab6.Math3d;
using lab6.Model;
using lab6.Rendering;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace lab6
{
    //переключение независимых алгоритмов трех участников.
    public partial class Form1
    {
        private ComboBox _lab9Shading;
        private TextBox _lab9LightX, _lab9LightY, _lab9LightZ;
        private CheckBox _lab9PointLight;
        private NumericUpDown _lab9Ambient, _lab9Bands;
        private Bitmap _lab9Texture;
        private Bitmap _lab9AppliedTexture;
        private Renderer _lab9AppliedRenderer;

        private void BuildLab9UI(FlowLayoutPanel host)
        {
            _lab9Shading = new ComboBox { Width = 380, DropDownStyle = ComboBoxStyle.DropDownList };
            _lab9Shading.Items.AddRange(new object[]
            {
                "1. Гуро — Ламберт",
                "2. Фонг — туншейдинг",
                "3. Текстурирование"
            });
            _lab9Shading.SelectedIndex = 0;
            _lab9Shading.SelectedIndexChanged += (_, __) => Redraw();

            host.Controls.Add(new Label { Text = "Режим отображения:", AutoSize = true });
            host.Controls.Add(_lab9Shading);

            _lab9PointLight = new CheckBox { Text = "Точечный источник света", AutoSize = true, Checked = true };
            _lab9PointLight.CheckedChanged += (_, __) => Redraw();
            host.Controls.Add(_lab9PointLight);
            host.Controls.Add(new Label { Text = "Свет: X, Y, Z (позиция или направление)", AutoSize = true });

            _lab9LightX = new TextBox { Width = 65, Text = "4" };
            _lab9LightY = new TextBox { Width = 65, Text = "5" };
            _lab9LightZ = new TextBox { Width = 65, Text = "7" };
            host.Controls.Add(Row(_lab9LightX, _lab9LightY, _lab9LightZ, MakeButton("Применить свет", (_, __) => Redraw())));

            _lab9Ambient = new NumericUpDown { Minimum = 0, Maximum = 100, Value = 18, Width = 60 };
            _lab9Bands = new NumericUpDown { Minimum = 2, Maximum = 12, Value = 4, Width = 60 };
            _lab9Ambient.ValueChanged += (_, __) => Redraw();
            _lab9Bands.ValueChanged += (_, __) => Redraw();

            host.Controls.Add(Row(new Label { Text = "Фоновый свет %:", AutoSize = true }, _lab9Ambient));
            host.Controls.Add(Row(new Label { Text = "Уровней тона:", AutoSize = true }, _lab9Bands));
            host.Controls.Add(MakeButton("Цвет активного объекта...", OnLab9SetColor));

            host.Controls.Add(Row(MakeButton("Загрузить текстуру...", OnLab9LoadTexture), MakeButton("Шахматная текстура", OnLab9Checker)));
            host.Controls.Add(Row(MakeButton("Тетраэдр", OnLab9AddTetrahedron), MakeButton("Куб", OnLab9AddCube), MakeButton("Октаэдр", OnLab9AddOctahedron)));
            host.Controls.Add(new Label { Text = "Для демонстрации выберите «Заливка», включите отсечение и Z-буфер.", AutoSize = true, MaximumSize = new Size(400, 0) });
        }

        private void OnLab9AddTetrahedron(object sender, EventArgs e)
        {
            AddModel(Primitives.Tetrahedron(), new Vec3(0.8, 0.55, 0.3), true);
        }

        private void OnLab9AddCube(object sender, EventArgs e)
        {
            AddModel(Primitives.Cube(), new Vec3(0.8, 0.55, 0.3), true);
        }

        private void OnLab9AddOctahedron(object sender, EventArgs e)
        {
            AddModel(Primitives.Octahedron(), new Vec3(0.8, 0.55, 0.3), true);
        }

        private void OnLab9SetColor(object sender, EventArgs e)
        {
            if (Active == null)
                return;
            using var dialog = new ColorDialog();
            Vec3 color = Active.BaseColor;
            dialog.Color = System.Drawing.Color.FromArgb((int)(color.X * 255), (int)(color.Y * 255), (int)(color.Z * 255));
            if (dialog.ShowDialog(this) != DialogResult.OK) 
                return;
            Active.BaseColor = new Vec3(dialog.Color.R / 255.0, dialog.Color.G / 255.0, dialog.Color.B / 255.0);
            Redraw();
        }

        private void OnLab9LoadTexture(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog { Filter = "Изображения|*.png;*.jpg;*.jpeg;*.bmp|Все файлы|*.*" };
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;
            try
            {
                using var file = new Bitmap(dialog.FileName);
                var copy = new Bitmap(file);
                _lab9Texture?.Dispose();
                _lab9Texture = copy;
                _lab9Shading.SelectedIndex = (int)ShadingMode.Textured;
                Redraw();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Ошибка загрузки текстуры: " + ex.Message);
            }
        }

        private void OnLab9Checker(object sender, EventArgs e)
        {
            _lab9Texture?.Dispose();
            _lab9Texture = null;
            _lab9Shading.SelectedIndex = (int)ShadingMode.Textured;
            Redraw();
        }

        private void ConfigureLab9Renderer()
        {
            if (_renderer == null || _lab9Shading == null)
                return;
            _renderer.Shading = (ShadingMode)_lab9Shading.SelectedIndex;
            _renderer.PositionalLight = _lab9PointLight.Checked;
            Vec3 light = new Vec3(D(_lab9LightX), D(_lab9LightY), D(_lab9LightZ));
            _renderer.LightPosition = light;
            _renderer.LightDir = light;
            _renderer.Ambient = (double)_lab9Ambient.Value / 100.0;
            _renderer.ToonBands = (int)_lab9Bands.Value;
            if (_lab9AppliedRenderer == _renderer && _lab9AppliedTexture == _lab9Texture)
                return;
            if (_lab9Texture == null)
                _renderer.MakeCheckerTexture();
            else 
                _renderer.SetTexture(_lab9Texture);

            _lab9AppliedRenderer = _renderer;
            _lab9AppliedTexture = _lab9Texture;
        }
    }
}
