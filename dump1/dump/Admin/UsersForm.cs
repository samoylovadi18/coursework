using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace dump
{
    public partial class UsersForm : Form
    {
        private DataTable usersTable;
        private DataTable rolesTable;
        private MySqlDataAdapter dataAdapter;
        private BindingSource bindingSource;

        private bool isEditMode = false;
        private bool isNewUser = false;
        private int currentUserId = 0;
        private int currentAdminId = 0;

        private string allowedSpecialChars = "!@#$%^&*()_-+=[]{}|;:'\",.<>?/`~";
        private bool isFormattingFIO = false;
        private bool isFormattingSearch = false;
        private System.Windows.Forms.ToolTip toolTip1;

        private void InitializeEditPanelAppearance()
        {
            panelEdit.BorderStyle = BorderStyle.None;
            panelEdit.BackColor = Color.WhiteSmoke;
            panelEdit.Paint += PanelEdit_Paint;
        }

        private void PanelEdit_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, panelEdit.ClientRectangle,
                Color.DarkGray, 4, ButtonBorderStyle.Solid,
                Color.DarkGray, 4, ButtonBorderStyle.Solid,
                Color.DarkGray, 4, ButtonBorderStyle.Solid,
                Color.DarkGray, 4, ButtonBorderStyle.Solid);
        }

        public UsersForm()
        {
            InitializeComponent();
            InitializeEditPanelAppearance();
            InitializeButtons();
            InitializeDataGridView();
            InitializeSearchAndFilter();
            InitializeInputValidation();

            toolTip1 = new System.Windows.Forms.ToolTip();
            currentAdminId = CurrentUser.UserId;

            if (CurrentUser.RoleId != 3)
            {
                MessageBox.Show("У вас нет прав для доступа к управлению пользователями!",
                    "Доступ запрещен", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            panelEdit.Visible = false;
            isEditMode = false;

            LoadUsers();
            LoadRolesForFilter();

            dataGridViewUsers.SelectionChanged += DataGridViewUsers_SelectionChanged;

            // ПОДПИСЫВАЕМСЯ НА СОБЫТИЕ ЗАКРЫТИЯ ФОРМЫ
            this.FormClosing += UsersForm_FormClosing;
        }

        // НОВЫЙ ОБРАБОТЧИК - при нажатии на крестик
        private void UsersForm_FormClosing(object sender, FormClosingEventArgs e)
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

        private void InitializeInputValidation()
        {
            // Для поля ФИО - только русские буквы, дефис, пробелы (макс 2)
            textBoxFIO.MaxLength = 100;
            textBoxFIO.KeyPress += TextBoxFIO_KeyPress;
            textBoxFIO.Validating += TextBoxFIO_Validating;
            textBoxFIO.TextChanged += TextBoxFIO_TextChanged;
            textBoxFIO.Leave += TextBoxFIO_Leave;

            // Для поля Логин
            textBoxLogin.MaxLength = 50;
            textBoxLogin.KeyPress += TextBoxLogin_KeyPress;
            textBoxLogin.Validating += TextBoxLogin_Validating;

            // Для поля Пароль
            textBoxPassword.MaxLength = 50;
            textBoxPassword.KeyPress += TextBoxPassword_KeyPress;
            textBoxPassword.Validating += TextBoxPassword_Validating;

            // ДЛЯ ПОЛЯ ПОИСКА - русские буквы, пробелы, макс 2 пробела
            textBoxSearch.MaxLength = 100;
            textBoxSearch.KeyPress += TextBoxSearch_KeyPress;
            textBoxSearch.TextChanged += TextBoxSearch_TextChanged;
            textBoxSearch.Leave += TextBoxSearch_Leave;
        }

        // KeyPress для ФИО - только русские буквы, дефис и пробелы (макс 2)
        private void TextBoxFIO_KeyPress(object sender, KeyPressEventArgs e)
        {
            System.Windows.Forms.TextBox textBox = sender as System.Windows.Forms.TextBox;

            if (e.KeyChar == (char)Keys.Back)
                return;

            // Разрешаем пробел
            if (e.KeyChar == ' ')
            {
                int currentSpaceCount = textBox.Text.Count(c => c == ' ');
                if (currentSpaceCount < 2)
                    return;
                else
                {
                    e.Handled = true;
                    System.Media.SystemSounds.Beep.Play();
                    return;
                }
            }

            // Разрешаем дефис
            if (e.KeyChar == '-')
                return;

            // Разрешаем русские буквы
            if (IsRussianLetter(e.KeyChar))
                return;

            // Все остальные символы запрещены
            e.Handled = true;
        }

        // KeyPress для ПОИСКА - только русские буквы и пробелы (макс 2)
        private void TextBoxSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            System.Windows.Forms.TextBox textBox = sender as System.Windows.Forms.TextBox;

            if (e.KeyChar == (char)Keys.Back)
                return;

            // Разрешаем пробел
            if (e.KeyChar == ' ')
            {
                int currentSpaceCount = textBox.Text.Count(c => c == ' ');
                if (currentSpaceCount < 2)
                    return;
                else
                {
                    e.Handled = true;
                    System.Media.SystemSounds.Beep.Play();
                    return;
                }
            }

            // Разрешаем русские буквы
            if (IsRussianLetter(e.KeyChar))
                return;

            // Все остальные символы запрещены
            e.Handled = true;
        }

        // TextChanged для ФИО - форматирование
        private void TextBoxFIO_TextChanged(object sender, EventArgs e)
        {
            if (isFormattingFIO) return;

            isFormattingFIO = true;

            try
            {
                System.Windows.Forms.TextBox textBox = sender as System.Windows.Forms.TextBox;
                string text = textBox.Text;
                int cursorPosition = textBox.SelectionStart;

                if (!string.IsNullOrEmpty(text))
                {
                    string formattedText = FormatFIOInRealTime(text);
                    if (formattedText != text)
                    {
                        textBox.Text = formattedText;
                        if (cursorPosition <= formattedText.Length)
                            textBox.SelectionStart = cursorPosition;
                        else
                            textBox.SelectionStart = formattedText.Length;
                    }
                }
            }
            finally
            {
                isFormattingFIO = false;
            }
        }

        // TextChanged для ПОИСКА - форматирование (первая буква заглавная)
        private void TextBoxSearch_TextChanged(object sender, EventArgs e)
        {
            if (isFormattingSearch) return;

            isFormattingSearch = true;

            try
            {
                System.Windows.Forms.TextBox textBox = sender as System.Windows.Forms.TextBox;
                string text = textBox.Text;
                int cursorPosition = textBox.SelectionStart;

                if (!string.IsNullOrEmpty(text))
                {
                    string formattedText = FormatSearchText(text);
                    if (formattedText != text)
                    {
                        textBox.Text = formattedText;
                        if (cursorPosition <= formattedText.Length)
                            textBox.SelectionStart = cursorPosition;
                        else
                            textBox.SelectionStart = formattedText.Length;
                    }
                }

                // Применяем фильтры после форматирования
                ApplyFilters();
            }
            finally
            {
                isFormattingSearch = false;
            }
        }

        // Форматирование текста поиска (первая буква каждого слова заглавная)
        private string FormatSearchText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            text = text.ToLower();
            char[] chars = text.ToCharArray();

            // Первая буква заглавная
            if (chars.Length > 0 && IsRussianLetter(chars[0]))
                chars[0] = char.ToUpper(chars[0]);

            // После пробела буква заглавная
            for (int i = 1; i < chars.Length; i++)
            {
                if (chars[i - 1] == ' ' && IsRussianLetter(chars[i]))
                    chars[i] = char.ToUpper(chars[i]);
            }

            return new string(chars);
        }

        // Форматирование ФИО в реальном времени
        private string FormatFIOInRealTime(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            char[] chars = text.ToCharArray();

            if (chars.Length > 0 && IsRussianLetter(chars[0]))
                chars[0] = char.ToUpper(chars[0]);

            for (int i = 1; i < chars.Length; i++)
            {
                if ((chars[i - 1] == ' ' || chars[i - 1] == '-') && IsRussianLetter(chars[i]))
                    chars[i] = char.ToUpper(chars[i]);
                else if (IsRussianLetter(chars[i]) && i > 0 && !IsWordSeparator(chars[i - 1]))
                    chars[i] = char.ToLower(chars[i]);
            }

            return new string(chars);
        }

        // Потеря фокуса для ПОИСКА
        private void TextBoxSearch_Leave(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBoxSearch.Text))
            {
                string formattedText = FormatSearchText(textBoxSearch.Text);
                if (formattedText != textBoxSearch.Text)
                    textBoxSearch.Text = formattedText;
            }
        }

        private bool IsWordSeparator(char c)
        {
            return c == ' ' || c == '-';
        }

        private void TextBoxFIO_Validating(object sender, CancelEventArgs e)
        {
            string text = textBoxFIO.Text.Trim();

            if (text.Length > 100)
            {
                MessageBox.Show("ФИО не может превышать 100 символов!",
                    "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxFIO.Focus();
                textBoxFIO.SelectAll();
                e.Cancel = true;
                return;
            }

            if (!string.IsNullOrEmpty(text))
            {
                if (!Regex.IsMatch(text, @"^[а-яА-ЯёЁ\s\-]+$"))
                {
                    MessageBox.Show("ФИО может содержать только русские буквы, пробелы и дефисы!",
                        "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxFIO.Focus();
                    textBoxFIO.SelectAll();
                    e.Cancel = true;
                }
            }
        }

        private void TextBoxFIO_Leave(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBoxFIO.Text))
            {
                string formattedFIO = FormatFIO(textBoxFIO.Text);
                if (formattedFIO != textBoxFIO.Text)
                    textBoxFIO.Text = formattedFIO;
            }
        }

        // Полное форматирование ФИО
        private string FormatFIO(string fio)
        {
            if (string.IsNullOrWhiteSpace(fio))
                return fio;

            string[] words = fio.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpper(words[i][0]) +
                              (words[i].Length > 1 ? words[i].Substring(1).ToLower() : "");
                }
            }
            return string.Join(" ", words);
        }

        // Для логина и пароля - английские буквы, цифры, спецсимволы (БЕЗ РУССКИХ)
        private void TextBoxLogin_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar))
                return;

            if (!IsValidCharacter(e.KeyChar))
                e.Handled = true;
        }

        private void TextBoxLogin_Validating(object sender, CancelEventArgs e)
        {
            string text = textBoxLogin.Text.Trim();

            if (text.Length > 50)
            {
                MessageBox.Show("Логин не может превышать 50 символов!", "Ошибка ввода",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxLogin.Focus();
                textBoxLogin.SelectAll();
                e.Cancel = true;
                return;
            }

            if (!string.IsNullOrEmpty(text))
            {
                if (text.Contains(" "))
                {
                    MessageBox.Show("Логин не должен содержать пробелов!", "Ошибка ввода",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxLogin.Focus();
                    textBoxLogin.SelectAll();
                    e.Cancel = true;
                    return;
                }

                if (ContainsInvalidCharacters(text))
                {
                    MessageBox.Show("Логин может содержать только:\n" +
                                  "• Английские буквы (A-Z, a-z)\n" +
                                  "• Цифры (0-9)\n" +
                                  "• Специальные символы: " + allowedSpecialChars,
                                  "Недопустимые символы", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxLogin.Focus();
                    textBoxLogin.SelectAll();
                    e.Cancel = true;
                }
            }
        }

        private void TextBoxPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar))
                return;

            if (!IsValidCharacter(e.KeyChar))
                e.Handled = true;
        }

        private void TextBoxPassword_Validating(object sender, CancelEventArgs e)
        {
            string text = textBoxPassword.Text;

            if (text.Length > 50)
            {
                MessageBox.Show("Пароль не может превышать 50 символов!", "Ошибка ввода",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxPassword.Focus();
                textBoxPassword.SelectAll();
                e.Cancel = true;
                return;
            }

            if (!string.IsNullOrEmpty(text))
            {
                if (text.Contains(" "))
                {
                    MessageBox.Show("Пароль не должен содержать пробелов!", "Ошибка ввода",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxPassword.Focus();
                    textBoxPassword.SelectAll();
                    e.Cancel = true;
                    return;
                }

                if (ContainsInvalidCharacters(text))
                {
                    MessageBox.Show("Пароль может содержать только:\n" +
                                  "• Английские буквы (A-Z, a-z)\n" +
                                  "• Цифры (0-9)\n" +
                                  "• Специальные символы: " + allowedSpecialChars,
                                  "Недопустимые символы", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxPassword.Focus();
                    textBoxPassword.SelectAll();
                    e.Cancel = true;
                }
            }
        }

        private bool IsValidCharacter(char c)
        {
            // Запрещаем русские буквы
            if ((c >= 'а' && c <= 'я') || (c >= 'А' && c <= 'Я'))
                return false;

            if (c == ' ')
                return false;

            if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
                return true;

            if (c >= '0' && c <= '9')
                return true;

            if (allowedSpecialChars.Contains(c))
                return true;

            return false;
        }

        private bool IsRussianLetter(char c)
        {
            return (c >= 'а' && c <= 'я') || (c >= 'А' && c <= 'Я') || c == 'ё' || c == 'Ё';
        }

        private bool ContainsInvalidCharacters(string text)
        {
            foreach (char c in text)
            {
                if (!IsValidCharacter(c))
                    return true;
            }
            return false;
        }

        private void InitializeSearchAndFilter()
        {
            // textBoxSearch уже подписан на TextChanged в InitializeInputValidation
            comboBoxRoleSort.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxRoleSort.SelectedIndexChanged += comboBoxRoleSort_SelectedIndexChanged;
        }

        private void InitializeButtons()
        {
            buttonAdd.FlatStyle = FlatStyle.Flat;
            buttonAdd.FlatAppearance.BorderSize = 1;
            buttonAdd.FlatAppearance.BorderColor = Color.Black;
            buttonAdd.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            buttonAdd.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;
            buttonAdd.MouseDown += (s, e) => buttonAdd.FlatAppearance.BorderColor = Color.DarkBlue;
            buttonAdd.MouseUp += (s, e) => buttonAdd.FlatAppearance.BorderColor = Color.Black;
            buttonAdd.MouseLeave += (s, e) => buttonAdd.FlatAppearance.BorderColor = Color.Black;

            buttonSave.FlatStyle = FlatStyle.Flat;
            buttonSave.FlatAppearance.BorderSize = 1;
            buttonSave.FlatAppearance.BorderColor = Color.Black;
            buttonSave.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            buttonSave.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            buttonEdit.FlatStyle = FlatStyle.Flat;
            buttonEdit.FlatAppearance.BorderSize = 1;
            buttonEdit.FlatAppearance.BorderColor = Color.Black;
            buttonEdit.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            buttonEdit.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;
            buttonEdit.MouseDown += (s, e) => buttonEdit.FlatAppearance.BorderColor = Color.DarkBlue;
            buttonEdit.MouseUp += (s, e) => buttonEdit.FlatAppearance.BorderColor = Color.Black;
            buttonEdit.MouseLeave += (s, e) => buttonEdit.FlatAppearance.BorderColor = Color.Black;

            buttonDelete.FlatStyle = FlatStyle.Flat;
            buttonDelete.FlatAppearance.BorderSize = 1;
            buttonDelete.FlatAppearance.BorderColor = Color.Black;
            buttonDelete.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            buttonDelete.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;
            buttonDelete.MouseDown += (s, e) => buttonDelete.FlatAppearance.BorderColor = Color.DarkBlue;
            buttonDelete.MouseUp += (s, e) => buttonDelete.FlatAppearance.BorderColor = Color.Black;
            buttonDelete.MouseLeave += (s, e) => buttonDelete.FlatAppearance.BorderColor = Color.Black;

            btnResetFilter.FlatStyle = FlatStyle.Flat;
            btnResetFilter.FlatAppearance.BorderSize = 1;
            btnResetFilter.FlatAppearance.BorderColor = Color.Black;
            btnResetFilter.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            btnResetFilter.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;
            btnResetFilter.MouseDown += (s, e) => btnResetFilter.FlatAppearance.BorderColor = Color.DarkBlue;
            btnResetFilter.MouseUp += (s, e) => btnResetFilter.FlatAppearance.BorderColor = Color.Black;
            btnResetFilter.MouseLeave += (s, e) => btnResetFilter.FlatAppearance.BorderColor = Color.Black;
        }

        private void InitializeDataGridView()
        {
            dataGridViewUsers.ShowCellToolTips = false;
            dataGridViewUsers.AutoGenerateColumns = false;
            dataGridViewUsers.AllowUserToAddRows = false;
            dataGridViewUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewUsers.ReadOnly = true;
            dataGridViewUsers.MultiSelect = false;
            dataGridViewUsers.RowHeadersVisible = false;
            dataGridViewUsers.EnableHeadersVisualStyles = false;
            dataGridViewUsers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            Color headerBackColor = Color.FromArgb(97, 173, 123);
            Color selectionColor = Color.FromArgb(233, 242, 236);

            dataGridViewUsers.ColumnHeadersDefaultCellStyle.BackColor = headerBackColor;
            dataGridViewUsers.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dataGridViewUsers.ColumnHeadersDefaultCellStyle.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            dataGridViewUsers.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewUsers.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridViewUsers.ColumnHeadersDefaultCellStyle.Padding = new Padding(0, 3, 0, 3);
            dataGridViewUsers.ColumnHeadersHeight = 45;
            dataGridViewUsers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            dataGridViewUsers.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            dataGridViewUsers.DefaultCellStyle.Padding = new Padding(5);
            dataGridViewUsers.DefaultCellStyle.BackColor = Color.White;
            dataGridViewUsers.DefaultCellStyle.ForeColor = Color.Black;
            dataGridViewUsers.DefaultCellStyle.SelectionBackColor = selectionColor;
            dataGridViewUsers.DefaultCellStyle.SelectionForeColor = Color.Black;

            dataGridViewUsers.RowsDefaultCellStyle.BackColor = Color.White;
            dataGridViewUsers.RowsDefaultCellStyle.ForeColor = Color.Black;
            dataGridViewUsers.RowsDefaultCellStyle.SelectionBackColor = selectionColor;
            dataGridViewUsers.RowsDefaultCellStyle.SelectionForeColor = Color.Black;

            dataGridViewUsers.RowTemplate.Height = 35;
            dataGridViewUsers.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridViewUsers.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            dataGridViewUsers.GridColor = Color.Gray;
            dataGridViewUsers.BorderStyle = BorderStyle.Fixed3D;
            dataGridViewUsers.CellBorderStyle = DataGridViewCellBorderStyle.Single;

            dataGridViewUsers.Columns.Clear();

            DataGridViewTextBoxColumn colId = new DataGridViewTextBoxColumn();
            colId.Name = "id_user";
            colId.DataPropertyName = "id_user";
            colId.Visible = false;
            colId.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridViewUsers.Columns.Add(colId);

            DataGridViewTextBoxColumn colFIO = new DataGridViewTextBoxColumn();
            colFIO.Name = "FIO";
            colFIO.HeaderText = "ФИО";
            colFIO.DataPropertyName = "FIO";
            colFIO.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colFIO.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridViewUsers.Columns.Add(colFIO);

            DataGridViewTextBoxColumn colLogin = new DataGridViewTextBoxColumn();
            colLogin.Name = "login";
            colLogin.HeaderText = "Логин";
            colLogin.DataPropertyName = "login";
            colLogin.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colLogin.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridViewUsers.Columns.Add(colLogin);

            DataGridViewTextBoxColumn colRoleId = new DataGridViewTextBoxColumn();
            colRoleId.Name = "id_role";
            colRoleId.DataPropertyName = "id_role";
            colRoleId.Visible = false;
            colRoleId.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridViewUsers.Columns.Add(colRoleId);

            DataGridViewTextBoxColumn colRole = new DataGridViewTextBoxColumn();
            colRole.Name = "role_name";
            colRole.HeaderText = "Роль";
            colRole.DataPropertyName = "role_name";
            colRole.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colRole.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridViewUsers.Columns.Add(colRole);
        }

        private void DataGridViewUsers_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewUsers.SelectedRows.Count > 0)
            {
                DataRowView selectedRow = (DataRowView)bindingSource.Current;
                if (selectedRow != null)
                {
                    int userId = Convert.ToInt32(selectedRow["id_user"]);
                    if (userId == currentAdminId)
                    {
                        buttonDelete.Enabled = false;
                        buttonDelete.FlatAppearance.BorderColor = Color.DarkGray;
                    }
                    else
                    {
                        buttonDelete.Enabled = true;
                        buttonDelete.FlatAppearance.BorderColor = Color.Black;
                    }
                }
            }
            else
            {
                buttonDelete.Enabled = false;
                buttonDelete.FlatAppearance.BorderColor = Color.DarkGray;
            }
        }

        private void LoadRolesForFilter()
        {
            try
            {
                DataTable filterRoles = new DataTable();
                string query = "SELECT id_role, role_name FROM roles ORDER BY role_name";

                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            filterRoles.Load(reader);
                        }
                    }
                }

                DataRow allRolesRow = filterRoles.NewRow();
                allRolesRow["id_role"] = 0;
                allRolesRow["role_name"] = "Все роли";
                filterRoles.Rows.InsertAt(allRolesRow, 0);

                comboBoxRoleSort.DataSource = filterRoles;
                comboBoxRoleSort.DisplayMember = "role_name";
                comboBoxRoleSort.ValueMember = "id_role";
                comboBoxRoleSort.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ролей для фильтра: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Загрузка пользователей с поиском по ФИО
        private void LoadUsers(string searchText = "", int roleId = 0)
        {
            try
            {
                string query = @"
                    SELECT u.id_user, u.FIO, u.login, 
                           u.id_role, r.role_name 
                    FROM users u 
                    LEFT JOIN roles r ON u.id_role = r.id_role 
                    WHERE 1=1";

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query += " AND u.FIO LIKE @search";
                }

                if (roleId > 0)
                {
                    query += " AND u.id_role = @roleId";
                }

                query += " ORDER BY u.FIO";

                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();
                    dataAdapter = new MySqlDataAdapter(query, connection);

                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        dataAdapter.SelectCommand.Parameters.AddWithValue("@search", $"%{searchText}%");
                    }

                    if (roleId > 0)
                    {
                        dataAdapter.SelectCommand.Parameters.AddWithValue("@roleId", roleId);
                    }

                    usersTable = new DataTable();
                    dataAdapter.Fill(usersTable);

                    bindingSource = new BindingSource();
                    bindingSource.DataSource = usersTable;
                    dataGridViewUsers.DataSource = bindingSource;

                    labelRecordCount.Text = $"Записей: {usersTable.Rows.Count}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void comboBoxRoleSort_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxRoleSort.SelectedIndex >= 0)
                ApplyFilters();
        }

        private void ApplyFilters()
        {
            string searchText = textBoxSearch.Text.Trim();
            int selectedRoleId = 0;

            if (comboBoxRoleSort.SelectedValue != null && comboBoxRoleSort.SelectedIndex > 0)
                selectedRoleId = Convert.ToInt32(comboBoxRoleSort.SelectedValue);

            LoadUsers(searchText, selectedRoleId);
        }

        private void ShowEditPanel()
        {
            if (!isEditMode)
            {
                panelEdit.Visible = true;
                isEditMode = true;
                if (isNewUser)
                {
                    addLabel.Visible = true;
                    editLabel.Visible = false;
                }
                else
                {
                    editLabel.Visible = true;
                    addLabel.Visible = false;
                }

                buttonAdd.Enabled = false;
                buttonEdit.Enabled = false;
                buttonDelete.Enabled = false;
                dataGridViewUsers.Enabled = false;
            }
        }

        private void HideEditPanel()
        {
            panelEdit.Visible = false;
            isEditMode = false;
            addLabel.Visible = false;
            editLabel.Visible = false;

            buttonAdd.Enabled = true;
            buttonEdit.Enabled = true;
            buttonDelete.Enabled = true;
            dataGridViewUsers.Enabled = true;
            ClearEditFields();
        }

        private void ClearEditFields()
        {
            textBoxFIO.Text = "";
            textBoxLogin.Text = "";
            textBoxPassword.Text = "";
            if (comboBoxRole != null)
                comboBoxRole.SelectedIndex = -1;
        }

        private void LoadEditFields()
        {
            if (!isNewUser && currentUserId > 0)
            {
                DataRow[] rows = usersTable.Select($"id_user = {currentUserId}");
                if (rows.Length > 0)
                {
                    DataRow row = rows[0];
                    textBoxFIO.Text = FormatFIO(row["FIO"].ToString());
                    textBoxLogin.Text = row["login"].ToString();

                    if (comboBoxRole != null && rolesTable != null)
                    {
                        comboBoxRole.DataSource = rolesTable;
                        comboBoxRole.DisplayMember = "role_name";
                        comboBoxRole.ValueMember = "id_role";

                        int roleId = Convert.ToInt32(row["id_role"]);
                        comboBoxRole.SelectedValue = roleId;
                    }
                }
            }
            else
            {
                if (comboBoxRole != null && rolesTable != null)
                {
                    comboBoxRole.DataSource = rolesTable;
                    comboBoxRole.DisplayMember = "role_name";
                    comboBoxRole.ValueMember = "id_role";
                }
            }
        }

        private DataTable LoadRoles()
        {
            DataTable roles = new DataTable();
            try
            {
                string query = "SELECT id_role, role_name FROM roles ORDER BY role_name";
                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            roles.Load(reader);
                        }
                    }
                }
            }
            catch (Exception)
            {
                roles.Columns.Add("id_role", typeof(int));
                roles.Columns.Add("role_name", typeof(string));
                roles.Rows.Add(1, "Администратор");
                roles.Rows.Add(2, "Менеджер");
                roles.Rows.Add(3, "Пользователь");
            }
            return roles;
        }

        private string ComputeSHA256Hash(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);

                StringBuilder builder = new StringBuilder();
                foreach (byte b in hash)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private int CountAdminsExcludingUser(int excludeUserId)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM users WHERE id_role = 3 AND id_user != @ExcludeUserId";
                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@ExcludeUserId", excludeUserId);
                        object result = cmd.ExecuteScalar();
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подсчете администраторов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            isNewUser = true;
            currentUserId = 0;
            ShowEditPanel();
            LoadEditFields();
            textBoxPassword.Text = "";
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            if (dataGridViewUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите пользователя для редактирования!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataRowView selectedRow = (DataRowView)bindingSource.Current;
            currentUserId = Convert.ToInt32(selectedRow["id_user"]);
            isNewUser = false;
            ShowEditPanel();
            LoadEditFields();
            textBoxPassword.Text = "";
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите пользователя для удаления!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataRowView selectedRow = (DataRowView)bindingSource.Current;
            int userId = Convert.ToInt32(selectedRow["id_user"]);
            string userName = selectedRow["FIO"].ToString();
            int userRoleId = Convert.ToInt32(selectedRow["id_role"]);

            if (userId == currentAdminId)
            {
                MessageBox.Show("Вы не можете удалить свою собственную учетную запись!",
                    "Невозможно удалить", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (userRoleId == 3)
            {
                int remainingAdmins = CountAdminsExcludingUser(userId);
                if (remainingAdmins == 0)
                {
                    MessageBox.Show("Нельзя удалить последнего администратора в системе!",
                        "Ошибка удаления", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            DialogResult result = MessageBox.Show($"Вы уверены, что хотите удалить пользователя '{userName}'?",
                "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    string query = "DELETE FROM users WHERE id_user = @UserId";
                    using (MySqlConnection connection = SettingsBD.GetConnection())
                    {
                        connection.Open();
                        using (MySqlCommand cmd = new MySqlCommand(query, connection))
                        {
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    ApplyFilters();
                    MessageBox.Show("Пользователь успешно удален!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления пользователя: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (!ValidateChildren(ValidationConstraints.Enabled))
            {
                MessageBox.Show("Пожалуйста, исправьте ошибки ввода!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (textBoxFIO.Text.Trim().Length > 100)
            {
                MessageBox.Show("ФИО не может превышать 100 символов!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxFIO.Focus();
                return;
            }

            if (textBoxLogin.Text.Trim().Length > 50)
            {
                MessageBox.Show("Логин не может превышать 50 символов!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxLogin.Focus();
                return;
            }

            if (textBoxPassword.Text.Length > 50)
            {
                MessageBox.Show("Пароль не может превышать 50 символов!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxPassword.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(textBoxFIO.Text))
            {
                MessageBox.Show("Введите ФИО пользователя!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBoxLogin.Text))
            {
                MessageBox.Show("Введите логин пользователя!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (isNewUser && string.IsNullOrWhiteSpace(textBoxPassword.Text))
            {
                MessageBox.Show("Введите пароль для нового пользователя!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (comboBoxRole.SelectedValue == null)
            {
                MessageBox.Show("Выберите роль пользователя!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string fio = textBoxFIO.Text.Trim();
            int spaceCount = fio.Count(c => c == ' ');
            if (spaceCount > 2)
            {
                MessageBox.Show("ФИО должно содержать не более 2 пробелов!\nФормат: Фамилия Имя Отчество",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxFIO.Focus();
                return;
            }

            if (fio.Contains("  "))
            {
                MessageBox.Show("ФИО не должно содержать двойных пробелов!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxFIO.Focus();
                return;
            }

            string[] words = fio.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length != 3)
            {
                MessageBox.Show("ФИО должно содержать ровно три слова!\nФормат: Фамилия Имя Отчество",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxFIO.Focus();
                return;
            }

            try
            {
                string formattedFio = FormatFIO(fio);
                string login = textBoxLogin.Text.Trim();
                string password = textBoxPassword.Text;
                int roleId = Convert.ToInt32(comboBoxRole.SelectedValue);

                if (isNewUser)
                {
                    string passwordHash = ComputeSHA256Hash(password);
                    string query = @"INSERT INTO users (FIO, id_role, login, password_hash) 
                           VALUES (@FIO, @RoleId, @Login, @PasswordHash)";

                    using (MySqlConnection connection = SettingsBD.GetConnection())
                    {
                        connection.Open();
                        using (MySqlCommand cmd = new MySqlCommand(query, connection))
                        {
                            cmd.Parameters.AddWithValue("@FIO", formattedFio);
                            cmd.Parameters.AddWithValue("@RoleId", roleId);
                            cmd.Parameters.AddWithValue("@Login", login);
                            cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    MessageBox.Show("Пользователь успешно добавлен!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        string passwordHash = ComputeSHA256Hash(password);
                        string query = @"UPDATE users SET FIO = @FIO, id_role = @RoleId, 
                              login = @Login, password_hash = @PasswordHash 
                              WHERE id_user = @UserId";

                        using (MySqlConnection connection = SettingsBD.GetConnection())
                        {
                            connection.Open();
                            using (MySqlCommand cmd = new MySqlCommand(query, connection))
                            {
                                cmd.Parameters.AddWithValue("@FIO", formattedFio);
                                cmd.Parameters.AddWithValue("@RoleId", roleId);
                                cmd.Parameters.AddWithValue("@Login", login);
                                cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                                cmd.Parameters.AddWithValue("@UserId", currentUserId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    else
                    {
                        string query = @"UPDATE users SET FIO = @FIO, id_role = @RoleId, 
                              login = @Login WHERE id_user = @UserId";

                        using (MySqlConnection connection = SettingsBD.GetConnection())
                        {
                            connection.Open();
                            using (MySqlCommand cmd = new MySqlCommand(query, connection))
                            {
                                cmd.Parameters.AddWithValue("@FIO", formattedFio);
                                cmd.Parameters.AddWithValue("@RoleId", roleId);
                                cmd.Parameters.AddWithValue("@Login", login);
                                cmd.Parameters.AddWithValue("@UserId", currentUserId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    MessageBox.Show("Пользователь успешно обновлен!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                ApplyFilters();
                HideEditPanel();
            }
            catch (MySqlException mysqlEx)
            {
                if (mysqlEx.Number == 1062)
                {
                    string errorMessage = GetUserFriendlyDuplicateError(mysqlEx.Message, textBoxLogin.Text);
                    MessageBox.Show(errorMessage, "Ошибка дублирования",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show($"Ошибка базы данных: {mysqlEx.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetUserFriendlyDuplicateError(string mysqlError, string login)
        {
            if (mysqlError.Contains("users.login"))
                return $"Пользователь с логином '{login}' уже существует!\nПожалуйста, выберите другой логин.";
            else if (mysqlError.Contains("PRIMARY") || mysqlError.Contains("id_user"))
                return "Ошибка дублирования ID пользователя. Обратитесь к администратору.";
            else if (mysqlError.Contains("users.FIO"))
                return "Пользователь с таким ФИО уже существует.";
            else
                return $"Запись уже существует в базе данных.\nЛогин '{login}' уже занят.";
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            HideEditPanel();
        }

        private void UsersForm_Load(object sender, EventArgs e)
        {
            rolesTable = LoadRoles();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            AdminForm admin = new AdminForm();
            admin.Show();
        }

        private void btnResetFilter_Click(object sender, EventArgs e)
        {
            textBoxSearch.Text = "";
            comboBoxRoleSort.SelectedIndex = 0;
        }
    }
}