using MySql.Data.MySqlClient;
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
        private bool isLockDialogOpen = false;
        public ManagerForm()
        {
            InitializeComponent();
            SetupButtonStyles();

            panel1.Visible = false;
            buttonUse.Visible = false;
            buttonIssue.Visible = false;

            // ПОДПИСЫВАЕМСЯ НА СОБЫТИЕ ЗАКРЫТИЯ ФОРМЫ
            this.FormClosing += ManagerForm_FormClosing;
            InactivityManager.RegisterForm(this);
            InactivityManager.OnLockRequest += LockSystem;
        }
        private void LockSystem()
        {
            if (isLockDialogOpen) return;
            isLockDialogOpen = true;

            this.Invoke(new Action(() =>
            {
                Form lockDialog = new Form();
                lockDialog.Text = "Блокировка системы";
                lockDialog.Size = new Size(380, 230);
                lockDialog.StartPosition = FormStartPosition.CenterScreen;
                lockDialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                lockDialog.MaximizeBox = false;
                lockDialog.MinimizeBox = false;
                lockDialog.TopMost = true;

                Label lblMessage = new Label();
                lblMessage.Text = $"Система заблокирована из-за бездействия ({InactivityManager.GetInactivityTime()} сек.)\nВведите пароль для разблокировки:";
                lblMessage.Location = new Point(20, 20);
                lblMessage.Size = new Size(330, 50);
                lblMessage.TextAlign = ContentAlignment.MiddleCenter;

                Label lblUser = new Label();
                lblUser.Text = $"Пользователь: {CurrentUser.FIO}";
                lblUser.Location = new Point(20, 75);
                lblUser.Size = new Size(330, 25);
                lblUser.TextAlign = ContentAlignment.MiddleCenter;
                lblUser.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Bold);

                TextBox txtPassword = new TextBox();
                txtPassword.Location = new Point(90, 110);
                txtPassword.Size = new Size(180, 20);
                txtPassword.UseSystemPasswordChar = true;

                Button btnUnlock = new Button();
                btnUnlock.Text = "Разблокировать";
                btnUnlock.Location = new Point(130, 145);
                btnUnlock.Size = new Size(100, 30);

                btnUnlock.Click += (s, e) => CheckPasswordAndUnlock(txtPassword, lockDialog);
                txtPassword.KeyPress += (s, e) =>
                {
                    if (e.KeyChar == (char)Keys.Enter)
                        CheckPasswordAndUnlock(txtPassword, lockDialog);
                };

                lockDialog.Controls.Add(lblMessage);
                lockDialog.Controls.Add(lblUser);
                lockDialog.Controls.Add(txtPassword);
                lockDialog.Controls.Add(btnUnlock);
                lockDialog.FormClosed += (s, e) => { isLockDialogOpen = false; };
                lockDialog.ShowDialog();
            }));
        }

        private void CheckPasswordAndUnlock(TextBox txtPassword, Form lockDialog)
        {
            bool isCorrect = false;

            if (CurrentUser.Username == "sisadmin" && CurrentUser.RoleId == 99)
            {
                if (txtPassword.Text == "admin")
                    isCorrect = true;
            }
            else
            {
                string dbPassword = GetPasswordFromDB();
                string inputHash = HashPassword(txtPassword.Text);
                if (inputHash == dbPassword)
                    isCorrect = true;
            }

            if (isCorrect)
            {
                lockDialog.Close();
                InactivityManager.ResetActivity();
            }
            else
            {
                MessageBox.Show("Неверный пароль! Вы будете перенаправлены на окно входа.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                lockDialog.Close();
                InactivityManager.UnregisterForm();
                this.Close();

                LoginForm login = new LoginForm();
                login.Show();
            }
        }

        private string GetPasswordFromDB()
        {
            try
            {
                using (var conn = SettingsBD.GetConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand("SELECT password_hash FROM users WHERE login = @login", conn);
                    cmd.Parameters.AddWithValue("@login", CurrentUser.Username);
                    return cmd.ExecuteScalar()?.ToString();
                }
            }
            catch { return null; }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                StringBuilder builder = new StringBuilder();
                foreach (byte b in hash)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            InactivityManager.UnregisterForm();
            base.OnFormClosed(e);
        }

        // ОБРАБОТЧИК - при нажатии на крестик
        private void ManagerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Проверяем, что закрытие не было вызвано из кода
            if (e.CloseReason == CloseReason.UserClosing)
            {
                // Отменяем закрытие формы
                e.Cancel = true;

                // Скрываем текущую форму
                this.Visible = false;

                // Открываем LoginForm
                LoginForm login = new LoginForm();
                login.Show();
            }
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

        // ОБРАБОТЧИК - при открытии панели отключаем ControlBox
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

            // ОТКЛЮЧАЕМ КРЕСТИК ПРИ ОТКРЫТОЙ ПАНЕЛИ
            this.ControlBox = false;
        }

        // ОБРАБОТЧИК - при закрытии панели включаем ControlBox обратно
        private void btnBackFromPanel_Click(object sender, EventArgs e)
        {
            panel1.Visible = false;

            // ВКЛЮЧАЕМ КРЕСТИК ОБРАТНО
            this.ControlBox = true;
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