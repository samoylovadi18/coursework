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
    /// <summary>
    /// Форма главного меню администратора.
    /// Предоставляет доступ к управлению пользователями, блюдами, заказами и справочниками.
    /// </summary>
    public partial class AdminForm : Form
    {
        private bool isLockDialogOpen = false;
        private bool isLoggingOut = false; // Флаг выхода

        /// <summary>
        /// Конструктор формы администратора.
        /// Инициализирует компоненты, настраивает стили кнопок и подписывается на события.
        /// </summary>
        public AdminForm()
        {
            InitializeComponent();

            // Настройка кнопок (ваш существующий код)
            button1.FlatStyle = FlatStyle.Flat;
            button1.FlatAppearance.BorderSize = 1;
            button1.FlatAppearance.BorderColor = Color.Black;
            button1.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            button1.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            button1.MouseDown += (s, e) =>
            {
                button1.FlatAppearance.BorderColor = Color.DarkBlue;
            };

            button1.MouseUp += (s, e) =>
            {
                button1.FlatAppearance.BorderColor = Color.Black;
            };
            button1.MouseLeave += (s, e) =>
            {
                button1.FlatAppearance.BorderColor = Color.Black;
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

            this.FormClosing += AdminForm_FormClosing;

            InactivityManager.RegisterForm(this);
            InactivityManager.OnLockRequest += () => LockSystem();
        }

        /// <summary>
        /// Блокирует систему при длительном бездействии пользователя.
        /// Отображает диалоговое окно для ввода пароля разблокировки.
        /// </summary>
        private void LockSystem()
        {
            if (isLockDialogOpen || isLoggingOut) return;
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
                    {
                        CheckPasswordAndUnlock(txtPassword, lockDialog);
                    }
                };

                lockDialog.Controls.Add(lblMessage);
                lockDialog.Controls.Add(lblUser);
                lockDialog.Controls.Add(txtPassword);
                lockDialog.Controls.Add(btnUnlock);

                lockDialog.FormClosed += (s, e) => { isLockDialogOpen = false; };

                lockDialog.ShowDialog();
            }));
        }

        /// <summary>
        /// Проверяет введённый пароль для разблокировки системы.
        /// </summary>
        /// <param name="txtPassword">Поле ввода пароля.</param>
        /// <param name="lockDialog">Диалоговое окно блокировки.</param>
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

                // Устанавливаем флаг выхода
                isLoggingOut = true;

                // Закрываем диалог
                lockDialog.Close();

                // Отписываемся от менеджера бездействия
                InactivityManager.UnregisterForm();

                // Закрываем текущую форму
                this.Close();

                // Открываем форму входа
                LoginForm login = new LoginForm();
                login.Show();
            }
        }

        /// <summary>
        /// Получает хеш пароля текущего пользователя из базы данных.
        /// </summary>
        /// <returns>Строка с хешем пароля или null в случае ошибки.</returns>
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
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Вычисляет хеш SHA-256 для переданного пароля.
        /// </summary>
        /// <param name="password">Пароль в открытом виде.</param>
        /// <returns>Строка с хешем пароля в шестнадцатеричном формате.</returns>
        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                StringBuilder builder = new StringBuilder();
                foreach (byte b in hash)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Обработчик события закрытия формы.
        /// Отписывается от менеджера бездействия, если не выполняется выход из системы.
        /// </summary>
        /// <param name="e">Аргументы события закрытия формы.</param>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (!isLoggingOut)
            {
                InactivityManager.UnregisterForm();
            }
            base.OnFormClosed(e);
        }

        /// <summary>
        /// Обработчик события закрытия формы.
        /// При закрытии формы пользователем (не системой) скрывает форму и открывает форму входа.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Аргументы события закрытия формы.</param>
        private void AdminForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !isLoggingOut)
            {
                e.Cancel = true;
                this.Visible = false;
                LoginForm login = new LoginForm();
                login.Show();
            }
        }

        private void AdminForm_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Обработчик нажатия кнопки "Пользователи".
        /// Открывает форму управления пользователями.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Аргументы события.</param>
        private void button1_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            UsersForm users = new UsersForm();
            users.Show();
        }

        /// <summary>
        /// Обработчик нажатия кнопки выхода (крестик в правом верхнем углу).
        /// Выполняет выход из системы и открывает форму входа.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Аргументы события.</param>
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            isLoggingOut = true;
            this.Close();
            LoginForm login = new LoginForm();
            login.Show();
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Заказы".
        /// Открывает форму управления заказами.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Аргументы события.</param>
        private void button3_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            OrdersForm Orders = new OrdersForm();
            Orders.Show();
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Меню".
        /// Открывает форму управления меню блюд.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Аргументы события.</param>
        private void button4_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            AdminMenu adminMenu = new AdminMenu();
            adminMenu.Show();
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Справочники".
        /// Открывает форму управления справочниками.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Аргументы события.</param>
        private void button2_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            Spravochnici Spravochnic = new Spravochnici();
            Spravochnic.Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {

        }
    }
}