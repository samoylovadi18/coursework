using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace dump
{
    public partial class SisAdminForm : Form
    {
        private bool isPasswordVisible = false;

        public SisAdminForm()
        {
            InitializeComponent();

            this.FormClosing += SisAdminForm_FormClosing;

            tabControlBD.SelectedIndexChanged += TabControlBD_SelectedIndexChanged;
            LoadCurrentSettings();
            InitializeCustomComponents();

            this.Shown += SisAdminForm_Shown;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 1;
            btnSave.FlatAppearance.BorderColor = Color.Black;
            btnSave.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            btnSave.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            btnSave.MouseDown += (s, e) => btnSave.FlatAppearance.BorderColor = Color.DarkBlue;
            btnSave.MouseUp += (s, e) => btnSave.FlatAppearance.BorderColor = Color.Black;
            btnSave.MouseLeave += (s, e) => btnSave.FlatAppearance.BorderColor = Color.Black;

            btnTestConnection.FlatStyle = FlatStyle.Flat;
            btnTestConnection.FlatAppearance.BorderSize = 1;
            btnTestConnection.FlatAppearance.BorderColor = Color.Black;
            btnTestConnection.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            btnTestConnection.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            btnTestConnection.MouseDown += (s, e) => btnTestConnection.FlatAppearance.BorderColor = Color.DarkBlue;
            btnTestConnection.MouseUp += (s, e) => btnTestConnection.FlatAppearance.BorderColor = Color.Black;
            btnTestConnection.MouseLeave += (s, e) => btnTestConnection.FlatAppearance.BorderColor = Color.Black;
        }

        private void SisAdminForm_Shown(object sender, EventArgs e)
        {
            // При загрузке формы проверяем, какая вкладка активна
            UpdateSettingsElementsVisibility();
        }

        private void InitializeCustomComponents()
        {
            // Скрываем лейбл статуса при загрузке формы
            if (lblStatus != null) lblStatus.Visible = false;

            // Гарантируем, что пароль скрыт по умолчанию
            if (txtPassword != null)
            {
                txtPassword.UseSystemPasswordChar = true;
            }

            // Сбрасываем состояние видимости пароля
            isPasswordVisible = false;

            // Настраиваем кнопку показа/скрытия пароля
            SetupPasswordToggleButton();
        }

        private void SetupPasswordToggleButton()
        {
            if (visible_password == null) return;

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
                        g.DrawEllipse(pen, 4, 6, 16, 12);
                        g.FillEllipse(Brushes.Gray, 10, 10, 4, 4);
                    }
                    else
                    {
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
            if (txtServer != null) txtServer.Text = config.Server;
            if (txtDatabase != null) txtDatabase.Text = config.Database;
            if (txtUsername != null) txtUsername.Text = config.Username;
            if (txtPassword != null)
            {
                txtPassword.Text = config.Password;
                txtPassword.UseSystemPasswordChar = true; // Пароль скрыт по умолчанию
            }
        }

        // Обновление видимости элементов в зависимости от выбранной вкладки
        private void UpdateSettingsElementsVisibility()
        {
            bool isSettingsTab = (tabControlBD.SelectedIndex == 0);

            if (txtServer != null) txtServer.Visible = isSettingsTab;
            if (txtDatabase != null) txtDatabase.Visible = isSettingsTab;
            if (txtUsername != null) txtUsername.Visible = isSettingsTab;
            if (txtPassword != null) txtPassword.Visible = isSettingsTab;
            if (btnSave != null) btnSave.Visible = isSettingsTab;
            if (btnTestConnection != null) btnTestConnection.Visible = isSettingsTab;
            if (visible_password != null) visible_password.Visible = isSettingsTab;
            if (lblStatus != null) lblStatus.Visible = false; // Статус всегда скрыт, пока не нажмут кнопку
        }

        // ОБРАБОТЧИК - при нажатии на крестик
        private void SisAdminForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Visible = false;
                LoginForm login = new LoginForm();
                login.Show();
            }
        }

        // ОБРАБОТЧИК - при переключении на вкладку
        private void TabControlBD_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSettingsElementsVisibility();
            if (tabControlBD.SelectedIndex == 0) // Если это вкладка с настройками, обновляем данные
            {
                LoadCurrentSettings();
                // Убеждаемся, что пароль скрыт при переключении на вкладку настроек
                if (txtPassword != null)
                {
                    txtPassword.UseSystemPasswordChar = true;
                    isPasswordVisible = false;
                }
            }
        }

        private void tabPage1_Click(object sender, EventArgs e) { }
        private void tabPage2_Click(object sender, EventArgs e) { }
        private void SisAdminForm_Load(object sender, EventArgs e) { }

        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            if (lblStatus != null) lblStatus.Visible = true;

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

        private void btnSave_Click(object sender, EventArgs e)
        {
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

            var newConfig = new SettingsBD.ConnectionConfig
            {
                Server = txtServer.Text.Trim(),
                Database = txtDatabase.Text.Trim(),
                Username = txtUsername.Text.Trim(),
                Password = txtPassword.Text
            };

            try
            {
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении настроек:\n{ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}