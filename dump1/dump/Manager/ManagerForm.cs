using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dump
{
    public partial class ManagerForm : Form
    {
        public ManagerForm()
        {
            InitializeComponent();
            SetupButtonStyles();

            panel1.Visible = false;
            buttonUse.Visible = false;
            buttonIssue.Visible = false;
        }

        private void SetupButtonStyles()
        {
            buttonOrder.FlatStyle = FlatStyle.Flat;
            buttonOrder.FlatAppearance.BorderSize = 1;
            buttonOrder.FlatAppearance.BorderColor = Color.Black;
            buttonOrder.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            buttonOrder.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            buttonOrder.MouseDown += (s, e) => buttonOrder.FlatAppearance.BorderColor = Color.DarkBlue;
            buttonOrder.MouseUp += (s, e) => buttonOrder.FlatAppearance.BorderColor = Color.Black;
            buttonOrder.MouseLeave += (s, e) => buttonOrder.FlatAppearance.BorderColor = Color.Black;

            buttonCerts.FlatStyle = FlatStyle.Flat;
            buttonCerts.FlatAppearance.BorderSize = 1;
            buttonCerts.FlatAppearance.BorderColor = Color.Black;
            buttonCerts.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            buttonCerts.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            buttonCerts.MouseDown += (s, e) => buttonCerts.FlatAppearance.BorderColor = Color.DarkBlue;
            buttonCerts.MouseUp += (s, e) => buttonCerts.FlatAppearance.BorderColor = Color.Black;
            buttonCerts.MouseLeave += (s, e) => buttonCerts.FlatAppearance.BorderColor = Color.Black;

            buttonCurrentOrders.FlatStyle = FlatStyle.Flat;
            buttonCurrentOrders.FlatAppearance.BorderSize = 1;
            buttonCurrentOrders.FlatAppearance.BorderColor = Color.Black;
            buttonCurrentOrders.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            buttonCurrentOrders.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            buttonCurrentOrders.MouseDown += (s, e) => buttonCurrentOrders.FlatAppearance.BorderColor = Color.DarkBlue;
            buttonCurrentOrders.MouseUp += (s, e) => buttonCurrentOrders.FlatAppearance.BorderColor = Color.Black;
            buttonCurrentOrders.MouseLeave += (s, e) => buttonCurrentOrders.FlatAppearance.BorderColor = Color.Black;

            buttonIssue.FlatStyle = FlatStyle.Flat;
            buttonIssue.FlatAppearance.BorderSize = 1;
            buttonIssue.FlatAppearance.BorderColor = Color.Black;
            buttonIssue.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            buttonIssue.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            buttonIssue.MouseDown += (s, e) => buttonIssue.FlatAppearance.BorderColor = Color.DarkBlue;
            buttonIssue.MouseUp += (s, e) => buttonIssue.FlatAppearance.BorderColor = Color.Black;
            buttonIssue.MouseLeave += (s, e) => buttonIssue.FlatAppearance.BorderColor = Color.Black;

            buttonUse.FlatStyle = FlatStyle.Flat;
            buttonUse.FlatAppearance.BorderSize = 1;
            buttonUse.FlatAppearance.BorderColor = Color.Black;
            buttonUse.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            buttonUse.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            buttonUse.MouseDown += (s, e) => buttonUse.FlatAppearance.BorderColor = Color.DarkBlue;
            buttonUse.MouseUp += (s, e) => buttonUse.FlatAppearance.BorderColor = Color.Black;
            buttonUse.MouseLeave += (s, e) => buttonUse.FlatAppearance.BorderColor = Color.Black;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            LoginForm login = new LoginForm();
            login.Show();
        }

        private void ManagerForm_Load(object sender, EventArgs e)
        {
            panel1.Visible = false;
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            ManagerForm manager = new ManagerForm();
            manager.Show();
        }

        private void buttonCerts_Click(object sender, EventArgs e)
        {
            panel1.Visible = true;
            panel1.BringToFront();

            // ПРИНУДИТЕЛЬНО ПОКАЗЫВАЕМ И ВЫНОСИМ НА ПЕРЕДНИЙ ПЛАН
            if (buttonIssue != null)
            {
                buttonIssue.Visible = true;
                buttonIssue.BringToFront();
            }

            if (buttonUse != null)
            {
                buttonUse.Visible = true;
                buttonUse.BringToFront();
            }
        }

        private void btnBackFromPanel_Click(object sender, EventArgs e)
        {
            panel1.Visible = false;
        }

        private void buttonOrder_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            Menu Menu1 = new Menu();
            Menu1.Show();
        }

        private void buttonCurrentOrders_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            Orders Order = new Orders();
            Order.Show();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
        }

        private void buttonIssue_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            AddSertificateForm add = new AddSertificateForm();
            add.Show();
        }

        private void buttonUse_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            EmloyForm Emloy = new EmloyForm();
            Emloy.Show();
        }
    }
}