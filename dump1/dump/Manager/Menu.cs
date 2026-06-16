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

namespace dump
{
    public partial class Menu : Form
    {
        private List<Dish> dishesList = new List<Dish>();
        private List<Category> categoriesList = new List<Category>();
        private BindingList<DishView> filteredDishes = new BindingList<DishView>();
        private Image defaultImage;
        private List<CartItem> cartItems = new List<CartItem>();

        private List<Gift> giftsList = new List<Gift>();
        private List<CartGift> cartGifts = new List<CartGift>();
        private bool isGiftAdded = false;

        private Color headerBackColor = Color.FromArgb(97, 173, 123);
        private Color selectionColor = Color.FromArgb(233, 242, 236);
        private Color buttonColor = Color.DarkSeaGreen;
        private bool isLockDialogOpen = false;

        public Menu()
        {
            InitializeComponent();
            CreateDefaultImage();
            textBoxSearch.KeyPress += textBoxSearch_KeyPress;
            textBoxSearch.TextChanged += textBoxSearch_TextChanged;
            dataGridView1.CellClick += dataGridView1_CellClick;
            dataGridView1.SelectionChanged += dataGridView1_SelectionChanged;
            dataGridView1.MouseClick += DataGridView1_MouseClick;
            LoadDataFromDatabase();
            LoadGiftsFromDatabase();
            SetupControls();
            InitializeDataGridView();
            ApplyRealTimeFiltering();
            UpdateCartCount();
            SetupButtonStyles();
            this.FormClosing += Menu_FormClosing;
            InactivityManager.RegisterForm(this);
            InactivityManager.OnLockRequest += LockSystem;
        }

        public void RefreshGiftFromCart()
        {
            UpdateGift();
            UpdateCartCount();
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

        private void Menu_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Visible = false;
                ManagerForm manager = new ManagerForm();
                manager.Show();
            }
        }

        public class Gift
        {
            public int Id_present { get; set; }
            public string Name { get; set; }
            public decimal FromPrice { get; set; }
        }

        public class CartGift
        {
            public int Id_present { get; set; }
            public string Name { get; set; }
            public int Quantity { get; set; } = 1;
            public decimal Price { get; set; } = 0;
            public bool IsGift { get; set; } = true;
        }

        public class CartItem
        {
            public int Id_dish { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public string WeightVolume { get; set; }
        }

        public class Dish
        {
            public int Id_dish { get; set; }
            public string Name { get; set; }
            public string Compound { get; set; }
            public int FK_id_category { get; set; }
            public decimal Price { get; set; }
            public byte[] Photo { get; set; }
            public string WeightVolume { get; set; }
        }

        public class DishView
        {
            public string Name { get; set; }
            public string Compound { get; set; }
            public decimal Price { get; set; }
            public string FormattedPrice => Price.ToString("N2") + " ₽";
            public Image Photo { get; set; }
            public string WeightVolume { get; set; }
        }

        public class Category
        {
            public int Id_category { get; set; }
            public string Name { get; set; }
        }

        private void SetupButtonStyles()
        {
            SetupPanelButtonStyle(buttonReset);
        }

        private void SetupPanelButtonStyle(Button btn)
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

        private void LoadGiftsFromDatabase()
        {
            try
            {
                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();
                    string giftsQuery = "SELECT id_present, name, from_price FROM present ORDER BY from_price";
                    using (MySqlCommand cmd = new MySqlCommand(giftsQuery, connection))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        giftsList.Clear();
                        while (reader.Read())
                        {
                            giftsList.Add(new Gift
                            {
                                Id_present = Convert.ToInt32(reader["id_present"]),
                                Name = reader["name"].ToString(),
                                FromPrice = Convert.ToDecimal(reader["from_price"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки подарков: {ex.Message}", "Ошибка");
            }
        }

        private void UpdateGift()
        {
            decimal total = cartItems.Sum(item => item.Price * item.Quantity);

            var availableGift = giftsList
                .Where(g => g.FromPrice <= total)
                .OrderByDescending(g => g.FromPrice)
                .FirstOrDefault();

            if (availableGift != null)
            {
                var currentGift = cartGifts.FirstOrDefault();

                if (currentGift == null || currentGift.Id_present != availableGift.Id_present)
                {
                    cartGifts.Clear();
                    cartGifts.Add(new CartGift
                    {
                        Id_present = availableGift.Id_present,
                        Name = $"🎁 {availableGift.Name} (Подарок)",
                        Quantity = 1,
                        Price = 0
                    });
                    isGiftAdded = true;
                }
            }
            else
            {
                if (cartGifts.Any())
                {
                    cartGifts.Clear();
                    isGiftAdded = false;
                }
            }
        }

        private void UpdateCartCount()
        {
            int totalItems = cartItems.Sum(item => item.Quantity) + cartGifts.Sum(g => g.Quantity);
            if (totalItems > 0)
            {
                labelCount.Text = totalItems.ToString();
                labelCount.Visible = true;
            }
            else
            {
                labelCount.Visible = false;
            }
        }

        private void DataGridView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hti = dataGridView1.HitTest(e.X, e.Y);
                if (hti.RowIndex >= 0)
                {
                    dataGridView1.ClearSelection();
                    dataGridView1.Rows[hti.RowIndex].Selected = true;

                    ContextMenuStrip menu = new ContextMenuStrip();
                    ToolStripMenuItem addToCart = new ToolStripMenuItem("Добавить в заказ");
                    addToCart.Click += (s, ev) => AddSelectedToCart();
                    menu.Items.Add(addToCart);
                    menu.Show(dataGridView1, e.Location);
                }
            }
        }

        private void AddSelectedToCart()
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                string dishName = filteredDishes[dataGridView1.SelectedRows[0].Index].Name;
                var dish = dishesList.FirstOrDefault(d => d.Name == dishName);

                if (dish != null)
                {
                    var existingItem = cartItems.FirstOrDefault(item => item.Id_dish == dish.Id_dish);

                    if (existingItem != null)
                    {
                        existingItem.Quantity++;
                    }
                    else
                    {
                        cartItems.Add(new CartItem
                        {
                            Id_dish = dish.Id_dish,
                            Name = dish.Name,
                            Price = dish.Price,
                            Quantity = 1,
                            WeightVolume = dish.WeightVolume
                        });
                    }

                    UpdateGift();
                    UpdateCartCount();
                }
            }
        }

        private void CreateDefaultImage()
        {
            defaultImage = new Bitmap(100, 100);
            using (Graphics g = Graphics.FromImage(defaultImage))
            {
                g.Clear(Color.LightGray);
                using (Font font = new Font("Times New Roman", 14))
                {
                    g.DrawString("Нет фото", font, Brushes.Black, new PointF(20, 40));
                }
            }
        }

        private void LoadDataFromDatabase()
        {
            try
            {
                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();

                    string categoriesQuery = "SELECT * FROM categories";
                    using (MySqlCommand cmd = new MySqlCommand(categoriesQuery, connection))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        categoriesList.Clear();
                        while (reader.Read())
                        {
                            categoriesList.Add(new Category
                            {
                                Id_category = Convert.ToInt32(reader["id_category"]),
                                Name = reader["category_name"].ToString()
                            });
                        }
                    }

                    string dishesQuery = "SELECT id_dish, dish_name, compound, id_category, price, photo, weight_volume FROM dishes";
                    using (MySqlCommand cmd = new MySqlCommand(dishesQuery, connection))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        dishesList.Clear();
                        while (reader.Read())
                        {
                            byte[] photoBytes = null;
                            if (reader["photo"] != DBNull.Value)
                            {
                                photoBytes = (byte[])reader["photo"];
                            }

                            dishesList.Add(new Dish
                            {
                                Id_dish = Convert.ToInt32(reader["id_dish"]),
                                Name = reader["dish_name"].ToString(),
                                Compound = reader["compound"]?.ToString() ?? "",
                                FK_id_category = Convert.ToInt32(reader["id_category"]),
                                Price = Convert.ToDecimal(reader["price"]),
                                Photo = photoBytes,
                                WeightVolume = reader["weight_volume"]?.ToString() ?? ""
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка");
            }
        }

        private void SetupControls()
        {
            if (textBoxSearch != null)
                textBoxSearch.TextChanged += (s, e) => ApplyRealTimeFiltering();

            if (comboBoxCategory != null)
                comboBoxCategory.SelectedIndexChanged += (s, e) => ApplyRealTimeFiltering();

            if (comboBoxSortPrice != null)
            {
                comboBoxSortPrice.Items.AddRange(new string[] {
                    "Без сортировки",
                    "Цена по возрастанию",
                    "Цена по убыванию"
                });
                comboBoxSortPrice.SelectedIndex = 0;
                comboBoxSortPrice.SelectedIndexChanged += (s, e) => ApplyRealTimeFiltering();
            }

            RefreshCategoryComboBox();
        }

        private void RefreshCategoryComboBox()
        {
            if (comboBoxCategory == null) return;

            comboBoxCategory.Items.Clear();
            comboBoxCategory.Items.Add("Все категории");

            foreach (var category in categoriesList)
            {
                comboBoxCategory.Items.Add(category.Name);
            }

            comboBoxCategory.SelectedIndex = 0;
        }

        private string GetCategoryName(int categoryId)
        {
            var category = categoriesList.FirstOrDefault(c => c.Id_category == categoryId);
            return category?.Name ?? "Неизвестно";
        }

        private void ApplyRealTimeFiltering()
        {
            try
            {
                var result = dishesList.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(textBoxSearch?.Text))
                {
                    string searchText = textBoxSearch.Text.ToLower();
                    result = result.Where(d => d.Name.ToLower().Contains(searchText));
                }

                if (comboBoxCategory != null && comboBoxCategory.SelectedIndex > 0)
                {
                    string selectedCategory = comboBoxCategory.SelectedItem.ToString();
                    result = result.Where(d => GetCategoryName(d.FK_id_category) == selectedCategory);
                }

                if (comboBoxSortPrice != null)
                {
                    switch (comboBoxSortPrice.SelectedIndex)
                    {
                        case 1:
                            result = result.OrderBy(d => d.Price);
                            break;
                        case 2:
                            result = result.OrderByDescending(d => d.Price);
                            break;
                    }
                }

                filteredDishes.Clear();
                foreach (var dish in result)
                {
                    Image dishImage = null;

                    if (dish.Photo != null && dish.Photo.Length > 0)
                    {
                        using (MemoryStream ms = new MemoryStream(dish.Photo))
                        {
                            try
                            {
                                dishImage = Image.FromStream(ms);
                            }
                            catch
                            {
                                dishImage = defaultImage;
                            }
                        }
                    }
                    else
                    {
                        dishImage = defaultImage;
                    }

                    filteredDishes.Add(new DishView
                    {
                        Name = dish.Name,
                        Compound = dish.Compound,
                        Price = dish.Price,
                        Photo = dishImage,
                        WeightVolume = dish.WeightVolume
                    });
                }

                dataGridView1.DataSource = null;
                dataGridView1.DataSource = filteredDishes;
                dataGridView1.AutoResizeRows(DataGridViewAutoSizeRowsMode.AllCells);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации: {ex.Message}", "Ошибка");
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Hide();
            ManagerForm Manager = new ManagerForm();
            Manager.Show();
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            if (textBoxSearch != null)
                textBoxSearch.Text = "";
            if (comboBoxCategory != null)
                comboBoxCategory.SelectedIndex = 0;
            if (comboBoxSortPrice != null)
                comboBoxSortPrice.SelectedIndex = 0;
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            if (cartItems.Count == 0 && cartGifts.Count == 0)
            {
                MessageBox.Show("Корзина пуста!", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            UpdateGift();
            OrderCompositionForm form = new OrderCompositionForm(cartItems, cartGifts, this);
            form.CartChanged += () => { UpdateGift(); UpdateCartCount(); };
            form.Show();
            this.Hide();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                dataGridView1.Rows[e.RowIndex].Selected = true;
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count > 0 && dataGridView1.SelectedRows.Count == 0)
            {
                var selectedCell = dataGridView1.SelectedCells[0];
                dataGridView1.ClearSelection();
                dataGridView1.Rows[selectedCell.RowIndex].Selected = true;
            }
        }

        private void textBoxSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar))
                return;

            if (e.KeyChar == ' ')
            {
                if (textBoxSearch.Text.Length >= 255)
                    e.Handled = true;
                return;
            }

            bool isRussian = (e.KeyChar >= 'А' && e.KeyChar <= 'Я') ||
                            (e.KeyChar >= 'а' && e.KeyChar <= 'я') ||
                            e.KeyChar == 'Ё' || e.KeyChar == 'ё';

            if (!isRussian)
            {
                e.Handled = true;
            }
        }

        private void textBoxSearch_TextChanged(object sender, EventArgs e) { }

        private void Menu_Load(object sender, EventArgs e) { }

        private void InitializeDataGridView()
        {
            dataGridView1.ShowCellToolTips = false;
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.ReadOnly = true;
            dataGridView1.MultiSelect = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeRows = false;

            dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.RowTemplate.Height = 60;
            dataGridView1.RowTemplate.MinimumHeight = 60;

            dataGridView1.RowHeadersVisible = false;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersHeight = 60;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = headerBackColor;
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Times New Roman", 16, FontStyle.Bold);
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.ColumnHeadersDefaultCellStyle.Padding = new Padding(0, 5, 0, 5);
            dataGridView1.ColumnHeadersDefaultCellStyle.SelectionBackColor = headerBackColor;
            dataGridView1.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.Black;

            dataGridView1.DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Regular);
            dataGridView1.DefaultCellStyle.Padding = new Padding(0, 2, 0, 2);
            dataGridView1.DefaultCellStyle.BackColor = Color.White;
            dataGridView1.DefaultCellStyle.ForeColor = Color.Black;
            dataGridView1.DefaultCellStyle.SelectionBackColor = selectionColor;
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.Black;

            dataGridView1.RowsDefaultCellStyle.SelectionBackColor = selectionColor;
            dataGridView1.RowsDefaultCellStyle.SelectionForeColor = Color.Black;
            dataGridView1.RowsDefaultCellStyle.BackColor = Color.White;
            dataGridView1.RowsDefaultCellStyle.ForeColor = Color.Black;

            dataGridView1.GridColor = Color.Gray;
            dataGridView1.BorderStyle = BorderStyle.FixedSingle;
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            dataGridView1.ScrollBars = ScrollBars.Both;

            dataGridView1.Columns.Clear();

            DataGridViewImageColumn colPhoto = new DataGridViewImageColumn();
            colPhoto.Name = "Photo";
            colPhoto.HeaderText = "Фото";
            colPhoto.DataPropertyName = "Photo";
            colPhoto.Width = 120;
            colPhoto.MinimumWidth = 100;
            colPhoto.ImageLayout = DataGridViewImageCellLayout.Zoom;
            colPhoto.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns.Add(colPhoto);

            DataGridViewTextBoxColumn colName = new DataGridViewTextBoxColumn();
            colName.Name = "Name";
            colName.HeaderText = "Название";
            colName.DataPropertyName = "Name";
            colName.Width = 200;
            colName.MinimumWidth = 150;
            colName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colName.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.Columns.Add(colName);

            DataGridViewTextBoxColumn colCompound = new DataGridViewTextBoxColumn();
            colCompound.Name = "Compound";
            colCompound.HeaderText = "Состав";
            colCompound.DataPropertyName = "Compound";
            colCompound.Width = 350;
            colCompound.MinimumWidth = 200;
            colCompound.DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopLeft;
            colCompound.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.Columns.Add(colCompound);

            DataGridViewTextBoxColumn colPrice = new DataGridViewTextBoxColumn();
            colPrice.Name = "Price";
            colPrice.HeaderText = "Цена";
            colPrice.DataPropertyName = "FormattedPrice";
            colPrice.Width = 120;
            colPrice.MinimumWidth = 100;
            colPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colPrice.DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Bold);
            colPrice.DefaultCellStyle.ForeColor = Color.DarkGreen;
            dataGridView1.Columns.Add(colPrice);

