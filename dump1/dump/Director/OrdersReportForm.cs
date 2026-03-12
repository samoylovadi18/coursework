using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using Excel = Microsoft.Office.Interop.Excel;

namespace dump
{
    public partial class OrdersReportForm : Form
    {
        private DataTable ordersData;
        private DateTime minDate = new DateTime(2024, 1, 1);
        private DateTime maxDate = new DateTime(2040, 12, 31);
        private CultureInfo russianCulture = new CultureInfo("ru-RU");

        private System.Windows.Forms.ToolTip toolTip1;

        // Класс для хранения информации о блюде/подарке в деталях заказа
        private class OrderDetailItem
        {
            public string Name { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public decimal TotalPrice { get; set; }
            public bool IsGift { get; set; }
            public string DisplayName => IsGift ? $"🎁 {Name} (Подарок)" : Name;
        }

        public OrdersReportForm()
        {
            InitializeComponent();
            ordersData = new DataTable();
            toolTip1 = new System.Windows.Forms.ToolTip();

            // Настройка кнопки Generate
            btnGenerate.FlatStyle = FlatStyle.Flat;
            btnGenerate.FlatAppearance.BorderSize = 1;
            btnGenerate.FlatAppearance.BorderColor = Color.Black;
            btnGenerate.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            btnGenerate.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            btnGenerate.MouseDown += (s, e) =>
            {
                btnGenerate.FlatAppearance.BorderColor = Color.DarkBlue;
            };
            btnGenerate.MouseUp += (s, e) =>
            {
                btnGenerate.FlatAppearance.BorderColor = Color.Black;
            };
            btnGenerate.MouseLeave += (s, e) =>
            {
                btnGenerate.FlatAppearance.BorderColor = Color.Black;
            };

            // Настройка кнопки Export
            btnExport.FlatStyle = FlatStyle.Flat;
            btnExport.FlatAppearance.BorderSize = 1;
            btnExport.FlatAppearance.BorderColor = Color.Black;
            btnExport.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            btnExport.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            btnExport.MouseDown += (s, e) =>
            {
                btnExport.FlatAppearance.BorderColor = Color.DarkBlue;
            };
            btnExport.MouseUp += (s, e) =>
            {
                btnExport.FlatAppearance.BorderColor = Color.Black;
            };
            btnExport.MouseLeave += (s, e) =>
            {
                btnExport.FlatAppearance.BorderColor = Color.Black;
            };

            // Настройка кнопки Detail
            buttonDetail.Click += ButtonDetail_Click;
            buttonDetail.FlatStyle = FlatStyle.Flat;
            buttonDetail.FlatAppearance.BorderSize = 1;
            buttonDetail.FlatAppearance.BorderColor = Color.Black;
            buttonDetail.BackColor = Color.DarkSeaGreen;
            buttonDetail.ForeColor = Color.Black;
            buttonDetail.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            buttonDetail.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            buttonDetail.MouseDown += (s, e) =>
            {
                buttonDetail.FlatAppearance.BorderColor = Color.DarkBlue;
            };
            buttonDetail.MouseUp += (s, e) =>
            {
                buttonDetail.FlatAppearance.BorderColor = Color.Black;
            };
            buttonDetail.MouseLeave += (s, e) =>
            {
                buttonDetail.FlatAppearance.BorderColor = Color.Black;
            };

            // Добавляем обработчик двойного клика
            dgvOrders.CellDoubleClick += DgvOrders_CellDoubleClick;
        }

        private void RevenueForm_Load(object sender, EventArgs e)
        {
            // Устанавливаем ограничения для DatePicker
            dtpStartDate.MinDate = minDate;
            dtpStartDate.MaxDate = DateTime.Now > maxDate ? maxDate : DateTime.Now;
            dtpEndDate.MinDate = minDate;
            dtpEndDate.MaxDate = DateTime.Now > maxDate ? maxDate : DateTime.Now;

            // Устанавливаем значения по умолчанию (текущий месяц)
            dtpEndDate.Value = DateTime.Now;
            dtpStartDate.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            // Настраиваем подписи
            labelStartDate.Text = "Период с:";
            labelEndDate.Text = "по:";

            // Подписываемся на события
            btnGenerate.Click += BtnGenerate_Click;
            btnExport.Click += BtnExport_Click;

            // Настройка DataGridView
            SetupDataGridView();

            // Загружаем заказы
            LoadOrders();
        }

