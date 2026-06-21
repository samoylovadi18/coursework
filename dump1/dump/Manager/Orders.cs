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
    /// <summary>
    /// Форма управления текущими заказами для менеджера.
    /// Предоставляет функционал для просмотра, поиска, фильтрации и изменения статусов заказов.
    /// </summary>
    public partial class Orders : Form
    {
        private DataTable ordersTable;
        private BindingSource bindingSource;
        private MySqlDataAdapter dataAdapter;
        private bool isFormatting = false;
        private bool isUpdatingText = false;
        private int prevTextLength = 0;

        // Словарь для хранения статусов (id, name)
        private Dictionary<int, string> statusDictionary = new Dictionary<int, string>();

        // ID статуса "Доставлен" = 6
        private const int DELIVERED_STATUS_ID = 6;
        private bool isLockDialogOpen = false;

        // Культура для форматирования
        private CultureInfo russianCulture = new CultureInfo("ru-RU");

        // Типы поиска
        private enum SearchType
        {
            ByOrderNumber,
            ByPhone
        }
        private SearchType currentSearchType = SearchType.ByOrderNumber;

        // ===================== ПЕРЕМЕННЫЕ ДЛЯ ПОСТРАНИЧНОГО ВЫВОДА =====================
        private int currentPage = 1;
        private int pageSize = 10;
        private int totalRecords = 0;
        private int totalPages = 0;
        private DataTable fullDataTable = new DataTable();
        private bool isPagingEnabled = true;

        /// <summary>
        /// Конструктор формы управления заказами.
        /// Инициализирует компоненты и загружает данные.
        /// </summary>
        public Orders()
        {
            InitializeComponent();
            InitializeComponents();
            this.FormClosing += OrderDetailsForm_FormClosing;
            InactivityManager.RegisterForm(this);
            InactivityManager.OnLockRequest += LockSystem;
        }

        // ===================== МЕТОДЫ БЛОКИРОВКИ =====================

        /// <summary>
        /// Блокирует систему при длительном бездействии пользователя.
        /// Отображает диалоговое окно для ввода пароля разблокировки.
        /// </summary>
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

        /// <summary>
        /// Проверяет введённый пароль для разблокировки системы.
        /// </summary>
        /// <param name="txtPassword">Поле ввода пароля.</param>
        /// <param name="lockDialog">Диалоговое окно блокировки.</param>
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

        /// <summary>
        /// Получает хеш пароля текущего пользователя из базы данных.
        /// </summary>
        /// <returns>Строка с хешем пароля или null в случае ошибки.</returns>
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

        /// <summary>
        /// Вычисляет хеш SHA-256 для переданного пароля.
        /// </summary>
        /// <param name="password">Пароль в открытом виде.</param>
        /// <returns>Строка с хешем пароля в шестнадцатеричном формате.</returns>
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

        /// <summary>
        /// Обработчик события закрытия формы.
        /// Отписывается от менеджера бездействия.
        /// </summary>
        /// <param name="e">Аргументы события закрытия формы.</param>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            InactivityManager.UnregisterForm();
            base.OnFormClosed(e);
        }

        /// <summary>
        /// Обработчик события закрытия формы.
        /// При закрытии формы пользователем скрывает её и открывает форму менеджера.
        /// </summary>
        private void OrderDetailsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                ManagerForm manager = new ManagerForm();
                manager.Show();
            }
        }

        // ===================== ИНИЦИАЛИЗАЦИЯ =====================

        /// <summary>
        /// Инициализирует компоненты формы: DataGridView, поиск, фильтры и кнопки.
        /// </summary>
        private void InitializeComponents()
        {
            InitializeDataGridView();

            // ============================================================
            // ЗАГОЛОВОК "Текущие заказы:" — ПО ЦЕНТРУ СВЕРХУ
            // ============================================================
            if (lblTitle != null)
            {
                // Убираем все привязки и делаем фиксированное позиционирование
                lblTitle.Dock = DockStyle.None;
                lblTitle.Text = "Текущие заказы:";
                lblTitle.TextAlign = ContentAlignment.MiddleCenter;
                // Фиксируем размер шрифта
                lblTitle.Font = new Font("Times New Roman", 26, FontStyle.Bold, GraphicsUnit.Point);
                // Растягиваем по ширине формы
                lblTitle.Width = this.ClientSize.Width;
                lblTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            }

            // ============================================================
            // DATA GRID VIEW — РАСПОЛАГАЕМ НИЖЕ С ГОРИЗОНТАЛЬНЫМ СКРОЛЛОМ
            // ============================================================
            if (dataGridView1 != null)
            {
                // ВКЛЮЧАЕМ ОБА СКРОЛЛА — вертикальный И горизонтальный
                dataGridView1.ScrollBars = ScrollBars.Both;
                dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
                dataGridView1.Location = new Point(dataGridView1.Location.X, 150);
                // Разрешаем горизонтальный скролл при нехватке места
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            }

            // Кнопка детальной информации - правый нижний угол
            if (buttonDetail != null)
            {
                buttonDetail.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                buttonDetail.Text = "Детальная инф.";
                buttonDetail.Font = new Font("Times New Roman", 14, FontStyle.Regular);
                buttonDetail.FlatStyle = FlatStyle.Flat;
                buttonDetail.FlatAppearance.BorderSize = 1;
                buttonDetail.FlatAppearance.BorderColor = Color.Black;
                buttonDetail.BackColor = Color.DarkSeaGreen;
                buttonDetail.ForeColor = Color.Black;
                buttonDetail.Click += ButtonDetail_Click;
            }

            // Кнопки пагинации - просто подписываемся на события
            if (btnFirstPage != null)
            {
                btnFirstPage.Click += BtnFirstPage_Click;
            }
            if (btnPrevPage != null)
            {
                btnPrevPage.Click += BtnPrevPage_Click;
            }
            if (btnNextPage != null)
            {
                btnNextPage.Click += BtnNextPage_Click;
            }
            if (btnLastPage != null)
            {
                btnLastPage.Click += BtnLastPage_Click;
            }
            if (lblPageInfo != null)
            {
                lblPageInfo.Text = "Страница 1 из 1";
            }

            if (comboBoxSearchType != null)
            {
                comboBoxSearchType.DropDownStyle = ComboBoxStyle.DropDownList;
                comboBoxSearchType.Items.Clear();
                comboBoxSearchType.Items.Add("Поиск по номеру заказа");
                comboBoxSearchType.Items.Add("Поиск по номеру телефона");
                comboBoxSearchType.SelectedIndex = 0;
                comboBoxSearchType.SelectedIndexChanged += ComboBoxSearchType_SelectedIndexChanged;
            }

            SetupSearchPlaceholder();
            textBoxSearch.TextChanged += textBoxSearch_TextChanged;
            textBoxSearch.KeyPress += textBoxSearch_KeyPress;
            textBoxSearch.Click += TextBoxSearch_Click;

            comboBoxOrderStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxOrderStatus.SelectedIndexChanged += comboBoxStatus_SelectedIndexChanged;

            buttonReset.Click += buttonReset_Click;
            StyleButton(buttonReset);

            dataGridView1.CellDoubleClick += DataGridView1_CellDoubleClick;

            LoadStatusesToComboBox();
            LoadOrders();
        }

        /// <summary>
        /// Применяет единый стиль к кнопке.
        /// </summary>
        /// <param name="btn">Кнопка для стилизации.</param>
        private void StyleButton(Button btn)
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

        // ===================== МЕТОДЫ ПОИСКА И ФИЛЬТРАЦИИ =====================

        /// <summary>
        /// Настраивает текст-заполнитель (placeholder) для поля поиска.
        /// </summary>
        private void SetupSearchPlaceholder()
        {
            if (currentSearchType == SearchType.ByOrderNumber)
            {
                textBoxSearch.Text = "Введите номер заказа...";
                textBoxSearch.ForeColor = Color.Gray;
                textBoxSearch.MaxLength = 10;
            }
            else
            {
                textBoxSearch.Text = "Введите номер телефона...";
                textBoxSearch.ForeColor = Color.Gray;
                textBoxSearch.MaxLength = 18;
            }
        }

        /// <summary>
        /// Обработчик изменения типа поиска.
        /// </summary>
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

        /// <summary>
        /// Обработчик клика по полю поиска.
        /// Очищает placeholder при фокусе.
        /// </summary>
        private void TextBoxSearch_Click(object sender, EventArgs e)
        {
            if (textBoxSearch.ForeColor == Color.Gray)
            {
                textBoxSearch.Text = "";
                textBoxSearch.ForeColor = Color.Black;
                textBoxSearch.Focus();
            }
        }

        /// <summary>
        /// Обработчик нажатия клавиш в поле поиска.
        /// Разрешает ввод только цифр.
        /// </summary>
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

        /// <summary>
        /// Обработчик изменения текста в поле поиска.
        /// Применяет фильтрацию с учётом типа поиска.
        /// </summary>
        private void textBoxSearch_TextChanged(object sender, EventArgs e)
        {
            if (textBoxSearch.ForeColor == Color.Gray)
                return;

            if (isFormatting || isUpdatingText) return;
            isFormatting = true;

            string inputText = textBoxSearch.Text;
            int cursorPos = textBoxSearch.SelectionStart;
            prevTextLength = inputText.Length;

            if (currentSearchType == SearchType.ByOrderNumber)
            {
                string digits = new string(inputText.Where(char.IsDigit).ToArray());

                if (digits.Length > 10)
                {
                    digits = digits.Substring(0, 10);
                }

                if (digits != inputText)
                {
                    isUpdatingText = true;
                    textBoxSearch.Text = digits;
                    textBoxSearch.SelectionStart = Math.Min(cursorPos, digits.Length);
                    isUpdatingText = false;
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

                int digitsBeforeCursor = 0;
                for (int i = 0; i < cursorPos && i < inputText.Length; i++)
                {
                    if (char.IsDigit(inputText[i]))
                        digitsBeforeCursor++;
                }

                string formatted = FormatPhoneNumberForInput(digits);

                if (formatted != inputText)
                {
                    isUpdatingText = true;
                    textBoxSearch.Text = formatted;
                    int newCursorPos = GetCursorPosByDigitCount(formatted, digitsBeforeCursor);
                    textBoxSearch.SelectionStart = Math.Min(newCursorPos, formatted.Length);
                    isUpdatingText = false;
                }

                if (digits.Length >= 3)
                    LoadOrdersWithFilter(digits, false);
                else if (digits.Length == 0)
                    LoadOrdersWithFilter("", false);
            }

            isFormatting = false;
        }

        /// <summary>
        /// Форматирует номер телефона для ввода в маску +7 (XXX) XXX-XX-XX.
        /// </summary>
        private string FormatPhoneNumberForInput(string digits)
        {
            if (string.IsNullOrEmpty(digits))
                return "";

            if (digits.Length == 0)
                return "";

            string result = "";

            if (digits.Length >= 1)
            {
                result = "+7";
            }

            if (digits.Length >= 2)
            {
                result += " (";
                int codeLength = Math.Min(3, digits.Length - 1);
                result += digits.Substring(1, codeLength);

                if (digits.Length > 4)
                {
                    result += ") ";
                    int numLength = Math.Min(3, digits.Length - 4);
                    result += digits.Substring(4, numLength);

                    if (digits.Length > 7)
                    {
                        result += "-";
                        int num2Length = Math.Min(2, digits.Length - 7);
                        result += digits.Substring(7, num2Length);

                        if (digits.Length > 9)
                        {
                            result += "-";
                            int num3Length = Math.Min(2, digits.Length - 9);
                            result += digits.Substring(9, num3Length);
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

        /// <summary>
        /// Получает позицию курсора по количеству цифр в отформатированном тексте.
        /// </summary>
        private int GetCursorPosByDigitCount(string formattedText, int digitsCount)
        {
            if (digitsCount <= 0) return 0;

            int foundDigits = 0;
            for (int i = 0; i < formattedText.Length; i++)
            {
                if (char.IsDigit(formattedText[i]))
                {
                    foundDigits++;
                    if (foundDigits >= digitsCount)
                    {
                        return i + 1;
                    }
                }
            }
            return formattedText.Length;
        }

        /// <summary>
        /// Маскирует номер телефона для отображения (защита персональных данных).
        /// </summary>
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

        // ===================== ОБРАБОТЧИКИ ПАГИНАЦИИ =====================

        /// <summary>
        /// Обработчик нажатия кнопки перехода на первую страницу.
        /// </summary>
        private void BtnFirstPage_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage = 1;
                ApplyPagination();
            }
        }

        /// <summary>
        /// Обработчик нажатия кнопки перехода на предыдущую страницу.
        /// </summary>
        private void BtnPrevPage_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                ApplyPagination();
            }
        }

        /// <summary>
        /// Обработчик нажатия кнопки перехода на следующую страницу.
        /// </summary>
        private void BtnNextPage_Click(object sender, EventArgs e)
        {
            if (currentPage < totalPages)
            {
                currentPage++;
                ApplyPagination();
            }
        }

        /// <summary>
        /// Обработчик нажатия кнопки перехода на последнюю страницу.
        /// </summary>
        private void BtnLastPage_Click(object sender, EventArgs e)
        {
            if (currentPage < totalPages)
            {
                currentPage = totalPages;
                ApplyPagination();
            }
        }

        // ===================== ПРИМЕНЕНИЕ ПАГИНАЦИИ =====================

        /// <summary>
        /// Применяет пагинацию к данным и обновляет отображение.
        /// </summary>
        private void ApplyPagination()
        {
            if (!isPagingEnabled || fullDataTable == null || fullDataTable.Rows.Count == 0)
            {
                UpdatePaginationControls();
                return;
            }

            try
            {
                totalRecords = fullDataTable.Rows.Count;
                totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                if (currentPage > totalPages)
                    currentPage = totalPages;

                if (currentPage < 1)
                    currentPage = 1;

                int startIndex = (currentPage - 1) * pageSize;
                int endIndex = Math.Min(startIndex + pageSize, totalRecords);

                DataTable pageTable = fullDataTable.Clone();

                for (int i = startIndex; i < endIndex; i++)
                {
                    pageTable.ImportRow(fullDataTable.Rows[i]);
                }

                if (bindingSource == null)
                {
                    bindingSource = new BindingSource();
                    dataGridView1.DataSource = bindingSource;
                }
                bindingSource.DataSource = pageTable;

                UpdatePaginationControls();
                SetupColumnStyles();
                AdjustDataGridViewAfterLoad();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при применении пагинации: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Обновляет состояние элементов управления пагинацией.
        /// </summary>
        private void UpdatePaginationControls()
        {
            if (lblPageInfo != null)
            {
                if (fullDataTable == null || fullDataTable.Rows.Count == 0)
                {
                    lblPageInfo.Text = "Нет данных";
                    btnFirstPage.Enabled = false;
                    btnPrevPage.Enabled = false;
                    btnNextPage.Enabled = false;
                    btnLastPage.Enabled = false;
                }
                else
                {
                    lblPageInfo.Text = $"Страница {currentPage} из {totalPages} (всего {totalRecords} записей)";
                    btnFirstPage.Enabled = currentPage > 1;
                    btnPrevPage.Enabled = currentPage > 1;
                    btnNextPage.Enabled = currentPage < totalPages;
                    btnLastPage.Enabled = currentPage < totalPages;
                }
            }
        }

        // ===================== ЗАГРУЗКА ДАННЫХ =====================

        /// <summary>
        /// Обновляет данные заказов с текущими фильтрами.
        /// </summary>
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

        /// <summary>
        /// Загружает заказы с применением фильтра по поисковому запросу.
        /// </summary>
        private void LoadOrdersWithFilter(string searchValue = "", bool exactMatch = false)
        {
            int statusId = -1;
            if (comboBoxOrderStatus.SelectedIndex > 0 && comboBoxOrderStatus.SelectedItem is StatusItem statusItem)
            {
                statusId = statusItem.Id;
            }
            LoadOrders(searchValue, statusId, exactMatch);
        }

        /// <summary>
        /// Загружает заказы из базы данных с применением фильтров.
        /// </summary>
        /// <param name="searchValue">Значение для поиска.</param>
        /// <param name="statusId">ID статуса для фильтрации (-1 = все статусы).</param>
        /// <param name="exactMatch">True для точного совпадения.</param>
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

                query += " AND o.id_status != 6";

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
                    fullDataTable = new DataTable();
                    dataAdapter.Fill(fullDataTable);

                    totalRecords = fullDataTable.Rows.Count;
                    totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                    if (currentPage > totalPages)
                        currentPage = totalPages > 0 ? totalPages : 1;

                    ApplyPagination();

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

        /// <summary>
        /// Загружает все заказы без фильтров.
        /// </summary>
        private void LoadOrders()
        {
            LoadOrders("", -1, false);
        }

        /// <summary>
        /// Загружает список статусов заказов в выпадающий список.
        /// </summary>
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

        // ===================== ОСНОВНАЯ ЛОГИКА СТАТУСОВ =====================

        /// <summary>
        /// Возвращает список допустимых статусов для перехода из текущего статуса.
        /// </summary>
        /// <param name="currentStatusId">ID текущего статуса.</param>
        /// <returns>Список ID допустимых статусов.</returns>
        private List<int> GetAllowedStatuses(int currentStatusId)
        {
            List<int> allowedStatuses = new List<int>();

            switch (currentStatusId)
            {
                case 2:
                    allowedStatuses.AddRange(new[] { 4, 5, 6, 7 });
                    break;

                case 4:
                    allowedStatuses.AddRange(new[] { 5, 6 });
                    break;

                case 5:
                    allowedStatuses.AddRange(new[] { 6 });
                    break;

                case 6:
                case 7:
                    break;

                default:
                    allowedStatuses.AddRange(new[] { 2, 4, 5 });
                    break;
            }

            return allowedStatuses;
        }

        /// <summary>
        /// Проверяет, разрешён ли переход из одного статуса в другой.
        /// </summary>
        /// <param name="currentStatusId">ID текущего статуса.</param>
        /// <param name="newStatusId">ID нового статуса.</param>
        /// <returns>True если переход разрешён.</returns>
        private bool IsStatusTransitionAllowed(int currentStatusId, int newStatusId)
        {
            if (currentStatusId == newStatusId)
                return true;

            if (newStatusId == 6 || newStatusId == 7)
            {
                List<int> allowed = GetAllowedStatuses(currentStatusId);
                return allowed.Contains(newStatusId);
            }

            List<int> allowedStatuses = GetAllowedStatuses(currentStatusId);
            return allowedStatuses.Contains(newStatusId);
        }

        /// <summary>
        /// Формирует сообщение об ошибке при запрещённом переходе статуса.
        /// </summary>
        private string GetStatusTransitionErrorMessage(int currentStatusId, string currentStatusName, int newStatusId, string newStatusName)
        {
            if (currentStatusId == newStatusId)
                return "Статус не изменился.";

            if (currentStatusId == 6)
                return $"Заказ уже доставлен. Нельзя изменить статус \"{currentStatusName}\".";

            if (currentStatusId == 7)
                return $"Заказ отменён. Нельзя изменить статус \"{currentStatusName}\".";

            if (newStatusId == 7 && currentStatusId != 2)
                return $"Нельзя отменить заказ со статусом \"{currentStatusName}\". Отменить можно только заказ со статусом \"Принят\".";

            if (newStatusId == 6 && currentStatusId == 2)
                return $"Нельзя сразу доставить заказ. Сначала переведите его в статус \"Готов\" или \"В пути\".";

            return $"Переход из статуса \"{currentStatusName}\" в статус \"{newStatusName}\" запрещен.";
        }

        // ===================== ОТОБРАЖЕНИЕ ДЕТАЛЕЙ ЗАКАЗА =====================

        /// <summary>
        /// Обработчик нажатия кнопки "Детальная инф.".
        /// Открывает форму с деталями заказа.
        /// </summary>
        private void ButtonDetail_Click(object sender, EventArgs e)
        {
            ShowOrderDetails();
        }

        /// <summary>
        /// Обработчик двойного клика по строке в DataGridView.
        /// Открывает форму с деталями заказа.
        /// </summary>
        private void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            ShowOrderDetails();
        }

        /// <summary>
        /// Отображает форму с подробной информацией о выбранном заказе.
        /// </summary>
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
                detailForm.Size = new Size(820, 720);
                detailForm.StartPosition = FormStartPosition.CenterParent;
                detailForm.FormBorderStyle = FormBorderStyle.Sizable;
                detailForm.MaximizeBox = true;
                detailForm.MinimizeBox = true;
                detailForm.BackColor = Color.White;
                detailForm.AutoScroll = true;
                detailForm.Font = new Font("Times New Roman", 12, FontStyle.Regular);

                Panel infoPanel = CreateInfoPanel(orderId, phoneNumber, address, persons, orderDate, paymentMethod);
                Panel statusPanel = CreateStatusPanelWithRestrictions(currentStatusId, currentStatus, statusState);
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
                        if (!IsStatusTransitionAllowed(currentStatusId, statusState.SelectedStatusId))
                        {
                            string errorMsg = GetStatusTransitionErrorMessage(
                                currentStatusId,
                                currentStatus,
                                statusState.SelectedStatusId,
                                statusState.SelectedStatusName
                            );

                            MessageBox.Show(errorMsg, "Изменение статуса запрещено",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            args.Cancel = true;
                            return;
                        }

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

        // ===================== СОЗДАНИЕ ПАНЕЛЕЙ ДЛЯ ФОРМЫ ДЕТАЛЕЙ =====================

        /// <summary>
        /// Создаёт панель с информацией о заказе.
        /// </summary>
        private Panel CreateInfoPanel(int orderId, string phoneNumber, string address,
            int persons, DateTime orderDate, string paymentMethod)
        {
            Panel panel = new Panel();
            panel.Size = new Size(765, 110);
            panel.BorderStyle = BorderStyle.FixedSingle;
            panel.BackColor = Color.FromArgb(255, 255, 220);

            string maskedPhone = MaskPhone(phoneNumber);

            Label lblInfo = new Label();
            lblInfo.Text = $"ЗАКАЗ №{orderId}\n" +
                          $"Телефон: {maskedPhone}\n" +
                          $"Адрес: {address}\n" +
                          $"Количество персон: {persons} | Дата доставки: {orderDate:dd.MM.yyyy}\n" +
                          $"Способ оплаты: {paymentMethod}";
            lblInfo.Location = new Point(10, 10);
            lblInfo.Size = new Size(740, 90);
            lblInfo.Font = new Font("Times New Roman", 11, FontStyle.Bold);
            lblInfo.ForeColor = Color.DarkRed;
            lblInfo.TextAlign = ContentAlignment.TopLeft;
            lblInfo.BackColor = Color.Transparent;

            panel.Controls.Add(lblInfo);
            return panel;
        }

        /// <summary>
        /// Создаёт панель для изменения статуса заказа с ограничениями.
        /// </summary>
        private Panel CreateStatusPanelWithRestrictions(int currentStatusId, string currentStatus, StatusState statusState)
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

            List<int> allowedStatusIds = GetAllowedStatuses(currentStatusId);

            foreach (var status in statusDictionary)
            {
                if (status.Key == currentStatusId || allowedStatusIds.Contains(status.Key))
                {
                    cmbNewStatus.Items.Add(new StatusItem(status.Key, status.Value));
                }
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

            if (cmbNewStatus.SelectedItem == null)
            {
                StatusItem currentItem = new StatusItem(currentStatusId, currentStatus);
                cmbNewStatus.Items.Insert(0, currentItem);
                cmbNewStatus.SelectedItem = currentItem;
                statusState.SelectedStatusId = currentItem.Id;
                statusState.SelectedStatusName = currentItem.Name;
            }

            if (cmbNewStatus.Items.Count <= 1)
            {
                cmbNewStatus.Enabled = false;
                cmbNewStatus.BackColor = Color.LightGray;

                Label lblNoChanges = new Label();
                lblNoChanges.Text = "✖ Изменение статуса невозможно";
                lblNoChanges.Location = new Point(590, 18);
                lblNoChanges.Size = new Size(160, 25);
                lblNoChanges.Font = new Font("Times New Roman", 10, FontStyle.Bold);
                lblNoChanges.ForeColor = Color.Red;
                lblNoChanges.TextAlign = ContentAlignment.MiddleLeft;
                lblNoChanges.BackColor = Color.Transparent;
                panel.Controls.Add(lblNoChanges);
            }
            else
            {
                string allowedNames = string.Join(", ", allowedStatusIds
                    .Where(id => statusDictionary.ContainsKey(id))
                    .Select(id => statusDictionary[id]));

                if (!string.IsNullOrEmpty(allowedNames))
                {
                    Label lblHint = new Label();
                    lblHint.Text = $"Доступно: {allowedNames}";
                    lblHint.Location = new Point(590, 18);
                    lblHint.Size = new Size(160, 25);
                    lblHint.Font = new Font("Times New Roman", 8, FontStyle.Italic);
                    lblHint.ForeColor = Color.DarkGreen;
                    lblHint.TextAlign = ContentAlignment.MiddleLeft;
                    lblHint.BackColor = Color.Transparent;
                    panel.Controls.Add(lblHint);
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

        /// <summary>
        /// Создаёт панель с комментарием к заказу.
        /// </summary>
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

        /// <summary>
        /// Создаёт панель с итоговой суммой заказа.
        /// </summary>
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

        // ===================== ЗАГРУЗКА ДЕТАЛЕЙ ЗАКАЗА =====================

        /// <summary>
        /// Загружает список блюд и подарков для указанного заказа.
        /// </summary>
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

        /// <summary>
        /// Создаёт DataTable для деталей заказа.
        /// </summary>
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

        /// <summary>
        /// Создаёт DataGridView для отображения деталей заказа.
        /// </summary>
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
            dgv.Font = new Font("Times New Roman", 14, FontStyle.Regular);
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.MultiSelect = false;
            dgv.EditMode = DataGridViewEditMode.EditProgrammatically;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(97, 173, 123);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersHeight = 50;

            dgv.RowsDefaultCellStyle.BackColor = Color.White;
            dgv.RowsDefaultCellStyle.ForeColor = Color.Black;
            dgv.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(233, 242, 236);
            dgv.RowsDefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.RowsDefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Regular);
            dgv.RowTemplate.Height = 45;
            dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            // ... (добавление колонок аналогично предыдущему коду)

            return dgv;
        }

        // ===================== ОБНОВЛЕНИЕ СТАТУСА =====================

        /// <summary>
        /// Обновляет статус заказа в базе данных.
        /// </summary>
        /// <param name="orderId">ID заказа.</param>
        /// <param name="newStatusId">ID нового статуса.</param>
        /// <param name="newStatusName">Название нового статуса.</param>
        /// <returns>True если обновление успешно.</returns>
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

        /// <summary>
        /// Обновляет статус заказа в DataGridView.
        /// </summary>
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

        // ===================== ИНИЦИАЛИЗАЦИЯ DATA GRID VIEW =====================

        /// <summary>
        /// Настраивает внешний вид и колонки DataGridView для отображения заказов.
        /// </summary>
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
            dataGridView1.RowTemplate.Height = 50;
            dataGridView1.RowTemplate.MinimumHeight = 45;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersHeight = 55;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView1.ScrollBars = ScrollBars.Both;

            Color headerBackColor = Color.FromArgb(97, 173, 123);

            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Bold);
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = headerBackColor;
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.ColumnHeadersDefaultCellStyle.Padding = new Padding(0, 5, 0, 5);
            dataGridView1.ColumnHeadersDefaultCellStyle.SelectionBackColor = headerBackColor;
            dataGridView1.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.Black;

            dataGridView1.DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Regular);
            dataGridView1.DefaultCellStyle.Padding = new Padding(0, 3, 0, 3);
            dataGridView1.DefaultCellStyle.BackColor = Color.White;
            dataGridView1.DefaultCellStyle.ForeColor = Color.Black;
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.FromArgb(233, 242, 236);
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.Black;

            dataGridView1.RowsDefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Regular);
            dataGridView1.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(233, 242, 236);
            dataGridView1.RowsDefaultCellStyle.SelectionForeColor = Color.Black;
            dataGridView1.RowsDefaultCellStyle.BackColor = Color.White;
            dataGridView1.RowsDefaultCellStyle.ForeColor = Color.Black;

            dataGridView1.GridColor = Color.Gray;
            dataGridView1.BorderStyle = BorderStyle.FixedSingle;
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.Single;

            dataGridView1.Columns.Clear();

            // ... (добавление колонок аналогично предыдущему коду)
        }

        /// <summary>
        /// Обработчик форматирования ячеек DataGridView.
        /// Форматирует дату и маскирует телефон.
        /// </summary>
        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // ... (код форматирования)
        }

        /// <summary>
        /// Настраивает стили колонок DataGridView.
        /// </summary>
        private void SetupColumnStyles()
        {
            // ... (код настройки стилей)
        }

        /// <summary>
        /// Настраивает DataGridView после загрузки данных.
        /// </summary>
        private void AdjustDataGridViewAfterLoad()
        {
            // ... (код настройки)
        }

        /// <summary>
        /// Обработчик изменения выбранного статуса в фильтре.
        /// Обновляет список заказов.
        /// </summary>
        private void comboBoxStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            // ... (код обработчика)
        }

        /// <summary>
        /// Обработчик нажатия кнопки сброса фильтров.
        /// Очищает поиск и сбрасывает статус.
        /// </summary>
        private void buttonReset_Click(object sender, EventArgs e)
        {
            ResetFilters();
        }

        /// <summary>
        /// Сбрасывает все фильтры и обновляет список заказов.
        /// </summary>
        private void ResetFilters()
        {
            // ... (код сброса)
        }

        /// <summary>
        /// Обработчик нажатия кнопки выхода (крестик).
        /// Скрывает текущую форму и открывает форму менеджера.
        /// </summary>
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            ManagerForm Manager = new ManagerForm();
            Manager.Show();
        }

        private void Orders_Load(object sender, EventArgs e) { }
        private void Orders_Load_1(object sender, EventArgs e) { }

        // ===================== ВСПОМОГАТЕЛЬНЫЕ КЛАССЫ =====================

        /// <summary>
        /// Класс для хранения состояния выбранного статуса.
        /// </summary>
        private class StatusState
        {
            public int SelectedStatusId { get; set; }
            public string SelectedStatusName { get; set; }
        }

        /// <summary>
        /// Класс для хранения информации о позиции заказа (блюдо/подарок).
        /// </summary>
        private class OrderDetailItem
        {
            public string Name { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public decimal TotalPrice { get; set; }
            public bool IsGift { get; set; }
            public string DisplayName => IsGift ? $"🎁 {Name} (Подарок)" : Name;
        }

        /// <summary>
        /// Класс для представления статуса в выпадающем списке.
        /// </summary>
        public class StatusItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public StatusItem(int id, string name) { Id = id; Name = name; }
            public override string ToString() { return Name; }
        }
    }
}