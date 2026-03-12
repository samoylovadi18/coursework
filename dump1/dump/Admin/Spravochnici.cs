using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace dump
{
    public partial class Spravochnici : Form
    {
        // Переменная для отслеживания режима редактирования
        private bool isEditMode = false;
        private CultureInfo russianCulture = new CultureInfo("ru-RU");
        private bool isFormatting = false; // Флаг для предотвращения рекурсии при форматировании

        public Spravochnici()
        {
            InitializeComponent();

            if (tabConrol1 != null)
            {
                tabConrol1.SelectedIndexChanged += tabConrol1_SelectedIndexChanged;
            }

            AddButton.FlatStyle = FlatStyle.Flat;
            AddButton.FlatAppearance.BorderSize = 1;
            AddButton.FlatAppearance.BorderColor = Color.Black;
            AddButton.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            AddButton.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            // Удаляем все изменения стиля кнопки Удалить - оставляем как было
            buttonDelete.FlatStyle = FlatStyle.Flat;
            buttonDelete.FlatAppearance.BorderSize = 1;
            buttonDelete.FlatAppearance.BorderColor = Color.Black;
            // Возвращаем оригинальные цвета
            buttonDelete.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            buttonDelete.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            SetupRussianOnlyTextBox(textBoxStatusName);
            SetupRussianOnlyTextBox(textBoxCategoryName);
            SetupRussianOnlyTextBox(textBoxPresentName);
            SetupPriceTextBox(textBoxFromPrice); // Изменено: SetupPriceTextBox вместо SetupNumericTextBox
            SetMaxLengthLimits();
            // Настройка двойного клика на DataGridView
            dataGridViewStatus.CellDoubleClick += DataGridView_CellDoubleClick;
            dataGridViewCategories.CellDoubleClick += DataGridView_CellDoubleClick;
            dataGridViewPresents.CellDoubleClick += DataGridView_CellDoubleClick;
            SetupDataGridViewRowSelection();

            // Подписываемся на клик по кнопке Удалить
            buttonDelete.Click += DeleteButton_Click;
        }

        private void SetupDataGridViewRowSelection()
        {
            // Настройка для dataGridViewStatus
            dataGridViewStatus.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewStatus.MultiSelect = false;

            // Настройка для dataGridViewCategories
            dataGridViewCategories.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewCategories.MultiSelect = false;

            // Настройка для dataGridViewPresents
            dataGridViewPresents.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewPresents.MultiSelect = false;
        }

        // Ограничения на количество символов в соответствии с БД
        private void SetMaxLengthLimits()
        {
            // Согласно скриншотам, все текстовые поля имеют VARCHAR(255)
            textBoxStatusName.MaxLength = 255;     // status_name VARCHAR(255) NOT NULL
            textBoxCategoryName.MaxLength = 255;   // category_name VARCHAR(255) NOT NULL
            textBoxPresentName.MaxLength = 255;    // name VARCHAR(255) NOT NULL

            // Для поля цены оставляем больше места для форматирования
            textBoxFromPrice.MaxLength = 20; // Увеличили для форматированной цены

            // Добавляем ограничение на максимальное значение цены согласно DECIMAL(10,2)
            // DECIMAL(10,2) означает: 10 цифр всего, 2 после запятой => максимум 8 цифр до запятой
            // Максимальное значение: 99,999,999.99 (9 цифр до запятой + 2 после = 11 символов, но форматирование)
        }

        // НАСТРОЙКА ТЕКСТОВОГО ПОЛЯ ДЛЯ ВВОДА ЦЕНЫ С ФОРМАТИРОВАНИЕМ ПРИ ВВОДЕ
        private void SetupPriceTextBox(TextBox textBox)
        {
            textBox.KeyPress += TextBoxPrice_KeyPress;
            textBox.TextChanged += TextBoxPrice_TextChanged;
            textBox.Enter += TextBoxPrice_Enter;
            textBox.Leave += TextBoxPrice_Leave;
            textBox.GotFocus += TextBoxPrice_GotFocus;

            // Устанавливаем выравнивание по правому краю
            textBox.TextAlign = HorizontalAlignment.Right;
        }

        private void TextBoxPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            // Разрешаем: цифры, Backspace, запятая
            if (char.IsDigit(e.KeyChar) ||
                e.KeyChar == (char)Keys.Back ||
                e.KeyChar == ',')
            {
                // Проверяем, что запятая только одна
                if (e.KeyChar == ',' && textBox.Text.Replace(" ", "").Replace("₽", "").Contains(","))
                {
                    e.Handled = true;
                    System.Media.SystemSounds.Beep.Play();
                    return;
                }

                e.Handled = false;
            }
            else
            {
                e.Handled = true;
                System.Media.SystemSounds.Beep.Play();
            }
        }

        private void TextBoxPrice_TextChanged(object sender, EventArgs e)
        {
            if (isFormatting) return;

            TextBox textBox = sender as TextBox;
            isFormatting = true;

            try
            {
                // Сохраняем позицию курсора
                int cursorPos = textBox.SelectionStart;
                string originalText = textBox.Text;

                // Если текст пустой, выходим
                if (string.IsNullOrEmpty(originalText))
                {
                    isFormatting = false;
                    return;
                }

                // Получаем только цифры и запятую из текста
                string cleanText = "";
                foreach (char c in originalText)
                {
                    if (char.IsDigit(c) || c == ',')
                        cleanText += c;
                }

                // Если запятых больше одной, оставляем только первую
                int commaIndex = cleanText.IndexOf(',');
                if (commaIndex != -1)
                {
                    string beforeComma = cleanText.Substring(0, commaIndex + 1);
                    string afterComma = cleanText.Substring(commaIndex + 1).Replace(",", "");
                    cleanText = beforeComma + afterComma;
                }

                // Ограничиваем 2 знаками после запятой (согласно DECIMAL(10,2))
                if (cleanText.Contains(","))
                {
                    string[] parts = cleanText.Split(',');
                    if (parts.Length > 1 && parts[1].Length > 2)
                    {
                        cleanText = parts[0] + "," + parts[1].Substring(0, 2);
                    }
                }

                // Проверяем ограничение DECIMAL(10,2) - максимум 8 цифр до запятой
                if (cleanText.Contains(","))
                {
                    string[] parts = cleanText.Split(',');
                    string beforeCommaPart = parts[0];
                    // Убираем ведущие нули для правильного подсчета
                    beforeCommaPart = beforeCommaPart.TrimStart('0');
                    if (beforeCommaPart.Length > 8) // Максимум 8 цифр до запятой
                    {
                        beforeCommaPart = beforeCommaPart.Substring(0, 8);
                        cleanText = beforeCommaPart + "," + (parts.Length > 1 ? parts[1] : "00");
                    }
                }
                else
                {
                    // Если нет запятой, проверяем общую длину
                    string temp = cleanText.TrimStart('0');
                    if (temp.Length > 8) // Максимум 8 цифр до запятой
                    {
                        cleanText = temp.Substring(0, 8);
                    }
                }

                // Форматируем только если у нас есть цифры
                if (!string.IsNullOrEmpty(cleanText))
                {
                    // Упрощенная логика форматирования
                    // Вместо немедленного форматирования при вводе, 
                    // будем форматировать только при потере фокуса

                    // Но сохраняем правильную позицию курсора
                    int newCursorPos = 0;
                    int nonFormatChars = 0;

                    // Считаем, сколько неформатируемых символов (цифр и запятых) было до курсора
                    for (int i = 0; i < cursorPos && i < originalText.Length; i++)
                    {
                        if (char.IsDigit(originalText[i]) || originalText[i] == ',')
                            nonFormatChars++;
                    }

                    // Устанавливаем курсор после того же количества цифр/запятых
                    textBox.Text = cleanText;
                    textBox.SelectionStart = Math.Min(nonFormatChars, textBox.Text.Length);
                }
            }
            finally
            {
                isFormatting = false;
            }
        }

        private void TextBoxPrice_Enter(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;

            // При фокусе убираем форматирование для удобного редактирования
            if (!string.IsNullOrEmpty(textBox.Text))
            {
                // Убираем пробелы разделителей тысяч и символ рубля
                string plainText = textBox.Text.Replace(" ", "").Replace("₽", "").Trim();
                if (decimal.TryParse(plainText, NumberStyles.Any, russianCulture, out decimal value))
                {
                    textBox.Text = plainText;
                    textBox.SelectionStart = textBox.Text.Length;
                }
            }
        }

        private void TextBoxPrice_Leave(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            FormatPriceTextBoxOnLeave(textBox);
        }

        private void TextBoxPrice_GotFocus(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            // Не выделяем весь текст автоматически, чтобы не мешать редактированию
            // textBox.SelectAll();
        }

        private void FormatPriceTextBoxOnLeave(TextBox textBox)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "";
                return;
            }

            string text = textBox.Text.Trim();

            // Заменяем точку на запятую (если пользователь ввел точку)
            text = text.Replace(".", ",");

            // Убираем все символы, кроме цифр и запятой
            string cleanText = new string(text.Where(c => char.IsDigit(c) || c == ',').ToArray());

            if (string.IsNullOrEmpty(cleanText))
            {
                textBox.Text = "";
                return;
            }

            // Если запятых больше одной, оставляем только первую
            int commaIndex = cleanText.IndexOf(',');
            if (commaIndex != -1)
            {
                string beforeComma = cleanText.Substring(0, commaIndex + 1);
                string afterComma = cleanText.Substring(commaIndex + 1).Replace(",", "");
                cleanText = beforeComma + afterComma;
            }

            // Проверяем ограничение DECIMAL(10,2) перед парсингом
            if (cleanText.Contains(","))
            {
                string[] parts = cleanText.Split(',');
                string beforeCommaPart = parts[0];
                // Убираем ведущие нули для правильного подсчета
                beforeCommaPart = beforeCommaPart.TrimStart('0');
                if (beforeCommaPart.Length > 8) // Максимум 8 цифр до запятой
                {
                    beforeCommaPart = beforeCommaPart.Substring(0, 8);
                    cleanText = beforeCommaPart + "," + (parts.Length > 1 ? parts[1] : "00");
                }

                // Ограничиваем 2 знаками после запятой
                if (parts.Length > 1 && parts[1].Length > 2)
                {
                    cleanText = beforeCommaPart + "," + parts[1].Substring(0, 2);
                }
            }

            // Парсим значение
            if (decimal.TryParse(cleanText, NumberStyles.Any, russianCulture, out decimal value))
            {
                // Ограничиваем 2 знаками после запятой
                value = Math.Round(value, 2);

                // Проверяем максимальное значение согласно DECIMAL(10,2)
                // Максимум: 99,999,999.99
                decimal maxValue = 99999999.99m;
                if (value > maxValue)
                {
                    value = maxValue;
                }

                // Форматируем с разделителями тысяч и добавляем символ рубля
                textBox.Text = value.ToString("N2", russianCulture) + " ₽";
            }
            else
            {
                textBox.Text = "";
            }
        }

        // Метод для получения числового значения из отформатированной цены
        private decimal GetPriceFromFormattedText(string formattedText)
        {
            if (string.IsNullOrWhiteSpace(formattedText))
                return 0;

            string cleanText = formattedText.Replace(" ", "").Replace("₽", "").Trim();

            if (decimal.TryParse(cleanText, NumberStyles.Any, russianCulture, out decimal value))
            {
                return Math.Round(value, 2);
            }

            return 0;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            AdminForm admin = new AdminForm();
            admin.Show();
        }

        private void Spravochnici_Load(object sender, EventArgs e)
        {
            LoadDataForSelectedTab();
        }

        // Обработчик изменения выбранной вкладки
        private void tabConrol1_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadDataForSelectedTab();
            // Сбрасываем режим редактирования при смене вкладки
            ResetEditMode();
        }

        // Сброс режима редактирования
        private void ResetEditMode()
        {
            isEditMode = false;
            ClearInputFields(tabConrol1.SelectedTab?.Name);
            ClearTags(tabConrol1.SelectedTab?.Name);
            // Меняем текст кнопки обратно на "Добавить"
            AddButton.Text = "Добавить";
        }

        private void LoadDataForSelectedTab()
        {
            string selectedTab = tabConrol1.SelectedTab?.Name;

            if (string.IsNullOrEmpty(selectedTab))
                return;

            try
            {
                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();
                    string query = "";
                    DataTable dataTable = new DataTable();

                    // Выбираем запрос в зависимости от вкладки
                    switch (selectedTab)
                    {
                        case "tabRole":
                            query = "SELECT id_role, role_name as 'Название роли' FROM roles";
                            break;
                        case "tabStatus":
                            query = "SELECT id_status, status_name as 'Название статуса' FROM order_statuses";
                            break;
                        case "tabCategories":
                            query = "SELECT id_category, category_name as 'Название категории' FROM categories";
                            break;
                        case "tabPresent":
                            query = "SELECT id_present, name as 'Название подарка', from_price as 'От какой суммы' FROM present";
                            break;
                        default:
                            return;
                    }

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection))
                    {
                        adapter.Fill(dataTable);
                    }

                    // Привязываем данные к соответствующему DataGridView
                    // и скрываем ID колонку
                    switch (selectedTab)
                    {
                        case "tabStatus":
                            dataGridViewStatus.DataSource = dataTable;
                            if (dataGridViewStatus.Columns.Contains("id_status"))
                                dataGridViewStatus.Columns["id_status"].Visible = false;
                            break;
                        case "tabCategories":
                            dataGridViewCategories.DataSource = dataTable;
                            if (dataGridViewCategories.Columns.Contains("id_category"))
                                dataGridViewCategories.Columns["id_category"].Visible = false;
                            break;
                        case "tabPresent":
                            dataGridViewPresents.DataSource = dataTable;
                            if (dataGridViewPresents.Columns.Contains("id_present"))
                                dataGridViewPresents.Columns["id_present"].Visible = false;
                            // Форматируем столбец цены
                            if (dataGridViewPresents.Columns.Contains("От какой суммы"))
                            {
                                dataGridViewPresents.Columns["От какой суммы"].DefaultCellStyle.Format = "N2";
                                dataGridViewPresents.Columns["От какой суммы"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Проверка на дубликат статуса
        private bool IsStatusDuplicate(string statusName, MySqlConnection connection, int? excludeId = null)
        {
            string query = "SELECT COUNT(*) FROM order_statuses WHERE LOWER(status_name) = LOWER(@statusName)";
            if (excludeId.HasValue)
            {
                query += " AND id_status != @excludeId";
            }

            using (MySqlCommand cmd = new MySqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@statusName", statusName.Trim());
                if (excludeId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@excludeId", excludeId.Value);
                }
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        // Проверка на дубликат категории
        private bool IsCategoryDuplicate(string categoryName, MySqlConnection connection, int? excludeId = null)
        {
            string query = "SELECT COUNT(*) FROM categories WHERE LOWER(category_name) = LOWER(@categoryName)";
            if (excludeId.HasValue)
            {
                query += " AND id_category != @excludeId";
            }

            using (MySqlCommand cmd = new MySqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@categoryName", categoryName.Trim());
                if (excludeId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@excludeId", excludeId.Value);
                }
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        // Проверка на дубликат подарка
        private bool IsPresentDuplicate(string presentName, MySqlConnection connection, int? excludeId = null)
        {
            string query = "SELECT COUNT(*) FROM present WHERE LOWER(name) = LOWER(@presentName)";
            if (excludeId.HasValue)
            {
                query += " AND id_present != @excludeId";
            }

            using (MySqlCommand cmd = new MySqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@presentName", presentName.Trim());
                if (excludeId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@excludeId", excludeId.Value);
                }
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        // Метод для добавления новой записи
        private void AddNewRecord()
        {
            string selectedTab = tabConrol1.SelectedTab?.Name;

            try
            {
                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();
                    string query = "";
                    MySqlCommand cmd;

                    switch (selectedTab)
                    {
                        case "tabStatus":
                            // Проверяем заполнение поля - ОБЯЗАТЕЛЬНОЕ ПОЛЕ (NOT NULL)
                            if (string.IsNullOrWhiteSpace(textBoxStatusName.Text))
                            {
                                MessageBox.Show("Введите название статуса! Это поле обязательно для заполнения.", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                textBoxStatusName.Focus();
                                return;
                            }

                            string statusName = textBoxStatusName.Text.Trim();

                            // Проверка на длину согласно VARCHAR(255)
                            if (statusName.Length > 255)
                            {
                                MessageBox.Show("Название статуса не должно превышать 255 символов!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                textBoxStatusName.Focus();
                                return;
                            }

                            // Проверяем на дубликат
                            if (IsStatusDuplicate(statusName, connection))
                            {
                                MessageBox.Show("Статус с таким названием уже существует!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                textBoxStatusName.Focus();
                                return;
                            }

                            query = "INSERT INTO order_statuses (status_name) VALUES (@name)";
                            cmd = new MySqlCommand(query, connection);
                            cmd.Parameters.AddWithValue("@name", statusName);
                            break;

                        case "tabCategories":
                            // Проверяем заполнение поля - ОБЯЗАТЕЛЬНОЕ ПОЛЕ (NOT NULL)
                            if (string.IsNullOrWhiteSpace(textBoxCategoryName.Text))
                            {
                                MessageBox.Show("Введите название категории! Это поле обязательно для заполнения.", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                textBoxCategoryName.Focus();
                                return;
                            }

                            string categoryName = textBoxCategoryName.Text.Trim();

                            // Проверка на длину согласно VARCHAR(255)
                            if (categoryName.Length > 255)
                            {
                                MessageBox.Show("Название категории не должно превышать 255 символов!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                textBoxCategoryName.Focus();
                                return;
                            }

                            // Проверяем на дубликат
                            if (IsCategoryDuplicate(categoryName, connection))
                            {
                                MessageBox.Show("Категория с таким названием уже существует!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                textBoxCategoryName.Focus();
                                return;
                            }

                            query = "INSERT INTO categories (category_name) VALUES (@name)";
                            cmd = new MySqlCommand(query, connection);
                            cmd.Parameters.AddWithValue("@name", categoryName);
                            break;

                        case "tabPresent":
                            // Проверяем заполнение полей - name ОБЯЗАТЕЛЬНОЕ ПОЛЕ (NOT NULL)
                            if (string.IsNullOrWhiteSpace(textBoxPresentName.Text))
                            {
                                MessageBox.Show("Введите название подарка! Это поле обязательно для заполнения.", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                textBoxPresentName.Focus();
                                return;
                            }

                            // Получаем цену из отформатированного текста
                            decimal price = GetPriceFromFormattedText(textBoxFromPrice.Text);
                            if (price <= 0)
                            {
                                MessageBox.Show("Введите корректную сумму! Значение должно быть больше 0.", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                textBoxFromPrice.Focus();
                                return;
                            }

                            string presentName = textBoxPresentName.Text.Trim();

                            // Проверка на длину названия согласно VARCHAR(255)
                            if (presentName.Length > 255)
                            {
                                MessageBox.Show("Название подарка не должно превышать 255 символов!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                textBoxPresentName.Focus();
                                return;
                            }

                            // Проверяем на дубликат
                            if (IsPresentDuplicate(presentName, connection))
                            {
                                MessageBox.Show("Подарок с таким названием уже существует!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                textBoxPresentName.Focus();
                                return;
                            }

                            // Проверяем максимальное значение цены согласно DECIMAL(10,2)
                            decimal maxPrice = 99999999.99m; // Максимум для DECIMAL(10,2)
                            if (price > maxPrice)
                            {
                                MessageBox.Show($"Сумма не должна превышать {maxPrice.ToString("N2", russianCulture)}!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                textBoxFromPrice.Focus();
                                return;
                            }

                            query = "INSERT INTO present (name, from_price) VALUES (@name, @price)";
                            cmd = new MySqlCommand(query, connection);
                            cmd.Parameters.AddWithValue("@name", presentName);
                            cmd.Parameters.AddWithValue("@price", price);
                            break;

                        default:
                            return;
                    }

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Запись успешно добавлена!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Очищаем поля ввода
                        ClearInputFields(selectedTab);

                        // Обновляем данные
                        LoadDataForSelectedTab();
                    }
                }
            }
            catch (MySqlException mysqlEx)
            {
                if (mysqlEx.Number == 1062)
                {
                    MessageBox.Show("Такая запись уже существует!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (mysqlEx.Number == 1048) // Column cannot be null
                {
                    MessageBox.Show("Обязательные поля не заполнены! Пожалуйста, заполните все необходимые поля.", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show($"Ошибка базы данных: {mysqlEx.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Метод для сохранения изменений при редактировании
        private void SaveChanges()
        {
            string selectedTab = tabConrol1.SelectedTab?.Name;

            try
            {
                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();
                    string query = "";
                    MySqlCommand cmd;
                    int? id = null;

                    switch (selectedTab)
                    {
                        case "tabStatus":
                            id = textBoxStatusName.Tag as int?;
                            if (!id.HasValue)
                            {
                                MessageBox.Show("Выберите запись для редактирования!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            // Проверяем заполнение поля - ОБЯЗАТЕЛЬНОЕ ПОЛЕ (NOT NULL)
                            if (string.IsNullOrWhiteSpace(textBoxStatusName.Text))
                            {
                                MessageBox.Show("Введите название статуса! Это поле обязательно для заполнения.", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                textBoxStatusName.Focus();
                                return;
                            }

                            string statusName = textBoxStatusName.Text.Trim();

                            // Проверка на длину согласно VARCHAR(255)
                            if (statusName.Length > 255)
                            {
                                MessageBox.Show("Название статуса не должно превышать 255 символов!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                textBoxStatusName.Focus();
                                return;
                            }

                            // Проверяем на дубликат (исключая текущую запись)
                            if (IsStatusDuplicate(statusName, connection, id))
                            {
                                MessageBox.Show("Статус с таким названием уже существует!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                textBoxStatusName.Focus();
                                return;
                            }

                            query = "UPDATE order_statuses SET status_name = @name WHERE id_status = @id";
                            cmd = new MySqlCommand(query, connection);
                            cmd.Parameters.AddWithValue("@name", statusName);
                            cmd.Parameters.AddWithValue("@id", id.Value);
                            break;

                        case "tabCategories":
                            id = textBoxCategoryName.Tag as int?;
                            if (!id.HasValue)
                            {
                                MessageBox.Show("Выберите запись для редактирования!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            // Проверяем заполнение поля - ОБЯЗАТЕЛЬНОЕ ПОЛЕ (NOT NULL)
                            if (string.IsNullOrWhiteSpace(textBoxCategoryName.Text))
                            {
                                MessageBox.Show("Введите название категории! Это поле обязательно для заполнения.", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                textBoxCategoryName.Focus();
                                return;
                            }

                            string categoryName = textBoxCategoryName.Text.Trim();

                            // Проверка на длину согласно VARCHAR(255)
                            if (categoryName.Length > 255)
                            {
                                MessageBox.Show("Название категории не должно превышать 255 символов!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                textBoxCategoryName.Focus();
                                return;
                            }

                            // Проверяем на дубликат (исключая текущую запись)
                            if (IsCategoryDuplicate(categoryName, connection, id))
                            {
                                MessageBox.Show("Категория с таким названием уже существует!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                textBoxCategoryName.Focus();
                                return;
                            }

                            query = "UPDATE categories SET category_name = @name WHERE id_category = @id";
                            cmd = new MySqlCommand(query, connection);
                            cmd.Parameters.AddWithValue("@name", categoryName);
                            cmd.Parameters.AddWithValue("@id", id.Value);
                            break;

                        case "tabPresent":
                            id = textBoxPresentName.Tag as int?;
                            if (!id.HasValue)
                            {
                                MessageBox.Show("Выберите запись для редактирования!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            // Проверяем заполнение поля - ОБЯЗАТЕЛЬНОЕ ПОЛЕ (NOT NULL)
                            if (string.IsNullOrWhiteSpace(textBoxPresentName.Text))
                            {
                                MessageBox.Show("Введите название подарка! Это поле обязательно для заполнения.", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                textBoxPresentName.Focus();
                                return;
                            }

                            // Получаем цену из отформатированного текста
                            decimal price = GetPriceFromFormattedText(textBoxFromPrice.Text);
                            if (price <= 0)
                            {
                                MessageBox.Show("Введите корректную сумму! Значение должно быть больше 0.", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                textBoxFromPrice.Focus();
                                return;
                            }

                            string presentName = textBoxPresentName.Text.Trim();

                            // Проверка на длину названия согласно VARCHAR(255)
                            if (presentName.Length > 255)
                            {
                                MessageBox.Show("Название подарка не должно превышать 255 символов!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                textBoxPresentName.Focus();
                                return;
                            }

                            // Проверяем на дубликат (исключая текущую запись)
                            if (IsPresentDuplicate(presentName, connection, id))
                            {
                                MessageBox.Show("Подарок с таким названием уже существует!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                textBoxPresentName.Focus();
                                return;
                            }

                            // Проверяем максимальное значение цены согласно DECIMAL(10,2)
                            decimal maxPrice = 99999999.99m; // Максимум для DECIMAL(10,2)
                            if (price > maxPrice)
                            {
                                MessageBox.Show($"Сумма не должна превышать {maxPrice.ToString("N2", russianCulture)}!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                textBoxFromPrice.Focus();
                                return;
                            }

                            query = "UPDATE present SET name = @name, from_price = @price WHERE id_present = @id";
                            cmd = new MySqlCommand(query, connection);
                            cmd.Parameters.AddWithValue("@name", presentName);
                            cmd.Parameters.AddWithValue("@price", price);
                            cmd.Parameters.AddWithValue("@id", id.Value);
                            break;

                        default:
                            return;
                    }

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Запись успешно обновлена!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Сбрасываем режим редактирования
                        ResetEditMode();

                        // Обновляем данные
                        LoadDataForSelectedTab();
                    }
                    else
                    {
                        MessageBox.Show("Запись не найдена!", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (MySqlException mysqlEx)
            {
                if (mysqlEx.Number == 1048) // Column cannot be null
                {
                    MessageBox.Show("Обязательные поля не заполнены! Пожалуйста, заполните все необходимые поля.", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show($"Ошибка базы данных: {mysqlEx.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Метод для очистки полей ввода
        private void ClearInputFields(string selectedTab)
        {
            switch (selectedTab)
            {
                case "tabStatus":
                    textBoxStatusName.Text = "";
                    break;
                case "tabCategories":
                    textBoxCategoryName.Text = "";
                    break;
                case "tabPresent":
                    textBoxPresentName.Text = "";
                    textBoxFromPrice.Text = "";
                    break;
            }
        }

        // Метод для удаления записи с проверкой на связанные записи
        private void DeleteButton_Click(object sender, EventArgs e)
        {
            DataGridView activeDGV = GetActiveDataGridView();

            if (activeDGV == null || activeDGV.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись для удаления", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string selectedTab = tabConrol1.SelectedTab?.Name;
            DataGridViewRow selectedRow = activeDGV.SelectedRows[0];

            // Получаем ID из скрытой колонки
            int id = 0;
            string idColumnName = "";
            string tableName = "";
            string nameColumnName = "";

            switch (selectedTab)
            {
                case "tabRole":
                    idColumnName = "id_role";
                    tableName = "roles";
                    nameColumnName = "Название роли";
                    id = Convert.ToInt32(selectedRow.Cells[idColumnName].Value);
                    break;
                case "tabStatus":
                    idColumnName = "id_status";
                    tableName = "order_statuses";
                    nameColumnName = "Название статуса";
                    id = Convert.ToInt32(selectedRow.Cells[idColumnName].Value);
                    break;
                case "tabCategories":
                    idColumnName = "id_category";
                    tableName = "categories";
                    nameColumnName = "Название категории";
                    id = Convert.ToInt32(selectedRow.Cells[idColumnName].Value);
                    break;
                case "tabPresent":
                    idColumnName = "id_present";
                    tableName = "present";
                    nameColumnName = "Название подарка";
                    id = Convert.ToInt32(selectedRow.Cells[idColumnName].Value);
                    break;
                default:
                    MessageBox.Show("Неизвестная вкладка", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
            }

            // Получаем название для отображения в сообщении
            string recordName = selectedRow.Cells[nameColumnName].Value?.ToString() ?? id.ToString();

            // Проверяем, есть ли связанные записи перед удалением
            if (HasRelatedRecords(id, tableName, selectedTab))
            {
                MessageBox.Show($"Невозможно удалить '{recordName}', так как существуют связанные записи в других таблицах!\n\nСначала удалите все связанные записи.",
                    "Ошибка удаления",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            // Подтверждение удаления
            DialogResult result = MessageBox.Show(
                $"Вы уверены, что хотите удалить запись: \"{recordName}\"?\n\nЭто действие нельзя отменить!",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    using (MySqlConnection connection = SettingsBD.GetConnection())
                    {
                        connection.Open();
                        string query = $"DELETE FROM {tableName} WHERE {idColumnName} = @id";

                        using (MySqlCommand cmd = new MySqlCommand(query, connection))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show($"Запись \"{recordName}\" успешно удалена!", "Успех",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                                // Сбрасываем режим редактирования если удаляем редактируемую запись
                                if (isEditMode)
                                {
                                    switch (selectedTab)
                                    {
                                        case "tabStatus":
                                            if (textBoxStatusName.Tag != null && Convert.ToInt32(textBoxStatusName.Tag) == id)
                                                ResetEditMode();
                                            break;
                                        case "tabCategories":
                                            if (textBoxCategoryName.Tag != null && Convert.ToInt32(textBoxCategoryName.Tag) == id)
                                                ResetEditMode();
                                            break;
                                        case "tabPresent":
                                            if (textBoxPresentName.Tag != null && Convert.ToInt32(textBoxPresentName.Tag) == id)
                                                ResetEditMode();
                                            break;
                                    }
                                }

                                // Обновляем данные
                                LoadDataForSelectedTab();
                            }
                            else
                            {
                                MessageBox.Show("Запись не найдена или уже была удалена", "Информация",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
                catch (MySqlException mysqlEx)
                {
                    if (mysqlEx.Number == 1451)
                    {
                        // Получаем детальную информацию о связанных записях
                        string detailedInfo = GetRelatedRecordsDetails(id, tableName, selectedTab);

                        MessageBox.Show($"Невозможно удалить '{recordName}', так как существуют связанные записи!\n\n{detailedInfo}",
                            "Ошибка удаления",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show($"Ошибка базы данных: {mysqlEx.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Проверка наличия связанных записей
        private bool HasRelatedRecords(int id, string tableName, string tabName)
        {
            try
            {
                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();
                    string query = "";

                    switch (tableName)
                    {
                        case "order_statuses":
                            query = "SELECT COUNT(*) FROM orders WHERE id_status = @id";
                            break;
                        case "categories":
                            query = "SELECT COUNT(*) FROM menu WHERE id_category = @id";
                            break;
                        case "present":
                            query = "SELECT COUNT(*) FROM orders WHERE id_present = @id";
                            break;
                        case "roles":
                            query = "SELECT COUNT(*) FROM staff WHERE id_role = @id";
                            break;
                        default:
                            return false;
                    }

                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            int count = Convert.ToInt32(result);
                            return count > 0;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // В случае ошибки лучше разрешить попытку удаления
                // Пусть база данных сама выдаст ошибку при нарушении foreign key
                return false;
            }

            return false;
        }

        // Получение детальной информации о связанных записях
        private string GetRelatedRecordsDetails(int id, string tableName, string tabName)
        {
            try
            {
                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();
                    string query = "";
                    string resultMessage = "";

                    switch (tableName)
                    {
                        case "order_statuses":
                            query = "SELECT COUNT(*) as count FROM orders WHERE id_status = @id";
                            using (MySqlCommand cmd = new MySqlCommand(query, connection))
                            {
                                cmd.Parameters.AddWithValue("@id", id);
                                int count = Convert.ToInt32(cmd.ExecuteScalar());
                                resultMessage = $"Статус используется в {count} заказах";
                            }
                            break;

                        case "categories":
                            query = "SELECT COUNT(*) as count FROM dishes WHERE id_category = @id";
                            using (MySqlCommand cmd = new MySqlCommand(query, connection))
                            {
                                cmd.Parameters.AddWithValue("@id", id);
                                int count = Convert.ToInt32(cmd.ExecuteScalar());
                                resultMessage = $"Категория используется в {count} блюдах";
                            }
                            break;

                        case "present":
                            query = "SELECT COUNT(*) as count FROM orders WHERE id_present = @id";
                            using (MySqlCommand cmd = new MySqlCommand(query, connection))
                            {
                                cmd.Parameters.AddWithValue("@id", id);
                                int count = Convert.ToInt32(cmd.ExecuteScalar());
                                resultMessage = $"Подарок используется в {count} заказах";
                            }
                            break;

                        case "roles":
                            query = "SELECT COUNT(*) as count FROM staff WHERE id_role = @id";
                            using (MySqlCommand cmd = new MySqlCommand(query, connection))
                            {
                                cmd.Parameters.AddWithValue("@id", id);
                                int count = Convert.ToInt32(cmd.ExecuteScalar());
                                resultMessage = $"Роль используется {count} сотрудниками";
                            }
                            break;

                        default:
                            resultMessage = "Запись используется в других таблицах";
                            break;
                    }

                    return resultMessage;
                }
            }
            catch (Exception)
            {
                return "Запись имеет связанные данные в системе";
            }
        }

        // Получаем активный DataGridView
        private DataGridView GetActiveDataGridView()
        {
            string selectedTab = tabConrol1.SelectedTab?.Name;

            if (selectedTab == "tabStatus")
                return dataGridViewStatus;
            else if (selectedTab == "tabCategories")
                return dataGridViewCategories;
            else if (selectedTab == "tabPresent")
                return dataGridViewPresents;
            else
                return null;
        }

        // Кнопка обновления данных
        private void RefreshButton_Click(object sender, EventArgs e)
        {
            LoadDataForSelectedTab();
        }

        // Общий обработчик двойного клика по всем DataGridView
        private void DataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridView dgv = sender as DataGridView;
                if (dgv != null)
                {
                    // Убедимся, что строка выбрана
                    dgv.ClearSelection();
                    dgv.Rows[e.RowIndex].Selected = true;

                    PrepareForEdit();
                }
            }
        }

        // Подготовка к редактированию записи
        private void PrepareForEdit()
        {
            DataGridView activeDGV = GetActiveDataGridView();

            if (activeDGV == null || activeDGV.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись для редактирования", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string selectedTab = tabConrol1.SelectedTab?.Name;
            DataGridViewRow selectedRow = activeDGV.SelectedRows[0];

            // Получаем ID из скрытой колонки и отображаем данные в textbox'ах
            switch (selectedTab)
            {
                case "tabStatus":
                    int statusId = Convert.ToInt32(selectedRow.Cells["id_status"].Value);
                    textBoxStatusName.Text = selectedRow.Cells["Название статуса"].Value?.ToString() ?? "";
                    textBoxStatusName.Tag = statusId;
                    break;

                case "tabCategories":
                    int categoryId = Convert.ToInt32(selectedRow.Cells["id_category"].Value);
                    textBoxCategoryName.Text = selectedRow.Cells["Название категории"].Value?.ToString() ?? "";
                    textBoxCategoryName.Tag = categoryId;
                    break;

                case "tabPresent":
                    int presentId = Convert.ToInt32(selectedRow.Cells["id_present"].Value);
                    textBoxPresentName.Text = selectedRow.Cells["Название подарка"].Value?.ToString() ?? "";

                    // Получаем цену
                    object priceValue = selectedRow.Cells["От какой суммы"].Value;

                    if (priceValue != null)
                    {
                        if (decimal.TryParse(priceValue.ToString(), out decimal price))
                        {
                            // Форматируем для отображения с символом рубля
                            textBoxFromPrice.Text = price.ToString("N2", russianCulture) + " ₽";
                        }
                        else
                        {
                            textBoxFromPrice.Text = "";
                        }
                    }
                    else
                    {
                        textBoxFromPrice.Text = "";
                    }

                    textBoxPresentName.Tag = presentId;
                    break;
            }

            // Включаем режим редактирования
            isEditMode = true;

            // Меняем текст кнопки на "Сохранить"
            AddButton.Text = "Сохранить";

            // Показываем сообщение о выборе записи
            MessageBox.Show("Запись выбрана для редактирования. Измените данные и нажмите 'Сохранить'",
                "Редактирование",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // Кнопка "Очистить" для очистки полей ввода
        private void ClearButton_Click(object sender, EventArgs e)
        {
            ResetEditMode();
        }

        // Очистка Tag'ов
        private void ClearTags(string selectedTab)
        {
            switch (selectedTab)
            {
                case "tabStatus":
                    textBoxStatusName.Tag = null;
                    break;
                case "tabCategories":
                    textBoxCategoryName.Tag = null;
                    break;
                case "tabPresent":
                    textBoxPresentName.Tag = null;
                    break;
            }
        }

        // Настройка TextBox для ввода только русских букв
        private void SetupRussianOnlyTextBox(TextBox textBox)
        {
            textBox.KeyPress += TextBoxRussianOnly_KeyPress;
            textBox.TextChanged += TextBoxRussianOnly_TextChanged;
            textBox.Leave += TextBoxRussianOnly_Leave;
        }

        // Обработчики для русских букв
        private void TextBoxRussianOnly_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            // Разрешаем: русские буквы, дефис, пробел, Backspace
            if (char.IsLetter(e.KeyChar) && IsRussianLetter(e.KeyChar) ||
                e.KeyChar == '-' || e.KeyChar == ' ' ||
                e.KeyChar == (char)Keys.Back)
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
                System.Media.SystemSounds.Beep.Play();
            }
        }

        private void TextBoxRussianOnly_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                string text = textBox.Text;
                string filteredText = FilterRussianOnly(text);

                if (text != filteredText)
                {
                    int cursorPosition = textBox.SelectionStart;
                    textBox.Text = filteredText;
                    textBox.SelectionStart = Math.Max(0, cursorPosition - (text.Length - filteredText.Length));
                }
            }
        }

        private void TextBoxRussianOnly_Leave(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (!string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = textBox.Text.Trim();

                if (textBox.Text.Length > 0)
                {
                    // Делаем первую букву заглавной, остальные строчные
                    textBox.Text = char.ToUpper(textBox.Text[0]) +
                                   textBox.Text.Substring(1).ToLower();
                }
            }
        }

        // Проверка, является ли символ русской буквой
        private bool IsRussianLetter(char c)
        {
            return (c >= 'А' && c <= 'Я') || (c >= 'а' && c <= 'я') || c == 'Ё' || c == 'ё';
        }

        // Фильтрация текста - оставляем только русские буквы, дефис и пробел
        private string FilterRussianOnly(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            string result = "";
            foreach (char c in input)
            {
                if (IsRussianLetter(c) || c == '-' || c == ' ')
                {
                    result += c;
                }
            }

            return result;
        }

        private void tabStatus_Click(object sender, EventArgs e)
        {
            // Обработчик клика по вкладке статусов
        }

        private void EditButton_Click(object sender, EventArgs e)
        {

        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            if (isEditMode)
            {
                // Если в режиме редактирования - сохраняем изменения
                SaveChanges();
            }
            else
            {
                // Если в режиме добавления - добавляем новую запись
                AddNewRecord();
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            // Обработчик уже добавлен в конструкторе
        }

        // Дополнительный метод для удаления по нажатию Delete на клавиатуре
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Delete)
            {
                DeleteButton_Click(null, null);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}