
namespace lab_cg_5
{
    partial class Task2
    {
        private System.ComponentModel.IContainer components = null;

        private BufferedPanel canvas;
        private Panel controlsPanel;
        private NumericUpDown numIterations;
        private NumericUpDown numRoughness;
        private NumericUpDown numDecay;
        private NumericUpDown numStartHeight;
        private NumericUpDown numSeed;
        private Button btnGenerate;
        private Button btnPrevious;
        private Button btnNext;
        private Button btnAll;
        private Button btnPlay;
        private CheckBox chkFill;
        private CheckBox chkGrid;
        private CheckBox chkPoints;
        private Label lblTitle;
        private Label lblIterations;
        private Label lblRoughness;
        private Label lblDecay;
        private Label lblStartHeight;
        private Label lblSeed;
        private Label lblStep;
        private Label lblInfo;
        private System.Windows.Forms.Timer animationTimer;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            canvas = new BufferedPanel();
            controlsPanel = new Panel();
            numIterations = new NumericUpDown();
            numRoughness = new NumericUpDown();
            numDecay = new NumericUpDown();
            numStartHeight = new NumericUpDown();
            numSeed = new NumericUpDown();
            btnGenerate = new Button();
            btnPrevious = new Button();
            btnNext = new Button();
            btnAll = new Button();
            btnPlay = new Button();
            chkFill = new CheckBox();
            chkGrid = new CheckBox();
            chkPoints = new CheckBox();
            lblTitle = new Label();
            lblIterations = new Label();
            lblRoughness = new Label();
            lblDecay = new Label();
            lblStartHeight = new Label();
            lblSeed = new Label();
            lblStep = new Label();
            lblInfo = new Label();
            animationTimer = new System.Windows.Forms.Timer(components);

