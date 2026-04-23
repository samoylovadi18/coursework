using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;

namespace dump
{
    public partial class AdminMenu : Form
    {
        private DataTable dishesTable;
        private bool isEditMode = false;
        private int currentDishId = -1;
        private string originalDishName = "";
        private CultureInfo russianCulture = new CultureInfo("ru-RU");
        private bool isFormatting = false;
        private Image defaultImage;
        private string userRole;

        private byte[] currentDishPhotoBytes = null;
        private string currentPhotoHash = "";

        private string originalDishNameValue = "";
        private string originalCompoundValue = "";
        private decimal originalPriceValue = 0;
        private decimal originalCostValue = 0;
        private string originalWeightVolumeValue = "";
        private int originalCategoryIdValue = -1;
        private string originalCategoryNameValue = "";
        private byte[] originalPhotoBytes = null;

        private bool isFormattingSearch = false;
        private Dictionary<string, int> categoryDictionary = new Dictionary<string, int>();

        public AdminMenu(string role)
        {
            InitializeComponent();
            userRole = role;

            CreateDefaultImage();
            InitializeButtonStyles();
            SetupDataGridView();
            LoadDishes();
            RefreshEditCategories();
            HideEditPanel();
            InitializeEditPanelAppearance();
            SetupValidationTextBoxes();

            ApplyRoleBasedVisibility();

            btnAdd.Click += AddButton_Click;
            btnEdit.Click += EditButton_Click;
            btnDelete.Click += DeleteButton_Click;
            btnCancel.Click += CancelButton_Click;
            dgvDishes.SelectionChanged += DataGridView_SelectionChanged;
            txtSearch.TextChanged += SearchTextBox_TextChanged;
            comboCategoryFilter.SelectedIndexChanged += CategoryFilter_SelectedIndexChanged;

            btnUploadPhoto.Click += BtnUploadPhoto_Click;
            btnDeletePhoto.Click += BtnDeletePhoto_Click;

            btnCancel.Text = "Отмена";
            btnCancel.Font = new Font("Times New Roman", 14, FontStyle.Bold);
        }