            DataGridViewTextBoxColumn colWeight = new DataGridViewTextBoxColumn();
            colWeight.Name = "WeightVolume";
            colWeight.HeaderText = "Вес/Объем";
            colWeight.DataPropertyName = "WeightVolume";
            colWeight.Width = 120;
            colWeight.MinimumWidth = 100;
            colWeight.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns.Add(colWeight);

            if (dataGridView1.Columns.Count > 0)
            {
                dataGridView1.Columns[dataGridView1.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
        }
    }

    // ========== ФОРМА СОСТАВА ЗАКАЗА ==========
    public class OrderCompositionForm : Form
    {
        private Label lblTitle;
        private Label lblDate;
        private DateTimePicker dtpDate;
        private DataGridView dgvCart;
        private Label lblTotal;
        private Label lblTotalValue;
        private Button btnContinue;
        private Button btnBack;
        private Panel panelButtons;

        private Button btnIncrease;
        private Button btnDecrease;
        private Button btnDelete;
        private Button btnRemoveGift;

        private List<Menu.CartItem> cartItems;
        private List<Menu.CartGift> cartGifts;
        private Menu parentMenu;

        private Color headerBackColor = Color.FromArgb(97, 173, 123);
        private Color selectionColor = Color.FromArgb(233, 242, 236);

        public event Action CartChanged;

        public OrderCompositionForm(List<Menu.CartItem> items, List<Menu.CartGift> gifts, Menu menu)
        {
            cartItems = items;
            cartGifts = gifts;
            parentMenu = menu;
            InitializeComponent();

            btnContinue.Click += BtnContinue_Click;
            btnIncrease.Click += BtnIncrease_Click;
            btnDecrease.Click += BtnDecrease_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRemoveGift.Click += BtnRemoveGift_Click;
            dgvCart.SelectionChanged += DgvCart_SelectionChanged;

            this.ControlBox = false;
            this.Text = "";

            this.Load += (s, e) =>
            {
                LoadCartData();
                CalculateTotal();
            };
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            RefreshCartDisplay();
        }

