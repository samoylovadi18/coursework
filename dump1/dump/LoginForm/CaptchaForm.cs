using MySql.Data.MySqlClient;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace dump
{
    public partial class CaptchaForm : Form
    {
        private string currentCaptcha = "";
        private Random random = new Random();
        private bool isLockDialogOpen = false;

        /// <summary>
        /// Результат проверки капчи
        /// </summary>
        public bool IsVerified { get; private set; } = false;

        public CaptchaForm()
        {
            InitializeComponent();
            SetupForm();
            GenerateCaptcha();
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

        private void SetupForm()
        {
            SetupButtonStyle(btnVerify);

            // Подписка на события
            btnRefresh.Click += BtnRefresh_Click;
            btnVerify.Click += BtnVerify_Click;
            txtCaptcha.KeyPress += TxtCaptcha_KeyPress;

            // Настройка поля ввода
            txtCaptcha.MaxLength = 4;

            // Настройка кнопки закрытия (крестик)
            this.FormClosing += CaptchaForm_FormClosing;

            // Добавляем подсказку о регистрозависимости
            Label lblCaseSensitive = new Label();
            lblCaseSensitive.Text = "Регистр имеет значение!";
            lblCaseSensitive.ForeColor = Color.Red;
            lblCaseSensitive.Font = new Font("Microsoft Sans Serif", 8, FontStyle.Bold);
            lblCaseSensitive.Location = new Point(190, 160);
            lblCaseSensitive.Size = new Size(150, 20);
            this.Controls.Add(lblCaseSensitive);
        }

        // Обработчик закрытия формы через крестик
        private void CaptchaForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Если капча не была подтверждена - считаем, что пользователь закрыл окно
            if (!IsVerified)
            {
                IsVerified = false;
                this.DialogResult = DialogResult.Cancel;
            }
        }

        private void SetupButtonStyle(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.Black;

            btn.MouseDown += (s, e) => btn.FlatAppearance.BorderColor = Color.DarkBlue;
            btn.MouseUp += (s, e) => btn.FlatAppearance.BorderColor = Color.Black;
            btn.MouseLeave += (s, e) => btn.FlatAppearance.BorderColor = Color.Black;
        }

        /// <summary>
        /// Генерация изображения CAPTCHA
        /// </summary>
        private void GenerateCaptcha()
        {
            // Набор символов (без путающихся: O, 0, I, 1, L)
            // Добавляем как заглавные, так и строчные буквы для регистрозависимости
            string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjklmnpqrstuvwxyz23456789";

            // Генерируем 4 случайных символа
            currentCaptcha = "";
            for (int i = 0; i < 4; i++)
            {
                currentCaptcha += chars[random.Next(chars.Length)];
            }

            // Создаем изображение
            Bitmap bmp = new Bitmap(picCaptcha.Width, picCaptcha.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);

                // Добавляем случайные линии (перечеркивание)
                for (int i = 0; i < 5; i++)
                {
                    using (Pen pen = new Pen(Color.FromArgb(random.Next(100, 200),
                                                           random.Next(100, 200),
                                                           random.Next(100, 200)), 2))
                    {
                        int x1 = random.Next(0, picCaptcha.Width);
                        int y1 = random.Next(0, picCaptcha.Height);
                        int x2 = random.Next(0, picCaptcha.Width);
                        int y2 = random.Next(0, picCaptcha.Height);
                        g.DrawLine(pen, x1, y1, x2, y2);
                    }
                }

                // Добавляем случайные точки (шум)
                for (int i = 0; i < 200; i++)
                {
                    int x = random.Next(0, picCaptcha.Width);
                    int y = random.Next(0, picCaptcha.Height);
                    bmp.SetPixel(x, y, Color.FromArgb(random.Next(150, 255),
                                                      random.Next(150, 255),
                                                      random.Next(150, 255)));
                }

                // Рисуем символы с наложением и искажением
                int[] xPos = { 30, 90, 150, 210 };
                int[] yPos = new int[4];
                float[] angles = new float[4];

                for (int i = 0; i < 4; i++)
                {
                    // Случайное смещение по Y (не на одной линии)
                    yPos[i] = 20 + random.Next(-10, 20);

                    // Случайный угол наклона
                    angles[i] = random.Next(-15, 15);

                    // Случайный размер шрифта
                    float fontSize = 30 + random.Next(-5, 10);

                    using (Font font = new Font("Arial", fontSize, FontStyle.Bold))
                    {
                        // Случайный цвет для каждого символа
                        Color charColor = Color.FromArgb(random.Next(50, 200),
                                                         random.Next(50, 200),
                                                         random.Next(50, 200));

                        using (Brush brush = new SolidBrush(charColor))
                        {
                            // Поворачиваем символ
                            g.TranslateTransform(xPos[i], yPos[i]);
                            g.RotateTransform(angles[i]);

                            // Рисуем символ
                            g.DrawString(currentCaptcha[i].ToString(), font, brush, 0, 0);

                            // Возвращаем трансформацию
                            g.ResetTransform();
                        }
                    }
                }

                // Добавляем еще несколько линий поверх символов
                for (int i = 0; i < 3; i++)
                {
                    using (Pen pen = new Pen(Color.FromArgb(random.Next(50, 150),
                                                           random.Next(50, 150),
                                                           random.Next(50, 150)), 1))
                    {
                        int x1 = random.Next(0, picCaptcha.Width);
                        int y1 = random.Next(0, picCaptcha.Height);
                        int x2 = random.Next(0, picCaptcha.Width);
                        int y2 = random.Next(0, picCaptcha.Height);
                        g.DrawLine(pen, x1, y1, x2, y2);
                    }
                }
            }

            picCaptcha.Image = bmp;
            txtCaptcha.Clear();
            txtCaptcha.Focus();
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            GenerateCaptcha();
        }

        private void BtnVerify_Click(object sender, EventArgs e)
        {
            // НЕ ПРИМЕНЯЕМ ToUpper() - сохраняем регистр
            string input = txtCaptcha.Text.Trim();

            if (string.IsNullOrEmpty(input))
            {
                MessageBox.Show("Введите символы с картинки!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCaptcha.Focus();
                return;
            }

            if (input.Length != 4)
            {
                MessageBox.Show("Введите ровно 4 символа!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCaptcha.Focus();
                txtCaptcha.SelectAll();
                return;
            }

            // Сравниваем с учетом регистра (по умолчанию StringComparison.Ordinal - регистрозависимое)
            if (input == currentCaptcha)
            {
                IsVerified = true;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный код CAPTCHA!\nОбратите внимание на регистр букв!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                // При неправильной капче - закрываем форму и возвращаем Cancel
                IsVerified = false;
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void TxtCaptcha_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем только буквы и цифры
            if (!char.IsControl(e.KeyChar) && !char.IsLetterOrDigit(e.KeyChar))
            {
                e.Handled = true;
            }

            // Enter - проверка
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                btnVerify.PerformClick();
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            IsVerified = false;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void CaptchaForm_Load(object sender, EventArgs e)
        {

        }

        private void picCaptcha_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
    }
}