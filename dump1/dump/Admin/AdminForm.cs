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
    public partial class AdminForm : Form
    {
        public AdminForm()
        {
            InitializeComponent();
            button1.FlatStyle = FlatStyle.Flat;

            button1.FlatAppearance.BorderSize = 1;
            button1.FlatAppearance.BorderColor = Color.Black;
            button1.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            button1.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            // Добавляем обработчики для возврата border при отпускании мыши
            button1.MouseDown += (s, e) =>
            {
                button1.FlatAppearance.BorderColor = Color.DarkBlue; // Темнее при нажатии
            };

            button1.MouseUp += (s, e) =>
            {
                button1.FlatAppearance.BorderColor = Color.Black; // Возвращаем черный
            };
            button1.MouseLeave += (s, e) =>
            {
                button1.FlatAppearance.BorderColor = Color.Black; // Возвращаем черный при уходе мыши
            };

            button2.FlatStyle = FlatStyle.Flat;

            button2.FlatAppearance.BorderSize = 1;
            button2.FlatAppearance.BorderColor = Color.Black;
            button2.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            button2.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            button2.MouseDown += (s, e) =>
            {
                button2.FlatAppearance.BorderColor = Color.DarkBlue;
            };

            button2.MouseUp += (s, e) =>
            {
                button2.FlatAppearance.BorderColor = Color.Black;
            };
            button2.MouseLeave += (s, e) =>
            {
                button2.FlatAppearance.BorderColor = Color.Black;
            };


            button3.FlatStyle = FlatStyle.Flat;

            button3.FlatAppearance.BorderSize = 1;
            button3.FlatAppearance.BorderColor = Color.Black;
            button3.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            button3.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            button3.MouseDown += (s, e) =>
            {
                button3.FlatAppearance.BorderColor = Color.DarkBlue;
            };

            button3.MouseUp += (s, e) =>
            {
                button3.FlatAppearance.BorderColor = Color.Black;
            };
            button3.MouseLeave += (s, e) =>
            {
                button3.FlatAppearance.BorderColor = Color.Black;
            };

            button4.FlatStyle = FlatStyle.Flat;

            button4.FlatAppearance.BorderSize = 1;
            button4.FlatAppearance.BorderColor = Color.Black;
            button4.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            button4.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            button4.MouseDown += (s, e) =>
            {
                button4.FlatAppearance.BorderColor = Color.DarkBlue;
            };

            button4.MouseUp += (s, e) =>
            {
                button4.FlatAppearance.BorderColor = Color.Black;
            };
            button4.MouseLeave += (s, e) =>
            {
                button4.FlatAppearance.BorderColor = Color.Black;
            };
        }

        private void AdminForm_Load(object sender, EventArgs e)
        {
    
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Visible = false;

            UsersForm users = new UsersForm();
            users.Show();
        }



        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Visible = false;

            LoginForm login = new LoginForm();
            login.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Visible = false;

            OrdersForm Orders = new OrdersForm();
            Orders.Show();
        }


        private void button4_Click(object sender, EventArgs e)
        {
            this.Visible = false;

            AdminMenu adminMenu = new AdminMenu("admin");
            adminMenu.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Visible = false;

            Spravochnici Spravochnic = new Spravochnici();
            Spravochnic.Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Visible = false;

            ImportRestoreForm import = new ImportRestoreForm();
            import.Show();
        }
    }
}
