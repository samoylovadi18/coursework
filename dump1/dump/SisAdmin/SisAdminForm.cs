using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace dump
{
    public partial class SisAdminForm : Form
    {
        private bool isPasswordVisible = false;
        private bool isLockDialogOpen = false;
        private Dictionary<string, int> tableColumnsCount = new Dictionary<string, int>();

        // ===================== ДЛЯ БЛОКИРОВКИ =====================
        private Timer inactivityTimer;
        private int inactivityTimeSeconds = 60; // Время бездействия в секундах
        private DateTime lastActivityTime;
        private bool isLocked = false;

        public SisAdminForm()
        {
            InitializeComponent();
            InitializeRestoreFeature();
            InitializeImportExportFeature();
            InitializeSecurityFeature(); // Добавляем инициализацию блокировки
        }

        // ===================== БЛОКИРОВКА СИСТЕМЫ =====================

        private void InitializeSecurityFeature()
        {
            // Настройка чекбокса
            if (chkAutoLock != null)
            {
                chkAutoLock.Text = "Включить блокировку при бездействии";
                chkAutoLock.CheckedChanged += ChkAutoLock_CheckedChanged;
            }

            // Настройка NumericUpDown для времени
            if (numInactivityTime != null)
            {
                numInactivityTime.Minimum = 1;
                numInactivityTime.Maximum = 3600;
                numInactivityTime.Value = inactivityTimeSeconds;
                numInactivityTime.Enabled = false; // По умолчанию выключено
                numInactivityTime.ValueChanged += NumInactivityTime_ValueChanged;
            }

            // Создаем таймер
            inactivityTimer = new Timer();
            inactivityTimer.Interval = 1000; // Проверяем каждую секунду
            inactivityTimer.Tick += InactivityTimer_Tick;

            // Подписываемся на события активности
            this.MouseMove += OnUserActivity;
            this.KeyPress += OnUserActivity;
            this.Click += OnUserActivity;

            // Записываем время последней активности
            lastActivityTime = DateTime.Now;
        }

        private void ChkAutoLock_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAutoLock.Checked)
            {
                // Если чекбокс включен - активируем таймер
                inactivityTimer.Start();
                if (numInactivityTime != null) numInactivityTime.Enabled = true;
                lastActivityTime = DateTime.Now;
                LogMessage($"Блокировка включена. Время бездействия: {inactivityTimeSeconds} сек.");
            }
            else
            {
                // Если чекбокс выключен - останавливаем таймер
                inactivityTimer.Stop();
                if (numInactivityTime != null) numInactivityTime.Enabled = false;
                LogMessage("Блокировка выключена.");
            }
        }

        private void NumInactivityTime_ValueChanged(object sender, EventArgs e)
        {
            if (numInactivityTime != null)
            {
                inactivityTimeSeconds = (int)numInactivityTime.Value;
                LogMessage($"Время бездействия изменено на {inactivityTimeSeconds} сек.");
            }
        }

        private void OnUserActivity(object sender, EventArgs e)
        {
            if (!isLocked)
            {
                lastActivityTime = DateTime.Now;
            }
        }

        private void InactivityTimer_Tick(object sender, EventArgs e)
        {
            // Проверяем только если блокировка включена и форма не заблокирована
            if (chkAutoLock != null && chkAutoLock.Checked && !isLocked)
            {
                TimeSpan inactiveDuration = DateTime.Now - lastActivityTime;

                if (inactiveDuration.TotalSeconds >= inactivityTimeSeconds)
                {
                    LockSystem();
                }
            }
        }

        private void LockSystem()
        {
            if (isLockDialogOpen) return;
            isLockDialogOpen = true;
            isLocked = true;

            this.Invoke(new Action(() =>
            {
                Form lockDialog = new Form();
                lockDialog.Text = "Блокировка системы";
                lockDialog.Size = new Size(400, 250);
                lockDialog.StartPosition = FormStartPosition.CenterScreen;
                lockDialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                lockDialog.MaximizeBox = false;
                lockDialog.MinimizeBox = false;
                lockDialog.TopMost = true;

                Label lblMessage = new Label();
                lblMessage.Text = $"Система заблокирована из-за бездействия ({inactivityTimeSeconds} сек.)\nВведите пароль для разблокировки:";
                lblMessage.Location = new Point(20, 20);
                lblMessage.Size = new Size(360, 60);
                lblMessage.TextAlign = ContentAlignment.MiddleCenter;

                Label lblUser = new Label();
                lblUser.Text = $"Пользователь: {CurrentUser.FIO}";
                lblUser.Location = new Point(20, 90);
                lblUser.Size = new Size(360, 25);
                lblUser.TextAlign = ContentAlignment.MiddleCenter;
                lblUser.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Bold);

                TextBox txtPassword = new TextBox();
                txtPassword.Location = new Point(100, 130);
                txtPassword.Size = new Size(200, 20);
                txtPassword.UseSystemPasswordChar = true;

                Button btnUnlock = new Button();
                btnUnlock.Text = "Разблокировать";
                btnUnlock.Location = new Point(150, 170);
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
                lockDialog.FormClosed += (s, e) => { isLockDialogOpen = false; isLocked = false; };
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
                lastActivityTime = DateTime.Now; // Сбрасываем время активности
                LogMessage("Система разблокирована.");
            }
            else
            {
                MessageBox.Show("Неверный пароль!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                StringBuilder builder = new StringBuilder();
                foreach (byte b in hash)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }

        // ===================== ВОССТАНОВЛЕНИЕ =====================

        private void InitializeRestoreFeature()
        {
            if (btnRestoreDB != null)
            {
                btnRestoreDB.Text = "Восстановить структуру БД";
                btnRestoreDB.Click += BtnRestoreDB_Click;
                btnRestoreDB.BackColor = Color.DarkSeaGreen;
                btnRestoreDB.FlatStyle = FlatStyle.Flat;
                btnRestoreDB.FlatAppearance.BorderSize = 1;
                btnRestoreDB.FlatAppearance.BorderColor = Color.Black;
            }

            if (txtLog != null)
            {
                txtLog.Clear();
                txtLog.ReadOnly = true;
                txtLog.BackColor = Color.WhiteSmoke;
            }
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
                LogMessage("Начало восстановления структуры БД...");

                using (MySqlConnection conn = SettingsBD.GetConnection())
                {
                    conn.Open();
                    LogMessage("Подключение успешно.");

                    using (MySqlCommand cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 0;", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    LogMessage("Удаление таблиц...");
                    DropAllTables(conn);

                    LogMessage("Создание таблиц...");
                    CreateAllTables(conn);

                    using (MySqlCommand cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 1;", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                LogMessage("Восстановление успешно завершено!");
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

            foreach (string table in tables)
            {
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand($"DROP TABLE IF EXISTS `{table}`;", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"Ошибка при удалении {table}: {ex.Message}");
                }
            }
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
            }

            using (MySqlCommand cmd = new MySqlCommand(@"
                INSERT INTO `roles` (`id_role`, `role_name`) VALUES 
                (1, 'manager'), (2, 'director'), (3, 'admin');", conn))
            {
                cmd.ExecuteNonQuery();
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
            }

            using (MySqlCommand cmd = new MySqlCommand(@"
                INSERT INTO `order_statuses` (`id_status`, `status_name`) VALUES 
                (1, 'В обработке'), (2, 'Принят'), (3, 'В приготовлении'),
                (4, 'Готов'), (5, 'В пути'), (6, 'Доставлен'), (7, 'Отменён');", conn))
            {
                cmd.ExecuteNonQuery();
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
            }

            using (MySqlCommand cmd = new MySqlCommand(@"
                INSERT INTO `status_certificates` (`id_status_certificate`, `name`) VALUES 
                (1, 'Активен'), (2, 'Использован'), (3, 'Возвращён');", conn))
            {
                cmd.ExecuteNonQuery();
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
            }

            // orders
            using (MySqlCommand cmd = new MySqlCommand(@"
                CREATE TABLE IF NOT EXISTS `orders` (
                    `id_order` INT NOT NULL AUTO_INCREMENT,
                    `order_number` VARCHAR(20) NOT NULL,
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
                    UNIQUE KEY `order_number` (`order_number`),
                    KEY `id_status` (`id_status`),
                    CONSTRAINT `orders_ibfk_1` FOREIGN KEY (`id_status`) REFERENCES `order_statuses` (`id_status`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;", conn))
            {
                cmd.ExecuteNonQuery();
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
            }
        }

        private void LogMessage(string message)
        {
            if (txtLog == null) return;

            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() => LogMessage(message)));
                return;
            }

            txtLog.AppendText($"{DateTime.Now:HH:mm:ss} - {message}{Environment.NewLine}");
            txtLog.ScrollToCaret();
        }

        // ===================== ИМПОРТ/ЭКСПОРТ =====================

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
                using (MySqlConnection conn = SettingsBD.GetConnection())
                {
                    conn.Open();
                    DataTable schema = conn.GetSchema("Tables");

                    if (cmbTables != null) cmbTables.Items.Clear();
                    if (cmbExportTables != null) cmbExportTables.Items.Clear();

                    foreach (DataRow row in schema.Rows)
                    {
                        string tableName = row["TABLE_NAME"].ToString();
                        if (!tableName.StartsWith("mysql") && !tableName.StartsWith("information_schema"))
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
                MessageBox.Show($"Ошибка: {ex.Message}");
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

                using (MySqlConnection conn = SettingsBD.GetConnection())
                {
                    conn.Open();

                    // ОТКЛЮЧАЕМ ПРОВЕРКУ ВНЕШНИХ КЛЮЧЕЙ
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

                    // ВКЛЮЧАЕМ ОБРАТНО ПРОВЕРКУ ВНЕШНИХ КЛЮЧЕЙ
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
            using (MySqlConnection conn = SettingsBD.GetConnection())
            {
                conn.Open();

                // Принудительно устанавливаем кодировку для соединения
                using (MySqlCommand cmd = new MySqlCommand("SET NAMES utf8mb4;", conn))
                {
                    cmd.ExecuteNonQuery();
                }

                string query = $"SELECT * FROM `{tableName}`";
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                StringBuilder sb = new StringBuilder();

                // Заголовки (разделитель ;)
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    sb.Append($"{dt.Columns[i].ColumnName}");
                    if (i < dt.Columns.Count - 1) sb.Append(";");
                }
                sb.AppendLine();

                // Данные
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

                // Сохраняем в UTF-8 с BOM
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

        // ===================== ЗАГЛУШКИ ДЛЯ ДРУГИХ СОБЫТИЙ =====================

        private void SisAdminForm_Load(object sender, EventArgs e) { }
        private void btnTestConnection_Click(object sender, EventArgs e) { }
        private void btnSave_Click(object sender, EventArgs e) { }
        private void tabPageSecure_Click(object sender, EventArgs e) { }
        private void tabPageCopy_Click(object sender, EventArgs e) { }
    }
}