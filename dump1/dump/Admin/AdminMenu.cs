using MySql.Data.MySqlClient;
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
    /// <summary>
    /// Форма управления меню блюд для администратора.
    /// Предоставляет функционал для добавления, редактирования, удаления и просмотра блюд.
    /// </summary>
    public partial class AdminMenu : Form
    {
        private DataTable dishesTable;
        private bool isEditMode = false;
        private int currentDishId = -1;
        private string originalDishName = "";
        private CultureInfo russianCulture = new CultureInfo("ru-RU");
        private bool isFormatting = false;
        private Image defaultImage;

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
        private bool isLockDialogOpen = false;

        // Цвет выделения как в справочниках
        private Color selectionColor = Color.FromArgb(233, 242, 236);
        private Color headerBackColor = Color.FromArgb(97, 173, 123);

        /// <summary>
        /// Конструктор формы управления меню.
        /// Инициализирует компоненты, настраивает внешний вид и загружает данные.
        /// </summary>
        public AdminMenu()
        {
            InitializeComponent();

            CreateDefaultImage();
            InitializeButtonStyles();
            SetupDataGridView();
            LoadDishes();
            RefreshEditCategories();
            HideEditPanel();
            InitializeEditPanelAppearance();
            SetupValidationTextBoxes();

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

            this.FormClosing += AdminMenu_FormClosing;
            InactivityManager.RegisterForm(this);
            InactivityManager.OnLockRequest += LockSystem;
        }

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

        /// <summary>
        /// Обработчик события закрытия формы.
        /// При закрытии формы пользователем скрывает её и открывает форму администратора.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Аргументы события закрытия формы.</param>
        private void AdminMenu_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Visible = false;
                AdminForm admin = new AdminForm();
                admin.Show();
            }
        }

        /// <summary>
        /// Освобождает управляемые ресурсы формы.
        /// </summary>
        /// <param name="disposing">True если освобождение выполняется явно.</param>
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

        /// <summary>
        /// Создаёт изображение-заглушку для блюд без фотографии.
        /// </summary>
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

        /// <summary>
        /// Вычисляет MD5-хеш изображения для проверки дубликатов.
        /// </summary>
        /// <param name="imageBytes">Массив байтов изображения.</param>
        /// <returns>Строка с хешем в шестнадцатеричном формате.</returns>
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

        /// <summary>
        /// Проверяет, используется ли данное фото для другого блюда.
        /// </summary>
        /// <param name="photoBytes">Массив байтов фотографии.</param>
        /// <param name="excludeDishId">ID блюда для исключения при проверке (при редактировании).</param>
        /// <returns>True если фото уже используется.</returns>
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

        /// <summary>
        /// Получает название блюда по фотографии.
        /// </summary>
        /// <param name="photoBytes">Массив байтов фотографии.</param>
        /// <returns>Название блюда или пустая строка.</returns>
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

        /// <summary>
        /// Настраивает внешний вид и колонки DataGridView для отображения блюд.
        /// </summary>
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
            dgvDishes.EnableHeadersVisualStyles = false;

            // ===== ШАПКА - ЗЕЛЕНАЯ, TIMES NEW ROMAN 14PT BOLD =====
            dgvDishes.ColumnHeadersDefaultCellStyle.BackColor = headerBackColor;
            dgvDishes.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvDishes.ColumnHeadersDefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Bold);
            dgvDishes.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvDishes.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvDishes.ColumnHeadersDefaultCellStyle.Padding = new Padding(0, 5, 0, 5);
            dgvDishes.ColumnHeadersHeight = 55;

            // ===== ЯЧЕЙКИ - TIMES NEW ROMAN 14PT REGULAR =====
            dgvDishes.DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Regular);
            dgvDishes.DefaultCellStyle.Padding = new Padding(0, 2, 0, 2);
            dgvDishes.DefaultCellStyle.BackColor = Color.White;
            dgvDishes.DefaultCellStyle.ForeColor = Color.Black;

            // ===== ВЫДЕЛЕНИЕ - СВЕТЛО-ЗЕЛЕНЫЙ/СЕРЫЙ (КАК В СПРАВОЧНИКАХ) =====
            dgvDishes.DefaultCellStyle.SelectionBackColor = selectionColor;
            dgvDishes.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvDishes.RowsDefaultCellStyle.SelectionBackColor = selectionColor;
            dgvDishes.RowsDefaultCellStyle.SelectionForeColor = Color.Black;

            dgvDishes.RowsDefaultCellStyle.BackColor = Color.White;
            dgvDishes.RowsDefaultCellStyle.ForeColor = Color.Black;

            dgvDishes.RowTemplate.Height = 80;
            dgvDishes.RowTemplate.MinimumHeight = 80;
            dgvDishes.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            dgvDishes.GridColor = Color.Gray;
            dgvDishes.BorderStyle = BorderStyle.FixedSingle;
            dgvDishes.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            dgvDishes.ScrollBars = ScrollBars.Both;

            dgvDishes.Columns.Clear();

            // Колонка Фото
            DataGridViewImageColumn imageColumn = new DataGridViewImageColumn();
            imageColumn.Name = "photo";
            imageColumn.HeaderText = "Фото";
            imageColumn.DataPropertyName = "photo_image";
            imageColumn.Width = 100;
            imageColumn.MinimumWidth = 80;
            imageColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
            imageColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvDishes.Columns.Add(imageColumn);

            // Колонка Название
            DataGridViewTextBoxColumn nameColumn = new DataGridViewTextBoxColumn();
            nameColumn.Name = "dish_name";
            nameColumn.HeaderText = "Название";
            nameColumn.DataPropertyName = "dish_name";
            nameColumn.Width = 200;
            nameColumn.MinimumWidth = 150;
            nameColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            nameColumn.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            nameColumn.DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Regular);
            dgvDishes.Columns.Add(nameColumn);

            // Колонка Состав
            DataGridViewTextBoxColumn compoundColumn = new DataGridViewTextBoxColumn();
            compoundColumn.Name = "compound";
            compoundColumn.HeaderText = "Состав";
            compoundColumn.DataPropertyName = "compound";
            compoundColumn.Width = 350;
            compoundColumn.MinimumWidth = 200;
            compoundColumn.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            compoundColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopLeft;
            compoundColumn.DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Regular);
            dgvDishes.Columns.Add(compoundColumn);

            // Колонка Категория
            DataGridViewTextBoxColumn categoryColumn = new DataGridViewTextBoxColumn();
            categoryColumn.Name = "category_name";
            categoryColumn.HeaderText = "Категория";
            categoryColumn.DataPropertyName = "category_name";
            categoryColumn.Width = 150;
            categoryColumn.MinimumWidth = 120;
            categoryColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            categoryColumn.DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Regular);
            dgvDishes.Columns.Add(categoryColumn);

            // Колонка Цена
            DataGridViewTextBoxColumn priceColumn = new DataGridViewTextBoxColumn();
            priceColumn.Name = "price_display";
            priceColumn.HeaderText = "Цена";
            priceColumn.Width = 140;
            priceColumn.MinimumWidth = 100;
            priceColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            priceColumn.DefaultCellStyle.ForeColor = Color.DarkGreen;
            priceColumn.DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Bold);
            dgvDishes.Columns.Add(priceColumn);

            // Колонка Себестоимость
            DataGridViewTextBoxColumn costColumn = new DataGridViewTextBoxColumn();
            costColumn.Name = "cost_display";
            costColumn.HeaderText = "Себестоимость";
            costColumn.Width = 140;
            costColumn.MinimumWidth = 100;
            costColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            costColumn.DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Regular);
            dgvDishes.Columns.Add(costColumn);

            // Колонка Вес/Объем
            DataGridViewTextBoxColumn weightColumn = new DataGridViewTextBoxColumn();
            weightColumn.Name = "weight_volume";
            weightColumn.HeaderText = "Вес/Объем";
            weightColumn.DataPropertyName = "weight_volume";
            weightColumn.Width = 130;
            weightColumn.MinimumWidth = 100;
            weightColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            weightColumn.DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Regular);
            dgvDishes.Columns.Add(weightColumn);

            // Скрытая колонка ID
            DataGridViewTextBoxColumn idColumn = new DataGridViewTextBoxColumn();
            idColumn.Name = "id_dish";
            idColumn.DataPropertyName = "id_dish";
            idColumn.Visible = false;
            dgvDishes.Columns.Add(idColumn);

            // Скрытая колонка Цена (значение)
            DataGridViewTextBoxColumn priceValueColumn = new DataGridViewTextBoxColumn();
            priceValueColumn.Name = "price";
            priceValueColumn.DataPropertyName = "price";
            priceValueColumn.Visible = false;
            dgvDishes.Columns.Add(priceValueColumn);

            // Скрытая колонка Себестоимость (значение)
            DataGridViewTextBoxColumn costValueColumn = new DataGridViewTextBoxColumn();
            costValueColumn.Name = "cost";
            costValueColumn.DataPropertyName = "cost";
            costValueColumn.Visible = false;
            dgvDishes.Columns.Add(costValueColumn);

            dgvDishes.CellFormatting += DgvDishes_CellFormatting;
        }

        /// <summary>
        /// Обработчик форматирования ячеек DataGridView.
        /// Форматирует отображение цены и себестоимости.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Аргументы события форматирования.</param>
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

        /// <summary>
        /// Загружает список блюд из базы данных.
        /// </summary>
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

        /// <summary>
        /// Обновляет список категорий в выпадающем списке фильтра.
        /// </summary>
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

        /// <summary>
        /// Обновляет список категорий в выпадающем списке редактирования.
        /// </summary>
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

        /// <summary>
        /// Загружает фотографию блюда из базы данных.
        /// </summary>
        /// <param name="dishId">ID блюда.</param>
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

        /// <summary>
        /// Загружает данные блюда в форму редактирования.
        /// </summary>
        /// <param name="dishId">ID блюда.</param>
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

        /// <summary>
        /// Очищает фотографию блюда в форме редактирования.
        /// </summary>
        private void ClearDishPhoto()
        {
            if (pbDishPhoto.Image != null && pbDishPhoto.Image != defaultImage)
                pbDishPhoto.Image.Dispose();

            pbDishPhoto.Image = (Image)defaultImage.Clone();
            currentDishPhotoBytes = null;
            currentPhotoHash = "";
            btnDeletePhoto.Enabled = false;
        }

        /// <summary>
        /// Обработчик нажатия кнопки загрузки фото.
        /// Открывает диалог выбора файла и загружает изображение.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Аргументы события.</param>
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

        /// <summary>
        /// Обработчик нажатия кнопки удаления фото.
        /// Удаляет фотографию блюда.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Аргументы события.</param>
        private void BtnDeletePhoto_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Удалить фотографию?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
                ClearDishPhoto();
        }

        /// <summary>
        /// Сжимает изображение до указанных размеров и качества.
        /// </summary>
        /// <param name="imagePath">Путь к исходному изображению.</param>
        /// <param name="maxWidth">Максимальная ширина.</param>
        /// <param name="maxHeight">Максимальная высота.</param>
        /// <param name="quality">Качество сжатия (0-100).</param>
        /// <returns>Массив байтов сжатого изображения.</returns>
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

        /// <summary>
        /// Инициализирует стили всех кнопок на форме.
        /// </summary>
        private void InitializeButtonStyles()
        {
            SetupButtonStyle(btnAdd);
            SetupButtonStyle(btnEdit);
            SetupButtonStyle(btnDelete);
            SetupButtonStyle(buttonReset);
            SetupButtonStyle(btnUploadPhoto);
            SetupButtonStyle(btnDeletePhoto);
        }

        /// <summary>
        /// Применяет единый стиль к кнопке.
        /// </summary>
        /// <param name="button">Кнопка для стилизации.</param>
        private void SetupButtonStyle(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = Color.Black;
            button.BackColor = Color.DarkSeaGreen;
            button.ForeColor = Color.Black;
            button.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            button.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;
            button.Font = new Font("Times New Roman", 14, FontStyle.Regular);
        }

        /// <summary>
        /// Настраивает внешний вид панели редактирования с рамкой.
        /// </summary>
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

        /// <summary>
        /// Настраивает валидацию текстовых полей ввода.
        /// </summary>
        private void SetupValidationTextBoxes()
        {
            txtEditDishName.MaxLength = 100;
            txtEditDishName.Font = new Font("Times New Roman", 14);
            txtEditDishName.KeyPress += (s, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !IsRussianLetter(e.KeyChar) && e.KeyChar != '-' && e.KeyChar != ' ')
                    e.Handled = true;
            };

            txtSearch.MaxLength = 100;
            txtSearch.Font = new Font("Times New Roman", 14);
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

                    if (text.Length > 100)
                    {
                        text = text.Substring(0, 100);
                        txtSearch.Text = text;
                        txtSearch.SelectionStart = Math.Min(cursorPos, text.Length);
                        return;
                    }

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

            txtCost.Font = new Font("Times New Roman", 14);
            numEditPrice.Font = new Font("Times New Roman", 14);
            txtWeightVolume.Font = new Font("Times New Roman", 14);
            txtEditCompound.Font = new Font("Times New Roman", 14);
            comboEditCategory.Font = new Font("Times New Roman", 14);
            comboCategoryFilter.Font = new Font("Times New Roman", 14);

            txtCost.KeyPress += TextBoxPrice_KeyPress;
            numEditPrice.KeyPress += TextBoxPrice_KeyPress;
            txtCost.Leave += (s, e) => FormatPriceTextBoxOnLeave(txtCost);
            numEditPrice.Leave += (s, e) => FormatPriceTextBoxOnLeave(numEditPrice);

            txtWeightVolume.MaxLength = 20;
        }

        /// <summary>
        /// Проверяет, является ли символ русской буквой.
        /// </summary>
        /// <param name="c">Проверяемый символ.</param>
        /// <returns>True если символ является русской буквой.</returns>
        private bool IsRussianLetter(char c)
        {
            return (c >= 'А' && c <= 'Я') || (c >= 'а' && c <= 'я') || c == 'Ё' || c == 'ё';
        }

        /// <summary>
        /// Обработчик нажатия клавиш в поле ввода цены.
        /// Разрешает ввод только цифр и запятой.
        /// </summary>
        private void TextBoxPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != ',')
                e.Handled = true;

            TextBox textBox = sender as TextBox;
            if (e.KeyChar == ',' && textBox.Text.Contains(","))
                e.Handled = true;
        }

        /// <summary>
        /// Форматирует текст в поле цены при потере фокуса.
        /// </summary>
        /// <param name="textBox">Поле ввода для форматирования.</param>
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

        /// <summary>
        /// Извлекает числовое значение из отформатированного текста цены.
        /// </summary>
        /// <param name="formattedText">Отформатированный текст с символом ₽.</param>
        /// <returns>Числовое значение цены.</returns>
        private decimal GetPriceFromFormattedText(string formattedText)
        {
            if (string.IsNullOrWhiteSpace(formattedText))
                return 0;

            string cleanText = formattedText.Replace(" ", "").Replace("₽", "").Trim();
            return decimal.TryParse(cleanText, NumberStyles.Any, russianCulture, out decimal value) ? value : 0;
        }

        /// <summary>
        /// Получает ID выбранной категории из выпадающего списка.
        /// </summary>
        /// <returns>ID категории или -1 если не выбрана.</returns>
        private int GetSelectedCategoryId()
        {
            if (comboEditCategory.SelectedIndex <= 0)
                return -1;

            string selectedCategory = comboEditCategory.SelectedItem.ToString();
            if (categoryDictionary.ContainsKey(selectedCategory))
                return categoryDictionary[selectedCategory];

            return -1;
        }

        /// <summary>
        /// Отображает панель редактирования блюда.
        /// </summary>
        private void ShowEditPanel()
        {
            panelEditDish.Visible = true;
            panelEditDish.BringToFront();
            editLabel.Text = isEditMode ? "Редактирование блюда" : "Добавление нового блюда";
            editLabel.Font = new Font("Times New Roman", 16, FontStyle.Bold);
        }

        /// <summary>
        /// Скрывает панель редактирования и очищает форму.
        /// </summary>
        private void HideEditPanel()
        {
            panelEditDish.Visible = false;
            ClearEditForm();
        }

        /// <summary>
        /// Очищает все поля формы редактирования.
        /// </summary>
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

        /// <summary>
        /// Сохраняет текущие значения полей для отслеживания изменений.
        /// </summary>
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

        /// <summary>
        /// Проверяет, были ли внесены изменения в форму редактирования.
        /// </summary>
        /// <returns>True если есть изменения.</returns>
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

        /// <summary>
        /// Проверяет, существует ли блюдо с таким названием в базе данных.
        /// </summary>
        /// <param name="dishName">Название блюда.</param>
        /// <param name="excludeDishId">ID блюда для исключения (при редактировании).</param>
        /// <returns>True если дубликат найден.</returns>
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

        /// <summary>
        /// Сохраняет блюдо в базу данных (добавление или обновление).
        /// </summary>
        /// <returns>True если сохранение успешно.</returns>
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

        /// <summary>
        /// Обработчик нажатия кнопки "Добавить".
        /// Открывает панель для создания нового блюда.
        /// </summary>
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

        /// <summary>
        /// Обработчик нажатия кнопки "Редактировать".
        /// Загружает данные выбранного блюда в форму редактирования.
        /// </summary>
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

        /// <summary>
        /// Обработчик нажатия кнопки "Удалить".
        /// Удаляет выбранное блюдо с проверкой наличия в заказах.
        /// </summary>
        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (dgvDishes.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите блюдо для удаления!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int dishId = Convert.ToInt32(dgvDishes.SelectedRows[0].Cells["id_dish"].Value);
            string dishName = dgvDishes.SelectedRows[0].Cells["dish_name"].Value?.ToString() ?? "";

            try
            {
                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();

                    // ===== ПРОВЕРЯЕМ, ИСПОЛЬЗУЕТСЯ ЛИ БЛЮДО В ЗАКАЗАХ =====
                    string checkQuery = "SELECT COUNT(*) FROM order_dish WHERE id_dish = @id";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@id", dishId);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        // ===== ЕСЛИ БЛЮДО В ЗАКАЗАХ - ЗАПРЕЩАЕМ УДАЛЕНИЕ =====
                        MessageBox.Show(
                            $"❌ Невозможно удалить блюдо \"{dishName}\"!\n\n" +
                            $"Оно используется в {count} заказах.\n\n" +
                            $"Сначала удалите это блюдо из всех заказов.",
                            "Удаление запрещено",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }

                    // ===== ЕСЛИ БЛЮДО НЕ В ЗАКАЗАХ - МОЖНО УДАЛИТЬ =====
                    DialogResult result = MessageBox.Show(
                        $"Вы действительно хотите удалить блюдо \"{dishName}\"?\n\nЭто действие невозможно отменить!",
                        "Подтверждение удаления",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        string deleteQuery = "DELETE FROM dishes WHERE id_dish = @id";
                        MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, connection);
                        deleteCmd.Parameters.AddWithValue("@id", dishId);
                        deleteCmd.ExecuteNonQuery();

                        MessageBox.Show($"✅ Блюдо \"{dishName}\" успешно удалено!",
                            "Успех",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        LoadDishes();
                    }
                }
            }
            catch (MySqlException ex)
            {
                // Если вдруг ошибка внешнего ключа - перехватываем
                if (ex.Number == 1451)
                {
                    MessageBox.Show($"❌ Невозможно удалить блюдо \"{dishName}\"!\n\n" +
                                  "Оно используется в заказах.\n\n" +
                                  "Сначала удалите это блюдо из всех заказов.",
                                  "Удаление запрещено",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show($"Ошибка базы данных:\n{ex.Message}",
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Отмена".
        /// Проверяет наличие изменений и закрывает панель редактирования.
        /// </summary>
        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (HasChanges())
            {
                DialogResult result = MessageBox.Show(
                    "У вас есть несохраненные изменения.\n\nСохранить изменения?",
                    "Несохраненные изменения",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    SaveDish();
                }
                else if (result == DialogResult.No)
                {
                    HideEditPanel();
                }
            }
            else
            {
                HideEditPanel();
            }
        }

        /// <summary>
        /// Обработчик изменения выделения в DataGridView.
        /// Обновляет состояние кнопок редактирования и удаления.
        /// </summary>
        private void DataGridView_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = dgvDishes.SelectedRows.Count > 0;
            btnEdit.Enabled = hasSelection;
            btnDelete.Enabled = hasSelection;
        }

        /// <summary>
        /// Обработчик изменения текста в поле поиска.
        /// Применяет фильтрацию.
        /// </summary>
        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            FilterData();
        }

        /// <summary>
        /// Обработчик изменения выбранной категории в фильтре.
        /// Применяет фильтрацию.
        /// </summary>
        private void CategoryFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterData();
        }

        /// <summary>
        /// Фильтрует список блюд по поисковому запросу и категории.
        /// </summary>
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

        /// <summary>
        /// Обработчик нажатия кнопки сброса фильтров.
        /// Очищает поиск и сбрасывает категорию.
        /// </summary>
        private void buttonReset_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            if (comboCategoryFilter.Items.Count > 0)
                comboCategoryFilter.SelectedIndex = 0;
            if (dishesTable != null)
                dgvDishes.DataSource = dishesTable;
        }

        /// <summary>
        /// Обработчик нажатия кнопки выхода (крестик).
        /// </summary>
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            AdminForm admin = new AdminForm();
            admin.Show();
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