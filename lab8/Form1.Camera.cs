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
            
        }

        partial void UpdateCameraUI()
        {
            if (_eyeX == null) return;
        }

        partial void OnCameraModelChanged() => UpdateCameraUI();

        void OnCameraApply(object s, EventArgs e)
        {
            
        }
        void OnCameraOrbitTick(object s, EventArgs e) 
        { 
        
        }
    }
}