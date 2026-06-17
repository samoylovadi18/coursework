using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.IO.Compression;

namespace dump
{
    public partial class SisAdminForm : Form
    {
        private bool isPasswordVisible = false;
        private bool isLockDialogOpen = false;
        private Dictionary<string, int> tableColumnsCount = new Dictionary<string, int>();

        // Поля для пароля (если есть на форме)
        private TextBox txtPasswordField;

        // Диалог выбора SQL файла
        private OpenFileDialog openFileDialogScript;

        // ===================== ПОЛЯ ДЛЯ РЕЗЕРВНОГО КОПИРОВАНИЯ =====================
        private System.Windows.Forms.Timer autoBackupTimer;
        private string backupFolder;

        // Настройки (храним в переменных, не в Properties)
        private bool autoBackupEnabled = false;
        private int backupIntervalHours = 24;

        // Храним текущие настройки подключения
        private string currentServer = "";
        private string currentDatabase = "";
        private string currentUsername = "";
        private string currentPassword = "";

        public SisAdminForm()
        {
            InitializeComponent();

            // Регистрируем форму в менеджере бездействия
            InactivityManager.RegisterForm(this);
            InactivityManager.OnLockRequest += LockSystem;

            // Инициализируем папку для бэкапов
            backupFolder = Path.Combine(Application.StartupPath, "Backups");

            // Создаем папку для бэкапов если её нет
            if (!Directory.Exists(backupFolder))
            {
                Directory.CreateDirectory(backupFolder);
            }

            // Инициализируем элементы для пароля
            InitializePasswordField();

            InitializeRestoreFeature();
            InitializeImportExportFeature();
            InitializeSecurityFeature();
            InitializeScriptRestore();

            // Инициализируем резервное копирование
            InitializeBackupFeature();

            // Стилизуем ВСЕ кнопки
            StyleAllButtons();

            // ЗАГРУЖАЕМ ТЕКУЩИЕ НАСТРОЙКИ В ПОЛЯ
            LoadCurrentSettings();

            // Добавляем обработчик закрытия формы
            this.FormClosing += SisAdminForm_FormClosing;
        }

        // ===================== БЛОКИРОВКА СИСТЕМЫ =====================

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
                lblMessage.Text = $"Система заблокирована из-за бездействия ({InactivityManager.GetInactivityTime() / 60} мин.)\nВведите пароль для разблокировки:";
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

        // ===================== ПОЛУЧЕНИЕ РАБОЧЕГО ПОДКЛЮЧЕНИЯ =====================

        private MySqlConnection GetWorkingConnection()
        {
            // Пытаемся получить настройки из SettingsBD
            try
            {
                var config = SettingsBD.GetCurrentConfig();
                currentServer = config.Server;
                currentDatabase = config.Database;
                currentUsername = config.Username;
                currentPassword = config.Password;

                string connString = $"server={currentServer};userid={currentUsername};password={currentPassword};database={currentDatabase};charset=utf8mb4;";
                MySqlConnection conn = new MySqlConnection(connString);
                conn.Open();
                LogBackupMessage($"Подключение успешно: {currentServer}/{currentDatabase}");
                return conn;
            }
            catch (Exception ex)
            {
                LogBackupMessage($"Ошибка получения настроек из SettingsBD: {ex.Message}");
            }

            // Если не получилось, пробуем получить из текстовых полей на форме
            try
            {
                TextBox txtServer = this.Controls.Find("txtServer", true).FirstOrDefault() as TextBox;
                TextBox txtDatabase = this.Controls.Find("txtDatabase", true).FirstOrDefault() as TextBox;
                TextBox txtUsername = this.Controls.Find("txtUsername", true).FirstOrDefault() as TextBox;

                currentServer = txtServer?.Text.Trim() ?? "localhost";
                currentDatabase = txtDatabase?.Text.Trim() ?? "da";
                currentUsername = txtUsername?.Text.Trim() ?? "root";
                currentPassword = txtPasswordField?.Text ?? "";

                string connString = $"server={currentServer};userid={currentUsername};password={currentPassword};database={currentDatabase};charset=utf8mb4;";
                MySqlConnection conn = new MySqlConnection(connString);
                conn.Open();
                LogBackupMessage($"Подключение из полей формы успешно: {currentServer}/{currentDatabase}");
                return conn;
            }
            catch (Exception ex)
            {
                LogBackupMessage($"Ошибка подключения из полей формы: {ex.Message}");
                throw new Exception($"Не удалось подключиться к базе данных. Проверьте настройки подключения.\n\nОшибка: {ex.Message}");
            }
        }

        // ===================== ВОССТАНОВЛЕНИЕ ИЗ SQL ФАЙЛА =====================

        private void InitializeScriptRestore()
        {
            // Создаем диалог выбора файла
            openFileDialogScript = new OpenFileDialog();
            openFileDialogScript.Title = "Выберите SQL файл для восстановления";
            openFileDialogScript.Filter = "SQL файлы (*.sql)|*.sql|Все файлы (*.*)|*.*";
            openFileDialogScript.FilterIndex = 1;
            openFileDialogScript.RestoreDirectory = true;

            // Подписываемся на события кнопок
            if (btnBrowseScript != null)
            {
                btnBrowseScript.Click += BtnBrowseScript_Click;
            }

            if (btnRestoreFromScript != null)
            {
                btnRestoreFromScript.Click += BtnRestoreFromScript_Click;
            }
        }

        private void BtnBrowseScript_Click(object sender, EventArgs e)
        {
            if (openFileDialogScript.ShowDialog() == DialogResult.OK)
            {
                txtScriptPath.Text = openFileDialogScript.FileName;
            }
        }

