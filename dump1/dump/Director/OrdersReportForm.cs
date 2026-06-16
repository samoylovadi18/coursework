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
        private DataTable revenueData;
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
            revenueData = new DataTable();
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
                btnExportOrders.Font = new Font("Times New Roman", 14, FontStyle.Regular);
                btnExportOrders.Click += BtnExportOrders_Click;
            }

            // Настройка кнопки экспорта выручки
            if (btnExportProfit != null)
            {
                btnExportProfit.Text = "Отчёт по выручке";
                btnExportProfit.FlatStyle = FlatStyle.Flat;
                btnExportProfit.FlatAppearance.BorderSize = 1;
                btnExportProfit.FlatAppearance.BorderColor = Color.Black;
                btnExportProfit.BackColor = Color.DarkSeaGreen;
                btnExportProfit.ForeColor = Color.Black;
                btnExportProfit.Font = new Font("Times New Roman", 14, FontStyle.Regular);
                btnExportProfit.Click += BtnExportRevenue_Click;
            }

            // Настройка CheckBox для фильтрации по периоду
            if (chkFilterByPeriod != null)
            {
                chkFilterByPeriod.Text = "Фильтровать по периоду";
                chkFilterByPeriod.Checked = false;
                chkFilterByPeriod.Font = new Font("Times New Roman", 14);
                chkFilterByPeriod.CheckedChanged += ChkFilterByPeriod_CheckedChanged;
            }

            // Настройка кнопки деталей заказа
            if (buttonDetail != null)
            {
                buttonDetail.Text = "Детали заказа";
                buttonDetail.FlatStyle = FlatStyle.Flat;
                buttonDetail.FlatAppearance.BorderSize = 1;
                buttonDetail.FlatAppearance.BorderColor = Color.Black;
                buttonDetail.BackColor = Color.DarkSeaGreen;
                buttonDetail.ForeColor = Color.Black;
                buttonDetail.Font = new Font("Times New Roman", 14, FontStyle.Regular);
                buttonDetail.Click += ButtonDetail_Click;
            }

            // Настройка DataGridView
            SetupDataGridView();

            // Подписка на изменение дат
            if (dtpStartDate != null)
            {
                dtpStartDate.Font = new Font("Times New Roman", 14);
                dtpStartDate.ValueChanged += DatePicker_ValueChanged;
            }
            if (dtpEndDate != null)
            {
                dtpEndDate.Font = new Font("Times New Roman", 14);
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

        // ===== МАСКИРОВАНИЕ НОМЕРА ТЕЛЕФОНА (ЗАЩИТА ПЕРСОНАЛЬНЫХ ДАННЫХ) =====
        private string MaskPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone)) return "";
            try
            {
                string digits = new string(phone.Where(char.IsDigit).ToArray());
                if (digits.Length >= 11)
                {
                    return $"+7 ({digits.Substring(1, 3)}) ****-{digits.Substring(8, 2)}-{digits.Substring(10, 1)}";
                }
                return phone;
            }
            catch
            {
                return phone;
            }
        }

        // ===================== ОБРАБОТЧИК ЗАКРЫТИЯ ФОРМЫ =====================

        private void OrdersReportForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Visible = false;
                DirectorForm director = new DirectorForm();
                director.Show();
            }
        }

        private void ChkFilterByPeriod_CheckedChanged(object sender, EventArgs e)
        {
            filterByPeriod = chkFilterByPeriod.Checked;
            dtpStartDate.Enabled = filterByPeriod;
            dtpEndDate.Enabled = filterByPeriod;

            LoadOrders();
        }

        private void DatePicker_ValueChanged(object sender, EventArgs e)
        {
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
            labelStartDate.Font = new Font("Times New Roman", 14);
            labelEndDate.Font = new Font("Times New Roman", 14);

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

            // ===== ЗЕЛЕНАЯ ШАПКА - TIMES NEW ROMAN 14PT BOLD =====
            dgvOrders.ColumnHeadersDefaultCellStyle.BackColor = headerBackColor;
            dgvOrders.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvOrders.ColumnHeadersDefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Bold);
            dgvOrders.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvOrders.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvOrders.ColumnHeadersDefaultCellStyle.Padding = new Padding(0, 3, 0, 3);
            dgvOrders.ColumnHeadersHeight = 50;
            dgvOrders.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            // ===== ЯЧЕЙКИ - TIMES NEW ROMAN 14PT REGULAR =====
            dgvOrders.DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Regular);
            dgvOrders.DefaultCellStyle.Padding = new Padding(5);
            dgvOrders.DefaultCellStyle.BackColor = Color.White;
            dgvOrders.DefaultCellStyle.ForeColor = Color.Black;
            dgvOrders.DefaultCellStyle.SelectionBackColor = selectionColor;
            dgvOrders.DefaultCellStyle.SelectionForeColor = Color.Black;

            dgvOrders.RowsDefaultCellStyle.BackColor = Color.White;
            dgvOrders.RowsDefaultCellStyle.ForeColor = Color.Black;
            dgvOrders.RowsDefaultCellStyle.SelectionBackColor = selectionColor;
            dgvOrders.RowsDefaultCellStyle.SelectionForeColor = Color.Black;

            dgvOrders.RowTemplate.Height = 40;
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
            colId.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colId);

            // Дата и время
            DataGridViewTextBoxColumn colDateTime = new DataGridViewTextBoxColumn();
            colDateTime.Name = "date_time";
            colDateTime.HeaderText = "Дата и время";
            colDateTime.DataPropertyName = "date_time";
            colDateTime.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colDateTime.DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Regular);
            colDateTime.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colDateTime);

            // Номер заказа
            DataGridViewTextBoxColumn colOrderNumber = new DataGridViewTextBoxColumn();
            colOrderNumber.Name = "order_number";
            colOrderNumber.HeaderText = "№ заказа";
            colOrderNumber.DataPropertyName = "order_number";
            colOrderNumber.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colOrderNumber.DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Regular);
            colOrderNumber.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colOrderNumber);

            // Телефон (маскированный)
            DataGridViewTextBoxColumn colPhone = new DataGridViewTextBoxColumn();
            colPhone.Name = "phone";
            colPhone.HeaderText = "Телефон";
            colPhone.DataPropertyName = "phone";
            colPhone.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colPhone.DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Regular);
            colPhone.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colPhone);

            // Адрес
            DataGridViewTextBoxColumn colAddress = new DataGridViewTextBoxColumn();
            colAddress.Name = "address";
            colAddress.HeaderText = "Адрес";
            colAddress.DataPropertyName = "address";
            colAddress.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colAddress.DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Regular);
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
            colAmount.DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Bold);
            colAmount.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colAmount);

            // Кол-во блюд
            DataGridViewTextBoxColumn colDishes = new DataGridViewTextBoxColumn();
            colDishes.Name = "dishes_count";
            colDishes.HeaderText = "Кол-во блюд";
            colDishes.DataPropertyName = "dishes_count";
            colDishes.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colDishes.DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Regular);
            colDishes.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colDishes);

            // Статус
            DataGridViewTextBoxColumn colStatus = new DataGridViewTextBoxColumn();
            colStatus.Name = "status";
            colStatus.HeaderText = "Статус";
            colStatus.DataPropertyName = "status";
            colStatus.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colStatus.DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Regular);
            colStatus.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colStatus);

            // Форматирование ячеек (маскирование телефона)
            dgvOrders.CellFormatting += DgvOrders_CellFormatting;

            // Выделение строк
            dgvOrders.RowsDefaultCellStyle.SelectionBackColor = selectionColor;
            dgvOrders.RowsDefaultCellStyle.SelectionForeColor = Color.Black;
        }

        // ===== ФОРМАТИРОВАНИЕ ЯЧЕЕК (МАСКИРОВАНИЕ ТЕЛЕФОНА) =====
        private void DgvOrders_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.Value != null)
            {
                string columnName = dgvOrders.Columns[e.ColumnIndex].Name;
                if (columnName == "phone")
                {
                    e.Value = MaskPhone(e.Value.ToString());
                    e.FormattingApplied = true;
                }
            }
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
            }
            catch { }
        }

        // ===================== ДЕТАЛИ ЗАКАЗА (С МАСКИРОВАНИЕМ ТЕЛЕФОНА) =====================

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
                string phoneNumber = selectedRow.Cells["phone"].Value?.ToString() ?? "";
                string address = selectedRow.Cells["address"].Value?.ToString() ?? "";
                string orderDate = selectedRow.Cells["date_time"].Value?.ToString() ?? "";
                decimal totalAmount = selectedRow.Cells["total_amount"].Value != null ?
                    Convert.ToDecimal(selectedRow.Cells["total_amount"].Value) : 0;

                Form detailForm = new Form();
                detailForm.Text = $"Просмотр заказа №{orderId}";
                detailForm.Size = new Size(820, 680);
                detailForm.StartPosition = FormStartPosition.CenterParent;
                detailForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                detailForm.MaximizeBox = false;
                detailForm.MinimizeBox = false;
                detailForm.BackColor = Color.White;
                detailForm.AutoScroll = true;
                detailForm.Font = new Font("Times New Roman", 12, FontStyle.Regular);

                // ===== ПАНЕЛЬ ИНФОРМАЦИИ (С МАСКИРОВАННЫМ ТЕЛЕФОНОМ) =====
                Panel infoPanel = new Panel();
                infoPanel.Size = new Size(765, 110);
                infoPanel.BorderStyle = BorderStyle.FixedSingle;
                infoPanel.BackColor = Color.FromArgb(240, 240, 240);

                string maskedPhone = MaskPhone(phoneNumber);

                Label lblInfo = new Label();
                lblInfo.Text = $"ЗАКАЗ №{orderId}\n" +
                              $"Телефон: {maskedPhone}\n" +
                              $"Адрес: {address}\n" +
                              $"Дата доставки: {orderDate}\n";
                lblInfo.Location = new Point(10, 10);
                lblInfo.Size = new Size(740, 90);
                lblInfo.Font = new Font("Times New Roman", 11, FontStyle.Regular);
                lblInfo.TextAlign = ContentAlignment.TopLeft;

                infoPanel.Controls.Add(lblInfo);

                // ===== СПИСОК БЛЮД =====
                List<OrderDetailItem> orderDetails = LoadOrderDetails(orderId);
                DataGridView dgvOrderDetails = CreateOrderDetailsDataGridView();
                DataTable dt = CreateOrderDetailsDataTable(orderDetails);
                dgvOrderDetails.DataSource = dt;

                // ===== ПАНЕЛЬ ИТОГО =====
                Panel totalPanel = new Panel();
                totalPanel.Size = new Size(765, 50);
                totalPanel.BorderStyle = BorderStyle.FixedSingle;
                totalPanel.BackColor = Color.FromArgb(230, 255, 230);

                Label lblTotalTitle = new Label();
                lblTotalTitle.Text = "ИТОГО:";
                lblTotalTitle.Location = new Point(10, 12);
                lblTotalTitle.Size = new Size(80, 25);
                lblTotalTitle.Font = new Font("Times New Roman", 14, FontStyle.Bold);
                lblTotalTitle.ForeColor = Color.DarkGreen;
                lblTotalTitle.TextAlign = ContentAlignment.MiddleLeft;

                Label lblTotalSum = new Label();
                lblTotalSum.Text = $"{totalAmount.ToString("N2", russianCulture)} ₽";
                lblTotalSum.Location = new Point(100, 12);
                lblTotalSum.Size = new Size(200, 25);
                lblTotalSum.Font = new Font("Times New Roman", 14, FontStyle.Bold);
                lblTotalSum.ForeColor = Color.DarkRed;
                lblTotalSum.TextAlign = ContentAlignment.MiddleLeft;

                totalPanel.Controls.Add(lblTotalTitle);
                totalPanel.Controls.Add(lblTotalSum);

                // ===== РАСПОЛОЖЕНИЕ =====
                int currentY = 15;
                infoPanel.Location = new Point(15, currentY);
                detailForm.Controls.Add(infoPanel);
                currentY += infoPanel.Height + 15;
                dgvOrderDetails.Location = new Point(15, currentY);
                dgvOrderDetails.Size = new Size(765, 380);
                detailForm.Controls.Add(dgvOrderDetails);
                currentY += dgvOrderDetails.Height + 10;
                totalPanel.Location = new Point(15, currentY);
                detailForm.Controls.Add(totalPanel);

                // ===== КНОПКА ЗАКРЫТЬ =====
                Button btnClose = new Button();
                btnClose.Text = "Закрыть";
                btnClose.Size = new Size(120, 35);
                btnClose.Location = new Point(660, currentY + 60);
                btnClose.Font = new Font("Times New Roman", 11, FontStyle.Bold);
                btnClose.BackColor = Color.DarkSeaGreen;
                btnClose.FlatStyle = FlatStyle.Flat;
                btnClose.FlatAppearance.BorderSize = 1;
                btnClose.FlatAppearance.BorderColor = Color.Black;
                btnClose.Click += (s, e) => detailForm.Close();
                detailForm.Controls.Add(btnClose);

                detailForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке деталей заказа: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

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
            dgv.Font = new Font("Times New Roman", 14, FontStyle.Regular);
            dgv.EnableHeadersVisualStyles = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;

            Color headerBackColor = Color.FromArgb(97, 173, 123);
            Color selectionColor = Color.FromArgb(233, 242, 236);

            dgv.ColumnHeadersDefaultCellStyle.BackColor = headerBackColor;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(0, 3, 0, 3);
            dgv.ColumnHeadersHeight = 45;

            dgv.DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Regular);
            dgv.DefaultCellStyle.Padding = new Padding(5);
            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.ForeColor = Color.Black;
            dgv.DefaultCellStyle.SelectionBackColor = selectionColor;
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;

            dgv.RowTemplate.Height = 40;
            dgv.GridColor = Color.Gray;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.Single;

            // Колонки
            DataGridViewTextBoxColumn colDishName = new DataGridViewTextBoxColumn();
            colDishName.Name = "dish_name";
            colDishName.HeaderText = "Наименование";
            colDishName.DataPropertyName = "dish_name";
            colDishName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colDishName.DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Regular);
            colDishName.FillWeight = 50;
            dgv.Columns.Add(colDishName);

            DataGridViewTextBoxColumn colQuantity = new DataGridViewTextBoxColumn();
            colQuantity.Name = "quantity";
            colQuantity.HeaderText = "Кол-во";
            colQuantity.DataPropertyName = "quantity";
            colQuantity.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colQuantity.DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Regular);
            colQuantity.FillWeight = 15;
            dgv.Columns.Add(colQuantity);

            DataGridViewTextBoxColumn colPrice = new DataGridViewTextBoxColumn();
            colPrice.Name = "price";
            colPrice.HeaderText = "Цена";
            colPrice.DataPropertyName = "price";
            colPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colPrice.DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Regular);
            colPrice.FillWeight = 15;
            dgv.Columns.Add(colPrice);

            DataGridViewTextBoxColumn colTotal = new DataGridViewTextBoxColumn();
            colTotal.Name = "total_price";
            colTotal.HeaderText = "Сумма";
            colTotal.DataPropertyName = "total_price";
            colTotal.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colTotal.DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Bold);
            colTotal.FillWeight = 20;
            dgv.Columns.Add(colTotal);

            DataGridViewCheckBoxColumn colIsGift = new DataGridViewCheckBoxColumn();
            colIsGift.Name = "is_gift";
            colIsGift.DataPropertyName = "is_gift";
            colIsGift.Visible = false;
            dgv.Columns.Add(colIsGift);

            dgv.DataError += (s, e) => e.ThrowException = false;

            dgv.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex == dgv.Columns["price"].Index && e.RowIndex >= 0 && e.Value != null)
                {
                    if (e.Value is decimal || e.Value is int || e.Value is double)
                    {
                        decimal price = Convert.ToDecimal(e.Value);
                        e.Value = price.ToString("N2", russianCulture) + " ₽";
                        e.FormattingApplied = true;
                    }
                }
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

            dgv.DataBindingComplete += (s, e) =>
            {
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (row.Cells["is_gift"].Value != null && (bool)row.Cells["is_gift"].Value)
                    {
                        row.DefaultCellStyle.BackColor = Color.LightYellow;
                        row.DefaultCellStyle.ForeColor = Color.DarkOrange;
                        row.DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Bold);
                    }
                }
            };

            return dgv;
        }

        // ===================== ЭКСПОРТ ЗАКАЗОВ (С МАСКИРОВАННЫМ ТЕЛЕФОНОМ) =====================

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
                worksheet.Cells[1, 1].Font.Name = "Times New Roman";
                Excel.Range titleRange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, 7]];
                titleRange.Merge();
                titleRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                // Заголовки колонок (БЕЗ КЛИЕНТА, телефон маскируем)
                string[] headers = { "Дата", "Номер заказа", "Телефон", "Адрес", "Сумма", "Кол-во блюд", "Статус" };
                for (int i = 0; i < headers.Length; i++)
                {
                    Excel.Range headerCell = worksheet.Cells[3, i + 1];
                    headerCell.Value = headers[i];
                    headerCell.Font.Bold = true;
                    headerCell.Font.Size = 12;
                    headerCell.Font.Name = "Times New Roman";
                    headerCell.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(97, 173, 123));
                    headerCell.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    headerCell.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                }

                // Данные (маскируем телефон)
                for (int row = 0; row < data.Rows.Count; row++)
                {
                    worksheet.Cells[row + 4, 1] = data.Rows[row]["date_time"].ToString();
                    worksheet.Cells[row + 4, 2] = data.Rows[row]["order_number"].ToString();
                    // МАСКИРУЕМ ТЕЛЕФОН В ЭКСПОРТЕ
                    worksheet.Cells[row + 4, 3] = MaskPhone(data.Rows[row]["phone"].ToString());
                    worksheet.Cells[row + 4, 4] = data.Rows[row]["address"].ToString();

                    Excel.Range amountCell = worksheet.Cells[row + 4, 5];
                    amountCell.Value = Convert.ToDouble(data.Rows[row]["total_amount"]);
                    amountCell.NumberFormat = "#,##0.00";
                    amountCell.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                    amountCell.Font.Name = "Times New Roman";

                    worksheet.Cells[row + 4, 6] = data.Rows[row]["dishes_count"].ToString();
                    worksheet.Cells[row + 4, 7] = data.Rows[row]["status"].ToString();

                    Excel.Range dataRange = worksheet.Range[worksheet.Cells[row + 4, 1], worksheet.Cells[row + 4, 7]];
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
                totalTitleCell.Font.Name = "Times New Roman";
                Excel.Range totalTitleRange = worksheet.Range[worksheet.Cells[lastRow, 1], worksheet.Cells[lastRow, 2]];
                totalTitleRange.Merge();

                worksheet.Cells[lastRow + 1, 1] = "Всего заказов:";
                worksheet.Cells[lastRow + 1, 1].Font.Bold = true;
                worksheet.Cells[lastRow + 1, 1].Font.Name = "Times New Roman";
                worksheet.Cells[lastRow + 1, 2] = totalOrders;

                worksheet.Cells[lastRow + 2, 1] = "Общая выручка:";
                worksheet.Cells[lastRow + 2, 1].Font.Bold = true;
                worksheet.Cells[lastRow + 2, 1].Font.Name = "Times New Roman";
                Excel.Range revenueCell = worksheet.Cells[lastRow + 2, 2];
                revenueCell.Value = Convert.ToDouble(totalRevenue);
                revenueCell.NumberFormat = "#,##0.00";
                revenueCell.Font.Bold = true;
                revenueCell.Font.Name = "Times New Roman";

                worksheet.Cells[lastRow + 3, 1] = "Всего блюд:";
                worksheet.Cells[lastRow + 3, 1].Font.Bold = true;
                worksheet.Cells[lastRow + 3, 1].Font.Name = "Times New Roman";
                worksheet.Cells[lastRow + 3, 2] = totalDishes;

                if (totalOrders > 0)
                {
                    worksheet.Cells[lastRow + 4, 1] = "Средний чек:";
                    worksheet.Cells[lastRow + 4, 1].Font.Bold = true;
                    worksheet.Cells[lastRow + 4, 1].Font.Name = "Times New Roman";
                    Excel.Range avgCell = worksheet.Cells[lastRow + 4, 2];
                    avgCell.Value = Convert.ToDouble(totalRevenue / totalOrders);
                    avgCell.NumberFormat = "#,##0.00";
                    avgCell.Font.Name = "Times New Roman";
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

        // ===================== ЭКСПОРТ ВЫРУЧКИ =====================

        private void BtnExportRevenue_Click(object sender, EventArgs e)
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

                DataTable exportData = LoadRevenueFromDB(startDate, endDate);

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
                    saveDialog.FileName = $"Выручка_{startDate:yyyyMMdd}-{endDate:yyyyMMdd}";
                }
                else
                {
                    saveDialog.FileName = $"Выручка_все_{DateTime.Now:yyyyMMdd_HHmmss}";
                }

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    ExportRevenueToExcel(saveDialog.FileName, exportData, startDate, endDate);

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

        private DataTable LoadRevenueFromDB(DateTime startDate, DateTime endDate)
        {
            DataTable dt = new DataTable();

            string query = @"
                SELECT 
                    DATE(o.created_at) as date,
                    COALESCE(SUM(od.quantity * od.price_at_order), 0) as revenue,
                    COUNT(DISTINCT o.id_order) as orders_count,
                    COALESCE(COUNT(od.id_order_dish), 0) as dishes_count
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

            dt.Columns["date"].ColumnName = "Дата";
            dt.Columns["revenue"].ColumnName = "Выручка";
            dt.Columns["orders_count"].ColumnName = "Кол-во заказов";
            dt.Columns["dishes_count"].ColumnName = "Кол-во блюд";

            return dt;
        }

        private void ExportRevenueToExcel(string filePath, DataTable data, DateTime startDate, DateTime endDate)
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
                worksheet.Name = "Выручка";

                if (filterByPeriod)
                {
                    worksheet.Cells[1, 1] = $"Отчет по выручке за период с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy}";
                }
                else
                {
                    worksheet.Cells[1, 1] = "Отчет по выручке за ВСЕ время";
                }
                worksheet.Cells[1, 1].Font.Bold = true;
                worksheet.Cells[1, 1].Font.Size = 14;
                worksheet.Cells[1, 1].Font.Name = "Times New Roman";
                Excel.Range titleRange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, 4]];
                titleRange.Merge();
                titleRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                // Заголовки колонок
                string[] headers = { "Дата", "Выручка (₽)", "Кол-во заказов", "Кол-во блюд" };
                for (int i = 0; i < headers.Length; i++)
                {
                    Excel.Range headerCell = worksheet.Cells[3, i + 1];
                    headerCell.Value = headers[i];
                    headerCell.Font.Bold = true;
                    headerCell.Font.Size = 12;
                    headerCell.Font.Name = "Times New Roman";
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
                    revenueCell.Font.Bold = true;
                    revenueCell.Font.Name = "Times New Roman";
                    revenueCell.Font.Color = System.Drawing.ColorTranslator.ToOle(Color.DarkGreen);

                    worksheet.Cells[row + 4, 3] = data.Rows[row]["Кол-во заказов"].ToString();
                    worksheet.Cells[row + 4, 3].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    worksheet.Cells[row + 4, 3].Font.Name = "Times New Roman";

                    worksheet.Cells[row + 4, 4] = data.Rows[row]["Кол-во блюд"].ToString();
                    worksheet.Cells[row + 4, 4].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    worksheet.Cells[row + 4, 4].Font.Name = "Times New Roman";

                    Excel.Range dataRange = worksheet.Range[worksheet.Cells[row + 4, 1], worksheet.Cells[row + 4, 4]];
                    dataRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                }

                // Итоги
                int lastRow = data.Rows.Count + 5;
                decimal totalRevenue = 0;
                int totalOrders = 0;
                int totalDishes = 0;

                foreach (DataRow row in data.Rows)
                {
                    totalRevenue += Convert.ToDecimal(row["Выручка"]);
                    totalOrders += Convert.ToInt32(row["Кол-во заказов"]);
                    totalDishes += Convert.ToInt32(row["Кол-во блюд"]);
                }

                Excel.Range totalTitleCell = worksheet.Cells[lastRow, 1];
                totalTitleCell.Value = "ИТОГИ:";
                totalTitleCell.Font.Bold = true;
                totalTitleCell.Font.Size = 12;
                totalTitleCell.Font.Name = "Times New Roman";
                Excel.Range totalTitleRange = worksheet.Range[worksheet.Cells[lastRow, 1], worksheet.Cells[lastRow, 2]];
                totalTitleRange.Merge();

                worksheet.Cells[lastRow + 1, 1] = "Общая выручка:";
                worksheet.Cells[lastRow + 1, 1].Font.Bold = true;
                worksheet.Cells[lastRow + 1, 1].Font.Name = "Times New Roman";
                Excel.Range revenueCell2 = worksheet.Cells[lastRow + 1, 2];
                revenueCell2.Value = Convert.ToDouble(totalRevenue);
                revenueCell2.NumberFormat = "#,##0.00";
                revenueCell2.Font.Bold = true;
                revenueCell2.Font.Name = "Times New Roman";
                revenueCell2.Font.Color = System.Drawing.ColorTranslator.ToOle(Color.DarkGreen);

                worksheet.Cells[lastRow + 2, 1] = "Всего заказов:";
                worksheet.Cells[lastRow + 2, 1].Font.Bold = true;
                worksheet.Cells[lastRow + 2, 1].Font.Name = "Times New Roman";
                worksheet.Cells[lastRow + 2, 2] = totalOrders;

                worksheet.Cells[lastRow + 3, 1] = "Всего блюд:";
                worksheet.Cells[lastRow + 3, 1].Font.Bold = true;
                worksheet.Cells[lastRow + 3, 1].Font.Name = "Times New Roman";
                worksheet.Cells[lastRow + 3, 2] = totalDishes;

                if (totalOrders > 0)
                {
                    worksheet.Cells[lastRow + 4, 1] = "Средний чек:";
                    worksheet.Cells[lastRow + 4, 1].Font.Bold = true;
                    worksheet.Cells[lastRow + 4, 1].Font.Name = "Times New Roman";
                    Excel.Range avgCell = worksheet.Cells[lastRow + 4, 2];
                    avgCell.Value = Convert.ToDouble(totalRevenue / totalOrders);
                    avgCell.NumberFormat = "#,##0.00";
                    avgCell.Font.Bold = true;
                    avgCell.Font.Name = "Times New Roman";
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