using lab6.Math3d;
using lab6.Model;
using lab6.Rendering;

namespace lab6
{
    public partial class Form1 : Form
    {
        Polyhedron _model;
        Mat4 _modelMatrix = Mat4.Identity;
        Mat4 _initialMatrix = Mat4.Identity;
        Renderer _renderer;

        readonly Dictionary<string, Polyhedron> _loadedModels = new();
        bool _suppressComboEvent = false;

        ComboBox _modelCombo;
        RadioButton _perspRadio, _axonRadio;
        TextBox _txBox, _tyBox, _tzBox;
        TextBox _rotAngleBox;
        TextBox _scaleBox;
        TextBox _axisAngleBox;
        ComboBox _axisCombo;
        TextBox _l1xBox, _l1yBox, _l1zBox, _l2xBox, _l2yBox, _l2zBox, _lineAngleBox;
        TextBox _generatrixBox;
        ComboBox _revolutionAxisCombo;
        TextBox _revolutionDivBox;
        ComboBox _functionCombo;
        TextBox _fx0, _fx1, _fy0, _fy1, _fnx, _fny;
        PictureBox _pictureBox;
        double _sceneRadius = 1.0;
        int _buildCounter = 0;


        public Form1()
        {
            Text = "Lab7 — Моделирование поверхностей";
            Width = 1400; Height = 1000;
            StartPosition = FormStartPosition.CenterScreen;

            BuildUI();
            _model = Primitives.Icosahedron();
            _sceneRadius = ComputeRadius(_model, Mat4.Identity);
            Load += (_, __) => Redraw();
        }

        void BuildUI()
        {
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 440,
                FixedPanel = FixedPanel.Panel1,
            };
            Controls.Add(split);

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

            _modelCombo = new ComboBox { Width = 400, DropDownStyle = ComboBoxStyle.DropDownList };
            _modelCombo.Items.AddRange(new object[]
            {
                "Икосаэдр", "Додекаэдр", "Куб",
                "Тетраэдр", "Гексаэдр", "Октаэдр"
            });
            _modelCombo.SelectedIndex = 0;
            _modelCombo.SelectedIndexChanged += OnModelChanged;

            var modelPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true, WrapContents = false };
            modelPanel.Controls.Add(_modelCombo);
            modelPanel.Controls.Add(Row(
                MakeButton("Загрузить из файла…", OnLoadFile),
                MakeButton("Сохранить в файл…", OnSaveFile)));
            AddGroup(stack, "Модель", modelPanel);

            _perspRadio = new RadioButton { Text = "Перспективная", Checked = true, AutoSize = true };
            _axonRadio = new RadioButton { Text = "Аксонометрическая", AutoSize = true };
            _perspRadio.CheckedChanged += OnProjectionChanged;
            _axonRadio.CheckedChanged += OnProjectionChanged;
            AddGroup(stack, "Проекция", Row(_perspRadio, _axonRadio));

            _txBox = NumBox("0.5"); _tyBox = NumBox("0"); _tzBox = NumBox("0");
            AddGroup(stack, "Смещение (dx, dy, dz)",
                Row(_txBox, _tyBox, _tzBox, MakeButton("Сдвинуть", OnTranslate)));

            _rotAngleBox = NumBox("15");
            AddGroup(stack, "Поворот вокруг оси (град)",
                Row(_rotAngleBox,
                    MakeButton("X", OnRotX),
                    MakeButton("Y", OnRotY),
                    MakeButton("Z", OnRotZ)));

            _scaleBox = NumBox("1.2");
            AddGroup(stack, "Масштаб",
                Row(_scaleBox,
                    MakeButton("Относительно 0", OnScale),
                    MakeButton("Относительно центра", OnScaleCenter)));

            AddGroup(stack, "Отражение относительно плоскости",
                Row(MakeButton("XY", OnReflectXY),
                    MakeButton("XZ", OnReflectXZ),
                    MakeButton("YZ", OnReflectYZ)));

            _axisAngleBox = NumBox("30");
            _axisCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            _axisCombo.Items.AddRange(new object[] { "X", "Y", "Z" });
            _axisCombo.SelectedIndex = 1;
            AddGroup(stack, "Поворот вокруг оси через центр",
                Row(_axisAngleBox, _axisCombo,
                    MakeButton("Повернуть", OnRotAroundCenterAxis)));

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

