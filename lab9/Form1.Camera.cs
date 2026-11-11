using lab6.Math3d;
using System;
using System.Windows.Forms;

namespace lab6
{
    public partial class Form1
    {
        // Контролы камеры — объявлены в ЭТОМ partial
        TextBox _eyeX, _eyeY, _eyeZ, _tgtX, _tgtY, _tgtZ, _fovBox;
        CheckBox _orbitCamCheck;
        System.Windows.Forms.Timer _camTimer;
        double _orbitAngle = 0;

        partial void BuildCameraUI(FlowLayoutPanel host)
        {
            _eyeX = NumBox("0");
            _eyeY = NumBox("0");
            _eyeZ = NumBox("5");
            _tgtX = NumBox("0");
            _tgtY = NumBox("0");
            _tgtZ = NumBox("0");
            _fovBox = NumBox("60");
            _orbitCamCheck = new CheckBox { Text = "Вращать камеру", AutoSize = true };

            host.Controls.Add(new Label { Text = "Положение камеры (x, y, z)", AutoSize = true });
            host.Controls.Add(Row(_eyeX, _eyeY, _eyeZ));
            host.Controls.Add(new Label { Text = "Точка взгляда (x, y, z)", AutoSize = true });
            host.Controls.Add(Row(_tgtX, _tgtY, _tgtZ));
            host.Controls.Add(Row(new Label { Text = "Угол обзора", AutoSize = true }, _fovBox));
            host.Controls.Add(_orbitCamCheck);
            host.Controls.Add(MakeButton("Применить", OnCameraApply));
            UpdateCameraUI();
        }

        partial void UpdateCameraUI()
        {
            if (_eyeX == null) return;
            _eyeX.Text = F(_camera.Eye.X);
            _eyeY.Text = F(_camera.Eye.Y);
            _eyeZ.Text = F(_camera.Eye.Z);
            _tgtX.Text = F(_camera.Target.X);
            _tgtY.Text = F(_camera.Target.Y);
            _tgtZ.Text = F(_camera.Target.Z);
            _fovBox.Text = F(_camera.FovDeg);
        }

        partial void OnCameraModelChanged() => UpdateCameraUI();

        void OnCameraApply(object s, EventArgs e)
        {
            _camera.Eye = new Vec3(D(_eyeX), D(_eyeY), D(_eyeZ));
            _camera.Target = new Vec3(D(_tgtX), D(_tgtY), D(_tgtZ));
            _camera.FovDeg = D(_fovBox);
            Redraw();
        }
        void OnCameraOrbitTick(object s, EventArgs e) 
        { 
        
        }
    }
}