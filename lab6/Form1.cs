using lab6.Math3d;
using lab6.Model;
using lab6.Rendering;

namespace lab6
{
    public partial class Form1 : Form
    {
        Polyhedron _model;
        Mat4 _modelMatrix = Mat4.Identity;
        Renderer _renderer;

        // ----- Контролы -----
        ComboBox _modelCombo;
        RadioButton _perspRadio, _axonRadio;
        TextBox _txBox, _tyBox, _tzBox;
        TextBox _rotAngleBox;
        TextBox _scaleBox;
        TextBox _axisAngleBox;
        ComboBox _axisCombo;
        TextBox _l1xBox, _l1yBox, _l1zBox, _l2xBox, _l2yBox, _l2zBox, _lineAngleBox;
        PictureBox _pictureBox;

        public Form1()
        {
            Text = "Lab6 — Аффинные преобразования многогранников";
            Width = 1400; Height = 1000;
            StartPosition = FormStartPosition.CenterScreen;

            BuildUI();
            Load += (_, __) => { LoadModel(0); Redraw(); };
        }

        // ================== UI ==================
        void BuildUI()
        {
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 400,
                FixedPanel = FixedPanel.Panel1,
            };
            Controls.Add(split);

            // ---- Левая панель с прокруткой ----
            var leftScroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            var stack = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                Padding = new Padding(8)
            };
            leftScroll.Controls.Add(stack);
            split.Panel1.Controls.Add(leftScroll);

            // Модель
            _modelCombo = new ComboBox { Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            _modelCombo.Items.AddRange(new object[] { "Икосаэдр", "Додекаэдр", "Куб" });
            _modelCombo.SelectedIndex = 0;
            _modelCombo.SelectedIndexChanged += OnModelChanged;
            AddGroup(stack, "Модель", _modelCombo);

            // Проекция
            _perspRadio = new RadioButton { Text = "Перспективная", Checked = true, AutoSize = true };
            _axonRadio = new RadioButton { Text = "Аксонометрическая", AutoSize = true };
            _perspRadio.CheckedChanged += OnProjectionChanged;
            _axonRadio.CheckedChanged += OnProjectionChanged;
            AddGroup(stack, "Проекция", Row(_perspRadio, _axonRadio));

            // Смещение
            _txBox = NumBox("0.5"); _tyBox = NumBox("0"); _tzBox = NumBox("0");
            AddGroup(stack, "Смещение (dx, dy, dz)",
                Row(_txBox, _tyBox, _tzBox, MakeButton("Сдвинуть", OnTranslate)));

            // Поворот вокруг координатной оси
            _rotAngleBox = NumBox("15");
            AddGroup(stack, "Поворот вокруг оси (град)",
                Row(_rotAngleBox,
                    MakeButton("X", OnRotX),
                    MakeButton("Y", OnRotY),
                    MakeButton("Z", OnRotZ)));

            // Масштаб
            _scaleBox = NumBox("0.5");
            AddGroup(stack, "Масштаб",
                Row(_scaleBox,
                    MakeButton("Относительно 0", OnScale),
                    MakeButton("Относительно центра", OnScaleCenter)));

            // Отражение
            AddGroup(stack, "Отражение относительно плоскости",
                Row(MakeButton("XY", OnReflectXY),
                    MakeButton("XZ", OnReflectXZ),
                    MakeButton("YZ", OnReflectYZ)));

            // Поворот вокруг оси через центр, параллельной координатной
            _axisAngleBox = NumBox("30");
            _axisCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            _axisCombo.Items.AddRange(new object[] { "X", "Y", "Z" });
            _axisCombo.SelectedIndex = 1;
            AddGroup(stack, "Поворот вокруг оси через центр",
                Row(_axisAngleBox, _axisCombo,
                    MakeButton("Повернуть", OnRotAroundCenterAxis)));

            // Произвольная прямая
            _l1xBox = NumBox("0"); _l1yBox = NumBox("0"); _l1zBox = NumBox("0");
            _l2xBox = NumBox("1"); _l2yBox = NumBox("1"); _l2zBox = NumBox("1");
            _lineAngleBox = NumBox("30");
            var linePanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                WrapContents = false
            };
            linePanel.Controls.Add(new Label { Text = "точка A (x, y, z)", AutoSize = true });
            linePanel.Controls.Add(Row(_l1xBox, _l1yBox, _l1zBox));
            linePanel.Controls.Add(new Label { Text = "точка B (x, y, z)", AutoSize = true });
            linePanel.Controls.Add(Row(_l2xBox, _l2yBox, _l2zBox));
            linePanel.Controls.Add(Row(_lineAngleBox,
                MakeButton("Повернуть вокруг A→B", OnRotAroundLine)));
            AddGroup(stack, "Поворот вокруг произвольной прямой", linePanel);

            // Сброс
            stack.Controls.Add(MakeButton("Сбросить все преобразования", OnReset));

