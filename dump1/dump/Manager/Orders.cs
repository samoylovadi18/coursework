using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;

namespace dump
{
    public partial class Orders : Form
    {
        private DataTable ordersTable;
        private BindingSource bindingSource;
        private MySqlDataAdapter dataAdapter;
        private bool isFormatting = false;

        // Словарь для хранения статусов (id, name)
        private Dictionary<int, string> statusDictionary = new Dictionary<int, string>();

        // ID статуса "Доставлен" = 6
        private const int DELIVERED_STATUS_ID = 6;

        // Культура для форматирования
        private CultureInfo russianCulture = new CultureInfo("ru-RU");

        // Максимальное количество символов для поиска
        private const int MAX_SEARCH_LENGTH = 11;

        public Orders()
        {
            InitializeComponent();
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // Инициализация DataGridView
            InitializeDataGridView();

            // Настройка textBoxSearch
            textBoxSearch.TextChanged += textBoxSearch_TextChanged;
            textBoxSearch.KeyPress += textBoxSearch_KeyPress;
            textBoxSearch.MaxLength = MAX_SEARCH_LENGTH;
            textBoxSearch.Text = "";
            textBoxSearch.ForeColor = Color.Black;

            // Настройка comboBoxOrderStatus
            comboBoxOrderStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxOrderStatus.SelectedIndexChanged += comboBoxStatus_SelectedIndexChanged;

            // Настройка кнопки Reset
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

            // Добавляем обработчик двойного клика
            dataGridView1.CellDoubleClick += DataGridView1_CellDoubleClick;

            // Загрузка данных
            LoadStatusesToComboBox();
            LoadOrders();
        }

        // Вспомогательный класс для хранения состояния статуса
        private class StatusState
        {
            public int SelectedStatusId { get; set; }
            public string SelectedStatusName { get; set; }
        }

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

        // ===== МЕТОДЫ ДЛЯ ЧАСТИЧНОЙ МАСКИРОВКИ ПЕРСОНАЛЬНЫХ ДАННЫХ =====
        private string MaskFullName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return "";

