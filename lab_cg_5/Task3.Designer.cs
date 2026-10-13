namespace lab_cg_5
{
    partial class Task3
    {
        private BufferedPanel canvas;
        private Panel sidePanel;
        private Label lblTitle;
        private Label lblHelp;
        private Label lblCount;
        private Button btnDelete;
        private Button btnClear;

        private void InitializeComponent()
        {
            canvas = new BufferedPanel();
            sidePanel = new Panel();
            lblTitle = new Label();
            lblHelp = new Label();
            lblCount = new Label();
            btnDelete = new Button();
            btnClear = new Button();

            sidePanel.SuspendLayout();
            SuspendLayout();

            canvas.Dock = DockStyle.Fill;
            canvas.BackColor = Color.White;
            canvas.Name = "canvas";
            canvas.TabStop = true;

            sidePanel.Dock = DockStyle.Right;
            sidePanel.Width = 300;
            sidePanel.BackColor = Color.WhiteSmoke;
            sidePanel.Name = "sidePanel";

            lblTitle.Left = 16;
            lblTitle.Top = 18;
            lblTitle.Width = 268;
            lblTitle.Height = 58;
            lblTitle.Text = "Кубический сплайн Безье";
            lblTitle.Font = new Font("Segoe UI", 14, FontStyle.Bold);

            lblHelp.Left = 16;
            lblHelp.Top = 84;
            lblHelp.Width = 268;
            lblHelp.Height = 110;
            lblHelp.Font = new Font("Segoe UI", 9);
            lblHelp.Text =
                "ЛКМ по пустому месту - добавить точку.\r\n\r\n" +
                "Перетаскивание - переместить.\r\n\r\n" +
                "ПКМ или Delete - удалить.";

            lblCount.Left = 16;
            lblCount.Top = 210;
            lblCount.Width = 268;
            lblCount.Height = 48;
            lblCount.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblCount.Text = "Точек: 0";

            btnDelete.Left = 16;
            btnDelete.Top = 270;
            btnDelete.Width = 268;
            btnDelete.Height = 36;
            btnDelete.Text = "Удалить выбранную";
            btnDelete.Font = new Font("Segoe UI", 9);
            btnDelete.BackColor = Color.White;
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.FlatAppearance.BorderColor = Color.LightGray;

            btnClear.Left = 16;
            btnClear.Top = 316;
            btnClear.Width = 268;
            btnClear.Height = 36;
            btnClear.Text = "Очистить";
            btnClear.Font = new Font("Segoe UI", 9);
            btnClear.BackColor = Color.White;
            btnClear.FlatStyle = FlatStyle.Flat;
            btnClear.FlatAppearance.BorderColor = Color.LightGray;

            sidePanel.Controls.Add(lblTitle);
            sidePanel.Controls.Add(lblHelp);
            sidePanel.Controls.Add(lblCount);
            sidePanel.Controls.Add(btnDelete);
            sidePanel.Controls.Add(btnClear);

            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1100, 700);
            MinimumSize = new Size(820, 560);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Кубические сплайны Безье";
            KeyPreview = true;

            Controls.Add(canvas);
            Controls.Add(sidePanel);

            sidePanel.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
