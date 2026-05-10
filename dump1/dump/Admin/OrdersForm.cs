using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Globalization;

namespace dump
{
    public partial class OrdersForm : Form
    {
        private DataTable ordersTable;
        private BindingSource bindingSource;
        private MySqlDataAdapter dataAdapter;
        private bool isFormatting = false;

        // Словарь для хранения статусов (id, name)
        private Dictionary<int, string> statusDictionary = new Dictionary<int, string>();

        // Максимальное количество символов для поиска
        private const int MAX_ORDER_SEARCH_LENGTH = 10;
        private const int MAX_PHONE_SEARCH_LENGTH = 18;

        // Культура для форматирования
        private CultureInfo russianCulture = new CultureInfo("ru-RU");

        // Типы поиска
        private enum SearchType
        {
            ByOrderNumber,
            ByPhone
        }
        private SearchType currentSearchType = SearchType.ByOrderNumber;

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

        public OrdersForm()
        {
            InitializeComponent();
            InitializeComponents();
            this.FormClosing += OrdersForm_FormClosing;
        }

        private void OrdersForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Visible = false;
                AdminForm admin = new AdminForm();
                admin.Show();
            }
        }

        private void InitializeComponents()
        {
            InitializeDataGridView();

            // НАСТРОЙКА КОМБОБОКСА ДЛЯ ВЫБОРА ТИПА ПОИСКА
            if (comboBoxSearchType != null)
            {
                comboBoxSearchType.DropDownStyle = ComboBoxStyle.DropDownList;
                comboBoxSearchType.Items.Clear();
                comboBoxSearchType.Items.Add("Поиск по номеру заказа");
                comboBoxSearchType.Items.Add("Поиск по номеру телефона");
                comboBoxSearchType.SelectedIndex = 0;
                comboBoxSearchType.SelectedIndexChanged += ComboBoxSearchType_SelectedIndexChanged;
            }

            // Настройка textBoxSearch
            SetupSearchPlaceholder();
            textBoxSearch.TextChanged += textBoxSearch_TextChanged;
            textBoxSearch.KeyPress += textBoxSearch_KeyPress;
            textBoxSearch.Click += TextBoxSearch_Click;

            // Настройка comboBoxStatus
            comboBoxStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxStatus.SelectedIndexChanged += comboBoxStatus_SelectedIndexChanged;

            // Настройка кнопок
            buttonReset.Click += buttonReset_Click;
            buttonReset.FlatStyle = FlatStyle.Flat;
            buttonReset.FlatAppearance.BorderSize = 1;
            buttonReset.FlatAppearance.BorderColor = Color.Black;
            buttonReset.BackColor = Color.DarkSeaGreen;
            buttonReset.ForeColor = Color.Black;
            buttonReset.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            buttonReset.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            buttonReset.MouseDown += (s, e) => buttonReset.FlatAppearance.BorderColor = Color.DarkBlue;
            buttonReset.MouseUp += (s, e) => buttonReset.FlatAppearance.BorderColor = Color.Black;
            buttonReset.MouseLeave += (s, e) => buttonReset.FlatAppearance.BorderColor = Color.Black;

            // Настройка кнопки деталей заказа
            buttonDetail.Click += ButtonDetail_Click;
            buttonDetail.FlatStyle = FlatStyle.Flat;
            buttonDetail.FlatAppearance.BorderSize = 1;
            buttonDetail.FlatAppearance.BorderColor = Color.Black;
            buttonDetail.BackColor = Color.DarkSeaGreen;
            buttonDetail.ForeColor = Color.Black;
            buttonDetail.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            buttonDetail.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            buttonDetail.MouseDown += (s, e) => buttonDetail.FlatAppearance.BorderColor = Color.DarkBlue;
            buttonDetail.MouseUp += (s, e) => buttonDetail.FlatAppearance.BorderColor = Color.Black;
            buttonDetail.MouseLeave += (s, e) => buttonDetail.FlatAppearance.BorderColor = Color.Black;

            // Двойной клик по строке
            dgvOrders.CellDoubleClick += DgvOrders_CellDoubleClick;

            // Загрузка данных
            LoadStatusesToComboBox();
            LoadOrders();
        }

        // ===== МАСКИРОВАНИЕ НОМЕРА ТЕЛЕФОНА =====
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

        // ===== НАСТРОЙКА PLACEHOLDER ДЛЯ ПОЛЯ ПОИСКА =====
        private void SetupSearchPlaceholder()
        {
            if (currentSearchType == SearchType.ByOrderNumber)
            {
                textBoxSearch.Text = "Введите номер заказа...";
                textBoxSearch.ForeColor = Color.Gray;
                textBoxSearch.MaxLength = MAX_ORDER_SEARCH_LENGTH;
            }
            else
            {
                textBoxSearch.Text = "Введите номер телефона...";
                textBoxSearch.ForeColor = Color.Gray;
                textBoxSearch.MaxLength = MAX_PHONE_SEARCH_LENGTH;
            }
        }

        private void ComboBoxSearchType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxSearchType.SelectedIndex == 0)
                currentSearchType = SearchType.ByOrderNumber;
            else
                currentSearchType = SearchType.ByPhone;

            textBoxSearch.Text = "";
            SetupSearchPlaceholder();
            LoadOrders();
        }

        private void TextBoxSearch_Click(object sender, EventArgs e)
        {
            if (textBoxSearch.ForeColor == Color.Gray)
            {
                textBoxSearch.Text = "";
                textBoxSearch.ForeColor = Color.Black;
                textBoxSearch.Focus();
            }
        }

        private void textBoxSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (textBoxSearch.ForeColor == Color.Gray)
            {
                textBoxSearch.Text = "";
                textBoxSearch.ForeColor = Color.Black;
                if (!char.IsControl(e.KeyChar))
                {
                    e.Handled = false;
                }
                return;
            }

            if (char.IsControl(e.KeyChar))
            {
                e.Handled = false;
                return;
            }

            if (!char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
                System.Media.SystemSounds.Beep.Play();
            }
        }

        private void textBoxSearch_TextChanged(object sender, EventArgs e)
        {
            if (textBoxSearch.ForeColor == Color.Gray)
                return;

            if (isFormatting) return;
            isFormatting = true;

            string inputText = textBoxSearch.Text;

            if (currentSearchType == SearchType.ByOrderNumber)
            {
                string digits = new string(inputText.Where(char.IsDigit).ToArray());

                if (digits.Length > MAX_ORDER_SEARCH_LENGTH)
                {
                    digits = digits.Substring(0, MAX_ORDER_SEARCH_LENGTH);
                }

                string formatted = digits;

                if (formatted != inputText)
                {
                    int cursorPos = textBoxSearch.SelectionStart;
                    textBoxSearch.Text = formatted;
                    textBoxSearch.SelectionStart = Math.Min(cursorPos, formatted.Length);
                }

                if (digits.Length > 0)
                    LoadOrdersWithFilter(digits, false);
                else
                    LoadOrdersWithFilter("", false);
            }
            else
            {
                string digits = new string(inputText.Where(char.IsDigit).ToArray());

                if (digits.Length > 11)
                    digits = digits.Substring(0, 11);

                int cursorPos = textBoxSearch.SelectionStart;
                int selectionLength = textBoxSearch.SelectionLength;

                string formatted = FormatPhoneNumber(digits);

                if (formatted != inputText)
                {
                    textBoxSearch.Text = formatted;

                    if (cursorPos <= textBoxSearch.Text.Length)
                    {
                        textBoxSearch.SelectionStart = cursorPos;
                        textBoxSearch.SelectionLength = selectionLength;
                    }
                    else
                    {
                        textBoxSearch.SelectionStart = textBoxSearch.Text.Length;
                    }
                }

                if (digits.Length >= 3)
                    LoadOrdersWithFilter(digits, false);
                else if (digits.Length == 0)
                    LoadOrdersWithFilter("", false);
            }

            isFormatting = false;
        }

        private string FormatPhoneNumber(string digits)
        {
            if (string.IsNullOrEmpty(digits))
                return "";

            if (digits.Length == 0)
                return "";

            string normalizedDigits = digits;
            if (normalizedDigits.Length > 0 && normalizedDigits[0] != '7')
            {
                normalizedDigits = "7" + normalizedDigits;
                if (normalizedDigits.Length > 11)
                    normalizedDigits = normalizedDigits.Substring(0, 11);
            }

            if (normalizedDigits.Length < 2)
                return normalizedDigits;

            string result = "+7";

            if (normalizedDigits.Length >= 2)
            {
                int operatorLength = Math.Min(3, normalizedDigits.Length - 1);
                result += " (" + normalizedDigits.Substring(1, operatorLength);

                if (normalizedDigits.Length > 4)
                {
                    result += ") " + normalizedDigits.Substring(4, Math.Min(3, normalizedDigits.Length - 4));

                    if (normalizedDigits.Length > 7)
                    {
                        result += "-" + normalizedDigits.Substring(7, Math.Min(2, normalizedDigits.Length - 7));

                        if (normalizedDigits.Length > 9)
                        {
                            result += "-" + normalizedDigits.Substring(9, Math.Min(2, normalizedDigits.Length - 9));
                        }
                    }
                }
                else
                {
                    result += ")";
                }
            }

            return result;
        }

        private class StatusState
        {
            public int SelectedStatusId { get; set; }
            public string SelectedStatusName { get; set; }
        }

        // ===== ПРОСМОТР ДЕТАЛЕЙ ЗАКАЗА =====
        private void ButtonDetail_Click(object sender, EventArgs e)
        {
            ShowOrderDetails();
        }

        private void DgvOrders_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            ShowOrderDetails();
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
                string phoneNumber = selectedRow.Cells["phone_number"].Value?.ToString() ?? "";
                string address = selectedRow.Cells["address"].Value?.ToString() ?? "";
                int persons = Convert.ToInt32(selectedRow.Cells["number_persons"].Value ?? 0);
                DateTime orderDate = selectedRow.Cells["delivery_date"].Value != null ?
                    Convert.ToDateTime(selectedRow.Cells["delivery_date"].Value) : DateTime.Now;
                string comment = selectedRow.Cells["comment"].Value?.ToString() ?? "";
                string paymentMethod = selectedRow.Cells["payment_method"].Value?.ToString() ?? "";
                int currentStatusId = Convert.ToInt32(selectedRow.Cells["id_status"].Value ?? 0);
                string currentStatus = selectedRow.Cells["status_name"].Value?.ToString() ?? "";

                StatusState statusState = new StatusState
                {
                    SelectedStatusId = currentStatusId,
                    SelectedStatusName = currentStatus
                };

                Form detailForm = new Form();
                detailForm.Text = $"Просмотр заказа №{orderId}";
                detailForm.Size = new Size(820, 700);
                detailForm.StartPosition = FormStartPosition.CenterParent;
                detailForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                detailForm.MaximizeBox = false;
                detailForm.MinimizeBox = false;
                detailForm.BackColor = Color.White;
                detailForm.AutoScroll = true;
                detailForm.Font = new Font("Times New Roman", 12, FontStyle.Regular);

                Panel infoPanel = CreateInfoPanel(orderId, phoneNumber, address, persons, orderDate, paymentMethod);
                Panel statusPanel = CreateStatusPanel(currentStatusId, currentStatus, statusState);
                Panel commentPanel = CreateCommentPanel(comment);

                List<OrderDetailItem> orderDetails = LoadOrderDetails(orderId);
                DataGridView dgvOrderDetails = CreateOrderDetailsDataGridView();
                DataTable dt = CreateOrderDetailsDataTable(orderDetails);
                dgvOrderDetails.DataSource = dt;

                int currentY = 15;
                infoPanel.Location = new Point(15, currentY);
                detailForm.Controls.Add(infoPanel);
                currentY += infoPanel.Height + 15;
                statusPanel.Location = new Point(15, currentY);
                detailForm.Controls.Add(statusPanel);
                currentY += statusPanel.Height + 15;
                commentPanel.Location = new Point(15, currentY);
                detailForm.Controls.Add(commentPanel);
                currentY += commentPanel.Height + 15;
                dgvOrderDetails.Location = new Point(15, currentY);
                dgvOrderDetails.Size = new Size(765, 280);
                detailForm.Controls.Add(dgvOrderDetails);
                currentY += dgvOrderDetails.Height + 10;

                decimal totalSum = orderDetails.Where(x => !x.IsGift).Sum(x => x.TotalPrice);
                Panel totalPanel = CreateTotalPanel(totalSum);
                totalPanel.Location = new Point(15, currentY);
                detailForm.Controls.Add(totalPanel);

                detailForm.FormClosing += (s, args) =>
                {
                    if (statusState.SelectedStatusId != currentStatusId)
                    {
                        DialogResult result = MessageBox.Show(
                            $"Изменить статус заказа с \"{currentStatus}\" на \"{statusState.SelectedStatusName}\"?",
                            "Сохранение изменений",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question);

                        if (result == DialogResult.Yes)
                        {
                            if (UpdateOrderStatus(orderId, statusState.SelectedStatusId, statusState.SelectedStatusName))
                            {
                                MessageBox.Show("Статус успешно обновлен!", "Успех",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        else if (result == DialogResult.Cancel)
                        {
                            args.Cancel = true;
                        }
                    }
                };

                detailForm.ShowDialog(this);
                RefreshOrdersData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке деталей заказа: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private Panel CreateInfoPanel(int orderId, string phoneNumber, string address,
            int persons, DateTime orderDate, string paymentMethod)
        {
            Panel panel = new Panel();
            panel.Size = new Size(765, 110);
            panel.BorderStyle = BorderStyle.FixedSingle;
            panel.BackColor = Color.FromArgb(240, 240, 240);

            // Маскируем номер телефона
            string maskedPhone = MaskPhone(phoneNumber);

            Label lblInfo = new Label();
            lblInfo.Text = $"ЗАКАЗ №{orderId}\n" +
                          $"Телефон: {maskedPhone}\n" +
                          $"Адрес: {address}\n" +
                          $"Количество персон: {persons} | Дата доставки: {orderDate:dd.MM.yyyy}\n" +
                          $"Способ оплаты: {paymentMethod}";
            lblInfo.Location = new Point(10, 10);
            lblInfo.Size = new Size(740, 90);
            lblInfo.Font = new Font("Times New Roman", 11, FontStyle.Regular);
            lblInfo.TextAlign = ContentAlignment.TopLeft;

            panel.Controls.Add(lblInfo);
            return panel;
        }

        private Panel CreateStatusPanel(int currentStatusId, string currentStatus, StatusState statusState)
        {
            Panel panel = new Panel();
            panel.Size = new Size(765, 60);
            panel.BorderStyle = BorderStyle.FixedSingle;
            panel.BackColor = Color.FromArgb(255, 255, 220);

            Label lblCurrentStatus = new Label();
            lblCurrentStatus.Text = "Текущий статус:";
            lblCurrentStatus.Location = new Point(10, 18);
            lblCurrentStatus.Size = new Size(100, 25);
            lblCurrentStatus.Font = new Font("Times New Roman", 11, FontStyle.Bold);
            lblCurrentStatus.TextAlign = ContentAlignment.MiddleLeft;

            Label lblCurrentStatusValue = new Label();
            lblCurrentStatusValue.Text = currentStatus;
            lblCurrentStatusValue.Location = new Point(120, 18);
            lblCurrentStatusValue.Size = new Size(150, 25);
            lblCurrentStatusValue.Font = new Font("Times New Roman", 11, FontStyle.Bold);
            lblCurrentStatusValue.ForeColor = Color.DarkBlue;
            lblCurrentStatusValue.TextAlign = ContentAlignment.MiddleLeft;

            Label lblNewStatus = new Label();
            lblNewStatus.Text = "Новый статус:";
            lblNewStatus.Location = new Point(280, 18);
            lblNewStatus.Size = new Size(90, 25);
            lblNewStatus.Font = new Font("Times New Roman", 11, FontStyle.Regular);
            lblNewStatus.TextAlign = ContentAlignment.MiddleLeft;

            ComboBox cmbNewStatus = new ComboBox();
            cmbNewStatus.Location = new Point(380, 18);
            cmbNewStatus.Size = new Size(200, 25);
            cmbNewStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbNewStatus.Font = new Font("Times New Roman", 11, FontStyle.Regular);

            foreach (var status in statusDictionary)
            {
                cmbNewStatus.Items.Add(new StatusItem(status.Key, status.Value));
            }
            cmbNewStatus.DisplayMember = "Name";

            foreach (StatusItem item in cmbNewStatus.Items)
            {
                if (item.Id == currentStatusId)
                {
                    cmbNewStatus.SelectedItem = item;
                    statusState.SelectedStatusId = item.Id;
                    statusState.SelectedStatusName = item.Name;
                    break;
                }
            }

            cmbNewStatus.SelectedIndexChanged += (s, e) =>
            {
                if (cmbNewStatus.SelectedItem is StatusItem selectedItem)
                {
                    statusState.SelectedStatusId = selectedItem.Id;
                    statusState.SelectedStatusName = selectedItem.Name;
                }
            };

            panel.Controls.Add(lblCurrentStatus);
            panel.Controls.Add(lblCurrentStatusValue);
            panel.Controls.Add(lblNewStatus);
            panel.Controls.Add(cmbNewStatus);

            return panel;
        }

        private Panel CreateCommentPanel(string comment)
        {
            Panel panel = new Panel();
            panel.Size = new Size(765, 60);
            panel.BorderStyle = BorderStyle.FixedSingle;
            panel.BackColor = Color.FromArgb(240, 255, 240);

            Label lblCommentTitle = new Label();
            lblCommentTitle.Text = "Комментарий к заказу:";
            lblCommentTitle.Location = new Point(10, 5);
            lblCommentTitle.Size = new Size(200, 20);
            lblCommentTitle.Font = new Font("Times New Roman", 11, FontStyle.Bold);

            Label lblComment = new Label();
            lblComment.Text = string.IsNullOrEmpty(comment) ? "(нет комментария)" : comment;
            lblComment.Location = new Point(10, 30);
            lblComment.Size = new Size(740, 25);
            lblComment.Font = new Font("Times New Roman", 11, FontStyle.Regular);
            lblComment.TextAlign = ContentAlignment.MiddleLeft;
            lblComment.AutoEllipsis = true;

            panel.Controls.Add(lblCommentTitle);
            panel.Controls.Add(lblComment);

            return panel;
        }

        private Panel CreateTotalPanel(decimal totalSum)
        {
            Panel panel = new Panel();
            panel.Size = new Size(765, 50);
            panel.BorderStyle = BorderStyle.FixedSingle;
            panel.BackColor = Color.FromArgb(230, 255, 230);

            Label lblTotalTitle = new Label();
            lblTotalTitle.Text = "ИТОГО:";
            lblTotalTitle.Location = new Point(10, 12);
            lblTotalTitle.Size = new Size(80, 25);
            lblTotalTitle.Font = new Font("Times New Roman", 14, FontStyle.Bold);
            lblTotalTitle.ForeColor = Color.DarkGreen;
            lblTotalTitle.TextAlign = ContentAlignment.MiddleLeft;

            Label lblTotalSum = new Label();
            lblTotalSum.Text = $"{totalSum.ToString("N2", russianCulture)} ₽";
            lblTotalSum.Location = new Point(100, 12);
            lblTotalSum.Size = new Size(200, 25);
            lblTotalSum.Font = new Font("Times New Roman", 14, FontStyle.Bold);
            lblTotalSum.ForeColor = Color.DarkRed;
            lblTotalSum.TextAlign = ContentAlignment.MiddleLeft;

            panel.Controls.Add(lblTotalTitle);
            panel.Controls.Add(lblTotalSum);

            return panel;
        }

        private DataGridView CreateOrderDetailsDataGridView()
        {
            DataGridView dgv = new DataGridView();
            dgv.AutoGenerateColumns = false;
            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.Fixed3D;
            dgv.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.AllowUserToResizeColumns = false;
            dgv.MultiSelect = false;
            dgv.EditMode = DataGridViewEditMode.EditProgrammatically;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(97, 173, 123);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersHeight = 40;

            dgv.RowsDefaultCellStyle.BackColor = Color.White;
            dgv.RowsDefaultCellStyle.ForeColor = Color.Black;
            dgv.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(233, 242, 236);
            dgv.RowsDefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.RowTemplate.Height = 35;
            dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            DataGridViewTextBoxColumn colDishName = new DataGridViewTextBoxColumn();
            colDishName.Name = "dish_name";
            colDishName.HeaderText = "Наименование";
            colDishName.DataPropertyName = "dish_name";
            colDishName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colDishName.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            colDishName.FillWeight = 50;
            dgv.Columns.Add(colDishName);

            DataGridViewTextBoxColumn colQuantity = new DataGridViewTextBoxColumn();
            colQuantity.Name = "quantity";
            colQuantity.HeaderText = "Кол-во";
            colQuantity.DataPropertyName = "quantity";
            colQuantity.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colQuantity.FillWeight = 15;
            dgv.Columns.Add(colQuantity);

            DataGridViewTextBoxColumn colPrice = new DataGridViewTextBoxColumn();
            colPrice.Name = "price";
            colPrice.HeaderText = "Цена";
            colPrice.DataPropertyName = "price";
            colPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colPrice.FillWeight = 15;
            dgv.Columns.Add(colPrice);

            DataGridViewTextBoxColumn colTotal = new DataGridViewTextBoxColumn();
            colTotal.Name = "total_price";
            colTotal.HeaderText = "Сумма";
            colTotal.DataPropertyName = "total_price";
            colTotal.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
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
                        row.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Bold);

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

        private bool UpdateOrderStatus(int orderId, int newStatusId, string newStatusName)
        {
            try
            {
                string query = "UPDATE orders SET id_status = @statusId WHERE id_order = @orderId";

                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@statusId", newStatusId);
                        cmd.Parameters.AddWithValue("@orderId", orderId);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            UpdateOrderStatusInGrid(orderId, newStatusId, newStatusName);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении статуса в БД: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        private void UpdateOrderStatusInGrid(int orderId, int newStatusId, string newStatusName)
        {
            foreach (DataGridViewRow row in dgvOrders.Rows)
            {
                if (row.Cells["id_order"].Value != null &&
                    Convert.ToInt32(row.Cells["id_order"].Value) == orderId)
                {
                    row.Cells["id_status"].Value = newStatusId;
                    row.Cells["status_name"].Value = newStatusName;
                    break;
                }
            }
        }

        private void RefreshOrdersData()
        {
            string searchText = textBoxSearch.Text;

            if (string.IsNullOrWhiteSpace(searchText) || textBoxSearch.ForeColor == Color.Gray)
            {
                LoadOrdersWithFilter("", false);
            }
            else
            {
                if (currentSearchType == SearchType.ByOrderNumber)
                {
                    string digits = new string(searchText.Where(char.IsDigit).ToArray());
                    if (digits.Length > 0)
                        LoadOrdersWithFilter(digits, false);
                    else
                        LoadOrdersWithFilter("", false);
                }
                else
                {
                    string digits = new string(searchText.Where(char.IsDigit).ToArray());
                    if (digits.Length >= 3)
                        LoadOrdersWithFilter(digits, false);
                    else
                        LoadOrdersWithFilter("", false);
                }
            }
        }

        // ===== ИНИЦИАЛИЗАЦИЯ DATA GRID VIEW (БЕЗ КОЛОНКИ "Клиент" И С МАСКИРОВАНИЕМ ТЕЛЕФОНА) =====
        private void InitializeDataGridView()
        {
            dgvOrders.ShowCellToolTips = false;
            dgvOrders.AutoGenerateColumns = false;
            dgvOrders.AllowUserToAddRows = false;
            dgvOrders.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvOrders.ReadOnly = true;
            dgvOrders.MultiSelect = false;
            dgvOrders.AllowUserToDeleteRows = false;
            dgvOrders.AllowUserToResizeRows = false;
            dgvOrders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvOrders.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvOrders.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvOrders.RowTemplate.Height = 40;
            dgvOrders.RowTemplate.MinimumHeight = 40;
            dgvOrders.RowHeadersVisible = false;
            dgvOrders.EnableHeadersVisualStyles = false;
            dgvOrders.ColumnHeadersHeight = 45;
            dgvOrders.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            Color headerBackColor = Color.FromArgb(97, 173, 123);

            dgvOrders.ColumnHeadersDefaultCellStyle.BackColor = headerBackColor;
            dgvOrders.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvOrders.ColumnHeadersDefaultCellStyle.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            dgvOrders.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvOrders.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvOrders.ColumnHeadersDefaultCellStyle.Padding = new Padding(0, 3, 0, 3);
            dgvOrders.ColumnHeadersDefaultCellStyle.SelectionBackColor = headerBackColor;
            dgvOrders.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.Black;

            dgvOrders.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            dgvOrders.DefaultCellStyle.Padding = new Padding(0, 2, 0, 2);
            dgvOrders.DefaultCellStyle.BackColor = Color.White;
            dgvOrders.DefaultCellStyle.ForeColor = Color.Black;
            dgvOrders.DefaultCellStyle.SelectionBackColor = Color.FromArgb(233, 242, 236);
            dgvOrders.DefaultCellStyle.SelectionForeColor = Color.Black;

            dgvOrders.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(233, 242, 236);
            dgvOrders.RowsDefaultCellStyle.SelectionForeColor = Color.Black;
            dgvOrders.RowsDefaultCellStyle.BackColor = Color.White;
            dgvOrders.RowsDefaultCellStyle.ForeColor = Color.Black;

            dgvOrders.GridColor = Color.Gray;
            dgvOrders.BorderStyle = BorderStyle.FixedSingle;
            dgvOrders.CellBorderStyle = DataGridViewCellBorderStyle.Single;

            dgvOrders.Columns.Clear();

            // Колонка №: Номер заказа
            DataGridViewTextBoxColumn colId = new DataGridViewTextBoxColumn();
            colId.Name = "id_order";
            colId.HeaderText = "№";
            colId.DataPropertyName = "id_order";
            colId.Width = 60;
            colId.MinimumWidth = 60;
            colId.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colId.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            colId.Resizable = DataGridViewTriState.True;
            colId.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colId);

            // Колонка: Телефон (с маскированием)
            DataGridViewTextBoxColumn colPhone = new DataGridViewTextBoxColumn();
            colPhone.Name = "phone_number";
            colPhone.HeaderText = "Телефон";
            colPhone.DataPropertyName = "phone_number";
            colPhone.Width = 130;
            colPhone.MinimumWidth = 130;
            colPhone.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            colPhone.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colPhone.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            colPhone.Resizable = DataGridViewTriState.True;
            colPhone.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colPhone);

            // Колонка: Адрес
            DataGridViewTextBoxColumn colAddress = new DataGridViewTextBoxColumn();
            colAddress.Name = "address";
            colAddress.HeaderText = "Адрес";
            colAddress.DataPropertyName = "address";
            colAddress.Width = 320;
            colAddress.MinimumWidth = 250;
            colAddress.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            colAddress.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colAddress.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            colAddress.Resizable = DataGridViewTriState.True;
            colAddress.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colAddress);

            // Колонка: Персон
            DataGridViewTextBoxColumn colPersons = new DataGridViewTextBoxColumn();
            colPersons.Name = "number_persons";
            colPersons.HeaderText = "Персон";
            colPersons.DataPropertyName = "number_persons";
            colPersons.Width = 75;
            colPersons.MinimumWidth = 75;
            colPersons.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colPersons.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            colPersons.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            colPersons.Resizable = DataGridViewTriState.True;
            colPersons.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colPersons);

            // Колонка: Дата
            DataGridViewTextBoxColumn colDate = new DataGridViewTextBoxColumn();
            colDate.Name = "delivery_date";
            colDate.HeaderText = "Дата";
            colDate.DataPropertyName = "delivery_date";
            colDate.Width = 90;
            colDate.MinimumWidth = 90;
            colDate.DefaultCellStyle.Format = "dd.MM.yyyy";
            colDate.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colDate.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            colDate.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            colDate.Resizable = DataGridViewTriState.True;
            colDate.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colDate);

            // Колонка: Комментарий
            DataGridViewTextBoxColumn colComment = new DataGridViewTextBoxColumn();
            colComment.Name = "comment";
            colComment.HeaderText = "Комментарий";
            colComment.DataPropertyName = "comment";
            colComment.Width = 200;
            colComment.MinimumWidth = 150;
            colComment.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            colComment.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colComment.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            colComment.Resizable = DataGridViewTriState.True;
            colComment.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colComment);

            // Колонка: Оплата
            DataGridViewTextBoxColumn colPayment = new DataGridViewTextBoxColumn();
            colPayment.Name = "payment_method";
            colPayment.HeaderText = "Оплата";
            colPayment.DataPropertyName = "payment_method";
            colPayment.Width = 100;
            colPayment.MinimumWidth = 100;
            colPayment.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            colPayment.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colPayment.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            colPayment.Resizable = DataGridViewTriState.True;
            colPayment.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colPayment);

            // Скрытая колонка ID статуса
            DataGridViewTextBoxColumn colStatusId = new DataGridViewTextBoxColumn();
            colStatusId.Name = "id_status";
            colStatusId.HeaderText = "ID статуса";
            colStatusId.DataPropertyName = "id_status";
            colStatusId.Visible = false;
            colStatusId.Width = 50;
            colStatusId.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colStatusId);

            // Колонка: Статус
            DataGridViewTextBoxColumn colStatusName = new DataGridViewTextBoxColumn();
            colStatusName.Name = "status_name";
            colStatusName.HeaderText = "Статус";
            colStatusName.DataPropertyName = "status_name";
            colStatusName.Width = 120;
            colStatusName.MinimumWidth = 120;
            colStatusName.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            colStatusName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colStatusName.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            colStatusName.Resizable = DataGridViewTriState.True;
            colStatusName.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colStatusName);

            dgvOrders.ScrollBars = ScrollBars.Both;
            dgvOrders.Columns[dgvOrders.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvOrders.CellFormatting += DgvOrders_CellFormatting;
        }

        // ===== ФОРМАТИРОВАНИЕ ЯЧЕЕК (МАСКИРОВАНИЕ ТЕЛЕФОНА И ДАТЫ) =====
        private void DgvOrders_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvOrders.Columns[e.ColumnIndex].Name == "delivery_date" && e.RowIndex >= 0)
            {
                if (e.Value != null && e.Value != DBNull.Value)
                {
                    if (e.Value is DateTime date)
                    {
                        e.Value = date.ToString("dd.MM.yyyy");
                        e.FormattingApplied = true;
                    }
                }
            }

            if (e.RowIndex >= 0 && e.Value != null)
            {
                string columnName = dgvOrders.Columns[e.ColumnIndex].Name;
                if (columnName == "phone_number")
                {
                    e.Value = MaskPhone(e.Value.ToString());
                    e.FormattingApplied = true;
                }
            }
        }

        private void LoadStatusesToComboBox()
        {
            try
            {
                string query = "SELECT id_status, status_name FROM order_statuses ORDER BY id_status";
                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            comboBoxStatus.Items.Clear();
                            comboBoxStatus.Items.Add("Все статусы");
                            statusDictionary.Clear();

                            while (reader.Read())
                            {
                                int id = reader.GetInt32("id_status");
                                string name = reader.GetString("status_name");
                                statusDictionary.Add(id, name);
                                comboBoxStatus.Items.Add(new StatusItem(id, name));
                            }
                        }
                    }
                }

                comboBoxStatus.DisplayMember = "Name";
                comboBoxStatus.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статусов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadOrdersWithFilter(string searchValue = "", bool exactMatch = false)
        {
            int statusId = -1;
            if (comboBoxStatus.SelectedIndex > 0 && comboBoxStatus.SelectedItem is StatusItem statusItem)
            {
                statusId = statusItem.Id;
            }
            LoadOrders(searchValue, statusId, exactMatch);
        }

        private void LoadOrders(string searchValue = "", int statusId = -1, bool exactMatch = false)
        {
            try
            {
                string query = @"
                    SELECT o.id_order, o.phone_number, o.address, 
                           o.number_persons, o.delivery_date, o.comment, 
                           o.payment_method, o.id_status,
                           s.status_name
                    FROM orders o
                    LEFT JOIN order_statuses s ON o.id_status = s.id_status
                    WHERE 1=1";

                List<MySqlParameter> parameters = new List<MySqlParameter>();

                if (!string.IsNullOrEmpty(searchValue))
                {
                    if (currentSearchType == SearchType.ByOrderNumber)
                    {
                        if (exactMatch)
                        {
                            query += " AND o.id_order = @SearchValue";
                            parameters.Add(new MySqlParameter("@SearchValue", searchValue));
                        }
                        else
                        {
                            query += " AND CAST(o.id_order AS CHAR) LIKE @SearchValue";
                            parameters.Add(new MySqlParameter("@SearchValue", searchValue + "%"));
                        }
                    }
                    else
                    {
                        query += " AND REPLACE(REPLACE(REPLACE(REPLACE(phone_number, ' ', ''), '-', ''), '(', ''), ')', '') LIKE @SearchValue";
                        parameters.Add(new MySqlParameter("@SearchValue", "%" + searchValue + "%"));
                    }
                }

                if (statusId > 0)
                {
                    query += " AND o.id_status = @StatusId";
                    parameters.Add(new MySqlParameter("@StatusId", statusId));
                }

                query += " ORDER BY o.delivery_date DESC, o.id_order DESC";

                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    foreach (var param in parameters)
                    {
                        cmd.Parameters.Add(param);
                    }

                    dataAdapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    dataAdapter.Fill(dt);

                    if (bindingSource == null)
                    {
                        bindingSource = new BindingSource();
                        dgvOrders.DataSource = bindingSource;
                    }
                    bindingSource.DataSource = dt;

                    SetupColumnStyles();
                    AdjustDataGridViewAfterLoad();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AdjustDataGridViewAfterLoad()
        {
            dgvOrders.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            foreach (DataGridViewColumn col in dgvOrders.Columns)
            {
                if (col.Name != "id_order" && col.Name != "number_persons" &&
                    col.Name != "id_status")
                {
                    col.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                }
            }
            dgvOrders.Refresh();
        }

        private void SetupColumnStyles()
        {
            if (dgvOrders.Columns.Count > 0)
            {
                Color selectionColor = Color.FromArgb(233, 242, 236);
                foreach (DataGridViewColumn col in dgvOrders.Columns)
                {
                    if (col.Name != "id_status" && col.Visible)
                    {
                        col.DefaultCellStyle.SelectionBackColor = selectionColor;
                        col.DefaultCellStyle.SelectionForeColor = Color.Black;
                    }
                }
            }
        }

        private void comboBoxStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            string searchText = textBoxSearch.Text;

            if (string.IsNullOrWhiteSpace(searchText) || textBoxSearch.ForeColor == Color.Gray)
            {
                LoadOrdersWithFilter("", false);
            }
            else
            {
                if (currentSearchType == SearchType.ByOrderNumber)
                {
                    string digits = new string(searchText.Where(char.IsDigit).ToArray());
                    if (digits.Length > 0)
                        LoadOrdersWithFilter(digits, false);
                    else
                        LoadOrdersWithFilter("", false);
                }
                else
                {
                    string digits = new string(searchText.Where(char.IsDigit).ToArray());
                    if (digits.Length >= 3)
                        LoadOrdersWithFilter(digits, false);
                    else
                        LoadOrdersWithFilter("", false);
                }
            }
        }

        public class StatusItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public StatusItem(int id, string name) { Id = id; Name = name; }
            public override string ToString() { return Name; }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            AdminForm admin = new AdminForm();
            admin.Show();
        }

        private void OrdersForm_Load(object sender, EventArgs e) { }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            textBoxSearch.Text = "";
            SetupSearchPlaceholder();
            comboBoxStatus.SelectedIndex = 0;
            LoadOrders();
            textBoxSearch.Focus();
        }

        private void LoadOrders()
        {
            LoadOrders("", -1, false);
        }
    }
}