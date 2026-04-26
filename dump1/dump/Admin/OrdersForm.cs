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
        private const int MAX_SEARCH_LENGTH = 11;

        // Культура для форматирования
        private CultureInfo russianCulture = new CultureInfo("ru-RU");

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

            // ПОДПИСЫВАЕМСЯ НА СОБЫТИЕ ЗАКРЫТИЯ ФОРМЫ
            this.FormClosing += OrdersForm_FormClosing;
        }

        // НОВЫЙ ОБРАБОТЧИК - при нажатии на крестик
        private void OrdersForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Проверяем, что закрытие не было вызвано из кода
            if (e.CloseReason == CloseReason.UserClosing)
            {
                // Отменяем закрытие формы
                e.Cancel = true;

                // Скрываем текущую форму
                this.Visible = false;

                // Открываем AdminForm
                AdminForm admin = new AdminForm();
                admin.Show();
            }
        }

        private void InitializeComponents()
        {
            // Инициализация DataGridView
            InitializeDataGridView();

            // Настройка textBoxSearch
            textBoxSearch.TextChanged += textBoxSearch_TextChanged;
            textBoxSearch.KeyPress += textBoxSearch_KeyPress;
            textBoxSearch.Enter += textBoxSearch_Enter;
            textBoxSearch.Leave += textBoxSearch_Leave;

            // Устанавливаем максимальную длину ввода
            textBoxSearch.MaxLength = MAX_SEARCH_LENGTH;

            // Убираем подсказку - просто пустое поле
            textBoxSearch.Text = "";
            textBoxSearch.ForeColor = Color.Black;

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

            buttonReset.MouseDown += (s, e) =>
            {
                buttonReset.FlatAppearance.BorderColor = Color.DarkBlue;
            };

            buttonReset.MouseUp += (s, e) =>
            {
                buttonReset.FlatAppearance.BorderColor = Color.Black;
            };
            buttonReset.MouseLeave += (s, e) =>
            {
                buttonReset.FlatAppearance.BorderColor = Color.Black;
            };

            // Настройка кнопки деталей заказа
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

        // ===== МЕТОД ДЛЯ ПРОСМОТРА ДЕТАЛЕЙ ЗАКАЗА И ИЗМЕНЕНИЯ СТАТУСА =====
        private void ButtonDetail_Click(object sender, EventArgs e)
        {
            try
            {
                // Проверяем, выбран ли какой-нибудь заказ
                if (dgvOrders.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Выберите заказ для просмотра деталей!", "Предупреждение",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Получаем выбранную строку
                DataGridViewRow selectedRow = dgvOrders.SelectedRows[0];

                // Получаем данные заказа
                int orderId = Convert.ToInt32(selectedRow.Cells["id_order"].Value);
                string clientName = selectedRow.Cells["name_client"].Value?.ToString() ?? "";
                string phoneNumber = selectedRow.Cells["phone_number"].Value?.ToString() ?? "";
                string address = selectedRow.Cells["address"].Value?.ToString() ?? "";
                int persons = Convert.ToInt32(selectedRow.Cells["number_persons"].Value ?? 0);
                DateTime orderDate = selectedRow.Cells["delivery_date"].Value != null ?
                    Convert.ToDateTime(selectedRow.Cells["delivery_date"].Value) : DateTime.Now;
                string comment = selectedRow.Cells["comment"].Value?.ToString() ?? "";
                string paymentMethod = selectedRow.Cells["payment_method"].Value?.ToString() ?? "";
                decimal totalAmount = selectedRow.Cells["total_amount"].Value != null ?
                    Convert.ToDecimal(selectedRow.Cells["total_amount"].Value) : 0;
                int currentStatusId = Convert.ToInt32(selectedRow.Cells["id_status"].Value ?? 0);
                string currentStatus = selectedRow.Cells["status_name"].Value?.ToString() ?? "";

                // Создаем объект для хранения состояния статуса
                StatusState statusState = new StatusState
                {
                    SelectedStatusId = currentStatusId,
                    SelectedStatusName = currentStatus
                };

                // Создаем форму для отображения деталей заказа
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

                // === ВЕРХНЯЯ ПАНЕЛЬ С ИНФОРМАЦИЕЙ О ЗАКАЗЕ ===
                Panel infoPanel = CreateInfoPanel(orderId, clientName, phoneNumber, address,
                    persons, orderDate, paymentMethod, totalAmount);

                // === ПАНЕЛЬ ДЛЯ ИЗМЕНЕНИЯ СТАТУСА ===
                Panel statusPanel = CreateStatusPanel(currentStatusId, currentStatus, statusState);

                // === ПАНЕЛЬ С КОММЕНТАРИЕМ ===
                Panel commentPanel = CreateCommentPanel(comment);

                // === ТАБЛИЦА С БЛЮДАМИ И ПОДАРКАМИ ===
                DataGridView dgvOrderDetails = CreateOrderDetailsDataGridView();

                // Загружаем детали заказа (блюда + подарки)
                List<OrderDetailItem> orderDetails = LoadOrderDetails(orderId);
                DataTable dt = CreateOrderDetailsDataTable(orderDetails);
                dgvOrderDetails.DataSource = dt;

                // Размещаем элементы на форме
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

                // Обработчик закрытия формы
                detailForm.FormClosing += (s, args) =>
                {
                    // Проверяем, изменился ли статус
                    if (statusState.SelectedStatusId != currentStatusId)
                    {
                        DialogResult result = MessageBox.Show(
                            $"Изменить статус заказа с \"{currentStatus}\" на \"{statusState.SelectedStatusName}\"?",
                            "Сохранение изменений",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question);

                        if (result == DialogResult.Yes)
                        {
                            // Сохраняем изменения
                            if (UpdateOrderStatus(orderId, statusState.SelectedStatusId, statusState.SelectedStatusName))
                            {
                                MessageBox.Show("Статус успешно обновлен!", "Успех",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        else if (result == DialogResult.Cancel)
                        {
                            // Отменяем закрытие формы
                            args.Cancel = true;
                        }
                        // При выборе No просто закрываем форму без сохранения
                    }
                };

                // Показываем форму
                detailForm.ShowDialog(this);

                // После закрытия формы обновляем основную таблицу
                RefreshOrdersData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке деталей заказа: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===== НОВЫЙ МЕТОД: ЗАГРУЗКА ДЕТАЛЕЙ ЗАКАЗА (БЛЮДА + ПОДАРКИ) =====
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

        // ===== СОЗДАНИЕ ПАНЕЛИ С ИНФОРМАЦИЕЙ О ЗАКАЗЕ =====
        private Panel CreateInfoPanel(int orderId, string clientName, string phoneNumber,
            string address, int persons, DateTime orderDate, string paymentMethod, decimal totalAmount)
        {
            Panel panel = new Panel();
            panel.Size = new Size(765, 130);
            panel.BorderStyle = BorderStyle.FixedSingle;
            panel.BackColor = Color.FromArgb(240, 240, 240);

            // Создаем Label с информацией о заказе
            Label lblInfo = new Label();
            lblInfo.Text = $"Заказ №{orderId}\n" +
                          $"Клиент: {clientName}\n" +
                          $"Телефон: {phoneNumber}\n" +
                          $"Адрес: {address}\n" +
                          $"Количество персон: {persons} | Дата доставки: {orderDate:dd.MM.yyyy}\n" +
                          $"Способ оплаты: {paymentMethod}";
            lblInfo.Location = new Point(10, 10);
            lblInfo.Size = new Size(740, 110);
            lblInfo.Font = new Font("Times New Roman", 11, FontStyle.Regular);
            lblInfo.TextAlign = ContentAlignment.TopLeft;

            panel.Controls.Add(lblInfo);
            return panel;
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

            // Загружаем статусы в ComboBox
            foreach (var status in statusDictionary)
            {
                cmbNewStatus.Items.Add(new StatusItem(status.Key, status.Value));
            }
            cmbNewStatus.DisplayMember = "Name";

            // Устанавливаем текущий статус как выбранный
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

            // Обработчик изменения выбора в комбобоксе
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

        // ===== СОЗДАНИЕ DATA GRID VIEW ДЛЯ БЛЮД И ПОДАРКОВ =====
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

            // Важно! Устанавливаем AutoSizeColumnsMode для растягивания колонок
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Настройка стилей
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

            // Колонка Название - будет занимать 50% ширины
            DataGridViewTextBoxColumn colDishName = new DataGridViewTextBoxColumn();
            colDishName.Name = "dish_name";
            colDishName.HeaderText = "Наименование";
            colDishName.DataPropertyName = "dish_name";
            colDishName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colDishName.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            colDishName.FillWeight = 50;
            dgv.Columns.Add(colDishName);

            // Колонка Количество - будет занимать 15% ширины
            DataGridViewTextBoxColumn colQuantity = new DataGridViewTextBoxColumn();
            colQuantity.Name = "quantity";
            colQuantity.HeaderText = "Кол-во";
            colQuantity.DataPropertyName = "quantity";
            colQuantity.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colQuantity.FillWeight = 15;
            dgv.Columns.Add(colQuantity);

            // Колонка Цена - будет занимать 15% ширины
            DataGridViewTextBoxColumn colPrice = new DataGridViewTextBoxColumn();
            colPrice.Name = "price";
            colPrice.HeaderText = "Цена";
            colPrice.DataPropertyName = "price";
            colPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colPrice.FillWeight = 15;
            dgv.Columns.Add(colPrice);

            // Колонка Сумма - будет занимать 20% ширины
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
                            // Обновляем отображение в основной таблице
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

        // ===== МЕТОД ДЛЯ ОБНОВЛЕНИЯ СТАТУСА В ОСНОВНОЙ ТАБЛИЦЕ =====
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

        // ===== МЕТОД ДЛЯ ОБНОВЛЕНИЯ ДАННЫХ В ОСНОВНОЙ ТАБЛИЦЕ =====
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

        // ===== ИНИЦИАЛИЗАЦИЯ DATA GRID VIEW =====
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

            // Отключаем AutoSizeColumnsMode, чтобы работали ручные настройки ширины
            dgvOrders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            // Включаем перенос текста в ячейках
            dgvOrders.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            // Автоматическая высота строк на основе содержимого
            dgvOrders.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvOrders.RowTemplate.Height = 40;
            dgvOrders.RowTemplate.MinimumHeight = 40;

            // Скрываем заголовки строк (серые ячейки слева)
            dgvOrders.RowHeadersVisible = false;

            // Отключаем стандартные стили для кастомной настройки шапки
            dgvOrders.EnableHeadersVisualStyles = false;

            // Настройка высоты шапки
            dgvOrders.ColumnHeadersHeight = 45;
            dgvOrders.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            // Цвет шапки как в AdminMenu
            Color headerBackColor = Color.FromArgb(97, 173, 123);

            // Настройка стиля заголовков колонок
            dgvOrders.ColumnHeadersDefaultCellStyle.BackColor = headerBackColor;
            dgvOrders.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvOrders.ColumnHeadersDefaultCellStyle.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            dgvOrders.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvOrders.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvOrders.ColumnHeadersDefaultCellStyle.Padding = new Padding(0, 3, 0, 3);
            dgvOrders.ColumnHeadersDefaultCellStyle.SelectionBackColor = headerBackColor;
            dgvOrders.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.Black;

            // Настройка строк таблицы
            dgvOrders.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            dgvOrders.DefaultCellStyle.Padding = new Padding(0, 2, 0, 2);
            dgvOrders.DefaultCellStyle.BackColor = Color.White;
            dgvOrders.DefaultCellStyle.ForeColor = Color.Black;
            dgvOrders.DefaultCellStyle.SelectionBackColor = Color.FromArgb(233, 242, 236);
            dgvOrders.DefaultCellStyle.SelectionForeColor = Color.Black;

            // Настройка строк при выделении
            dgvOrders.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(233, 242, 236);
            dgvOrders.RowsDefaultCellStyle.SelectionForeColor = Color.Black;
            dgvOrders.RowsDefaultCellStyle.BackColor = Color.White;
            dgvOrders.RowsDefaultCellStyle.ForeColor = Color.Black;

            // Настройка внешнего вида
            dgvOrders.GridColor = Color.Gray;
            dgvOrders.BorderStyle = BorderStyle.FixedSingle;
            dgvOrders.CellBorderStyle = DataGridViewCellBorderStyle.Single;

            dgvOrders.Columns.Clear();

            // === КОЛОНКА №1: НОМЕР ЗАКАЗА ===
            DataGridViewTextBoxColumn colId = new DataGridViewTextBoxColumn();
            colId.Name = "id_order";
            colId.HeaderText = "№";
            colId.DataPropertyName = "id_order";
            colId.Width = 50;
            colId.MinimumWidth = 50;
            colId.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colId.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            colId.Resizable = DataGridViewTriState.True;
            colId.HeaderCell.Style.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            colId.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colId.HeaderCell.Style.BackColor = headerBackColor;
            colId.HeaderCell.Style.ForeColor = Color.Black;
            colId.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colId);

            // === КОЛОНКА №2: КЛИЕНТ ===
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
            colClient.HeaderCell.Style.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            colClient.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colClient.HeaderCell.Style.BackColor = headerBackColor;
            colClient.HeaderCell.Style.ForeColor = Color.Black;
            colClient.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colClient);

            // === КОЛОНКА №3: ТЕЛЕФОН ===
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
            colPhone.HeaderCell.Style.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            colPhone.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colPhone.HeaderCell.Style.BackColor = headerBackColor;
            colPhone.HeaderCell.Style.ForeColor = Color.Black;
            colPhone.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colPhone);

            // === КОЛОНКА №4: АДРЕС ===
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
            colAddress.HeaderCell.Style.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            colAddress.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colAddress.HeaderCell.Style.BackColor = headerBackColor;
            colAddress.HeaderCell.Style.ForeColor = Color.Black;
            colAddress.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colAddress);

            // === КОЛОНКА №5: КОЛИЧЕСТВО ПЕРСОН ===
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
            colPersons.HeaderCell.Style.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            colPersons.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colPersons.HeaderCell.Style.BackColor = headerBackColor;
            colPersons.HeaderCell.Style.ForeColor = Color.Black;
            colPersons.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colPersons);

            // === КОЛОНКА №6: ДАТА ===
            DataGridViewTextBoxColumn colDate = new DataGridViewTextBoxColumn();
            colDate.Name = "delivery_date";
            colDate.HeaderText = "Дата";
            colDate.DataPropertyName = "delivery_date";
            colDate.Width = 85;
            colDate.MinimumWidth = 80;
            colDate.DefaultCellStyle.Format = "dd.MM.yyyy";
            colDate.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colDate.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            colDate.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            colDate.Resizable = DataGridViewTriState.True;
            colDate.HeaderCell.Style.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            colDate.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colDate.HeaderCell.Style.BackColor = headerBackColor;
            colDate.HeaderCell.Style.ForeColor = Color.Black;
            colDate.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colDate);

            // === КОЛОНКА №7: КОММЕНТАРИЙ ===
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
            colComment.HeaderCell.Style.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            colComment.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colComment.HeaderCell.Style.BackColor = headerBackColor;
            colComment.HeaderCell.Style.ForeColor = Color.Black;
            colComment.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colComment);

            // === КОЛОНКА №8: ОПЛАТА ===
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
            colPayment.HeaderCell.Style.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            colPayment.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colPayment.HeaderCell.Style.BackColor = headerBackColor;
            colPayment.HeaderCell.Style.ForeColor = Color.Black;
            colPayment.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colPayment);

            // === КОЛОНКА №9: СТОИМОСТЬ ===
            DataGridViewTextBoxColumn colTotalAmount = new DataGridViewTextBoxColumn();
            colTotalAmount.Name = "total_amount";
            colTotalAmount.HeaderText = "Стоимость";
            colTotalAmount.DataPropertyName = "total_amount";
            colTotalAmount.Width = 100;
            colTotalAmount.MinimumWidth = 90;
            colTotalAmount.DefaultCellStyle.Format = "N2";
            colTotalAmount.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colTotalAmount.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            colTotalAmount.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            colTotalAmount.DefaultCellStyle.ForeColor = Color.DarkGreen;
            colTotalAmount.Resizable = DataGridViewTriState.True;
            colTotalAmount.HeaderCell.Style.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            colTotalAmount.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colTotalAmount.HeaderCell.Style.BackColor = headerBackColor;
            colTotalAmount.HeaderCell.Style.ForeColor = Color.Black;
            colTotalAmount.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colTotalAmount);

            // === СКРЫТАЯ КОЛОНКА ДЛЯ ХРАНЕНИЯ ID СТАТУСА ===
            DataGridViewTextBoxColumn colStatusId = new DataGridViewTextBoxColumn();
            colStatusId.Name = "id_status";
            colStatusId.HeaderText = "ID статуса";
            colStatusId.DataPropertyName = "id_status";
            colStatusId.Visible = false;
            colStatusId.Width = 50;
            colStatusId.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colStatusId);

            // === КОЛОНКА ДЛЯ ОТОБРАЖЕНИЯ НАЗВАНИЯ СТАТУСА ===
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
            colStatusName.HeaderCell.Style.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            colStatusName.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colStatusName.HeaderCell.Style.BackColor = headerBackColor;
            colStatusName.HeaderCell.Style.ForeColor = Color.Black;
            colStatusName.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvOrders.Columns.Add(colStatusName);

            // Добавляем возможность горизонтальной прокрутки
            dgvOrders.ScrollBars = ScrollBars.Both;

            // Устанавливаем режим заполнения для последней колонки
            dgvOrders.Columns[dgvOrders.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            // Подписка на событие форматирования ячеек
            dgvOrders.CellFormatting += DgvOrders_CellFormatting;
        }

        // ===== ФОРМАТИРОВАНИЕ ЯЧЕЕК DATA GRID VIEW =====
        private void DgvOrders_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Форматирование стоимости
            if (dgvOrders.Columns[e.ColumnIndex].Name == "total_amount" && e.RowIndex >= 0)
            {
                if (e.Value != null && e.Value != DBNull.Value)
                {
                    decimal amount = Convert.ToDecimal(e.Value);
                    e.Value = amount.ToString("N2", russianCulture) + " ₽";
                    e.FormattingApplied = true;
                }
            }

            // Форматирование даты
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
        }

        // ===== МЕТОД ДЛЯ НАСТРОЙКИ СТИЛЕЙ КОЛОНОК ПОСЛЕ ЗАГРУЗКИ ДАННЫХ =====
        private void SetupColumnStyles()
        {
            if (dgvOrders.Columns.Count > 0)
            {
                Color selectionColor = Color.FromArgb(233, 242, 236);

                // Настройка для всех колонок
                foreach (DataGridViewColumn col in dgvOrders.Columns)
                {
                    if (col.Name != "id_status" && col.Visible)
                    {
                        col.DefaultCellStyle.SelectionBackColor = selectionColor;
                        col.DefaultCellStyle.SelectionForeColor = Color.Black;
                    }
                }

                // Специальная настройка для колонки "Стоимость"
                if (dgvOrders.Columns["total_amount"] != null)
                {
                    dgvOrders.Columns["total_amount"].DefaultCellStyle.ForeColor = Color.DarkGreen;
                    dgvOrders.Columns["total_amount"].DefaultCellStyle.SelectionForeColor = Color.DarkGreen;
                }
            }
        }

        // ===== МЕТОДЫ ДЛЯ ПОИСКА ПО НОМЕРУ ЗАКАЗА =====
        private void textBoxSearch_Enter(object sender, EventArgs e)
        {
            // Ничего не делаем
        }

        private void textBoxSearch_Leave(object sender, EventArgs e)
        {
            // Ничего не делаем
        }

        private void textBoxSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем только цифры и управляющие символы
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void textBoxSearch_TextChanged(object sender, EventArgs e)
        {
            // Защита от рекурсии
            if (isFormatting) return;

            isFormatting = true;

            // Получаем введенные цифры
            string searchText = textBoxSearch.Text;
            string digits = new string(searchText.Where(char.IsDigit).ToArray());

            // Ограничиваем длину до MAX_SEARCH_LENGTH
            if (digits.Length > MAX_SEARCH_LENGTH)
            {
                digits = digits.Substring(0, MAX_SEARCH_LENGTH);
                // Обновляем текст в поле, если он превышает лимит
                textBoxSearch.Text = digits;
                textBoxSearch.SelectionStart = digits.Length;
            }

            // Если есть цифры - ищем по НАЧАЛУ НОМЕРА
            if (digits.Length > 0)
            {
                // Передаем exactMatch = false для поиска по началу строки
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
                            comboBoxStatus.Items.Clear();
                            comboBoxStatus.Items.Add("Все статусы");

                            // Очищаем словарь и добавляем в него статусы
                            statusDictionary.Clear();

                            while (reader.Read())
                            {
                                int id = reader.GetInt32("id_status");
                                string name = reader.GetString("status_name");

                                // Добавляем в словарь
                                statusDictionary.Add(id, name);

                                // Добавляем в комбобокс
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

        private void LoadOrdersWithFilter(string orderNumber = "", bool exactMatch = false)
        {
            // Получаем выбранный статус
            int statusId = -1;
            if (comboBoxStatus.SelectedIndex > 0 && comboBoxStatus.SelectedItem is StatusItem statusItem)
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
                    SELECT o.id_order, o.phone_number, o.address, 
                           o.number_persons, o.delivery_date, o.comment, 
                           o.payment_method, o.id_status, o.name_client,
                           s.status_name, o.total_amount
                    FROM orders o
                    LEFT JOIN order_statuses s ON o.id_status = s.id_status
                    WHERE 1=1";

                List<MySqlParameter> parameters = new List<MySqlParameter>();

                // ФИЛЬТР ПО НОМЕРУ ЗАКАЗА
                if (!string.IsNullOrEmpty(orderNumber) && orderNumber.All(char.IsDigit))
                {
                    if (exactMatch)
                    {
                        // Точное совпадение
                        query += " AND o.id_order = @OrderNumber";
                        parameters.Add(new MySqlParameter("@OrderNumber", orderNumber));
                    }
                    else
                    {
                        // Поиск по НАЧАЛУ номера (номера, начинающиеся с введенных цифр)
                        query += " AND CAST(o.id_order AS CHAR) LIKE @OrderNumber";
                        // Добавляем % в конец для поиска по началу строки
                        parameters.Add(new MySqlParameter("@OrderNumber", orderNumber + "%"));
                    }
                }

                // Фильтр по статусу
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

                    // Применяем стили для колонок после загрузки данных
                    SetupColumnStyles();

                    // Настройка высоты строк после загрузки данных
                    AdjustDataGridViewAfterLoad();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===== МЕТОД ДЛЯ НАСТРОЙКИ ПОСЛЕ ЗАГРУЗКИ ДАННЫХ =====
        private void AdjustDataGridViewAfterLoad()
        {
            // Устанавливаем режим автоподбора высоты строк
            dgvOrders.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            // Разрешаем перенос текста для всех текстовых колонок
            foreach (DataGridViewColumn col in dgvOrders.Columns)
            {
                if (col.Name != "id_order" && col.Name != "number_persons" &&
                    col.Name != "id_status" && col.Name != "total_amount")
                {
                    col.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                }
            }

            // Обновляем отображение
            dgvOrders.Refresh();
        }

        private void comboBoxStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            // При изменении статуса обновляем данные с текущим фильтром
            string searchText = textBoxSearch.Text;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                LoadOrdersWithFilter("", false);
            }
            else
            {
                string digits = new string(searchText.Where(char.IsDigit).ToArray());
                // Ограничиваем длину до MAX_SEARCH_LENGTH
                if (digits.Length > MAX_SEARCH_LENGTH)
                {
                    digits = digits.Substring(0, MAX_SEARCH_LENGTH);
                }

                if (digits.Length > 0)
                {
                    // Ищем по НАЧАЛУ номера (не точное совпадение)
                    LoadOrdersWithFilter(digits, false);
                }
                else
                {
                    LoadOrdersWithFilter("", false);
                }
            }
        }

        // Вспомогательный класс для хранения статусов
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
            AdminForm admin = new AdminForm();
            admin.Show();
        }

        private void OrdersForm_Load(object sender, EventArgs e)
        {
            // Уже инициализировано в InitializeComponents()
        }

        private void LoadOrders()
        {
            LoadOrders("", -1, false);
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            // Сбрасываем поле поиска - просто очищаем
            textBoxSearch.Text = "";
            textBoxSearch.ForeColor = Color.Black;

            // Сбрасываем комбобокс статуса на "Все статусы"
            comboBoxStatus.SelectedIndex = 0;

            // Загружаем все заказы без фильтров
            LoadOrders("", -1, false);
        }
    }
}