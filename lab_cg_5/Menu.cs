namespace lab_cg_5
{
    public partial class Menu : Form
    {
        public Menu()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var form = new Task1())
            {
                form.ShowDialog();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (var form = new Task2())
            {
                form.ShowDialog();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (var form = new Task3())
            {
                form.ShowDialog();
            }
        }
    }
}
