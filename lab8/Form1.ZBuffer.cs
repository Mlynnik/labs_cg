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
            
        }

        void OnModeChanged(object s, EventArgs e)
        {
            
        }

        partial void OnFlagsChanged(object s, EventArgs e)
        {
            
        }

        partial void OnAddCopy(object s, EventArgs e)
        {
            
        }

        partial void OnRemoveActive(object s, EventArgs e)
        {
            
        }

        partial void OnClearScene(object s, EventArgs e)
        {
            _scene.Clear(); _activeIdx = -1;
            RefreshSceneCombo(); Redraw();
        }

        partial void OnOverlapDemo(object s, EventArgs e)
        {
            
        }
    }
}