        private void ApplyRoleBasedVisibility()
        {
            if (userRole == "director")
            {
                btnAdd.Visible = false;
                btnEdit.Visible = false;
                btnDelete.Visible = false;
                btnUploadPhoto.Visible = false;
                btnDeletePhoto.Visible = false;
                this.Text = "Просмотр меню (Режим директора)";
            }
            else
            {
                btnAdd.Visible = true;
                btnEdit.Visible = true;
                btnDelete.Visible = true;
                btnUploadPhoto.Visible = true;
                btnDeletePhoto.Visible = true;
                this.Text = "Управление меню (Режим администратора)";
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (defaultImage != null)
                    defaultImage.Dispose();

                if (pbDishPhoto.Image != null && pbDishPhoto.Image != defaultImage)
                    pbDishPhoto.Image.Dispose();

                if (components != null)
                    components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void CreateDefaultImage()
        {
            defaultImage = new Bitmap(80, 80);
            using (Graphics g = Graphics.FromImage(defaultImage))
            {
                g.Clear(Color.LightGray);
                using (Font font = new Font("Times New Roman", 8))
                {
                    g.DrawString("Нет фото", font, Brushes.Black, new PointF(15, 30));
                }
            }
            pbDishPhoto.Image = (Image)defaultImage.Clone();
        }

        private string ComputeImageHash(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                return "";

            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(imageBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        private bool CheckPhotoDuplicate(byte[] photoBytes, int excludeDishId = -1)
        {
            if (photoBytes == null || photoBytes.Length == 0)
                return false;

            try
            {
                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();
                    string query;
                    MySqlCommand command;

                    if (excludeDishId > 0)
                    {
                        query = "SELECT COUNT(*) FROM dishes WHERE photo IS NOT NULL AND MD5(photo) = @hash AND id_dish != @id";
                        command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@hash", ComputeImageHash(photoBytes));
                        command.Parameters.AddWithValue("@id", excludeDishId);
                    }
                    else
                    {
                        query = "SELECT COUNT(*) FROM dishes WHERE photo IS NOT NULL AND MD5(photo) = @hash";
                        command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@hash", ComputeImageHash(photoBytes));
                    }

                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private string GetDishNameByPhoto(byte[] photoBytes)
        {
            if (photoBytes == null || photoBytes.Length == 0)
                return "";

            try
            {
                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT dish_name FROM dishes WHERE photo IS NOT NULL AND MD5(photo) = @hash LIMIT 1";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@hash", ComputeImageHash(photoBytes));
                    return command.ExecuteScalar()?.ToString() ?? "";
                }
            }
            catch
            {
                return "";
            }
        }

        private void SetupDataGridView()
        {
            dgvDishes.AutoGenerateColumns = false;
            dgvDishes.RowTemplate.Height = 80;
            dgvDishes.RowTemplate.MinimumHeight = 80;
            dgvDishes.AllowUserToAddRows = false;
            dgvDishes.ReadOnly = true;
            dgvDishes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDishes.MultiSelect = false;
            dgvDishes.RowHeadersVisible = false;

            dgvDishes.Columns.Clear();

            DataGridViewImageColumn imageColumn = new DataGridViewImageColumn();
            imageColumn.Name = "photo";
            imageColumn.HeaderText = "Фото";
            imageColumn.DataPropertyName = "photo_image";
            imageColumn.Width = 80;
            imageColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
            dgvDishes.Columns.Add(imageColumn);

            DataGridViewTextBoxColumn nameColumn = new DataGridViewTextBoxColumn();
            nameColumn.Name = "dish_name";
            nameColumn.HeaderText = "Название";
            nameColumn.DataPropertyName = "dish_name";
            nameColumn.Width = 200;
            dgvDishes.Columns.Add(nameColumn);

            DataGridViewTextBoxColumn compoundColumn = new DataGridViewTextBoxColumn();
            compoundColumn.Name = "compound";
            compoundColumn.HeaderText = "Состав";
            compoundColumn.DataPropertyName = "compound";
            compoundColumn.Width = 300;
            compoundColumn.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvDishes.Columns.Add(compoundColumn);

            DataGridViewTextBoxColumn categoryColumn = new DataGridViewTextBoxColumn();
            categoryColumn.Name = "category_name";
            categoryColumn.HeaderText = "Категория";
            categoryColumn.DataPropertyName = "category_name";
            categoryColumn.Width = 150;
            dgvDishes.Columns.Add(categoryColumn);

            DataGridViewTextBoxColumn priceColumn = new DataGridViewTextBoxColumn();
            priceColumn.Name = "price_display";
            priceColumn.HeaderText = "Цена";
            priceColumn.Width = 120;
            dgvDishes.Columns.Add(priceColumn);

            DataGridViewTextBoxColumn costColumn = new DataGridViewTextBoxColumn();
            costColumn.Name = "cost_display";
            costColumn.HeaderText = "Себестоимость";
            costColumn.Width = 120;
            dgvDishes.Columns.Add(costColumn);

            DataGridViewTextBoxColumn weightColumn = new DataGridViewTextBoxColumn();
            weightColumn.Name = "weight_volume";
            weightColumn.HeaderText = "Вес/Объем";
            weightColumn.DataPropertyName = "weight_volume";
            weightColumn.Width = 120;
            dgvDishes.Columns.Add(weightColumn);

            DataGridViewTextBoxColumn idColumn = new DataGridViewTextBoxColumn();
            idColumn.Name = "id_dish";
            idColumn.DataPropertyName = "id_dish";
            idColumn.Visible = false;
            dgvDishes.Columns.Add(idColumn);

            DataGridViewTextBoxColumn priceValueColumn = new DataGridViewTextBoxColumn();
            priceValueColumn.Name = "price";
            priceValueColumn.DataPropertyName = "price";
            priceValueColumn.Visible = false;
            dgvDishes.Columns.Add(priceValueColumn);

            DataGridViewTextBoxColumn costValueColumn = new DataGridViewTextBoxColumn();
            costValueColumn.Name = "cost";
            costValueColumn.DataPropertyName = "cost";
            costValueColumn.Visible = false;
            dgvDishes.Columns.Add(costValueColumn);

            dgvDishes.ColumnHeadersHeight = 50;
            dgvDishes.ColumnHeadersDefaultCellStyle.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            dgvDishes.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvDishes.DefaultCellStyle.Font = new Font("Times New Roman", 11);
            dgvDishes.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvDishes.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            Color headerColor = Color.FromArgb(97, 173, 123);
            dgvDishes.ColumnHeadersDefaultCellStyle.BackColor = headerColor;
            dgvDishes.EnableHeadersVisualStyles = false;

            dgvDishes.CellFormatting += DgvDishes_CellFormatting;
        }

        private void DgvDishes_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvDishes.Columns[e.ColumnIndex].Name == "price_display" && e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvDishes.Rows[e.RowIndex];
                if (row.Cells["price"].Value != null && row.Cells["price"].Value != DBNull.Value)
                {
                    decimal price = Convert.ToDecimal(row.Cells["price"].Value);
                    e.Value = price.ToString("N2", russianCulture) + " ₽";
                    e.FormattingApplied = true;
                }
            }
            else if (dgvDishes.Columns[e.ColumnIndex].Name == "cost_display" && e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvDishes.Rows[e.RowIndex];
                if (row.Cells["cost"].Value != null && row.Cells["cost"].Value != DBNull.Value)
                {
                    decimal cost = Convert.ToDecimal(row.Cells["cost"].Value);
                    e.Value = cost.ToString("N2", russianCulture) + " ₽";
                    e.FormattingApplied = true;
                }
            }
        }

        private void LoadDishes()
        {
            try
            {
                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();
                    string query = @"
                        SELECT d.id_dish, d.dish_name, d.compound, 
                               c.id_category, c.category_name, 
                               d.price, d.cost, d.weight_volume, d.photo 
                        FROM dishes d 
                        LEFT JOIN categories c ON d.id_category = c.id_category
                        ORDER BY d.dish_name";

                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                    dishesTable = new DataTable();
                    adapter.Fill(dishesTable);

                    if (!dishesTable.Columns.Contains("photo_image"))
                        dishesTable.Columns.Add("photo_image", typeof(Image));

                    foreach (DataRow row in dishesTable.Rows)
                    {
                        if (row["photo"] != DBNull.Value && row["photo"] != null)
                        {
                            try
                            {
                                byte[] imageData = (byte[])row["photo"];
                                using (MemoryStream ms = new MemoryStream(imageData))
                                {
                                    row["photo_image"] = Image.FromStream(ms);
                                }
                            }
                            catch
                            {
                                row["photo_image"] = defaultImage;
                            }
                        }
                        else
                        {
                            row["photo_image"] = defaultImage;
                        }
                    }

                    dgvDishes.DataSource = dishesTable;
                    UpdateCategoryFilter();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке блюд: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateCategoryFilter()
        {
            try
            {
                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT id_category, category_name FROM categories ORDER BY category_name";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();

                    comboCategoryFilter.Items.Clear();
                    comboCategoryFilter.Items.Add("Все категории");

                    while (reader.Read())
                    {
                        comboCategoryFilter.Items.Add(reader["category_name"].ToString());
                    }

                    if (comboCategoryFilter.Items.Count > 0)
                        comboCategoryFilter.SelectedIndex = 0;

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке категорий: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshEditCategories()
        {
            try
            {
                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT id_category, category_name FROM categories ORDER BY category_name";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();

                    comboEditCategory.Items.Clear();
                    categoryDictionary.Clear();
                    comboEditCategory.Items.Add("-- Выберите категорию --");

                    while (reader.Read())
                    {
                        string categoryName = reader["category_name"].ToString();
                        int categoryId = Convert.ToInt32(reader["id_category"]);

                        comboEditCategory.Items.Add(categoryName);
                        categoryDictionary[categoryName] = categoryId;
                    }
                    reader.Close();

                    if (comboEditCategory.Items.Count > 0)
                        comboEditCategory.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке категорий: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadDishPhoto(int dishId)
        {
            try
            {
                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT photo FROM dishes WHERE id_dish = @id";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@id", dishId);

                    object result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        currentDishPhotoBytes = (byte[])result;
                        currentPhotoHash = ComputeImageHash(currentDishPhotoBytes);

                        if (pbDishPhoto.Image != null && pbDishPhoto.Image != defaultImage)
                            pbDishPhoto.Image.Dispose();

                        using (MemoryStream ms = new MemoryStream(currentDishPhotoBytes))
                        {
                            pbDishPhoto.Image = Image.FromStream(ms);
                        }
                        btnDeletePhoto.Enabled = true;
                    }
                    else
                    {
                        ClearDishPhoto();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки фото: {ex.Message}");
                ClearDishPhoto();
            }
        }

        private void LoadDishData(int dishId)
        {
            try
            {
                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT * FROM dishes WHERE id_dish = @id";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@id", dishId);

                    MySqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        originalDishName = reader["dish_name"].ToString();
                        txtEditDishName.Text = originalDishName;
                        txtEditCompound.Text = reader["compound"].ToString();

                        decimal price = reader["price"] != DBNull.Value ? Convert.ToDecimal(reader["price"]) : 0;
                        numEditPrice.Text = price.ToString("N2", russianCulture) + " ₽";

                        decimal cost = reader["cost"] != DBNull.Value ? Convert.ToDecimal(reader["cost"]) : 0;
                        txtCost.Text = cost.ToString("N2", russianCulture) + " ₽";

                        txtWeightVolume.Text = reader["weight_volume"] != DBNull.Value ? reader["weight_volume"].ToString() : "";

                        int categoryId = reader["id_category"] != DBNull.Value ? Convert.ToInt32(reader["id_category"]) : -1;

                        bool categoryFound = false;
                        foreach (var item in categoryDictionary)
                        {
                            if (item.Value == categoryId)
                            {
                                comboEditCategory.SelectedItem = item.Key;
                                categoryFound = true;
                                break;
                            }
                        }

                        if (!categoryFound && comboEditCategory.Items.Count > 0)
                            comboEditCategory.SelectedIndex = 0;
                    }
                    reader.Close();
                }

                LoadDishPhoto(dishId);
                SaveOriginalValues();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearDishPhoto()
        {
            if (pbDishPhoto.Image != null && pbDishPhoto.Image != defaultImage)
                pbDishPhoto.Image.Dispose();

            pbDishPhoto.Image = (Image)defaultImage.Clone();
            currentDishPhotoBytes = null;
            currentPhotoHash = "";
            btnDeletePhoto.Enabled = false;
        }

        private void BtnUploadPhoto_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                openFileDialog.Title = "Выберите фотографию блюда";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(openFileDialog.FileName);
                        if (fileInfo.Length > 5 * 1024 * 1024)
                        {
                            MessageBox.Show("Размер файла не должен превышать 5 МБ!", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        byte[] newPhotoBytes = CompressImage(openFileDialog.FileName, 300, 300, 80);

                        int excludeId = isEditMode ? currentDishId : -1;
                        if (CheckPhotoDuplicate(newPhotoBytes, excludeId))
                        {
                            string existingDishName = GetDishNameByPhoto(newPhotoBytes);
                            string message = string.IsNullOrEmpty(existingDishName)
                                ? "Это фото уже используется для другого блюда!"
                                : $"Это фото уже используется для блюда '{existingDishName}'!";

                            MessageBox.Show(message + "\n\nКаждое блюдо должно иметь уникальное фото.",
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        currentDishPhotoBytes = newPhotoBytes;

                        if (pbDishPhoto.Image != null && pbDishPhoto.Image != defaultImage)
                            pbDishPhoto.Image.Dispose();

                        using (MemoryStream ms = new MemoryStream(currentDishPhotoBytes))
                        {
                            pbDishPhoto.Image = Image.FromStream(ms);
                        }
                        btnDeletePhoto.Enabled = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка загрузки фото: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnDeletePhoto_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Удалить фотографию?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
                ClearDishPhoto();
        }

        private byte[] CompressImage(string imagePath, int maxWidth, int maxHeight, int quality)
        {
            using (Image image = Image.FromFile(imagePath))
            {
                double ratio = Math.Min((double)maxWidth / image.Width, (double)maxHeight / image.Height);
                int newWidth = (int)(image.Width * ratio);
                int newHeight = (int)(image.Height * ratio);

                using (Bitmap newImage = new Bitmap(newWidth, newHeight))
                {
                    using (Graphics graphics = Graphics.FromImage(newImage))
                    {
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.DrawImage(image, 0, 0, newWidth, newHeight);
                    }

                    using (MemoryStream ms = new MemoryStream())
                    {
                        var jpegCodec = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()
                            .FirstOrDefault(c => c.MimeType == "image/jpeg");

                        if (jpegCodec != null)
                        {
                            var encoderParams = new System.Drawing.Imaging.EncoderParameters(1);
                            encoderParams.Param[0] = new System.Drawing.Imaging.EncoderParameter(
                                System.Drawing.Imaging.Encoder.Quality, quality);
                            newImage.Save(ms, jpegCodec, encoderParams);
                        }
                        else
                        {
                            newImage.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        }
                        return ms.ToArray();
                    }
                }
            }
        }

        private void InitializeButtonStyles()
        {
            SetupButtonStyle(btnAdd);
            SetupButtonStyle(btnEdit);
            SetupButtonStyle(btnDelete);
            SetupButtonStyle(buttonReset);
            SetupButtonStyle(btnUploadPhoto);
            SetupButtonStyle(btnDeletePhoto);
 
        }

        private void SetupButtonStyle(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = Color.Black;
            button.BackColor = Color.DarkSeaGreen;
            button.ForeColor = Color.Black;
            button.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            button.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;
        }

        private void InitializeEditPanelAppearance()
        {
            panelEditDish.BorderStyle = BorderStyle.None;
            panelEditDish.BackColor = Color.WhiteSmoke;
            panelEditDish.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, panelEditDish.ClientRectangle,
                    Color.DarkGray, 4, ButtonBorderStyle.Solid,
                    Color.DarkGray, 4, ButtonBorderStyle.Solid,
                    Color.DarkGray, 4, ButtonBorderStyle.Solid,
                    Color.DarkGray, 4, ButtonBorderStyle.Solid);
            };
        }

        private void SetupValidationTextBoxes()
        {
            txtEditDishName.KeyPress += (s, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !IsRussianLetter(e.KeyChar) && e.KeyChar != '-' && e.KeyChar != ' ')
                    e.Handled = true;
            };

            txtSearch.KeyPress += (s, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !IsRussianLetter(e.KeyChar) && e.KeyChar != ' ')
                    e.Handled = true;
            };

            txtSearch.TextChanged += (s, e) =>
            {
                if (isFormattingSearch) return;
                isFormattingSearch = true;

                try
                {
                    string text = txtSearch.Text;
                    int cursorPos = txtSearch.SelectionStart;

                    string newText = "";
                    bool lastWasSpace = false;
                    foreach (char c in text)
                    {
                        if (c == ' ')
                        {
                            if (!lastWasSpace)
                            {
                                newText += c;
                                lastWasSpace = true;
                            }
                        }
                        else
                        {
                            newText += c;
                            lastWasSpace = false;
                        }
                    }

                    if (newText.Length > 0 && char.IsLower(newText[0]) && IsRussianLetter(newText[0]))
                    {
                        newText = char.ToUpper(newText[0]) + newText.Substring(1);
                    }

                    if (newText != text)
                    {
                        txtSearch.Text = newText;
                        txtSearch.SelectionStart = Math.Min(cursorPos, newText.Length);
                    }
                }
                finally
                {
                    isFormattingSearch = false;
                }
            };

            txtCost.KeyPress += TextBoxPrice_KeyPress;
            numEditPrice.KeyPress += TextBoxPrice_KeyPress;
            txtCost.Leave += (s, e) => FormatPriceTextBoxOnLeave(txtCost);
            numEditPrice.Leave += (s, e) => FormatPriceTextBoxOnLeave(numEditPrice);
        }

        private bool IsRussianLetter(char c)
        {
            return (c >= 'А' && c <= 'Я') || (c >= 'а' && c <= 'я') || c == 'Ё' || c == 'ё';
        }

        private void TextBoxPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != ',')
                e.Handled = true;

            TextBox textBox = sender as TextBox;
            if (e.KeyChar == ',' && textBox.Text.Contains(","))
                e.Handled = true;
        }

        private void FormatPriceTextBoxOnLeave(TextBox textBox)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "0.00 ₽";
                return;
            }

            string cleanText = new string(textBox.Text.Where(c => char.IsDigit(c) || c == ',').ToArray());
            if (decimal.TryParse(cleanText, out decimal value))
            {
                textBox.Text = value.ToString("N2", russianCulture) + " ₽";
            }
            else
            {
                textBox.Text = "0.00 ₽";
            }
        }

        private decimal GetPriceFromFormattedText(string formattedText)
        {
            if (string.IsNullOrWhiteSpace(formattedText))
                return 0;

            string cleanText = formattedText.Replace(" ", "").Replace("₽", "").Trim();
            return decimal.TryParse(cleanText, NumberStyles.Any, russianCulture, out decimal value) ? value : 0;
        }

        private int GetSelectedCategoryId()
        {
            if (comboEditCategory.SelectedIndex <= 0)
                return -1;

            string selectedCategory = comboEditCategory.SelectedItem.ToString();
            if (categoryDictionary.ContainsKey(selectedCategory))
                return categoryDictionary[selectedCategory];

            return -1;
        }

        private void ShowEditPanel()
        {
            panelEditDish.Visible = true;
            panelEditDish.BringToFront();
            editLabel.Text = isEditMode ? "Редактирование блюда" : "Добавление нового блюда";
        }

        private void HideEditPanel()
        {
            panelEditDish.Visible = false;
            ClearEditForm();
        }

        private void ClearEditForm()
        {
            txtEditDishName.Text = "";
            txtEditCompound.Text = "";
            numEditPrice.Text = "";
            txtCost.Text = "";
            txtWeightVolume.Text = "";
            if (comboEditCategory.Items.Count > 0)
                comboEditCategory.SelectedIndex = 0;
            ClearDishPhoto();
            currentDishPhotoBytes = null;
        }

        private void SaveOriginalValues()
        {
            originalDishNameValue = txtEditDishName.Text.Trim();
            originalCompoundValue = txtEditCompound.Text.Trim();
            originalPriceValue = GetPriceFromFormattedText(numEditPrice.Text);
            originalCostValue = GetPriceFromFormattedText(txtCost.Text);
            originalWeightVolumeValue = txtWeightVolume.Text.Trim();
            originalCategoryIdValue = GetSelectedCategoryId();
            originalCategoryNameValue = comboEditCategory.SelectedIndex > 0 ? comboEditCategory.SelectedItem.ToString() : "";

            if (currentDishPhotoBytes != null)
            {
                originalPhotoBytes = new byte[currentDishPhotoBytes.Length];
                Array.Copy(currentDishPhotoBytes, originalPhotoBytes, currentDishPhotoBytes.Length);
            }
            else
            {
                originalPhotoBytes = null;
            }
        }

        private bool HasChanges()
        {
            if (txtEditDishName.Text.Trim() != originalDishNameValue) return true;
            if (txtEditCompound.Text.Trim() != originalCompoundValue) return true;
            if (GetPriceFromFormattedText(numEditPrice.Text) != originalPriceValue) return true;
            if (GetPriceFromFormattedText(txtCost.Text) != originalCostValue) return true;
            if (txtWeightVolume.Text.Trim() != originalWeightVolumeValue) return true;
            if (GetSelectedCategoryId() != originalCategoryIdValue) return true;

            if (currentDishPhotoBytes == null && originalPhotoBytes != null) return true;
            if (currentDishPhotoBytes != null && originalPhotoBytes == null) return true;

            if (currentDishPhotoBytes != null && originalPhotoBytes != null)
            {
                if (currentDishPhotoBytes.Length != originalPhotoBytes.Length) return true;
                for (int i = 0; i < currentDishPhotoBytes.Length; i++)
                    if (currentDishPhotoBytes[i] != originalPhotoBytes[i]) return true;
            }
            return false;
        }

        private bool CheckDishDuplicate(string dishName, int excludeDishId = -1)
        {
            try
            {
                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();
                    string query = excludeDishId > 0
                        ? "SELECT COUNT(*) FROM dishes WHERE LOWER(TRIM(dish_name)) = LOWER(TRIM(@name)) AND id_dish != @id"
                        : "SELECT COUNT(*) FROM dishes WHERE LOWER(TRIM(dish_name)) = LOWER(TRIM(@name))";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@name", dishName);
                    if (excludeDishId > 0)
                        command.Parameters.AddWithValue("@id", excludeDishId);

                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
            catch { return false; }
        }

        private bool SaveDish()
        {
            if (string.IsNullOrWhiteSpace(txtEditDishName.Text))
            {
                MessageBox.Show("Введите название блюда!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEditDishName.Focus();
                return false;
            }

            if (comboEditCategory.SelectedIndex <= 0)
            {
                MessageBox.Show("Выберите категорию!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                comboEditCategory.Focus();
                return false;
            }

            decimal price = GetPriceFromFormattedText(numEditPrice.Text);
            if (price <= 0)
            {
                MessageBox.Show("Введите корректную цену!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numEditPrice.Focus();
                return false;
            }

            decimal cost = GetPriceFromFormattedText(txtCost.Text);
            if (cost < 0)
            {
                MessageBox.Show("Себестоимость не может быть отрицательной!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCost.Focus();
                return false;
            }

            string dishName = txtEditDishName.Text.Trim();

            if (CheckDishDuplicate(dishName, isEditMode ? currentDishId : -1))
            {
                MessageBox.Show($"Блюдо с названием '{dishName}' уже существует!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEditDishName.Focus();
                return false;
            }

            if (currentDishPhotoBytes != null && CheckPhotoDuplicate(currentDishPhotoBytes, isEditMode ? currentDishId : -1))
            {
                MessageBox.Show("Это фото уже используется для другого блюда!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            try
            {
                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();
                    int categoryId = GetSelectedCategoryId();
                    string weightVolume = txtWeightVolume.Text.Trim();

                    if (isEditMode)
                    {
                        string query = @"UPDATE dishes SET 
                                        dish_name = @name, 
                                        compound = @compound, 
                                        id_category = @category, 
                                        price = @price, 
                                        cost = @cost,
                                        weight_volume = @weight_volume,
                                        photo = @photo 
                                    WHERE id_dish = @id";

                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@name", dishName);
                        command.Parameters.AddWithValue("@compound", txtEditCompound.Text.Trim());
                        command.Parameters.AddWithValue("@category", categoryId);
                        command.Parameters.AddWithValue("@price", price);
                        command.Parameters.AddWithValue("@cost", cost);
                        command.Parameters.AddWithValue("@weight_volume", string.IsNullOrEmpty(weightVolume) ? (object)DBNull.Value : weightVolume);
                        command.Parameters.AddWithValue("@photo", currentDishPhotoBytes ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@id", currentDishId);
                        command.ExecuteNonQuery();

                        MessageBox.Show("Блюдо успешно обновлено!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        string query = @"INSERT INTO dishes (dish_name, compound, id_category, price, cost, weight_volume, photo) 
                                        VALUES (@name, @compound, @category, @price, @cost, @weight_volume, @photo)";

                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@name", dishName);
                        command.Parameters.AddWithValue("@compound", txtEditCompound.Text.Trim());
                        command.Parameters.AddWithValue("@category", categoryId);
                        command.Parameters.AddWithValue("@price", price);
                        command.Parameters.AddWithValue("@cost", cost);
                        command.Parameters.AddWithValue("@weight_volume", string.IsNullOrEmpty(weightVolume) ? (object)DBNull.Value : weightVolume);
                        command.Parameters.AddWithValue("@photo", currentDishPhotoBytes ?? (object)DBNull.Value);
                        command.ExecuteNonQuery();

                        MessageBox.Show("Блюдо успешно добавлено!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                HideEditPanel();
                LoadDishes();
                UpdateCategoryFilter();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            isEditMode = false;
            currentDishId = -1;
            RefreshEditCategories();
            ClearEditForm();
            if (comboEditCategory.Items.Count > 0)
                comboEditCategory.SelectedIndex = 0;
            SaveOriginalValues();
            ShowEditPanel();
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            if (dgvDishes.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите блюдо для редактирования!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            isEditMode = true;
            currentDishId = Convert.ToInt32(dgvDishes.SelectedRows[0].Cells["id_dish"].Value);
            RefreshEditCategories();
            LoadDishData(currentDishId);
            ShowEditPanel();
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (dgvDishes.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите блюдо для удаления!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Удалить выбранное блюдо?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                int dishId = Convert.ToInt32(dgvDishes.SelectedRows[0].Cells["id_dish"].Value);

                try
                {
                    using (MySqlConnection connection = SettingsBD.GetConnection())
                    {
                        connection.Open();
                        MySqlCommand command = new MySqlCommand("DELETE FROM dishes WHERE id_dish = @id", connection);
                        command.Parameters.AddWithValue("@id", dishId);
                        command.ExecuteNonQuery();
                        MessageBox.Show("Блюдо удалено!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadDishes();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ИСПРАВЛЕННЫЙ МЕТОД CancelButton_Click
        private void CancelButton_Click(object sender, EventArgs e)
        {
            // Проверяем, есть ли несохраненные изменения
            if (HasChanges())
            {
                DialogResult result = MessageBox.Show(
                    "У вас есть несохраненные изменения.\n\nСохранить изменения?",
                    "Несохраненные изменения",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Сохраняем и закрываем
                    SaveDish();
                }
                else if (result == DialogResult.No)
                {
                    // Не сохраняем, просто закрываем панель
                    HideEditPanel();
                }
                // Если Cancel - ничего не делаем, остаемся в режиме редактирования
            }
            else
            {
                // Изменений нет - просто закрываем панель
                HideEditPanel();
            }
        }

        private void DataGridView_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = dgvDishes.SelectedRows.Count > 0;
            btnEdit.Enabled = hasSelection;
            btnDelete.Enabled = hasSelection;
        }

        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            FilterData();
        }

        private void CategoryFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterData();
        }

        private void FilterData()
        {
            if (dishesTable == null) return;

            string searchText = txtSearch.Text.ToLower().Trim();
            string selectedCategory = comboCategoryFilter.SelectedItem?.ToString();

            var filteredRows = dishesTable.AsEnumerable().Where(row =>
            {
                bool matchesSearch = string.IsNullOrEmpty(searchText) ||
                    row["dish_name"].ToString().ToLower().Contains(searchText);
                bool matchesCategory = selectedCategory == "Все категории" ||
                    string.IsNullOrEmpty(selectedCategory) ||
                    row["category_name"].ToString() == selectedCategory;
                return matchesSearch && matchesCategory;
            });

            dgvDishes.DataSource = filteredRows.Any() ? filteredRows.CopyToDataTable() : dishesTable.Clone();
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            if (comboCategoryFilter.Items.Count > 0)
                comboCategoryFilter.SelectedIndex = 0;
            if (dishesTable != null)
                dgvDishes.DataSource = dishesTable;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            if (userRole == "admin")
            {
                AdminForm admin = new AdminForm();
                admin.Show();
            }
            else
            {
                DirectorForm director = new DirectorForm();
                director.Show();
            }
        }

        private void AdminMenu_Load(object sender, EventArgs e)
        {
            LoadDishes();
        }

        private void pictureBox2_Click_1(object sender, EventArgs e)
        {
            this.Visible = false;
            AdminForm admin = new AdminForm();
            admin.Show();
        }
    }
}