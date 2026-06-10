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
    public partial class DirectorForm : Form
    {
        private bool isLockDialogOpen = false;

        public DirectorForm()
        {
            InitializeComponent();

            // Подписываемся на события ТОЛЬКО для кнопок, которые НА ПАНЕЛИ
           

            // Подписываемся на события для кнопок статистики

            // При загрузке формы проверяем, что панель статистики скрыта
            this.Load += DirectorForm_Load;

            // Настройка стилей для кнопок
            SetupButtonStyles();
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
        private void SetupButtonStyles()
        {
            SetupPanelButtonStyle(buttonStatistics);
            SetupPanelButtonStyle(buttonProfit);
            SetupPanelButtonStyle(ButtonReport);
        }

        private void SetupPanelButtonStyle(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.Black;
            btn.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            btn.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            btn.MouseDown += (s, e) => btn.FlatAppearance.BorderColor = Color.DarkBlue;
            btn.MouseUp += (s, e) => btn.FlatAppearance.BorderColor = Color.Black;
            btn.MouseLeave += (s, e) => btn.FlatAppearance.BorderColor = Color.Black;
        }

        private void DirectorForm_Load(object sender, EventArgs e)
        {
            // Находим панель статистики
            Panel statisticsPanel = this.Controls["panelStatistics"] as Panel;
            if (statisticsPanel != null)
            {
                // Изначально панель скрыта
                statisticsPanel.Visible = false;

                // Убеждаемся, что на панели ТОЛЬКО нужные кнопки
                // (buttonCertificates, buttonClientTop, buttonTopDish)
                // ButtonRev там быть НЕ ДОЛЖНО
            }
        }

        // Обработчик нажатия на кнопку Statistics
       

        // Закрытие панели через pictureBox4
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            CloseStatisticsPanel();
        }

        // Закрытие панели через pictureBox3
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            CloseStatisticsPanel();
        }

        private void CloseStatisticsPanel()
        {
            Panel statisticsPanel = this.Controls["panelStatistics"] as Panel;
            if (statisticsPanel != null)
            {
                statisticsPanel.Visible = false;
            }

            // Скрываем кнопки на панели

            // ButtonRev НЕ ТРОГАЕМ
        }

        // Выход
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            LoginForm login = new LoginForm();
            login.Show();
        }

        // Обработчики для кнопок статистики

        private void buttonClientTop_Click(object sender, EventArgs e)
        {
           
        }

        private void buttonTopDish_Click(object sender, EventArgs e)
        {
            TopDishForm topDish = new TopDishForm();
            topDish.Owner = this; // Устанавливаем владельца
            this.Hide(); // Прячем DirectorForm
            topDish.Show();
        }

        private void ButtonReport_Click(object sender, EventArgs e)
        {
            OrdersReportForm ordersReport = new OrdersReportForm();
            ordersReport.Owner = this; // Устанавливаем владельца
            this.Hide(); // Прячем DirectorForm
            ordersReport.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TopDishForm profit = new TopDishForm();
            profit.Owner = this;
            this.Hide();
            profit.Show();
        }

        private void buttonMenu_Click(object sender, EventArgs e)
        {
           
        }

        private void panelStatistics_Paint(object sender, PaintEventArgs e)
        {

        }

        private void buttonStatistics_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            CertificateStatisticsForm certificate = new CertificateStatisticsForm();
            certificate.Show();
        }
    }
}