            // ---- Правая панель — изображение ----
            _pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(0xF2, 0xF2, 0xF2),
                SizeMode = PictureBoxSizeMode.Normal
            };
            _pictureBox.Resize += (_, __) => Redraw();
            split.Panel2.Controls.Add(_pictureBox);
            split.Panel2.Resize += (_, __) => _pictureBox.Bounds = split.Panel2.ClientRectangle;
            split.Panel2.BackColor = Color.AliceBlue;
            split.Panel1MinSize = 400;
        }

        // ---- Вспомогательные билдеры ----
        TextBox NumBox(string text) => new TextBox { Text = text, Width = 60 };

        Button MakeButton(string text, EventHandler click)
        {
            var b = new Button { Text = text, AutoSize = true };
            b.Click += click;
            return b;
        }

        FlowLayoutPanel Row(params Control[] items)
        {
            var p = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
            p.Controls.AddRange(items);
            return p;
        }

        void AddGroup(Control parent, string title, Control content)
        {
            var gb = new GroupBox
            {
                Text = title,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(8, 4, 8, 4),
                Margin = new Padding(3, 3, 3, 8)
            };
            content.Location = new Point(8, 20);
            gb.Controls.Add(content);
            parent.Controls.Add(gb);
        }

        // ================== Логика ==================
        void LoadModel(int idx)
        {
            _model = idx switch
            {
                0 => Primitives.Icosahedron(),
                1 => Primitives.Dodecahedron(),
                2 => Primitives.Cube(),
                _ => Primitives.Icosahedron()
            };
            _modelMatrix = Mat4.Identity;
        }

        void Redraw()
        {
            if (_model == null) return;
            int w = Math.Max(1, _pictureBox.ClientSize.Width);
            int h = Math.Max(1, _pictureBox.ClientSize.Height);
            if (w < 50) w = 900;
            if (h < 50) h = 700;

            if (_renderer == null || _renderer.W != w || _renderer.H != h)
            {
                _pictureBox.Image = null;
                _renderer?.Bitmap?.Dispose();
                _renderer = new Renderer(w, h);
                _pictureBox.Image = _renderer.Bitmap;
            }

            _renderer.Projection = _perspRadio.Checked
                ? ProjectionMode.Perspective
                : ProjectionMode.Axonometric;

            _renderer.Clear(unchecked((int)0xFFF2F2F2));
            _renderer.Render(_model, _modelMatrix);
            _renderer.Present();
            _pictureBox.Invalidate();
        }

        void OnModelChanged(object s, EventArgs e)
        {
            if (IsHandleCreated) { LoadModel(_modelCombo.SelectedIndex); Redraw(); }
        }

        void OnProjectionChanged(object s, EventArgs e)
        {
            if (IsHandleCreated) Redraw();
        }

        double D(TextBox b)
        {
            var s = b.Text.Replace(',', '.').Trim();
            return double.TryParse(
                s,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out var v) ? v : 0;
        }
        double Rad(double deg) => deg * Math.PI / 180.0;

        // ---- Аффинные преобразования ----
        void OnTranslate(object s, EventArgs e)
        {
            _modelMatrix = Mat4.Translation(D(_txBox), D(_tyBox), D(_tzBox)) * _modelMatrix;
            Redraw();
        }
        void OnRotX(object s, EventArgs e) { _modelMatrix = Mat4.RotationX(Rad(D(_rotAngleBox))) * _modelMatrix; Redraw(); }
        void OnRotY(object s, EventArgs e) { _modelMatrix = Mat4.RotationY(Rad(D(_rotAngleBox))) * _modelMatrix; Redraw(); }
        void OnRotZ(object s, EventArgs e) { _modelMatrix = Mat4.RotationZ(Rad(D(_rotAngleBox))) * _modelMatrix; Redraw(); }

        void OnScale(object s, EventArgs e)
        {
            double k = D(_scaleBox);
            if (Math.Abs(k) < 1e-6 || double.IsNaN(k) || double.IsInfinity(k)) return;
            _modelMatrix = Mat4.Scale(k, k, k) * _modelMatrix;
            Redraw();
        }

        void OnScaleCenter(object s, EventArgs e)
        {
            double k = D(_scaleBox);
            if (Math.Abs(k) < 1e-6 || double.IsNaN(k) || double.IsInfinity(k)) return;
            // Масштаб в ОБЪЕКТНОМ пространстве → вокруг собственного центра модели.
            _modelMatrix = _modelMatrix * Mat4.ScaleAround(_model.Center, k);
            Redraw();
        }

        void OnReflectXY(object s, EventArgs e) { _modelMatrix = Mat4.ReflectionXY() * _modelMatrix; Redraw(); }
        void OnReflectXZ(object s, EventArgs e) { _modelMatrix = Mat4.ReflectionXZ() * _modelMatrix; Redraw(); }
        void OnReflectYZ(object s, EventArgs e) { _modelMatrix = Mat4.ReflectionYZ() * _modelMatrix; Redraw(); }

        void OnRotAroundCenterAxis(object s, EventArgs e)
        {
            var axis = _axisCombo.SelectedIndex == 0 ? Vec3.UnitX
                     : _axisCombo.SelectedIndex == 1 ? Vec3.UnitY
                     : Vec3.UnitZ;

            // мировой центр модели при текущем накопленном преобразовании
            var worldCenter = _modelMatrix.TransformPoint(_model.Center);

            _modelMatrix = Mat4.RotateAroundAxisThroughPoint(worldCenter, axis, Rad(D(_axisAngleBox)))
                         * _modelMatrix;
            Redraw();
        }

        void OnRotAroundLine(object s, EventArgs e)
        {
            var p1 = new Vec3(D(_l1xBox), D(_l1yBox), D(_l1zBox));
            var p2 = new Vec3(D(_l2xBox), D(_l2yBox), D(_l2zBox));
            _modelMatrix = Mat4.RotationAroundLine(p1, p2, Rad(D(_lineAngleBox))) * _modelMatrix;
            Redraw();
        }

        void OnReset(object s, EventArgs e) { _modelMatrix = Mat4.Identity; Redraw(); }
    }
}
