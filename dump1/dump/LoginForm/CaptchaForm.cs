using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace dump
{
    public partial class CaptchaForm : Form
    {
        private string currentCaptcha = "";
        private Random random = new Random();

        /// <summary>
        /// Результат проверки капчи
        /// </summary>
        public bool IsVerified { get; private set; } = false;

        public CaptchaForm()
        {
            InitializeComponent();
            SetupForm();
            GenerateCaptcha();
        }

        private void SetupForm()
        {
            // Настройка стилей кнопок
            SetupButtonStyle(btnRefresh);
            SetupButtonStyle(btnVerify);

            // Подписка на события
            btnRefresh.Click += BtnRefresh_Click;
            btnVerify.Click += BtnVerify_Click;
            txtCaptcha.KeyPress += TxtCaptcha_KeyPress;

            // Настройка поля ввода
            txtCaptcha.MaxLength = 4;
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
            string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

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
            string input = txtCaptcha.Text.Trim().ToUpper();

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

            if (input == currentCaptcha)
            {
                IsVerified = true;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный код!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                GenerateCaptcha();
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