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

            // КНОПКА ДОБАВИТЬ
            AddButton.FlatStyle = FlatStyle.Flat;
            AddButton.FlatAppearance.BorderSize = 1;
            AddButton.FlatAppearance.BorderColor = Color.Black;
            AddButton.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            AddButton.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;
            AddButton.Click += AddButton_Click;
            AddButton.Visible = true;
            AddButton.Text = "Добавить";

            // КНОПКА СОХРАНИТЬ (НОВАЯ)
            buttonSave.FlatStyle = FlatStyle.Flat;
            buttonSave.FlatAppearance.BorderSize = 1;
            buttonSave.FlatAppearance.BorderColor = Color.Black;
            buttonSave.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            buttonSave.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;
            buttonSave.Click += ButtonSave_Click;
            buttonSave.Visible = false; // Скрыта по умолчанию

            // КНОПКА РЕДАКТИРОВАТЬ
            buttonEdit.FlatStyle = FlatStyle.Flat;
            buttonEdit.FlatAppearance.BorderSize = 1;
            buttonEdit.FlatAppearance.BorderColor = Color.Black;
            buttonEdit.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            buttonEdit.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;
            buttonEdit.Click += ButtonEdit_Click;

            // КНОПКА УДАЛИТЬ
            buttonDelete.FlatStyle = FlatStyle.Flat;
            buttonDelete.FlatAppearance.BorderSize = 1;
            buttonDelete.FlatAppearance.BorderColor = Color.Black;
            buttonDelete.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            buttonDelete.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;
            buttonDelete.Click += DeleteButton_Click;

            SetupRussianOnlyTextBox(textBoxStatusName);
            SetupRussianOnlyTextBox(textBoxCategoryName);
            SetupRussianOnlyTextBox(textBoxPresentName);
            SetupPriceTextBox(textBoxFromPrice);
            SetMaxLengthLimits();

            // Настройка двойного клика на DataGridView
            dataGridViewStatus.CellDoubleClick += DataGridView_CellDoubleClick;
            dataGridViewCategories.CellDoubleClick += DataGridView_CellDoubleClick;
            dataGridViewPresents.CellDoubleClick += DataGridView_CellDoubleClick;

            // Настройка одиночного клика для выделения строки
            dataGridViewStatus.CellClick += DataGridView_CellClick;
            dataGridViewCategories.CellClick += DataGridView_CellClick;
            dataGridViewPresents.CellClick += DataGridView_CellClick;

            SetupDataGridViewRowSelection();

            // ПОДПИСЫВАЕМСЯ НА СОБЫТИЕ ЗАКРЫТИЯ ФОРМЫ
            this.FormClosing += Spravochnici_FormClosing;
        }

        // ОБРАБОТЧИК - при нажатии на крестик
        private void Spravochnici_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Visible = false;
                AdminForm admin = new AdminForm();
                admin.Show();
            }
        }

        // ОБРАБОТЧИК - клик по ячейке DataGridView для выделения строки
        private void DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridView dgv = sender as DataGridView;
                if (dgv != null)
                {
                    dgv.ClearSelection();
                    dgv.Rows[e.RowIndex].Selected = true;
                }
            }
        }

        // ОБРАБОТЧИК - кнопка Редактировать
        private void ButtonEdit_Click(object sender, EventArgs e)
        {
            DataGridView activeDGV = GetActiveDataGridView();

            if (activeDGV == null || activeDGV.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись для редактирования!", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            PrepareForEdit();
        }

        // ОБРАБОТЧИК - кнопка Сохранить
        private void ButtonSave_Click(object sender, EventArgs e)
        {
            SaveChanges();
        }

        private void SetupDataGridViewRowSelection()
        {
            dataGridViewStatus.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewStatus.MultiSelect = false;

            dataGridViewCategories.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewCategories.MultiSelect = false;

            dataGridViewPresents.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewPresents.MultiSelect = false;
        }

        private void SetMaxLengthLimits()
        {
            textBoxStatusName.MaxLength = 255;
            textBoxCategoryName.MaxLength = 255;
            textBoxPresentName.MaxLength = 255;
            textBoxFromPrice.MaxLength = 20;
        }

        private void SetupPriceTextBox(TextBox textBox)
        {
            textBox.KeyPress += TextBoxPrice_KeyPress;
            textBox.TextChanged += TextBoxPrice_TextChanged;
            textBox.Enter += TextBoxPrice_Enter;
            textBox.Leave += TextBoxPrice_Leave;
            textBox.TextAlign = HorizontalAlignment.Right;
        }

        private void TextBoxPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (char.IsDigit(e.KeyChar) ||
                e.KeyChar == (char)Keys.Back ||
                e.KeyChar == ',')
            {
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
                int cursorPos = textBox.SelectionStart;
                string originalText = textBox.Text;

                if (string.IsNullOrEmpty(originalText))
                {
                    isFormatting = false;
                    return;
                }

                string cleanText = "";
                foreach (char c in originalText)
                {
                    if (char.IsDigit(c) || c == ',')
                        cleanText += c;
                }

                int commaIndex = cleanText.IndexOf(',');
                if (commaIndex != -1)
                {
                    string beforeComma = cleanText.Substring(0, commaIndex + 1);
                    string afterComma = cleanText.Substring(commaIndex + 1).Replace(",", "");
                    cleanText = beforeComma + afterComma;
                }

                if (cleanText.Contains(","))
                {
                    string[] parts = cleanText.Split(',');
                    if (parts.Length > 1 && parts[1].Length > 2)
                    {
                        cleanText = parts[0] + "," + parts[1].Substring(0, 2);
                    }
                }

                if (cleanText.Contains(","))
                {
                    string[] parts = cleanText.Split(',');
                    string beforeCommaPart = parts[0];
                    beforeCommaPart = beforeCommaPart.TrimStart('0');
                    if (beforeCommaPart.Length > 8)
                    {
                        beforeCommaPart = beforeCommaPart.Substring(0, 8);
                        cleanText = beforeCommaPart + "," + (parts.Length > 1 ? parts[1] : "00");
                    }
                }
                else
                {
                    string temp = cleanText.TrimStart('0');
                    if (temp.Length > 8)
                    {
                        cleanText = temp.Substring(0, 8);
                    }
                }

                if (!string.IsNullOrEmpty(cleanText))
                {
                    int nonFormatChars = 0;
                    for (int i = 0; i < cursorPos && i < originalText.Length; i++)
                    {
                        if (char.IsDigit(originalText[i]) || originalText[i] == ',')
                            nonFormatChars++;
                    }
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
            if (!string.IsNullOrEmpty(textBox.Text))
            {
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

        private void FormatPriceTextBoxOnLeave(TextBox textBox)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "";
                return;
            }

            string text = textBox.Text.Trim();
            text = text.Replace(".", ",");
            string cleanText = new string(text.Where(c => char.IsDigit(c) || c == ',').ToArray());

            if (string.IsNullOrEmpty(cleanText))
            {
                textBox.Text = "";
                return;
            }

            int commaIndex = cleanText.IndexOf(',');
            if (commaIndex != -1)
            {
                string beforeComma = cleanText.Substring(0, commaIndex + 1);
                string afterComma = cleanText.Substring(commaIndex + 1).Replace(",", "");
                cleanText = beforeComma + afterComma;
            }

            if (cleanText.Contains(","))
            {
                string[] parts = cleanText.Split(',');
                string beforeCommaPart = parts[0];
                beforeCommaPart = beforeCommaPart.TrimStart('0');
                if (beforeCommaPart.Length > 8)
                {
                    beforeCommaPart = beforeCommaPart.Substring(0, 8);
                    cleanText = beforeCommaPart + "," + (parts.Length > 1 ? parts[1] : "00");
                }
                if (parts.Length > 1 && parts[1].Length > 2)
                {
                    cleanText = beforeCommaPart + "," + parts[1].Substring(0, 2);
                }
            }

            if (decimal.TryParse(cleanText, NumberStyles.Any, russianCulture, out decimal value))
            {
                value = Math.Round(value, 2);
                decimal maxValue = 99999999.99m;
                if (value > maxValue)
                {
                    value = maxValue;
                }
                textBox.Text = value.ToString("N2", russianCulture) + " ₽";
            }
            else
            {
                textBox.Text = "";
            }
        }

        private decimal GetPriceFromFormattedText(string formattedText)
        {
            if (string.IsNullOrWhiteSpace(formattedText))
                return 0;

            string cleanText = formattedText.Replace(" ", "").Replace("₽", "").Trim();
            return decimal.TryParse(cleanText, NumberStyles.Any, russianCulture, out decimal value) ? Math.Round(value, 2) : 0;
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

        private void tabConrol1_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadDataForSelectedTab();
            ResetEditMode();
        }

        private void ResetEditMode()
        {
            isEditMode = false;
            ClearInputFields(tabConrol1.SelectedTab?.Name);
            ClearTags(tabConrol1.SelectedTab?.Name);

            // Показываем кнопку "Добавить", скрываем "Сохранить"
            AddButton.Visible = true;
            buttonSave.Visible = false;
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
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

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
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

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
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

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
                            if (string.IsNullOrWhiteSpace(textBoxStatusName.Text))
                            {
                                MessageBox.Show("Введите название статуса!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                textBoxStatusName.Focus();
                                return;
                            }

                            string statusName = textBoxStatusName.Text.Trim();
                            if (statusName.Length > 255)
                            {
                                MessageBox.Show("Название статуса не должно превышать 255 символов!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                textBoxStatusName.Focus();
                                return;
                            }

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
                            if (string.IsNullOrWhiteSpace(textBoxCategoryName.Text))
                            {
                                MessageBox.Show("Введите название категории!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                textBoxCategoryName.Focus();
                                return;
                            }

                            string categoryName = textBoxCategoryName.Text.Trim();
                            if (categoryName.Length > 255)
                            {
                                MessageBox.Show("Название категории не должно превышать 255 символов!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                textBoxCategoryName.Focus();
                                return;
                            }

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
                            if (string.IsNullOrWhiteSpace(textBoxPresentName.Text))
                            {
                                MessageBox.Show("Введите название подарка!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                textBoxPresentName.Focus();
                                return;
                            }

                            decimal price = GetPriceFromFormattedText(textBoxFromPrice.Text);
                            if (price <= 0)
                            {
                                MessageBox.Show("Введите корректную сумму! Значение должно быть больше 0.", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                textBoxFromPrice.Focus();
                                return;
                            }

                            string presentName = textBoxPresentName.Text.Trim();
                            if (presentName.Length > 255)
                            {
                                MessageBox.Show("Название подарка не должно превышать 255 символов!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                textBoxPresentName.Focus();
                                return;
                            }

                            if (IsPresentDuplicate(presentName, connection))
                            {
                                MessageBox.Show("Подарок с таким названием уже существует!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                textBoxPresentName.Focus();
                                return;
                            }

                            decimal maxPrice = 99999999.99m;
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
                        ClearInputFields(selectedTab);
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
                else if (mysqlEx.Number == 1048)
                {
                    MessageBox.Show("Обязательные поля не заполнены!", "Ошибка",
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

                            if (string.IsNullOrWhiteSpace(textBoxStatusName.Text))
                            {
                                MessageBox.Show("Введите название статуса!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                textBoxStatusName.Focus();
                                return;
                            }

                            string statusName = textBoxStatusName.Text.Trim();
                            if (statusName.Length > 255)
                            {
                                MessageBox.Show("Название статуса не должно превышать 255 символов!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                textBoxStatusName.Focus();
                                return;
                            }

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

                            if (string.IsNullOrWhiteSpace(textBoxCategoryName.Text))
                            {
                                MessageBox.Show("Введите название категории!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                textBoxCategoryName.Focus();
                                return;
                            }

                            string categoryName = textBoxCategoryName.Text.Trim();
                            if (categoryName.Length > 255)
                            {
                                MessageBox.Show("Название категории не должно превышать 255 символов!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                textBoxCategoryName.Focus();
                                return;
                            }

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

                            if (string.IsNullOrWhiteSpace(textBoxPresentName.Text))
                            {
                                MessageBox.Show("Введите название подарка!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                textBoxPresentName.Focus();
                                return;
                            }

                            decimal price = GetPriceFromFormattedText(textBoxFromPrice.Text);
                            if (price <= 0)
                            {
                                MessageBox.Show("Введите корректную сумму!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                textBoxFromPrice.Focus();
                                return;
                            }

                            string presentName = textBoxPresentName.Text.Trim();
                            if (presentName.Length > 255)
                            {
                                MessageBox.Show("Название подарка не должно превышать 255 символов!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                textBoxPresentName.Focus();
                                return;
                            }

                            if (IsPresentDuplicate(presentName, connection, id))
                            {
                                MessageBox.Show("Подарок с таким названием уже существует!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                textBoxPresentName.Focus();
                                return;
                            }

                            decimal maxPrice = 99999999.99m;
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
                        ResetEditMode();
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
                if (mysqlEx.Number == 1048)
                {
                    MessageBox.Show("Обязательные поля не заполнены!", "Ошибка",
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

            string recordName = selectedRow.Cells[nameColumnName].Value?.ToString() ?? id.ToString();

            if (HasRelatedRecords(id, tableName, selectedTab))
            {
                MessageBox.Show($"Невозможно удалить '{recordName}', так как существуют связанные записи!\n\nСначала удалите все связанные записи.",
                    "Ошибка удаления",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

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
                            query = "SELECT COUNT(*) FROM dishes WHERE id_category = @id";
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
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

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
            catch
            {
                return "Запись имеет связанные данные в системе";
            }
        }

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

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            LoadDataForSelectedTab();
        }

        private void DataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridView dgv = sender as DataGridView;
                if (dgv != null)
                {
                    dgv.ClearSelection();
                    dgv.Rows[e.RowIndex].Selected = true;
                    PrepareForEdit();
                }
            }
        }

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

                    object priceValue = selectedRow.Cells["От какой суммы"].Value;
                    if (priceValue != null && decimal.TryParse(priceValue.ToString(), out decimal price))
                    {
                        textBoxFromPrice.Text = price.ToString("N2", russianCulture) + " ₽";
                    }
                    else
                    {
                        textBoxFromPrice.Text = "";
                    }
                    textBoxPresentName.Tag = presentId;
                    break;
            }

            isEditMode = true;

            // Скрываем кнопку "Добавить", показываем "Сохранить"
            AddButton.Visible = false;
            buttonSave.Visible = true;

            MessageBox.Show("Запись выбрана для редактирования. Измените данные и нажмите 'Сохранить'",
                "Редактирование",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            ResetEditMode();
        }

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

        private void SetupRussianOnlyTextBox(TextBox textBox)
        {
            textBox.KeyPress += TextBoxRussianOnly_KeyPress;
            textBox.TextChanged += TextBoxRussianOnly_TextChanged;
            textBox.Leave += TextBoxRussianOnly_Leave;
        }

        private void TextBoxRussianOnly_KeyPress(object sender, KeyPressEventArgs e)
        {
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
                    textBox.Text = char.ToUpper(textBox.Text[0]) + textBox.Text.Substring(1).ToLower();
                }
            }
        }

        private bool IsRussianLetter(char c)
        {
            return (c >= 'А' && c <= 'Я') || (c >= 'а' && c <= 'я') || c == 'Ё' || c == 'ё';
        }

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

        private void AddButton_Click(object sender, EventArgs e)
        {
            AddNewRecord();
        }

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