            string[] parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length > 2)
                {
                    // Показываем первую букву и последнюю, середину заменяем звездочками
                    // Иванов -> И***в
                    parts[i] = parts[i][0] + new string('*', parts[i].Length - 2) + parts[i][parts[i].Length - 1];
                }
                else if (parts[i].Length == 2)
                {
                    // Для коротких слов показываем первую букву, вторую звездочкой
                    parts[i] = parts[i][0] + "*";
                }
            }
            return string.Join(" ", parts);
        }

        private string MaskPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone)) return "";

            // Показываем первые 2 цифры и последние 2 цифры, остальное звездочки
            // +7 (910) 123-45-67 -> +7 (***) ***-**-67
            char[] chars = phone.ToCharArray();
            int digitCount = 0;
            int lastDigitPos = -1;

            // Находим последнюю цифру
            for (int i = chars.Length - 1; i >= 0; i--)
            {
                if (char.IsDigit(chars[i]))
                {
                    lastDigitPos = i;
                    break;
                }
            }

            for (int i = 0; i < chars.Length; i++)
            {
                if (char.IsDigit(chars[i]))
                {
                    digitCount++;
                    // Оставляем первые 2 цифры и последние 2 цифры
                    if (digitCount > 2 && i < lastDigitPos - 1)
                    {
                        chars[i] = '*';
                    }
                }
            }
            return new string(chars);
        }

        private string MaskAddress(string address)
        {
            if (string.IsNullOrEmpty(address)) return "";

            // Показываем первую букву каждого слова и последнюю букву
            string[] words = address.Split(new[] { ' ', ',', '.' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 2 && !char.IsDigit(words[i][0]))
                {
                    // Улица -> У***а
                    words[i] = words[i][0] + new string('*', words[i].Length - 2) + words[i][words[i].Length - 1];
                }
                else if (words[i].Length == 2 && !char.IsDigit(words[i][0]))
                {
                    words[i] = words[i][0] + "*";
                }
            }
            return string.Join(" ", words);
        }

        // ===== МЕТОД ДЛЯ ПРОСМОТРА ДЕТАЛЕЙ ЗАКАЗА =====
        private void ButtonDetail_Click(object sender, EventArgs e)
        {
            ShowOrderDetails();
        }

        private void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            ShowOrderDetails();
        }

        private void ShowOrderDetails()
        {
            try
            {
                if (dataGridView1.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Выберите заказ для просмотра деталей!", "Предупреждение",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];

                int orderId = Convert.ToInt32(selectedRow.Cells["id_order"].Value);
                string clientName = selectedRow.Cells["name_client"].Value?.ToString() ?? "";
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
                detailForm.Text = $"Детали заказа №{orderId}";
                detailForm.Size = new Size(820, 750);
                detailForm.StartPosition = FormStartPosition.CenterParent;
                detailForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                detailForm.MaximizeBox = false;
                detailForm.MinimizeBox = false;
                detailForm.BackColor = Color.White;
                detailForm.AutoScroll = true;
                detailForm.Font = new Font("Times New Roman", 12, FontStyle.Regular);

                // Панель информации с ПОЛНЫМИ персональными данными (без маскировки)
                Panel infoPanel = CreateInfoPanel(orderId, clientName, phoneNumber, address,
                    persons, orderDate, paymentMethod);

                Panel statusPanel = CreateStatusPanel(currentStatusId, currentStatus, statusState);
                Panel commentPanel = CreateCommentPanel(comment);

                // ЗАГРУЖАЕМ ДЕТАЛИ ЗАКАЗА (блюда + подарки)
                List<OrderDetailItem> orderDetails = LoadOrderDetails(orderId);
                DataGridView dgvOrderDetails = CreateOrderDetailsDataGridView();

                // Заполняем DataGridView
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

        // ===== СОЗДАНИЕ ПАНЕЛИ С ИНФОРМАЦИЕЙ О ЗАКАЗЕ (ПОЛНЫЕ ДАННЫЕ) =====
        private Panel CreateInfoPanel(int orderId, string clientName, string phoneNumber,
            string address, int persons, DateTime orderDate, string paymentMethod)
        {
            Panel panel = new Panel();
            panel.Size = new Size(765, 130);
            panel.BorderStyle = BorderStyle.FixedSingle;
            panel.BackColor = Color.FromArgb(255, 255, 220); // Желтый фон для персональных данных

            Label lblInfo = new Label();
            lblInfo.Text = $"ЗАКАЗ №{orderId} (ПЕРСОНАЛЬНЫЕ ДАННЫЕ)\n" +
                          $"Клиент: {clientName}\n" +
                          $"Телефон: {phoneNumber}\n" +
                          $"Адрес: {address}\n" +
                          $"Количество персон: {persons} | Дата доставки: {orderDate:dd.MM.yyyy}\n" +
                          $"Способ оплаты: {paymentMethod}";
            lblInfo.Location = new Point(10, 10);
            lblInfo.Size = new Size(740, 110);
            lblInfo.Font = new Font("Times New Roman", 11, FontStyle.Bold);
            lblInfo.ForeColor = Color.DarkRed;
            lblInfo.TextAlign = ContentAlignment.TopLeft;
            lblInfo.BackColor = Color.Transparent;

            panel.Controls.Add(lblInfo);
            return panel;
        }

        // ===== ИСПРАВЛЕННЫЙ МЕТОД: ЗАГРУЗКА ДЕТАЛЕЙ ЗАКАЗА (БЛЮДА + ПОДАРКИ) =====
        private List<OrderDetailItem> LoadOrderDetails(int orderId)
        {
            List<OrderDetailItem> items = new List<OrderDetailItem>();

            try
            {
                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();

                    // Загружаем все позиции из order_dish (блюда и подарки)
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

        // ===== НОВЫЙ МЕТОД: СОЗДАНИЕ DataTable ДЛЯ ОТОБРАЖЕНИЯ ДЕТАЛЕЙ =====
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

        // ===== СОЗДАНИЕ ПАНЕЛИ ДЛЯ ИЗМЕНЕНИЯ СТАТУСА =====
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
            lblCurrentStatus.BackColor = Color.Transparent;

            Label lblCurrentStatusValue = new Label();
            lblCurrentStatusValue.Text = currentStatus;
            lblCurrentStatusValue.Location = new Point(120, 18);
            lblCurrentStatusValue.Size = new Size(150, 25);
            lblCurrentStatusValue.Font = new Font("Times New Roman", 11, FontStyle.Bold);
            lblCurrentStatusValue.ForeColor = Color.DarkBlue;
            lblCurrentStatusValue.TextAlign = ContentAlignment.MiddleLeft;
            lblCurrentStatusValue.BackColor = Color.Transparent;

            Label lblNewStatus = new Label();
            lblNewStatus.Text = "Новый статус:";
            lblNewStatus.Location = new Point(280, 18);
            lblNewStatus.Size = new Size(90, 25);
            lblNewStatus.Font = new Font("Times New Roman", 11, FontStyle.Regular);
            lblNewStatus.TextAlign = ContentAlignment.MiddleLeft;
            lblNewStatus.BackColor = Color.Transparent;

            ComboBox cmbNewStatus = new ComboBox();
            cmbNewStatus.Location = new Point(380, 18);
            cmbNewStatus.Size = new Size(200, 25);
            cmbNewStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbNewStatus.Font = new Font("Times New Roman", 11, FontStyle.Regular);
            cmbNewStatus.BackColor = Color.White;

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

        // ===== СОЗДАНИЕ ПАНЕЛИ С КОММЕНТАРИЕМ =====
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
            lblCommentTitle.BackColor = Color.Transparent;

            Label lblComment = new Label();
            lblComment.Text = string.IsNullOrEmpty(comment) ? "(нет комментария)" : comment;
            lblComment.Location = new Point(10, 30);
            lblComment.Size = new Size(740, 25);
            lblComment.Font = new Font("Times New Roman", 11, FontStyle.Regular);
            lblComment.TextAlign = ContentAlignment.MiddleLeft;
            lblComment.BackColor = Color.Transparent;
            lblComment.AutoEllipsis = true;

            panel.Controls.Add(lblCommentTitle);
            panel.Controls.Add(lblComment);

            return panel;
        }

        // ===== ПАНЕЛЬ ДЛЯ ИТОГОВОЙ СУММЫ =====
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
            lblTotalTitle.BackColor = Color.Transparent;

            Label lblTotalSum = new Label();
            lblTotalSum.Text = $"{totalSum.ToString("N2", russianCulture)} ₽";
            lblTotalSum.Location = new Point(100, 12);
            lblTotalSum.Size = new Size(200, 25);
            lblTotalSum.Font = new Font("Times New Roman", 14, FontStyle.Bold);
            lblTotalSum.ForeColor = Color.DarkRed;
            lblTotalSum.TextAlign = ContentAlignment.MiddleLeft;
            lblTotalSum.BackColor = Color.Transparent;

            panel.Controls.Add(lblTotalTitle);
            panel.Controls.Add(lblTotalSum);

            return panel;
        }

        // ===== СОЗДАНИЕ DATA GRID VIEW ДЛЯ БЛЮД =====
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

            // Скрытая колонка для флага подарка
            DataGridViewCheckBoxColumn colIsGift = new DataGridViewCheckBoxColumn();
            colIsGift.Name = "is_gift";
            colIsGift.DataPropertyName = "is_gift";
            colIsGift.Visible = false;
            dgv.Columns.Add(colIsGift);

            dgv.DataError += (s, e) => e.ThrowException = false;

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

            // Применяем стиль к строкам после загрузки данных
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

        // ===== МЕТОД ДЛЯ ОБНОВЛЕНИЯ СТАТУСА В БАЗЕ ДАННЫХ =====
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
            foreach (DataGridViewRow row in dataGridView1.Rows)
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
            if (string.IsNullOrWhiteSpace(searchText))
            {
                LoadOrdersWithFilter("", false);
            }
            else
            {
                string digits = new string(searchText.Where(char.IsDigit).ToArray());
                if (digits.Length > MAX_SEARCH_LENGTH)
                {
                    digits = digits.Substring(0, MAX_SEARCH_LENGTH);
                }

                if (digits.Length > 0)
                {
                    LoadOrdersWithFilter(digits, false);
                }
                else
                {
                    LoadOrdersWithFilter("", false);
                }
            }
        }

        private void InitializeDataGridView()
        {
            if (dataGridView1 == null)
            {
                MessageBox.Show("DataGridView не найден! Проверьте имя элемента управления.");
                return;
            }

            dataGridView1.ShowCellToolTips = false;
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.ReadOnly = true;
            dataGridView1.MultiSelect = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.RowTemplate.Height = 40;
            dataGridView1.RowTemplate.MinimumHeight = 40;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersHeight = 45;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            Color headerBackColor = Color.FromArgb(97, 173, 123);

            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = headerBackColor;
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.ColumnHeadersDefaultCellStyle.Padding = new Padding(0, 3, 0, 3);
            dataGridView1.ColumnHeadersDefaultCellStyle.SelectionBackColor = headerBackColor;
            dataGridView1.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.Black;

            dataGridView1.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            dataGridView1.DefaultCellStyle.Padding = new Padding(0, 2, 0, 2);
            dataGridView1.DefaultCellStyle.BackColor = Color.White;
            dataGridView1.DefaultCellStyle.ForeColor = Color.Black;
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.FromArgb(233, 242, 236);
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.Black;

            dataGridView1.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(233, 242, 236);
            dataGridView1.RowsDefaultCellStyle.SelectionForeColor = Color.Black;
            dataGridView1.RowsDefaultCellStyle.BackColor = Color.White;
            dataGridView1.RowsDefaultCellStyle.ForeColor = Color.Black;

            dataGridView1.GridColor = Color.Gray;
            dataGridView1.BorderStyle = BorderStyle.FixedSingle;
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.Single;

            dataGridView1.Columns.Clear();

            DataGridViewTextBoxColumn colId = new DataGridViewTextBoxColumn();
            colId.Name = "id_order";
            colId.HeaderText = "№";
            colId.DataPropertyName = "id_order";
            colId.Width = 50;
            colId.MinimumWidth = 50;
            colId.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colId.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            colId.Resizable = DataGridViewTriState.True;
            colId.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns.Add(colId);

            DataGridViewTextBoxColumn colClient = new DataGridViewTextBoxColumn();
            colClient.Name = "name_client";
            colClient.HeaderText = "Клиент";
            colClient.DataPropertyName = "name_client";
            colClient.Width = 180;
            colClient.MinimumWidth = 150;
            colClient.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            colClient.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colClient.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            colClient.Resizable = DataGridViewTriState.True;
            colClient.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns.Add(colClient);

            DataGridViewTextBoxColumn colPhone = new DataGridViewTextBoxColumn();
            colPhone.Name = "phone_number";
            colPhone.HeaderText = "Телефон";
            colPhone.DataPropertyName = "phone_number";
            colPhone.Width = 120;
            colPhone.MinimumWidth = 120;
            colPhone.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            colPhone.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colPhone.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            colPhone.Resizable = DataGridViewTriState.True;
            colPhone.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns.Add(colPhone);

            DataGridViewTextBoxColumn colAddress = new DataGridViewTextBoxColumn();
            colAddress.Name = "address";
            colAddress.HeaderText = "Адрес";
            colAddress.DataPropertyName = "address";
            colAddress.Width = 300;
            colAddress.MinimumWidth = 250;
            colAddress.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            colAddress.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colAddress.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            colAddress.Resizable = DataGridViewTriState.True;
            colAddress.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns.Add(colAddress);

            DataGridViewTextBoxColumn colPersons = new DataGridViewTextBoxColumn();
            colPersons.Name = "number_persons";
            colPersons.HeaderText = "Персон";
            colPersons.DataPropertyName = "number_persons";
            colPersons.Width = 70;
            colPersons.MinimumWidth = 70;
            colPersons.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colPersons.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            colPersons.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            colPersons.Resizable = DataGridViewTriState.True;
            colPersons.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns.Add(colPersons);

            DataGridViewTextBoxColumn colDate = new DataGridViewTextBoxColumn();
            colDate.Name = "delivery_date";
            colDate.HeaderText = "Дата доставки";
            colDate.DataPropertyName = "delivery_date";
            colDate.Width = 100;
            colDate.MinimumWidth = 90;
            colDate.DefaultCellStyle.Format = "dd.MM.yyyy";
            colDate.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colDate.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            colDate.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            colDate.Resizable = DataGridViewTriState.True;
            colDate.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns.Add(colDate);

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
            dataGridView1.Columns.Add(colComment);

            DataGridViewTextBoxColumn colPayment = new DataGridViewTextBoxColumn();
            colPayment.Name = "payment_method";
            colPayment.HeaderText = "Оплата";
            colPayment.DataPropertyName = "payment_method";
            colPayment.Width = 100;
            colPayment.MinimumWidth = 90;
            colPayment.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            colPayment.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colPayment.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            colPayment.Resizable = DataGridViewTriState.True;
            colPayment.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns.Add(colPayment);

            DataGridViewTextBoxColumn colStatusId = new DataGridViewTextBoxColumn();
            colStatusId.Name = "id_status";
            colStatusId.HeaderText = "ID статуса";
            colStatusId.DataPropertyName = "id_status";
            colStatusId.Visible = false;
            colStatusId.Width = 50;
            colStatusId.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns.Add(colStatusId);

            DataGridViewTextBoxColumn colStatusName = new DataGridViewTextBoxColumn();
            colStatusName.Name = "status_name";
            colStatusName.HeaderText = "Статус";
            colStatusName.DataPropertyName = "status_name";
            colStatusName.Width = 120;
            colStatusName.MinimumWidth = 100;
            colStatusName.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            colStatusName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colStatusName.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            colStatusName.Resizable = DataGridViewTriState.True;
            colStatusName.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns.Add(colStatusName);

            dataGridView1.ScrollBars = ScrollBars.Both;
            dataGridView1.Columns[dataGridView1.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.CellFormatting += DataGridView1_CellFormatting;
        }

        // ===== ФОРМАТИРОВАНИЕ ЯЧЕЕК ДЛЯ МАСКИРОВКИ =====
        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex].Name == "delivery_date" && e.RowIndex >= 0)
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

            // ЧАСТИЧНАЯ МАСКИРОВКА персональных данных
            if (e.RowIndex >= 0 && e.Value != null)
            {
                string columnName = dataGridView1.Columns[e.ColumnIndex].Name;

                if (columnName == "name_client")
                {
                    e.Value = MaskFullName(e.Value.ToString());
                    e.FormattingApplied = true;
                }
                else if (columnName == "phone_number")
                {
                    e.Value = MaskPhone(e.Value.ToString());
                    e.FormattingApplied = true;
                }
                else if (columnName == "address")
                {
                    e.Value = MaskAddress(e.Value.ToString());
                    e.FormattingApplied = true;
                }
            }
        }

        private void SetupColumnStyles()
        {
            if (dataGridView1.Columns.Count > 0)
            {
                Color selectionColor = Color.FromArgb(233, 242, 236);

                foreach (DataGridViewColumn col in dataGridView1.Columns)
                {
                    if (col.Name != "id_status" && col.Visible)
                    {
                        col.DefaultCellStyle.SelectionBackColor = selectionColor;
                        col.DefaultCellStyle.SelectionForeColor = Color.Black;
                    }
                }
            }
        }

        private void textBoxSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void textBoxSearch_TextChanged(object sender, EventArgs e)
        {
            if (isFormatting) return;

            isFormatting = true;

            string searchText = textBoxSearch.Text;
            string digits = new string(searchText.Where(char.IsDigit).ToArray());

            if (digits.Length > MAX_SEARCH_LENGTH)
            {
                digits = digits.Substring(0, MAX_SEARCH_LENGTH);
                textBoxSearch.Text = digits;
                textBoxSearch.SelectionStart = digits.Length;
            }

            if (digits.Length > 0)
            {
                LoadOrdersWithFilter(digits, false);
            }
            else
            {
                LoadOrdersWithFilter("", false);
            }

            isFormatting = false;
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
                            comboBoxOrderStatus.Items.Clear();
                            comboBoxOrderStatus.Items.Add("Все статусы");
                            statusDictionary.Clear();

                            while (reader.Read())
                            {
                                int id = reader.GetInt32("id_status");
                                string name = reader.GetString("status_name");

                                statusDictionary.Add(id, name);

                                if (id != DELIVERED_STATUS_ID)
                                {
                                    comboBoxOrderStatus.Items.Add(new StatusItem(id, name));
                                }
                            }
                        }
                    }
                }

                comboBoxOrderStatus.DisplayMember = "Name";
                comboBoxOrderStatus.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статусов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadOrdersWithFilter(string orderNumber = "", bool exactMatch = false)
        {
            int statusId = -1;
            if (comboBoxOrderStatus.SelectedIndex > 0 && comboBoxOrderStatus.SelectedItem is StatusItem statusItem)
            {
                statusId = statusItem.Id;
            }

            LoadOrders(orderNumber, statusId, exactMatch);
        }

        private void LoadOrders(string orderNumber = "", int statusId = -1, bool exactMatch = false)
        {
            try
            {
                string query = @"
                    SELECT o.id_order, o.name_client, o.phone_number, o.address, 
                           o.number_persons, o.delivery_date, o.comment, 
                           o.payment_method, o.id_status,
                           s.status_name
                    FROM orders o
                    LEFT JOIN order_statuses s ON o.id_status = s.id_status
                    WHERE 1=1";

                List<MySqlParameter> parameters = new List<MySqlParameter>();

                if (!string.IsNullOrEmpty(orderNumber) && orderNumber.All(char.IsDigit))
                {
                    if (exactMatch)
                    {
                        query += " AND o.id_order = @OrderNumber";
                        parameters.Add(new MySqlParameter("@OrderNumber", orderNumber));
                    }
                    else
                    {
                        query += " AND CAST(o.id_order AS CHAR) LIKE @OrderNumber";
                        parameters.Add(new MySqlParameter("@OrderNumber", orderNumber + "%"));
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
                        dataGridView1.DataSource = bindingSource;
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
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                if (col.Name != "id_order" && col.Name != "number_persons" &&
                    col.Name != "id_status")
                {
                    col.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                }
            }

            dataGridView1.Refresh();
        }

        private void comboBoxStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            string searchText = textBoxSearch.Text;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                LoadOrdersWithFilter("", false);
            }
            else
            {
                string digits = new string(searchText.Where(char.IsDigit).ToArray());
                if (digits.Length > MAX_SEARCH_LENGTH)
                {
                    digits = digits.Substring(0, MAX_SEARCH_LENGTH);
                }

                if (digits.Length > 0)
                {
                    LoadOrdersWithFilter(digits, false);
                }
                else
                {
                    LoadOrdersWithFilter("", false);
                }
            }
        }

        public class StatusItem
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public StatusItem(int id, string name)
            {
                Id = id;
                Name = name;
            }

            public override string ToString()
            {
                return Name;
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            ManagerForm Manager = new ManagerForm();
            Manager.Show();
        }

        private void Orders_Load(object sender, EventArgs e) { }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            ResetFilters();
        }

        private void ResetFilters()
        {
            try
            {
                textBoxSearch.Text = "";
                textBoxSearch.ForeColor = Color.Black;
                comboBoxOrderStatus.SelectedIndex = 0;
                LoadOrders();
                textBoxSearch.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сбросе фильтров: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadOrders()
        {
            LoadOrders("", -1, false);
        }
    }
}