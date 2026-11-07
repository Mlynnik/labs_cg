using lab6.Math3d;
using lab6.Model;
using lab6.Rendering;
using lab6.Rendering;

namespace lab6
{
    public partial class Form1 : Form
    {
        Polyhedron _model;
        Mat4 _modelMatrix = Mat4.Identity;
        Mat4 _initialMatrix = Mat4.Identity;
        Renderer _renderer;

        List<SceneObject> _scene = new List<SceneObject>();
        int _activeIdx = -1;
        SceneObject Active => (_activeIdx >= 0 && _activeIdx < _scene.Count) ? _scene[_activeIdx] : null;

        Camera _camera = new Camera();
        System.Windows.Forms.Timer _timer;

        double _animationAngle = 0;      // вращение объекта
        bool _suppressCameraEvent = false;

        // контролы ЛР8
        ComboBox _sceneCombo;
        RadioButton _perspRadio, _axonRadio;
        CheckBox _cullCheck, _rotateObjCheck;
        TextBox _lightX, _lightY, _lightZ;

        readonly Dictionary<string, Polyhedron> _loadedModels = new();
        bool _suppressComboEvent = false;

        ComboBox _modelCombo;
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

        partial void BuildZBufferUI(FlowLayoutPanel host);
        partial void BuildCameraUI(FlowLayoutPanel host);
        partial void UpdateCameraUI();
        partial void OnCameraModelChanged();
        partial void OnFlagsChanged(object s, EventArgs e);
        partial void OnAddCopy(object s, EventArgs e);
        partial void OnRemoveActive(object s, EventArgs e);
        partial void OnClearScene(object s, EventArgs e);
        partial void OnOverlapDemo(object s, EventArgs e);

        public Form1()
        {
            Text = "Lab8 — отсечение, Z-буфер, камера";
            Width = 1400; Height = 1000;
            StartPosition = FormStartPosition.CenterScreen;

            BuildUI();
            Load += (_, __) =>
            {
                AddModel(Primitives.Icosahedron(), new Vec3(0.85, 0.55, 0.30));
                ResetCameraToScene();
                Redraw();
            };

            _timer = new System.Windows.Forms.Timer { Interval = 33 };
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        void ResetCameraToScene()
        {
            double maxR = 1.0;
            foreach (var o in _scene)
                foreach (var v in o.Model.Vertices)
                {
                    double r = o.ModelMatrix.TransformPoint(v).Length;
                    if (r > maxR) maxR = r;
                }
            double d = 4.0 * maxR;
            _camera = new Camera
            {
                Eye = new Vec3(0, 0, d),
                Target = Vec3.Zero,
                Near = d * 0.01,
                Far = d * 20,
                FovDeg = 60
            };
            _renderer.OrthoHalfSize = maxR * 1.2;
            OnCameraModelChanged();
        }

        void OnTimerTick(object s, EventArgs e)
        {
            bool changed = false;

            if (_rotateObjCheck?.Checked == true)
            {
                _animationAngle += 0.03;
                changed = true;
            }
            if (_orbitCamCheck?.Checked == true)
            {
                _orbitAngle += 0.02;
                double r = Math.Sqrt(_camera.Eye.X * _camera.Eye.X + _camera.Eye.Z * _camera.Eye.Z);
                if (r < 1e-3) r = _camera.Eye.Length;
                _camera.Eye = new Vec3(r * Math.Sin(_orbitAngle), _camera.Eye.Y, r * Math.Cos(_orbitAngle));
                UpdateCameraUI();
                changed = true;
            }
            if (changed) Redraw();
        }


        static string F(double v) => v.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);


        void AddModel(Polyhedron model, Vec3 color, bool fitCamera = false)
        {
            var obj = new SceneObject
            {
                Name = $"{model.Name}_{_scene.Count + 1}",
                Model = model,
                BaseColor = color,
                ModelMatrix = Mat4.Translation(-model.Center)
            };
            _scene.Add(obj);
            _activeIdx = _scene.Count - 1;
            RefreshSceneCombo();
            if (fitCamera) ResetCameraToScene();
            Redraw();
        }

        void RefreshSceneCombo()
        {
            if (_sceneCombo == null) return;
            _suppressCameraEvent = true;
            try
            {
                _sceneCombo.Items.Clear();
                foreach (var o in _scene) _sceneCombo.Items.Add(o.Name);
                if (_activeIdx >= 0 && _activeIdx < _sceneCombo.Items.Count)
                    _sceneCombo.SelectedIndex = _activeIdx;
            }
            finally { _suppressCameraEvent = false; }
        }

        void OnSceneComboChanged(object s, EventArgs e)
        {
            if (_suppressCameraEvent) return;
            _activeIdx = _sceneCombo.SelectedIndex;
            Redraw();
        }

