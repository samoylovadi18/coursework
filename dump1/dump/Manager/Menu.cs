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

        // Поля для подарков
        private List<Gift> giftsList = new List<Gift>();
        private List<CartGift> cartGifts = new List<CartGift>();
        private bool isGiftAdded = false;

        // Цвета для единого стиля
        private Color headerBackColor = Color.FromArgb(97, 173, 123);
        private Color selectionColor = Color.FromArgb(233, 242, 236);
        private Color buttonColor = Color.DarkSeaGreen;

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
        // Класс подарка из базы
        public class Gift
        {
            public int Id_present { get; set; }
            public string Name { get; set; }
            public decimal FromPrice { get; set; }
        }

        // Класс подарка в корзине
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
            StyleButton(buttonReset);
        }

        private void StyleButton(Button btn)
        {
            if (btn == null) return;

            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.Black;
            btn.BackColor = buttonColor;
            btn.ForeColor = Color.Black;
            btn.FlatAppearance.MouseOverBackColor = buttonColor;
            btn.FlatAppearance.MouseDownBackColor = buttonColor;

            btn.MouseDown += (s, e) => btn.FlatAppearance.BorderColor = Color.DarkBlue;
            btn.MouseUp += (s, e) => btn.FlatAppearance.BorderColor = Color.Black;
            btn.MouseLeave += (s, e) => btn.FlatAppearance.BorderColor = Color.Black;
        }

        // Загрузка подарков
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

        // Обновление подарка
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
            OrderCompositionForm form = new OrderCompositionForm(cartItems, cartGifts);
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

            // Колонка для фото
            DataGridViewImageColumn colPhoto = new DataGridViewImageColumn();
            colPhoto.Name = "Photo";
            colPhoto.HeaderText = "Фото";
            colPhoto.DataPropertyName = "Photo";
            colPhoto.Width = 120;
            colPhoto.MinimumWidth = 100;
            colPhoto.ImageLayout = DataGridViewImageCellLayout.Zoom;
            colPhoto.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns.Add(colPhoto);

            // Колонка для названия
            DataGridViewTextBoxColumn colName = new DataGridViewTextBoxColumn();
            colName.Name = "Name";
            colName.HeaderText = "Название";
            colName.DataPropertyName = "Name";
            colName.Width = 200;
            colName.MinimumWidth = 150;
            colName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colName.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.Columns.Add(colName);

            // Колонка для состава
            DataGridViewTextBoxColumn colCompound = new DataGridViewTextBoxColumn();
            colCompound.Name = "Compound";
            colCompound.HeaderText = "Состав";
            colCompound.DataPropertyName = "Compound";
            colCompound.Width = 350;
            colCompound.MinimumWidth = 200;
            colCompound.DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopLeft;
            colCompound.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.Columns.Add(colCompound);

            // Колонка для цены
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

            // Колонка для веса
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
        private Label lblOrderNum;
        private TextBox txtOrderNum;
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

        private Color headerBackColor = Color.FromArgb(97, 173, 123);
        private Color selectionColor = Color.FromArgb(233, 242, 236);
        private Color buttonColor = Color.DarkSeaGreen;

        public OrderCompositionForm(List<Menu.CartItem> items, List<Menu.CartGift> gifts)
        {
            cartItems = items;
            cartGifts = gifts;
            InitializeComponent();
            GenerateOrderNumber();

            btnContinue.Click += BtnContinue_Click;
            btnIncrease.Click += BtnIncrease_Click;
            btnDecrease.Click += BtnDecrease_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRemoveGift.Click += BtnRemoveGift_Click;
            dgvCart.SelectionChanged += DgvCart_SelectionChanged;

            this.ControlBox = false;
            this.Text = "";

            this.Load += (s, e) => {
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
            this.Size = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.Font = new Font("Times New Roman", 12, FontStyle.Regular);

            this.FormBorderStyle = FormBorderStyle.None;
            this.ControlBox = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            lblTitle = new Label();
            lblTitle.Text = "СОСТАВ ЗАКАЗА";
            lblTitle.Font = new Font("Times New Roman", 18, FontStyle.Bold);
            lblTitle.Size = new Size(250, 35);
            lblTitle.Location = new Point(325, 20);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblTitle);

            lblOrderNum = new Label();
            lblOrderNum.Text = "Номер заказа:";
            lblOrderNum.Font = new Font("Times New Roman", 12);
            lblOrderNum.Location = new Point(50, 70);
            lblOrderNum.Size = new Size(100, 25);
            this.Controls.Add(lblOrderNum);

            txtOrderNum = new TextBox();
            txtOrderNum.Font = new Font("Times New Roman", 12);
            txtOrderNum.Location = new Point(160, 67);
            txtOrderNum.Size = new Size(200, 30);
            txtOrderNum.ReadOnly = true;
            txtOrderNum.BackColor = Color.LightGray;
            this.Controls.Add(txtOrderNum);

            lblDate = new Label();
            lblDate.Text = "Дата:";
            lblDate.Font = new Font("Times New Roman", 12);
            lblDate.Location = new Point(400, 70);
            lblDate.Size = new Size(50, 25);
            this.Controls.Add(lblDate);

            dtpDate = new DateTimePicker();
            dtpDate.Font = new Font("Times New Roman", 12);
            dtpDate.Location = new Point(460, 67);
            dtpDate.Size = new Size(150, 30);
            dtpDate.Value = DateTime.Now;
            dtpDate.Enabled = false;
            this.Controls.Add(dtpDate);

            panelButtons = new Panel();
            panelButtons.Location = new Point(50, 110);
            panelButtons.Size = new Size(800, 80);
            panelButtons.BackColor = Color.FromArgb(240, 240, 240);
            this.Controls.Add(panelButtons);

            btnIncrease = new Button();
            btnIncrease.Text = "+";
            btnIncrease.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            btnIncrease.Size = new Size(40, 30);
            btnIncrease.Location = new Point(10, 5);
            panelButtons.Controls.Add(btnIncrease);

            btnDecrease = new Button();
            btnDecrease.Text = "-";
            btnDecrease.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            btnDecrease.Size = new Size(40, 30);
            btnDecrease.Location = new Point(60, 5);
            panelButtons.Controls.Add(btnDecrease);

            btnDelete = new Button();
            btnDelete.Text = "Удалить";
            btnDelete.Font = new Font("Times New Roman", 12);
            btnDelete.Size = new Size(100, 30);
            btnDelete.Location = new Point(110, 5);
            panelButtons.Controls.Add(btnDelete);

            btnRemoveGift = new Button();
            btnRemoveGift.Text = "Удалить подарок";
            btnRemoveGift.Font = new Font("Times New Roman", 12);
            btnRemoveGift.Size = new Size(150, 30);
            btnRemoveGift.Location = new Point(220, 5);
            btnRemoveGift.BackColor = Color.Orange;
            btnRemoveGift.ForeColor = Color.Black;
            panelButtons.Controls.Add(btnRemoveGift);

            Label lblSelectHint = new Label();
            lblSelectHint.Text = "Выберите строку для изменения количества или удаления блюда";
            lblSelectHint.Font = new Font("Times New Roman", 10);
            lblSelectHint.ForeColor = Color.Gray;
            lblSelectHint.Location = new Point(10, 45);
            lblSelectHint.Size = new Size(600, 25);
            panelButtons.Controls.Add(lblSelectHint);

            dgvCart = new DataGridView();
            dgvCart.Location = new Point(50, 200);
            dgvCart.Size = new Size(800, 300);
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
            lblTotal.Font = new Font("Times New Roman", 16, FontStyle.Bold);
            lblTotal.Location = new Point(600, 520);
            lblTotal.Size = new Size(100, 35);
            this.Controls.Add(lblTotal);

            lblTotalValue = new Label();
            lblTotalValue.Font = new Font("Times New Roman", 16, FontStyle.Bold);
            lblTotalValue.Location = new Point(700, 520);
            lblTotalValue.Size = new Size(150, 35);
            lblTotalValue.ForeColor = Color.Red;
            this.Controls.Add(lblTotalValue);

            btnContinue = new Button();
            btnContinue.Text = "Далее";
            btnContinue.Font = new Font("Times New Roman", 12);
            btnContinue.Size = new Size(120, 45);
            btnContinue.Location = new Point(600, 570);
            this.Controls.Add(btnContinue);

            btnBack = new Button();
            btnBack.Text = "Назад";
            btnBack.Font = new Font("Times New Roman", 12);
            btnBack.Size = new Size(120, 45);
            btnBack.Location = new Point(200, 570);
            btnBack.Click += (s, e) => {
                this.Close();
                new Menu().Show();
            };
            this.Controls.Add(btnBack);
        }

        private void GenerateOrderNumber()
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            string random = new Random().Next(1000, 9999).ToString();
            txtOrderNum.Text = $"ORD-{timestamp}-{random}";
        }

        private void StyleButtons()
        {
            StyleButton(btnContinue);
            StyleButton(btnBack);
            StyleButton(btnIncrease);
            StyleButton(btnDecrease);
            StyleButton(btnDelete);
            StyleButton(btnRemoveGift);
        }

        private void StyleButton(Button btn)
        {
            if (btn == null) return;

            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.Black;

            if (btn.Text == "Удалить подарок")
                btn.BackColor = Color.Orange;
            else
                btn.BackColor = buttonColor;

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

            dgvCart.DefaultCellStyle.Font = new Font("Times New Roman", 12, FontStyle.Regular);
            dgvCart.DefaultCellStyle.Padding = new Padding(5);
            dgvCart.DefaultCellStyle.BackColor = Color.White;
            dgvCart.DefaultCellStyle.ForeColor = Color.Black;
            dgvCart.DefaultCellStyle.SelectionBackColor = selectionColor;
            dgvCart.DefaultCellStyle.SelectionForeColor = Color.Black;

            dgvCart.RowTemplate.Height = 35;
            dgvCart.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvCart.GridColor = Color.Gray;
            dgvCart.BorderStyle = BorderStyle.FixedSingle;
            dgvCart.CellBorderStyle = DataGridViewCellBorderStyle.Single;
        }

        private void LoadCartData()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("№", typeof(int));
            dt.Columns.Add("ID", typeof(int));
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
                    item.Id_dish,
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
                    gift.Id_present,
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
                if (dgvCart.Columns["ID"] != null)
                    dgvCart.Columns["ID"].Visible = false;

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
                        dgvCart.Columns["Сумма"].DefaultCellStyle.Font = new Font("Times New Roman", 12, FontStyle.Bold);
                    }

                    foreach (DataGridViewRow row in dgvCart.Rows)
                    {
                        if (row.Cells["Тип"].Value?.ToString() == "Подарок")
                        {
                            row.DefaultCellStyle.BackColor = Color.LightYellow;
                            row.DefaultCellStyle.ForeColor = Color.DarkOrange;
                            row.DefaultCellStyle.Font = new Font("Times New Roman", 12, FontStyle.Bold);
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

                int dishId = Convert.ToInt32(dgvCart.SelectedRows[0].Cells["ID"].Value);
                var item = cartItems.FirstOrDefault(i => i.Id_dish == dishId);

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

                int dishId = Convert.ToInt32(dgvCart.SelectedRows[0].Cells["ID"].Value);
                var item = cartItems.FirstOrDefault(i => i.Id_dish == dishId);

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

                int dishId = Convert.ToInt32(dgvCart.SelectedRows[0].Cells["ID"].Value);
                var item = cartItems.FirstOrDefault(i => i.Id_dish == dishId);
                string dishName = dgvCart.SelectedRows[0].Cells["Наименование"].Value.ToString();

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
            OrderDetailsForm form = new OrderDetailsForm(txtOrderNum.Text, cartItems, cartGifts);
            form.Show();
            this.Close();
        }
    }

    // ========== ФОРМА ДЕТАЛЕЙ ЗАКАЗА ==========
    public class OrderDetailsForm : Form
    {
        private Label lblTitle;
        private Label lblOrderNum;
        private TextBox txtOrderNum;
        private Label lblDate;
        private DateTimePicker dtpDate;
        private Label lblTime;
        private DateTimePicker dtpTime;
        private Label lblClientName;
        private TextBox txtClientName;
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

        private string orderNumber;
        private List<Menu.CartItem> cartItems;
        private List<Menu.CartGift> cartGifts;

        private Color headerBackColor = Color.FromArgb(97, 173, 123);
        private Color selectionColor = Color.FromArgb(233, 242, 236);
        private Color buttonColor = Color.DarkSeaGreen;
        private bool isFormatting = false;
        private System.Windows.Forms.Timer updateTimer;

        public OrderDetailsForm(string orderNum, List<Menu.CartItem> items, List<Menu.CartGift> gifts)
        {
            orderNumber = orderNum;
            cartItems = items;
            cartGifts = gifts;
            InitializeComponent();
            LoadOrderItems();
            CalculateTotal();
            StyleButtons();

            txtClientName.KeyPress += TxtClientName_KeyPress;
            txtClientName.TextChanged += TxtClientName_TextChanged;
            txtClientName.Leave += TxtClientName_Leave;
            btnSave.Click += BtnSave_Click;

            SetupTimeRestrictions();
            this.ControlBox = false;
            this.Text = "";
        }

        private void SetupTimeRestrictions()
        {
            DateTime now = DateTime.Now;
            DateTime minTime = now.AddHours(1);
            DateTime maxTime = now.Date.AddHours(22);

            if (minTime > maxTime)
            {
                minTime = now.Date.AddDays(1).AddHours(8);
                maxTime = minTime.Date.AddHours(22);
                dtpDate.Value = minTime.Date;
                dtpDate.MinDate = minTime.Date;
                dtpDate.MaxDate = minTime.Date.AddDays(7);
            }
            else
            {
                dtpDate.MinDate = now.Date;
                dtpDate.MaxDate = now.Date.AddDays(7);
            }

            dtpTime.Format = DateTimePickerFormat.Custom;
            dtpTime.CustomFormat = "HH:mm";
            dtpTime.ShowUpDown = true;
            dtpTime.Value = minTime;

            updateTimer = new System.Windows.Forms.Timer();
            updateTimer.Interval = 60000;
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();

            dtpDate.ValueChanged += DtpDate_ValueChanged;
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
            UpdateTimeRestrictions();
        }

        private void UpdateTimeRestrictions()
        {
            DateTime selectedDate = dtpDate.Value.Date;
            DateTime now = DateTime.Now;

            if (selectedDate.Date == now.Date)
            {
                DateTime minTime = now.AddHours(1);
                DateTime maxTime = selectedDate.AddHours(22);

                if (minTime > maxTime)
                {
                    MessageBox.Show("Сегодня уже нельзя заказать на выбранное время.\n" +
                                  "Будет установлена дата на завтра.",
                                  "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    dtpDate.Value = now.Date.AddDays(1);
                    selectedDate = dtpDate.Value.Date;
                    minTime = selectedDate.AddHours(8);
                    maxTime = selectedDate.AddHours(22);
                }

                if (dtpTime.Value < minTime || dtpTime.Value > maxTime)
                    dtpTime.Value = minTime;
            }
            else
            {
                DateTime minTime = selectedDate.AddHours(8);
                DateTime maxTime = selectedDate.AddHours(22);

                if (dtpTime.Value < minTime || dtpTime.Value > maxTime)
                    dtpTime.Value = minTime;
            }
        }

        private void InitializeComponent()
        {
            this.Text = "";
            this.Size = new Size(950, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.Font = new Font("Times New Roman", 12, FontStyle.Regular);

            this.FormBorderStyle = FormBorderStyle.None;
            this.ControlBox = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            lblTitle = new Label();
            lblTitle.Text = "ДЕТАЛИ ЗАКАЗА";
            lblTitle.Font = new Font("Times New Roman", 20, FontStyle.Bold);
            lblTitle.Size = new Size(300, 40);
            lblTitle.Location = new Point(325, 20);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblTitle);

            lblOrderNum = new Label();
            lblOrderNum.Text = "Заказ №:";
            lblOrderNum.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            lblOrderNum.Location = new Point(50, 80);
            lblOrderNum.Size = new Size(80, 25);
            this.Controls.Add(lblOrderNum);

            txtOrderNum = new TextBox();
            txtOrderNum.Text = orderNumber;
            txtOrderNum.Font = new Font("Times New Roman", 12);
            txtOrderNum.Location = new Point(140, 77);
            txtOrderNum.Size = new Size(220, 30);
            txtOrderNum.ReadOnly = true;
            txtOrderNum.BackColor = Color.LightGray;
            this.Controls.Add(txtOrderNum);

            lblDate = new Label();
            lblDate.Text = "Дата:";
            lblDate.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            lblDate.Location = new Point(400, 80);
            lblDate.Size = new Size(50, 25);
            this.Controls.Add(lblDate);

            dtpDate = new DateTimePicker();
            dtpDate.Font = new Font("Times New Roman", 12);
            dtpDate.Location = new Point(460, 77);
            dtpDate.Size = new Size(120, 30);
            dtpDate.Value = DateTime.Now;
            dtpDate.MinDate = DateTime.Now.Date;
            dtpDate.MaxDate = DateTime.Now.Date.AddDays(7);
            this.Controls.Add(dtpDate);

            lblTime = new Label();
            lblTime.Text = "Время:";
            lblTime.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            lblTime.Location = new Point(600, 80);
            lblTime.Size = new Size(60, 25);
            this.Controls.Add(lblTime);

            dtpTime = new DateTimePicker();
            dtpTime.Font = new Font("Times New Roman", 12);
            dtpTime.Location = new Point(670, 77);
            dtpTime.Size = new Size(100, 30);
            dtpTime.Format = DateTimePickerFormat.Custom;
            dtpTime.CustomFormat = "HH:mm";
            dtpTime.ShowUpDown = true;
            this.Controls.Add(dtpTime);

            lblClientName = new Label();
            lblClientName.Text = "ФИО клиента:";
            lblClientName.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            lblClientName.Location = new Point(50, 130);
            lblClientName.Size = new Size(110, 25);
            this.Controls.Add(lblClientName);

            txtClientName = new TextBox();
            txtClientName.Font = new Font("Times New Roman", 12);
            txtClientName.Location = new Point(170, 127);
            txtClientName.Size = new Size(350, 30);
            txtClientName.MaxLength = 100;
            this.Controls.Add(txtClientName);

            lblPhone = new Label();
            lblPhone.Text = "Телефон:";
            lblPhone.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            lblPhone.Location = new Point(50, 180);
            lblPhone.Size = new Size(80, 25);
            this.Controls.Add(lblPhone);

            mtxtPhone = new MaskedTextBox();
            mtxtPhone.Mask = "+7 (999) 000-00-00";
            mtxtPhone.Font = new Font("Times New Roman", 12);
            mtxtPhone.Location = new Point(140, 177);
            mtxtPhone.Size = new Size(220, 30);
            this.Controls.Add(mtxtPhone);

            lblPersons = new Label();
            lblPersons.Text = "Кол-во персон:";
            lblPersons.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            lblPersons.Location = new Point(450, 180);
            lblPersons.Size = new Size(130, 25);
            this.Controls.Add(lblPersons);

            cmbPersons = new ComboBox();
            cmbPersons.Font = new Font("Times New Roman", 12);
            cmbPersons.Location = new Point(590, 177);
            cmbPersons.Size = new Size(80, 30);
            cmbPersons.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPersons.Items.AddRange(new object[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            cmbPersons.SelectedIndex = 0;
            this.Controls.Add(cmbPersons);

            grbDelivery = new GroupBox();
            grbDelivery.Text = "Способ получения";
            grbDelivery.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            grbDelivery.Size = new Size(830, 110);
            grbDelivery.Location = new Point(50, 230);
            grbDelivery.BackColor = Color.FromArgb(240, 240, 240);
            this.Controls.Add(grbDelivery);

            rbDelivery = new RadioButton();
            rbDelivery.Text = "Доставка";
            rbDelivery.Font = new Font("Times New Roman", 12);
            rbDelivery.Location = new Point(30, 35);
            rbDelivery.Size = new Size(100, 30);
            rbDelivery.Checked = true;
            rbDelivery.CheckedChanged += RbDelivery_CheckedChanged;
            grbDelivery.Controls.Add(rbDelivery);

            rbPickup = new RadioButton();
            rbPickup.Text = "Самовывоз";
            rbPickup.Font = new Font("Times New Roman", 12);
            rbPickup.Location = new Point(160, 35);
            rbPickup.Size = new Size(110, 30);
            grbDelivery.Controls.Add(rbPickup);

            lblAddress = new Label();
            lblAddress.Text = "Адрес:";
            lblAddress.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            lblAddress.Location = new Point(30, 70);
            lblAddress.Size = new Size(70, 25);
            grbDelivery.Controls.Add(lblAddress);

            txtAddress = new TextBox();
            txtAddress.Font = new Font("Times New Roman", 12);
            txtAddress.Location = new Point(110, 67);
            txtAddress.Size = new Size(600, 30);
            grbDelivery.Controls.Add(txtAddress);

            lblPayment = new Label();
            lblPayment.Text = "Вид оплаты:";
            lblPayment.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            lblPayment.Location = new Point(50, 360);
            lblPayment.Size = new Size(110, 25);
            this.Controls.Add(lblPayment);

            cmbPayment = new ComboBox();
            cmbPayment.Font = new Font("Times New Roman", 12);
            cmbPayment.Location = new Point(170, 357);
            cmbPayment.Size = new Size(220, 30);
            cmbPayment.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPayment.Items.AddRange(new string[] { "Наличные", "Карта", "Перевод" });
            cmbPayment.SelectedIndex = 0;
            this.Controls.Add(cmbPayment);

            lblComment = new Label();
            lblComment.Text = "Комментарий:";
            lblComment.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            lblComment.Location = new Point(50, 410);
            lblComment.Size = new Size(110, 25);
            this.Controls.Add(lblComment);

            txtComment = new TextBox();
            txtComment.Font = new Font("Times New Roman", 12);
            txtComment.Location = new Point(170, 407);
            txtComment.Size = new Size(600, 30);
            this.Controls.Add(txtComment);

            lstOrderItems = new ListBox();
            lstOrderItems.Font = new Font("Times New Roman", 12);
            lstOrderItems.Location = new Point(50, 460);
            lstOrderItems.Size = new Size(720, 120);
            lstOrderItems.BackColor = Color.FromArgb(240, 240, 240);
            lstOrderItems.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(lstOrderItems);

            lblTotal = new Label();
            lblTotal.Text = "ИТОГО:";
            lblTotal.Font = new Font("Times New Roman", 18, FontStyle.Bold);
            lblTotal.Location = new Point(550, 600);
            lblTotal.Size = new Size(100, 35);
            this.Controls.Add(lblTotal);

            lblTotalValue = new Label();
            lblTotalValue.Font = new Font("Times New Roman", 18, FontStyle.Bold);
            lblTotalValue.Location = new Point(660, 600);
            lblTotalValue.Size = new Size(180, 35);
            lblTotalValue.ForeColor = Color.Red;
            this.Controls.Add(lblTotalValue);

            btnBack = new Button();
            btnBack.Text = "Назад";
            btnBack.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            btnBack.Size = new Size(120, 50);
            btnBack.Location = new Point(200, 660);
            this.Controls.Add(btnBack);

            btnSave = new Button();
            btnSave.Text = "Оформить заказ";
            btnSave.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            btnSave.Size = new Size(180, 50);
            btnSave.Location = new Point(500, 660);
            this.Controls.Add(btnSave);

            btnBack.Click += (s, e) => {
                this.Close();
                new OrderCompositionForm(cartItems, cartGifts).Show();
            };
        }

        private void TxtClientName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;

            if (e.KeyChar == ' ')
            {
                TextBox textBox = sender as TextBox;
                if (textBox != null)
                {
                    int currentSpaces = textBox.Text.Count(c => c == ' ');
                    if (currentSpaces >= 2)
                    {
                        e.Handled = true;
                        System.Media.SystemSounds.Beep.Play();
                        MessageBox.Show("ФИО должно содержать не более 2 пробелов!\n" +
                                      "Формат: Фамилия Имя Отчество",
                                      "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                return;
            }

            bool isRussian = (e.KeyChar >= 'А' && e.KeyChar <= 'Я') ||
                             (e.KeyChar >= 'а' && e.KeyChar <= 'я') ||
                             e.KeyChar == 'Ё' || e.KeyChar == 'ё';

            if (!isRussian)
            {
                e.Handled = true;
                System.Media.SystemSounds.Beep.Play();
            }
        }

        private void TxtClientName_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null || isFormatting) return;

            isFormatting = true;

            string text = textBox.Text;
            int cursorPos = textBox.SelectionStart;

            string[] words = text.Split(' ');

            for (int i = 0; i < words.Length; i++)
            {
                if (!string.IsNullOrEmpty(words[i]))
                {
                    bool isValid = true;
                    foreach (char c in words[i])
                    {
                        if (!((c >= 'А' && c <= 'Я') || (c >= 'а' && c <= 'я') || c == 'Ё' || c == 'ё'))
                        {
                            isValid = false;
                            break;
                        }
                    }

                    if (isValid && words[i].Length > 0)
                    {
                        char firstChar = words[i][0];
                        string rest = words[i].Length > 1 ? words[i].Substring(1).ToLower() : "";

                        if ((firstChar >= 'а' && firstChar <= 'я') || (firstChar >= 'А' && firstChar <= 'Я') ||
                            firstChar == 'ё' || firstChar == 'Ё')
                        {
                            words[i] = char.ToUpper(firstChar) + rest;
                        }
                    }
                }
            }

            string newText = string.Join(" ", words);
            string[] parts = newText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 3)
                newText = string.Join(" ", parts.Take(3));

            if (text != newText)
            {
                textBox.Text = newText;
                textBox.SelectionStart = Math.Min(cursorPos, newText.Length);
            }

            isFormatting = false;
        }

        private void TxtClientName_Leave(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;

            string text = textBox.Text.Trim();
            while (text.Contains("  "))
                text = text.Replace("  ", " ");

            string[] words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > 3)
                words = words.Take(3).ToArray();

            for (int i = 0; i < words.Length; i++)
            {
                if (!string.IsNullOrEmpty(words[i]))
                {
                    string cleanWord = "";
                    foreach (char c in words[i])
                    {
                        if ((c >= 'А' && c <= 'Я') || (c >= 'а' && c <= 'я') || c == 'Ё' || c == 'ё')
                            cleanWord += c;
                    }

                    if (cleanWord.Length > 0)
                    {
                        char firstChar = cleanWord[0];
                        string rest = cleanWord.Length > 1 ? cleanWord.Substring(1).ToLower() : "";
                        words[i] = char.ToUpper(firstChar) + rest;
                    }
                }
            }

            textBox.Text = string.Join(" ", words.Where(w => !string.IsNullOrEmpty(w)));

            if (words.Length < 3)
            {
                MessageBox.Show("Рекомендуется вводить ФИО полностью:\nФамилия Имя Отчество",
                              "Подсказка", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void StyleButtons()
        {
            StyleButton(btnSave);
            StyleButton(btnBack);
        }

        private void StyleButton(Button btn)
        {
            if (btn == null) return;

            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 2;
            btn.FlatAppearance.BorderColor = Color.Black;
            btn.BackColor = buttonColor;
            btn.ForeColor = Color.Black;
            btn.FlatAppearance.MouseOverBackColor = buttonColor;
            btn.FlatAppearance.MouseDownBackColor = buttonColor;

            btn.MouseDown += (s, e) => btn.FlatAppearance.BorderColor = Color.DarkBlue;
            btn.MouseUp += (s, e) => btn.FlatAppearance.BorderColor = Color.Black;
            btn.MouseLeave += (s, e) => btn.FlatAppearance.BorderColor = Color.Black;
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
                lstOrderItems.Items.Add($"{item.Name} x{item.Quantity} = {item.Price * item.Quantity:N2} ₽");

            foreach (var gift in cartGifts)
                lstOrderItems.Items.Add($"🎁 {gift.Name} - ПОДАРОК (бесплатно)");
        }

        private void CalculateTotal()
        {
            decimal total = cartItems.Sum(item => item.Price * item.Quantity);
            lblTotalValue.Text = total.ToString("N2") + " ₽";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            DateTime selectedDateTime = dtpDate.Value.Date.Add(dtpTime.Value.TimeOfDay);
            DateTime minAllowedTime = DateTime.Now.AddHours(1);
            DateTime maxAllowedTime = dtpDate.Value.Date.AddHours(22);

            if (selectedDateTime < minAllowedTime)
            {
                MessageBox.Show($"Время доставки должно быть минимум через час от текущего времени!\n" +
                              $"Текущее время: {DateTime.Now:HH:mm}\n" +
                              $"Минимальное время: {minAllowedTime:HH:mm}",
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedDateTime > maxAllowedTime)
            {
                MessageBox.Show($"Время доставки не может быть позже 22:00!\n" +
                              $"Максимальное время: 22:00",
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtClientName.Text))
            {
                MessageBox.Show("Введите ФИО клиента!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtClientName.Focus();
                return;
            }

            int spaceCount = txtClientName.Text.Count(c => c == ' ');
            if (spaceCount > 2)
            {
                MessageBox.Show("ФИО должно содержать не более 2 пробелов!\n" +
                              "Формат: Фамилия Имя Отчество",
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtClientName.Focus();
                return;
            }

            string[] words = txtClientName.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length < 3)
            {
                MessageBox.Show("Введите полное ФИО!\nФормат: Фамилия Имя Отчество",
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtClientName.Focus();
                return;
            }

            foreach (char c in txtClientName.Text)
            {
                if (c != ' ' && !((c >= 'А' && c <= 'Я') || (c >= 'а' && c <= 'я') || c == 'Ё' || c == 'ё'))
                {
                    MessageBox.Show("ФИО может содержать только русские буквы и пробелы!",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtClientName.Focus();
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

            try
            {
                SaveToDatabase();
                CreateWordReceipt();
                MessageBox.Show($"Заказ {orderNumber} оформлен!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Close();
                new Menu().Show();
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                string newNumber = $"ORD-{DateTime.Now:yyyyMMdd-HHmmss}-{new Random().Next(10000, 99999)}";
                DialogResult result = MessageBox.Show(
                    $"Заказ с номером {orderNumber} уже существует.\n\nСоздать заказ с новым номером {newNumber}?",
                    "Ошибка уникальности",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    orderNumber = newNumber;
                    txtOrderNum.Text = orderNumber;
                    SaveToDatabase();
                    CreateWordReceipt();
                    MessageBox.Show($"Заказ {orderNumber} оформлен!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                    new Menu().Show();
                }
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

                string orderQuery = @"INSERT INTO orders 
                    (order_number, name_client, phone_number, address, number_persons, 
                     delivery_date, delivery_time, comment, payment_method, id_status, total_amount) 
                    VALUES 
                    (@num, @name, @phone, @addr, @pers, @date, @time, @comm, @pay, 1, @total);
                    SELECT LAST_INSERT_ID();";

                long orderId;

                using (MySqlCommand cmd = new MySqlCommand(orderQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@num", orderNumber);
                    cmd.Parameters.AddWithValue("@name", txtClientName.Text.Trim());
                    cmd.Parameters.AddWithValue("@phone", mtxtPhone.Text);
                    cmd.Parameters.AddWithValue("@addr", rbDelivery.Checked ? txtAddress.Text : "Самовывоз");
                    cmd.Parameters.AddWithValue("@pers", Convert.ToInt32(cmbPersons.SelectedItem));
                    cmd.Parameters.AddWithValue("@date", dtpDate.Value.Date);
                    cmd.Parameters.AddWithValue("@time", dtpTime.Value.TimeOfDay);
                    cmd.Parameters.AddWithValue("@comm", txtComment.Text);
                    cmd.Parameters.AddWithValue("@pay", cmbPayment.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@total", cartItems.Sum(i => i.Price * i.Quantity));

                    orderId = Convert.ToInt64(cmd.ExecuteScalar());
                }

                // Сохраняем блюда
                foreach (var item in cartItems)
                {
                    string dishQuery = @"INSERT INTO order_dish 
                        (id_order, id_dish, quantity, price_at_order, is_gift, id_present) 
                        VALUES (@oid, @did, @qty, @price, FALSE, NULL)";

                    using (MySqlCommand dishCmd = new MySqlCommand(dishQuery, conn))
                    {
                        dishCmd.Parameters.AddWithValue("@oid", orderId);
                        dishCmd.Parameters.AddWithValue("@did", item.Id_dish);
                        dishCmd.Parameters.AddWithValue("@qty", item.Quantity);
                        dishCmd.Parameters.AddWithValue("@price", item.Price);
                        dishCmd.ExecuteNonQuery();
                    }
                }

                // Сохраняем подарки
                if (cartGifts != null && cartGifts.Count > 0)
                {
                    foreach (var gift in cartGifts)
                    {
                        // Для подарков используем id_dish = 1 (любое существующее блюдо)
                        string giftQuery = @"INSERT INTO order_dish 
                            (id_order, id_dish, quantity, price_at_order, is_gift, id_present) 
                            VALUES (@oid, 1, @qty, 0, TRUE, @pid)";

                        using (MySqlCommand giftCmd = new MySqlCommand(giftQuery, conn))
                        {
                            giftCmd.Parameters.AddWithValue("@oid", orderId);
                            giftCmd.Parameters.AddWithValue("@qty", gift.Quantity);
                            giftCmd.Parameters.AddWithValue("@pid", gift.Id_present);
                            giftCmd.ExecuteNonQuery();
                        }
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
                saveFileDialog.FileName = $"Чек_{orderNumber}";

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
                range.Text = $"Заказ: {orderNumber}\n";
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
                range.Text = $"ИТОГО: {cartItems.Sum(i => i.Price * i.Quantity):N2} ₽\n";
                range.InsertParagraphAfter();
                range.Collapse(Microsoft.Office.Interop.Word.WdCollapseDirection.wdCollapseEnd);

                range.Font.Bold = 0;
                range.Font.Size = 12;
                range.Text = "---------------------------------\n";
                range.InsertParagraphAfter();
                range.Collapse(Microsoft.Office.Interop.Word.WdCollapseDirection.wdCollapseEnd);

                range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphLeft;
                range.Text = $"Клиент: {txtClientName.Text}\n";
                range.InsertParagraphAfter();
                range.Collapse(Microsoft.Office.Interop.Word.WdCollapseDirection.wdCollapseEnd);

                range.Text = $"Телефон: {mtxtPhone.Text}\n";
                range.InsertParagraphAfter();
                range.Collapse(Microsoft.Office.Interop.Word.WdCollapseDirection.wdCollapseEnd);

                string address = rbDelivery.Checked ? txtAddress.Text : "Самовывоз";
                range.Text = $"Адрес: {address}\n";
                range.InsertParagraphAfter();
                range.Collapse(Microsoft.Office.Interop.Word.WdCollapseDirection.wdCollapseEnd);

                range.Text = $"Оплата: {cmbPayment.SelectedItem}\n";
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
                sw.WriteLine($"Заказ: {orderNumber}");
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
                sw.WriteLine($"ИТОГО: {cartItems.Sum(i => i.Price * i.Quantity):N2} ₽".PadLeft(60));
                sw.WriteLine("---------------------------------");
                sw.WriteLine($"Клиент: {txtClientName.Text}");
                sw.WriteLine($"Телефон: {mtxtPhone.Text}");
                sw.WriteLine($"Адрес: {(rbDelivery.Checked ? txtAddress.Text : "Самовывоз")}");
                sw.WriteLine($"Оплата: {cmbPayment.SelectedItem}");
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