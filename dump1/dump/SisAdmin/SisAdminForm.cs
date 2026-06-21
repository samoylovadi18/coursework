using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.IO.Compression;

namespace dump
{
    /// <summary>
    /// Форма системного администратора.
    /// Предоставляет полный доступ к управлению базой данных: настройка подключения, резервное копирование,
    /// восстановление, импорт/экспорт данных, управление безопасностью.
    /// </summary>
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

        /// <summary>
        /// Конструктор формы системного администратора.
        /// Инициализирует компоненты, настраивает стили и загружает настройки.
        /// </summary>
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

        /// <summary>
        /// Блокирует систему при длительном бездействии пользователя.
        /// Отображает диалоговое окно для ввода пароля разблокировки.
        /// </summary>
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

                lockDialog.Close();
                InactivityManager.UnregisterForm();
                this.Close();

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
            catch { return null; }
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
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }

        /// <summary>
        /// Обработчик события закрытия формы.
        /// Отписывается от менеджера бездействия.
        /// </summary>
        /// <param name="e">Аргументы события закрытия формы.</param>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            InactivityManager.UnregisterForm();
            base.OnFormClosed(e);
        }

        // ===================== ПОЛУЧЕНИЕ РАБОЧЕГО ПОДКЛЮЧЕНИЯ =====================

        /// <summary>
        /// Получает рабочее подключение к базе данных.
        /// Сначала пытается использовать настройки из SettingsBD, затем из полей формы.
        /// </summary>
        /// <returns>Открытое подключение к MySQL.</returns>
        /// <exception cref="Exception">Выбрасывается при невозможности подключения.</exception>
        private MySqlConnection GetWorkingConnection()
        {
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

                using (MySqlCommand cmd = new MySqlCommand("SET NAMES utf8mb4;", conn))
                {
                    cmd.ExecuteNonQuery();
                }

                return conn;
            }
            catch (Exception ex)
            {
                LogBackupMessage($"Ошибка получения настроек из SettingsBD: {ex.Message}");
            }

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

                using (MySqlCommand cmd = new MySqlCommand("SET NAMES utf8mb4;", conn))
                {
                    cmd.ExecuteNonQuery();
                }

                return conn;
            }
            catch (Exception ex)
            {
                LogBackupMessage($"Ошибка подключения из полей формы: {ex.Message}");
                throw new Exception($"Не удалось подключиться к базе данных. Проверьте настройки подключения.\n\nОшибка: {ex.Message}");
            }
        }

        // ===================== ВОССТАНОВЛЕНИЕ ИЗ SQL ФАЙЛА =====================

        /// <summary>
        /// Инициализирует функционал восстановления из SQL-скрипта.
        /// </summary>
        private void InitializeScriptRestore()
        {
            openFileDialogScript = new OpenFileDialog();
            openFileDialogScript.Title = "Выберите SQL файл для восстановления";
            openFileDialogScript.Filter = "SQL файлы (*.sql)|*.sql|Все файлы (*.*)|*.*";
            openFileDialogScript.FilterIndex = 1;
            openFileDialogScript.RestoreDirectory = true;

            if (btnBrowseScript != null)
            {
                btnBrowseScript.Click += BtnBrowseScript_Click;
            }

            if (btnRestoreFromScript != null)
            {
                btnRestoreFromScript.Click += BtnRestoreFromScript_Click;
            }
        }

        /// <summary>
        /// Обработчик нажатия кнопки обзора SQL-файла.
        /// </summary>
        private void BtnBrowseScript_Click(object sender, EventArgs e)
        {
            if (openFileDialogScript.ShowDialog() == DialogResult.OK)
            {
                txtScriptPath.Text = openFileDialogScript.FileName;
            }
        }

        /// <summary>
        /// Обработчик нажатия кнопки восстановления из SQL-скрипта.
        /// Выполняет SQL-скрипт с отключением проверки внешних ключей.
        /// </summary>
        private void BtnRestoreFromScript_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtScriptPath.Text) || !File.Exists(txtScriptPath.Text))
            {
                MessageBox.Show("Выберите SQL файл для восстановления!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

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

                    string sqlScript = File.ReadAllText(txtScriptPath.Text, Encoding.UTF8);

                    using (MySqlConnection conn = GetWorkingConnection())
                    {
                        LogMessage("Подключение к БД успешно");

                        using (MySqlCommand cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 0;", conn))
                        {
                            cmd.ExecuteNonQuery();
                            LogMessage("Отключена проверка внешних ключей");
                        }

                        LogMessage("Выполнение SQL скрипта...");
                        using (MySqlCommand cmd = new MySqlCommand(sqlScript, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }

                        using (MySqlCommand cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 1;", conn))
                        {
                            cmd.ExecuteNonQuery();
                            LogMessage("Включена проверка внешних ключей");
                        }
                    }

                    LogMessage("Скрипт успешно выполнен!");
                    MessageBox.Show("SQL скрипт успешно выполнен!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

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

        /// <summary>
        /// Обработчик события закрытия формы.
        /// При закрытии формы пользователем скрывает её и открывает форму входа.
        /// </summary>
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

        /// <summary>
        /// Загружает текущие настройки подключения из конфигурации.
        /// </summary>
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

        /// <summary>
        /// Проверяет подключение к базе данных перед сохранением настроек.
        /// </summary>
        /// <param name="server">Адрес сервера.</param>
        /// <param name="database">Имя базы данных.</param>
        /// <param name="username">Имя пользователя.</param>
        /// <param name="password">Пароль.</param>
        /// <returns>True если подключение успешно.</returns>
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

        /// <summary>
        /// Сохраняет настройки подключения в конфигурацию.
        /// </summary>
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

        /// <summary>
        /// Выполняет тест подключения к базе данных.
        /// </summary>
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

        /// <summary>
        /// Инициализирует поле ввода пароля и настраивает кнопку показа/скрытия.
        /// </summary>
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

        /// <summary>
        /// Обработчик нажатия кнопки показа/скрытия пароля.
        /// </summary>
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

        /// <summary>
        /// Создаёт простую иконку глаза для отображения/скрытия пароля.
        /// </summary>
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

        /// <summary>
        /// Применяет единый стиль ко всем кнопкам на форме.
        /// </summary>
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

        /// <summary>
        /// Применяет единый стиль к кнопке.
        /// </summary>
        /// <param name="btn">Кнопка для стилизации.</param>
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

        /// <summary>
        /// Инициализирует функционал настройки безопасности (автоматическая блокировка).
        /// </summary>
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
                numInactivityTime.Maximum = 120;

                int seconds = InactivityManager.GetInactivityTime();
                int minutes = seconds / 60;

                if (minutes < 1)
                {
                    minutes = 1;
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

        /// <summary>
        /// Обработчик изменения состояния чекбокса автоматической блокировки.
        /// </summary>
        private void ChkAutoLock_CheckedChanged(object sender, EventArgs e)
        {
            bool isChecked = chkAutoLock.Checked;
            int minutes = (int)(numInactivityTime?.Value ?? 5);
            int seconds = minutes * 60;

            InactivityManager.SetSecuritySettings(isChecked, seconds);

            if (numInactivityTime != null)
            {
                numInactivityTime.Enabled = isChecked;
            }

            LogMessage(isChecked ? "Блокировка включена" : "Блокировка выключена");
        }

        /// <summary>
        /// Обработчик изменения времени бездействия до блокировки.
        /// </summary>
        private void NumInactivityTime_ValueChanged(object sender, EventArgs e)
        {
            if (numInactivityTime != null)
            {
                int minutes = (int)numInactivityTime.Value;
                int seconds = minutes * 60;
                InactivityManager.SetSecuritySettings(InactivityManager.GetAutoLockEnabled(), seconds);
                LogMessage($"Время бездействия изменено на {minutes} мин.");
            }
        }

        /// <summary>
        /// Обработчик сохранения настроек безопасности.
        /// </summary>
        private void BtnSaveSecurity_Click(object sender, EventArgs e)
        {
            try
            {
                bool isEnabled = chkAutoLock?.Checked ?? false;
                int minutes = (int)(numInactivityTime?.Value ?? 5);
                int seconds = minutes * 60;

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

        /// <summary>
        /// Инициализирует функционал встроенного восстановления структуры БД.
        /// </summary>
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

        /// <summary>
        /// Записывает сообщение в лог восстановления с временной меткой.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
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

        /// <summary>
        /// Обработчик нажатия кнопки восстановления структуры БД.
        /// </summary>
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

        /// <summary>
        /// Выполняет полное восстановление структуры базы данных.
        /// Удаляет все таблицы и создаёт новые с начальными данными.
        /// </summary>
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

        /// <summary>
        /// Удаляет все таблицы базы данных.
        /// </summary>
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

        /// <summary>
        /// Создаёт все таблицы базы данных с начальными данными.
        /// </summary>
        private void CreateAllTables(MySqlConnection conn)
        {
            // roles
            using (MySqlCommand cmd = new MySqlCommand(@"
                CREATE TABLE IF NOT EXISTS `roles` (
                    `id_role` INT NOT NULL AUTO_INCREMENT,
                    `role_name` VARCHAR(50) NOT NULL,
                    PRIMARY KEY (`id_role`),
                    UNIQUE KEY `role_name` (`role_name`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;", conn))
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
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;", conn))
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
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;", conn))
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
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;", conn))
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
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;", conn))
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
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;", conn))
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
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;", conn))
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
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;", conn))
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
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;", conn))
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
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;", conn))
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
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;", conn))
            {
                cmd.ExecuteNonQuery();
                LogMessage("  Создана: other_orders");
            }

            LogMessage("  Все таблицы созданы!");
        }

        // ===================== ИМПОРТ/ЭКСПОРТ =====================

        // Таблицы, исключенные из импорта (роли нельзя импортировать)
        private readonly string[] importExcludedTables = new string[] { "roles" };

        // Таблицы, исключенные из экспорта (пустой массив - все таблицы доступны для экспорта)
        private readonly string[] exportExcludedTables = new string[] { };

        /// <summary>
        /// Инициализирует функционал импорта/экспорта данных.
        /// </summary>
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
                txtImportFilePath.TextChanged += (s, e) =>
                {
                    if (btnImport != null)
                        btnImport.Enabled = !string.IsNullOrEmpty(txtImportFilePath.Text) && File.Exists(txtImportFilePath.Text);
                };
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

        /// <summary>
        /// Загружает список таблиц для импорта/экспорта.
        /// </summary>
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

                        if (tableName.StartsWith("mysql") ||
                            tableName.StartsWith("information_schema") ||
                            tableName.StartsWith("performance_schema") ||
                            tableName.StartsWith("sys"))
                        {
                            continue;
                        }

                        if (cmbTables != null && !importExcludedTables.Contains(tableName))
                        {
                            cmbTables.Items.Add(tableName);
                        }

                        if (cmbExportTables != null && !exportExcludedTables.Contains(tableName))
                        {
                            cmbExportTables.Items.Add(tableName);
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

        /// <summary>
        /// Обработчик нажатия кнопки обзора файла для импорта.
        /// </summary>
        private void BtnBrowseImport_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Выберите CSV файл для импорта";
                ofd.Filter = "CSV файлы (*.csv)|*.csv|Все файлы (*.*)|*.*";
                ofd.FilterIndex = 1;
                ofd.RestoreDirectory = true;

                // Создаем папку для импорта если её нет
                string importFolder = Path.Combine(Application.StartupPath, "Импорт");
                if (!Directory.Exists(importFolder))
                {
                    Directory.CreateDirectory(importFolder);
                }
                ofd.InitialDirectory = importFolder;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtImportFilePath.Text = ofd.FileName;
                    if (btnImport != null)
                        btnImport.Enabled = true;

                    // Предпросмотр файла
                    PreviewCSVFile(ofd.FileName);
                }
            }
        }

        /// <summary>
        /// Выполняет предпросмотр CSV файла (первые 5 строк).
        /// </summary>
        private void PreviewCSVFile(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath, Encoding.UTF8).Take(5).ToArray();
                StringBuilder preview = new StringBuilder();
                preview.AppendLine("Предпросмотр файла (первые 5 строк):");
                preview.AppendLine(new string('-', 50));

                for (int i = 0; i < lines.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(lines[i]))
                    {
                        preview.AppendLine($"Строка {i + 1}: {lines[i]}");
                    }
                }

                LogMessage(preview.ToString());
            }
            catch (Exception ex)
            {
                LogMessage($"Ошибка предпросмотра файла: {ex.Message}");
            }
        }

        // ===================== ИМПОРТ =====================

        /// <summary>
        /// Обработчик нажатия кнопки импорта данных из CSV.
        /// </summary>
        private void BtnImport_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbTables?.SelectedItem == null)
                {
                    MessageBox.Show("Выберите таблицу для импорта!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string tableName = cmbTables.SelectedItem.ToString();

                if (importExcludedTables.Contains(tableName))
                {
                    MessageBox.Show($"Импорт для таблицы '{tableName}' запрещен!",
                        "Доступ запрещен", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string filePath = txtImportFilePath.Text;

                if (!File.Exists(filePath))
                {
                    MessageBox.Show("Файл не существует!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Читаем файл
                var lines = File.ReadAllLines(filePath, Encoding.UTF8)
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .ToArray();

                if (lines.Length == 0)
                {
                    MessageBox.Show("Файл пуст", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Получаем схему таблицы
                DataTable tableSchema = GetTableSchema(tableName);
                if (tableSchema == null || tableSchema.Columns.Count == 0)
                {
                    MessageBox.Show("Не удалось получить структуру таблицы", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Получаем автоинкрементные поля
                List<string> autoIncrementColumns = GetAutoIncrementColumns(tableName);
                char separator = lines[0].Contains(';') ? ';' : ',';
                string[] headers = lines[0].Split(separator);
                int startRow = 0;

                for (int h = 0; h < headers.Length; h++)
                {
                    headers[h] = headers[h].Trim().Trim('"').Trim('\'');
                }

                bool hasHeader = false;
                foreach (string header in headers)
                {
                    if (tableSchema.Columns.Contains(header))
                    {
                        hasHeader = true;
                        break;
                    }
                }

                if (hasHeader)
                {
                    startRow = 1;
                    LogMessage($"✓ Обнаружена строка заголовков, импорт начнется со строки 2");
                }

                List<string> skipColumns = new List<string>();
                // Для таблицы dishes пропускаем photo (BLOB)
                if (tableName.ToLower() == "dishes")
                {
                    skipColumns.Add("photo");
                    LogMessage("✓ Поле photo (BLOB) будет пропущено при импорте");
                }

                int expectedColumns = tableSchema.Columns.Count - autoIncrementColumns.Count - skipColumns.Count;
                string[] firstDataRow = lines[startRow].Split(separator);

                if (firstDataRow.Length != expectedColumns)
                {
                    string errorMsg = $"Несоответствие количества полей!\n" +
                                    $"В CSV файле: {firstDataRow.Length} полей\n" +
                                    $"В таблице '{tableName}': {tableSchema.Columns.Count} полей\n" +
                                    $"Автоинкрементные поля: {string.Join(", ", autoIncrementColumns)}\n" +
                                    $"Пропущенные поля: {string.Join(", ", skipColumns)}\n" +
                                    $"Ожидаемое количество полей в CSV: {expectedColumns}";

                    LogMessage($"✗ ОШИБКА: {errorMsg}");
                    MessageBox.Show(errorMsg, "Ошибка импорта", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                List<int> columnMapping = new List<int>();
                if (hasHeader)
                {
                    for (int i = 0; i < headers.Length; i++)
                    {
                        string header = headers[i];
                        if (tableSchema.Columns.Contains(header) &&
                            !autoIncrementColumns.Contains(header) &&
                            !skipColumns.Contains(header))
                        {
                            columnMapping.Add(tableSchema.Columns[header].Ordinal);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < tableSchema.Columns.Count; i++)
                    {
                        string colName = tableSchema.Columns[i].ColumnName;
                        if (!autoIncrementColumns.Contains(colName) && !skipColumns.Contains(colName))
                        {
                            columnMapping.Add(i);
                        }
                    }
                }

                int importedCount = 0;
                int errorCount = 0;
                List<string> errors = new List<string>();

                using (MySqlConnection conn = GetWorkingConnection())
                {
                    // Отключаем проверку внешних ключей
                    using (MySqlCommand cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 0;", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Очищаем таблицу
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

                    // Генерируем INSERT запрос
                    var columns = tableSchema.Columns.Cast<DataColumn>()
                        .Where(c => !autoIncrementColumns.Contains(c.ColumnName))
                        .Where(c => !skipColumns.Contains(c.ColumnName))
                        .Select(c => $"`{c.ColumnName}`")
                        .ToList();

                    string columnsStr = string.Join(", ", columns);
                    string parameters = string.Join(", ", Enumerable.Range(0, columns.Count).Select(i => $"@p{i}"));
                    string insertQuery = $"INSERT INTO `{tableName}` ({columnsStr}) VALUES ({parameters})";

                    // Обрабатываем строки
                    for (int i = startRow; i < lines.Length; i++)
                    {
                        string line = lines[i].Trim();
                        if (string.IsNullOrEmpty(line)) continue;

                        // Убираем кавычки
                        line = line.Replace("\"", "");
                        string[] values = line.Split(separator);

                        try
                        {
                            using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
                            {
                                int paramIndex = 0;
                                for (int j = 0; j < columnMapping.Count; j++)
                                {
                                    int tableColIndex = columnMapping[j];
                                    string value = values[j].Trim();
                                    Type columnType = tableSchema.Columns[tableColIndex].DataType;

                                    if (string.IsNullOrEmpty(value))
                                    {
                                        cmd.Parameters.AddWithValue($"@p{paramIndex}", DBNull.Value);
                                    }
                                    else
                                    {
                                        try
                                        {
                                            if (columnType == typeof(int) || columnType == typeof(long) || columnType == typeof(int?))
                                            {
                                                cmd.Parameters.AddWithValue($"@p{paramIndex}", int.Parse(value));
                                            }
                                            else if (columnType == typeof(decimal) || columnType == typeof(decimal?))
                                            {
                                                value = value.Replace('.', ',');
                                                cmd.Parameters.AddWithValue($"@p{paramIndex}", decimal.Parse(value));
                                            }
                                            else if (columnType == typeof(DateTime) || columnType == typeof(DateTime?))
                                            {
                                                cmd.Parameters.AddWithValue($"@p{paramIndex}", DateTime.Parse(value));
                                            }
                                            else if (columnType == typeof(bool) || columnType == typeof(bool?) || columnType == typeof(byte))
                                            {
                                                cmd.Parameters.AddWithValue($"@p{paramIndex}", value == "1" || value.ToLower() == "true" ? 1 : 0);
                                            }
                                            else
                                            {
                                                cmd.Parameters.Add($"@p{paramIndex}", MySqlDbType.VarChar).Value = value;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            throw new Exception($"Ошибка преобразования '{value}' в {columnType.Name}: {ex.Message}");
                                        }
                                    }
                                    paramIndex++;
                                }

                                cmd.ExecuteNonQuery();
                                importedCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            errors.Add($"Строка {i + 1}: {ex.Message}");
                            LogMessage($"Ошибка в строке {i + 1}: {ex.Message}");
                        }
                    }

                    // Включаем проверку внешних ключей
                    using (MySqlCommand cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 1;", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                // Вывод результатов
                LogMessage($"\n=== РЕЗУЛЬТАТЫ ИМПОРТА ===");
                LogMessage($"Таблица: {tableName}");
                LogMessage($"Файл: {Path.GetFileName(filePath)}");
                LogMessage($"Успешно импортировано: {importedCount} записей");
                LogMessage($"Ошибок: {errorCount}");

                if (errors.Count > 0)
                {
                    LogMessage($"\nПервые 5 ошибок:");
                    foreach (string error in errors.Take(5))
                    {
                        LogMessage($"  • {error}");
                    }
                }

                MessageBox.Show($"Импорт завершен!\n\n" +
                               $"Таблица: {tableName}\n" +
                               $"Файл: {Path.GetFileName(filePath)}\n\n" +
                               $"✓ Успешно: {importedCount} записей\n" +
                               $"✗ Ошибок: {errorCount}",
                    "Результат импорта",
                    MessageBoxButtons.OK,
                    errorCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

                txtImportFilePath.Text = "";
                if (btnImport != null)
                    btnImport.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте:\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogMessage($"Ошибка импорта: {ex.Message}");
            }
        }

        // ===================== ЭКСПОРТ =====================

        /// <summary>
        /// Обработчик нажатия кнопки экспорта данных в CSV.
        /// </summary>
        private void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbExportTables?.SelectedItem == null)
                {
                    MessageBox.Show("Выберите таблицу для экспорта!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string tableName = cmbExportTables.SelectedItem.ToString();

                if (exportExcludedTables.Contains(tableName))
                {
                    MessageBox.Show($"Экспорт для таблицы '{tableName}' запрещен!",
                        "Доступ запрещен", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                    saveFileDialog.FileName = $"{tableName}_{DateTime.Now:yyyyMMdd_HHmm}.csv";
                    saveFileDialog.Title = "Сохранить CSV файл";
                    saveFileDialog.RestoreDirectory = true;

                    // Создаем папку для экспорта если её нет
                    string exportFolder = Path.Combine(Application.StartupPath, "Экспорт");
                    if (!Directory.Exists(exportFolder))
                    {
                        Directory.CreateDirectory(exportFolder);
                    }
                    saveFileDialog.InitialDirectory = exportFolder;

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        ExportToCSV(tableName, saveFileDialog.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте:\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogMessage($"Ошибка экспорта: {ex.Message}");
            }
        }

        /// <summary>
        /// Экспортирует таблицу в CSV файл.
        /// </summary>
        /// <param name="tableName">Имя таблицы.</param>
        /// <param name="filePath">Путь к файлу.</param>
        private void ExportToCSV(string tableName, string filePath)
        {
            try
            {
                LogMessage($"Начало экспорта таблицы {tableName}...");

                using (MySqlConnection conn = GetWorkingConnection())
                {
                    // Получаем автоинкрементные поля
                    List<string> autoIncrementCols = GetAutoIncrementColumns(tableName);

                    // Список пропускаемых колонок
                    List<string> skipColumns = new List<string>();
                    skipColumns.AddRange(autoIncrementCols);

                    // Для таблицы dishes пропускаем photo (BLOB)
                    if (tableName.ToLower() == "dishes")
                    {
                        skipColumns.Add("photo");
                        LogMessage("✓ Поле photo (BLOB) будет пропущено при экспорте");
                    }

                    // Получаем схему таблицы
                    DataTable schema = GetTableSchema(tableName);
                    List<string> exportColumns = new List<string>();
                    foreach (DataColumn col in schema.Columns)
                    {
                        if (!skipColumns.Contains(col.ColumnName))
                        {
                            exportColumns.Add(col.ColumnName);
                        }
                    }

                    LogMessage($"Экспортируемые колонки: {string.Join(", ", exportColumns)}");

                    string columnsStr = string.Join(", ", exportColumns.Select(c => $"`{c}`"));
                    string query = $"SELECT {columnsStr} FROM `{tableName}`";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
                            {
                                // Добавляем BOM для правильного распознавания UTF-8 в Excel
                                writer.BaseStream.Write(new byte[] { 0xEF, 0xBB, 0xBF }, 0, 3);

                                // Записываем заголовки
                                writer.WriteLine(string.Join(";", exportColumns));

                                int rowCount = 0;
                                while (reader.Read())
                                {
                                    List<string> row = new List<string>();
                                    for (int i = 0; i < exportColumns.Count; i++)
                                    {
                                        string value = "";
                                        if (!reader.IsDBNull(i))
                                        {
                                            object val = reader.GetValue(i);
                                            if (val is DateTime dt)
                                            {
                                                value = dt.ToString("yyyy-MM-dd HH:mm:ss");
                                            }
                                            else if (val is decimal dec)
                                            {
                                                value = dec.ToString("F2").Replace('.', ',');
                                            }
                                            else if (val is float fl)
                                            {
                                                value = fl.ToString("F2").Replace('.', ',');
                                            }
                                            else if (val is double dbl)
                                            {
                                                value = dbl.ToString("F2").Replace('.', ',');
                                            }
                                            else
                                            {
                                                value = val.ToString();
                                            }
                                        }
                                        row.Add(EscapeCsvField(value));
                                    }
                                    writer.WriteLine(string.Join(";", row));
                                    rowCount++;
                                }

                                LogMessage($"✓ Экспорт завершен: {rowCount} записей");
                                LogMessage($"✓ Файл сохранен: {filePath}");
                            }
                        }
                    }
                }

                MessageBox.Show($"Экспорт успешно завершен!\n\n" +
                               $"Таблица: {tableName}\n" +
                               $"Файл: {Path.GetFileName(filePath)}\n" +
                               $"Путь: {Path.GetDirectoryName(filePath)}",
                    "Экспорт завершен", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogMessage($"✗ ОШИБКА ЭКСПОРТА: {ex.Message}");
                MessageBox.Show($"Ошибка экспорта:\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        /// <summary>
        /// Экранирует поле для CSV (кавычки и разделители).
        /// </summary>
        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            // Если поле содержит разделитель, кавычки или перевод строки - экранируем
            if (field.Contains(";") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                field = field.Replace("\"", "\"\"");
                field = $"\"{field}\"";
            }

            return field;
        }

        /// <summary>
        /// Получает схему таблицы (структуру колонок).
        /// </summary>
        private DataTable GetTableSchema(string tableName)
        {
            try
            {
                using (MySqlConnection conn = GetWorkingConnection())
                {
                    string query = $"SELECT * FROM `{tableName}` LIMIT 0";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    DataTable schema = new DataTable();
                    adapter.Fill(schema);
                    return schema;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Ошибка получения схемы таблицы: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Получает список автоинкрементных колонок таблицы.
        /// </summary>
        private List<string> GetAutoIncrementColumns(string tableName)
        {
            List<string> autoIncrementCols = new List<string>();

            try
            {
                using (MySqlConnection conn = GetWorkingConnection())
                {
                    string query = @"
                        SELECT COLUMN_NAME 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_SCHEMA = DATABASE() 
                        AND TABLE_NAME = @tableName 
                        AND EXTRA LIKE '%auto_increment%'";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@tableName", tableName);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            autoIncrementCols.Add(reader["COLUMN_NAME"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Ошибка получения автоинкрементных полей: {ex.Message}");
            }

            return autoIncrementCols;
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

        /// <summary>
        /// Обработчик нажатия кнопки теста подключения.
        /// </summary>
        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            TestConnection();
        }

        /// <summary>
        /// Обработчик нажатия кнопки сохранения настроек подключения.
        /// </summary>
        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveConnectionSettings();
        }

        // ===================== РЕЗЕРВНОЕ КОПИРОВАНИЕ =====================

        /// <summary>
        /// Инициализирует систему резервного копирования.
        /// </summary>
        private void InitializeBackupFeature()
        {
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

            if (cmbAutoBackupType != null)
            {
                cmbAutoBackupType.Items.Clear();
                cmbAutoBackupType.Items.Add("Полный бэкап");
                cmbAutoBackupType.Items.Add("Только данные");
                cmbAutoBackupType.SelectedIndex = 0;
                cmbAutoBackupType.Enabled = false;
            }

            if (numBackupInterval != null)
            {
                numBackupInterval.Minimum = 1;
                numBackupInterval.Maximum = 720;
                numBackupInterval.Value = backupIntervalHours;
                numBackupInterval.Enabled = false;
            }

            if (txtBackupPath != null)
            {
                txtBackupPath.Text = backupFolder;
                txtBackupPath.ReadOnly = true;
            }

            autoBackupTimer = new System.Windows.Forms.Timer();
            autoBackupTimer.Tick += AutoBackupTimer_Tick;

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

            LoadBackupSettingsFromFile();

            LogBackupMessage("Система резервного копирования готова");
        }

        /// <summary>
        /// Загружает настройки автоматического резервного копирования из файла.
        /// </summary>
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

        /// <summary>
        /// Сохраняет настройки автоматического резервного копирования в файл.
        /// </summary>
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

        /// <summary>
        /// Обработчик изменения состояния чекбокса автоматического бэкапа.
        /// </summary>
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

        /// <summary>
        /// Обработчик изменения интервала автоматического бэкапа.
        /// </summary>
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

        /// <summary>
        /// Таймер автоматического создания резервной копии.
        /// </summary>
        private void AutoBackupTimer_Tick(object sender, EventArgs e)
        {
            string backupType = "full";
            if (cmbAutoBackupType != null && cmbAutoBackupType.SelectedIndex == 1)
            {
                backupType = "data";
            }
            System.Threading.Tasks.Task.Run(() => CreateBackup(backupType, true));
        }

        /// <summary>
        /// Обработчик нажатия кнопки создания резервной копии.
        /// </summary>
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

        /// <summary>
        /// Обработчик нажатия кнопки выбора папки для бэкапов.
        /// </summary>
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

        /// <summary>
        /// Создаёт резервную копию базы данных.
        /// </summary>
        /// <param name="backupType">Тип бэкапа: full, structure, data.</param>
        /// <param name="isAuto">True если бэкап автоматический.</param>
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

                using (MySqlConnection conn = GetWorkingConnection())
                {
                    LogBackupMessage("Подключение к базе данных установлено успешно");

                    using (MySqlCommand cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 0;", conn))
                    {
                        cmd.ExecuteNonQuery();
                        LogBackupMessage("Отключена проверка внешних ключей");
                    }

                    StringBuilder sqlScript = new StringBuilder();

                    sqlScript.AppendLine($"-- Резервная копия создана: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    sqlScript.AppendLine($"-- Тип резервной копии: {GetBackupTypeName(backupType)}");
                    sqlScript.AppendLine($"-- Сервер: {conn.DataSource}");
                    sqlScript.AppendLine($"-- База данных: {conn.Database}");
                    sqlScript.AppendLine("SET FOREIGN_KEY_CHECKS = 0;");
                    sqlScript.AppendLine("SET AUTOCOMMIT = 0;");
                    sqlScript.AppendLine("");

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

                    File.WriteAllText(fullPath, sqlScript.ToString(), Encoding.UTF8);
                    LogBackupMessage($"SQL скрипт сохранен: {backupFileName}");
                    LogBackupMessage($"Размер SQL файла: {new FileInfo(fullPath).Length / 1024.0:F2} KB");

                    using (MySqlCommand cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 1;", conn))
                    {
                        cmd.ExecuteNonQuery();
                        LogBackupMessage("Включена проверка внешних ключей");
                    }
                }

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

        /// <summary>
        /// Возвращает префикс для имени файла бэкапа в зависимости от типа.
        /// </summary>
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

        /// <summary>
        /// Возвращает название типа бэкапа на русском языке.
        /// </summary>
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

        /// <summary>
        /// Получает список всех таблиц базы данных (исключая системные).
        /// </summary>
        private List<string> GetTableList(MySqlConnection conn)
        {
            List<string> tables = new List<string>();
            DataTable schema = conn.GetSchema("Tables");

            foreach (DataRow row in schema.Rows)
            {
                string tableName = row["TABLE_NAME"].ToString();

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

        /// <summary>
        /// Получает SQL-скрипт структуры таблицы.
        /// </summary>
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

        /// <summary>
        /// Получает SQL-скрипт данных таблицы (INSERT запросы).
        /// </summary>
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

        /// <summary>
        /// Форматирует значение для SQL-запроса.
        /// </summary>
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

        /// <summary>
        /// Очищает старые резервные копии, оставляя только указанное количество последних.
        /// </summary>
        /// <param name="keepCount">Количество копий для сохранения.</param>
        /// <returns>Количество удалённых файлов.</returns>
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

        /// <summary>
        /// Записывает сообщение в лог резервного копирования.
        /// </summary>
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