        void OnAddIco(object s, EventArgs e) => AddModel(Primitives.Icosahedron(), new Vec3(0.85, 0.55, 0.30));
        void OnAddDodec(object s, EventArgs e) => AddModel(Primitives.Dodecahedron(), new Vec3(0.30, 0.55, 0.85));



        void OnLightChanged(object s, EventArgs e)
        {
            _renderer.LightDir = new Vec3(D(_lightX), D(_lightY), D(_lightZ));
            Redraw();
        }

        //----------------------

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

            // ---------- Сцена ----------
            _sceneCombo = new ComboBox
            {
                Width = 380,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _sceneCombo.SelectedIndexChanged += OnSceneComboChanged;

            var scenePanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                WrapContents = false
            };
            scenePanel.Controls.Add(_sceneCombo);
            scenePanel.Controls.Add(Row(
                MakeButton("Добавить икосаэдр", OnAddIco),
                MakeButton("Добавить додекаэдр", OnAddDodec),
                MakeButton("Добавить копию", OnAddCopy)));
            scenePanel.Controls.Add(Row(
                MakeButton("Удалить активный", OnRemoveActive),
                MakeButton("Очистить сцену", OnClearScene)));
            scenePanel.Controls.Add(Row(
                MakeButton("Загрузить OBJ…", OnLoadFile),
                MakeButton("Сохранить OBJ…", OnSaveFile)));

            AddGroup(stack, "Сцена", scenePanel);

            _perspRadio = new RadioButton { Text = "Перспективная", Checked = true, AutoSize = true };
            _axonRadio = new RadioButton { Text = "Аксонометрическая", AutoSize = true };
            _perspRadio.CheckedChanged += OnProjectionChanged;
            _axonRadio.CheckedChanged += OnProjectionChanged;
            AddGroup(stack, "Проекция", Row(_perspRadio, _axonRadio));

            // ---------- Задание 1: отсечение + вращение ----------
            _cullCheck = new CheckBox { Text = "Отсечение нелицевых граней", Checked = true, AutoSize = true };
            _cullCheck.CheckedChanged += OnFlagsChanged;

            _rotateObjCheck = new CheckBox { Text = "Вращать объект", Checked = false, AutoSize = true };

            AddGroup(stack, "Задание 1: отсечение",
                Row(_cullCheck, _rotateObjCheck));



            // -------- Z-буфер и режимы (P2 наполнит) --------
            var zbHost = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                WrapContents = false
            };
            AddGroup(stack, "Z-буфер и режимы", zbHost);
            BuildZBufferUI(zbHost);                // ← P2 реализует тело