            ((System.ComponentModel.ISupportInitialize)numIterations).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numRoughness).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numDecay).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numStartHeight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSeed).BeginInit();

            controlsPanel.SuspendLayout();
            SuspendLayout();

            canvas.Dock = DockStyle.Fill;
            canvas.BackColor = Color.FromArgb(235, 245, 255);
            canvas.Name = "canvas";
            canvas.Paint += Canvas_Paint;

            controlsPanel.Dock = DockStyle.Right;
            controlsPanel.Width = 280;
            controlsPanel.BackColor = Color.FromArgb(245, 247, 250);
            controlsPanel.AutoScroll = true;
            controlsPanel.Name = "controlsPanel";

            lblTitle.Left = 15;
            lblTitle.Top = 20;
            lblTitle.Width = 250;
            lblTitle.Height = 40;
            lblTitle.Text = "Midpoint Displacement";
            lblTitle.Font = new Font("Segoe UI", 14, FontStyle.Bold);

            ConfigureLabel(lblIterations, "Количество итераций", 70);
            ConfigureLabel(lblRoughness, "Шероховатость", 140);
            ConfigureLabel(lblDecay, "Уменьшение смещения", 210);
            ConfigureLabel(lblStartHeight, "Начальная высота (%)", 280);
            ConfigureLabel(lblSeed, "Seed", 350);

            ConfigureNumeric(numIterations, 95, 1, 11, 7, 0, 1);
            ConfigureNumeric(numRoughness, 165, 0, 2, 0.55m, 2, 0.05m);
            ConfigureNumeric(numDecay, 235, 0.1m, 1, 0.55m, 2, 0.05m);
            ConfigureNumeric(numStartHeight, 305, 10, 90, 50, 0, 5);
            ConfigureNumeric(numSeed, 375, 0, 999999, 12345, 0, 1);

            ConfigureButton(btnGenerate, "Построить", 425);
            ConfigureButton(btnPrevious, "Предыдущий шаг", 468);
            ConfigureButton(btnNext, "Следующий шаг", 511);
            ConfigureButton(btnAll, "Все шаги", 554);
            ConfigureButton(btnPlay, "Автовоспроизведение", 597);

            btnGenerate.Click += Generate_Click;
            btnPrevious.Click += Previous_Click;
            btnNext.Click += Next_Click;
            btnAll.Click += All_Click;
            btnPlay.Click += Play_Click;

            ConfigureCheckBox(chkFill, "Заливка гор", 650, true);
            ConfigureCheckBox(chkGrid, "Показывать сетку", 682, true);
            ConfigureCheckBox(chkPoints, "Показывать вершины", 714, false);

            chkFill.CheckedChanged += DisplayOptions_Changed;
            chkGrid.CheckedChanged += DisplayOptions_Changed;
            chkPoints.CheckedChanged += DisplayOptions_Changed;

            lblStep.Left = 15;
            lblStep.Top = 765;
            lblStep.Width = 245;
            lblStep.Height = 45;
            lblStep.Font = new Font("Segoe UI", 11, FontStyle.Bold);

            lblInfo.Left = 15;
            lblInfo.Top = 815;
            lblInfo.Width = 245;
            lblInfo.Height = 110;
            lblInfo.Font = new Font("Segoe UI", 9);
            lblInfo.ForeColor = Color.DimGray;

            animationTimer.Interval = 650;
            animationTimer.Tick += AnimationTimer_Tick;

            controlsPanel.Controls.Add(lblTitle);
            controlsPanel.Controls.Add(lblIterations);
            controlsPanel.Controls.Add(lblRoughness);
            controlsPanel.Controls.Add(lblDecay);
            controlsPanel.Controls.Add(lblStartHeight);
            controlsPanel.Controls.Add(lblSeed);

            controlsPanel.Controls.Add(numIterations);
            controlsPanel.Controls.Add(numRoughness);
            controlsPanel.Controls.Add(numDecay);
            controlsPanel.Controls.Add(numStartHeight);
            controlsPanel.Controls.Add(numSeed);

            controlsPanel.Controls.Add(btnGenerate);
            controlsPanel.Controls.Add(btnPrevious);
            controlsPanel.Controls.Add(btnNext);
            controlsPanel.Controls.Add(btnAll);
            controlsPanel.Controls.Add(btnPlay);

            controlsPanel.Controls.Add(chkFill);
            controlsPanel.Controls.Add(chkGrid);
            controlsPanel.Controls.Add(chkPoints);
            controlsPanel.Controls.Add(lblStep);
            controlsPanel.Controls.Add(lblInfo);

            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 750);
            MinimumSize = new Size(950, 600);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Midpoint Displacement — горный массив";

            Controls.Add(canvas);
            Controls.Add(controlsPanel);

            ((System.ComponentModel.ISupportInitialize)numIterations).EndInit();
            ((System.ComponentModel.ISupportInitialize)numRoughness).EndInit();
            ((System.ComponentModel.ISupportInitialize)numDecay).EndInit();
            ((System.ComponentModel.ISupportInitialize)numStartHeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSeed).EndInit();

            controlsPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        private void ConfigureLabel(Label label, string text, int top)
        {
            label.Left = 15;
            label.Top = top;
            label.Width = 245;
            label.Height = 23;
            label.Text = text;
            label.Font = new Font("Segoe UI", 9);
        }

        private void ConfigureNumeric(NumericUpDown numeric, int top, decimal min, decimal max, decimal value, int decimals, decimal increment)
        {
            numeric.Left = 15;
            numeric.Top = top;
            numeric.Width = 245;
            numeric.Height = 30;
            numeric.Minimum = min;
            numeric.Maximum = max;
            numeric.DecimalPlaces = decimals;
            numeric.Increment = increment;
            numeric.Value = value;
            numeric.Font = new Font("Segoe UI", 10);
        }

        private void ConfigureButton(Button button, string text, int top)
        {
            button.Left = 15;
            button.Top = top;
            button.Width = 245;
            button.Height = 36;
            button.Text = text;
            button.Font = new Font("Segoe UI", 9);
            button.BackColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Color.LightGray;
        }

        private void ConfigureCheckBox(CheckBox checkBox, string text, int top, bool value)
        {
            checkBox.Left = 15;
            checkBox.Top = top;
            checkBox.Width = 245;
            checkBox.Height = 28;
            checkBox.Text = text;
            checkBox.Checked = value;
            checkBox.Font = new Font("Segoe UI", 9);
        }
    }
}
