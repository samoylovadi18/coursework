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
using System.Configuration;

namespace dump
{
    /// <summary>
    /// Форма для восстановления структуры БД и импорта данных из CSV
    /// Доступна только для администраторов и специальной учётной записи admin/admin
    /// </summary>
    public partial class ImportRestoreForm : Form
    {
        /// <summary>
        /// Список таблиц базы данных для выбора при импорте
        /// </summary>
        private List<string> tableList = new List<string>();

        /// <summary>
        /// Словарь для хранения количества столбцов в каждой таблице
        /// </summary>
        private Dictionary<string, int> tableColumnsCount = new Dictionary<string, int>();

        /// <summary>
        /// Конструктор формы импорта и восстановления
        /// </summary>
        public ImportRestoreForm()
        {
            InitializeComponent();
            InitializeForm();
            LoadTableList();
            CheckAccess();
            StyleButtons();
        }

        /// <summary>
        /// Инициализация формы
        /// </summary>
        private void InitializeForm()
        {
            // Настройка кнопки восстановления
            btnRestore.Text = "Восстановить структуру БД";
            btnRestore.Click += BtnRestore_Click;

            // Настройка кнопки обзора
            btnBrowse.Text = "Обзор...";
            btnBrowse.Click += BtnBrowse_Click;

            // Настройка кнопки импорта
            btnImport.Text = "Импортировать";
            btnImport.Enabled = false; // Отключена, пока не выбран файл
            btnImport.Click += BtnImport_Click;

            // Настройка поля пути к файлу
            txtFilePath.ReadOnly = true;
            txtFilePath.TextChanged += (s, e) => btnImport.Enabled = !string.IsNullOrEmpty(txtFilePath.Text);

            // Настройка выпадающего списка таблиц
            cmbTables.DropDownStyle = ComboBoxStyle.DropDownList;

            // Настройка заголовков групп
            grpRestore.Text = "Восстановление базы данных";
            grpImport.Text = "Импорт данных";

            // Настройка меток
            lblTable.Text = "Таблица:";
            lblFile.Text = "Файл:";
        }

        /// <summary>
        /// Настройка стилей кнопок
        /// </summary>
        private void StyleButtons()
        {
            StyleButton(btnRestore);
            StyleButton(btnBrowse);
            StyleButton(btnImport);
        }

        /// <summary>
        /// Применение стиля к кнопке
        /// </summary>
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

