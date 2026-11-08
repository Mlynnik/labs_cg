using lab6.Math3d;
using lab6.Model;
using lab6.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace lab6
{
    public partial class Form1
    {
        RadioButton _wireRadio, _fillRadio, _fillWireRadio;
        CheckBox _zBufCheck;
        partial void BuildZBufferUI(FlowLayoutPanel host)
        {
            _wireRadio = new RadioButton { Text = "Каркас", AutoSize = true };
            _fillRadio = new RadioButton { Text = "Заливка", AutoSize = true };
            _fillWireRadio = new RadioButton { Text = "Заливка+каркас", AutoSize = true, Checked = true };
            _wireRadio.CheckedChanged += OnModeChanged;
            _fillRadio.CheckedChanged += OnModeChanged;
            _fillWireRadio.CheckedChanged += OnModeChanged;
            _zBufCheck = new CheckBox { Text = "Z-буфер", Checked = true, AutoSize = true };
            _zBufCheck.CheckedChanged += OnFlagsChanged;
            host.Controls.Add(Row(_wireRadio, _fillRadio, _fillWireRadio));
            host.Controls.Add(_zBufCheck);
            host.Controls.Add(Row(
                MakeButton("Демо: перекрытие", OnOverlapDemo),
                MakeButton("Добавить копию", OnAddCopy),
                MakeButton("Удалить активный", OnRemoveActive),
                MakeButton("Очистить сцену", OnClearScene)));
        }

        void OnModeChanged(object s, EventArgs e)
        {
            if (_renderer == null) 
                return;
            _renderer.Mode = _wireRadio?.Checked == true ? RenderMode.Wireframe
                           : _fillRadio?.Checked == true ? RenderMode.Filled
                           : RenderMode.FilledWireframe;
            Redraw();
        }

        partial void OnFlagsChanged(object s, EventArgs e)
        {
            _renderer.CullBackFaces = _cullCheck?.Checked ?? true;
            _renderer.UseZBuffer = _zBufCheck?.Checked ?? true;
            Redraw();
        }

        partial void OnAddCopy(object s, EventArgs e)
        {
            if (Active == null)
                return;
            var copy = Active.Clone();
            copy.Name = $"{Active.Name}_copy{_scene.Count}";
            _scene.Add(copy);
            _activeIdx = _scene.Count - 1;
            RefreshSceneCombo(); 
            Redraw();
        }

        partial void OnRemoveActive(object s, EventArgs e)
        {
            if (_activeIdx < 0) 
                return;
            _scene.RemoveAt(_activeIdx);
            _activeIdx = Math.Min(_activeIdx, _scene.Count - 1);
            RefreshSceneCombo(); 
            Redraw();
        }

        partial void OnClearScene(object s, EventArgs e)
        {
            _scene.Clear(); 
            _activeIdx = -1;
            RefreshSceneCombo(); 
            Redraw();
        }

        partial void OnOverlapDemo(object s, EventArgs e)
        {
            _scene.Clear();
            var ico = Primitives.Icosahedron();
            _scene.Add(new SceneObject
            {
                Name = "Front",
                Model = ico,
                ModelMatrix = Mat4.Translation(0.0, 0.0, 0.0),
                BaseColor = new Vec3(0.9, 0.4, 0.3)
            });
            _scene.Add(new SceneObject
            {
                Name = "Mid",
                Model = ico,
                ModelMatrix = Mat4.Translation(0.8, 0.4, -1.2),
                BaseColor = new Vec3(0.3, 0.8, 0.4)
            });
            _scene.Add(new SceneObject
            {
                Name = "Back",
                Model = ico,
                ModelMatrix = Mat4.Translation(-0.6, -0.4, -2.2),
                BaseColor = new Vec3(0.35, 0.5, 0.95)
            });
            _activeIdx = 0;
            RefreshSceneCombo();
            ResetCameraToScene();
            Redraw();
        }
    }
}