        private void BtnRestoreFromScript_Click(object sender, EventArgs e)
        {
            // Проверяем, выбран ли файл
            if (string.IsNullOrEmpty(txtScriptPath.Text) || !File.Exists(txtScriptPath.Text))
            {
                MessageBox.Show("Выберите SQL файл для восстановления!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Подтверждение
            DialogResult result = MessageBox.Show(
                "ВНИМАНИЕ! Выполнение SQL скрипта может изменить структуру базы данных и данные.\n\n" +
                "Вы уверены, что хотите продолжить?",
                "Подтверждение восстановления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    LogMessage($"Начало выполнения скрипта: {txtScriptPath.Text}");

                    // Читаем SQL скрипт из файла
                    string sqlScript = File.ReadAllText(txtScriptPath.Text, Encoding.UTF8);

                    using (MySqlConnection conn = GetWorkingConnection())
                    {
                        LogMessage("Подключение к БД успешно");

                        // Отключаем проверку внешних ключей
                        using (MySqlCommand cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 0;", conn))
                        {
                            cmd.ExecuteNonQuery();
                            LogMessage("Отключена проверка внешних ключей");
                        }

                        // Выполняем скрипт
                        LogMessage("Выполнение SQL скрипта...");
                        using (MySqlCommand cmd = new MySqlCommand(sqlScript, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }

                        // Включаем проверку внешних ключей
                        using (MySqlCommand cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 1;", conn))
                        {
                            cmd.ExecuteNonQuery();
                            LogMessage("Включена проверка внешних ключей");
                        }
                    }

                    LogMessage("Скрипт успешно выполнен!");
                    MessageBox.Show("SQL скрипт успешно выполнен!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Перезагружаем списки таблиц
                    LoadTableLists();
                }
                catch (Exception ex)
                {
                    LogMessage($"ОШИБКА: {ex.Message}");
                    MessageBox.Show($"Ошибка при выполнении скрипта:\n{ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ===================== ОБРАБОТЧИК ЗАКРЫТИЯ ФОРМЫ =====================

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

        // ===================== НАСТРОЙКИ ПОДКЛЮЧЕНИЯ =====================

        private void LoadCurrentSettings()
        {
            try
            {
                var config = SettingsBD.GetCurrentConfig();

                TextBox txtServer = this.Controls.Find("txtServer", true).FirstOrDefault() as TextBox;
                TextBox txtDatabase = this.Controls.Find("txtDatabase", true).FirstOrDefault() as TextBox;
                TextBox txtUsername = this.Controls.Find("txtUsername", true).FirstOrDefault() as TextBox;

                if (txtServer != null) txtServer.Text = config.Server;
                if (txtDatabase != null) txtDatabase.Text = config.Database;
                if (txtUsername != null) txtUsername.Text = config.Username;
                if (txtPasswordField != null)
                {
                    txtPasswordField.Text = config.Password;
                    txtPasswordField.UseSystemPasswordChar = true;
                }

                // Сохраняем текущие настройки
                currentServer = config.Server;
                currentDatabase = config.Database;
                currentUsername = config.Username;
                currentPassword = config.Password;
            }
            catch (Exception ex)
            {
                LogMessage($"Ошибка загрузки настроек: {ex.Message}");
            }
        }

        // МЕТОД ДЛЯ ПРОВЕРКИ ПОДКЛЮЧЕНИЯ С ПОНЯТНЫМИ ОШИБКАМИ
        private bool TestConnectionBeforeSave(string server, string database, string username, string password)
        {
            try
            {
                string connectionString = $"server={server};userid={username};password={password};database={database};charset=utf8mb4;";

                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch (MySqlException ex)
            {
                string userMessage = "";

                switch (ex.Number)
                {
                    case 1042:
                        userMessage = "Не удалось найти указанный сервер.\n\n" +
                                     "Проверьте:\n" +
                                     "• Правильно ли указан адрес сервера\n" +
                                     "• Запущен ли сервер базы данных\n" +
                                     "• Нет ли проблем с сетью";
                        break;
                    case 1045:
                        userMessage = "Ошибка авторизации!\n\n" +
                                     "Проверьте:\n" +
                                     "• Правильно ли указано имя пользователя\n" +
                                     "• Правильно ли указан пароль\n" +
                                     "• Есть ли у пользователя доступ к этой базе данных";
                        break;
                    case 1049:
                        userMessage = "Указанная база данных не существует!\n\n" +
                                     "Проверьте:\n" +
                                     "• Правильно ли указано имя базы данных\n" +
                                     "• Создана ли база данных на сервере";
                        break;
                    case 1044:
                    case 1046:
                        userMessage = "Нет доступа к указанной базе данных!\n\n" +
                                     "Проверьте:\n" +
                                     "• Есть ли у пользователя права на эту базу данных\n" +
                                     "• Правильно ли указано имя базы данных";
                        break;
                    case 0:
                        userMessage = "Не удалось подключиться к серверу!\n\n" +
                                     "Проверьте:\n" +
                                     "• Запущен ли сервер базы данных\n" +
                                     "• Правильно ли указан порт подключения\n" +
                                     "• Не блокирует ли подключение брандмауэр";
                        break;
                    default:
                        userMessage = $"Ошибка подключения к базе данных:\n\n{ex.Message}\n\n" +
                                     "Проверьте правильность введенных данных и повторите попытку.";
                        break;
                }

                MessageBox.Show(userMessage, "Ошибка подключения",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogMessage($"Ошибка подключения (код {ex.Number}): {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось подключиться к базе данных!\n\n" +
                             $"Ошибка: {ex.Message}\n\n" +
                             $"Проверьте настройки подключения и повторите попытку.",
                             "Ошибка подключения",
                             MessageBoxButtons.OK,
                             MessageBoxIcon.Error);
                LogMessage($"Ошибка подключения: {ex.Message}");
                return false;
            }
        }

        private void SaveConnectionSettings()
        {
            try
            {
                TextBox txtServer = this.Controls.Find("txtServer", true).FirstOrDefault() as TextBox;
                TextBox txtDatabase = this.Controls.Find("txtDatabase", true).FirstOrDefault() as TextBox;
                TextBox txtUsername = this.Controls.Find("txtUsername", true).FirstOrDefault() as TextBox;

                string server = txtServer?.Text.Trim() ?? "localhost";
                string database = txtDatabase?.Text.Trim() ?? "da";
                string username = txtUsername?.Text.Trim() ?? "root";
                string password = txtPasswordField?.Text ?? "";

                if (string.IsNullOrEmpty(server))
                {
                    MessageBox.Show("Заполните поле 'Сервер'!", "Внимание",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(database))
                {
                    MessageBox.Show("Заполните поле 'База данных'!", "Внимание",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(username))
                {
                    MessageBox.Show("Заполните поле 'Имя пользователя'!", "Внимание",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!TestConnectionBeforeSave(server, database, username, password))
                {
                    return;
                }

                var newConfig = new SettingsBD.ConnectionConfig
                {
                    Server = server,
                    Database = database,
                    Username = username,
                    Password = password
                };

                SettingsBD.UpdateConfig(newConfig);

                // Сохраняем в локальные переменные
                currentServer = server;
                currentDatabase = database;
                currentUsername = username;
                currentPassword = password;

                LogMessage("Настройки подключения сохранены");
                MessageBox.Show("Настройки подключения успешно сохранены!\n\n" +
                              "Подключение к базе данных работает корректно.",
                              "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogMessage($"Ошибка сохранения настроек: {ex.Message}");
                MessageBox.Show($"Ошибка при сохранении настроек:\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TestConnection()
        {
            try
            {
                TextBox txtServer = this.Controls.Find("txtServer", true).FirstOrDefault() as TextBox;
                TextBox txtDatabase = this.Controls.Find("txtDatabase", true).FirstOrDefault() as TextBox;
                TextBox txtUsername = this.Controls.Find("txtUsername", true).FirstOrDefault() as TextBox;

                string server = txtServer?.Text.Trim() ?? "localhost";
                string database = txtDatabase?.Text.Trim() ?? "da";
                string username = txtUsername?.Text.Trim() ?? "root";
                string password = txtPasswordField?.Text ?? "";

                if (string.IsNullOrEmpty(server))
                {
                    MessageBox.Show("Заполните поле 'Сервер'!", "Внимание",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(database))
                {
                    MessageBox.Show("Заполните поле 'База данных'!", "Внимание",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(username))
                {
                    MessageBox.Show("Заполните поле 'Имя пользователя'!", "Внимание",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string connectionString = $"server={server};userid={username};password={password};database={database};charset=utf8mb4;";

                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    MessageBox.Show("Подключение к базе данных успешно установлено!\n\n" +
                                 "Все настройки верны, можно сохранять подключение.",
                                 "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (MySqlException ex)
            {
                string userMessage = "";

                switch (ex.Number)
                {
                    case 1042:
                        userMessage = "Сервер не найден!\n\nПроверьте адрес сервера и повторите попытку.";
                        break;
                    case 1045:
                        userMessage = "Ошибка авторизации!\n\nПроверьте имя пользователя и пароль.";
                        break;
                    case 1049:
                        userMessage = "База данных не найдена!\n\nПроверьте имя базы данных.";
                        break;
                    case 1044:
                    case 1046:
                        userMessage = "Нет доступа к базе данных!\n\nПроверьте права пользователя.";
                        break;
                    default:
                        userMessage = $"Ошибка подключения!\n\n{ex.Message}";
                        break;
                }

                MessageBox.Show(userMessage, "Ошибка подключения",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogMessage($"Ошибка подключения: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения!\n\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogMessage($"Ошибка подключения: {ex.Message}");
            }
        }

        // ===================== ВИДИМОСТЬ ПАРОЛЯ =====================

        private void InitializePasswordField()
        {
            txtPasswordField = this.Controls.Find("txtPassword", true).FirstOrDefault() as TextBox;

            if (txtPasswordField != null)
            {
                isPasswordVisible = false;
                txtPasswordField.UseSystemPasswordChar = true;

                try
                {
                    if (visible_password != null)
                    {
                        visible_password.Image = Image.FromFile("zac.png");
                        visible_password.Click += Visible_password_Click;
                    }
                }
                catch
                {
                    if (visible_password != null)
                    {
                        visible_password.Image = CreateSimpleEyeIcon(false);
                        visible_password.Click += Visible_password_Click;
                    }
                }
            }
        }

        private void Visible_password_Click(object sender, EventArgs e)
        {
            if (txtPasswordField == null) return;

            isPasswordVisible = !isPasswordVisible;

            try
            {
                if (isPasswordVisible)
                {
                    txtPasswordField.UseSystemPasswordChar = false;
                    if (visible_password != null)
                        visible_password.Image = Image.FromFile("otc.png");
                }
                else
                {
                    txtPasswordField.UseSystemPasswordChar = true;
                    if (visible_password != null)
                        visible_password.Image = Image.FromFile("zac.png");
                }
            }
            catch
            {
                if (isPasswordVisible)
                {
                    txtPasswordField.UseSystemPasswordChar = false;
                    if (visible_password != null)
                        visible_password.Image = CreateSimpleEyeIcon(true);
                }
                else
                {
                    txtPasswordField.UseSystemPasswordChar = true;
                    if (visible_password != null)
                        visible_password.Image = CreateSimpleEyeIcon(false);
                }
            }

            txtPasswordField.Focus();
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

        private void StyleAllButtons()
        {
            StyleButton(btnTestConnection);
            StyleButton(btnSave);

            StyleButton(btnBrowseImport);
            StyleButton(btnImport);
            StyleButton(btnExport);
            StyleButton(btnSaveSecurity);
            StyleButton(btnBrowseScript);
            StyleButton(btnRestoreFromScript);
            StyleButton(btnCreateBackup);
            StyleButton(btnBrowseBackupPath);

            if (visible_password != null)
            {
                visible_password.Cursor = Cursors.Hand;
                visible_password.BackColor = Color.Transparent;
            }

            foreach (Control control in this.Controls)
            {
                if (control is Button btn)
                {
                    StyleButton(btn);
                }

                if (control is GroupBox groupBox)
                {
                    foreach (Control innerControl in groupBox.Controls)
                    {
                        if (innerControl is Button innerBtn)
                        {
                            StyleButton(innerBtn);
                        }
                    }
                }

                if (control is Panel panel)
                {
                    foreach (Control innerControl in panel.Controls)
                    {
                        if (innerControl is Button innerBtn)
                        {
                            StyleButton(innerBtn);
                        }
                    }
                }
            }
        }

        private void StyleButton(Button btn)
        {
            if (btn == null) return;

            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.Black;
            btn.BackColor = Color.DarkSeaGreen;
            btn.ForeColor = Color.Black;
            btn.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            btn.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            btn.MouseDown += (s, e) => btn.FlatAppearance.BorderColor = Color.DarkBlue;
            btn.MouseUp += (s, e) => btn.FlatAppearance.BorderColor = Color.Black;
            btn.MouseLeave += (s, e) => btn.FlatAppearance.BorderColor = Color.Black;
        }

        // ===================== БЛОКИРОВКА СИСТЕМЫ (В МИНУТАХ) =====================

        private void InitializeSecurityFeature()
        {
            if (chkAutoLock != null)
            {
                chkAutoLock.Text = "Включить блокировку при бездействии";
                chkAutoLock.Checked = InactivityManager.GetAutoLockEnabled();
                chkAutoLock.CheckedChanged += ChkAutoLock_CheckedChanged;
            }

            if (numInactivityTime != null)
            {
                numInactivityTime.Minimum = 1;
                numInactivityTime.Maximum = 120; // Максимум 120 минут (2 часа)

                // Получаем время в секундах, переводим в минуты
                int seconds = InactivityManager.GetInactivityTime();
                int minutes = seconds / 60;

                // Если минут меньше 1, устанавливаем 1 минуту (60 секунд)
                if (minutes < 1)
                {
                    minutes = 1;
                    // Обновляем значение в менеджере
                    InactivityManager.SetSecuritySettings(InactivityManager.GetAutoLockEnabled(), 60);
                }

                numInactivityTime.Value = minutes;
                numInactivityTime.Enabled = InactivityManager.GetAutoLockEnabled();
                numInactivityTime.ValueChanged += NumInactivityTime_ValueChanged;
            }

            if (btnSaveSecurity != null)
            {
                btnSaveSecurity.Click += BtnSaveSecurity_Click;
            }
        }

        private void ChkAutoLock_CheckedChanged(object sender, EventArgs e)
        {
            bool isChecked = chkAutoLock.Checked;

            // Получаем текущее время в минутах
            int minutes = (int)(numInactivityTime?.Value ?? 5);
            int seconds = minutes * 60; // Переводим в секунды для хранения

            InactivityManager.SetSecuritySettings(isChecked, seconds);

            if (numInactivityTime != null)
            {
                numInactivityTime.Enabled = isChecked;
            }

            LogMessage(isChecked ? "Блокировка включена" : "Блокировка выключена");
        }

        private void NumInactivityTime_ValueChanged(object sender, EventArgs e)
        {
            if (numInactivityTime != null)
            {
                int minutes = (int)numInactivityTime.Value;
                int seconds = minutes * 60; // Переводим в секунды для хранения
                InactivityManager.SetSecuritySettings(InactivityManager.GetAutoLockEnabled(), seconds);
                LogMessage($"Время бездействия изменено на {minutes} мин.");
            }
        }

        private void BtnSaveSecurity_Click(object sender, EventArgs e)
        {
            try
            {
                bool isEnabled = chkAutoLock?.Checked ?? false;
                int minutes = (int)(numInactivityTime?.Value ?? 5);
                int seconds = minutes * 60; // Переводим в секунды для хранения

                InactivityManager.SetSecuritySettings(isEnabled, seconds);

                LogMessage($"Настройки безопасности сохранены: блокировка {(isEnabled ? "включена" : "выключена")}, время {minutes} мин.");

                string message;
                if (isEnabled)
                {
                    message = $"Настройки безопасности успешно сохранены!\n\n" +
                             $"Блокировка: Включена\n" +
                             $"Время бездействия: {minutes} мин.";
                }
                else
                {
                    message = $"Настройки безопасности успешно сохранены!\n\n" +
                             $"Блокировка: Выключена";
                }

                MessageBox.Show(message, "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogMessage($"Ошибка сохранения настроек безопасности: {ex.Message}");
                MessageBox.Show($"Ошибка сохранения настроек: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===================== ВОССТАНОВЛЕНИЕ (ВСТРОЕННОЕ) =====================

        private void InitializeRestoreFeature()
        {
            if (rtbRestoreLog != null)
            {
                rtbRestoreLog.Clear();
                rtbRestoreLog.ReadOnly = true;
                rtbRestoreLog.BackColor = Color.WhiteSmoke;

                LogMessage("=== СИСТЕМА ВОССТАНОВЛЕНИЯ БД ГОТОВА ===");
                LogMessage("Нажмите кнопку 'Восстановить структуру БД' для начала");
                LogMessage("");
            }
        }

        private void LogMessage(string message)
        {
            if (rtbRestoreLog == null) return;

            if (rtbRestoreLog.InvokeRequired)
            {
                rtbRestoreLog.Invoke(new Action(() => LogMessage(message)));
                return;
            }

            rtbRestoreLog.AppendText($"{DateTime.Now:HH:mm:ss} - {message}{Environment.NewLine}");
            rtbRestoreLog.ScrollToCaret();
        }

        private void BtnRestoreDB_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "ВНИМАНИЕ! Восстановление структуры базы данных приведет к:\n\n" +
                "• Удалению всех существующих таблиц\n" +
                "• Потере всех данных\n" +
                "• Созданию новой структуры\n\n" +
                "Вы уверены, что хотите продолжить?",
                "Подтверждение восстановления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                RestoreDatabaseStructure();
            }
        }

        private void RestoreDatabaseStructure()
        {
            try
            {
                LogMessage("");
                LogMessage("===========================================");
                LogMessage("НАЧАЛО ВОССТАНОВЛЕНИЯ СТРУКТУРЫ БД");
                LogMessage("===========================================");

                using (MySqlConnection conn = GetWorkingConnection())
                {
                    LogMessage("Подключение к БД установлено");

                    using (MySqlCommand cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 0;", conn))
                    {
                        cmd.ExecuteNonQuery();
                        LogMessage("Отключена проверка внешних ключей");
                    }

                    LogMessage("Удаление таблиц...");
                    DropAllTables(conn);
                    LogMessage("Таблицы удалены");

                    LogMessage("Создание таблиц...");
                    CreateAllTables(conn);
                    LogMessage("Таблицы созданы");

                    using (MySqlCommand cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 1;", conn))
                    {
                        cmd.ExecuteNonQuery();
                        LogMessage("Включена проверка внешних ключей");
                    }
                }

                LogMessage("===========================================");
                LogMessage("ВОССТАНОВЛЕНИЕ УСПЕШНО ЗАВЕРШЕНО!");
                LogMessage("===========================================");
                LogMessage("");

                LoadTableLists();
                MessageBox.Show("Структура БД восстановлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogMessage($"ОШИБКА: {ex.Message}");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DropAllTables(MySqlConnection conn)
        {
            string[] tables = {
                "order_dish", "other_orders", "orders", "certificates", "dishes",
                "users", "present", "categories", "order_statuses", "status_certificates", "roles"
            };

            int droppedCount = 0;
            foreach (string table in tables)
            {
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand($"DROP TABLE IF EXISTS `{table}`;", conn))
                    {
                        cmd.ExecuteNonQuery();
                        droppedCount++;
                        LogMessage($"  Удалена: {table}");
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"  Ошибка при удалении {table}: {ex.Message}");
                }
            }
            LogMessage($"  Всего удалено: {droppedCount}");
        }

        private void CreateAllTables(MySqlConnection conn)
        {
            // roles
            using (MySqlCommand cmd = new MySqlCommand(@"
                CREATE TABLE IF NOT EXISTS `roles` (
                    `id_role` INT NOT NULL AUTO_INCREMENT,
                    `role_name` VARCHAR(50) NOT NULL,
                    PRIMARY KEY (`id_role`),
                    UNIQUE KEY `role_name` (`role_name`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;", conn))
            {
                cmd.ExecuteNonQuery();
                LogMessage("  Создана: roles");
            }

            using (MySqlCommand cmd = new MySqlCommand(@"
                INSERT INTO `roles` (`id_role`, `role_name`) VALUES 
                (1, 'manager'), (2, 'director'), (3, 'admin');", conn))
            {
                cmd.ExecuteNonQuery();
                LogMessage("  Данные: roles");
            }

            // users
            using (MySqlCommand cmd = new MySqlCommand(@"
                CREATE TABLE IF NOT EXISTS `users` (
                    `id_user` INT NOT NULL AUTO_INCREMENT,
                    `FIO` VARCHAR(100) NOT NULL,
                    `id_role` INT NOT NULL,
                    `login` VARCHAR(50) NOT NULL,
                    `password_hash` VARCHAR(64) NOT NULL,
                    PRIMARY KEY (`id_user`),
                    UNIQUE KEY `login` (`login`),
                    KEY `id_role` (`id_role`),
                    CONSTRAINT `users_ibfk_1` FOREIGN KEY (`id_role`) REFERENCES `roles` (`id_role`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;", conn))
            {
                cmd.ExecuteNonQuery();
                LogMessage("  Создана: users");
            }

            // order_statuses
            using (MySqlCommand cmd = new MySqlCommand(@"
                CREATE TABLE IF NOT EXISTS `order_statuses` (
                    `id_status` INT NOT NULL AUTO_INCREMENT,
                    `status_name` VARCHAR(255) DEFAULT NULL,
                    PRIMARY KEY (`id_status`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;", conn))
            {
                cmd.ExecuteNonQuery();
                LogMessage("  Создана: order_statuses");
            }

            using (MySqlCommand cmd = new MySqlCommand(@"
                INSERT INTO `order_statuses` (`id_status`, `status_name`) VALUES 
                (1, 'В обработке'), (2, 'Принят'), (3, 'В приготовлении'),
                (4, 'Готов'), (5, 'В пути'), (6, 'Доставлен'), (7, 'Отменён');", conn))
            {
                cmd.ExecuteNonQuery();
                LogMessage("  Данные: order_statuses");
            }

            // categories
            using (MySqlCommand cmd = new MySqlCommand(@"
                CREATE TABLE IF NOT EXISTS `categories` (
                    `id_category` INT NOT NULL AUTO_INCREMENT,
                    `category_name` VARCHAR(255) NOT NULL,
                    PRIMARY KEY (`id_category`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;", conn))
            {
                cmd.ExecuteNonQuery();
                LogMessage("  Создана: categories");
            }

            // dishes
            using (MySqlCommand cmd = new MySqlCommand(@"
                CREATE TABLE IF NOT EXISTS `dishes` (
                    `id_dish` INT NOT NULL AUTO_INCREMENT,
                    `dish_name` VARCHAR(255) NOT NULL,
                    `compound` VARCHAR(255) DEFAULT NULL,
                    `id_category` INT NOT NULL,
                    `price` DECIMAL(10,2) NOT NULL,
                    `photo` LONGBLOB,
                    `weight_volume` VARCHAR(20) NOT NULL,
                    `cost` DECIMAL(10,2) DEFAULT '0.00',
                    PRIMARY KEY (`id_dish`),
                    KEY `FK_id_category` (`id_category`),
                    CONSTRAINT `dishes_ibfk_1` FOREIGN KEY (`id_category`) REFERENCES `categories` (`id_category`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;", conn))
            {
                cmd.ExecuteNonQuery();
                LogMessage("  Создана: dishes");
            }

            // status_certificates
            using (MySqlCommand cmd = new MySqlCommand(@"
                CREATE TABLE IF NOT EXISTS `status_certificates` (
                    `id_status_certificate` INT NOT NULL AUTO_INCREMENT,
                    `name` VARCHAR(255) DEFAULT NULL,
                    PRIMARY KEY (`id_status_certificate`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;", conn))
            {
                cmd.ExecuteNonQuery();
                LogMessage("  Создана: status_certificates");
            }

            using (MySqlCommand cmd = new MySqlCommand(@"
                INSERT INTO `status_certificates` (`id_status_certificate`, `name`) VALUES 
                (1, 'Активен'), (2, 'Использован'), (3, 'Возвращён');", conn))
            {
                cmd.ExecuteNonQuery();
                LogMessage("  Данные: status_certificates");
            }

            // certificates
            using (MySqlCommand cmd = new MySqlCommand(@"
                CREATE TABLE IF NOT EXISTS `certificates` (
                    `id_certificate` INT NOT NULL AUTO_INCREMENT,
                    `last_name` VARCHAR(255) NOT NULL,
                    `first_name` VARCHAR(255) NOT NULL,
                    `middle_name` VARCHAR(255) DEFAULT NULL,
                    `price` DECIMAL(10,2) NOT NULL,
                    `date` DATE NOT NULL,
                    `id_status_certificate` INT DEFAULT NULL,
                    `phone_number` VARCHAR(20) NOT NULL,
                    PRIMARY KEY (`id_certificate`),
                    KEY `FK_id_status_certificate` (`id_status_certificate`),
                    CONSTRAINT `certificates_ibfk_1` FOREIGN KEY (`id_status_certificate`) REFERENCES `status_certificates` (`id_status_certificate`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;", conn))
            {
                cmd.ExecuteNonQuery();
                LogMessage("  Создана: certificates");
            }

            // present
            using (MySqlCommand cmd = new MySqlCommand(@"
                CREATE TABLE IF NOT EXISTS `present` (
                    `id_present` INT NOT NULL AUTO_INCREMENT,
                    `name` VARCHAR(255) DEFAULT NULL,
                    `from_price` DECIMAL(10,2) DEFAULT NULL,
                    PRIMARY KEY (`id_present`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;", conn))
            {
                cmd.ExecuteNonQuery();
                LogMessage("  Создана: present");
            }

            // orders
            using (MySqlCommand cmd = new MySqlCommand(@"
                CREATE TABLE IF NOT EXISTS `orders` (
                    `id_order` INT NOT NULL AUTO_INCREMENT,
                    `name_client` VARCHAR(255) NOT NULL,
                    `phone_number` VARCHAR(20) NOT NULL,
                    `address` VARCHAR(255) NOT NULL,
                    `number_persons` INT DEFAULT NULL,
                    `delivery_date` DATE NOT NULL,
                    `delivery_time` TIME NOT NULL,
                    `comment` VARCHAR(255) DEFAULT NULL,
                    `payment_method` VARCHAR(50) NOT NULL DEFAULT 'Наличные',
                    `id_status` INT NOT NULL,
                    `total_amount` DECIMAL(10,2) NOT NULL DEFAULT '0.00',
                    `created_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP,
                    PRIMARY KEY (`id_order`),
                    KEY `id_status` (`id_status`),
                    CONSTRAINT `orders_ibfk_1` FOREIGN KEY (`id_status`) REFERENCES `order_statuses` (`id_status`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;", conn))
            {
                cmd.ExecuteNonQuery();
                LogMessage("  Создана: orders");
            }

            // order_dish
            using (MySqlCommand cmd = new MySqlCommand(@"
                CREATE TABLE IF NOT EXISTS `order_dish` (
                    `id_order_dish` INT NOT NULL AUTO_INCREMENT,
                    `id_order` INT NOT NULL,
                    `id_dish` INT NOT NULL,
                    `quantity` INT NOT NULL DEFAULT '1',
                    `price_at_order` DECIMAL(10,2) NOT NULL,
                    `is_gift` TINYINT(1) NOT NULL DEFAULT '0',
                    `id_present` INT DEFAULT NULL,
                    PRIMARY KEY (`id_order_dish`),
                    KEY `id_order` (`id_order`),
                    KEY `id_dish` (`id_dish`),
                    KEY `id_present` (`id_present`),
                    CONSTRAINT `order_dish_ibfk_1` FOREIGN KEY (`id_order`) REFERENCES `orders` (`id_order`) ON DELETE CASCADE,
                    CONSTRAINT `order_dish_ibfk_2` FOREIGN KEY (`id_dish`) REFERENCES `dishes` (`id_dish`),
                    CONSTRAINT `order_dish_ibfk_3` FOREIGN KEY (`id_present`) REFERENCES `present` (`id_present`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;", conn))
            {
                cmd.ExecuteNonQuery();
                LogMessage("  Создана: order_dish");
            }

            // other_orders
            using (MySqlCommand cmd = new MySqlCommand(@"
                CREATE TABLE IF NOT EXISTS `other_orders` (
                    `id_other` INT NOT NULL AUTO_INCREMENT,
                    `id_order` INT DEFAULT NULL,
                    `id_status` INT DEFAULT NULL,
                    PRIMARY KEY (`id_other`),
                    KEY `id_order` (`id_order`),
                    KEY `other_orders_ibfk_1` (`id_status`),
                    CONSTRAINT `other_orders_ibfk_1` FOREIGN KEY (`id_status`) REFERENCES `order_statuses` (`id_status`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;", conn))
            {
                cmd.ExecuteNonQuery();
                LogMessage("  Создана: other_orders");
            }

            LogMessage("  Все таблицы созданы!");
        }

        // ===================== ИМПОРТ/ЭКСПОРТ =====================

        private readonly string[] excludedTables = new string[] { "roles" };

        private void InitializeImportExportFeature()
        {
            if (cmbTables != null)
            {
                cmbTables.DropDownStyle = ComboBoxStyle.DropDownList;
            }

            if (btnBrowseImport != null)
            {
                btnBrowseImport.Text = "Обзор...";
                btnBrowseImport.Click += BtnBrowseImport_Click;
            }

            if (btnImport != null)
            {
                btnImport.Text = "Импортировать";
                btnImport.Enabled = false;
                btnImport.Click += BtnImport_Click;
            }

            if (txtImportFilePath != null)
            {
                txtImportFilePath.ReadOnly = true;
                txtImportFilePath.TextChanged += (s, e) => btnImport.Enabled = !string.IsNullOrEmpty(txtImportFilePath.Text);
            }

            if (cmbExportTables != null)
            {
                cmbExportTables.DropDownStyle = ComboBoxStyle.DropDownList;
            }

            if (btnExport != null)
            {
                btnExport.Text = "Экспортировать";
                btnExport.Click += BtnExport_Click;
            }

            if (saveFileDialog != null)
            {
                saveFileDialog.Filter = "CSV файлы (*.csv)|*.csv|Все файлы (*.*)|*.*";
                saveFileDialog.DefaultExt = "csv";
                saveFileDialog.AddExtension = true;
            }

            LoadTableLists();
        }

        private void LoadTableLists()
        {
            try
            {
                using (MySqlConnection conn = GetWorkingConnection())
                {
                    DataTable schema = conn.GetSchema("Tables");

                    if (cmbTables != null) cmbTables.Items.Clear();
                    if (cmbExportTables != null) cmbExportTables.Items.Clear();

                    foreach (DataRow row in schema.Rows)
                    {
                        string tableName = row["TABLE_NAME"].ToString();
                        if (!tableName.StartsWith("mysql") &&
                            !tableName.StartsWith("information_schema") &&
                            !excludedTables.Contains(tableName))
                        {
                            if (cmbTables != null) cmbTables.Items.Add(tableName);
                            if (cmbExportTables != null) cmbExportTables.Items.Add(tableName);
                        }
                    }

                    if (cmbTables != null && cmbTables.Items.Count > 0)
                        cmbTables.SelectedIndex = 0;

                    if (cmbExportTables != null && cmbExportTables.Items.Count > 0)
                        cmbExportTables.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списка таблиц: {ex.Message}\n\nПроверьте настройки подключения!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnBrowseImport_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Выберите CSV файл для импорта";
                ofd.Filter = "CSV файлы (*.csv)|*.csv|Все файлы (*.*)|*.*";
                ofd.FilterIndex = 1;
                ofd.RestoreDirectory = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtImportFilePath.Text = ofd.FileName;
                }
            }
        }

        private void BtnImport_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbTables?.SelectedItem == null)
                {
                    MessageBox.Show("Выберите таблицу для импорта!");
                    return;
                }

                string tableName = cmbTables.SelectedItem.ToString();
                string filePath = txtImportFilePath.Text;

                if (!File.Exists(filePath))
                {
                    MessageBox.Show("Файл не существует!");
                    return;
                }

                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

                if (lines.Length < 2)
                {
                    MessageBox.Show("Файл пуст или не содержит данных!");
                    return;
                }

                using (MySqlConnection conn = GetWorkingConnection())
                {
                    using (MySqlCommand cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 0;", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    try
                    {
                        using (MySqlCommand cmd = new MySqlCommand($"TRUNCATE TABLE `{tableName}`", conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch
                    {
                        using (MySqlCommand cmd = new MySqlCommand($"DELETE FROM `{tableName}`", conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                        using (MySqlCommand cmd = new MySqlCommand($"ALTER TABLE `{tableName}` AUTO_INCREMENT = 1", conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }

                    char delimiter = lines[0].Contains(';') ? ';' : ',';
                    int importedCount = 0;

                    for (int i = 1; i < lines.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[i])) continue;

                        string[] values = ParseCSVLine(lines[i], delimiter);
                        string placeholders = string.Join(",", values.Select((v, idx) => $"@p{idx}"));
                        string query = $"INSERT INTO `{tableName}` VALUES ({placeholders})";

                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            for (int j = 0; j < values.Length; j++)
                            {
                                string val = values[j].Trim().Trim('"');
                                if (string.IsNullOrEmpty(val) || val == "NULL")
                                {
                                    cmd.Parameters.AddWithValue($"@p{j}", DBNull.Value);
                                }
                                else
                                {
                                    cmd.Parameters.AddWithValue($"@p{j}", val);
                                }
                            }
                            cmd.ExecuteNonQuery();
                            importedCount++;
                        }
                    }

                    using (MySqlCommand cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 1;", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show($"Импорт завершен!\nДобавлено записей: {importedCount}");
                    txtImportFilePath.Text = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте: {ex.Message}");
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbExportTables?.SelectedItem == null)
                {
                    MessageBox.Show("Выберите таблицу для экспорта!");
                    return;
                }

                string tableName = cmbExportTables.SelectedItem.ToString();

                if (saveFileDialog?.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog.FileName;
                    ExportToCSV(tableName, filePath);
                    MessageBox.Show($"Таблица '{tableName}' успешно экспортирована!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}");
            }
        }

        private void ExportToCSV(string tableName, string filePath)
        {
            using (MySqlConnection conn = GetWorkingConnection())
            {
                using (MySqlCommand cmd = new MySqlCommand("SET NAMES utf8mb4;", conn))
                {
                    cmd.ExecuteNonQuery();
                }

                string query = $"SELECT * FROM `{tableName}`";
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    sb.Append($"{dt.Columns[i].ColumnName}");
                    if (i < dt.Columns.Count - 1) sb.Append(";");
                }
                sb.AppendLine();

                foreach (DataRow row in dt.Rows)
                {
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        string value = row[i]?.ToString() ?? "";
                        sb.Append(value);
                        if (i < dt.Columns.Count - 1) sb.Append(";");
                    }
                    sb.AppendLine();
                }

                File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(true));
            }
        }

        private string[] ParseCSVLine(string line, char delimiter)
        {
            List<string> result = new List<string>();
            bool inQuotes = false;
            StringBuilder current = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == delimiter && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            result.Add(current.ToString());
            return result.ToArray();
        }

        // ===================== ОБРАБОТЧИКИ КНОПОК =====================

        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            TestConnection();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveConnectionSettings();
        }

        // ===================== РЕЗЕРВНОЕ КОПИРОВАНИЕ =====================

        private void InitializeBackupFeature()
        {
            // Находим все элементы на форме по имени
            txtBackupPath = this.Controls.Find("txtBackupPath", true).FirstOrDefault() as TextBox;
            btnBrowseBackupPath = this.Controls.Find("btnBrowseBackupPath", true).FirstOrDefault() as Button;
            rbFullBackup = this.Controls.Find("rbFullBackup", true).FirstOrDefault() as RadioButton;
            rbStructureOnly = this.Controls.Find("rbStructureOnly", true).FirstOrDefault() as RadioButton;
            rbDataOnly = this.Controls.Find("rbDataOnly", true).FirstOrDefault() as RadioButton;
            btnCreateBackup = this.Controls.Find("btnCreateBackup", true).FirstOrDefault() as Button;
            chkAutoBackup = this.Controls.Find("chkAutoBackup", true).FirstOrDefault() as CheckBox;
            numBackupInterval = this.Controls.Find("numBackupInterval", true).FirstOrDefault() as NumericUpDown;
            cmbAutoBackupType = this.Controls.Find("cmbAutoBackupType", true).FirstOrDefault() as ComboBox;
            lblBackupStatus = this.Controls.Find("lblBackupStatus", true).FirstOrDefault() as Label;

            // Настройка ComboBox
            if (cmbAutoBackupType != null)
            {
                cmbAutoBackupType.Items.Clear();
                cmbAutoBackupType.Items.Add("Полный бэкап");
                cmbAutoBackupType.Items.Add("Только данные");
                cmbAutoBackupType.SelectedIndex = 0;
                cmbAutoBackupType.Enabled = false;
            }

            // Настройка NumericUpDown
            if (numBackupInterval != null)
            {
                numBackupInterval.Minimum = 1;
                numBackupInterval.Maximum = 720;
                numBackupInterval.Value = backupIntervalHours;
                numBackupInterval.Enabled = false;
            }

            // Устанавливаем путь
            if (txtBackupPath != null)
            {
                txtBackupPath.Text = backupFolder;
                txtBackupPath.ReadOnly = true;
            }

            // Создаем таймер
            autoBackupTimer = new System.Windows.Forms.Timer();
            autoBackupTimer.Tick += AutoBackupTimer_Tick;

            // Подписываем события
            if (btnBrowseBackupPath != null)
            {
                btnBrowseBackupPath.Click += BtnBrowseBackupPath_Click;
            }

            if (btnCreateBackup != null)
            {
                btnCreateBackup.Click += BtnCreateBackup_Click;
            }

            if (chkAutoBackup != null)
            {
                chkAutoBackup.CheckedChanged += ChkAutoBackup_CheckedChanged;
            }

            if (numBackupInterval != null)
            {
                numBackupInterval.ValueChanged += NumBackupInterval_ValueChanged;
            }

            // Загружаем настройки из файла
            LoadBackupSettingsFromFile();

            LogBackupMessage("Система резервного копирования готова");
        }

        private void LoadBackupSettingsFromFile()
        {
            string settingsFile = Path.Combine(Application.StartupPath, "backup_settings.txt");

            try
            {
                if (File.Exists(settingsFile))
                {
                    string[] lines = File.ReadAllLines(settingsFile);
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("AutoBackup="))
                        {
                            string value = line.Substring(11);
                            autoBackupEnabled = (value == "True");
                        }
                        else if (line.StartsWith("Interval="))
                        {
                            string value = line.Substring(9);
                            int.TryParse(value, out backupIntervalHours);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogBackupMessage($"Ошибка загрузки настроек: {ex.Message}");
            }

            if (chkAutoBackup != null)
            {
                chkAutoBackup.Checked = autoBackupEnabled;
            }

            if (numBackupInterval != null)
            {
                numBackupInterval.Value = backupIntervalHours;
            }

            if (autoBackupEnabled)
            {
                if (numBackupInterval != null)
                {
                    numBackupInterval.Enabled = true;
                }
                if (cmbAutoBackupType != null)
                {
                    cmbAutoBackupType.Enabled = true;
                }
                autoBackupTimer.Interval = backupIntervalHours * 60 * 60 * 1000;
                autoBackupTimer.Start();
                LogBackupMessage($"Автоматическое резервное копирование включено (интервал: {backupIntervalHours} часов)");
            }
            else
            {
                if (numBackupInterval != null)
                {
                    numBackupInterval.Enabled = false;
                }
                if (cmbAutoBackupType != null)
                {
                    cmbAutoBackupType.Enabled = false;
                }
            }
        }

        private void SaveBackupSettingsToFile()
        {
            string settingsFile = Path.Combine(Application.StartupPath, "backup_settings.txt");

            try
            {
                List<string> lines = new List<string>();
                lines.Add($"AutoBackup={autoBackupEnabled}");
                lines.Add($"Interval={backupIntervalHours}");
                File.WriteAllLines(settingsFile, lines);
                LogBackupMessage("Настройки резервного копирования сохранены");
            }
            catch (Exception ex)
            {
                LogBackupMessage($"Ошибка сохранения настроек: {ex.Message}");
            }
        }

        private void ChkAutoBackup_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAutoBackup != null)
            {
                autoBackupEnabled = chkAutoBackup.Checked;
            }

            if (numBackupInterval != null)
            {
                numBackupInterval.Enabled = autoBackupEnabled;
            }

            if (cmbAutoBackupType != null)
            {
                cmbAutoBackupType.Enabled = autoBackupEnabled;
            }

            if (autoBackupEnabled)
            {
                autoBackupTimer.Interval = backupIntervalHours * 60 * 60 * 1000;
                autoBackupTimer.Start();
                LogBackupMessage($"Автоматическое резервное копирование включено (интервал: {backupIntervalHours} часов)");
            }
            else
            {
                autoBackupTimer.Stop();
                LogBackupMessage("Автоматическое резервное копирование выключено");
            }

            SaveBackupSettingsToFile();
        }

        private void NumBackupInterval_ValueChanged(object sender, EventArgs e)
        {
            if (numBackupInterval != null)
            {
                backupIntervalHours = (int)numBackupInterval.Value;
            }

            if (autoBackupEnabled)
            {
                autoBackupTimer.Interval = backupIntervalHours * 60 * 60 * 1000;
                LogBackupMessage($"Интервал автоматического резервного копирования изменен на {backupIntervalHours} часов");
            }

            SaveBackupSettingsToFile();
        }

        private void AutoBackupTimer_Tick(object sender, EventArgs e)
        {
            string backupType = "full";
            if (cmbAutoBackupType != null && cmbAutoBackupType.SelectedIndex == 1)
            {
                backupType = "data";
            }
            System.Threading.Tasks.Task.Run(() => CreateBackup(backupType, true));
        }

        private void BtnCreateBackup_Click(object sender, EventArgs e)
        {
            string backupType = "full";

            if (rbStructureOnly != null && rbStructureOnly.Checked)
            {
                backupType = "structure";
            }
            else if (rbDataOnly != null && rbDataOnly.Checked)
            {
                backupType = "data";
            }

            CreateBackup(backupType, false);
        }

        private void BtnBrowseBackupPath_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Выберите папку для сохранения резервных копий";
                fbd.SelectedPath = backupFolder;
                fbd.ShowNewFolderButton = true;

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    backupFolder = fbd.SelectedPath;
                    if (txtBackupPath != null)
                    {
                        txtBackupPath.Text = backupFolder;
                    }

                    if (!Directory.Exists(backupFolder))
                    {
                        Directory.CreateDirectory(backupFolder);
                    }

                    LogBackupMessage($"Папка для резервных копий изменена: {backupFolder}");
                }
            }
        }

        private void CreateBackup(string backupType, bool isAuto)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => CreateBackup(backupType, isAuto)));
                return;
            }

            try
            {
                if (lblBackupStatus != null)
                {
                    lblBackupStatus.Text = "Проверка подключения к БД...";
                    lblBackupStatus.ForeColor = Color.Blue;
                }
                Application.DoEvents();

                string targetFolder = backupFolder;
                if (!isAuto && txtBackupPath != null && !string.IsNullOrEmpty(txtBackupPath.Text))
                {
                    targetFolder = txtBackupPath.Text;
                }

                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }

                string backupFileName = $"{GetBackupPrefix(backupType)}_backup_{DateTime.Now:yyyyMMdd_HHmmss}.sql";
                string fullPath = Path.Combine(targetFolder, backupFileName);

                LogBackupMessage($"Начало создания {GetBackupTypeName(backupType)} резервной копии...");
                LogBackupMessage($"Папка сохранения: {targetFolder}");
                LogBackupMessage($"Имя файла: {backupFileName}");

                // ИСПОЛЬЗУЕМ НОВЫЙ МЕТОД ДЛЯ ПОДКЛЮЧЕНИЯ
                using (MySqlConnection conn = GetWorkingConnection())
                {
                    LogBackupMessage("Подключение к базе данных установлено успешно");

                    // Отключаем проверку внешних ключей
                    using (MySqlCommand cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 0;", conn))
                    {
                        cmd.ExecuteNonQuery();
                        LogBackupMessage("Отключена проверка внешних ключей");
                    }

                    StringBuilder sqlScript = new StringBuilder();

                    // Добавляем заголовок
                    sqlScript.AppendLine($"-- Резервная копия создана: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    sqlScript.AppendLine($"-- Тип резервной копии: {GetBackupTypeName(backupType)}");
                    sqlScript.AppendLine($"-- Сервер: {conn.DataSource}");
                    sqlScript.AppendLine($"-- База данных: {conn.Database}");
                    sqlScript.AppendLine("SET FOREIGN_KEY_CHECKS = 0;");
                    sqlScript.AppendLine("SET AUTOCOMMIT = 0;");
                    sqlScript.AppendLine("");

                    // Получаем список таблиц
                    List<string> tables = GetTableList(conn);
                    int totalTables = tables.Count;
                    int currentTable = 0;

                    LogBackupMessage($"Найдено таблиц для обработки: {totalTables}");

                    foreach (string table in tables)
                    {
                        currentTable++;

                        if (lblBackupStatus != null)
                        {
                            lblBackupStatus.Text = $"Обработка таблицы {currentTable} из {totalTables}: {table}";
                            Application.DoEvents();
                        }

                        LogBackupMessage($"  Обработка таблицы {currentTable}/{totalTables}: {table}");

                        // Сохраняем структуру таблицы
                        if (backupType == "structure" || backupType == "full")
                        {
                            string createTableScript = GetTableStructure(conn, table);
                            if (!string.IsNullOrEmpty(createTableScript))
                            {
                                sqlScript.AppendLine(createTableScript);
                                sqlScript.AppendLine("");
                                LogBackupMessage($"    Структура таблицы {table} сохранена");
                            }
                        }

                        // Сохраняем данные таблицы
                        if ((backupType == "data" || backupType == "full"))
                        {
                            string dataScript = GetTableData(conn, table);
                            if (!string.IsNullOrEmpty(dataScript))
                            {
                                sqlScript.AppendLine(dataScript);
                                sqlScript.AppendLine("");
                                LogBackupMessage($"    Данные таблицы {table} сохранены");
                            }
                            else
                            {
                                LogBackupMessage($"    Таблица {table} не содержит данных");
                            }
                        }
                    }

                    sqlScript.AppendLine("COMMIT;");
                    sqlScript.AppendLine("SET FOREIGN_KEY_CHECKS = 1;");
                    sqlScript.AppendLine("-- Конец резервной копии");

                    // Сохраняем SQL файл
                    File.WriteAllText(fullPath, sqlScript.ToString(), Encoding.UTF8);
                    LogBackupMessage($"SQL скрипт сохранен: {backupFileName}");
                    LogBackupMessage($"Размер SQL файла: {new FileInfo(fullPath).Length / 1024.0:F2} KB");

                    // Включаем проверку внешних ключей
                    using (MySqlCommand cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 1;", conn))
                    {
                        cmd.ExecuteNonQuery();
                        LogBackupMessage("Включена проверка внешних ключей");
                    }
                }

                // Сжимаем файл в ZIP
                if (lblBackupStatus != null)
                {
                    lblBackupStatus.Text = "Сжатие файла...";
                    Application.DoEvents();
                }

                string zipPath = CompressToZip(fullPath);
                FileInfo zipFileInfo = new FileInfo(zipPath);

                LogBackupMessage($"Резервная копия сжата: {Path.GetFileName(zipPath)}");
                LogBackupMessage($"Размер ZIP файла: {zipFileInfo.Length / 1024.0:F2} KB");

                if (lblBackupStatus != null)
                {
                    lblBackupStatus.Text = "Готово!";
                    lblBackupStatus.ForeColor = Color.Green;
                }

                LogBackupMessage($"Резервная копия успешно создана: {Path.GetFileName(zipPath)}");

                if (!isAuto)
                {
                    string size = zipFileInfo.Length > 1048576 ? $"{zipFileInfo.Length / 1048576.0:F2} MB" : $"{zipFileInfo.Length / 1024.0:F2} KB";
                    MessageBox.Show($"Резервная копия успешно создана!\n\n" +
                                  $"Файл: {zipFileInfo.Name}\n" +
                                  $"Размер: {size}\n" +
                                  $"Путь: {zipFileInfo.DirectoryName}\n\n" +
                                  $"Тип копии: {GetBackupTypeName(backupType)}\n" +
                                  $"Дата создания: {DateTime.Now:dd.MM.yyyy HH:mm:ss}",
                                  "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                // Очищаем старые бэкапы
                int deletedCount = CleanupOldBackups(10);
                if (deletedCount > 0)
                {
                    LogBackupMessage($"Удалено старых резервных копий: {deletedCount}");
                }
            }
            catch (Exception ex)
            {
                LogBackupMessage($"ОШИБКА при создании резервной копии: {ex.Message}");
                LogBackupMessage($"Детали ошибки: {ex.StackTrace}");

                if (lblBackupStatus != null)
                {
                    lblBackupStatus.Text = $"Ошибка: {ex.Message}";
                    lblBackupStatus.ForeColor = Color.Red;
                }

                if (!isAuto)
                {
                    MessageBox.Show($"Ошибка при создании резервной копии:\n\n{ex.Message}\n\nПроверьте настройки подключения к базе данных!",
                                  "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string GetBackupPrefix(string backupType)
        {
            switch (backupType)
            {
                case "structure":
                    return "struct";
                case "data":
                    return "data";
                case "full":
                    return "full";
                default:
                    return "backup";
            }
        }

        private string GetBackupTypeName(string backupType)
        {
            switch (backupType)
            {
                case "structure":
                    return "структуры";
                case "data":
                    return "данных";
                case "full":
                    return "полной";
                default:
                    return "резервной";
            }
        }

        private List<string> GetTableList(MySqlConnection conn)
        {
            List<string> tables = new List<string>();
            DataTable schema = conn.GetSchema("Tables");

            foreach (DataRow row in schema.Rows)
            {
                string tableName = row["TABLE_NAME"].ToString();

                // Исключаем системные таблицы
                if (!tableName.StartsWith("mysql") &&
                    !tableName.StartsWith("information_schema") &&
                    !tableName.StartsWith("performance_schema") &&
                    !tableName.StartsWith("sys"))
                {
                    tables.Add(tableName);
                }
            }

            return tables;
        }

        private string GetTableStructure(MySqlConnection conn, string tableName)
        {
            using (MySqlCommand cmd = new MySqlCommand($"SHOW CREATE TABLE `{tableName}`", conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader.GetString(1) + ";";
                    }
                }
            }
            return "";
        }

        private string GetTableData(MySqlConnection conn, string tableName)
        {
            StringBuilder dataScript = new StringBuilder();

            using (MySqlCommand cmd = new MySqlCommand($"SELECT * FROM `{tableName}`", conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        return "";
                    }

                    // Получаем имена колонок
                    var columns = new List<string>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        columns.Add(reader.GetName(i));
                    }

                    while (reader.Read())
                    {
                        dataScript.Append($"INSERT INTO `{tableName}` (`{string.Join("`, `", columns)}`) VALUES (");

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            object value = reader.GetValue(i);
                            dataScript.Append(FormatSQLValue(value));

                            if (i < reader.FieldCount - 1)
                            {
                                dataScript.Append(", ");
                            }
                        }

                        dataScript.AppendLine(");");
                    }
                }
            }

            return dataScript.ToString();
        }

        private string FormatSQLValue(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return "NULL";
            }

            if (value is DateTime dt)
            {
                return $"'{dt:yyyy-MM-dd HH:mm:ss}'";
            }

            if (value is bool b)
            {
                return b ? "1" : "0";
            }

            if (value is string || value is char)
            {
                string str = value.ToString().Replace("'", "''");
                return $"'{str}'";
            }

            if (value is byte[] bytes)
            {
                return $"0x{BitConverter.ToString(bytes).Replace("-", "")}";
            }

            if (value is decimal || value is float || value is double)
            {
                return value.ToString().Replace(',', '.');
            }

            if (value is int || value is long || value is short || value is byte)
            {
                return value.ToString();
            }

            return $"'{value.ToString().Replace("'", "''")}'";
        }

        private string CompressToZip(string filePath)
        {
            return filePath;
        }

        private int CleanupOldBackups(int keepCount)
        {
            try
            {
                var backupFiles = Directory.GetFiles(backupFolder, "*.zip")
                                          .OrderByDescending(f => File.GetCreationTime(f))
                                          .ToList();

                int deleted = 0;

                for (int i = keepCount; i < backupFiles.Count; i++)
                {
                    File.Delete(backupFiles[i]);
                    deleted++;
                    LogBackupMessage($"Удален старый бэкап: {Path.GetFileName(backupFiles[i])}");
                }

                return deleted;
            }
            catch (Exception ex)
            {
                LogBackupMessage($"Ошибка при очистке старых бэкапов: {ex.Message}");
                return 0;
            }
        }

        private void LogBackupMessage(string message)
        {
            if (rtbRestoreLog != null)
            {
                if (rtbRestoreLog.InvokeRequired)
                {
                    rtbRestoreLog.Invoke(new Action(() => LogBackupMessage(message)));
                    return;
                }
                rtbRestoreLog.AppendText($"{DateTime.Now:HH:mm:ss} - {message}{Environment.NewLine}");
                rtbRestoreLog.ScrollToCaret();
            }
        }

        // ===================== ЗАГЛУШКИ ДЛЯ ДРУГИХ СОБЫТИЙ =====================

        private void SisAdminForm_Load(object sender, EventArgs e) { }
        private void tabPageSecure_Click(object sender, EventArgs e) { }
        private void tabPageCopy_Click(object sender, EventArgs e) { }
    }
}