        private void SetupDataGridView()
        {
            dgvOrders.ReadOnly = true;
            dgvOrders.AllowUserToAddRows = false;
            dgvOrders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvOrders.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvOrders.MultiSelect = false;
            dgvOrders.RowHeadersVisible = false;
            dgvOrders.EnableHeadersVisualStyles = false;

            // Цвет шапки как в OrdersForm (зеленый)
            Color headerBackColor = Color.FromArgb(97, 173, 123);

            // Цвет выделения как в OrdersForm (светло-зеленый)
            Color selectionColor = Color.FromArgb(233, 242, 236);

            // Настройка шапки - КАК В ORDERSFORM
            dgvOrders.ColumnHeadersDefaultCellStyle.BackColor = headerBackColor;
            dgvOrders.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvOrders.ColumnHeadersDefaultCellStyle.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            dgvOrders.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvOrders.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvOrders.ColumnHeadersDefaultCellStyle.Padding = new Padding(0, 3, 0, 3);
            dgvOrders.ColumnHeadersHeight = 45;
            dgvOrders.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            // Настройка строк - КАК В ORDERSFORM
            dgvOrders.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            dgvOrders.DefaultCellStyle.Padding = new Padding(5);
            dgvOrders.DefaultCellStyle.BackColor = Color.White;
            dgvOrders.DefaultCellStyle.ForeColor = Color.Black;
            dgvOrders.DefaultCellStyle.SelectionBackColor = selectionColor;
            dgvOrders.DefaultCellStyle.SelectionForeColor = Color.Black;

            dgvOrders.RowsDefaultCellStyle.BackColor = Color.White;
            dgvOrders.RowsDefaultCellStyle.ForeColor = Color.Black;
            dgvOrders.RowsDefaultCellStyle.SelectionBackColor = selectionColor;
            dgvOrders.RowsDefaultCellStyle.SelectionForeColor = Color.Black;

            dgvOrders.RowTemplate.Height = 35;
            dgvOrders.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvOrders.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            // Настройка сетки
            dgvOrders.GridColor = Color.Gray;
            dgvOrders.BorderStyle = BorderStyle.Fixed3D;
            dgvOrders.CellBorderStyle = DataGridViewCellBorderStyle.Single;

            // Добавляем подсказку
            toolTip1.SetToolTip(dgvOrders, "Двойной клик для просмотра деталей заказа");

            // Создаем колонки
            dgvOrders.Columns.Clear();

            // ID заказа (скрытая)
            DataGridViewTextBoxColumn colId = new DataGridViewTextBoxColumn();
            colId.Name = "id_order";
            colId.DataPropertyName = "id_order";
            colId.Visible = false;
            colId.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colId);

            // Дата и время
            DataGridViewTextBoxColumn colDateTime = new DataGridViewTextBoxColumn();
            colDateTime.Name = "date_time";
            colDateTime.HeaderText = "Дата и время";
            colDateTime.DataPropertyName = "date_time";
            colDateTime.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colDateTime.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colDateTime);

            // Номер заказа
            DataGridViewTextBoxColumn colOrderNumber = new DataGridViewTextBoxColumn();
            colOrderNumber.Name = "order_number";
            colOrderNumber.HeaderText = "№ заказа";
            colOrderNumber.DataPropertyName = "order_number";
            colOrderNumber.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colOrderNumber.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colOrderNumber);