        private void InitializeComponent()
        {
            this.Text = "";
            this.Size = new Size(950, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.Font = new Font("Times New Roman", 14, FontStyle.Regular);

            this.FormBorderStyle = FormBorderStyle.None;
            this.ControlBox = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            lblTitle = new Label();
            lblTitle.Text = "СОСТАВ ЗАКАЗА";
            lblTitle.Font = new Font("Times New Roman", 18, FontStyle.Bold);
            lblTitle.Size = new Size(250, 35);
            lblTitle.Location = new Point(350, 20);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblTitle);

            lblDate = new Label();
            lblDate.Text = "Дата:";
            lblDate.Font = new Font("Times New Roman", 14);
            lblDate.Location = new Point(50, 70);
            lblDate.Size = new Size(50, 30);
            this.Controls.Add(lblDate);

            dtpDate = new DateTimePicker();
            dtpDate.Font = new Font("Times New Roman", 14);
            dtpDate.Location = new Point(110, 67);
            dtpDate.Size = new Size(200, 30);
            dtpDate.Value = DateTime.Now;
            dtpDate.Enabled = false;
            this.Controls.Add(dtpDate);

            panelButtons = new Panel();
            panelButtons.Location = new Point(50, 110);
            panelButtons.Size = new Size(850, 80);
            panelButtons.BackColor = Color.FromArgb(240, 240, 240);
            this.Controls.Add(panelButtons);

            btnIncrease = new Button();
            btnIncrease.Text = "+";
            btnIncrease.Font = new Font("Times New Roman", 14, FontStyle.Bold);
            btnIncrease.Size = new Size(50, 35);
            btnIncrease.Location = new Point(10, 5);
            SetupPanelButtonStyle(btnIncrease);
            panelButtons.Controls.Add(btnIncrease);

            btnDecrease = new Button();
            btnDecrease.Text = "-";
            btnDecrease.Font = new Font("Times New Roman", 14, FontStyle.Bold);
            btnDecrease.Size = new Size(50, 35);
            btnDecrease.Location = new Point(65, 5);
            SetupPanelButtonStyle(btnDecrease);
            panelButtons.Controls.Add(btnDecrease);

            btnDelete = new Button();
            btnDelete.Text = "Удалить";
            btnDelete.Font = new Font("Times New Roman", 14);
            btnDelete.Size = new Size(120, 35);
            btnDelete.Location = new Point(120, 5);
            SetupPanelButtonStyle(btnDelete);
            panelButtons.Controls.Add(btnDelete);

            btnRemoveGift = new Button();
            btnRemoveGift.Text = "Удалить подарок";
            btnRemoveGift.Font = new Font("Times New Roman", 14);
            btnRemoveGift.Size = new Size(180, 35);
            btnRemoveGift.Location = new Point(245, 5);
            btnRemoveGift.BackColor = Color.Orange;
            btnRemoveGift.ForeColor = Color.Black;
            SetupPanelButtonStyle(btnRemoveGift);
            panelButtons.Controls.Add(btnRemoveGift);

            Label lblSelectHint = new Label();
            lblSelectHint.Text = "Выберите строку для изменения количества или удаления блюда";
            lblSelectHint.Font = new Font("Times New Roman", 12);
            lblSelectHint.ForeColor = Color.Gray;
            lblSelectHint.Location = new Point(10, 45);
            lblSelectHint.Size = new Size(450, 25);
            panelButtons.Controls.Add(lblSelectHint);

            dgvCart = new DataGridView();
            dgvCart.Location = new Point(50, 200);
            dgvCart.Size = new Size(850, 300);
            dgvCart.BackgroundColor = Color.White;
            dgvCart.AllowUserToAddRows = false;
            dgvCart.AllowUserToDeleteRows = false;
            dgvCart.ReadOnly = true;
            dgvCart.RowHeadersVisible = false;
            dgvCart.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCart.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCart.MultiSelect = false;
            this.Controls.Add(dgvCart);

            lblTotal = new Label();
            lblTotal.Text = "ИТОГО:";
            lblTotal.Font = new Font("Times New Roman", 18, FontStyle.Bold);
            lblTotal.Location = new Point(650, 520);
            lblTotal.Size = new Size(100, 35);
            this.Controls.Add(lblTotal);

            lblTotalValue = new Label();
            lblTotalValue.Font = new Font("Times New Roman", 18, FontStyle.Bold);
            lblTotalValue.Location = new Point(750, 520);
            lblTotalValue.Size = new Size(150, 35);
            lblTotalValue.ForeColor = Color.Red;
            this.Controls.Add(lblTotalValue);

            btnContinue = new Button();
            btnContinue.Text = "Далее";
            btnContinue.Font = new Font("Times New Roman", 14);
            btnContinue.Size = new Size(160, 45);
            btnContinue.Location = new Point(650, 570);
            SetupPanelButtonStyle(btnContinue);
            this.Controls.Add(btnContinue);

            btnBack = new Button();
            btnBack.Text = "Назад";
            btnBack.Font = new Font("Times New Roman", 14);
            btnBack.Size = new Size(160, 45);
            btnBack.Location = new Point(160, 570);
            btnBack.Click += (s, e) =>
            {
                this.Close();
                new Menu().Show();
            };
            SetupPanelButtonStyle(btnBack);
            this.Controls.Add(btnBack);

            InitializeDataGridView();
        }

        private void SetupPanelButtonStyle(Button btn)
        {
            if (btn == null) return;

            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.Black;

            if (btn.Text == "Удалить подарок")
                btn.BackColor = Color.Orange;
            else
                btn.BackColor = Color.DarkSeaGreen;

            btn.ForeColor = Color.Black;
            btn.FlatAppearance.MouseOverBackColor = btn.BackColor;
            btn.FlatAppearance.MouseDownBackColor = btn.BackColor;

            btn.MouseDown += (s, e) => btn.FlatAppearance.BorderColor = Color.DarkBlue;
            btn.MouseUp += (s, e) => btn.FlatAppearance.BorderColor = Color.Black;
            btn.MouseLeave += (s, e) => btn.FlatAppearance.BorderColor = Color.Black;
        }

        private void InitializeDataGridView()
        {
            dgvCart.EnableHeadersVisualStyles = false;
            dgvCart.ColumnHeadersDefaultCellStyle.BackColor = headerBackColor;
            dgvCart.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvCart.ColumnHeadersDefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Bold);
            dgvCart.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCart.ColumnHeadersHeight = 45;
            dgvCart.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            dgvCart.RowsDefaultCellStyle.BackColor = Color.White;
            dgvCart.RowsDefaultCellStyle.ForeColor = Color.Black;
            dgvCart.RowsDefaultCellStyle.SelectionBackColor = selectionColor;
            dgvCart.RowsDefaultCellStyle.SelectionForeColor = Color.Black;

            dgvCart.DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Regular);
            dgvCart.DefaultCellStyle.Padding = new Padding(5);
            dgvCart.DefaultCellStyle.BackColor = Color.White;
            dgvCart.DefaultCellStyle.ForeColor = Color.Black;
            dgvCart.DefaultCellStyle.SelectionBackColor = selectionColor;
            dgvCart.DefaultCellStyle.SelectionForeColor = Color.Black;

            dgvCart.RowTemplate.Height = 40;
            dgvCart.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvCart.GridColor = Color.Gray;
            dgvCart.BorderStyle = BorderStyle.FixedSingle;
            dgvCart.CellBorderStyle = DataGridViewCellBorderStyle.Single;
        }

        private void LoadCartData()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("№", typeof(int));
            dt.Columns.Add("Тип", typeof(string));
            dt.Columns.Add("Наименование", typeof(string));
            dt.Columns.Add("Цена", typeof(string));
            dt.Columns.Add("Кол-во", typeof(int));
            dt.Columns.Add("Сумма", typeof(string));

            int index = 1;

            foreach (var item in cartItems)
            {
                dt.Rows.Add(
                    index++,
                    "Блюдо",
                    item.Name,
                    item.Price.ToString("N2") + " ₽",
                    item.Quantity,
                    (item.Price * item.Quantity).ToString("N2") + " ₽"
                );
            }

            foreach (var gift in cartGifts)
            {
                dt.Rows.Add(
                    index++,
                    "Подарок",
                    gift.Name,
                    "0 ₽",
                    gift.Quantity,
                    "0 ₽"
                );
            }