        /// <summary>
        /// Проверка доступа к форме
        /// </summary>
        private void CheckAccess()
        {
            // Если пользователь не авторизован (специальный вход admin/admin)
            if (CurrentUser.UserId == 0 && CurrentUser.Username == "admin")
            {
                // Разрешаем доступ
                return;
            }

            // Проверяем, является ли пользователь администратором
            if (CurrentUser.RoleId != 3)
            {
                MessageBox.Show("У вас нет прав для доступа к этой форме!\nДоступ только для администраторов.",
                    "Доступ запрещен", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        /// <summary>
        /// Загрузка списка таблиц из базы данных
        /// </summary>
        private void LoadTableList()
        {
            try
            {
                using (MySqlConnection conn = SettingsBD.GetConnection())
                {
                    conn.Open();
                    DataTable schema = conn.GetSchema("Tables");

                    cmbTables.Items.Clear();
                    tableList.Clear();
                    tableColumnsCount.Clear();

                    foreach (DataRow row in schema.Rows)
                    {
                        string tableName = row["TABLE_NAME"].ToString();
                        // Исключаем системные таблицы
                        if (!tableName.StartsWith("mysql") && !tableName.StartsWith("information_schema"))
                        {
                            cmbTables.Items.Add(tableName);
                            tableList.Add(tableName);

                            // Получаем количество столбцов в таблице
                            int columnCount = GetTableColumnsCount(conn, tableName);
                            tableColumnsCount[tableName] = columnCount;
                        }
                    }

                    if (cmbTables.Items.Count > 0)
                        cmbTables.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке списка таблиц: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Получение количества столбцов в таблице
        /// </summary>
        private int GetTableColumnsCount(MySqlConnection conn, string tableName)
        {
            try
            {
                DataTable schema = conn.GetSchema("Columns", new string[] { null, null, tableName });
                return schema.Rows.Count;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Обработчик кнопки восстановления структуры БД
        /// </summary>
        private void BtnRestore_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Внимание! Восстановление структуры базы данных приведет к:\n" +
                "• Удалению всех существующих таблиц\n" +
                "• Созданию новой структуры (таблицы, связи, индексы)\n" +
                "• Все данные будут потеряны!\n\n" +
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
        /// Восстановление структуры базы данных
        /// </summary>
        private void RestoreDatabaseStructure()
        {
            try
            {
                using (MySqlConnection conn = SettingsBD.GetConnection())
                {
                    conn.Open();

                    // Отключаем проверку внешних ключей
                    using (MySqlCommand cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 0;", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Удаляем существующие таблицы
                    DropAllTables(conn);

                    // Создаем все таблицы
                    CreateAllTables(conn);

                    // Включаем проверку внешних ключей
                    using (MySqlCommand cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 1;", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Структура базы данных успешно восстановлена!\n\n" +
                                  "Созданы следующие таблицы:\n" +
                                  "• roles (роли пользователей)\n" +
                                  "• users (пользователи)\n" +
                                  "• order_statuses (статусы заказов)\n" +
                                  "• categories (категории блюд)\n" +
                                  "• dishes (блюда)\n" +
                                  "• status_certificates (статусы сертификатов)\n" +
                                  "• certificates (сертификаты)\n" +
                                  "• present (подарки)\n" +
                                  "• orders (заказы)\n" +
                                  "• order_dish (связь заказов и блюд)\n" +
                                  "• other_orders (доп. таблица заказов)",
                        "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Обновляем список таблиц
                    LoadTableList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при восстановлении структуры БД: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Удаление всех таблиц
        /// </summary>
        private void DropAllTables(MySqlConnection conn)
        {
            // Удаляем в обратном порядке (сначала дочерние, потом родительские)
            string[] tables = {
                "order_dish",
                "other_orders",
                "orders",
                "certificates",
                "dishes",
                "users",
                "present",
                "categories",
                "order_statuses",
                "status_certificates",
                "roles"
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
                    Console.WriteLine($"Ошибка при удалении таблицы {table}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Создание всех таблиц базы данных
        /// </summary>
        private void CreateAllTables(MySqlConnection conn)
        {
            // 1. Таблица roles
            using (MySqlCommand cmd = new MySqlCommand(@"
                CREATE TABLE IF NOT EXISTS `roles` (
                    `id_role` INT NOT NULL AUTO_INCREMENT,
                    `role_name` VARCHAR(50) NOT NULL,
                    PRIMARY KEY (`id_role`),
                    UNIQUE KEY `role_name` (`role_name`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;", conn))
            {
                cmd.ExecuteNonQuery();
            }

            // Добавляем начальные данные в roles
            using (MySqlCommand cmd = new MySqlCommand(@"
                INSERT INTO `roles` (`id_role`, `role_name`) VALUES 
                (1, 'manager'),
                (2, 'director'),
                (3, 'admin');", conn))
            {
                cmd.ExecuteNonQuery();
            }

            // 2. Таблица users
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
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;", conn))
            {
                cmd.ExecuteNonQuery();
            }

            // 3. Таблица order_statuses
            using (MySqlCommand cmd = new MySqlCommand(@"
                CREATE TABLE IF NOT EXISTS `order_statuses` (
                    `id_status` INT NOT NULL AUTO_INCREMENT,
                    `status_name` VARCHAR(255) DEFAULT NULL,
                    PRIMARY KEY (`id_status`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;", conn))
            {
                cmd.ExecuteNonQuery();
            }

            // Добавляем начальные данные в order_statuses
            using (MySqlCommand cmd = new MySqlCommand(@"
                INSERT INTO `order_statuses` (`id_status`, `status_name`) VALUES 
                (1, 'В обработке'),
                (2, 'Принят'),
                (3, 'В приготовлении'),
                (4, 'Готов'),
                (5, 'В пути'),
                (6, 'Доставлен'),
                (7, 'Отменён');", conn))
            {
                cmd.ExecuteNonQuery();
            }

            // 4. Таблица categories
            using (MySqlCommand cmd = new MySqlCommand(@"
                CREATE TABLE IF NOT EXISTS `categories` (
                    `id_category` INT NOT NULL AUTO_INCREMENT,
                    `category_name` VARCHAR(255) NOT NULL,
                    PRIMARY KEY (`id_category`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;", conn))
            {
                cmd.ExecuteNonQuery();
            }

            // 5. Таблица dishes
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
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;", conn))
            {
                cmd.ExecuteNonQuery();
            }

            // 6. Таблица status_certificates
            using (MySqlCommand cmd = new MySqlCommand(@"
                CREATE TABLE IF NOT EXISTS `status_certificates` (
                    `id_status_certificate` INT NOT NULL AUTO_INCREMENT,
                    `name` VARCHAR(255) DEFAULT NULL,
                    PRIMARY KEY (`id_status_certificate`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;", conn))
            {
                cmd.ExecuteNonQuery();
            }

            // Добавляем начальные данные в status_certificates
            using (MySqlCommand cmd = new MySqlCommand(@"
                INSERT INTO `status_certificates` (`id_status_certificate`, `name`) VALUES 
                (1, 'Активен'),
                (2, 'Использован'),
                (3, 'Возвращён');", conn))
            {
                cmd.ExecuteNonQuery();
            }

            // 7. Таблица certificates
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
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;", conn))
            {
                cmd.ExecuteNonQuery();
            }

            // 8. Таблица present
            using (MySqlCommand cmd = new MySqlCommand(@"
                CREATE TABLE IF NOT EXISTS `present` (
                    `id_present` INT NOT NULL AUTO_INCREMENT,
                    `name` VARCHAR(255) DEFAULT NULL,
                    `from_price` DECIMAL(10,2) DEFAULT NULL,
                    PRIMARY KEY (`id_present`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;", conn))
            {
                cmd.ExecuteNonQuery();
            }

            // 9. Таблица orders
            using (MySqlCommand cmd = new MySqlCommand(@"
                CREATE TABLE IF NOT EXISTS `orders` (
                    `id_order` INT NOT NULL AUTO_INCREMENT,
                    `order_number` VARCHAR(20) NOT NULL,
                    `name_client` VARCHAR(255) NOT NULL,
                    `phone_number` VARCHAR(20) NOT NULL,
                    `address` VARCHAR(255) NOT NULL,
                    `number_persons` INT DEFAULT NULL COMMENT 'Количество персон',
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
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;", conn))
            {
                cmd.ExecuteNonQuery();
            }

            // 10. Таблица order_dish
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
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;", conn))
            {
                cmd.ExecuteNonQuery();
            }

            // 11. Таблица other_orders
            using (MySqlCommand cmd = new MySqlCommand(@"
                CREATE TABLE IF NOT EXISTS `other_orders` (
                    `id_other` INT NOT NULL AUTO_INCREMENT,
                    `id_order` INT DEFAULT NULL,
                    `id_status` INT DEFAULT NULL,
                    PRIMARY KEY (`id_other`),
                    KEY `id_order` (`id_order`),
                    KEY `other_orders_ibfk_1` (`id_status`),
                    CONSTRAINT `other_orders_ibfk_1` FOREIGN KEY (`id_status`) REFERENCES `order_statuses` (`id_status`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;", conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Обработчик кнопки обзора файлов
        /// </summary>
        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Выберите CSV файл для импорта";
                ofd.Filter = "CSV файлы (*.csv)|*.csv|Все файлы (*.*)|*.*";
                ofd.FilterIndex = 1;
                ofd.RestoreDirectory = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtFilePath.Text = ofd.FileName;
                }
            }
        }

        /// <summary>
        /// Обработчик кнопки импорта
        /// </summary>
        private void BtnImport_Click(object sender, EventArgs e)
        {
            try
            {
                // Проверка выбора таблицы
                if (cmbTables.SelectedItem == null)
                {
                    MessageBox.Show("Выберите таблицу для импорта!",
                        "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string tableName = cmbTables.SelectedItem.ToString();
                string filePath = txtFilePath.Text;

                // Проверка существования файла
                if (!File.Exists(filePath))
                {
                    MessageBox.Show("Файл не существует!",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Получаем количество столбцов в таблице
                int expectedColumns = tableColumnsCount.ContainsKey(tableName) ? tableColumnsCount[tableName] : 0;

                if (expectedColumns == 0)
                {
                    MessageBox.Show("Не удалось определить количество столбцов в таблице!",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Чтение CSV файла
                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

                if (lines.Length == 0)
                {
                    MessageBox.Show("Файл пуст!",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Проверяем, есть ли заголовок (первая строка)
                bool hasHeader = lines[0].Contains("id_") || lines[0].ToLower().Contains("id");

                int startLine = hasHeader ? 1 : 0;
                int importedCount = 0;
                int errorCount = 0;
                StringBuilder errorMessages = new StringBuilder();

                using (MySqlConnection conn = SettingsBD.GetConnection())
                {
                    conn.Open();

                    // Отключаем проверку внешних ключей временно
                    using (MySqlCommand cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 0;", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    for (int i = startLine; i < lines.Length; i++)
                    {
                        string line = lines[i].Trim();

                        // Пропускаем пустые строки
                        if (string.IsNullOrEmpty(line))
                            continue;

                        try
                        {
                            // Определяем разделитель (запятая или точка с запятой)
                            char delimiter = DetectDelimiter(line);

                            // Разделяем строку на значения
                            string[] values = ParseCSVLine(line, delimiter);

                            // Проверка количества столбцов
                            if (values.Length != expectedColumns)
                            {
                                errorCount++;
                                errorMessages.AppendLine($"Строка {i + 1}: Ожидалось {expectedColumns} полей, получено {values.Length}");
                                continue;
                            }

                            // Формируем INSERT запрос
                            string placeholders = string.Join(",", values.Select((v, index) => $"@p{index}"));
                            string query = $"INSERT INTO `{tableName}` VALUES ({placeholders})";

                            using (MySqlCommand cmd = new MySqlCommand(query, conn))
                            {
                                for (int j = 0; j < values.Length; j++)
                                {
                                    string paramName = $"@p{j}";
                                    string value = values[j].Trim();

                                    // Обработка пустых значений
                                    if (string.IsNullOrEmpty(value) || value == "NULL" || value.ToUpper() == "NULL")
                                    {
                                        cmd.Parameters.AddWithValue(paramName, DBNull.Value);
                                    }
                                    else
                                    {
                                        cmd.Parameters.AddWithValue(paramName, value);
                                    }
                                }

                                cmd.ExecuteNonQuery();
                                importedCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            errorMessages.AppendLine($"Строка {i + 1}: {ex.Message}");
                        }
                    }

                    // Включаем проверку внешних ключей обратно
                    using (MySqlCommand cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 1;", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                // Вывод результатов импорта
                string resultMessage = $"Импорт завершен!\n\n" +
                                     $"Успешно импортировано: {importedCount} записей\n" +
                                     $"Ошибок: {errorCount}";

                if (errorCount > 0)
                {
                    resultMessage += $"\n\nДетали ошибок:\n{errorMessages.ToString()}";
                    MessageBox.Show(resultMessage, "Результаты импорта (с ошибками)",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show(resultMessage, "Импорт успешно завершен",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Очищаем поля после успешного импорта
                    txtFilePath.Text = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Определение разделителя в CSV строке
        /// </summary>
        private char DetectDelimiter(string line)
        {
            // Считаем количество запятых и точек с запятой
            int commaCount = line.Count(c => c == ',');
            int semicolonCount = line.Count(c => c == ';');

            // Выбираем тот разделитель, которого больше
            if (semicolonCount > commaCount)
                return ';';
            else
                return ',';
        }

        /// <summary>
        /// Парсинг CSV строки с учетом кавычек и указанным разделителем
        /// </summary>
        private string[] ParseCSVLine(string line, char delimiter)
        {
            List<string> result = new List<string>();
            bool inQuotes = false;
            StringBuilder currentValue = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    // Проверяем, не экранирована ли кавычка
                    if (i < line.Length - 1 && line[i + 1] == '"')
                    {
                        // Двойные кавычки внутри строки - это одна кавычка
                        currentValue.Append('"');
                        i++; // Пропускаем следующую кавычку
                    }
                    else
                    {
                        // Открываем или закрываем кавычки
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == delimiter && !inQuotes)
                {
                    // Разделитель вне кавычек
                    result.Add(currentValue.ToString().Trim());
                    currentValue.Clear();
                }
                else
                {
                    currentValue.Append(c);
                }
            }

            // Добавляем последнее значение
            result.Add(currentValue.ToString().Trim());

            return result.ToArray();
        }

        /// <summary>
        /// Обработчик кнопки назад
        /// </summary>
        private void pictureBox2_Click_1(object sender, EventArgs e)
        {
            this.Visible = false;
            AdminForm admin = new AdminForm();
            admin.Show();
        }

        /// <summary>
        /// Обработчик загрузки формы
        /// </summary>
        private void ImportRestoreForm_Load(object sender, EventArgs e)
        {
            // Действия при загрузке формы
        }
    }
}