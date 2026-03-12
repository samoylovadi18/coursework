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
        private Dictionary<int, Image> dishImages = new Dictionary<int, Image>();
        private Image defaultImage;

        // НОВОЕ ПОЛЕ ДЛЯ РОЛИ ПОЛЬЗОВАТЕЛЯ
        private string userRole; // "admin" или "director"

        // НОВЫЕ ПОЛЯ ДЛЯ РАБОТЫ С ФОТО
        private byte[] currentDishPhotoBytes = null;
        private string currentPhotoPath = "";
        private string currentPhotoHash = ""; // Хеш текущего фото

        // ПОЛЯ ДЛЯ ОТСЛЕЖИВАНИЯ ИЗМЕНЕНИЙ
        private string originalDishNameValue = "";
        private string originalCompoundValue = "";
        private decimal originalPriceValue = 0;
        private decimal originalCostValue = 0; // НОВОЕ поле
        private int originalCategoryIdValue = -1;
        private byte[] originalPhotoBytes = null;
        private string originalPhotoHash = ""; // Хеш оригинального фото

        // ИЗМЕНЕННЫЙ КОНСТРУКТОР - ПРИНИМАЕТ РОЛЬ
        public AdminMenu(string role)
        {
            InitializeComponent();
            userRole = role; // Сохраняем роль пользователя

            CreateDefaultImage();
            InitializeButtonStyles();
            LoadDishes();
            RefreshEditCategories();
            SetupDataGridView();
            HideEditPanel();
            InitializeEditPanelAppearance();
            SetupValidationTextBoxes();

            // Применяем настройки видимости в зависимости от роли
            ApplyRoleBasedVisibility();

            // Подписка на события
            btnAdd.Click += AddButton_Click;
            btnEdit.Click += EditButton_Click;
            btnDelete.Click += DeleteButton_Click;
            btnCancel.Click += CancelButton_Click;
            dgvDishes.SelectionChanged += DataGridView_SelectionChanged;
            txtSearch.TextChanged += SearchTextBox_TextChanged;
            comboCategoryFilter.SelectedIndexChanged += CategoryFilter_SelectedIndexChanged;

            // НОВЫЕ ПОДПИСКИ ДЛЯ ФОТО
            btnUploadPhoto.Click += BtnUploadPhoto_Click;
            btnDeletePhoto.Click += BtnDeletePhoto_Click;

            dgvDishes.ColumnHeaderMouseClick += (s, e) => {
                dgvDishes.ClearSelection();
            };

            // ДОПОЛНИТЕЛЬНАЯ НАСТРОЙКА ДЛЯ btnCancel
            btnCancel.Text = "Готово";
            btnCancel.Font = new Font("Times New Roman", 14, FontStyle.Bold);
        }

        // НОВЫЙ МЕТОД: ПРИМЕНЕНИЕ ВИДИМОСТИ В ЗАВИСИМОСТИ ОТ РОЛИ
        private void ApplyRoleBasedVisibility()
        {
            if (userRole == "director")
            {
                // Для директора скрываем кнопки добавления, редактирования и удаления
                btnAdd.Visible = false;
                btnEdit.Visible = false;
                btnDelete.Visible = false;

                // Также можно скрыть кнопки управления фото на панели редактирования,
                // но панель редактирования директору не будет доступна

                // Опционально: изменить заголовок формы
                this.Text = "Просмотр меню (Режим директора)";
            }
            else // admin
            {
                // Для администратора все кнопки видимы
                btnAdd.Visible = true;
                btnEdit.Visible = true;
                btnDelete.Visible = true;

                this.Text = "Управление меню (Режим администратора)";
            }
        }

        // ИСПРАВЛЕНО: Добавлен метод Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Освобождаем ресурсы изображений
                if (dishImages != null)
                {
                    foreach (var image in dishImages.Values)
                    {
                        image?.Dispose();
                    }
                    dishImages.Clear();
                }

                if (defaultImage != null)
                {
                    defaultImage.Dispose();
                }

                if (pbDishPhoto.Image != null && pbDishPhoto.Image != defaultImage)
                {
                    pbDishPhoto.Image.Dispose();
                }

                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        // СОЗДАЕМ ИЗОБРАЖЕНИЕ-ЗАГЛУШКУ
        private void CreateDefaultImage()
        {
            defaultImage = new Bitmap(150, 150);
            using (Graphics g = Graphics.FromImage(defaultImage))
            {
                g.Clear(Color.LightGray);
                using (Font font = new Font("Times New Roman", 12))
                {
                    g.DrawString("Нет фото", font, Brushes.Black, new PointF(30, 60));
                }
            }
            pbDishPhoto.Image = defaultImage;
        }

        // НОВЫЙ МЕТОД: ВЫЧИСЛЕНИЕ ХЕША ИЗОБРАЖЕНИЯ
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

        // НОВЫЙ МЕТОД: ПРОВЕРКА УНИКАЛЬНОСТИ ФОТО
        private bool CheckPhotoDuplicate(byte[] photoBytes, int excludeDishId = -1)
        {
            if (photoBytes == null || photoBytes.Length == 0)
                return false;

            try
            {
                string photoHash = ComputeImageHash(photoBytes);

                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();

                    string query;
                    MySqlCommand command;

                    if (excludeDishId > 0)
                    {
                        // При редактировании исключаем текущее блюдо
                        query = "SELECT COUNT(*) FROM dishes WHERE photo IS NOT NULL AND MD5(photo) = @hash AND id_dish != @id";
                        command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@hash", photoHash);
                        command.Parameters.AddWithValue("@id", excludeDishId);
                    }
                    else
                    {
                        // При добавлении проверяем все блюда
                        query = "SELECT COUNT(*) FROM dishes WHERE photo IS NOT NULL AND MD5(photo) = @hash";
                        command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@hash", photoHash);
                    }

                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке уникальности фото: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // НОВЫЙ МЕТОД: ПОЛУЧЕНИЕ НАЗВАНИЯ БЛЮДА ПО ФОТО
        private string GetDishNameByPhoto(byte[] photoBytes)
        {
            if (photoBytes == null || photoBytes.Length == 0)
                return "";

            try
            {
                string photoHash = ComputeImageHash(photoBytes);

                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();

                    string query = "SELECT dish_name FROM dishes WHERE photo IS NOT NULL AND MD5(photo) = @hash LIMIT 1";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@hash", photoHash);

                    object result = command.ExecuteScalar();
                    return result?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при поиске блюда по фото: {ex.Message}");
                return "";
            }
        }

        // НАСТРОЙКА DATA GRID VIEW - ПОЛНОСТЬЮ БЕЗ ФОТО
        private void SetupDataGridView()
        {
            dgvDishes.AutoGenerateColumns = false;

            dgvDishes.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvDishes.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvDishes.RowTemplate.Height = 60;
            dgvDishes.RowTemplate.MinimumHeight = 60;

            dgvDishes.Columns.Clear();

            // 1. КОЛОНКА ДЛЯ НАЗВАНИЯ
            DataGridViewTextBoxColumn nameColumn = new DataGridViewTextBoxColumn();
            nameColumn.Name = "dish_name";
            nameColumn.HeaderText = "Название";
            nameColumn.DataPropertyName = "dish_name";
            nameColumn.Width = 180;
            nameColumn.ReadOnly = true;
            nameColumn.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            nameColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvDishes.Columns.Add(nameColumn);

            // 2. КОЛОНКА ДЛЯ СОСТАВА
            DataGridViewTextBoxColumn compoundColumn = new DataGridViewTextBoxColumn();
            compoundColumn.Name = "compound";
            compoundColumn.HeaderText = "Состав";
            compoundColumn.DataPropertyName = "compound";
            compoundColumn.Width = 300;
            compoundColumn.ReadOnly = true;
            compoundColumn.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            compoundColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopCenter;
            dgvDishes.Columns.Add(compoundColumn);

            // 3. КОЛОНКА ДЛЯ КАТЕГОРИИ
            DataGridViewTextBoxColumn categoryColumn = new DataGridViewTextBoxColumn();
            categoryColumn.Name = "category_name";
            categoryColumn.HeaderText = "Категория";
            categoryColumn.DataPropertyName = "category_name";
            categoryColumn.Width = 150;
            categoryColumn.ReadOnly = true;
            categoryColumn.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            categoryColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvDishes.Columns.Add(categoryColumn);

            // 4. КОЛОНКА ДЛЯ ЦЕНЫ
            DataGridViewTextBoxColumn priceDisplayColumn = new DataGridViewTextBoxColumn();
            priceDisplayColumn.Name = "price_display";
            priceDisplayColumn.HeaderText = "Цена";
            priceDisplayColumn.Width = 120;
            priceDisplayColumn.ReadOnly = true;
            priceDisplayColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            priceDisplayColumn.DefaultCellStyle.Font = new Font("Times New Roman", 14);
            priceDisplayColumn.DefaultCellStyle.ForeColor = Color.DarkGreen;
            dgvDishes.Columns.Add(priceDisplayColumn);

            // 5. КОЛОНКА ДЛЯ СЕБЕСТОИМОСТИ (НОВАЯ)
            DataGridViewTextBoxColumn costDisplayColumn = new DataGridViewTextBoxColumn();
            costDisplayColumn.Name = "cost_display";
            costDisplayColumn.HeaderText = "Себестоимость";
            costDisplayColumn.Width = 120;
            costDisplayColumn.ReadOnly = true;
            costDisplayColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            costDisplayColumn.DefaultCellStyle.Font = new Font("Times New Roman", 14);
            costDisplayColumn.DefaultCellStyle.ForeColor = Color.DarkBlue;
            dgvDishes.Columns.Add(costDisplayColumn);

            // 6. КОЛОНКА ДЛЯ ВЕСА/ОБЪЕМА
            DataGridViewTextBoxColumn weightVolumeColumn = new DataGridViewTextBoxColumn();
            weightVolumeColumn.Name = "weight_volume";
            weightVolumeColumn.HeaderText = "Вес/Объем";
            weightVolumeColumn.DataPropertyName = "weight_volume";
            weightVolumeColumn.Width = 120;
            weightVolumeColumn.ReadOnly = true;
            weightVolumeColumn.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            weightVolumeColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvDishes.Columns.Add(weightVolumeColumn);

            // 7. СКРЫТАЯ КОЛОНКА ID
            DataGridViewTextBoxColumn idColumn = new DataGridViewTextBoxColumn();
            idColumn.Name = "id_dish";
            idColumn.HeaderText = "ID";
            idColumn.DataPropertyName = "id_dish";
            idColumn.Width = 50;
            idColumn.Visible = false;
            dgvDishes.Columns.Add(idColumn);

            // 8. СКРЫТАЯ КОЛОНКА ЦЕНЫ
            DataGridViewTextBoxColumn priceValueColumn = new DataGridViewTextBoxColumn();
            priceValueColumn.Name = "price";
            priceValueColumn.HeaderText = "Price";
            priceValueColumn.DataPropertyName = "price";
            priceValueColumn.Width = 80;
            priceValueColumn.Visible = false;
            dgvDishes.Columns.Add(priceValueColumn);

            // 9. СКРЫТАЯ КОЛОНКА СЕБЕСТОИМОСТИ (НОВАЯ)
            DataGridViewTextBoxColumn costValueColumn = new DataGridViewTextBoxColumn();
            costValueColumn.Name = "cost";
            costValueColumn.HeaderText = "Cost";
            costValueColumn.DataPropertyName = "cost";
            costValueColumn.Width = 80;
            costValueColumn.Visible = false;
            dgvDishes.Columns.Add(costValueColumn);

            // НАСТРОЙКА ЗАГОЛОВКОВ
            dgvDishes.ColumnHeadersHeight = 60;
            dgvDishes.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvDishes.ColumnHeadersDefaultCellStyle.Font = new Font("Times New Roman", 16);

            Color headerBackColor = Color.FromArgb(97, 173, 123);
            dgvDishes.ColumnHeadersDefaultCellStyle.BackColor = headerBackColor;
            dgvDishes.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;

            dgvDishes.EnableHeadersVisualStyles = false;
            dgvDishes.ColumnHeadersDefaultCellStyle.SelectionBackColor = headerBackColor;
            dgvDishes.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.Black;

            foreach (DataGridViewColumn column in dgvDishes.Columns)
            {
                column.HeaderCell.Style.BackColor = headerBackColor;
                column.HeaderCell.Style.ForeColor = Color.Black;
                column.HeaderCell.Style.SelectionBackColor = headerBackColor;
                column.HeaderCell.Style.SelectionForeColor = Color.Black;
                column.HeaderCell.Style.Font = new Font("Times New Roman", 16);
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            dgvDishes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvDishes.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;

            dgvDishes.GridColor = Color.Gray;
            dgvDishes.BorderStyle = BorderStyle.FixedSingle;
            dgvDishes.CellBorderStyle = DataGridViewCellBorderStyle.Single;

            dgvDishes.DefaultCellStyle.Font = new Font("Times New Roman", 14);
            dgvDishes.DefaultCellStyle.SelectionBackColor = Color.FromArgb(233, 242, 236);
            dgvDishes.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvDishes.DefaultCellStyle.BackColor = Color.White;
            dgvDishes.DefaultCellStyle.ForeColor = Color.Black;

            dgvDishes.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(233, 242, 236);
            dgvDishes.RowsDefaultCellStyle.SelectionForeColor = Color.Black;
            dgvDishes.RowsDefaultCellStyle.BackColor = Color.White;
            dgvDishes.RowsDefaultCellStyle.ForeColor = Color.Black;

            dgvDishes.RowHeadersVisible = false;
            dgvDishes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDishes.MultiSelect = false;
            dgvDishes.ReadOnly = true;

            dgvDishes.CellFormatting += DgvDishes_CellFormatting;
            dgvDishes.CellPainting += DgvDishes_CellPainting;

            dgvDishes.ColumnStateChanged += (s, e) =>
            {
                if (e.Column?.HeaderCell != null)
                {
                    e.Column.HeaderCell.Style.BackColor = headerBackColor;
                    e.Column.HeaderCell.Style.SelectionBackColor = headerBackColor;
                }
            };
        }

        // ФОРМАТИРОВАНИЕ ЯЧЕЕК DATA GRID VIEW - ТОЛЬКО ЦЕНА И СЕБЕСТОИМОСТЬ
        private void DgvDishes_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Форматирование цены
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
            // Форматирование себестоимости
            else if (dgvDishes.Columns[e.ColumnIndex].Name == "cost_display" && e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvDishes.Rows[e.RowIndex];
                if (row.Cells["cost"].Value != null && row.Cells["cost"].Value != DBNull.Value)
                {
                    decimal cost = Convert.ToDecimal(row.Cells["cost"].Value);
                    e.Value = cost.ToString("N2", russianCulture) + " ₽";
                    e.FormattingApplied = true;
                }
                else
                {
                    e.Value = "0.00 ₽";
                    e.FormattingApplied = true;
                }
            }
        }

        // ВЫРАВНИВАНИЕ ТЕКСТА В КОЛОНКЕ СОСТАВ
        private void DgvDishes_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 1 && e.Value != null)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);

                string text = e.Value.ToString();
                using (Font font = new Font("Times New Roman", 14))
                {
                    TextFormatFlags flags = TextFormatFlags.WordBreak |
                                           TextFormatFlags.VerticalCenter |
                                           TextFormatFlags.HorizontalCenter |
                                           TextFormatFlags.TextBoxControl;

                    Color textColor;
                    if ((e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected)
                    {
                        textColor = dgvDishes.DefaultCellStyle.SelectionForeColor;
                    }
                    else
                    {
                        textColor = dgvDishes.DefaultCellStyle.ForeColor;
                    }

                    TextRenderer.DrawText(e.Graphics, text, font, e.CellBounds,
                        textColor, flags);
                }

                e.Handled = true;
            }
        }

        // ЗАГРУЗКА ФОТО ИЗ БАЗЫ ДАННЫХ
        private Image LoadDishImage(int dishId)
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
                        byte[] imageData = (byte[])result;
                        using (MemoryStream ms = new MemoryStream(imageData))
                        {
                            return Image.FromStream(ms);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки фото: {ex.Message}");
            }

            return null;
        }

        // ЗАГРУЗКА ФОТО ДЛЯ РЕДАКТИРОВАНИЯ - ИСПРАВЛЕНО
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

                        // ИСПРАВЛЕНО: создаем копию массива байт
                        originalPhotoBytes = new byte[currentDishPhotoBytes.Length];
                        Array.Copy(currentDishPhotoBytes, originalPhotoBytes, currentDishPhotoBytes.Length);
                        originalPhotoHash = currentPhotoHash;

                        using (MemoryStream ms = new MemoryStream(currentDishPhotoBytes))
                        {
                            pbDishPhoto.Image = Image.FromStream(ms);
                        }
                        btnDeletePhoto.Enabled = true;
                    }
                    else
                    {
                        ClearDishPhoto();
                        originalPhotoBytes = null;
                        originalPhotoHash = "";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки фото: {ex.Message}");
                ClearDishPhoto();
                originalPhotoBytes = null;
                originalPhotoHash = "";
            }
        }

        // ОЧИСТКА ФОТО НА ПАНЕЛИ РЕДАКТИРОВАНИЯ
        private void ClearDishPhoto()
        {
            if (pbDishPhoto.Image != null && pbDishPhoto.Image != defaultImage)
            {
                pbDishPhoto.Image.Dispose();
            }
            pbDishPhoto.Image = defaultImage;
            currentDishPhotoBytes = null;
            currentPhotoHash = "";
            btnDeletePhoto.Enabled = false;
        }

        // ЗАГРУЗКА ФОТО ПРИ ДОБАВЛЕНИИ (ИСПРАВЛЕНО)
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
                        // Проверка размера файла (не более 5 МБ)
                        FileInfo fileInfo = new FileInfo(openFileDialog.FileName);
                        if (fileInfo.Length > 5 * 1024 * 1024)
                        {
                            MessageBox.Show("Размер файла не должен превышать 5 МБ!", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        // Сжимаем изображение
                        byte[] newPhotoBytes = CompressImage(openFileDialog.FileName, 300, 300, 80);

                        // Проверяем уникальность фото
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
                        currentPhotoHash = ComputeImageHash(currentDishPhotoBytes);

                        // Отображаем в PictureBox
                        if (pbDishPhoto.Image != null && pbDishPhoto.Image != defaultImage)
                        {
                            pbDishPhoto.Image.Dispose();
                        }

                        using (MemoryStream ms = new MemoryStream(currentDishPhotoBytes))
                        {
                            pbDishPhoto.Image = Image.FromStream(ms);
                        }

                        currentPhotoPath = openFileDialog.FileName;
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

        // УДАЛЕНИЕ ФОТО
        private void BtnDeletePhoto_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Удалить фотографию?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ClearDishPhoto();
            }
        }

        // СЖАТИЕ ИЗОБРАЖЕНИЯ
        private byte[] CompressImage(string imagePath, int maxWidth, int maxHeight, int quality)
        {
            using (Image image = Image.FromFile(imagePath))
            {
                double ratioX = (double)maxWidth / image.Width;
                double ratioY = (double)maxHeight / image.Height;
                double ratio = Math.Min(ratioX, ratioY);

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
                        System.Drawing.Imaging.ImageCodecInfo jpegCodec = null;
                        System.Drawing.Imaging.ImageCodecInfo[] codecs = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();
                        foreach (var codec in codecs)
                        {
                            if (codec.MimeType == "image/jpeg")
                            {
                                jpegCodec = codec;
                                break;
                            }
                        }

                        if (jpegCodec != null)
                        {
                            System.Drawing.Imaging.EncoderParameters encoderParams = new System.Drawing.Imaging.EncoderParameters(1);
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

        // ЗАГРУЗКА БЛЮД ИЗ БАЗЫ
        private void LoadDishes()
        {
            try
            {
                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();

                    string query = @"
                        SELECT d.id_dish, d.dish_name, d.compound, c.id_category, c.category_name, d.price, d.weight_volume, d.cost 
                        FROM dishes d 
                        LEFT JOIN categories c ON d.id_category = c.id_category";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(command);

                    dishesTable = new DataTable();
                    adapter.Fill(dishesTable);

                    dishImages.Clear();
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

        // ОБНОВЛЕНИЕ ФИЛЬТРА КАТЕГОРИЙ
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
                MessageBox.Show($"Ошибка при загрузке категорий для фильтра: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ОБНОВЛЕНИЕ КАТЕГОРИЙ ДЛЯ РЕДАКТИРОВАНИЯ
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
                    comboEditCategory.DisplayMember = "Text";
                    comboEditCategory.ValueMember = "Value";

                    comboEditCategory.Items.Add(new { Text = "-- Выберите категорию --", Value = -1 });

                    while (reader.Read())
                    {
                        comboEditCategory.Items.Add(new
                        {
                            Text = reader["category_name"].ToString(),
                            Value = Convert.ToInt32(reader["id_category"])
                        });
                    }

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке категорий: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ИНИЦИАЛИЗАЦИЯ СТИЛЕЙ КНОПОК - ВСЕ КНОПКИ ВСЕГДА ЗЕЛЕНЫЕ
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
            button.BackColor = Color.DarkSeaGreen; // ВСЕГДА ЗЕЛЕНЫЙ
            button.ForeColor = Color.Black; // Черный текст

            // Убираем изменение цвета при наведении - ставим тот же зеленый
            button.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            button.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            // Оставляем только изменение цвета границы
            button.MouseDown += (s, e) => button.FlatAppearance.BorderColor = Color.DarkBlue;
            button.MouseUp += (s, e) => button.FlatAppearance.BorderColor = Color.Black;
            button.MouseLeave += (s, e) => button.FlatAppearance.BorderColor = Color.Black;
        }

        // ИНИЦИАЛИЗАЦИЯ ВНЕШНЕГО ВИДА ПАНЕЛИ
        private void InitializeEditPanelAppearance()
        {
            panelEditDish.BorderStyle = BorderStyle.None;
            panelEditDish.BackColor = Color.WhiteSmoke;
            panelEditDish.Paint += PanelEdit_Paint;
        }

        private void PanelEdit_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, panelEditDish.ClientRectangle,
                Color.DarkGray, 4, ButtonBorderStyle.Solid,
                Color.DarkGray, 4, ButtonBorderStyle.Solid,
                Color.DarkGray, 4, ButtonBorderStyle.Solid,
                Color.DarkGray, 4, ButtonBorderStyle.Solid);
        }

        // ВАЛИДАЦИЯ ТЕКСТОВЫХ ПОЛЕЙ
        private void SetupValidationTextBoxes()
        {
            SetupRussianOnlyTextBox(txtEditDishName);
            SetupRussianOnlyTextBox(txtSearch);
            SetupCompoundTextBox(txtEditCompound);
            SetupPriceTextBox(numEditPrice);
            SetupPriceTextBox(txtCost); // НОВОЕ поле
            SetMaxLengthLimits();
        }

        private void SetMaxLengthLimits()
        {
            txtEditDishName.MaxLength = 255;
            txtEditCompound.MaxLength = 4000;
            txtSearch.MaxLength = 255;
            numEditPrice.MaxLength = 20;
            txtCost.MaxLength = 20; // НОВОЕ поле
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
                    textBox.Text = char.ToUpper(textBox.Text[0]) +
                                   textBox.Text.Substring(1).ToLower();
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

        private void SetupCompoundTextBox(TextBox textBox)
        {
            textBox.KeyPress += TextBoxCompound_KeyPress;
            textBox.TextChanged += TextBoxCompound_TextChanged;
            textBox.Leave += TextBoxCompound_Leave;
        }

        private void TextBoxCompound_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsLetter(e.KeyChar) && IsRussianLetter(e.KeyChar) ||
                char.IsDigit(e.KeyChar) ||
                e.KeyChar == ' ' || e.KeyChar == ',' || e.KeyChar == '.' ||
                e.KeyChar == '-' || e.KeyChar == ';' || e.KeyChar == ':' ||
                e.KeyChar == '(' || e.KeyChar == ')' || e.KeyChar == '/' ||
                e.KeyChar == (char)Keys.Back || e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
                System.Media.SystemSounds.Beep.Play();
            }
        }

        private void TextBoxCompound_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                string text = textBox.Text;
                string filteredText = FilterCompoundText(text);

                if (text != filteredText)
                {
                    int cursorPosition = textBox.SelectionStart;
                    textBox.Text = filteredText;
                    textBox.SelectionStart = Math.Max(0, cursorPosition - (text.Length - filteredText.Length));
                }
            }
        }

        private void TextBoxCompound_Leave(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (!string.IsNullOrWhiteSpace(textBox.Text))
            {
                string text = textBox.Text.Trim();

                while (text.Contains("  "))
                    text = text.Replace("  ", " ");

                text = text.Replace(",", ", ");
                text = text.Replace(" ,", ",");

                while (text.Contains(",  "))
                    text = text.Replace(",  ", ", ");

                textBox.Text = text;
            }
        }

        private string FilterCompoundText(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            string result = "";
            foreach (char c in input)
            {
                if (IsRussianLetter(c) || char.IsDigit(c) ||
                    c == ' ' || c == ',' || c == '.' ||
                    c == '-' || c == ';' || c == ':' ||
                    c == '(' || c == ')' || c == '/')
                {
                    result += c;
                }
            }

            return result;
        }

        private void SetupPriceTextBox(TextBox textBox)
        {
            textBox.KeyPress += TextBoxPrice_KeyPress;
            textBox.TextChanged += TextBoxPrice_TextChanged;
            textBox.Enter += TextBoxPrice_Enter;
            textBox.Leave += TextBoxPrice_Leave;
            textBox.GotFocus += TextBoxPrice_GotFocus;
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

        private void TextBoxPrice_GotFocus(object sender, EventArgs e)
        {
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

            if (decimal.TryParse(cleanText, NumberStyles.Any, russianCulture, out decimal value))
            {
                return Math.Round(value, 2);
            }

            return 0;
        }

        // ПОКАЗ ПАНЕЛИ РЕДАКТИРОВАНИЯ
        private void ShowEditPanel()
        {
            panelEditDish.Visible = true;
            panelEditDish.BringToFront();
            editLabel.Text = isEditMode ? "Редактирование блюда" : "Добавление нового блюда";
        }

        // СКРЫТИЕ ПАНЕЛИ РЕДАКТИРОВАНИЯ
        private void HideEditPanel()
        {
            panelEditDish.Visible = false;
        }

        // ОЧИСТКА ФОРМЫ РЕДАКТИРОВАНИЯ
        private void ClearEditForm()
        {
            txtEditDishName.Text = "";
            txtEditCompound.Text = "";
            numEditPrice.Text = "";
            txtCost.Text = ""; // НОВОЕ поле
            originalDishName = "";

            if (comboEditCategory.Items.Count > 0)
                comboEditCategory.SelectedIndex = 0;

            // Очищаем фото
            ClearDishPhoto();
            currentDishPhotoBytes = null;
            currentPhotoHash = "";

            // Сбрасываем оригинальные значения
            originalDishNameValue = "";
            originalCompoundValue = "";
            originalPriceValue = 0;
            originalCostValue = 0; // НОВОЕ поле
            originalCategoryIdValue = -1;
            originalPhotoBytes = null;
            originalPhotoHash = "";
        }

        // СОХРАНЕНИЕ ОРИГИНАЛЬНЫХ ЗНАЧЕНИЙ - ИСПРАВЛЕНО
        private void SaveOriginalValues()
        {
            originalDishNameValue = txtEditDishName.Text.Trim();
            originalCompoundValue = txtEditCompound.Text.Trim();
            originalPriceValue = GetPriceFromFormattedText(numEditPrice.Text);
            originalCostValue = GetPriceFromFormattedText(txtCost.Text); // НОВОЕ поле

            dynamic selectedCategory = comboEditCategory.SelectedItem;
            if (selectedCategory != null)
            {
                originalCategoryIdValue = selectedCategory.Value;
            }

            // ИСПРАВЛЕНО: создаем копию массива байт
            if (currentDishPhotoBytes != null)
            {
                originalPhotoBytes = new byte[currentDishPhotoBytes.Length];
                Array.Copy(currentDishPhotoBytes, originalPhotoBytes, currentDishPhotoBytes.Length);
                originalPhotoHash = currentPhotoHash;
            }
            else
            {
                originalPhotoBytes = null;
                originalPhotoHash = "";
            }
        }

        // ПРОВЕРКА НАЛИЧИЯ ИЗМЕНЕНИЙ
        private bool HasChanges()
        {
            // Проверяем название
            if (txtEditDishName.Text.Trim() != originalDishNameValue)
                return true;

            // Проверяем состав
            if (txtEditCompound.Text.Trim() != originalCompoundValue)
                return true;

            // Проверяем цену
            decimal currentPrice = GetPriceFromFormattedText(numEditPrice.Text);
            if (currentPrice != originalPriceValue)
                return true;

            // Проверяем себестоимость
            decimal currentCost = GetPriceFromFormattedText(txtCost.Text);
            if (currentCost != originalCostValue)
                return true;

            // Проверяем категорию
            dynamic selectedCategory = comboEditCategory.SelectedItem;
            int currentCategoryId = selectedCategory != null ? selectedCategory.Value : -1;
            if (currentCategoryId != originalCategoryIdValue)
                return true;

            // Проверяем фото
            if (currentDishPhotoBytes == null && originalPhotoBytes != null)
                return true;
            if (currentDishPhotoBytes != null && originalPhotoBytes == null)
                return true;
            if (currentDishPhotoBytes != null && originalPhotoBytes != null)
            {
                if (currentDishPhotoBytes.Length != originalPhotoBytes.Length)
                    return true;

                for (int i = 0; i < currentDishPhotoBytes.Length; i++)
                {
                    if (currentDishPhotoBytes[i] != originalPhotoBytes[i])
                        return true;
                }
            }

            return false;
        }

        // ЗАГРУЗКА ДАННЫХ БЛЮДА ДЛЯ РЕДАКТИРОВАНИЯ
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

                        object priceObj = reader["price"];
                        if (priceObj != DBNull.Value && priceObj != null)
                        {
                            decimal price = Convert.ToDecimal(priceObj);
                            numEditPrice.Text = price.ToString("N2", russianCulture) + " ₽";
                        }
                        else
                        {
                            numEditPrice.Text = "";
                        }

                        // НОВОЕ поле: себестоимость
                        object costObj = reader["cost"];
                        if (costObj != DBNull.Value && costObj != null)
                        {
                            decimal cost = Convert.ToDecimal(costObj);
                            txtCost.Text = cost.ToString("N2", russianCulture) + " ₽";
                        }
                        else
                        {
                            txtCost.Text = "0.00 ₽";
                        }

                        object categoryObj = reader["id_category"];
                        int categoryId = -1;

                        if (categoryObj != DBNull.Value && categoryObj != null)
                        {
                            categoryId = Convert.ToInt32(categoryObj);
                        }

                        bool categoryFound = false;
                        for (int i = 0; i < comboEditCategory.Items.Count; i++)
                        {
                            dynamic item = comboEditCategory.Items[i];
                            if (item.Value == categoryId)
                            {
                                comboEditCategory.SelectedIndex = i;
                                categoryFound = true;
                                break;
                            }
                        }

                        if (!categoryFound && comboEditCategory.Items.Count > 0)
                        {
                            comboEditCategory.SelectedIndex = 0;
                        }
                    }
                    reader.Close();
                }

                // Загружаем фото отдельно
                LoadDishPhoto(dishId);

                // Сохраняем оригинальные значения
                SaveOriginalValues();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных блюда: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ПРОВЕРКА ДУБЛИКАТА НАЗВАНИЯ (УЛУЧШЕННАЯ ВЕРСИЯ)
        private bool CheckDishDuplicate(string dishName, int excludeDishId = -1)
        {
            try
            {
                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();

                    string query;
                    MySqlCommand command;

                    if (excludeDishId > 0)
                    {
                        // При редактировании исключаем текущее блюдо
                        query = "SELECT COUNT(*) FROM dishes WHERE LOWER(TRIM(dish_name)) = LOWER(TRIM(@name)) AND id_dish != @id";
                        command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@name", dishName.Trim());
                        command.Parameters.AddWithValue("@id", excludeDishId);
                    }
                    else
                    {
                        // При добавлении проверяем все блюда
                        query = "SELECT COUNT(*) FROM dishes WHERE LOWER(TRIM(dish_name)) = LOWER(TRIM(@name))";
                        command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@name", dishName.Trim());
                    }

                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке дубликата: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private string GetDuplicateErrorMessage(string dishName)
        {
            return $"Блюдо с названием '{dishName}' уже существует!\nПожалуйста, выберите другое название.";
        }

        private string GetUserFriendlyDuplicateError(string mysqlError, string dishName)
        {
            if (mysqlError.Contains("dishes.dish_name"))
            {
                return GetDuplicateErrorMessage(dishName);
            }
            else if (mysqlError.Contains("PRIMARY") || mysqlError.Contains("id_dish"))
            {
                return "Ошибка дублирования ID блюда. Обратитесь к администратору.";
            }
            else
            {
                return $"Блюдо с названием '{dishName}' уже существует в базе данных.";
            }
        }

        // УЛУЧШЕННАЯ ВАЛИДАЦИЯ ПРИ СОХРАНЕНИИ
        private bool ValidateDishBeforeSave()
        {
            // Базовая валидация
            if (string.IsNullOrWhiteSpace(txtEditDishName.Text))
            {
                MessageBox.Show("Введите название блюда! Это поле обязательно для заполнения.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtEditDishName.Focus();
                return false;
            }

            if (txtEditDishName.Text.Length > 255)
            {
                MessageBox.Show("Название блюда не должно превышать 255 символов!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtEditDishName.Focus();
                txtEditDishName.SelectAll();
                return false;
            }

            if (comboEditCategory.SelectedIndex <= 0)
            {
                MessageBox.Show("Выберите категорию", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                comboEditCategory.Focus();
                return false;
            }

            decimal price = GetPriceFromFormattedText(numEditPrice.Text);
            if (price <= 0)
            {
                MessageBox.Show("Введите корректную цену! Значение должно быть больше 0.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                numEditPrice.Focus();
                return false;
            }

            decimal maxPrice = 99999999.99m;
            if (price > maxPrice)
            {
                MessageBox.Show($"Сумма не должна превышать {maxPrice.ToString("N2", russianCulture)}!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                numEditPrice.Focus();
                return false;
            }

            // Валидация себестоимости
            decimal cost = GetPriceFromFormattedText(txtCost.Text);
            if (cost < 0)
            {
                MessageBox.Show("Себестоимость не может быть отрицательной!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtCost.Focus();
                return false;
            }

            if (cost > maxPrice)
            {
                MessageBox.Show($"Себестоимость не должна превышать {maxPrice.ToString("N2", russianCulture)}!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtCost.Focus();
                return false;
            }

            if (txtEditCompound.Text.Length > 4000)
            {
                MessageBox.Show("Состав блюда не должен превышать 4000 символов!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtEditCompound.Focus();
                return false;
            }

            return true;
        }

        // СОХРАНЕНИЕ БЛЮДА (ИСПРАВЛЕННАЯ ВЕРСИЯ С ПРОВЕРКОЙ ФОТО И СЕБЕСТОИМОСТИ)
        private void SaveDish()
        {
            // Базовая валидация
            if (!ValidateDishBeforeSave())
                return;

            string dishName = txtEditDishName.Text.Trim();

            // Проверка дубликата названия
            if (isEditMode)
            {
                // При редактировании проверяем, изменилось ли название
                if (dishName != originalDishName)
                {
                    if (CheckDishDuplicate(dishName, currentDishId))
                    {
                        MessageBox.Show(GetDuplicateErrorMessage(dishName), "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txtEditDishName.Focus();
                        txtEditDishName.SelectAll();
                        return;
                    }
                }
            }
            else
            {
                // При добавлении всегда проверяем
                if (CheckDishDuplicate(dishName))
                {
                    MessageBox.Show(GetDuplicateErrorMessage(dishName), "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtEditDishName.Focus();
                    txtEditDishName.SelectAll();
                    return;
                }
            }

            // Проверка уникальности фото (если фото загружено и оно изменилось)
            if (currentDishPhotoBytes != null)
            {
                bool photoChanged = true;

                if (isEditMode && originalPhotoBytes != null)
                {
                    // Проверяем, изменилось ли фото
                    if (currentDishPhotoBytes.Length == originalPhotoBytes.Length)
                    {
                        bool identical = true;
                        for (int i = 0; i < currentDishPhotoBytes.Length; i++)
                        {
                            if (currentDishPhotoBytes[i] != originalPhotoBytes[i])
                            {
                                identical = false;
                                break;
                            }
                        }
                        photoChanged = !identical;
                    }
                }

                if (photoChanged && CheckPhotoDuplicate(currentDishPhotoBytes, isEditMode ? currentDishId : -1))
                {
                    string existingDishName = GetDishNameByPhoto(currentDishPhotoBytes);
                    string message = string.IsNullOrEmpty(existingDishName)
                        ? "Это фото уже используется для другого блюда!"
                        : $"Это фото уже используется для блюда '{existingDishName}'!";

                    MessageBox.Show(message + "\n\nКаждое блюдо должно иметь уникальное фото.",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Сохранение в базу
            try
            {
                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();

                    dynamic selectedCategory = comboEditCategory.SelectedItem;
                    int categoryId = selectedCategory.Value;
                    decimal price = GetPriceFromFormattedText(numEditPrice.Text);
                    decimal cost = GetPriceFromFormattedText(txtCost.Text); // НОВОЕ поле

                    if (isEditMode)
                    {
                        string query = @"
                            UPDATE dishes 
                            SET dish_name = @name, 
                                compound = @compound, 
                                id_category = @category, 
                                price = @price,
                                cost = @cost,
                                photo = @photo
                            WHERE id_dish = @id";

                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@name", dishName);
                        command.Parameters.AddWithValue("@compound", txtEditCompound.Text.Trim());
                        command.Parameters.AddWithValue("@category", categoryId);
                        command.Parameters.AddWithValue("@price", price);
                        command.Parameters.AddWithValue("@cost", cost); // НОВОЕ поле
                        command.Parameters.AddWithValue("@photo", currentDishPhotoBytes ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@id", currentDishId);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Блюдо успешно обновлено", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        string query = @"
                            INSERT INTO dishes (dish_name, compound, id_category, price, cost, photo) 
                            VALUES (@name, @compound, @category, @price, @cost, @photo)";

                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@name", dishName);
                        command.Parameters.AddWithValue("@compound", txtEditCompound.Text.Trim());
                        command.Parameters.AddWithValue("@category", categoryId);
                        command.Parameters.AddWithValue("@price", price);
                        command.Parameters.AddWithValue("@cost", cost); // НОВОЕ поле
                        command.Parameters.AddWithValue("@photo", currentDishPhotoBytes ?? (object)DBNull.Value);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Блюдо успешно добавлено", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }

                    HideEditPanel();
                    LoadDishes();
                    UpdateCategoryFilter();
                }
            }
            catch (MySqlException mysqlEx)
            {
                if (mysqlEx.Number == 1062) // Ошибка дубликата
                {
                    string errorMessage = GetUserFriendlyDuplicateError(mysqlEx.Message, dishName);
                    MessageBox.Show(errorMessage, "Ошибка дублирования",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);

                    // Подсвечиваем поле с названием
                    txtEditDishName.Focus();
                    txtEditDishName.SelectAll();
                }
                else if (mysqlEx.Number == 1366)
                {
                    MessageBox.Show($"Ошибка в данных: {mysqlEx.Message}\n\nПроверьте правильность введенных значений.", "Ошибка данных",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (mysqlEx.Number == 1048)
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
                MessageBox.Show($"Ошибка при сохранении блюда: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ОБРАБОТЧИКИ КНОПОК
        private void AddButton_Click(object sender, EventArgs e)
        {
            isEditMode = false;
            currentDishId = -1;

            RefreshEditCategories();
            ClearEditForm();

            if (comboEditCategory.Items.Count > 0)
                comboEditCategory.SelectedIndex = 0;

            // Для нового блюда сохраняем пустые оригинальные значения
            SaveOriginalValues();

            ShowEditPanel();
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            if (dgvDishes.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите блюдо для редактирования", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                MessageBox.Show("Выберите блюдо для удаления", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show(
                "Вы уверены, что хотите удалить выбранное блюдо?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                int dishId = Convert.ToInt32(dgvDishes.SelectedRows[0].Cells["id_dish"].Value);

                try
                {
                    using (MySqlConnection connection = SettingsBD.GetConnection())
                    {
                        connection.Open();

                        string query = "DELETE FROM dishes WHERE id_dish = @id";
                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@id", dishId);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Блюдо успешно удалено", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadDishes();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении блюда: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // НОВЫЙ ОБРАБОТЧИК CancelButton
        private void CancelButton_Click(object sender, EventArgs e)
        {
            // Проверяем, были ли изменения
            if (HasChanges())
            {
                DialogResult result = MessageBox.Show(
                    "У вас есть несохраненные изменения.\n\nСохранить изменения?",
                    "Несохраненные изменения",
                    MessageBoxButtons.YesNo,  // Изменено с YesNoCancel на YesNo
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Сохраняем изменения
                    SaveDish();
                }
                else // if (result == DialogResult.No)
                {
                    // Не сохраняем, просто закрываем панель
                    HideEditPanel();
                }
                // Кнопка Cancel больше не появляется
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

        // ФИЛЬТРАЦИЯ ДАННЫХ
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

            if (filteredRows.Any())
            {
                dgvDishes.DataSource = filteredRows.CopyToDataTable();
            }
            else
            {
                dgvDishes.DataSource = dishesTable.Clone();
            }
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            try
            {
                txtSearch.Text = "";

                if (comboCategoryFilter.Items.Count > 0)
                    comboCategoryFilter.SelectedIndex = 0;

                if (dishesTable != null)
                {
                    dgvDishes.DataSource = dishesTable;

                    if (dgvDishes.Rows.Count > 0)
                    {
                        dgvDishes.ClearSelection();
                        btnEdit.Enabled = false;
                        btnDelete.Enabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сбросе фильтров: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // НАВИГАЦИЯ
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Visible = false;

            if (userRole == "admin")
            {
                // Если админ - открываем AdminForm
                AdminForm admin = new AdminForm();
                admin.Show();
            }
            else // director
            {
                // Если директор - открываем DirectorForm (или другую форму для директора)
                DirectorForm director = new DirectorForm();
                director.Show();
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Visible = false;

            if (userRole == "admin")
            {
                // Если админ - открываем AdminForm
                AdminForm admin = new AdminForm();
                admin.Show();
            }
            else // director
            {
                // Если директор - открываем DirectorForm (или другую форму для директора)
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

            if (userRole == "admin")
            {
                // Если админ - открываем AdminForm
                AdminForm admin = new AdminForm();
                admin.Show();
            }
            else // director
            {
                // Если директор - открываем DirectorForm (или другую форму для директора)
                DirectorForm director = new DirectorForm();
                director.Show();
            }
        }

        private void panelEditDish_Paint(object sender, PaintEventArgs e)
        {
        }

        private void TxtEditPrice_TextChanged(object sender, EventArgs e)
        {
        }
    }
}