            dgvCart.DataSource = dt;
            dgvCart.AutoGenerateColumns = true;
            dgvCart.Refresh();
            Application.DoEvents();
            ConfigureDataGridViewColumns();
        }

        private void ConfigureDataGridViewColumns()
        {
            try
            {
                if (dgvCart.Columns.Count > 0)
                {
                    if (dgvCart.Columns["№"] != null)
                    {
                        dgvCart.Columns["№"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        dgvCart.Columns["№"].Width = 50;
                    }

                    if (dgvCart.Columns["Тип"] != null)
                    {
                        dgvCart.Columns["Тип"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        dgvCart.Columns["Тип"].Width = 80;
                    }

                    if (dgvCart.Columns["Наименование"] != null)
                        dgvCart.Columns["Наименование"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

                    if (dgvCart.Columns["Цена"] != null)
                        dgvCart.Columns["Цена"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

                    if (dgvCart.Columns["Кол-во"] != null)
                    {
                        dgvCart.Columns["Кол-во"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        dgvCart.Columns["Кол-во"].Width = 80;
                    }

                    if (dgvCart.Columns["Сумма"] != null)
                    {
                        dgvCart.Columns["Сумма"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        dgvCart.Columns["Сумма"].DefaultCellStyle.ForeColor = Color.DarkGreen;
                        dgvCart.Columns["Сумма"].DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Bold);
                    }

                    foreach (DataGridViewRow row in dgvCart.Rows)
                    {
                        if (row.Cells["Тип"].Value?.ToString() == "Подарок")
                        {
                            row.DefaultCellStyle.BackColor = Color.LightYellow;
                            row.DefaultCellStyle.ForeColor = Color.DarkOrange;
                            row.DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Bold);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка настройки колонок: " + ex.Message);
            }
        }

        private void CalculateTotal()
        {
            decimal total = cartItems.Sum(item => item.Price * item.Quantity);
            lblTotalValue.Text = total.ToString("N2") + " ₽";
        }

        private void RefreshCartDisplay()
        {
            LoadCartData();
            CalculateTotal();
            CartChanged?.Invoke();
        }

        private void DgvCart_SelectionChanged(object sender, EventArgs e) { }

        private void BtnRemoveGift_Click(object sender, EventArgs e)
        {
            if (cartGifts.Count == 0)
            {
                MessageBox.Show("В корзине нет подарков!", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show(
                "Удалить подарок из заказа?",
                "Подтверждение удаления подарка",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                cartGifts.Clear();
                RefreshCartDisplay();
                MessageBox.Show("Подарок удален из заказа.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnIncrease_Click(object sender, EventArgs e)
        {
            if (dgvCart.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите блюдо для увеличения количества!",
                    "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string type = dgvCart.SelectedRows[0].Cells["Тип"].Value?.ToString();
                if (type == "Подарок")
                {
                    MessageBox.Show("Количество подарков нельзя изменить!", "Внимание",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string dishName = dgvCart.SelectedRows[0].Cells["Наименование"].Value.ToString();
                var item = cartItems.FirstOrDefault(i => i.Name == dishName);

                if (item != null)
                {
                    item.Quantity++;
                    RefreshCartDisplay();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDecrease_Click(object sender, EventArgs e)
        {
            if (dgvCart.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите блюдо для уменьшения количества!",
                    "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string type = dgvCart.SelectedRows[0].Cells["Тип"].Value?.ToString();
                if (type == "Подарок")
                {
                    MessageBox.Show("Количество подарков нельзя изменить!", "Внимание",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string dishName = dgvCart.SelectedRows[0].Cells["Наименование"].Value.ToString();
                var item = cartItems.FirstOrDefault(i => i.Name == dishName);

                if (item != null)
                {
                    if (item.Quantity > 1)
                    {
                        item.Quantity--;
                        RefreshCartDisplay();
                    }
                    else
                    {
                        DialogResult result = MessageBox.Show(
                            "Количество станет 0. Удалить позицию из заказа?",
                            "Подтверждение",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                        if (result == DialogResult.Yes)
                        {
                            cartItems.Remove(item);
                            RefreshCartDisplay();

                            if (cartItems.Count == 0 && cartGifts.Count == 0)
                            {
                                MessageBox.Show("Корзина пуста. Возврат в меню.",
                                    "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                this.Close();
                                new Menu().Show();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvCart.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите блюдо для удаления!",
                    "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string type = dgvCart.SelectedRows[0].Cells["Тип"].Value?.ToString();
                if (type == "Подарок")
                {
                    MessageBox.Show("Для удаления подарка используйте кнопку 'Удалить подарок'!",
                        "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string dishName = dgvCart.SelectedRows[0].Cells["Наименование"].Value.ToString();
                var item = cartItems.FirstOrDefault(i => i.Name == dishName);

                if (item != null)
                {
                    DialogResult result = MessageBox.Show(
                        $"Удалить '{dishName}' из заказа?",
                        "Подтверждение удаления",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        cartItems.Remove(item);
                        RefreshCartDisplay();

                        if (cartItems.Count == 0 && cartGifts.Count == 0)
                        {
                            MessageBox.Show("Корзина пуста. Возврат в меню.",
                                "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.Close();
                            new Menu().Show();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnContinue_Click(object sender, EventArgs e)
        {
            OrderDetailsForm form = new OrderDetailsForm(cartItems, cartGifts);
            form.Show();
            this.Close();
        }
    }

    // ========== ФОРМА ДЕТАЛЕЙ ЗАКАЗА (С ИСПРАВЛЕННЫМИ ОГРАНИЧЕНИЯМИ) ==========
    // ========== ФОРМА ДЕТАЛЕЙ ЗАКАЗА (С ИСПРАВЛЕННЫМИ ОГРАНИЧЕНИЯМИ) ==========
    public class OrderDetailsForm : Form
    {
        private Label lblTitle;
        private Label lblDateTime;
        private DateTimePicker dtpDate;
        private DateTimePicker dtpTime;
        private Label lblPhone;
        private MaskedTextBox mtxtPhone;
        private Label lblPersons;
        private ComboBox cmbPersons;
        private GroupBox grbDelivery;
        private RadioButton rbDelivery;
        private RadioButton rbPickup;
        private Label lblAddress;
        private TextBox txtAddress;
        private Label lblPayment;
        private ComboBox cmbPayment;
        private Label lblComment;
        private TextBox txtComment;
        private ListBox lstOrderItems;
        private Label lblTotal;
        private Label lblTotalValue;
        private Button btnSave;
        private Button btnBack;

        private GroupBox grbCertificate;
        private RadioButton rbCertificate;
        private TextBox txtCertificateNumber;
        private Button btnCheckCertificate;
        private Button btnCancelCertificate;
        private Label lblCertificateAmount;

        private decimal orderTotal = 0;
        private decimal certificateRemainingAmount = 0;
        private decimal remainingToPay = 0;
        private int certificateId = -1;
        private bool isCertificateValid = false;
        private bool isPartialPayment = false;
        private int actualOrderId = 0;

        private List<Menu.CartItem> cartItems;
        private List<Menu.CartGift> cartGifts;

        private Color headerBackColor = Color.FromArgb(97, 173, 123);
        private Color selectionColor = Color.FromArgb(233, 242, 236);
        private System.Windows.Forms.Timer updateTimer;

        public OrderDetailsForm(List<Menu.CartItem> items, List<Menu.CartGift> gifts)
        {
            cartItems = items;
            cartGifts = gifts;
            orderTotal = cartItems.Sum(i => i.Price * i.Quantity);
            InitializeComponent();
            LoadOrderItems();
            CalculateTotal();
            SetupTimeRestrictions();
            this.ControlBox = false;
            this.Text = "";
        }

        private void SetupTimeRestrictions()
        {
            DateTime now = DateTime.Now;

            // ЗАПРЕЩАЕМ ВЫБОР ДАТЫ В ПРОШЛОМ
            dtpDate.MinDate = DateTime.Now.Date;
            dtpDate.MaxDate = DateTime.Now.Date.AddDays(7);

            if (dtpDate.Value.Date < DateTime.Now.Date)
            {
                dtpDate.Value = DateTime.Now.Date;
            }

            // НАСТРАИВАЕМ ВРЕМЯ
            dtpTime.Format = DateTimePickerFormat.Custom;
            dtpTime.CustomFormat = "HH:mm";
            dtpTime.ShowUpDown = true;

            // МИНИМАЛЬНОЕ ВРЕМЯ - СЕЙЧАС + 1 ЧАС (НО НЕ РАНЬШЕ 8:00 И НЕ ПОЗЖЕ 22:00)
            DateTime minTime = now.AddHours(1);

            // ЕСЛИ СЕЙЧАС ПОЗЖЕ 21:00 ИЛИ РАНЬШЕ 8:00 - ПЕРЕНОСИМ НА ЗАВТРА
            if (now.Hour >= 21 || now.Hour < 8)
            {
                dtpDate.Value = now.Date.AddDays(1);
                dtpTime.Value = new DateTime(now.Year, now.Month, now.Day, 8, 0, 0);
                MessageBox.Show("Заказы принимаются с 08:00 до 22:00.\n" +
                              "Будет установлена завтрашняя дата на 08:00.",
                              "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                // Округляем время до следующего часа
                DateTime roundedMinTime = new DateTime(now.Year, now.Month, now.Day, now.Hour + 1, 0, 0);

                // Проверяем, что время не выходит за 22:00
                if (roundedMinTime.Hour >= 22)
                {
                    dtpDate.Value = now.Date.AddDays(1);
                    dtpTime.Value = new DateTime(now.Year, now.Month, now.Day, 8, 0, 0);
                    MessageBox.Show("Сегодня уже нельзя заказать (после 22:00).\n" +
                                  "Будет установлена завтрашняя дата на 08:00.",
                                  "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    dtpTime.Value = roundedMinTime;
                }
            }

            updateTimer = new System.Windows.Forms.Timer();
            updateTimer.Interval = 60000;
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();

            dtpDate.ValueChanged += DtpDate_ValueChanged;
            dtpTime.ValueChanged += DtpTime_ValueChanged;
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            if (this.IsHandleCreated && !this.IsDisposed)
            {
                this.Invoke((MethodInvoker)delegate { UpdateTimeRestrictions(); });
            }
        }

        private void DtpDate_ValueChanged(object sender, EventArgs e)
        {
            if (dtpDate.Value.Date < DateTime.Now.Date)
            {
                MessageBox.Show("Нельзя выбрать дату в прошлом!",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtpDate.Value = DateTime.Now.Date;
                return;
            }
            UpdateTimeRestrictions();
        }

        private void DtpTime_ValueChanged(object sender, EventArgs e)
        {
            TimeSpan time = dtpTime.Value.TimeOfDay;

            // ПРОВЕРКА - НЕ РАНЬШЕ 8:00
            if (time < new TimeSpan(8, 0, 0))
            {
                MessageBox.Show("Заказы принимаются с 08:00!",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtpTime.Value = new DateTime(dtpTime.Value.Year, dtpTime.Value.Month, dtpTime.Value.Day, 8, 0, 0);
                return;
            }

            // ПРОВЕРКА - НЕ ПОЗЖЕ 22:00
            if (time > new TimeSpan(22, 0, 0))
            {
                MessageBox.Show("Заказы принимаются до 22:00!",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtpTime.Value = new DateTime(dtpTime.Value.Year, dtpTime.Value.Month, dtpTime.Value.Day, 22, 0, 0);
                return;
            }

            // ПРОВЕРКА ДЛЯ СЕГОДНЯШНЕЙ ДАТЫ - НЕ РАНЬШЕ ТЕКУЩЕГО ВРЕМЕНИ + 1 ЧАС
            if (dtpDate.Value.Date == DateTime.Now.Date)
            {
                DateTime minTime = DateTime.Now.AddHours(1);
                if (dtpTime.Value < minTime)
                {
                    MessageBox.Show($"Время не может быть раньше {minTime:HH:mm}!\n" +
                                  "Минимальное время заказа - через час от текущего времени.",
                                  "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dtpTime.Value = minTime;
                    return;
                }
            }
        }

        private void UpdateTimeRestrictions()
        {
            DateTime selectedDate = dtpDate.Value.Date;
            DateTime now = DateTime.Now;

            if (selectedDate == now.Date)
            {
                // ЕСЛИ СЕГОДНЯ ПОЗЖЕ 21:00 - ПЕРЕНОСИМ НА ЗАВТРА
                if (now.Hour >= 21)
                {
                    MessageBox.Show("Сегодня уже нельзя заказать (после 22:00).\n" +
                                  "Будет установлена завтрашняя дата на 08:00.",
                                  "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dtpDate.Value = now.Date.AddDays(1);
                    dtpTime.Value = new DateTime(now.Year, now.Month, now.Day, 8, 0, 0);
                    return;
                }

                DateTime minTime = now.AddHours(1);

                // ЕСЛИ МИНИМАЛЬНОЕ ВРЕМЯ ВЫХОДИТ ЗА 22:00
                if (minTime.Hour >= 22)
                {
                    MessageBox.Show("Сегодня уже нельзя заказать (после 22:00).\n" +
                                  "Будет установлена завтрашняя дата на 08:00.",
                                  "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dtpDate.Value = now.Date.AddDays(1);
                    dtpTime.Value = new DateTime(now.Year, now.Month, now.Day, 8, 0, 0);
                    return;
                }

                // ОКРУГЛЯЕМ ДО СЛЕДУЮЩЕГО ЧАСА
                DateTime roundedMinTime = new DateTime(now.Year, now.Month, now.Day, minTime.Hour, 0, 0);

                if (dtpTime.Value < roundedMinTime)
                {
                    dtpTime.Value = roundedMinTime;
                }
            }
            else
            {
                // ДЛЯ БУДУЩИХ ДНЕЙ - ВРЕМЯ ОТ 8:00 ДО 22:00
                if (dtpTime.Value.TimeOfDay < new TimeSpan(8, 0, 0))
                {
                    dtpTime.Value = new DateTime(dtpTime.Value.Year, dtpTime.Value.Month, dtpTime.Value.Day, 8, 0, 0);
                }
                else if (dtpTime.Value.TimeOfDay > new TimeSpan(22, 0, 0))
                {
                    dtpTime.Value = new DateTime(dtpTime.Value.Year, dtpTime.Value.Month, dtpTime.Value.Day, 22, 0, 0);
                }
            }
        }

        private void InitializeComponent()
        {
            this.Text = "";
            this.Size = new Size(1000, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.Font = new Font("Times New Roman", 14, FontStyle.Regular);

            this.FormBorderStyle = FormBorderStyle.None;
            this.ControlBox = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            lblTitle = new Label();
            lblTitle.Text = "ДЕТАЛИ ЗАКАЗА";
            lblTitle.Font = new Font("Times New Roman", 20, FontStyle.Bold);
            lblTitle.Size = new Size(300, 40);
            lblTitle.Location = new Point(350, 20);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblTitle);

            lblDateTime = new Label();
            lblDateTime.Text = "Дата и время:";
            lblDateTime.Font = new Font("Times New Roman", 14, FontStyle.Bold);
            lblDateTime.Location = new Point(50, 80);
            lblDateTime.Size = new Size(120, 30);
            this.Controls.Add(lblDateTime);

            dtpDate = new DateTimePicker();
            dtpDate.Font = new Font("Times New Roman", 14);
            dtpDate.Location = new Point(180, 77);
            dtpDate.Size = new Size(120, 30);
            dtpDate.Value = DateTime.Now;
            dtpDate.MinDate = DateTime.Now.Date;
            dtpDate.MaxDate = DateTime.Now.Date.AddDays(7);
            this.Controls.Add(dtpDate);

            dtpTime = new DateTimePicker();
            dtpTime.Font = new Font("Times New Roman", 14);
            dtpTime.Location = new Point(310, 77);
            dtpTime.Size = new Size(100, 30);
            dtpTime.Format = DateTimePickerFormat.Custom;
            dtpTime.CustomFormat = "HH:mm";
            dtpTime.ShowUpDown = true;
            this.Controls.Add(dtpTime);

            lblPhone = new Label();
            lblPhone.Text = "Телефон:";
            lblPhone.Font = new Font("Times New Roman", 14, FontStyle.Bold);
            lblPhone.Location = new Point(50, 130);
            lblPhone.Size = new Size(80, 30);
            this.Controls.Add(lblPhone);

            mtxtPhone = new MaskedTextBox();
            mtxtPhone.Mask = "+7 (999) 000-00-00";
            mtxtPhone.Font = new Font("Times New Roman", 14);
            mtxtPhone.Location = new Point(140, 127);
            mtxtPhone.Size = new Size(220, 30);
            this.Controls.Add(mtxtPhone);

            lblPersons = new Label();
            lblPersons.Text = "Кол-во персон:";
            lblPersons.Font = new Font("Times New Roman", 14, FontStyle.Bold);
            lblPersons.Location = new Point(420, 130);
            lblPersons.Size = new Size(140, 30);
            this.Controls.Add(lblPersons);

            cmbPersons = new ComboBox();
            cmbPersons.Font = new Font("Times New Roman", 14);
            cmbPersons.Location = new Point(570, 127);
            cmbPersons.Size = new Size(80, 30);
            cmbPersons.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPersons.Items.AddRange(new object[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            cmbPersons.SelectedIndex = 0;
            this.Controls.Add(cmbPersons);

            grbCertificate = new GroupBox();
            grbCertificate.Text = "Оплата сертификатом";
            grbCertificate.Font = new Font("Times New Roman", 14, FontStyle.Bold);
            grbCertificate.Size = new Size(900, 130);
            grbCertificate.Location = new Point(50, 180);
            grbCertificate.BackColor = Color.FromArgb(255, 255, 220);
            this.Controls.Add(grbCertificate);

            rbCertificate = new RadioButton();
            rbCertificate.Text = "Оплатить сертификатом";
            rbCertificate.Font = new Font("Times New Roman", 14);
            rbCertificate.Location = new Point(15, 25);
            rbCertificate.Size = new Size(200, 30);
            rbCertificate.CheckedChanged += RbCertificate_CheckedChanged;
            grbCertificate.Controls.Add(rbCertificate);

            Label lblCertNum = new Label();
            lblCertNum.Text = "Номер сертификата:";
            lblCertNum.Font = new Font("Times New Roman", 14);
            lblCertNum.Location = new Point(15, 60);
            lblCertNum.Size = new Size(160, 30);
            grbCertificate.Controls.Add(lblCertNum);

            txtCertificateNumber = new TextBox();
            txtCertificateNumber.Font = new Font("Times New Roman", 14);
            txtCertificateNumber.Location = new Point(180, 60);
            txtCertificateNumber.Size = new Size(120, 30);
            txtCertificateNumber.Enabled = false;
            txtCertificateNumber.KeyPress += TxtCertificateNumber_KeyPress;
            grbCertificate.Controls.Add(txtCertificateNumber);

            btnCheckCertificate = new Button();
            btnCheckCertificate.Text = "Проверить";
            btnCheckCertificate.Font = new Font("Times New Roman", 14);
            btnCheckCertificate.Size = new Size(120, 30);
            btnCheckCertificate.Location = new Point(310, 60);
            btnCheckCertificate.Enabled = false;
            btnCheckCertificate.Click += BtnCheckCertificate_Click;
            SetupPanelButtonStyle(btnCheckCertificate);
            grbCertificate.Controls.Add(btnCheckCertificate);

            btnCancelCertificate = new Button();
            btnCancelCertificate.Text = "Отменить оплату сертификатом";
            btnCancelCertificate.Font = new Font("Times New Roman", 14);
            btnCancelCertificate.Size = new Size(220, 30);
            btnCancelCertificate.Location = new Point(440, 60);
            btnCancelCertificate.Enabled = false;
            btnCancelCertificate.BackColor = Color.LightCoral;
            btnCancelCertificate.Click += BtnCancelCertificate_Click;
            SetupPanelButtonStyle(btnCancelCertificate);
            grbCertificate.Controls.Add(btnCancelCertificate);

            lblCertificateAmount = new Label();
            lblCertificateAmount.Text = "";
            lblCertificateAmount.Font = new Font("Times New Roman", 12);
            lblCertificateAmount.ForeColor = Color.Blue;
            lblCertificateAmount.Location = new Point(15, 95);
            lblCertificateAmount.Size = new Size(860, 25);
            grbCertificate.Controls.Add(lblCertificateAmount);

            grbDelivery = new GroupBox();
            grbDelivery.Text = "Способ получения";
            grbDelivery.Font = new Font("Times New Roman", 14, FontStyle.Bold);
            grbDelivery.Size = new Size(900, 110);
            grbDelivery.Location = new Point(50, 330);
            grbDelivery.BackColor = Color.FromArgb(240, 240, 240);
            this.Controls.Add(grbDelivery);

            rbDelivery = new RadioButton();
            rbDelivery.Text = "Доставка";
            rbDelivery.Font = new Font("Times New Roman", 14);
            rbDelivery.Location = new Point(30, 30);
            rbDelivery.Size = new Size(120, 30);
            rbDelivery.Checked = true;
            rbDelivery.CheckedChanged += RbDelivery_CheckedChanged;
            grbDelivery.Controls.Add(rbDelivery);

            rbPickup = new RadioButton();
            rbPickup.Text = "Самовывоз";
            rbPickup.Font = new Font("Times New Roman", 14);
            rbPickup.Location = new Point(170, 30);
            rbPickup.Size = new Size(130, 30);
            grbDelivery.Controls.Add(rbPickup);

            lblAddress = new Label();
            lblAddress.Text = "Адрес:";
            lblAddress.Font = new Font("Times New Roman", 14, FontStyle.Bold);
            lblAddress.Location = new Point(30, 70);
            lblAddress.Size = new Size(70, 30);
            grbDelivery.Controls.Add(lblAddress);

            txtAddress = new TextBox();
            txtAddress.Font = new Font("Times New Roman", 14);
            txtAddress.Location = new Point(110, 67);
            txtAddress.Size = new Size(700, 30);
            grbDelivery.Controls.Add(txtAddress);

            lblPayment = new Label();
            lblPayment.Text = "Выберите способ оплаты для доплаты:";
            lblPayment.Font = new Font("Times New Roman", 14, FontStyle.Bold);
            lblPayment.Location = new Point(50, 460);
            lblPayment.Size = new Size(380, 30);
            this.Controls.Add(lblPayment);

            cmbPayment = new ComboBox();
            cmbPayment.Font = new Font("Times New Roman", 14);
            cmbPayment.Location = new Point(440, 457);
            cmbPayment.Size = new Size(220, 30);
            cmbPayment.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPayment.Items.AddRange(new string[] { "Наличные", "Карта", "Перевод" });
            cmbPayment.SelectedIndex = 0;
            this.Controls.Add(cmbPayment);

            lblComment = new Label();
            lblComment.Text = "Комментарий:";
            lblComment.Font = new Font("Times New Roman", 14, FontStyle.Bold);
            lblComment.Location = new Point(50, 510);
            lblComment.Size = new Size(130, 30);
            this.Controls.Add(lblComment);

            txtComment = new TextBox();
            txtComment.Font = new Font("Times New Roman", 14);
            txtComment.Location = new Point(190, 507);
            txtComment.Size = new Size(700, 30);
            this.Controls.Add(txtComment);

            lstOrderItems = new ListBox();
            lstOrderItems.Font = new Font("Times New Roman", 14);
            lstOrderItems.Location = new Point(50, 560);
            lstOrderItems.Size = new Size(900, 120);
            lstOrderItems.BackColor = Color.FromArgb(240, 240, 240);
            lstOrderItems.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(lstOrderItems);

            lblTotal = new Label();
            lblTotal.Text = "ИТОГО:";
            lblTotal.Font = new Font("Times New Roman", 20, FontStyle.Bold);
            lblTotal.Location = new Point(580, 700);
            lblTotal.Size = new Size(100, 40);
            this.Controls.Add(lblTotal);

            lblTotalValue = new Label();
            lblTotalValue.Font = new Font("Times New Roman", 20, FontStyle.Bold);
            lblTotalValue.Location = new Point(700, 700);
            lblTotalValue.Size = new Size(200, 40);
            lblTotalValue.ForeColor = Color.Red;
            this.Controls.Add(lblTotalValue);

            btnBack = new Button();
            btnBack.Text = "Назад";
            btnBack.Font = new Font("Times New Roman", 14, FontStyle.Bold);
            btnBack.Size = new Size(160, 50);
            btnBack.Location = new Point(150, 750);
            btnBack.Click += (s, e) =>
            {
                this.Close();
                new Menu().Show();
            };
            SetupPanelButtonStyle(btnBack);
            this.Controls.Add(btnBack);

            btnSave = new Button();
            btnSave.Text = "Оформить заказ";
            btnSave.Font = new Font("Times New Roman", 14, FontStyle.Bold);
            btnSave.Size = new Size(200, 50);
            btnSave.Location = new Point(600, 750);
            btnSave.Click += BtnSave_Click;
            SetupPanelButtonStyle(btnSave);
            this.Controls.Add(btnSave);
        }

        private void SetupPanelButtonStyle(Button btn)
        {
            if (btn == null) return;

            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.Black;

            if (btn.Text == "Отменить оплату сертификатом")
                btn.BackColor = Color.LightCoral;
            else
                btn.BackColor = Color.DarkSeaGreen;

            btn.ForeColor = Color.Black;
            btn.FlatAppearance.MouseOverBackColor = btn.BackColor;
            btn.FlatAppearance.MouseDownBackColor = btn.BackColor;

            btn.MouseDown += (s, e) => btn.FlatAppearance.BorderColor = Color.DarkBlue;
            btn.MouseUp += (s, e) => btn.FlatAppearance.BorderColor = Color.Black;
            btn.MouseLeave += (s, e) => btn.FlatAppearance.BorderColor = Color.Black;
        }

        private void RbCertificate_CheckedChanged(object sender, EventArgs e)
        {
            txtCertificateNumber.Enabled = rbCertificate.Checked;
            btnCheckCertificate.Enabled = rbCertificate.Checked;
            btnCancelCertificate.Enabled = rbCertificate.Checked;

            if (!rbCertificate.Checked)
            {
                isCertificateValid = false;
                certificateId = -1;
                certificateRemainingAmount = 0;
                remainingToPay = 0;
                isPartialPayment = false;
                txtCertificateNumber.Text = "";
                CalculateTotal();
            }
        }

        private void BtnCancelCertificate_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Отменить оплату сертификатом?\n" +
                "Будет выбран обычный способ оплаты.",
                "Подтверждение отмены",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                rbCertificate.Checked = false;
                isCertificateValid = false;
                certificateId = -1;
                certificateRemainingAmount = 0;
                remainingToPay = 0;
                isPartialPayment = false;
                lblCertificateAmount.Text = "";
                txtCertificateNumber.Text = "";
                txtCertificateNumber.Enabled = false;
                btnCheckCertificate.Enabled = false;
                btnCancelCertificate.Enabled = false;
                CalculateTotal();

                MessageBox.Show("Оплата сертификатом отменена.\n" +
                               "Будет использован обычный способ оплаты.",
                               "Отмена", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void TxtCertificateNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void BtnCheckCertificate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCertificateNumber.Text))
            {
                MessageBox.Show("Введите номер сертификата!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtCertificateNumber.Text, out int certNumber))
            {
                MessageBox.Show("Номер сертификата должен состоять только из цифр!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string query = @"
                SELECT c.id_certificate, c.last_name, c.first_name, c.middle_name, 
                       c.price, c.id_status_certificate, sc.name as status_name
                FROM certificates c
                LEFT JOIN status_certificates sc ON c.id_status_certificate = sc.id_status_certificate
                WHERE c.id_certificate = @id";

                using (MySqlConnection conn = SettingsBD.GetConnection())
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", certNumber);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int statusId = reader.GetInt32("id_status_certificate");
                                string statusName = reader["status_name"].ToString();
                                decimal certificatePrice = reader.GetDecimal("price");

                                if (statusName != "Активен")
                                {
                                    MessageBox.Show($"Сертификат №{certNumber} имеет статус '{statusName}'!\nОплата невозможна.",
                                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    isCertificateValid = false;
                                    CalculateTotal();
                                    return;
                                }

                                if (certificatePrice >= orderTotal)
                                {
                                    isCertificateValid = true;
                                    certificateId = certNumber;
                                    certificateRemainingAmount = certificatePrice - orderTotal;
                                    isPartialPayment = false;
                                    remainingToPay = 0;

                                    MessageBox.Show($"✅ Сертификат №{certNumber} будет использован ПОЛНОСТЬЮ!\n" +
                                                  $"Сумма сертификата: {certificatePrice:N2} ₽\n" +
                                                  $"Сумма заказа: {orderTotal:N2} ₽",
                                                  "Сертификат найден", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                    CalculateTotal();
                                }
                                else
                                {
                                    DialogResult result = MessageBox.Show(
                                        $"На сертификате недостаточно средств!\n\n" +
                                        $"Сертификат №{certNumber} будет использован ЧАСТИЧНО.\n" +
                                        $"Сумма сертификата: {certificatePrice:N2} ₽\n" +
                                        $"Сумма заказа: {orderTotal:N2} ₽\n" +
                                        $"Необходимо доплатить: {(orderTotal - certificatePrice):N2} ₽\n\n" +
                                        $"Продолжить?",
                                        "Частичная оплата",
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Question);

                                    if (result == DialogResult.Yes)
                                    {
                                        isCertificateValid = true;
                                        certificateId = certNumber;
                                        certificateRemainingAmount = 0;
                                        remainingToPay = orderTotal - certificatePrice;
                                        isPartialPayment = true;

                                        MessageBox.Show($"✅ Сертификат №{certNumber} будет использован ЧАСТИЧНО!\n" +
                                                      $"Списано с сертификата: {certificatePrice:N2} ₽\n" +
                                                      $"Осталось доплатить: {remainingToPay:N2} ₽\n" +
                                                      $"Выберите способ оплаты для доплаты.",
                                                      "Сертификат найден", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                        CalculateTotalWithPartialPayment();
                                    }
                                    else
                                    {
                                        isCertificateValid = false;
                                        CalculateTotal();
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show($"Сертификат №{certNumber} не найден!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                isCertificateValid = false;
                                CalculateTotal();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                isCertificateValid = false;
                CalculateTotal();
            }
        }

        private void CalculateTotal()
        {
            if (isCertificateValid && rbCertificate.Checked && !isPartialPayment)
            {
                lblTotalValue.Text = "0.00 ₽ (Оплачено сертификатом)";
                lblTotalValue.ForeColor = Color.Green;
            }
            else if (isPartialPayment)
            {
                lblTotalValue.Text = $"{remainingToPay:N2} ₽ (Доплата)";
                lblTotalValue.ForeColor = Color.Red;
            }
            else
            {
                lblTotalValue.Text = orderTotal.ToString("N2") + " ₽";
                lblTotalValue.ForeColor = Color.Red;
            }
        }

        private void CalculateTotalWithPartialPayment()
        {
            lblTotalValue.Text = $"{remainingToPay:N2} ₽ (Доплата)";
            lblTotalValue.ForeColor = Color.Red;
        }

        private void RbDelivery_CheckedChanged(object sender, EventArgs e)
        {
            txtAddress.Enabled = rbDelivery.Checked;
            if (!rbDelivery.Checked)
                txtAddress.Text = "";
        }

        private void LoadOrderItems()
        {
            lstOrderItems.Items.Clear();

            foreach (var item in cartItems)
                lstOrderItems.Items.Add($"{item.Name} x{item.Quantity} = {(item.Price * item.Quantity):N2} ₽");

            foreach (var gift in cartGifts)
                lstOrderItems.Items.Add($"🎁 {gift.Name} - ПОДАРОК (бесплатно)");
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // ===== ПРОВЕРКА - НЕЛЬЗЯ ВЫБИРАТЬ ДАТУ В ПРОШЛОМ =====
            if (dtpDate.Value.Date < DateTime.Now.Date)
            {
                MessageBox.Show("Нельзя выбрать дату в прошлом!\nВыберите сегодняшнюю или будущую дату.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtpDate.Focus();
                return;
            }

            // ===== ПРОВЕРКА - ВРЕМЯ ОТ 8:00 ДО 22:00 =====
            TimeSpan time = dtpTime.Value.TimeOfDay;
            if (time < new TimeSpan(8, 0, 0))
            {
                MessageBox.Show("Заказы принимаются с 08:00 до 22:00!",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtpTime.Focus();
                return;
            }
            if (time > new TimeSpan(22, 0, 0))
            {
                MessageBox.Show("Заказы принимаются с 08:00 до 22:00!",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtpTime.Focus();
                return;
            }

            // ===== ПРОВЕРКА - НЕЛЬЗЯ ВЫБИРАТЬ ВРЕМЯ В ПРОШЛОМ =====
            if (dtpDate.Value.Date == DateTime.Now.Date)
            {
                DateTime minTime = DateTime.Now.AddHours(1);
                if (dtpTime.Value < minTime)
                {
                    MessageBox.Show($"Время не может быть раньше {minTime:HH:mm}!\n" +
                                  "Минимальное время заказа - через час от текущего времени.",
                                  "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dtpTime.Focus();
                    return;
                }
            }

            if (!mtxtPhone.MaskCompleted)
            {
                MessageBox.Show("Введите номер телефона!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                mtxtPhone.Focus();
                return;
            }

            if (rbDelivery.Checked && string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                MessageBox.Show("Введите адрес доставки!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAddress.Focus();
                return;
            }

            if (rbCertificate.Checked)
            {
                if (!isCertificateValid)
                {
                    MessageBox.Show("Сертификат не проверен или недействителен!\n" +
                                  "Пожалуйста, проверьте сертификат перед оформлением.",
                                  "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    string checkQuery = "SELECT price, id_status_certificate FROM certificates WHERE id_certificate = @id";
                    using (MySqlConnection conn = SettingsBD.GetConnection())
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(checkQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", certificateId);
                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    decimal currentPrice = reader.GetDecimal("price");
                                    int statusId = reader.GetInt32("id_status_certificate");
                                    if (statusId != 1)
                                    {
                                        MessageBox.Show("Статус сертификата изменился! Оплата невозможна.",
                                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        return;
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Сертификат не найден!", "Ошибка",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка проверки сертификата: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            try
            {
                SaveToDatabase();
                CreateWordReceipt();

                string paymentMessage = "";
                if (rbCertificate.Checked)
                {
                    if (isPartialPayment)
                        paymentMessage = $"Оплачено сертификатом №{certificateId} ({(orderTotal - remainingToPay):N2} ₽) + доплата {remainingToPay:N2} ₽ {cmbPayment.SelectedItem}";
                    else
                        paymentMessage = $"Оплачено сертификатом №{certificateId} (полностью)";
                }
                else
                {
                    paymentMessage = $"Способ оплаты: {cmbPayment.SelectedItem}";
                }

                MessageBox.Show($"✅ Заказ №{actualOrderId} оформлен!\n{paymentMessage}", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Close();
                new Menu().Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveToDatabase()
        {
            using (MySqlConnection conn = SettingsBD.GetConnection())
            {
                conn.Open();
                using (MySqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        if (rbCertificate.Checked && isCertificateValid)
                        {
                            decimal newPrice = 0;
                            int newStatusId = 2;

                            if (isPartialPayment)
                            {
                                string getCurrentPriceQuery = "SELECT price FROM certificates WHERE id_certificate = @id";
                                using (MySqlCommand getPriceCmd = new MySqlCommand(getCurrentPriceQuery, conn, transaction))
                                {
                                    getPriceCmd.Parameters.AddWithValue("@id", certificateId);
                                    decimal currentPrice = Convert.ToDecimal(getPriceCmd.ExecuteScalar());
                                    newPrice = 0;
                                    newStatusId = 2;
                                }
                            }
                            else
                            {
                                newPrice = certificateRemainingAmount;
                                newStatusId = newPrice > 0 ? 1 : 2;
                            }

                            string updateCertQuery = "UPDATE certificates SET price = @newPrice, id_status_certificate = @statusId WHERE id_certificate = @id";
                            using (MySqlCommand updateCmd = new MySqlCommand(updateCertQuery, conn, transaction))
                            {
                                updateCmd.Parameters.AddWithValue("@newPrice", newPrice);
                                updateCmd.Parameters.AddWithValue("@statusId", newStatusId);
                                updateCmd.Parameters.AddWithValue("@id", certificateId);
                                updateCmd.ExecuteNonQuery();
                            }
                        }

                        string orderQuery = @"INSERT INTO orders 
                        (phone_number, address, number_persons, 
                         delivery_date, delivery_time, comment, payment_method, id_status, total_amount) 
                        VALUES 
                        (@phone, @addr, @pers, @date, @time, @comm, @pay, 2, @total);
                        SELECT LAST_INSERT_ID();";

                        long orderId;
                        string paymentMethod;
                        decimal totalAmount;

                        if (rbCertificate.Checked)
                        {
                            if (isPartialPayment)
                            {
                                paymentMethod = $"Сертификат №{certificateId} + {cmbPayment.SelectedItem}";
                                totalAmount = remainingToPay;
                            }
                            else
                            {
                                paymentMethod = $"Сертификат №{certificateId}";
                                totalAmount = 0;
                            }
                        }
                        else
                        {
                            paymentMethod = cmbPayment.SelectedItem.ToString();
                            totalAmount = orderTotal;
                        }

                        using (MySqlCommand cmd = new MySqlCommand(orderQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@phone", mtxtPhone.Text);
                            cmd.Parameters.AddWithValue("@addr", rbDelivery.Checked ? txtAddress.Text : "Самовывоз");
                            cmd.Parameters.AddWithValue("@pers", Convert.ToInt32(cmbPersons.SelectedItem));
                            cmd.Parameters.AddWithValue("@date", dtpDate.Value.Date);
                            cmd.Parameters.AddWithValue("@time", dtpTime.Value.TimeOfDay);
                            cmd.Parameters.AddWithValue("@comm", txtComment.Text);
                            cmd.Parameters.AddWithValue("@pay", paymentMethod);
                            cmd.Parameters.AddWithValue("@total", totalAmount);

                            orderId = Convert.ToInt64(cmd.ExecuteScalar());
                            actualOrderId = (int)orderId;
                        }

                        foreach (var item in cartItems)
                        {
                            string dishQuery = @"INSERT INTO order_dish 
                            (id_order, id_dish, quantity, price_at_order, is_gift, id_present) 
                            VALUES (@oid, @did, @qty, @price, FALSE, NULL)";

                            using (MySqlCommand dishCmd = new MySqlCommand(dishQuery, conn, transaction))
                            {
                                dishCmd.Parameters.AddWithValue("@oid", orderId);
                                dishCmd.Parameters.AddWithValue("@did", item.Id_dish);
                                dishCmd.Parameters.AddWithValue("@qty", item.Quantity);
                                dishCmd.Parameters.AddWithValue("@price", item.Price);
                                dishCmd.ExecuteNonQuery();
                            }
                        }

                        if (cartGifts != null && cartGifts.Count > 0)
                        {
                            foreach (var gift in cartGifts)
                            {
                                string giftQuery = @"INSERT INTO order_dish 
                                (id_order, id_dish, quantity, price_at_order, is_gift, id_present) 
                                VALUES (@oid, 1, @qty, 0, TRUE, @pid)";

                                using (MySqlCommand giftCmd = new MySqlCommand(giftQuery, conn, transaction))
                                {
                                    giftCmd.Parameters.AddWithValue("@oid", orderId);
                                    giftCmd.Parameters.AddWithValue("@qty", gift.Quantity);
                                    giftCmd.Parameters.AddWithValue("@pid", gift.Id_present);
                                    giftCmd.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private void CreateWordReceipt()
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "Сохранить чек";
                saveFileDialog.Filter = "Документ Word (*.docx)|*.docx|Текстовый файл (*.txt)|*.txt";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.DefaultExt = "docx";
                saveFileDialog.FileName = $"Чек_{actualOrderId}";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog.FileName;
                    string extension = Path.GetExtension(filePath).ToLower();

                    if (extension == ".docx")
                        CreateWordDocument(filePath);
                    else if (extension == ".txt")
                        CreateTextFile(filePath);

                    System.Diagnostics.Process.Start(filePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании чека: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateWordDocument(string filePath)
        {
            try
            {
                Microsoft.Office.Interop.Word.Application wordApp = new Microsoft.Office.Interop.Word.Application();
                wordApp.Visible = false;

                Microsoft.Office.Interop.Word.Document wordDoc = wordApp.Documents.Add();

                wordDoc.PageSetup.TopMargin = wordApp.CentimetersToPoints(1.5f);
                wordDoc.PageSetup.BottomMargin = wordApp.CentimetersToPoints(1.5f);
                wordDoc.PageSetup.LeftMargin = wordApp.CentimetersToPoints(2f);
                wordDoc.PageSetup.RightMargin = wordApp.CentimetersToPoints(1.5f);

                Microsoft.Office.Interop.Word.Range range = wordDoc.Content;
                range.Font.Name = "Times New Roman";
                range.Font.Size = 12;

                range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                range.Font.Bold = 1;
                range.Font.Size = 16;
                range.Text = "РЕСТОРАН\n";
                range.InsertParagraphAfter();
                range.Collapse(Microsoft.Office.Interop.Word.WdCollapseDirection.wdCollapseEnd);

                range.Font.Bold = 0;
                range.Font.Size = 12;
                range.Text = "=================================\n";
                range.InsertParagraphAfter();
                range.Collapse(Microsoft.Office.Interop.Word.WdCollapseDirection.wdCollapseEnd);

                range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphLeft;
                range.Text = $"Заказ: {actualOrderId}\n";
                range.InsertParagraphAfter();
                range.Collapse(Microsoft.Office.Interop.Word.WdCollapseDirection.wdCollapseEnd);

                range.Text = $"Дата: {dtpDate.Value.ToShortDateString()}\n";
                range.InsertParagraphAfter();
                range.Collapse(Microsoft.Office.Interop.Word.WdCollapseDirection.wdCollapseEnd);

                range.Text = $"Время: {dtpTime.Value:HH:mm}\n";
                range.InsertParagraphAfter();
                range.Collapse(Microsoft.Office.Interop.Word.WdCollapseDirection.wdCollapseEnd);

                range.Text = "---------------------------------\n";
                range.InsertParagraphAfter();
                range.Collapse(Microsoft.Office.Interop.Word.WdCollapseDirection.wdCollapseEnd);

                Microsoft.Office.Interop.Word.Table table = wordDoc.Tables.Add(range, cartItems.Count + cartGifts.Count + 1, 4);
                table.Borders.Enable = 1;
                table.Range.Font.Name = "Times New Roman";
                table.Range.Font.Size = 12;

                table.Columns[1].Width = wordApp.CentimetersToPoints(1.5f);
                table.Columns[2].Width = wordApp.CentimetersToPoints(8f);
                table.Columns[3].Width = wordApp.CentimetersToPoints(2f);
                table.Columns[4].Width = wordApp.CentimetersToPoints(3f);

                table.Cell(1, 1).Range.Text = "№";
                table.Cell(1, 2).Range.Text = "Наименование";
                table.Cell(1, 3).Range.Text = "Кол-во";
                table.Cell(1, 4).Range.Text = "Сумма";

                for (int i = 1; i <= 4; i++)
                {
                    table.Cell(1, i).Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    table.Cell(1, i).Range.Font.Bold = 1;
                }

                int row = 2;
                int index = 1;

                foreach (var item in cartItems)
                {
                    table.Cell(row, 1).Range.Text = index.ToString();
                    table.Cell(row, 2).Range.Text = item.Name;
                    table.Cell(row, 3).Range.Text = item.Quantity.ToString();
                    table.Cell(row, 4).Range.Text = (item.Price * item.Quantity).ToString("N2") + " ₽";

                    table.Cell(row, 1).Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    table.Cell(row, 2).Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphLeft;
                    table.Cell(row, 3).Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    table.Cell(row, 4).Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphRight;

                    row++;
                    index++;
                }

                foreach (var gift in cartGifts)
                {
                    table.Cell(row, 1).Range.Text = index.ToString();
                    table.Cell(row, 2).Range.Text = gift.Name;
                    table.Cell(row, 3).Range.Text = gift.Quantity.ToString();
                    table.Cell(row, 4).Range.Text = "0 ₽ (Подарок)";

                    table.Cell(row, 1).Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    table.Cell(row, 2).Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphLeft;
                    table.Cell(row, 3).Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    table.Cell(row, 4).Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphRight;

                    row++;
                    index++;
                }

                range = wordDoc.Range();
                range.Collapse(Microsoft.Office.Interop.Word.WdCollapseDirection.wdCollapseEnd);

                range.Text = "\n";
                range.InsertParagraphAfter();
                range.Collapse(Microsoft.Office.Interop.Word.WdCollapseDirection.wdCollapseEnd);

                range.Font.Bold = 1;
                range.Font.Size = 14;
                range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphRight;

                if (rbCertificate.Checked && isCertificateValid)
                {
                    if (isPartialPayment)
                        range.Text = $"ОПЛАЧЕНО: Сертификат №{certificateId} ({(orderTotal - remainingToPay):N2} ₽) + Доплата {remainingToPay:N2} ₽\n";
                    else
                        range.Text = $"ОПЛАЧЕНО СЕРТИФИКАТОМ №{certificateId}\n";
                }
                else
                {
                    range.Text = $"ИТОГО: {orderTotal:N2} ₽\n";
                }

                range.InsertParagraphAfter();
                range.Collapse(Microsoft.Office.Interop.Word.WdCollapseDirection.wdCollapseEnd);

                range.Font.Bold = 0;
                range.Font.Size = 12;
                range.Text = "---------------------------------\n";
                range.InsertParagraphAfter();
                range.Collapse(Microsoft.Office.Interop.Word.WdCollapseDirection.wdCollapseEnd);

                range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphLeft;
                range.Text = $"Телефон: {mtxtPhone.Text}\n";
                range.InsertParagraphAfter();
                range.Collapse(Microsoft.Office.Interop.Word.WdCollapseDirection.wdCollapseEnd);

                string address = rbDelivery.Checked ? txtAddress.Text : "Самовывоз";
                range.Text = $"Адрес: {address}\n";
                range.InsertParagraphAfter();
                range.Collapse(Microsoft.Office.Interop.Word.WdCollapseDirection.wdCollapseEnd);

                if (rbCertificate.Checked && isCertificateValid)
                {
                    if (isPartialPayment)
                        range.Text = $"Оплата: Сертификат №{certificateId} + {cmbPayment.SelectedItem}\n";
                    else
                        range.Text = $"Оплата: Сертификат №{certificateId}\n";
                }
                else
                {
                    range.Text = $"Оплата: {cmbPayment.SelectedItem}\n";
                }

                range.InsertParagraphAfter();
                range.Collapse(Microsoft.Office.Interop.Word.WdCollapseDirection.wdCollapseEnd);

                range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                range.Text = "=================================\n";

                wordDoc.SaveAs(filePath);
                wordDoc.Close();
                wordApp.Quit();

                System.Runtime.InteropServices.Marshal.ReleaseComObject(table);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(range);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(wordDoc);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(wordApp);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при создании Word документа: {ex.Message}");
            }
        }

        private void CreateTextFile(string filePath)
        {
            using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                sw.WriteLine("=================================");
                sw.WriteLine("         РЕСТОРАН");
                sw.WriteLine("=================================");
                sw.WriteLine($"Заказ: {actualOrderId}");
                sw.WriteLine($"Дата: {dtpDate.Value.ToShortDateString()}");
                sw.WriteLine($"Время: {dtpTime.Value:HH:mm}");
                sw.WriteLine("---------------------------------");

                sw.WriteLine($"{"№",-4} {"Наименование",-40} {"Кол-во",-8} {"Сумма",-12}");
                sw.WriteLine("------------------------------------------------------------");

                int index = 1;

                foreach (var item in cartItems)
                {
                    string name = item.Name.Length > 38 ? item.Name.Substring(0, 35) + "..." : item.Name;
                    sw.WriteLine($"{index,-4} {name,-40} {item.Quantity,-8} {(item.Price * item.Quantity):N2} ₽");
                    index++;
                }

                foreach (var gift in cartGifts)
                {
                    string name = gift.Name.Length > 38 ? gift.Name.Substring(0, 35) + "..." : gift.Name;
                    sw.WriteLine($"{index,-4} {name,-40} {gift.Quantity,-8} {"0 ₽ (Подарок)",-12}");
                    index++;
                }

                sw.WriteLine("------------------------------------------------------------");

                if (rbCertificate.Checked && isCertificateValid)
                {
                    if (isPartialPayment)
                        sw.WriteLine($"ОПЛАЧЕНО: Сертификат №{certificateId} ({(orderTotal - remainingToPay):N2} ₽) + Доплата {remainingToPay:N2} ₽".PadLeft(60));
                    else
                        sw.WriteLine($"ОПЛАЧЕНО СЕРТИФИКАТОМ №{certificateId}".PadLeft(60));
                }
                else
                {
                    sw.WriteLine($"ИТОГО: {orderTotal:N2} ₽".PadLeft(60));
                }

                sw.WriteLine("---------------------------------");
                sw.WriteLine($"Телефон: {mtxtPhone.Text}");
                sw.WriteLine($"Адрес: {(rbDelivery.Checked ? txtAddress.Text : "Самовывоз")}");

                if (rbCertificate.Checked && isCertificateValid)
                {
                    if (isPartialPayment)
                        sw.WriteLine($"Оплата: Сертификат №{certificateId} + {cmbPayment.SelectedItem}");
                    else
                        sw.WriteLine($"Оплата: Сертификат №{certificateId}");
                }
                else
                {
                    sw.WriteLine($"Оплата: {cmbPayment.SelectedItem}");
                }

                sw.WriteLine("=================================");
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (updateTimer != null)
            {
                updateTimer.Stop();
                updateTimer.Dispose();
            }
            base.OnFormClosed(e);
        }
    }
}