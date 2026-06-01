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
using System.IO;

namespace dump
{
    public partial class SisAdminForm : Form
    {
        private bool isPasswordVisible = false;
        private bool isLockDialogOpen = false;
        public SisAdminForm()
        {
            InitializeComponent();

            // Регистрируем форму в глобальном менеджере бездействия
            InactivityManager.RegisterForm(this);
            InactivityManager.OnLockRequest += InactivityManager_OnLockRequest;

            this.FormClosing += SisAdminForm_FormClosing;
            tabControl.SelectedIndexChanged += TabControlBD_SelectedIndexChanged;
            LoadCurrentSettings();
            InitializeCustomComponents();
            InitSecurityTab();

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

        private void InactivityManager_OnLockRequest()
        {
            LockSystem();
        }

        // Инициализация вкладки безопасности
        // Инициализация вкладки безопасности
        private void InitSecurityTab()
        {
            numInactivityTime.Minimum = 0;
            numInactivityTime.Maximum = 3600;

            LoadSecuritySettings();



            btnSaveSecurity.FlatStyle = FlatStyle.Flat;
            btnSaveSecurity.FlatAppearance.BorderSize = 1;
            btnSaveSecurity.FlatAppearance.BorderColor = Color.Black;
            btnSaveSecurity.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            btnSaveSecurity.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            btnSaveSecurity.MouseDown += (s, e) => btnSaveSecurity.FlatAppearance.BorderColor = Color.DarkBlue;
            btnSaveSecurity.MouseUp += (s, e) => btnSaveSecurity.FlatAppearance.BorderColor = Color.Black;
            btnSaveSecurity.MouseLeave += (s, e) => btnSaveSecurity.FlatAppearance.BorderColor = Color.Black;



            btnCancelSecurity.FlatStyle = FlatStyle.Flat;
            btnCancelSecurity.FlatAppearance.BorderSize = 1;
            btnCancelSecurity.FlatAppearance.BorderColor = Color.Black;
            btnCancelSecurity.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            btnCancelSecurity.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            btnCancelSecurity.MouseDown += (s, e) => btnCancelSecurity.FlatAppearance.BorderColor = Color.DarkBlue;
            btnCancelSecurity.MouseUp += (s, e) => btnCancelSecurity.FlatAppearance.BorderColor = Color.Black;
            btnCancelSecurity.MouseLeave += (s, e) => btnCancelSecurity.FlatAppearance.BorderColor = Color.Black;

            btnSaveSecurity.Click += BtnSaveSecurity_Click;
            btnCancelSecurity.Click += BtnCancelSecurity_Click;
        }
        private void LoadSecuritySettings()
        {
            numInactivityTime.Value = InactivityManager.GetInactivityTime();
            chkAutoLock.Checked = InactivityManager.GetAutoLockEnabled();
        }

        private void BtnSaveSecurity_Click(object sender, EventArgs e)
        {
            if (chkAutoLock.Checked && numInactivityTime.Value == 0)
            {
                MessageBox.Show("Установите время бездействия больше 0!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            InactivityManager.SaveSecuritySettings((int)numInactivityTime.Value, chkAutoLock.Checked);
            MessageBox.Show("Настройки безопасности сохранены!", "Успех",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnCancelSecurity_Click(object sender, EventArgs e)
        {
            LoadSecuritySettings();
            MessageBox.Show("Изменения отменены", "Информация",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void SisAdminForm_Shown(object sender, EventArgs e)
        {
            UpdateSettingsElementsVisibility();
        }

        private void InitializeCustomComponents()
        {
            if (lblStatus != null) lblStatus.Visible = false;
            if (txtPassword != null) txtPassword.UseSystemPasswordChar = true;
            isPasswordVisible = false;
            SetupPasswordToggleButton();
        }

        private void SetupPasswordToggleButton()
        {
            if (visible_password == null) return;

            visible_password.FlatStyle = FlatStyle.Flat;
            visible_password.FlatAppearance.BorderSize = 0;
            visible_password.BackColor = Color.Transparent;
            visible_password.Cursor = Cursors.Hand;

            try
            {
                visible_password.Image = Image.FromFile("zac.png");
            }
            catch
            {
                visible_password.Image = CreateSimpleEyeIcon(false);
            }

            visible_password.ImageAlign = ContentAlignment.MiddleCenter;
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
                txtPassword.UseSystemPasswordChar = true;
            }
        }

        private void UpdateSettingsElementsVisibility()
        {
            bool isSettingsTab = (tabControl.SelectedIndex == 0);

            if (txtServer != null) txtServer.Visible = isSettingsTab;
            if (txtDatabase != null) txtDatabase.Visible = isSettingsTab;
            if (txtUsername != null) txtUsername.Visible = isSettingsTab;
            if (txtPassword != null) txtPassword.Visible = isSettingsTab;
            if (btnSave != null) btnSave.Visible = isSettingsTab;
            if (btnTestConnection != null) btnTestConnection.Visible = isSettingsTab;
            if (visible_password != null) visible_password.Visible = isSettingsTab;
            if (lblStatus != null) lblStatus.Visible = false;
        }

        private void SisAdminForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            InactivityManager.UnregisterForm();

            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Visible = false;
                LoginForm login = new LoginForm();
                login.Show();
            }
        }

        private void TabControlBD_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSettingsElementsVisibility();
            if (tabControl.SelectedIndex == 0)
            {
                LoadCurrentSettings();
                if (txtPassword != null)
                {
                    txtPassword.UseSystemPasswordChar = true;
                    isPasswordVisible = false;
                }
            }
        }

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

            Task.Run(() =>
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

        private void tabPageSecure_Click(object sender, EventArgs e)
        {

        }

        private void SisAdminForm_Load(object sender, EventArgs e)
        {

        }
    }
}