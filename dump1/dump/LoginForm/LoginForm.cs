using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dump
{
    public partial class LoginForm : Form
    {
        private bool isPasswordVisible = false;
        private string allowedSpecialChars = "!@#$%^&*()_-+=[]{}|;:'\",.<>?/`~";

        // Поля для CAPTCHA
        private int failedAttempts = 0;              // Счетчик неудачных попыток
        private DateTime blockUntil = DateTime.MinValue; // Время до блокировки
        private bool captchaRequired = false;        // Флаг необходимости капчи

        // Таймер для обратного отсчета на кнопке
        private Timer blockTimer = new Timer();
        private int remainingSeconds = 0;

        private void SetupUnderlineTextBox(TextBox textBox)
        {
            Panel underline = new Panel();
            underline.Height = 1;
            underline.Width = textBox.Width;
            underline.Location = new Point(textBox.Location.X, textBox.Location.Y + textBox.Height);
            underline.BackColor = Color.Gray;
            underline.Name = textBox.Name + "_Underline";

            this.Controls.Add(underline);
            underline.BringToFront();

            textBox.Enter += (s, e) => { underline.BackColor = Color.DarkSeaGreen; };
            textBox.Leave += (s, e) => { underline.BackColor = Color.Gray; };

            textBox.LocationChanged += (s, e) =>
            {
                underline.Location = new Point(textBox.Location.X, textBox.Location.Y + textBox.Height);
            };

            textBox.SizeChanged += (s, e) =>
            {
                underline.Width = textBox.Width;
                underline.Location = new Point(textBox.Location.X, textBox.Location.Y + textBox.Height);
            };
        }

        public LoginForm()
        {
            InitializeComponent();

            SetMaxLengthLimits();
            SetupInputValidation();

            isPasswordVisible = false;
            Password.UseSystemPasswordChar = true;

            try
            {
                visible_password.Image = Image.FromFile("zac.png");
            }
            catch
            {
                visible_password.Image = CreateSimpleEyeIcon(false);
            }

            visible_password.Font = new Font("Segoe UI Emoji", 13f);

            Enter.FlatStyle = FlatStyle.Flat;
            Enter.FlatAppearance.BorderSize = 1;
            Enter.FlatAppearance.BorderColor = Color.Black;
            Enter.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            Enter.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            Enter.MouseDown += (s, e) =>
            {
                Enter.FlatAppearance.BorderColor = Color.DarkBlue;
            };

            Enter.MouseUp += (s, e) =>
            {
                Enter.FlatAppearance.BorderColor = Color.Black;
            };

            Enter.MouseLeave += (s, e) =>
            {
                Enter.FlatAppearance.BorderColor = Color.Black;
            };

            Enter.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            Enter.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            SetupUnderlineTextBox(Login);
            SetupUnderlineTextBox(Password);

            // Настройка таймера для блокировки кнопки
            blockTimer.Interval = 1000; // 1 секунда
            blockTimer.Tick += BlockTimer_Tick;
        }

        private void BlockTimer_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now >= blockUntil)
            {
                // Блокировка закончилась
                blockTimer.Stop();
                Enter.Enabled = true;
                Enter.Text = "ВОЙТИ";
                remainingSeconds = 0;

                // Сбрасываем счетчик неудачных попыток после блокировки
                failedAttempts = 0;
            }
            else
            {
                // Обновляем текст на кнопке
                remainingSeconds = (int)(blockUntil - DateTime.Now).TotalSeconds;
                Enter.Text = $"Заблокировано на {remainingSeconds} секунд";
                Enter.Enabled = false;
            }
        }

        private Image CreateSimpleEyeIcon(bool open)
        {
            Bitmap bmp = new Bitmap(32, 32);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                using (Pen pen = new Pen(Color.Black, 2))
                {
                    if (open)
                    {
                        g.DrawEllipse(pen, 8, 8, 16, 12);
                        g.FillEllipse(Brushes.Black, 14, 12, 4, 4);
                    }
                    else
                    {
                        g.DrawLine(pen, 8, 16, 24, 16);
                        g.DrawLine(pen, 8, 14, 24, 18);
                        g.DrawLine(pen, 8, 18, 24, 14);
                    }
                }
            }
            return bmp;
        }

        private void SetMaxLengthLimits()
        {
            Login.MaxLength = 50;
            Password.MaxLength = 50;
        }

        private void SetupInputValidation()
        {
            Login.KeyPress += TextBox_KeyPress;
            Login.TextChanged += Login_TextChanged;

            Password.KeyPress += TextBox_KeyPress;
            Password.TextChanged += Password_TextChanged;

            Login.ShortcutsEnabled = true;
            Password.ShortcutsEnabled = true;

            Login.ContextMenuStrip = new ContextMenuStrip();
            Password.ContextMenuStrip = new ContextMenuStrip();
        }

        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            if (!IsValidCharacter(e.KeyChar))
            {
                e.Handled = true;
                System.Media.SystemSounds.Beep.Play();
            }
        }

        private bool IsValidCharacter(char c)
        {
            if ((c >= 'а' && c <= 'я') || (c >= 'А' && c <= 'Я'))
            {
                return false;
            }

            if (c == ' ')
            {
                return false;
            }

            if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
            {
                return true;
            }

            if (c >= '0' && c <= '9')
            {
                return true;
            }

            if (allowedSpecialChars.Contains(c))
            {
                return true;
            }

            return false;
        }

        private void Login_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string originalText = textBox.Text;

            StringBuilder filteredText = new StringBuilder();
            foreach (char c in originalText)
            {
                if (IsValidCharacter(c) || char.IsControl(c))
                {
                    filteredText.Append(c);
                }
            }

            if (filteredText.ToString() != originalText)
            {
                int selectionStart = textBox.SelectionStart;
                int selectionLength = textBox.SelectionLength;

                textBox.Text = filteredText.ToString();
                textBox.SelectionStart = Math.Max(0, selectionStart - (originalText.Length - filteredText.Length));
                textBox.SelectionLength = selectionLength;
            }

            if (textBox.Text.Length > textBox.MaxLength)
            {
                textBox.Text = textBox.Text.Substring(0, textBox.MaxLength);
                textBox.SelectionStart = textBox.MaxLength;
            }
        }

        private void Password_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string originalText = textBox.Text;

            StringBuilder filteredText = new StringBuilder();
            foreach (char c in originalText)
            {
                if (IsValidCharacter(c) || char.IsControl(c))
                {
                    filteredText.Append(c);
                }
            }

            if (filteredText.ToString() != originalText)
            {
                int selectionStart = textBox.SelectionStart;
                int selectionLength = textBox.SelectionLength;

                textBox.Text = filteredText.ToString();
                textBox.SelectionStart = Math.Max(0, selectionStart - (originalText.Length - filteredText.Length));
                textBox.SelectionLength = selectionLength;
            }

            if (textBox.Text.Length > textBox.MaxLength)
            {
                textBox.Text = textBox.Text.Substring(0, textBox.MaxLength);
                textBox.SelectionStart = textBox.MaxLength;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnLog_Click(object sender, EventArgs e)
        {
            // Проверка блокировки (дополнительная защита)
            if (DateTime.Now < blockUntil)
            {
                int secondsLeft = (int)(blockUntil - DateTime.Now).TotalSeconds;
                MessageBox.Show($"Слишком много неудачных попыток!\nПодождите {secondsLeft} секунд.",
                    "Блокировка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string login = Login.Text.Trim();
            string password = Password.Text;

            if (!ValidateLoginInput(login, password))
            {
                return;
            }

            try
            {
                using (var connection = SettingsBD.GetConnection())
                {
                    connection.Open();
                    string query = @"SELECT u.id_user, u.FIO, u.login, u.id_role, r.role_name, u.password_hash 
                             FROM users u 
                             LEFT JOIN roles r ON u.id_role = r.id_role 
                             WHERE u.login = @login";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@login", login);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedHash = reader.GetString("password_hash");
                                string inputHash = HashPassword(password);

                                if (inputHash.Equals(storedHash, StringComparison.OrdinalIgnoreCase))
                                {
                                    // УСПЕШНЫЙ ВХОД
                                    failedAttempts = 0;
                                    captchaRequired = false;

                                    int userId = reader.GetInt32("id_user");
                                    string fio = reader.GetString("FIO");
                                    int roleId = reader.GetInt32("id_role");
                                    string roleName = reader.GetString("role_name");

                                    CurrentUser.Initialize(userId, login, fio, roleId, roleName);
                                    OpenFormByRole(roleId);
                                    this.Hide();
                                    return;
                                }
                            }

                            // НЕУДАЧНАЯ АВТОРИЗАЦИЯ
                            failedAttempts++;

                            // Первая неудачная попытка - просто сообщение
                            if (failedAttempts == 1)
                            {
                                MessageBox.Show("Неверный логин или пароль", "Ошибка авторизации",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            // Вторая попытка - показываем капчу
                            else if (failedAttempts == 2)
                            {
                                MessageBox.Show("Неверный логин или пароль.\nТребуется подтверждение безопасности.",
                                    "Ошибка авторизации", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                                // Скрываем форму логина
                                this.Hide();

                                // Показываем капчу
                                using (var captchaForm = new CaptchaForm())
                                {
                                    var result = captchaForm.ShowDialog();

                                    if (result == DialogResult.OK && captchaForm.IsVerified)
                                    {
                                        // Капча пройдена - показываем форму логина
                                        this.Show();
                                        failedAttempts = 1; // Сбрасываем до 1
                                        Login.Clear();
                                        Password.Clear();
                                        Login.Focus();
                                    }
                                    else
                                    {
                                        // Капча НЕ пройдена - БЛОКИРОВКА НА 10 СЕКУНД
                                        blockUntil = DateTime.Now.AddSeconds(10);

                                        // Запускаем таймер для обновления кнопки
                                        blockTimer.Start();

                                        // Обновляем кнопку сразу
                                        Enter.Enabled = false;
                                        Enter.Text = $"Заблокировано на 10 секунд";

                                        // Показываем форму логина
                                        this.Show();

                                        MessageBox.Show("Неверный ввод CAPTCHA!\nВход заблокирован на 10 секунд.",
                                            "Блокировка", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                                        // Очищаем поля
                                        Login.Clear();
                                        Password.Clear();
                                        Login.Focus();
                                    }
                                }
                            }
                            // Третья и последующие попытки - блокировка сразу
                            else
                            {
                                blockUntil = DateTime.Now.AddSeconds(10);

                                // Запускаем таймер для обновления кнопки
                                blockTimer.Start();

                                // Обновляем кнопку сразу
                                Enter.Enabled = false;
                                Enter.Text = $"Заблокировано на 10 секунд";

                                MessageBox.Show("Слишком много неудачных попыток!\nВход заблокирован на 10 секунд.",
                                    "Блокировка", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                                // Очищаем поля
                                Login.Clear();
                                Password.Clear();
                                Login.Focus();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при подключении к базе данных\n{ex.Message}",
                "Ошибка подключения",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateLoginInput(string login, string password)
        {
            if (string.IsNullOrEmpty(login))
            {
                MessageBox.Show("Введите логин", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Login.Focus();
                return false;
            }

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите пароль", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Password.Focus();
                return false;
            }

            if (login.Length < 3)
            {
                MessageBox.Show("Логин должен содержать не менее 3 символов", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Login.Focus();
                Login.SelectAll();
                return false;
            }

            if (login.Length > 50)
            {
                MessageBox.Show("Логин не должен превышать 50 символов", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Login.Focus();
                Login.SelectAll();
                return false;
            }

            if (password.Length < 3)
            {
                MessageBox.Show("Пароль должен содержать не менее 3 символов", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Password.Focus();
                Password.SelectAll();
                return false;
            }

            if (password.Length > 50)
            {
                MessageBox.Show("Пароль не должен превышать 50 символов", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Password.Focus();
                Password.SelectAll();
                return false;
            }

            if (ContainsInvalidCharacters(login) || ContainsInvalidCharacters(password))
            {
                MessageBox.Show("Логин и пароль могут содержать только:\n" +
                              "• Английские буквы (A-Z, a-z)\n" +
                              "• Цифры (0-9)\n" +
                              "• Специальные символы: " + allowedSpecialChars + "\n" +
                              "Запрещены: русские буквы и пробелы",
                              "Недопустимые символы",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private bool ContainsInvalidCharacters(string text)
        {
            foreach (char c in text)
            {
                if (!IsValidCharacter(c))
                {
                    return true;
                }
            }
            return false;
        }

        private void OpenFormByRole(int roleId)
        {
            switch (roleId)
            {
                case 1:
                    ManagerForm managerForm = new ManagerForm();
                    managerForm.Show();
                    break;

                case 2:
                    DirectorForm directorForm = new DirectorForm();
                    directorForm.Show();
                    break;

                case 3:
                    AdminForm adminForm = new AdminForm();
                    adminForm.Show();
                    break;

                default:
                    MessageBox.Show("Неизвестная роль пользователя", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);

                System.Text.StringBuilder builder = new System.Text.StringBuilder();
                foreach (byte b in hash)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private void lblPassword_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            isPasswordVisible = !isPasswordVisible;

            try
            {
                if (isPasswordVisible)
                {
                    Password.UseSystemPasswordChar = false;
                    visible_password.Image = Image.FromFile("otc.png");
                }
                else
                {
                    Password.UseSystemPasswordChar = true;
                    visible_password.Image = Image.FromFile("zac.png");
                }
            }
            catch
            {
                if (isPasswordVisible)
                {
                    Password.UseSystemPasswordChar = false;
                    visible_password.Image = CreateSimpleEyeIcon(true);
                }
                else
                {
                    Password.UseSystemPasswordChar = true;
                    visible_password.Image = CreateSimpleEyeIcon(false);
                }
            }

            Password.Focus();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Вы действительно хотите закрыть приложение?", "Подтверждение закрытия",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            SettingsForm s = new SettingsForm();
            s.ShowDialog();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                btnLog_Click(null, null);
                return true;
            }

            if (keyData == Keys.Escape)
            {
                Application.Exit();
                return true;
            }

            if (keyData == (Keys.Control | Keys.A))
            {
                if (Login.Focused)
                {
                    Login.SelectAll();
                    return true;
                }
                if (Password.Focused)
                {
                    Password.SelectAll();
                    return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}