            _generatrixBox = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Width = 400,
                Height = 120,
                Font = new Font(FontFamily.GenericMonospace, 9f),
                Text = "0 -1 0\r\n0.3 -0.8 0\r\n0.5 -0.4 0\r\n0.4 0 0\r\n0.5 0.4 0\r\n0.3 0.8 0\r\n0 1 0"
            };
            _revolutionAxisCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            _revolutionAxisCombo.Items.AddRange(new object[] { "X", "Y", "Z" });
            _revolutionAxisCombo.SelectedIndex = 1;
            _revolutionDivBox = NumBox("24");

            var revPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                WrapContents = false
            };
            revPanel.Controls.Add(new Label { Text = "Образующая (x y z, по строке на точку):", AutoSize = true });
            revPanel.Controls.Add(_generatrixBox);
            revPanel.Controls.Add(Row(
                new Label { Text = "Ось:", AutoSize = true }, _revolutionAxisCombo,
                new Label { Text = "Разбиений:", AutoSize = true }, _revolutionDivBox));
            revPanel.Controls.Add(MakeButton("Построить фигуру вращения", OnBuildRevolution));
            AddGroup(stack, "Фигура вращения", revPanel);

            _functionCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };
            _functionCombo.Items.AddRange(new object[]
            {
                "sin(x) * cos(y)",
                "x*x - y*y",
                "exp(-(x*x + y*y))",
                "sin(sqrt(x*x + y*y))",
                "x*x + y*y",
                "cos(x) * sin(y) + 0.5 * sin(3*x) * cos(3*y)"
            });
            _functionCombo.SelectedIndex = 0;

            _fx0 = NumBox("-3"); _fx1 = NumBox("3");
            _fy0 = NumBox("-3"); _fy1 = NumBox("3");
            _fnx = NumBox("20"); _fny = NumBox("20");

            var surfPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                WrapContents = false
            };
            surfPanel.Controls.Add(Row(new Label { Text = "f(x, y) =", AutoSize = true }, _functionCombo));
            surfPanel.Controls.Add(Row(
                new Label { Text = "x ∈ [", AutoSize = true }, _fx0, new Label { Text = ",", AutoSize = true }, _fx1, new Label { Text = "]", AutoSize = true },
                new Label { Text = " y ∈ [", AutoSize = true }, _fy0, new Label { Text = ",", AutoSize = true }, _fy1, new Label { Text = "]", AutoSize = true }));
            surfPanel.Controls.Add(Row(
                new Label { Text = "Разбиений X:", AutoSize = true }, _fnx,
                new Label { Text = "Y:", AutoSize = true }, _fny));
            surfPanel.Controls.Add(MakeButton("Построить график", OnBuildSurface));
            AddGroup(stack, "График двух переменных (z = f(x, y))", surfPanel);

            stack.Controls.Add(MakeButton("Сбросить все преобразования", OnReset));

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
            split.Panel1MinSize = 440;
        }

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

        void SetModel(Polyhedron newModel, bool center, bool registerInCombo = true)
        {
            _model = newModel;
            if (registerInCombo)
            {
                _loadedModels[newModel.Name] = newModel;
                _suppressComboEvent = true;
                try
                {
                    if (!_modelCombo.Items.Contains(newModel.Name))
                        _modelCombo.Items.Add(newModel.Name);
                    _modelCombo.SelectedItem = newModel.Name;
                }
                finally { _suppressComboEvent = false; }
            }

            _initialMatrix = center ? Mat4.Translation(-newModel.Center) : Mat4.Identity;
            _modelMatrix = _initialMatrix;

            _sceneRadius = ComputeRadius(_model, _initialMatrix);

            Redraw();
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

            _renderer.Projection = _perspRadio.Checked ? ProjectionMode.Perspective
                                                 : ProjectionMode.Axonometric;
            _renderer.SceneRadius = _sceneRadius;   

            _renderer.Clear(unchecked((int)0xFFF2F2F2));
            _renderer.Render(_model, _modelMatrix);
            _renderer.Present();
            _pictureBox.Invalidate();
        }

        void OnModelChanged(object s, EventArgs e)
        {
            if (_suppressComboEvent || !IsHandleCreated) return;
            var name = _modelCombo.SelectedItem as string;
            if (string.IsNullOrEmpty(name)) return;

            if (name == "Икосаэдр")
                SetModel(Primitives.Icosahedron(), false, registerInCombo: false);
            else if (name == "Додекаэдр")
                SetModel(Primitives.Dodecahedron(), false, registerInCombo: false);
            else if (name == "Куб")
                SetModel(Primitives.Cube(), false, registerInCombo: false);
            else if (name == "Тетраэдр")
                SetModel(Primitives.Tetrahedron(), false, registerInCombo: false);
            else if (name == "Гексаэдр")
                SetModel(Primitives.Cube(), false, registerInCombo: false);
            else if (name == "Октаэдр")
                SetModel(Primitives.Octahedron(), false, registerInCombo: false);
            else if (_loadedModels.TryGetValue(name, out var m))
                SetModel(m, true);
        }

        void OnProjectionChanged(object s, EventArgs e) { if (IsHandleCreated) Redraw(); }

        double D(TextBox b)
        {
            var s = b.Text.Replace(',', '.').Trim();
            return double.TryParse(s, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0;
        }

        static double ParseD(string s)
        {
            s = s.Replace(',', '.').Trim();
            if (!double.TryParse(s, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var v))
                throw new FormatException($"Не удалось разобрать число: '{s}'");
            return v;
        }

        double Rad(double deg) => deg * Math.PI / 180.0;

        void OnLoadFile(object s, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "Wavefront OBJ (*.obj)|*.obj|Все файлы (*.*)|*.*",
                Title = "Загрузить модель"
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            Polyhedron loaded;
            try { loaded = ObjIO.Load(dlg.FileName); }
            catch (Exception ex) { MessageBox.Show(this, "Ошибка загрузки: " + ex.Message); return; }

            SetModel(loaded, center: true);
        }

        void OnSaveFile(object s, EventArgs e)
        {
            if (_model == null) return;
            using var dlg = new SaveFileDialog
            {
                Filter = "Wavefront OBJ (*.obj)|*.obj",
                Title = "Сохранить модель",
                FileName = _model.Name + ".obj"
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            try { ObjIO.Save(dlg.FileName, _model); }
            catch (Exception ex) { MessageBox.Show(this, "Ошибка сохранения: " + ex.Message); }
        }


        static double ComputeRadius(Polyhedron m, Mat4 mat)
        {
            if (m == null || m.Vertices.Count == 0) return 1.0;
            double r2 = 0;
            foreach (var v in m.Vertices)
            {
                var w = mat.TransformPoint(v);
                double d2 = w.X * w.X + w.Y * w.Y + w.Z * w.Z;
                if (d2 > r2) r2 = d2;
            }
            double r = Math.Sqrt(r2);
            return r > 1e-6 ? r : 1.0;
        }

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
            if (Math.Abs(k) < 1e-6 || double.IsNaN(k) || double.IsInfinity(k))
            {
                MessageBox.Show("Введите коэффициент масштаба ≠ 0 (например, 1.2).");
                return;
            }
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
            var worldCenter = _modelMatrix.TransformPoint(_model.Center);
            _modelMatrix = Mat4.RotateAroundAxisThroughPoint(worldCenter, axis, Rad(D(_axisAngleBox)))
                         * _modelMatrix;
            Redraw();
        }

        void OnRotAroundLine(object s, EventArgs e)
        {
            var p1 = new Vec3(D(_l1xBox), D(_l1yBox), D(_l1zBox));
            var p2 = new Vec3(D(_l2xBox), D(_l2yBox), D(_l2zBox));
            if ((p2 - p1).Length < 1e-9)
            {
                MessageBox.Show("Точки A и B должны быть различны.");
                return;
            }
            _modelMatrix = Mat4.RotationAroundLine(p1, p2, Rad(D(_lineAngleBox))) * _modelMatrix;
            Redraw();
        }

        void OnReset(object s, EventArgs e) { _modelMatrix = _initialMatrix; Redraw(); }

        void OnBuildRevolution(object s, EventArgs e)
        {
            try
            {
                var pts = new List<Vec3>();
                foreach (var raw in _generatrixBox.Text.Split('\n'))
                {
                    var line = raw.Replace(',', '.').Trim();
                    if (line.Length == 0) continue;
                    var p = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (p.Length < 2) throw new FormatException($"Строка «{raw}»: нужно хотя бы 2 координаты.");
                    double x = ParseD(p[0]);
                    double y = ParseD(p[1]);
                    double z = p.Length >= 3 ? ParseD(p[2]) : 0.0;
                    pts.Add(new Vec3(x, y, z));
                }

                int div = (int)D(_revolutionDivBox);
                Vec3 axis = _revolutionAxisCombo.SelectedIndex == 0 ? Vec3.UnitX
                          : _revolutionAxisCombo.SelectedIndex == 1 ? Vec3.UnitY
                          : Vec3.UnitZ;

                var m = Revolution.Build(pts, axis, div);
                m.Name = $"Revolution_{++_buildCounter}";
                SetModel(m, center: true);
            }
            catch (Exception ex) { MessageBox.Show(this, "Ошибка построения: " + ex.Message); }
        }
        void OnBuildSurface(object s, EventArgs e)
        {
            try
            {
                var f = GetSelectedFunction(_functionCombo.SelectedIndex);
                double x0 = D(_fx0), x1 = D(_fx1), y0 = D(_fy0), y1 = D(_fy1);
                if (x1 <= x0 || y1 <= y0) throw new ArgumentException("Диапазоны должны быть непустыми (x1 > x0, y1 > y0).");
                int nx = (int)D(_fnx), ny = (int)D(_fny);

                var m = SurfacePlot.Build(f, x0, x1, y0, y1, nx, ny);
                m.Name = $"Surface_{++_buildCounter}";
                SetModel(m, center: true);
            }
            catch (Exception ex) { MessageBox.Show(this, "Ошибка построения: " + ex.Message); }
        }

        static Func<double, double, double> GetSelectedFunction(int idx) => idx switch
        {
            0 => (x, y) => Math.Sin(x) * Math.Cos(y),
            1 => (x, y) => x * x - y * y,
            2 => (x, y) => Math.Exp(-(x * x + y * y)),
            3 => (x, y) => Math.Sin(Math.Sqrt(x * x + y * y)),
            4 => (x, y) => x * x + y * y,
            5 => (x, y) => Math.Cos(x) * Math.Sin(y) + 0.5 * Math.Sin(3 * x) * Math.Cos(3 * y),
            _ => (x, y) => 0
        };
    }
}