            // -------- Камера (P3 наполнит) --------
            var camHost = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                WrapContents = false
            };
            AddGroup(stack, "Камера", camHost);
            BuildCameraUI(camHost);                // ← P3 реализует тело



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
            int w = Math.Max(1, _pictureBox.ClientSize.Width);
            int h = Math.Max(1, _pictureBox.ClientSize.Height);
            if (w < 50) w = 900; if (h < 50) h = 700;

            if (_renderer == null || _renderer.W != w || _renderer.H != h)
            {
                _pictureBox.Image = null;
                _renderer?.Bitmap?.Dispose();
                _renderer = new Renderer(w, h);
                _pictureBox.Image = _renderer.Bitmap;
            }

            _renderer.Camera = _camera;
            _renderer.Projection = (_perspRadio?.Checked ?? true) ? ProjectionMode.Perspective
                                                                   : ProjectionMode.Axonometric;
            _renderer.CullBackFaces = _cullCheck?.Checked ?? true;
            _renderer.UseZBuffer = _zBufCheck?.Checked ?? true;    // ← если P2 ещё не влит, дефолт true
            _renderer.Mode = _wireRadio?.Checked == true ? RenderMode.Wireframe
                                    : _fillRadio?.Checked == true ? RenderMode.Filled
                                    : RenderMode.FilledWireframe;

            bool rotate = _rotateObjCheck?.Checked ?? false;
            _renderer.SceneTransform = rotate ? Mat4.RotationY(_animationAngle) : Mat4.Identity;

            _renderer.Clear(_renderer.BackgroundColor);
            _renderer.RenderScene(_scene);
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



        void ApplyActive(Mat4 m, bool right = false)
        {
            if (Active == null) return;
            Active.ModelMatrix = right ? Active.ModelMatrix * m : m * Active.ModelMatrix;
            Redraw();
        }

        void OnTranslate(object s, EventArgs e) => ApplyActive(Mat4.Translation(D(_txBox), D(_tyBox), D(_tzBox)));
        void OnRotX(object s, EventArgs e) => ApplyActive(Mat4.RotationX(Rad(D(_rotAngleBox))));
        void OnRotY(object s, EventArgs e) => ApplyActive(Mat4.RotationY(Rad(D(_rotAngleBox))));
        void OnRotZ(object s, EventArgs e) => ApplyActive(Mat4.RotationZ(Rad(D(_rotAngleBox))));
        void OnScale(object s, EventArgs e) { double k = D(_scaleBox); if (Math.Abs(k) > 1e-6) ApplyActive(Mat4.Scale(k, k, k)); }
        void OnReflectXY(object s, EventArgs e) => ApplyActive(Mat4.ReflectionXY());
        void OnReflectXZ(object s, EventArgs e) => ApplyActive(Mat4.ReflectionXZ());
        void OnReflectYZ(object s, EventArgs e) => ApplyActive(Mat4.ReflectionYZ());

        void OnScaleCenter(object s, EventArgs e)
        {
            if (Active == null) return;
            double k = D(_scaleBox);
            if (Math.Abs(k) < 1e-6) return;
            Active.ModelMatrix = Active.ModelMatrix * Mat4.ScaleAround(Active.Model.Center, k);
            Redraw();
        }

        void OnRotAroundCenterAxis(object s, EventArgs e)
        {
            if (Active == null) return;
            var axis = _axisCombo.SelectedIndex == 0 ? Vec3.UnitX
                     : _axisCombo.SelectedIndex == 1 ? Vec3.UnitY
                     : Vec3.UnitZ;
            var worldCenter = Active.ModelMatrix.TransformPoint(Active.Model.Center);
            Active.ModelMatrix = Mat4.RotateAroundAxisThroughPoint(worldCenter, axis, Rad(D(_axisAngleBox)))
                               * Active.ModelMatrix;
            Redraw();
        }

        void OnRotAroundLine(object s, EventArgs e)
        {
            if (Active == null) return;
            var p1 = new Vec3(D(_l1xBox), D(_l1yBox), D(_l1zBox));
            var p2 = new Vec3(D(_l2xBox), D(_l2yBox), D(_l2zBox));
            if ((p2 - p1).Length < 1e-9) return;
            ApplyActive(Mat4.RotationAroundLine(p1, p2, Rad(D(_lineAngleBox))));
        }

        void OnReset(object s, EventArgs e)
        {
            if (Active == null) return;
            Active.ModelMatrix = Mat4.Identity;
            Redraw();
        }


        void OnLoadFile(object s, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "Wavefront OBJ (*.obj)|*.obj|Все файлы (*.*)|*.*"
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            try { AddModel(ObjIO.Load(dlg.FileName), new Vec3(0.85, 0.55, 0.30), fitCamera: true); }
            catch (Exception ex) { MessageBox.Show(this, "Ошибка загрузки: " + ex.Message); }
        }

        void OnSaveFile(object s, EventArgs e)
        {
            if (Active == null) return;
            using var dlg = new SaveFileDialog
            {
                Filter = "Wavefront OBJ (*.obj)|*.obj",
                FileName = Active.Model.Name + ".obj"
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            try { ObjIO.Save(dlg.FileName, Active.Model); }
            catch (Exception ex) { MessageBox.Show(this, "Ошибка сохранения: " + ex.Message); }
        }

        // Постройка фигуры вращения / поверхности — те же, что в ЛР7,
        // только в конце вместо SetModel(...) вызывают AddModel(m, color).
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
                    if (p.Length < 2) throw new FormatException($"Строка «{raw}»: нужно ≥2 координаты");
                    double x = ParseD(p[0]), y = ParseD(p[1]),
                           z = p.Length >= 3 ? ParseD(p[2]) : 0;
                    pts.Add(new Vec3(x, y, z));
                }
                int div = (int)D(_revolutionDivBox);
                var axis = _revolutionAxisCombo.SelectedIndex == 0 ? Vec3.UnitX
                         : _revolutionAxisCombo.SelectedIndex == 1 ? Vec3.UnitY
                         : Vec3.UnitZ;
                AddModel(Revolution.Build(pts, axis, div), new Vec3(0.8, 0.6, 0.3), fitCamera: true);
            }
            catch (Exception ex) { MessageBox.Show(this, "Ошибка: " + ex.Message); }
        }

        void OnBuildSurface(object s, EventArgs e)
        {
            try
            {
                var f = GetSelectedFunction(_functionCombo.SelectedIndex);
                double x0 = D(_fx0), x1 = D(_fx1), y0 = D(_fy0), y1 = D(_fy1);
                if (x1 <= x0 || y1 <= y0) throw new ArgumentException("Непустые диапазоны: x1>x0, y1>y0");
                int nx = (int)D(_fnx), ny = (int)D(_fny);
                AddModel(SurfacePlot.Build(f, x0, x1, y0, y1, nx, ny), new Vec3(0.4, 0.7, 0.4), fitCamera: true);
            }
            catch (Exception ex) { MessageBox.Show(this, "Ошибка: " + ex.Message); }
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