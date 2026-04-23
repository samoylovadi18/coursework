using MySql.Data.MySqlClient;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace dump
{
    public partial class SettingsForm : Form
    {
        private bool isPasswordVisible = false;

        public SettingsForm()
        {
            InitializeComponent();
            LoadCurrentSettings();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            // Скрываем лейбл статуса при загрузке формы
            lblStatus.Visible = false;

            // Настраиваем кнопку показа/скрытия пароля
            SetupPasswordToggleButton();
        }

        private void SetupPasswordToggleButton()
        {
            // Настройка кнопки видимости пароля
            visible_password.FlatStyle = FlatStyle.Flat;
            visible_password.FlatAppearance.BorderSize = 0;
            visible_password.BackColor = Color.Transparent;
            visible_password.Cursor = Cursors.Hand;

            // Загружаем иконку "закрытый глаз" по умолчанию
            try
            {
                visible_password.Image = Image.FromFile("zac.png");
            }
            catch
            {
                visible_password.Image = CreateSimpleEyeIcon(false);
            }   

            visible_password.ImageAlign = ContentAlignment.MiddleCenter;

            // Подписываемся на событие клика
            visible_password.Click += Visible_password_settings_Click;
        }

        private void Visible_password_settings_Click(object sender, EventArgs e)
        {
            isPasswordVisible = !isPasswordVisible;

            try
            {
                if (isPasswordVisible)
                {
                    txtPassword.UseSystemPasswordChar = false;
                    visible_password.Image = Image.FromFile("otc.png");
                }
                else
                {
                    txtPassword.UseSystemPasswordChar = true;
                    visible_password.Image = Image.FromFile("zac.png");
                }
            }
            catch
            {
                if (isPasswordVisible)
                {
                    txtPassword.UseSystemPasswordChar = false;
                    visible_password.Image = CreateSimpleEyeIcon(true);
                }
                else
                {
                    txtPassword.UseSystemPasswordChar = true;
                    visible_password.Image = CreateSimpleEyeIcon(false);
                }
            }

            txtPassword.Focus();
        }

        private Image CreateSimpleEyeIcon(bool open)
        {
            Bitmap bmp = new Bitmap(24, 24);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                using (Pen pen = new Pen(Color.Gray, 2))
                {
                    if (open)
                    {
                        // Открытый глаз
                        g.DrawEllipse(pen, 4, 6, 16, 12);
                        g.FillEllipse(Brushes.Gray, 10, 10, 4, 4);
                    }
                    else
                    {
                        // Закрытый глаз (перечеркнутый)
                        g.DrawLine(pen, 4, 6, 20, 18);
                        g.DrawLine(pen, 4, 12, 20, 12);
                        g.DrawLine(pen, 4, 18, 20, 6);
                    }
                }
            }
            return bmp;
        }

        private void LoadCurrentSettings()
        {
            var config = SettingsBD.GetCurrentConfig();
            txtServer.Text = config.Server;
            txtDatabase.Text = config.Database;
            txtUsername.Text = config.Username;
            txtPassword.Text = config.Password;
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            // Дополнительная гарантия, что лейбл скрыт при загрузке
            lblStatus.Visible = false;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(txtServer.Text))
            {
                MessageBox.Show("Введите адрес сервера!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtServer.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDatabase.Text))
            {
                MessageBox.Show("Введите название базы данных!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDatabase.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Введите имя пользователя!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            // Сохраняем настройки
            var newConfig = new SettingsBD.ConnectionConfig
            {
                Server = txtServer.Text.Trim(),
                Database = txtDatabase.Text.Trim(),
                Username = txtUsername.Text.Trim(),
                Password = txtPassword.Text
            };

            try
            {
                // Проверяем подключение перед сохранением
                if (!SettingsBD.TestConnection(newConfig.GetConnectionString()))
                {
                    DialogResult result = MessageBox.Show(
                        "Не удалось подключиться к базе данных с указанными настройками.\n" +
                        "Сохранить настройки всё равно?",
                        "Предупреждение",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result != DialogResult.Yes)
                        return;
                }

                SettingsBD.UpdateConfig(newConfig);

                MessageBox.Show("Настройки успешно сохранены!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Visible = false;

                LoginForm login = new LoginForm();
                login.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении настроек:\n{ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            // Показываем лейбл статуса при нажатии на кнопку
            lblStatus.Visible = true;

            if (string.IsNullOrWhiteSpace(txtServer.Text))
            {
                lblStatus.Text = "❌ Введите сервер!";
                lblStatus.ForeColor = Color.Red;
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDatabase.Text))
            {
                lblStatus.Text = "❌ Введите название базы данных!";
                lblStatus.ForeColor = Color.Red;
                return;
            }

            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                lblStatus.Text = "❌ Введите имя пользователя!";
                lblStatus.ForeColor = Color.Red;
                return;
            }

            string connectionString = $"server={txtServer.Text};username={txtUsername.Text};password={txtPassword.Text};database={txtDatabase.Text};";

            lblStatus.Text = "⏳ Проверка подключения...";
            lblStatus.ForeColor = Color.Blue;
            btnTestConnection.Enabled = false;
            this.Cursor = Cursors.WaitCursor;

            // Проверка в отдельном потоке, чтобы не блокировать UI
            System.Threading.Tasks.Task.Run(() =>
            {
                bool isConnected = false;
                string errorMessage = "";

                try
                {
                    using (var connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        isConnected = true;
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                }

                this.Invoke(new Action(() =>
                {
                    btnTestConnection.Enabled = true;
                    this.Cursor = Cursors.Default;

                    if (isConnected)
                    {
                        lblStatus.Text = "✅ Подключение успешно!";
                        lblStatus.ForeColor = Color.Green;
                        MessageBox.Show("Подключение к базе данных успешно установлено!",
                            "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        lblStatus.Text = "❌ Ошибка подключения!";
                        lblStatus.ForeColor = Color.Red;
                        MessageBox.Show($"Не удалось подключиться к базе данных:\n{errorMessage}",
                            "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }));
            });
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Visible = false;

            LoginForm login = new LoginForm();
            login.ShowDialog();
        }
    }
}