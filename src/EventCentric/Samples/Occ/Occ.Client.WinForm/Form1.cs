using Microsoft.Practices.Unity;
using Occ.Client.Shared;
using System.Windows.Forms;

namespace Occ.Client.WinForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            this.panel1.Enabled = false;
        }

        private void loginBtn_Click(object sender, System.EventArgs e)
        {
            this.userNameInput.Enabled = false;
            this.loginBtn.Enabled = false;

            Program.InitializeSystem(this.userNameInput.Text);

            this.panel1.Enabled = true;
        }

        private void createItemBtn_click(object sender, System.EventArgs e)
        {
            this.panel1.Enabled = false;

            var app = Program.Container.Resolve<ItemClientApp>();
            app.CreateItem(this.itemNameInput.Text);

            this.panel1.Enabled = true;
        }
    }
}