            // Клиент
            DataGridViewTextBoxColumn colClient = new DataGridViewTextBoxColumn();
            colClient.Name = "client";
            colClient.HeaderText = "Клиент";
            colClient.DataPropertyName = "client";
            colClient.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colClient.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colClient);

            // Телефон
            DataGridViewTextBoxColumn colPhone = new DataGridViewTextBoxColumn();
            colPhone.Name = "phone";
            colPhone.HeaderText = "Телефон";
            colPhone.DataPropertyName = "phone";
            colPhone.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colPhone.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colPhone);

            // Адрес
            DataGridViewTextBoxColumn colAddress = new DataGridViewTextBoxColumn();
            colAddress.Name = "address";
            colAddress.HeaderText = "Адрес";
            colAddress.DataPropertyName = "address";
            colAddress.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colAddress.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colAddress);

            // Сумма
            DataGridViewTextBoxColumn colAmount = new DataGridViewTextBoxColumn();
            colAmount.Name = "total_amount";
            colAmount.HeaderText = "Сумма";
            colAmount.DataPropertyName = "total_amount";
            colAmount.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colAmount.DefaultCellStyle.Format = "N2";
            colAmount.DefaultCellStyle.ForeColor = Color.DarkGreen;
            colAmount.DefaultCellStyle.SelectionForeColor = Color.DarkGreen;
            colAmount.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colAmount);

            // Кол-во блюд
            DataGridViewTextBoxColumn colDishes = new DataGridViewTextBoxColumn();
            colDishes.Name = "dishes_count";
            colDishes.HeaderText = "Кол-во блюд";
            colDishes.DataPropertyName = "dishes_count";
            colDishes.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colDishes.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colDishes);

            // Статус
            DataGridViewTextBoxColumn colStatus = new DataGridViewTextBoxColumn();
            colStatus.Name = "status";
            colStatus.HeaderText = "Статус";
            colStatus.DataPropertyName = "status";
            colStatus.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colStatus.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colStatus);
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                if (dtpStartDate.Value.Date > dtpEndDate.Value.Date)
                {
                    MessageBox.Show("Дата 'С' не может быть позже даты 'По'!",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DateTime startDate = dtpStartDate.Value.Date;
                DateTime endDate = dtpEndDate.Value.Date;

                ordersData = LoadOrdersFromDB(startDate, endDate);
                dgvOrders.DataSource = ordersData;
                UpdateStatistics(startDate, endDate);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке заказов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DataTable LoadOrdersFromDB(DateTime startDate, DateTime endDate)
        {
            DataTable dt = new DataTable();

            string query = @"
                SELECT 
                    o.id_order,
                    DATE_FORMAT(o.created_at, '%d.%m.%Y %H:%i') as date_time,
                    o.order_number,
                    o.name_client as client,
                    o.phone_number as phone,
                    o.address,
                    COALESCE(SUM(od.quantity * od.price_at_order), 0) as total_amount,
                    COALESCE(COUNT(od.id_order_dish), 0) as dishes_count,
                    os.status_name as status
                FROM orders o
                LEFT JOIN order_dish od ON o.id_order = od.id_order
                LEFT JOIN order_statuses os ON o.id_status = os.id_status
                WHERE DATE(o.created_at) BETWEEN @startDate AND @endDate
                GROUP BY o.id_order
                ORDER BY o.created_at DESC";

            using (var connection = SettingsBD.GetConnection())
            {
                connection.Open();
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@startDate", startDate);
                    cmd.Parameters.AddWithValue("@endDate", endDate);

                    using (var adapter = new MySqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }

            return dt;
        }

        private void UpdateStatistics(DateTime startDate, DateTime endDate)
        {
            try
            {
                decimal totalRevenue = 0;
                int totalOrders = ordersData.Rows.Count;
                int totalDishes = 0;

                foreach (DataRow row in ordersData.Rows)
                {
                    totalRevenue += Convert.ToDecimal(row["total_amount"]);
                    totalDishes += Convert.ToInt32(row["dishes_count"]);
                }

            }
            catch
            {

            }
        }

        private void ButtonDetail_Click(object sender, EventArgs e)
        {
            ShowOrderDetails();
        }

        private void DgvOrders_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            ShowOrderDetails();
        }

        // ===== ИСПРАВЛЕННЫЙ МЕТОД: Загрузка деталей заказа с подарками =====
        private List<OrderDetailItem> LoadOrderDetails(int orderId)
        {
            List<OrderDetailItem> items = new List<OrderDetailItem>();

            try
            {
                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();

                    string query = @"
                        SELECT 
                            CASE 
                                WHEN od.is_gift = TRUE THEN p.name
                                ELSE d.dish_name
                            END as item_name,
                            od.quantity,
                            CASE 
                                WHEN od.is_gift = TRUE THEN 0
                                ELSE d.price
                            END as price,
                            od.price_at_order as total_price,
                            od.is_gift
                        FROM order_dish od
                        LEFT JOIN dishes d ON od.id_dish = d.id_dish
                        LEFT JOIN present p ON od.id_present = p.id_present
                        WHERE od.id_order = @orderId
                        ORDER BY od.is_gift, item_name";

                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@orderId", orderId);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                items.Add(new OrderDetailItem
                                {
                                    Name = reader["item_name"].ToString(),
                                    Quantity = Convert.ToInt32(reader["quantity"]),
                                    Price = Convert.ToDecimal(reader["price"]),
                                    TotalPrice = Convert.ToDecimal(reader["total_price"]),
                                    IsGift = Convert.ToBoolean(reader["is_gift"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке деталей заказа: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return items;
        }

        // ===== НОВЫЙ МЕТОД: Создание DataTable для отображения деталей =====
        private DataTable CreateOrderDetailsDataTable(List<OrderDetailItem> items)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("dish_name", typeof(string));
            dt.Columns.Add("quantity", typeof(int));
            dt.Columns.Add("price", typeof(decimal));
            dt.Columns.Add("total_price", typeof(decimal));
            dt.Columns.Add("is_gift", typeof(bool));

            foreach (var item in items)
            {
                DataRow row = dt.NewRow();
                row["dish_name"] = item.DisplayName;
                row["quantity"] = item.Quantity;
                row["price"] = item.Price;
                row["total_price"] = item.TotalPrice;
                row["is_gift"] = item.IsGift;
                dt.Rows.Add(row);
            }

            return dt;
        }

        // ===== ИСПРАВЛЕННЫЙ МЕТОД: Показ деталей заказа =====
        private void ShowOrderDetails()
        {
            try
            {
                if (dgvOrders.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Выберите заказ для просмотра деталей!", "Предупреждение",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DataGridViewRow selectedRow = dgvOrders.SelectedRows[0];
                int orderId = Convert.ToInt32(selectedRow.Cells["id_order"].Value);
                string clientName = selectedRow.Cells["client"].Value?.ToString() ?? "";
                string orderDate = selectedRow.Cells["date_time"].Value?.ToString() ?? "";

                // Создаем форму для деталей заказа
                Form detailForm = new Form();
                detailForm.Text = $"Детали заказа №{orderId}";
                detailForm.Size = new Size(900, 680);
                detailForm.StartPosition = FormStartPosition.CenterParent;
                detailForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                detailForm.MaximizeBox = false;
                detailForm.MinimizeBox = false;
                detailForm.BackColor = Color.White;
                detailForm.Font = new Font("Times New Roman", 12, FontStyle.Regular);

                // Панель информации
                Panel infoPanel = new Panel();
                infoPanel.Location = new Point(10, 10);
                infoPanel.Size = new Size(865, 100);
                infoPanel.BorderStyle = BorderStyle.FixedSingle;
                infoPanel.BackColor = Color.FromArgb(240, 240, 240);

                Label lblOrderInfo = new Label();
                lblOrderInfo.Text = $"Заказ №{orderId}\nКлиент: {clientName}\nДата: {orderDate}";
                lblOrderInfo.Location = new Point(10, 10);
                lblOrderInfo.Size = new Size(845, 80);
                lblOrderInfo.Font = new Font("Times New Roman", 12, FontStyle.Bold);
                lblOrderInfo.TextAlign = ContentAlignment.MiddleLeft;
                infoPanel.Controls.Add(lblOrderInfo);

                // DataGridView для блюд и подарков - СТИЛЬ КАК В OrdersForm
                DataGridView dgvOrderDetails = CreateOrderDetailsDataGridView();
                dgvOrderDetails.Location = new Point(10, 120);
                dgvOrderDetails.Size = new Size(865, 400);

                // Загружаем детали заказа (блюда + подарки)
                List<OrderDetailItem> orderDetails = LoadOrderDetails(orderId);
                DataTable dt = CreateOrderDetailsDataTable(orderDetails);
                dgvOrderDetails.DataSource = dt;

                // Панель итога
                Panel totalPanel = new Panel();
                totalPanel.Location = new Point(10, 530);
                totalPanel.Size = new Size(865, 40);
                totalPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                totalPanel.BorderStyle = BorderStyle.FixedSingle;
                totalPanel.BackColor = Color.FromArgb(240, 240, 240);

                Label lblTotal = new Label();
                decimal totalAmount = selectedRow.Cells["total_amount"].Value != null ?
                    Convert.ToDecimal(selectedRow.Cells["total_amount"].Value) : 0;
                lblTotal.Text = $"ИТОГО: {totalAmount:N2} ₽";
                lblTotal.Location = new Point(10, 10);
                lblTotal.Size = new Size(845, 20);
                lblTotal.Font = new Font("Times New Roman", 14, FontStyle.Bold);
                lblTotal.ForeColor = Color.DarkRed;
                lblTotal.TextAlign = ContentAlignment.MiddleRight;
                totalPanel.Controls.Add(lblTotal);

                // Добавляем элементы
                detailForm.Controls.Add(infoPanel);
                detailForm.Controls.Add(dgvOrderDetails);
                detailForm.Controls.Add(totalPanel);

                detailForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке деталей заказа: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===== НОВЫЙ МЕТОД: Создание DataGridView для деталей с поддержкой подарков =====
        private DataGridView CreateOrderDetailsDataGridView()
        {
            DataGridView dgv = new DataGridView();
            dgv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgv.AutoGenerateColumns = false;
            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;
            dgv.RowHeadersVisible = false;
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.Fixed3D;
            dgv.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            dgv.EnableHeadersVisualStyles = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;

            // Цвет шапки как в OrdersForm (зеленый)
            Color headerBackColor = Color.FromArgb(97, 173, 123);
            Color selectionColor = Color.FromArgb(233, 242, 236);

            // Настройка стилей шапки
            dgv.ColumnHeadersDefaultCellStyle.BackColor = headerBackColor;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(0, 3, 0, 3);
            dgv.ColumnHeadersHeight = 45;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            // Настройка строк
            dgv.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            dgv.DefaultCellStyle.Padding = new Padding(5);
            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.ForeColor = Color.Black;
            dgv.DefaultCellStyle.SelectionBackColor = selectionColor;
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;

            dgv.RowsDefaultCellStyle.BackColor = Color.White;
            dgv.RowsDefaultCellStyle.ForeColor = Color.Black;
            dgv.RowsDefaultCellStyle.SelectionBackColor = selectionColor;
            dgv.RowsDefaultCellStyle.SelectionForeColor = Color.Black;

            dgv.RowTemplate.Height = 35;
            dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            // Настройка сетки
            dgv.GridColor = Color.Gray;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.Single;

            // Колонка Название блюда
            DataGridViewTextBoxColumn colDishName = new DataGridViewTextBoxColumn();
            colDishName.Name = "dish_name";
            colDishName.HeaderText = "Наименование";
            colDishName.DataPropertyName = "dish_name";
            colDishName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colDishName.ReadOnly = true;
            colDishName.FillWeight = 50;
            colDishName.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgv.Columns.Add(colDishName);

            // Колонка Количество
            DataGridViewTextBoxColumn colQuantity = new DataGridViewTextBoxColumn();
            colQuantity.Name = "quantity";
            colQuantity.HeaderText = "Кол-во";
            colQuantity.DataPropertyName = "quantity";
            colQuantity.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colQuantity.ReadOnly = true;
            colQuantity.FillWeight = 15;
            colQuantity.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgv.Columns.Add(colQuantity);

            // Колонка Цена
            DataGridViewTextBoxColumn colPrice = new DataGridViewTextBoxColumn();
            colPrice.Name = "price";
            colPrice.HeaderText = "Цена";
            colPrice.DataPropertyName = "price";
            colPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colPrice.ReadOnly = true;
            colPrice.FillWeight = 15;
            colPrice.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgv.Columns.Add(colPrice);

            // Колонка Сумма
            DataGridViewTextBoxColumn colTotal = new DataGridViewTextBoxColumn();
            colTotal.Name = "total_price";
            colTotal.HeaderText = "Сумма";
            colTotal.DataPropertyName = "total_price";
            colTotal.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colTotal.ReadOnly = true;
            colTotal.FillWeight = 20;
            colTotal.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgv.Columns.Add(colTotal);

            // Скрытая колонка для флага подарка
            DataGridViewCheckBoxColumn colIsGift = new DataGridViewCheckBoxColumn();
            colIsGift.Name = "is_gift";
            colIsGift.DataPropertyName = "is_gift";
            colIsGift.Visible = false;
            dgv.Columns.Add(colIsGift);

            // Форматирование ячеек
            dgv.CellFormatting += (s, e) =>
            {
                // Форматирование цены
                if (e.ColumnIndex == dgv.Columns["price"].Index && e.RowIndex >= 0 && e.Value != null)
                {
                    if (e.Value is decimal || e.Value is int || e.Value is double)
                    {
                        decimal price = Convert.ToDecimal(e.Value);
                        e.Value = price.ToString("N2", russianCulture) + " ₽";
                        e.FormattingApplied = true;
                    }
                }
                // Форматирование суммы
                else if (e.ColumnIndex == dgv.Columns["total_price"].Index && e.RowIndex >= 0 && e.Value != null)
                {
                    if (e.Value is decimal || e.Value is int || e.Value is double)
                    {
                        decimal total = Convert.ToDecimal(e.Value);
                        e.Value = total.ToString("N2", russianCulture) + " ₽";
                        e.FormattingApplied = true;
                    }
                }
            };

            // Подсветка строк с подарками после загрузки данных
            dgv.DataBindingComplete += (s, e) =>
            {
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (row.Cells["is_gift"].Value != null && (bool)row.Cells["is_gift"].Value)
                    {
                        row.DefaultCellStyle.BackColor = Color.LightYellow;
                        row.DefaultCellStyle.ForeColor = Color.DarkOrange;
                        row.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Bold);

                        // Также меняем цвет для каждой ячейки отдельно
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            cell.Style.BackColor = Color.LightYellow;
                            cell.Style.ForeColor = Color.DarkOrange;
                            cell.Style.Font = new Font("Times New Roman", 10, FontStyle.Bold);
                        }
                    }
                }
            };

            return dgv;
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                if (ordersData == null || ordersData.Rows.Count == 0)
                {
                    MessageBox.Show("Нет данных для экспорта!", "Предупреждение",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Excel файлы (*.xlsx)|*.xlsx|CSV файлы (*.csv)|*.csv";
                saveDialog.FileName = $"Заказы_за_период_{dtpStartDate.Value:yyyyMMdd}-{dtpEndDate.Value:yyyyMMdd}";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    if (saveDialog.FileName.EndsWith(".csv"))
                        ExportToCSV(saveDialog.FileName);
                    else
                        ExportToExcel(saveDialog.FileName);

                    DialogResult result = MessageBox.Show($"✅ Файл сохранен!\n{saveDialog.FileName}\n\nОткрыть файл?",
                        "Готово", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = saveDialog.FileName,
                            UseShellExecute = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToCSV(string filePath)
        {
            using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                sw.WriteLine($"Отчет по заказам за период с {dtpStartDate.Value:dd.MM.yyyy} по {dtpEndDate.Value:dd.MM.yyyy}");
                sw.WriteLine("=================================================================");
                sw.WriteLine();
                sw.WriteLine("Дата;Номер заказа;Клиент;Телефон;Адрес;Сумма;Кол-во блюд;Статус");

                foreach (DataRow row in ordersData.Rows)
                {
                    sw.WriteLine($"{row["date_time"]};{row["order_number"]};{row["client"]};{row["phone"]};{row["address"]};{Convert.ToDecimal(row["total_amount"]):F2};{row["dishes_count"]};{row["status"]}");
                }

                sw.WriteLine();
                sw.WriteLine("ИТОГИ:");
                decimal totalRevenue = 0;
                int totalOrders = ordersData.Rows.Count;
                int totalDishes = 0;

                foreach (DataRow row in ordersData.Rows)
                {
                    totalRevenue += Convert.ToDecimal(row["total_amount"]);
                    totalDishes += Convert.ToInt32(row["dishes_count"]);
                }

                sw.WriteLine($"Всего заказов: {totalOrders}");
                sw.WriteLine($"Общая выручка: {totalRevenue:F2}");
                sw.WriteLine($"Всего блюд: {totalDishes}");
                if (totalOrders > 0)
                {
                    sw.WriteLine($"Средний чек: {(totalRevenue / totalOrders):F2}");
                }
            }
        }

        private void ExportToExcel(string filePath)
        {
            Excel.Application excelApp = null;
            Excel.Workbook workbook = null;
            Excel.Worksheet worksheet = null;

            try
            {
                excelApp = new Excel.Application();
                excelApp.Visible = false;
                excelApp.DisplayAlerts = false;

                workbook = excelApp.Workbooks.Add();
                worksheet = workbook.Worksheets[1];
                worksheet.Name = "Заказы";

                // Заголовок
                worksheet.Cells[1, 1] = $"Отчет по заказам за период с {dtpStartDate.Value:dd.MM.yyyy} по {dtpEndDate.Value:dd.MM.yyyy}";
                worksheet.Cells[1, 1].Font.Bold = true;
                worksheet.Cells[1, 1].Font.Size = 14;
                Excel.Range titleRange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, 8]];
                titleRange.Merge();
                titleRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                // Заголовки колонок
                string[] headers = { "Дата", "Номер заказа", "Клиент", "Телефон", "Адрес", "Сумма", "Кол-во блюд", "Статус" };
                for (int i = 0; i < headers.Length; i++)
                {
                    Excel.Range headerCell = worksheet.Cells[3, i + 1];
                    headerCell.Value = headers[i];
                    headerCell.Font.Bold = true;
                    headerCell.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(97, 173, 123));
                    headerCell.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    headerCell.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                }

                // Данные
                for (int row = 0; row < ordersData.Rows.Count; row++)
                {
                    worksheet.Cells[row + 4, 1] = ordersData.Rows[row]["date_time"].ToString();
                    worksheet.Cells[row + 4, 2] = ordersData.Rows[row]["order_number"].ToString();
                    worksheet.Cells[row + 4, 3] = ordersData.Rows[row]["client"].ToString();
                    worksheet.Cells[row + 4, 4] = ordersData.Rows[row]["phone"].ToString();
                    worksheet.Cells[row + 4, 5] = ordersData.Rows[row]["address"].ToString();

                    // Сумма - форматируем как число
                    Excel.Range amountCell = worksheet.Cells[row + 4, 6];
                    amountCell.Value = Convert.ToDouble(ordersData.Rows[row]["total_amount"]);
                    amountCell.NumberFormat = "#,##0.00";
                    amountCell.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;

                    worksheet.Cells[row + 4, 7] = ordersData.Rows[row]["dishes_count"].ToString();
                    worksheet.Cells[row + 4, 8] = ordersData.Rows[row]["status"].ToString();

                    // Границы
                    Excel.Range dataRange = worksheet.Range[worksheet.Cells[row + 4, 1], worksheet.Cells[row + 4, 8]];
                    dataRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                }

                // Итоги
                int lastRow = ordersData.Rows.Count + 5;
                decimal totalRevenue = 0;
                int totalOrders = ordersData.Rows.Count;
                int totalDishes = 0;

                foreach (DataRow row in ordersData.Rows)
                {
                    totalRevenue += Convert.ToDecimal(row["total_amount"]);
                    totalDishes += Convert.ToInt32(row["dishes_count"]);
                }

                // Заголовок "ИТОГИ"
                Excel.Range totalTitleCell = worksheet.Cells[lastRow, 1];
                totalTitleCell.Value = "ИТОГИ:";
                totalTitleCell.Font.Bold = true;
                totalTitleCell.Font.Size = 12;
                Excel.Range totalTitleRange = worksheet.Range[worksheet.Cells[lastRow, 1], worksheet.Cells[lastRow, 2]];
                totalTitleRange.Merge();

                // Всего заказов
                worksheet.Cells[lastRow + 1, 1] = "Всего заказов:";
                worksheet.Cells[lastRow + 1, 1].Font.Bold = true;
                worksheet.Cells[lastRow + 1, 2] = totalOrders;

                // Общая выручка
                worksheet.Cells[lastRow + 2, 1] = "Общая выручка:";
                worksheet.Cells[lastRow + 2, 1].Font.Bold = true;
                Excel.Range revenueCell = worksheet.Cells[lastRow + 2, 2];
                revenueCell.Value = Convert.ToDouble(totalRevenue);
                revenueCell.NumberFormat = "#,##0.00";
                revenueCell.Font.Bold = true;

                // Всего блюд
                worksheet.Cells[lastRow + 3, 1] = "Всего блюд:";
                worksheet.Cells[lastRow + 3, 1].Font.Bold = true;
                worksheet.Cells[lastRow + 3, 2] = totalDishes;

                // Средний чек
                if (totalOrders > 0)
                {
                    worksheet.Cells[lastRow + 4, 1] = "Средний чек:";
                    worksheet.Cells[lastRow + 4, 1].Font.Bold = true;
                    Excel.Range avgCell = worksheet.Cells[lastRow + 4, 2];
                    avgCell.Value = Convert.ToDouble(totalRevenue / totalOrders);
                    avgCell.NumberFormat = "#,##0.00";
                }

                worksheet.Columns.AutoFit();
                workbook.SaveAs(filePath);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при создании Excel: {ex.Message}");
            }
            finally
            {
                // Освобождаем ресурсы
                if (worksheet != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);
                if (workbook != null)
                {
                    workbook.Close(false);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                }
                if (excelApp != null)
                {
                    excelApp.Quit();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            if (this.Owner != null && !this.Owner.IsDisposed)
            {
                this.Owner.Show(); // Показываем родительскую форму
            }
        }
    }
}