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
        private DataTable profitData;
        private DateTime minDate = new DateTime(2024, 1, 1);
        private DateTime maxDate = new DateTime(2040, 12, 31);
        private CultureInfo russianCulture = new CultureInfo("ru-RU");
        private bool filterByPeriod = false;

        private System.Windows.Forms.ToolTip toolTip1;

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
            profitData = new DataTable();
            toolTip1 = new System.Windows.Forms.ToolTip();

            // Настройка кнопки экспорта заказов
            if (btnExportOrders != null)
            {
                btnExportOrders.Text = "Отчёт по заказам";
                btnExportOrders.FlatStyle = FlatStyle.Flat;
                btnExportOrders.FlatAppearance.BorderSize = 1;
                btnExportOrders.FlatAppearance.BorderColor = Color.Black;
                btnExportOrders.BackColor = Color.DarkSeaGreen;
                btnExportOrders.ForeColor = Color.Black;
                btnExportOrders.Click += BtnExportOrders_Click;
            }

            // Настройка кнопки экспорта прибыли
            if (btnExportProfit != null)
            {
                btnExportProfit.Text = "Отчёт по прибыли";
                btnExportProfit.FlatStyle = FlatStyle.Flat;
                btnExportProfit.FlatAppearance.BorderSize = 1;
                btnExportProfit.FlatAppearance.BorderColor = Color.Black;
                btnExportProfit.BackColor = Color.DarkSeaGreen;
                btnExportProfit.ForeColor = Color.Black;
                btnExportProfit.Click += BtnExportProfit_Click;
            }

            // Настройка CheckBox для фильтрации по периоду
            if (chkFilterByPeriod != null)
            {
                chkFilterByPeriod.Text = "Фильтровать по периоду";
                chkFilterByPeriod.Checked = false;
                chkFilterByPeriod.CheckedChanged += ChkFilterByPeriod_CheckedChanged;
            }

            // Настройка кнопки деталей заказа
            if (buttonDetail != null)
            {
                buttonDetail.Click += ButtonDetail_Click;
            }

            // Настройка DataGridView
            SetupDataGridView();

            // Подписка на изменение дат
            if (dtpStartDate != null)
            {
                dtpStartDate.ValueChanged += DatePicker_ValueChanged;
            }
            if (dtpEndDate != null)
            {
                dtpEndDate.ValueChanged += DatePicker_ValueChanged;
            }

            // Подписка на двойной клик
            if (dgvOrders != null)
            {
                dgvOrders.CellDoubleClick += DgvOrders_CellDoubleClick;
            }

            // Добавляем обработчик закрытия формы
            this.FormClosing += OrdersReportForm_FormClosing;
        }

        // ===================== ОБРАБОТЧИК ЗАКРЫТИЯ ФОРМЫ =====================

        /// <summary>
        /// Обработчик закрытия формы - при нажатии на крестик переходим на DirectorForm
        /// </summary>
        private void OrdersReportForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Проверяем, что закрытие инициировано пользователем (крестик или Alt+F4)
            if (e.CloseReason == CloseReason.UserClosing)
            {
                // Отменяем закрытие формы
                e.Cancel = true;

                // Скрываем текущую форму
                this.Visible = false;

                // Открываем форму директора
                DirectorForm director = new DirectorForm();
                director.Show();
            }
        }

        private void ChkFilterByPeriod_CheckedChanged(object sender, EventArgs e)
        {
            filterByPeriod = chkFilterByPeriod.Checked;
            dtpStartDate.Enabled = filterByPeriod;
            dtpEndDate.Enabled = filterByPeriod;

            // Загружаем заказы при изменении фильтра
            LoadOrders();
        }

        private void DatePicker_ValueChanged(object sender, EventArgs e)
        {
            // Загружаем заказы при изменении дат (только если фильтр включен)
            if (filterByPeriod)
            {
                LoadOrders();
            }
        }

        private void RevenueForm_Load(object sender, EventArgs e)
        {
            dtpStartDate.MinDate = minDate;
            dtpStartDate.MaxDate = DateTime.Now > maxDate ? maxDate : DateTime.Now;
            dtpEndDate.MinDate = minDate;
            dtpEndDate.MaxDate = DateTime.Now > maxDate ? maxDate : DateTime.Now;

            dtpEndDate.Value = DateTime.Now;
            dtpStartDate.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            dtpStartDate.Enabled = false;
            dtpEndDate.Enabled = false;

            labelStartDate.Text = "Период с:";
            labelEndDate.Text = "по:";

            // Загружаем все заказы при загрузке формы
            LoadOrders();
        }

        private void SetupDataGridView()
        {
            if (dgvOrders == null) return;

            dgvOrders.ReadOnly = true;
            dgvOrders.AllowUserToAddRows = false;
            dgvOrders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvOrders.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvOrders.MultiSelect = false;
            dgvOrders.RowHeadersVisible = false;
            dgvOrders.EnableHeadersVisualStyles = false;

            Color headerBackColor = Color.FromArgb(97, 173, 123);
            Color selectionColor = Color.FromArgb(233, 242, 236);

            dgvOrders.ColumnHeadersDefaultCellStyle.BackColor = headerBackColor;
            dgvOrders.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvOrders.ColumnHeadersDefaultCellStyle.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            dgvOrders.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvOrders.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvOrders.ColumnHeadersDefaultCellStyle.Padding = new Padding(0, 3, 0, 3);
            dgvOrders.ColumnHeadersHeight = 45;

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
            dgvOrders.GridColor = Color.Gray;
            dgvOrders.BorderStyle = BorderStyle.Fixed3D;
            dgvOrders.CellBorderStyle = DataGridViewCellBorderStyle.Single;

            toolTip1.SetToolTip(dgvOrders, "Двойной клик для просмотра деталей заказа");

            dgvOrders.Columns.Clear();

            // ID заказа (скрытая)
            DataGridViewTextBoxColumn colId = new DataGridViewTextBoxColumn();
            colId.Name = "id_order";
            colId.DataPropertyName = "id_order";
            colId.Visible = false;
            dgvOrders.Columns.Add(colId);

            // Дата и время
            DataGridViewTextBoxColumn colDateTime = new DataGridViewTextBoxColumn();
            colDateTime.Name = "date_time";
            colDateTime.HeaderText = "Дата и время";
            colDateTime.DataPropertyName = "date_time";
            colDateTime.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvOrders.Columns.Add(colDateTime);

            // Номер заказа
            DataGridViewTextBoxColumn colOrderNumber = new DataGridViewTextBoxColumn();
            colOrderNumber.Name = "order_number";
            colOrderNumber.HeaderText = "№ заказа";
            colOrderNumber.DataPropertyName = "order_number";
            colOrderNumber.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvOrders.Columns.Add(colOrderNumber);

            // Клиент
            DataGridViewTextBoxColumn colClient = new DataGridViewTextBoxColumn();
            colClient.Name = "client";
            colClient.HeaderText = "Клиент";
            colClient.DataPropertyName = "client";
            colClient.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvOrders.Columns.Add(colClient);

            // Телефон
            DataGridViewTextBoxColumn colPhone = new DataGridViewTextBoxColumn();
            colPhone.Name = "phone";
            colPhone.HeaderText = "Телефон";
            colPhone.DataPropertyName = "phone";
            colPhone.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvOrders.Columns.Add(colPhone);

            // Адрес
            DataGridViewTextBoxColumn colAddress = new DataGridViewTextBoxColumn();
            colAddress.Name = "address";
            colAddress.HeaderText = "Адрес";
            colAddress.DataPropertyName = "address";
            colAddress.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvOrders.Columns.Add(colAddress);

            // Сумма
            DataGridViewTextBoxColumn colAmount = new DataGridViewTextBoxColumn();
            colAmount.Name = "total_amount";
            colAmount.HeaderText = "Сумма";
            colAmount.DataPropertyName = "total_amount";
            colAmount.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colAmount.DefaultCellStyle.Format = "N2";
            colAmount.DefaultCellStyle.ForeColor = Color.DarkGreen;
            dgvOrders.Columns.Add(colAmount);

            // Кол-во блюд
            DataGridViewTextBoxColumn colDishes = new DataGridViewTextBoxColumn();
            colDishes.Name = "dishes_count";
            colDishes.HeaderText = "Кол-во блюд";
            colDishes.DataPropertyName = "dishes_count";
            colDishes.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvOrders.Columns.Add(colDishes);

            // Статус
            DataGridViewTextBoxColumn colStatus = new DataGridViewTextBoxColumn();
            colStatus.Name = "status";
            colStatus.HeaderText = "Статус";
            colStatus.DataPropertyName = "status";
            colStatus.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvOrders.Columns.Add(colStatus);
        }

        private void LoadOrders()
        {
            try
            {
                DateTime startDate = dtpStartDate.Value.Date;
                DateTime endDate = dtpEndDate.Value.Date;

                if (filterByPeriod && startDate > endDate)
                {
                    MessageBox.Show("Дата 'С' не может быть позже даты 'По'!",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                ordersData = LoadOrdersFromDB(startDate, endDate);
                dgvOrders.DataSource = ordersData;

                // Обновляем статистику
                UpdateStatistics();
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
                    CONCAT('#', o.id_order) as order_number,
                    o.name_client as client,
                    o.phone_number as phone,
                    o.address,
                    COALESCE(SUM(od.quantity * od.price_at_order), 0) as total_amount,
                    COALESCE(COUNT(od.id_order_dish), 0) as dishes_count,
                    os.status_name as status
                FROM orders o
                LEFT JOIN order_dish od ON o.id_order = od.id_order
                LEFT JOIN order_statuses os ON o.id_status = os.id_status";

            if (filterByPeriod)
            {
                query += " WHERE DATE(o.created_at) BETWEEN @startDate AND @endDate";
            }

            query += " GROUP BY o.id_order ORDER BY o.created_at DESC";

            using (var connection = SettingsBD.GetConnection())
            {
                connection.Open();
                using (var cmd = new MySqlCommand(query, connection))
                {
                    if (filterByPeriod)
                    {
                        cmd.Parameters.AddWithValue("@startDate", startDate);
                        cmd.Parameters.AddWithValue("@endDate", endDate);
                    }

                    using (var adapter = new MySqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }

            return dt;
        }

        private void UpdateStatistics()
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

                // Можно обновить лейблы если они есть
                // labelTotalOrders.Text = $"Всего заказов: {totalOrders}";
                // labelTotalRevenue.Text = $"Общая выручка: {totalRevenue:N2} ₽";
            }
            catch
            {

            }
        }

        // ===================== ДЕТАЛИ ЗАКАЗА =====================

        private void ButtonDetail_Click(object sender, EventArgs e)
        {
            ShowOrderDetails();
        }

        private void DgvOrders_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            ShowOrderDetails();
        }

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

                Form detailForm = new Form();
                detailForm.Text = $"Детали заказа №{orderId}";
                detailForm.Size = new Size(900, 680);
                detailForm.StartPosition = FormStartPosition.CenterParent;
                detailForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                detailForm.MaximizeBox = false;
                detailForm.MinimizeBox = false;
                detailForm.BackColor = Color.White;
                detailForm.Font = new Font("Times New Roman", 12, FontStyle.Regular);

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

                DataGridView dgvOrderDetails = CreateOrderDetailsDataGridView();
                dgvOrderDetails.Location = new Point(10, 120);
                dgvOrderDetails.Size = new Size(865, 400);

                List<OrderDetailItem> orderDetails = LoadOrderDetails(orderId);
                DataTable dt = new DataTable();
                dt.Columns.Add("dish_name", typeof(string));
                dt.Columns.Add("quantity", typeof(int));
                dt.Columns.Add("price", typeof(decimal));
                dt.Columns.Add("total_price", typeof(decimal));
                dt.Columns.Add("is_gift", typeof(bool));

                foreach (var item in orderDetails)
                {
                    DataRow row = dt.NewRow();
                    row["dish_name"] = item.DisplayName;
                    row["quantity"] = item.Quantity;
                    row["price"] = item.Price;
                    row["total_price"] = item.TotalPrice;
                    row["is_gift"] = item.IsGift;
                    dt.Rows.Add(row);
                }

                dgvOrderDetails.DataSource = dt;

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

            Color headerBackColor = Color.FromArgb(97, 173, 123);
            Color selectionColor = Color.FromArgb(233, 242, 236);

            dgv.ColumnHeadersDefaultCellStyle.BackColor = headerBackColor;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(0, 3, 0, 3);
            dgv.ColumnHeadersHeight = 45;

            dgv.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            dgv.DefaultCellStyle.Padding = new Padding(5);
            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.ForeColor = Color.Black;
            dgv.DefaultCellStyle.SelectionBackColor = selectionColor;
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;

            dgv.RowTemplate.Height = 35;
            dgv.GridColor = Color.Gray;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.Single;

            // Колонки
            DataGridViewTextBoxColumn colDishName = new DataGridViewTextBoxColumn();
            colDishName.Name = "dish_name";
            colDishName.HeaderText = "Наименование";
            colDishName.DataPropertyName = "dish_name";
            colDishName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.Columns.Add(colDishName);

            DataGridViewTextBoxColumn colQuantity = new DataGridViewTextBoxColumn();
            colQuantity.Name = "quantity";
            colQuantity.HeaderText = "Кол-во";
            colQuantity.DataPropertyName = "quantity";
            colQuantity.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.Columns.Add(colQuantity);

            DataGridViewTextBoxColumn colPrice = new DataGridViewTextBoxColumn();
            colPrice.Name = "price";
            colPrice.HeaderText = "Цена";
            colPrice.DataPropertyName = "price";
            colPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgv.Columns.Add(colPrice);

            DataGridViewTextBoxColumn colTotal = new DataGridViewTextBoxColumn();
            colTotal.Name = "total_price";
            colTotal.HeaderText = "Сумма";
            colTotal.DataPropertyName = "total_price";
            colTotal.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgv.Columns.Add(colTotal);

            DataGridViewCheckBoxColumn colIsGift = new DataGridViewCheckBoxColumn();
            colIsGift.Name = "is_gift";
            colIsGift.DataPropertyName = "is_gift";
            colIsGift.Visible = false;
            dgv.Columns.Add(colIsGift);

            dgv.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex == dgv.Columns["price"].Index && e.RowIndex >= 0 && e.Value != null)
                {
                    if (e.Value is decimal || e.Value is int || e.Value is double)
                    {
                        decimal price = Convert.ToDecimal(e.Value);
                        e.Value = price.ToString("N2") + " ₽";
                        e.FormattingApplied = true;
                    }
                }
                else if (e.ColumnIndex == dgv.Columns["total_price"].Index && e.RowIndex >= 0 && e.Value != null)
                {
                    if (e.Value is decimal || e.Value is int || e.Value is double)
                    {
                        decimal total = Convert.ToDecimal(e.Value);
                        e.Value = total.ToString("N2") + " ₽";
                        e.FormattingApplied = true;
                    }
                }
            };

            dgv.DataBindingComplete += (s, e) =>
            {
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (row.Cells["is_gift"].Value != null && (bool)row.Cells["is_gift"].Value)
                    {
                        row.DefaultCellStyle.BackColor = Color.LightYellow;
                        row.DefaultCellStyle.ForeColor = Color.DarkOrange;
                        row.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Bold);
                    }
                }
            };

            return dgv;
        }

        // ===================== ЭКСПОРТ ЗАКАЗОВ =====================

        private void BtnExportOrders_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime startDate = dtpStartDate.Value.Date;
                DateTime endDate = dtpEndDate.Value.Date;

                if (filterByPeriod && startDate > endDate)
                {
                    MessageBox.Show("Дата 'С' не может быть позже даты 'По'!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Cursor = Cursors.WaitCursor;

                DataTable exportData = LoadOrdersFromDB(startDate, endDate);

                if (exportData == null || exportData.Rows.Count == 0)
                {
                    Cursor = Cursors.Default;
                    MessageBox.Show("Нет данных для экспорта!", "Предупреждение",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Excel файлы (*.xlsx)|*.xlsx";

                if (filterByPeriod)
                {
                    saveDialog.FileName = $"Заказы_{startDate:yyyyMMdd}-{endDate:yyyyMMdd}";
                }
                else
                {
                    saveDialog.FileName = $"Заказы_все_{DateTime.Now:yyyyMMdd_HHmmss}";
                }

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    ExportOrdersToExcel(saveDialog.FileName, exportData, startDate, endDate);

                    DialogResult result = MessageBox.Show($"✅ Файл успешно сохранен!\n{saveDialog.FileName}\n\nОткрыть файл?",
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
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void ExportOrdersToExcel(string filePath, DataTable data, DateTime startDate, DateTime endDate)
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
                if (filterByPeriod)
                {
                    worksheet.Cells[1, 1] = $"Отчет по заказам за период с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy}";
                }
                else
                {
                    worksheet.Cells[1, 1] = "Отчет по ВСЕМ заказам";
                }
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
                for (int row = 0; row < data.Rows.Count; row++)
                {
                    worksheet.Cells[row + 4, 1] = data.Rows[row]["date_time"].ToString();
                    worksheet.Cells[row + 4, 2] = data.Rows[row]["order_number"].ToString();
                    worksheet.Cells[row + 4, 3] = data.Rows[row]["client"].ToString();
                    worksheet.Cells[row + 4, 4] = data.Rows[row]["phone"].ToString();
                    worksheet.Cells[row + 4, 5] = data.Rows[row]["address"].ToString();

                    Excel.Range amountCell = worksheet.Cells[row + 4, 6];
                    amountCell.Value = Convert.ToDouble(data.Rows[row]["total_amount"]);
                    amountCell.NumberFormat = "#,##0.00";
                    amountCell.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;

                    worksheet.Cells[row + 4, 7] = data.Rows[row]["dishes_count"].ToString();
                    worksheet.Cells[row + 4, 8] = data.Rows[row]["status"].ToString();

                    Excel.Range dataRange = worksheet.Range[worksheet.Cells[row + 4, 1], worksheet.Cells[row + 4, 8]];
                    dataRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                }

                // Итоги
                int lastRow = data.Rows.Count + 5;
                decimal totalRevenue = 0;
                int totalOrders = data.Rows.Count;
                int totalDishes = 0;

                foreach (DataRow row in data.Rows)
                {
                    totalRevenue += Convert.ToDecimal(row["total_amount"]);
                    totalDishes += Convert.ToInt32(row["dishes_count"]);
                }

                Excel.Range totalTitleCell = worksheet.Cells[lastRow, 1];
                totalTitleCell.Value = "ИТОГИ:";
                totalTitleCell.Font.Bold = true;
                totalTitleCell.Font.Size = 12;
                Excel.Range totalTitleRange = worksheet.Range[worksheet.Cells[lastRow, 1], worksheet.Cells[lastRow, 2]];
                totalTitleRange.Merge();

                worksheet.Cells[lastRow + 1, 1] = "Всего заказов:";
                worksheet.Cells[lastRow + 1, 1].Font.Bold = true;
                worksheet.Cells[lastRow + 1, 2] = totalOrders;

                worksheet.Cells[lastRow + 2, 1] = "Общая выручка:";
                worksheet.Cells[lastRow + 2, 1].Font.Bold = true;
                Excel.Range revenueCell = worksheet.Cells[lastRow + 2, 2];
                revenueCell.Value = Convert.ToDouble(totalRevenue);
                revenueCell.NumberFormat = "#,##0.00";
                revenueCell.Font.Bold = true;

                worksheet.Cells[lastRow + 3, 1] = "Всего блюд:";
                worksheet.Cells[lastRow + 3, 1].Font.Bold = true;
                worksheet.Cells[lastRow + 3, 2] = totalDishes;

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
            finally
            {
                if (worksheet != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);
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

        // ===================== ЭКСПОРТ ПРИБЫЛИ =====================

        private void BtnExportProfit_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime startDate = dtpStartDate.Value.Date;
                DateTime endDate = dtpEndDate.Value.Date;

                if (filterByPeriod && startDate > endDate)
                {
                    MessageBox.Show("Дата 'С' не может быть позже даты 'По'!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Cursor = Cursors.WaitCursor;

                DataTable exportData = LoadProfitFromDB(startDate, endDate);

                if (exportData == null || exportData.Rows.Count == 0)
                {
                    Cursor = Cursors.Default;
                    MessageBox.Show("Нет данных для экспорта!", "Предупреждение",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Excel файлы (*.xlsx)|*.xlsx";

                if (filterByPeriod)
                {
                    saveDialog.FileName = $"Прибыль_{startDate:yyyyMMdd}-{endDate:yyyyMMdd}";
                }
                else
                {
                    saveDialog.FileName = $"Прибыль_все_{DateTime.Now:yyyyMMdd_HHmmss}";
                }

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    ExportProfitToExcel(saveDialog.FileName, exportData, startDate, endDate);

                    DialogResult result = MessageBox.Show($"✅ Файл успешно сохранен!\n{saveDialog.FileName}\n\nОткрыть файл?",
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
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private DataTable LoadProfitFromDB(DateTime startDate, DateTime endDate)
        {
            DataTable dt = new DataTable();

            string query = @"
                SELECT 
                    DATE(o.created_at) as date,
                    COALESCE(SUM(od.quantity * od.price_at_order), 0) as revenue,
                    COALESCE(SUM(od.quantity * d.cost), 0) as total_cost,
                    COUNT(DISTINCT o.id_order) as orders_count
                FROM orders o
                LEFT JOIN order_dish od ON o.id_order = od.id_order
                LEFT JOIN dishes d ON od.id_dish = d.id_dish
                WHERE o.id_status IN (4,5,6)";

            if (filterByPeriod)
            {
                query += " AND DATE(o.created_at) BETWEEN @startDate AND @endDate";
            }

            query += " GROUP BY DATE(o.created_at) ORDER BY DATE(o.created_at)";

            using (var connection = SettingsBD.GetConnection())
            {
                connection.Open();
                using (var cmd = new MySqlCommand(query, connection))
                {
                    if (filterByPeriod)
                    {
                        cmd.Parameters.AddWithValue("@startDate", startDate);
                        cmd.Parameters.AddWithValue("@endDate", endDate);
                    }

                    using (var adapter = new MySqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }

            // Добавляем колонки для прибыли и маржи
            dt.Columns.Add("profit", typeof(decimal));
            dt.Columns.Add("margin", typeof(decimal));

            foreach (DataRow row in dt.Rows)
            {
                decimal revenue = Convert.ToDecimal(row["revenue"]);
                decimal cost = Convert.ToDecimal(row["total_cost"]);
                decimal profit = revenue - cost;
                decimal margin = revenue > 0 ? (profit / revenue) * 100 : 0;

                row["profit"] = profit;
                row["margin"] = margin;
            }

            // Переименовываем колонки
            dt.Columns["date"].ColumnName = "Дата";
            dt.Columns["revenue"].ColumnName = "Выручка";
            dt.Columns["total_cost"].ColumnName = "Себестоимость";
            dt.Columns["orders_count"].ColumnName = "Кол-во заказов";
            dt.Columns["profit"].ColumnName = "Прибыль";
            dt.Columns["margin"].ColumnName = "Маржа %";

            return dt;
        }

        private void ExportProfitToExcel(string filePath, DataTable data, DateTime startDate, DateTime endDate)
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
                worksheet.Name = "Прибыль";

                // Заголовок
                if (filterByPeriod)
                {
                    worksheet.Cells[1, 1] = $"Отчет по прибыли за период с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy}";
                }
                else
                {
                    worksheet.Cells[1, 1] = "Отчет по прибыли за ВСЕ время";
                }
                worksheet.Cells[1, 1].Font.Bold = true;
                worksheet.Cells[1, 1].Font.Size = 14;
                Excel.Range titleRange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, 6]];
                titleRange.Merge();
                titleRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                // Заголовки колонок
                string[] headers = { "Дата", "Выручка (₽)", "Себестоимость (₽)", "Прибыль (₽)", "Кол-во заказов", "Маржа %" };
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
                for (int row = 0; row < data.Rows.Count; row++)
                {
                    worksheet.Cells[row + 4, 1] = Convert.ToDateTime(data.Rows[row]["Дата"]).ToString("dd.MM.yyyy");

                    Excel.Range revenueCell = worksheet.Cells[row + 4, 2];
                    revenueCell.Value = Convert.ToDouble(data.Rows[row]["Выручка"]);
                    revenueCell.NumberFormat = "#,##0.00";
                    revenueCell.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;

                    Excel.Range costCell = worksheet.Cells[row + 4, 3];
                    costCell.Value = Convert.ToDouble(data.Rows[row]["Себестоимость"]);
                    costCell.NumberFormat = "#,##0.00";
                    costCell.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;

                    Excel.Range profitCell = worksheet.Cells[row + 4, 4];
                    profitCell.Value = Convert.ToDouble(data.Rows[row]["Прибыль"]);
                    profitCell.NumberFormat = "#,##0.00";
                    profitCell.Font.Bold = true;
                    profitCell.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;

                    worksheet.Cells[row + 4, 5] = data.Rows[row]["Кол-во заказов"].ToString();

                    Excel.Range marginCell = worksheet.Cells[row + 4, 6];
                    marginCell.Value = Convert.ToDouble(data.Rows[row]["Маржа %"]);
                    marginCell.NumberFormat = "0.0";
                    marginCell.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                    Excel.Range dataRange = worksheet.Range[worksheet.Cells[row + 4, 1], worksheet.Cells[row + 4, 6]];
                    dataRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                }

                // Итоги
                int lastRow = data.Rows.Count + 5;
                decimal totalRevenue = 0, totalCost = 0;
                int totalOrders = 0;

                foreach (DataRow row in data.Rows)
                {
                    totalRevenue += Convert.ToDecimal(row["Выручка"]);
                    totalCost += Convert.ToDecimal(row["Себестоимость"]);
                    totalOrders += Convert.ToInt32(row["Кол-во заказов"]);
                }

                decimal totalProfit = totalRevenue - totalCost;
                decimal avgMargin = totalRevenue > 0 ? (totalProfit / totalRevenue) * 100 : 0;

                Excel.Range totalTitleCell = worksheet.Cells[lastRow, 1];
                totalTitleCell.Value = "ИТОГИ:";
                totalTitleCell.Font.Bold = true;
                totalTitleCell.Font.Size = 12;
                Excel.Range totalTitleRange = worksheet.Range[worksheet.Cells[lastRow, 1], worksheet.Cells[lastRow, 2]];
                totalTitleRange.Merge();

                worksheet.Cells[lastRow + 1, 1] = "Общая выручка:";
                worksheet.Cells[lastRow + 1, 1].Font.Bold = true;
                Excel.Range revenueCell2 = worksheet.Cells[lastRow + 1, 2];
                revenueCell2.Value = Convert.ToDouble(totalRevenue);
                revenueCell2.NumberFormat = "#,##0.00";
                revenueCell2.Font.Bold = true;

                worksheet.Cells[lastRow + 2, 1] = "Общая себестоимость:";
                worksheet.Cells[lastRow + 2, 1].Font.Bold = true;
                Excel.Range costCell2 = worksheet.Cells[lastRow + 2, 2];
                costCell2.Value = Convert.ToDouble(totalCost);
                costCell2.NumberFormat = "#,##0.00";
                costCell2.Font.Bold = true;

                worksheet.Cells[lastRow + 3, 1] = "Общая прибыль:";
                worksheet.Cells[lastRow + 3, 1].Font.Bold = true;
                Excel.Range profitCell2 = worksheet.Cells[lastRow + 3, 2];
                profitCell2.Value = Convert.ToDouble(totalProfit);
                profitCell2.NumberFormat = "#,##0.00";
                profitCell2.Font.Bold = true;
                profitCell2.Font.Color = totalProfit >= 0 ?
                    System.Drawing.ColorTranslator.ToOle(Color.DarkGreen) :
                    System.Drawing.ColorTranslator.ToOle(Color.Red);

                worksheet.Cells[lastRow + 4, 1] = "Всего заказов:";
                worksheet.Cells[lastRow + 4, 1].Font.Bold = true;
                worksheet.Cells[lastRow + 4, 2] = totalOrders;

                worksheet.Cells[lastRow + 5, 1] = "Средняя маржа:";
                worksheet.Cells[lastRow + 5, 1].Font.Bold = true;
                Excel.Range avgCell = worksheet.Cells[lastRow + 5, 2];
                avgCell.Value = Convert.ToDouble(avgMargin);
                avgCell.NumberFormat = "0.0";
                avgCell.Font.Bold = true;

                worksheet.Columns.AutoFit();
                workbook.SaveAs(filePath);
            }
            finally
            {
                if (worksheet != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);
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
                this.Owner.Show();
            }
        }
    }
}