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
        }

        private void InitializeComponents()
        {
            // Инициализация DataGridView
            InitializeDataGridView();

            // Настройка textBoxSearch (поиск по номеру сертификата)
            textBoxSearch.TextChanged += textBoxSearch_TextChanged;
            textBoxSearch.KeyPress += textBoxSearch_KeyPress;
            textBoxSearch.Enter += textBoxSearch_Enter;
            textBoxSearch.Leave += textBoxSearch_Leave;

            // Устанавливаем максимальную длину ввода
            textBoxSearch.MaxLength = MAX_SEARCH_LENGTH;
            textBoxSearch.Text = "";
            textBoxSearch.ForeColor = Color.Black;

            // Настройка comboBoxStatusSert
            comboBoxStatusSert.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxStatusSert.SelectedIndexChanged += comboBoxStatusSert_SelectedIndexChanged;

            // Настройка кнопки Reset
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

            // Настройка кнопки изменения статуса
            buttonChangeStatus.Click += ButtonChangeStatus_Click;
            buttonChangeStatus.FlatStyle = FlatStyle.Flat;
            buttonChangeStatus.FlatAppearance.BorderSize = 1;
            buttonChangeStatus.FlatAppearance.BorderColor = Color.Black;
            buttonChangeStatus.BackColor = Color.DarkSeaGreen;
            buttonChangeStatus.ForeColor = Color.Black;
            buttonChangeStatus.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            buttonChangeStatus.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            buttonChangeStatus.MouseDown += (s, e) =>
            {
                buttonChangeStatus.FlatAppearance.BorderColor = Color.DarkBlue;
            };

            buttonChangeStatus.MouseUp += (s, e) =>
            {
                buttonChangeStatus.FlatAppearance.BorderColor = Color.Black;
            };
            buttonChangeStatus.MouseLeave += (s, e) =>
            {
                buttonChangeStatus.FlatAppearance.BorderColor = Color.Black;
            };

            // Добавляем обработчик двойного клика для изменения статуса
            dataGridView1.CellDoubleClick += DataGridView1_CellDoubleClick;

            // Загрузка данных
            LoadStatusesToComboBox();
            LoadCertificates();
        }

        // Вспомогательный класс для хранения состояния статуса
        private class StatusState
        {
            public int SelectedStatusId { get; set; }
            public string SelectedStatusName { get; set; }
        }

        // ===== ИНИЦИАЛИЗАЦИЯ DATA GRID VIEW =====
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

            // Отключаем AutoSizeColumnsMode, чтобы работали ручные настройки ширины
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            // Включаем перенос текста в ячейках
            dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            // Автоматическая высота строк на основе содержимого
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.RowTemplate.Height = 40;
            dataGridView1.RowTemplate.MinimumHeight = 40;

            // Скрываем заголовки строк (серые ячейки слева)
            dataGridView1.RowHeadersVisible = false;

            // Отключаем стандартные стили для кастомной настройки шапки
            dataGridView1.EnableHeadersVisualStyles = false;

            // Настройка высоты шапки
            dataGridView1.ColumnHeadersHeight = 45;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            // Цвет шапки
            Color headerBackColor = Color.FromArgb(97, 173, 123);

            // Настройка стиля заголовков колонок
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = headerBackColor;
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.ColumnHeadersDefaultCellStyle.Padding = new Padding(0, 3, 0, 3);
            dataGridView1.ColumnHeadersDefaultCellStyle.SelectionBackColor = headerBackColor;
            dataGridView1.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.Black;

            // Настройка строк таблицы
            dataGridView1.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            dataGridView1.DefaultCellStyle.Padding = new Padding(0, 2, 0, 2);
            dataGridView1.DefaultCellStyle.BackColor = Color.White;
            dataGridView1.DefaultCellStyle.ForeColor = Color.Black;
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.FromArgb(233, 242, 236);
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.Black;

            // Настройка строк при выделении
            dataGridView1.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(233, 242, 236);
            dataGridView1.RowsDefaultCellStyle.SelectionForeColor = Color.Black;
            dataGridView1.RowsDefaultCellStyle.BackColor = Color.White;
            dataGridView1.RowsDefaultCellStyle.ForeColor = Color.Black;

            // Настройка внешнего вида
            dataGridView1.GridColor = Color.Gray;
            dataGridView1.BorderStyle = BorderStyle.FixedSingle;
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.Single;

            dataGridView1.Columns.Clear();

            // === КОЛОНКА №1: НОМЕР СЕРТИФИКАТА ===
            DataGridViewTextBoxColumn colId = new DataGridViewTextBoxColumn();
            colId.Name = "id_certificate";
            colId.HeaderText = "№";
            colId.DataPropertyName = "id_certificate";
            colId.Width = 60;
            colId.MinimumWidth = 60;
            colId.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colId.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            colId.Resizable = DataGridViewTriState.True;
            colId.HeaderCell.Style.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            colId.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colId.HeaderCell.Style.BackColor = headerBackColor;
            colId.HeaderCell.Style.ForeColor = Color.Black;
            colId.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns.Add(colId);

            // === КОЛОНКА №2: ФАМИЛИЯ ===
            DataGridViewTextBoxColumn colLastName = new DataGridViewTextBoxColumn();
            colLastName.Name = "last_name";
            colLastName.HeaderText = "Фамилия";
            colLastName.DataPropertyName = "last_name";
            colLastName.Width = 130;
            colLastName.MinimumWidth = 120;
            colLastName.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            colLastName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colLastName.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            colLastName.Resizable = DataGridViewTriState.True;
            colLastName.HeaderCell.Style.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            colLastName.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colLastName.HeaderCell.Style.BackColor = headerBackColor;
            colLastName.HeaderCell.Style.ForeColor = Color.Black;
            colLastName.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns.Add(colLastName);

            // === КОЛОНКА №3: ИМЯ ===
            DataGridViewTextBoxColumn colFirstName = new DataGridViewTextBoxColumn();
            colFirstName.Name = "first_name";
            colFirstName.HeaderText = "Имя";
            colFirstName.DataPropertyName = "first_name";
            colFirstName.Width = 130;
            colFirstName.MinimumWidth = 120;
            colFirstName.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            colFirstName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colFirstName.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            colFirstName.Resizable = DataGridViewTriState.True;
            colFirstName.HeaderCell.Style.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            colFirstName.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colFirstName.HeaderCell.Style.BackColor = headerBackColor;
            colFirstName.HeaderCell.Style.ForeColor = Color.Black;
            colFirstName.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns.Add(colFirstName);

            // === КОЛОНКА №4: ОТЧЕСТВО ===
            DataGridViewTextBoxColumn colMiddleName = new DataGridViewTextBoxColumn();
            colMiddleName.Name = "middle_name";
            colMiddleName.HeaderText = "Отчество";
            colMiddleName.DataPropertyName = "middle_name";
            colMiddleName.Width = 130;
            colMiddleName.MinimumWidth = 120;
            colMiddleName.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            colMiddleName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colMiddleName.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            colMiddleName.Resizable = DataGridViewTriState.True;
            colMiddleName.HeaderCell.Style.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            colMiddleName.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colMiddleName.HeaderCell.Style.BackColor = headerBackColor;
            colMiddleName.HeaderCell.Style.ForeColor = Color.Black;
            colMiddleName.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns.Add(colMiddleName);

            // === КОЛОНКА №5: ТЕЛЕФОН ===
            DataGridViewTextBoxColumn colPhone = new DataGridViewTextBoxColumn();
            colPhone.Name = "phone_number";
            colPhone.HeaderText = "Телефон";
            colPhone.DataPropertyName = "phone_number";
            colPhone.Width = 150;
            colPhone.MinimumWidth = 140;
            colPhone.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            colPhone.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colPhone.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            colPhone.Resizable = DataGridViewTriState.True;
            colPhone.HeaderCell.Style.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            colPhone.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colPhone.HeaderCell.Style.BackColor = headerBackColor;
            colPhone.HeaderCell.Style.ForeColor = Color.Black;
            colPhone.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns.Add(colPhone);

            // === КОЛОНКА №6: СТОИМОСТЬ ===
            DataGridViewTextBoxColumn colPrice = new DataGridViewTextBoxColumn();
            colPrice.Name = "price";
            colPrice.HeaderText = "Стоимость";
            colPrice.DataPropertyName = "price";
            colPrice.Width = 100;
            colPrice.MinimumWidth = 90;
            colPrice.DefaultCellStyle.Format = "N2";
            colPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colPrice.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            colPrice.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            colPrice.DefaultCellStyle.ForeColor = Color.DarkGreen;
            colPrice.Resizable = DataGridViewTriState.True;
            colPrice.HeaderCell.Style.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            colPrice.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colPrice.HeaderCell.Style.BackColor = headerBackColor;
            colPrice.HeaderCell.Style.ForeColor = Color.Black;
            colPrice.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns.Add(colPrice);

            // === КОЛОНКА №7: ДАТА ВЫДАЧИ ===
            DataGridViewTextBoxColumn colDate = new DataGridViewTextBoxColumn();
            colDate.Name = "date";
            colDate.HeaderText = "Дата выдачи";
            colDate.DataPropertyName = "date";
            colDate.Width = 100;
            colDate.MinimumWidth = 90;
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
            dataGridView1.Columns.Add(colDate);

            // === СКРЫТАЯ КОЛОНКА ДЛЯ ХРАНЕНИЯ ID СТАТУСА ===
            DataGridViewTextBoxColumn colStatusId = new DataGridViewTextBoxColumn();
            colStatusId.Name = "id_status_certificate";
            colStatusId.HeaderText = "ID статуса";
            colStatusId.DataPropertyName = "id_status_certificate";
            colStatusId.Visible = false;
            colStatusId.Width = 50;
            colStatusId.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns.Add(colStatusId);

            // === КОЛОНКА №8: СТАТУС ===
            DataGridViewTextBoxColumn colStatusName = new DataGridViewTextBoxColumn();
            colStatusName.Name = "status_name";
            colStatusName.HeaderText = "Статус";
            colStatusName.DataPropertyName = "status_name";
            colStatusName.Width = 120;
            colStatusName.MinimumWidth = 100;
            colStatusName.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            colStatusName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colStatusName.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Bold);
            colStatusName.Resizable = DataGridViewTriState.True;
            colStatusName.HeaderCell.Style.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            colStatusName.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colStatusName.HeaderCell.Style.BackColor = headerBackColor;
            colStatusName.HeaderCell.Style.ForeColor = Color.Black;
            colStatusName.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns.Add(colStatusName);

            // Добавляем возможность горизонтальной прокрутки
            dataGridView1.ScrollBars = ScrollBars.Both;

            // Устанавливаем режим заполнения для последней колонки
            dataGridView1.Columns[dataGridView1.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            // Подписка на событие форматирования ячеек
            dataGridView1.CellFormatting += DataGridView1_CellFormatting;
        }

        // ===== ФОРМАТИРОВАНИЕ ЯЧЕЕК DATA GRID VIEW =====
        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Форматирование стоимости
            if (dataGridView1.Columns[e.ColumnIndex].Name == "price" && e.RowIndex >= 0)
            {
                if (e.Value != null && e.Value != DBNull.Value)
                {
                    decimal price = Convert.ToDecimal(e.Value);
                    e.Value = price.ToString("N2", russianCulture) + " ₽";
                    e.FormattingApplied = true;
                }
            }

            // Форматирование даты
            if (dataGridView1.Columns[e.ColumnIndex].Name == "date" && e.RowIndex >= 0)
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

            // Форматирование статуса (цвет)
            if (dataGridView1.Columns[e.ColumnIndex].Name == "status_name" && e.RowIndex >= 0)
            {
                if (e.Value != null)
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
                    }
                }
            }
        }

        // ===== МЕТОД ДЛЯ НАСТРОЙКИ СТИЛЕЙ КОЛОНОК ПОСЛЕ ЗАГРУЗКИ ДАННЫХ =====
        private void SetupColumnStyles()
        {
            if (dataGridView1.Columns.Count > 0)
            {
                Color selectionColor = Color.FromArgb(233, 242, 236);

                // Настройка для всех колонок
                foreach (DataGridViewColumn col in dataGridView1.Columns)
                {
                    if (col.Name != "id_status_certificate" && col.Visible)
                    {
                        col.DefaultCellStyle.SelectionBackColor = selectionColor;
                        col.DefaultCellStyle.SelectionForeColor = Color.Black;
                    }
                }

                // Специальная настройка для колонки "Стоимость"
                if (dataGridView1.Columns["price"] != null)
                {
                    dataGridView1.Columns["price"].DefaultCellStyle.ForeColor = Color.DarkGreen;
                    dataGridView1.Columns["price"].DefaultCellStyle.SelectionForeColor = Color.DarkGreen;
                }
            }
        }

        // ===== МЕТОД ДЛЯ НАСТРОЙКИ ПОСЛЕ ЗАГРУЗКИ ДАННЫХ =====
        private void AdjustDataGridViewAfterLoad()
        {
            // Устанавливаем режим автоподбора высоты строк
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            // Разрешаем перенос текста для всех текстовых колонок
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                if (col.Name != "id_certificate" && col.Name != "price" &&
                    col.Name != "id_status_certificate" && col.Name != "date")
                {
                    col.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                }
            }

            // Обновляем отображение
            dataGridView1.Refresh();
        }

        // ===== МЕТОДЫ ДЛЯ ПОИСКА ПО НОМЕРУ СЕРТИФИКАТА =====
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

            // Если есть цифры - ищем по НОМЕРУ СЕРТИФИКАТА
            if (digits.Length > 0)
            {
                LoadCertificatesWithFilter(digits, false);
            }
            else
            {
                LoadCertificatesWithFilter("", false);
            }

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
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            comboBoxStatusSert.Items.Clear();
                            comboBoxStatusSert.Items.Add("Все статусы");

                            // Очищаем словарь и добавляем в него статусы
                            statusDictionary.Clear();

                            while (reader.Read())
                            {
                                int id = reader.GetInt32("id_status_certificate");
                                string name = reader.GetString("name");

                                // Добавляем в словарь
                                statusDictionary.Add(id, name);

                                // Добавляем в комбобокс
                                comboBoxStatusSert.Items.Add(new StatusItem(id, name));
                            }
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
            // Получаем выбранный статус
            int statusId = -1;
            if (comboBoxStatusSert.SelectedIndex > 0 && comboBoxStatusSert.SelectedItem is StatusItem statusItem)
            {
                statusId = statusItem.Id;
            }

            LoadCertificates(certificateNumber, statusId, exactMatch);
        }

        private void LoadCertificates(string certificateNumber = "", int statusId = -1, bool exactMatch = false)
        {
            try
            {
                string query = @"
                    SELECT c.id_certificate, c.last_name, c.first_name, c.middle_name,
                           c.phone_number, c.price, c.date, c.id_status_certificate,
                           sc.name as status_name
                    FROM certificates c
                    LEFT JOIN status_certificates sc ON c.id_status_certificate = sc.id_status_certificate
                    WHERE 1=1";

                List<MySqlParameter> parameters = new List<MySqlParameter>();

                // ФИЛЬТР ПО НОМЕРУ СЕРТИФИКАТА
                if (!string.IsNullOrEmpty(certificateNumber) && certificateNumber.All(char.IsDigit))
                {
                    if (exactMatch)
                    {
                        // Точное совпадение
                        query += " AND c.id_certificate = @CertificateNumber";
                        parameters.Add(new MySqlParameter("@CertificateNumber", certificateNumber));
                    }
                    else
                    {
                        // Поиск по НАЧАЛУ номера (номера, начинающиеся с введенных цифр)
                        query += " AND CAST(c.id_certificate AS CHAR) LIKE @CertificateNumber";
                        // Добавляем % в конец для поиска по началу строки
                        parameters.Add(new MySqlParameter("@CertificateNumber", certificateNumber + "%"));
                    }
                }

                // Фильтр по статусу
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

                    // Применяем стили для колонок после загрузки данных
                    SetupColumnStyles();

                    // Настройка высоты строк после загрузки данных
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
            // При изменении статуса обновляем данные с текущим фильтром
            string searchText = textBoxSearch.Text;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                LoadCertificatesWithFilter("", false);
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
                    LoadCertificatesWithFilter(digits, false);
                }
                else
                {
                    LoadCertificatesWithFilter("", false);
                }
            }
        }

        // ===== МЕТОД ДЛЯ ИЗМЕНЕНИЯ СТАТУСА СЕРТИФИКАТА =====
        private void ButtonChangeStatus_Click(object sender, EventArgs e)
        {
            ShowChangeStatusDialog();
        }

        private void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            ShowChangeStatusDialog();
        }

        private void ShowChangeStatusDialog()
        {
            try
            {
                // Проверяем, выбран ли какой-нибудь сертификат
                if (dataGridView1.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Выберите сертификат для изменения статуса!", "Предупреждение",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Получаем выбранную строку
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];

                // Получаем данные сертификата
                int certificateId = Convert.ToInt32(selectedRow.Cells["id_certificate"].Value);
                string fullName = $"{selectedRow.Cells["last_name"].Value} {selectedRow.Cells["first_name"].Value} {selectedRow.Cells["middle_name"].Value}";
                string phone = selectedRow.Cells["phone_number"].Value?.ToString() ?? "";
                decimal price = Convert.ToDecimal(selectedRow.Cells["price"].Value ?? 0);
                DateTime date = Convert.ToDateTime(selectedRow.Cells["date"].Value ?? DateTime.Now);
                int currentStatusId = Convert.ToInt32(selectedRow.Cells["id_status_certificate"].Value ?? 0);
                string currentStatus = selectedRow.Cells["status_name"].Value?.ToString() ?? "";

                // Создаем объект для хранения состояния статуса
                StatusState statusState = new StatusState
                {
                    SelectedStatusId = currentStatusId,
                    SelectedStatusName = currentStatus
                };

                // Создаем форму для изменения статуса
                Form statusForm = new Form();
                statusForm.Text = $"Изменение статуса сертификата №{certificateId}";
                statusForm.Size = new Size(500, 300);
                statusForm.StartPosition = FormStartPosition.CenterParent;
                statusForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                statusForm.MaximizeBox = false;
                statusForm.MinimizeBox = false;
                statusForm.BackColor = Color.White;

                // Устанавливаем шрифт
                statusForm.Font = new Font("Times New Roman", 12, FontStyle.Regular);

                // Информационная панель
                Panel infoPanel = new Panel();
                infoPanel.Size = new Size(460, 100);
                infoPanel.Location = new Point(15, 15);
                infoPanel.BorderStyle = BorderStyle.FixedSingle;
                infoPanel.BackColor = Color.FromArgb(240, 240, 240);

                Label lblInfo = new Label();
                lblInfo.Text = $"Сертификат №{certificateId}\n" +
                              $"Владелец: {fullName}\n" +
                              $"Телефон: {phone}\n" +
                              $"Стоимость: {price.ToString("N2", russianCulture)} ₽ от {date:dd.MM.yyyy}";
                lblInfo.Location = new Point(10, 10);
                lblInfo.Size = new Size(440, 80);
                lblInfo.Font = new Font("Times New Roman", 10, FontStyle.Regular);
                lblInfo.TextAlign = ContentAlignment.TopLeft;

                infoPanel.Controls.Add(lblInfo);

                // Панель статуса
                Panel statusPanel = new Panel();
                statusPanel.Size = new Size(460, 80);
                statusPanel.Location = new Point(15, 130);
                statusPanel.BorderStyle = BorderStyle.FixedSingle;
                statusPanel.BackColor = Color.FromArgb(255, 255, 220);

                Label lblCurrentStatus = new Label();
                lblCurrentStatus.Text = "Текущий статус:";
                lblCurrentStatus.Location = new Point(10, 15);
                lblCurrentStatus.Size = new Size(100, 25);
                lblCurrentStatus.Font = new Font("Times New Roman", 10, FontStyle.Bold);

                Label lblCurrentStatusValue = new Label();
                lblCurrentStatusValue.Text = currentStatus;
                lblCurrentStatusValue.Location = new Point(120, 15);
                lblCurrentStatusValue.Size = new Size(150, 25);
                lblCurrentStatusValue.Font = new Font("Times New Roman", 10, FontStyle.Bold);
                lblCurrentStatusValue.ForeColor = Color.DarkBlue;

                Label lblNewStatus = new Label();
                lblNewStatus.Text = "Новый статус:";
                lblNewStatus.Location = new Point(10, 45);
                lblNewStatus.Size = new Size(100, 25);
                lblNewStatus.Font = new Font("Times New Roman", 10, FontStyle.Regular);

                ComboBox cmbNewStatus = new ComboBox();
                cmbNewStatus.Location = new Point(120, 45);
                cmbNewStatus.Size = new Size(200, 25);
                cmbNewStatus.DropDownStyle = ComboBoxStyle.DropDownList;
                cmbNewStatus.Font = new Font("Times New Roman", 10, FontStyle.Regular);

                // Загружаем все статусы
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

                cmbNewStatus.SelectedIndexChanged += (s, e) =>
                {
                    if (cmbNewStatus.SelectedItem is StatusItem selectedItem)
                    {
                        statusState.SelectedStatusId = selectedItem.Id;
                        statusState.SelectedStatusName = selectedItem.Name;
                    }
                };

                statusPanel.Controls.Add(lblCurrentStatus);
                statusPanel.Controls.Add(lblCurrentStatusValue);
                statusPanel.Controls.Add(lblNewStatus);
                statusPanel.Controls.Add(cmbNewStatus);

                // Кнопки
                Button btnSave = new Button();
                btnSave.Text = "Сохранить";
                btnSave.Location = new Point(280, 220);
                btnSave.Size = new Size(100, 35);
                btnSave.FlatStyle = FlatStyle.Flat;
                btnSave.FlatAppearance.BorderSize = 1;
                btnSave.FlatAppearance.BorderColor = Color.Black;
                btnSave.BackColor = Color.DarkSeaGreen;
                btnSave.ForeColor = Color.Black;
                btnSave.Font = new Font("Times New Roman", 11, FontStyle.Bold);

                Button btnCancel = new Button();
                btnCancel.Text = "Отмена";
                btnCancel.Location = new Point(390, 220);
                btnCancel.Size = new Size(80, 35);
                btnCancel.FlatStyle = FlatStyle.Flat;
                btnCancel.FlatAppearance.BorderSize = 1;
                btnCancel.FlatAppearance.BorderColor = Color.Black;
                btnCancel.BackColor = Color.LightGray;
                btnCancel.ForeColor = Color.Black;
                btnCancel.Font = new Font("Times New Roman", 11, FontStyle.Regular);

                statusForm.Controls.Add(infoPanel);
                statusForm.Controls.Add(statusPanel);
                statusForm.Controls.Add(btnSave);
                statusForm.Controls.Add(btnCancel);

                // Обработчики кнопок
                btnSave.Click += (s, e) =>
                {
                    if (statusState.SelectedStatusId == currentStatusId)
                    {
                        MessageBox.Show("Статус не изменен!", "Информация",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        statusForm.Close();
                        return;
                    }

                    DialogResult result = MessageBox.Show(
                        $"Изменить статус сертификата с \"{currentStatus}\" на \"{statusState.SelectedStatusName}\"?",
                        "Подтверждение",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        if (UpdateCertificateStatus(certificateId, statusState.SelectedStatusId, statusState.SelectedStatusName))
                        {
                            MessageBox.Show("Статус успешно обновлен!", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            statusForm.Close();
                        }
                    }
                };

                btnCancel.Click += (s, e) =>
                {
                    statusForm.Close();
                };

                statusForm.ShowDialog(this);

                // После закрытия формы обновляем таблицу
                RefreshCertificatesData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении статуса: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===== МЕТОД ДЛЯ ОБНОВЛЕНИЯ СТАТУСА В БАЗЕ ДАННЫХ =====
        private bool UpdateCertificateStatus(int certificateId, int newStatusId, string newStatusName)
        {
            try
            {
                string query = "UPDATE certificates SET id_status_certificate = @statusId WHERE id_certificate = @certificateId";

                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@statusId", newStatusId);
                        cmd.Parameters.AddWithValue("@certificateId", certificateId);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Обновляем отображение в таблице
                            UpdateCertificateStatusInGrid(certificateId, newStatusId, newStatusName);
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

        // ===== МЕТОД ДЛЯ ОБНОВЛЕНИЯ СТАТУСА В ТАБЛИЦЕ =====
        private void UpdateCertificateStatusInGrid(int certificateId, int newStatusId, string newStatusName)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["id_certificate"].Value != null &&
                    Convert.ToInt32(row.Cells["id_certificate"].Value) == certificateId)
                {
                    row.Cells["id_status_certificate"].Value = newStatusId;
                    row.Cells["status_name"].Value = newStatusName;
                    break;
                }
            }
        }

        // ===== МЕТОД ДЛЯ ОБНОВЛЕНИЯ ДАННЫХ В ТАБЛИЦЕ =====
        private void RefreshCertificatesData()
        {
            string searchText = textBoxSearch.Text;
            if (string.IsNullOrWhiteSpace(searchText))
            {
                LoadCertificatesWithFilter("", false);
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
                    LoadCertificatesWithFilter(digits, false);
                }
                else
                {
                    LoadCertificatesWithFilter("", false);
                }
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

        private void EmloyForm_Load(object sender, EventArgs e)
        {
        }
    }
}