using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Globalization;

namespace dump
{
    public partial class EmloyForm : Form
    {
        private DataTable certificatesTable;
        private BindingSource bindingSource;
        private MySqlDataAdapter dataAdapter;
        private bool isFormatting = false;

        // Словарь для хранения статусов сертификатов
        private Dictionary<int, string> statusDictionary = new Dictionary<int, string>();

        // Максимальное количество символов для поиска
        private const int MAX_SEARCH_LENGTH = 11;

        // Культура для форматирования
        private CultureInfo russianCulture = new CultureInfo("ru-RU");

        public EmloyForm()
        {
            InitializeComponent();
            InitializeComponents();

            this.FormClosing += EmloyForm_FormClosing;
        }

        private void EmloyForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Visible = false;
                ManagerForm manager = new ManagerForm();
                manager.Show();
            }
        }

        private void InitializeComponents()
        {
            InitializeDataGridView();

            textBoxSearch.TextChanged += textBoxSearch_TextChanged;
            textBoxSearch.KeyPress += textBoxSearch_KeyPress;
            textBoxSearch.Enter += textBoxSearch_Enter;
            textBoxSearch.Leave += textBoxSearch_Leave;

            textBoxSearch.MaxLength = MAX_SEARCH_LENGTH;
            textBoxSearch.Text = "";
            textBoxSearch.ForeColor = Color.Black;

            comboBoxStatusSert.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxStatusSert.SelectedIndexChanged += comboBoxStatusSert_SelectedIndexChanged;

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

            // Добавляем контекстное меню при клике на строку
            dataGridView1.CellMouseClick += DataGridView1_CellMouseClick;
            LoadStatusesToComboBox();
            LoadCertificates();
        }

        // ===== КОНТЕКСТНОЕ МЕНЮ =====
        private void DataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.Button != MouseButtons.Right) return;

            dataGridView1.ClearSelection();
            dataGridView1.Rows[e.RowIndex].Selected = true;

            int certificateId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["id_certificate"].Value);
            string currentStatus = dataGridView1.Rows[e.RowIndex].Cells["status_name"].Value?.ToString() ?? "";

            ContextMenuStrip contextMenu = new ContextMenuStrip();

            // Пункт "Поставить статус 'Возвращён'"
            ToolStripMenuItem changeStatusItem = new ToolStripMenuItem("Поставить статус 'Возвращён'");
            changeStatusItem.Click += (s, ev) => ChangeStatusToReturned(certificateId, currentStatus);
            contextMenu.Items.Add(changeStatusItem);

            // Пункт "Детальная информация"
            ToolStripMenuItem detailsItem = new ToolStripMenuItem("Детальная информация");
            detailsItem.Click += (s, ev) => ShowCertificateDetails(certificateId);
            contextMenu.Items.Add(detailsItem);

            contextMenu.Show(dataGridView1, dataGridView1.PointToClient(Cursor.Position));
        }

        // ===== ИЗМЕНЕНИЕ СТАТУСА НА "ВОЗВРАЩЁН" =====
        private void ChangeStatusToReturned(int certificateId, string currentStatus)
        {
            int returnedStatusId = -1;
            string returnedStatusName = "";
            foreach (var status in statusDictionary)
            {
                if (status.Value == "Возвращён")
                {
                    returnedStatusId = status.Key;
                    returnedStatusName = status.Value;
                    break;
                }
            }

            if (returnedStatusId == -1)
            {
                MessageBox.Show("Статус 'Возвращён' не найден в базе данных!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (currentStatus == "Возвращён")
            {
                MessageBox.Show($"Сертификат №{certificateId} уже имеет статус 'Возвращён'!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (currentStatus == "Использован")
            {
                MessageBox.Show($"Сертификат №{certificateId} уже использован! Возврат невозможен.", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (currentStatus == "Просрочен")
            {
                MessageBox.Show($"Сертификат №{certificateId} просрочен! Возврат невозможен.", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show(
                $"Изменить статус сертификата №{certificateId}\nс \"{currentStatus}\" на \"{returnedStatusName}\"?",
                "Подтверждение возврата",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    string query = "UPDATE certificates SET id_status_certificate = @statusId WHERE id_certificate = @certificateId";
                    using (MySqlConnection connection = SettingsBD.GetConnection())
                    {
                        connection.Open();
                        using (MySqlCommand cmd = new MySqlCommand(query, connection))
                        {
                            cmd.Parameters.AddWithValue("@statusId", returnedStatusId);
                            cmd.Parameters.AddWithValue("@certificateId", certificateId);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                foreach (DataGridViewRow row in dataGridView1.Rows)
                                {
                                    if (Convert.ToInt32(row.Cells["id_certificate"].Value) == certificateId)
                                    {
                                        row.Cells["id_status_certificate"].Value = returnedStatusId;
                                        row.Cells["status_name"].Value = returnedStatusName;
                                        break;
                                    }
                                }
                                MessageBox.Show($"Статус сертификата №{certificateId} изменён на \"{returnedStatusName}\"!",
                                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ===== ПОКАЗ ДЕТАЛЬНОЙ ИНФОРМАЦИИ (БЕЗ ФИО, НО С ПОЛНЫМ ТЕЛЕФОНОМ) =====
        private void ShowCertificateDetails(int certificateId)
        {
            try
            {
                string query = @"
                    SELECT c.id_certificate, c.phone_number, c.price, c.date, 
                           sc.name as status_name
                    FROM certificates c
                    LEFT JOIN status_certificates sc ON c.id_status_certificate = sc.id_status_certificate
                    WHERE c.id_certificate = @id";

                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", certificateId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string phone = reader["phone_number"]?.ToString() ?? "";
                                // В детальной информации показываем ПОЛНЫЙ номер телефона (без маскировки)
                                string fullPhone = FormatPhoneNumber(phone);
                                decimal price = Convert.ToDecimal(reader["price"]);
                                DateTime date = Convert.ToDateTime(reader["date"]);
                                string status = reader["status_name"]?.ToString() ?? "";

                                Form detailsForm = new Form();
                                detailsForm.Text = $"Детальная информация о сертификате №{certificateId}";
                                detailsForm.Size = new Size(400, 350);
                                detailsForm.StartPosition = FormStartPosition.CenterParent;
                                detailsForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                                detailsForm.MaximizeBox = false;
                                detailsForm.MinimizeBox = false;
                                detailsForm.BackColor = Color.White;

                                TableLayoutPanel tlp = new TableLayoutPanel();
                                tlp.Dock = DockStyle.Fill;
                                tlp.ColumnCount = 2;
                                tlp.RowCount = 5;
                                tlp.Padding = new Padding(10);
                                tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
                                tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));

                                AddDetailRow(tlp, "Номер сертификата:", certificateId.ToString());
                                AddDetailRow(tlp, "Телефон:", fullPhone);
                                AddDetailRow(tlp, "Стоимость:", price.ToString("N2", russianCulture) + " ₽");
                                AddDetailRow(tlp, "Дата выдачи:", date.ToString("dd.MM.yyyy"));
                                AddDetailRow(tlp, "Статус:", status);

                                Button btnOk = new Button();
                                btnOk.Text = "OK";
                                btnOk.Size = new Size(80, 35);
                                btnOk.Location = new Point(150, 270);
                                btnOk.DialogResult = DialogResult.OK;
                                btnOk.FlatStyle = FlatStyle.Flat;
                                btnOk.FlatAppearance.BorderSize = 1;
                                btnOk.FlatAppearance.BorderColor = Color.Black;
                                btnOk.BackColor = Color.DarkSeaGreen;
                                btnOk.ForeColor = Color.Black;

                                detailsForm.Controls.Add(tlp);
                                detailsForm.Controls.Add(btnOk);
                                detailsForm.ShowDialog();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Форматирование полного номера телефона для отображения
        private string FormatPhoneNumber(string phone)
        {
            if (string.IsNullOrEmpty(phone)) return phone;

            try
            {
                string digits = new string(phone.Where(char.IsDigit).ToArray());
                if (digits.Length >= 11)
                {
                    // Формат: +7 (999) 123-45-67
                    return $"+7 ({digits.Substring(1, 3)}) {digits.Substring(4, 3)}-{digits.Substring(7, 2)}-{digits.Substring(9, 2)}";
                }
                return phone;
            }
            catch
            {
                return phone;
            }
        }

        private void AddDetailRow(TableLayoutPanel tlp, string label, string value)
        {
            Label lbl = new Label();
            lbl.Text = label;
            lbl.Font = new Font("Times New Roman", 10, FontStyle.Bold);
            lbl.TextAlign = ContentAlignment.MiddleLeft;
            lbl.Dock = DockStyle.Fill;

            Label val = new Label();
            val.Text = value;
            val.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            val.TextAlign = ContentAlignment.MiddleLeft;
            val.Dock = DockStyle.Fill;

            int row = tlp.RowCount;
            tlp.RowCount++;
            tlp.Controls.Add(lbl, 0, row);
            tlp.Controls.Add(val, 1, row);
        }

        // ===== МАСКИРОВАНИЕ ТЕЛЕФОНА В ТАБЛИЦЕ (скрываем 4 цифры в середине) =====
        private string MaskPhoneNumber(string phone)
        {
            if (string.IsNullOrEmpty(phone)) return phone;

            try
            {
                string digits = new string(phone.Where(char.IsDigit).ToArray());
                if (digits.Length >= 11)
                {
                    // В таблице показываем с маской: +7 (999) ****-45-67
                    return $"+7 ({digits.Substring(1, 3)}) ****-{digits.Substring(8, 2)}-{digits.Substring(10, 1)}";
                }
                return phone;
            }
            catch
            {
                return phone;
            }
        }

        // ===== ИНИЦИАЛИЗАЦИЯ DATA GRID VIEW (БЕЗ КОЛОНОК ФИО) =====
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

            // КОЛОНКА №1: НОМЕР
            DataGridViewTextBoxColumn colId = new DataGridViewTextBoxColumn();
            colId.Name = "id_certificate";
            colId.HeaderText = "№";
            colId.DataPropertyName = "id_certificate";
            colId.Width = 60;
            colId.MinimumWidth = 60;
            colId.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colId.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns.Add(colId);

            // КОЛОНКА №2: ТЕЛЕФОН (ЗАМАСКИРОВАННЫЙ)
            DataGridViewTextBoxColumn colPhone = new DataGridViewTextBoxColumn();
            colPhone.Name = "phone_number";
            colPhone.HeaderText = "Телефон";
            colPhone.DataPropertyName = "phone_number";
            colPhone.Width = 150;
            colPhone.MinimumWidth = 140;
            colPhone.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colPhone.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns.Add(colPhone);

            // КОЛОНКА №3: СТОИМОСТЬ
            DataGridViewTextBoxColumn colPrice = new DataGridViewTextBoxColumn();
            colPrice.Name = "price";
            colPrice.HeaderText = "Стоимость";
            colPrice.DataPropertyName = "price";
            colPrice.Width = 100;
            colPrice.MinimumWidth = 90;
            colPrice.DefaultCellStyle.Format = "N2";
            colPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colPrice.DefaultCellStyle.ForeColor = Color.DarkGreen;
            colPrice.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns.Add(colPrice);

            // КОЛОНКА №4: ДАТА ВЫДАЧИ
            DataGridViewTextBoxColumn colDate = new DataGridViewTextBoxColumn();
            colDate.Name = "date";
            colDate.HeaderText = "Дата выдачи";
            colDate.DataPropertyName = "date";
            colDate.Width = 100;
            colDate.MinimumWidth = 90;
            colDate.DefaultCellStyle.Format = "dd.MM.yyyy";
            colDate.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colDate.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns.Add(colDate);

            // СКРЫТАЯ КОЛОНКА ДЛЯ ID СТАТУСА
            DataGridViewTextBoxColumn colStatusId = new DataGridViewTextBoxColumn();
            colStatusId.Name = "id_status_certificate";
            colStatusId.DataPropertyName = "id_status_certificate";
            colStatusId.Visible = false;
            dataGridView1.Columns.Add(colStatusId);

            // КОЛОНКА №5: СТАТУС
            DataGridViewTextBoxColumn colStatusName = new DataGridViewTextBoxColumn();
            colStatusName.Name = "status_name";
            colStatusName.HeaderText = "Статус";
            colStatusName.DataPropertyName = "status_name";
            colStatusName.Width = 120;
            colStatusName.MinimumWidth = 100;
            colStatusName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colStatusName.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Bold);
            colStatusName.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns.Add(colStatusName);

            dataGridView1.ScrollBars = ScrollBars.Both;
            dataGridView1.Columns[dataGridView1.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.CellFormatting += DataGridView1_CellFormatting;
        }

        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex].Name == "price" && e.RowIndex >= 0 && e.Value != null && e.Value != DBNull.Value)
            {
                decimal price = Convert.ToDecimal(e.Value);
                e.Value = price.ToString("N2", russianCulture) + " ₽";
                e.FormattingApplied = true;
            }

            if (dataGridView1.Columns[e.ColumnIndex].Name == "date" && e.RowIndex >= 0 && e.Value != null && e.Value != DBNull.Value && e.Value is DateTime date)
            {
                e.Value = date.ToString("dd.MM.yyyy");
                e.FormattingApplied = true;
            }

            if (dataGridView1.Columns[e.ColumnIndex].Name == "phone_number" && e.RowIndex >= 0 && e.Value != null && e.Value != DBNull.Value)
            {
                string phone = e.Value.ToString();
                e.Value = MaskPhoneNumber(phone);
                e.FormattingApplied = true;
            }

            if (dataGridView1.Columns[e.ColumnIndex].Name == "status_name" && e.RowIndex >= 0 && e.Value != null)
            {
                string status = e.Value.ToString();
                switch (status)
                {
                    case "Активен":
                        e.CellStyle.ForeColor = Color.Green;
                        e.CellStyle.Font = new Font(dataGridView1.Font, FontStyle.Bold);
                        break;
                    case "Использован":
                        e.CellStyle.ForeColor = Color.Blue;
                        e.CellStyle.Font = new Font(dataGridView1.Font, FontStyle.Bold);
                        break;
                    case "Просрочен":
                        e.CellStyle.ForeColor = Color.Red;
                        e.CellStyle.Font = new Font(dataGridView1.Font, FontStyle.Bold);
                        break;
                    case "Возвращён":
                        e.CellStyle.ForeColor = Color.Orange;
                        e.CellStyle.Font = new Font(dataGridView1.Font, FontStyle.Bold);
                        break;
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
                    if (col.Name != "id_status_certificate" && col.Visible)
                    {
                        col.DefaultCellStyle.SelectionBackColor = selectionColor;
                        col.DefaultCellStyle.SelectionForeColor = Color.Black;
                    }
                }
                if (dataGridView1.Columns["price"] != null)
                {
                    dataGridView1.Columns["price"].DefaultCellStyle.ForeColor = Color.DarkGreen;
                    dataGridView1.Columns["price"].DefaultCellStyle.SelectionForeColor = Color.DarkGreen;
                }
            }
        }

        private void AdjustDataGridViewAfterLoad()
        {
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.Refresh();
        }

        private void textBoxSearch_Enter(object sender, EventArgs e) { }
        private void textBoxSearch_Leave(object sender, EventArgs e) { }

        private void textBoxSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
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
                LoadCertificatesWithFilter(digits, false);
            else
                LoadCertificatesWithFilter("", false);

            isFormatting = false;
        }

        private void LoadStatusesToComboBox()
        {
            try
            {
                string query = "SELECT id_status_certificate, name FROM status_certificates ORDER BY id_status_certificate";
                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        comboBoxStatusSert.Items.Clear();
                        comboBoxStatusSert.Items.Add("Все статусы");
                        statusDictionary.Clear();

                        while (reader.Read())
                        {
                            int id = reader.GetInt32("id_status_certificate");
                            string name = reader.GetString("name");
                            statusDictionary.Add(id, name);
                            comboBoxStatusSert.Items.Add(new StatusItem(id, name));
                        }
                    }
                }
                comboBoxStatusSert.DisplayMember = "Name";
                comboBoxStatusSert.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статусов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCertificatesWithFilter(string certificateNumber = "", bool exactMatch = false)
        {
            int statusId = -1;
            if (comboBoxStatusSert.SelectedIndex > 0 && comboBoxStatusSert.SelectedItem is StatusItem statusItem)
                statusId = statusItem.Id;

            LoadCertificates(certificateNumber, statusId, exactMatch);
        }

        private void LoadCertificates(string certificateNumber = "", int statusId = -1, bool exactMatch = false)
        {
            try
            {
                string query = @"
                    SELECT c.id_certificate, c.phone_number, c.price, c.date, c.id_status_certificate,
                           sc.name as status_name
                    FROM certificates c
                    LEFT JOIN status_certificates sc ON c.id_status_certificate = sc.id_status_certificate
                    WHERE 1=1";

                List<MySqlParameter> parameters = new List<MySqlParameter>();

                if (!string.IsNullOrEmpty(certificateNumber) && certificateNumber.All(char.IsDigit))
                {
                    if (exactMatch)
                    {
                        query += " AND c.id_certificate = @CertificateNumber";
                        parameters.Add(new MySqlParameter("@CertificateNumber", certificateNumber));
                    }
                    else
                    {
                        query += " AND CAST(c.id_certificate AS CHAR) LIKE @CertificateNumber";
                        parameters.Add(new MySqlParameter("@CertificateNumber", certificateNumber + "%"));
                    }
                }

                if (statusId > 0)
                {
                    query += " AND c.id_status_certificate = @StatusId";
                    parameters.Add(new MySqlParameter("@StatusId", statusId));
                }

                query += " ORDER BY c.date DESC, c.id_certificate DESC";

                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    foreach (var param in parameters)
                        cmd.Parameters.Add(param);

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
                MessageBox.Show($"Ошибка загрузки сертификатов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void comboBoxStatusSert_SelectedIndexChanged(object sender, EventArgs e)
        {
            string searchText = textBoxSearch.Text;
            if (string.IsNullOrWhiteSpace(searchText))
                LoadCertificatesWithFilter("", false);
            else
            {
                string digits = new string(searchText.Where(char.IsDigit).ToArray());
                if (digits.Length > MAX_SEARCH_LENGTH)
                    digits = digits.Substring(0, MAX_SEARCH_LENGTH);

                if (digits.Length > 0)
                    LoadCertificatesWithFilter(digits, false);
                else
                    LoadCertificatesWithFilter("", false);
            }
        }

        private void ButtonChangeStatus_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int certificateId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["id_certificate"].Value);
                string currentStatus = dataGridView1.SelectedRows[0].Cells["status_name"].Value?.ToString() ?? "";
                ChangeStatusToReturned(certificateId, currentStatus);
            }
            else
            {
                MessageBox.Show("Выберите сертификат для изменения статуса!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadCertificates()
        {
            LoadCertificates("", -1, false);
        }

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
                comboBoxStatusSert.SelectedIndex = 0;
                LoadCertificates();
                textBoxSearch.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сбросе фильтров: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            ManagerForm manager = new ManagerForm();
            manager.Show();
        }

        public class StatusItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public StatusItem(int id, string name) { Id = id; Name = name; }
            public override string ToString() { return Name; }
        }

        private void EmloyForm_Load(object sender, EventArgs e) { }
    }
}