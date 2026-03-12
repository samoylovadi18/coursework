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
        private int currentAdminId = 0; // ID текущего пользователя

        // Список разрешенных специальных символов (такой же как в LoginForm)
        private string allowedSpecialChars = "!@#$%^&*()_-+=[]{}|;:'\",.<>?/`~";

        // Флаг для предотвращения рекурсивного вызова TextChanged
        private bool isFormattingFIO = false;

        private System.Windows.Forms.ToolTip toolTip1;

        //внешний вид панели
        private void InitializeEditPanelAppearance()
        {
            panelEdit.BorderStyle = BorderStyle.None;  // Убираем стандартную рамку
            panelEdit.BackColor = Color.WhiteSmoke;
            panelEdit.Paint += PanelEdit_Paint;  // Добавляем обработчик отрисовки
        }

        private void PanelEdit_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, panelEdit.ClientRectangle,
                Color.DarkGray, 4, ButtonBorderStyle.Solid,    // верх
                Color.DarkGray, 4, ButtonBorderStyle.Solid,    // право
                Color.DarkGray, 4, ButtonBorderStyle.Solid,    // низ
                Color.DarkGray, 4, ButtonBorderStyle.Solid);   // лево
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

            // ИСПРАВЛЕНО: получаем ID текущего пользователя из CurrentUser
            currentAdminId = CurrentUser.UserId;

            // Проверяем, является ли текущий пользователь администратором
            if (CurrentUser.RoleId != 3)
            {
                MessageBox.Show("У вас нет прав для доступа к управлению пользователями!",
                    "Доступ запрещен", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            // Скрываем панель сразу при инициализации
            panelEdit.Visible = false;
            isEditMode = false;

            LoadUsers();
            LoadRolesForFilter();

            // Добавляем обработчик для отображения информации о выбранном пользователе
            dataGridViewUsers.SelectionChanged += DataGridViewUsers_SelectionChanged;
        }

        // Инициализация контроля ввода
        private void InitializeInputValidation()
        {
            // Для поля ФИО - только русский алфавит, пробелы и дефис
            textBoxFIO.MaxLength = 100; // VARCHAR(100)
            textBoxFIO.KeyPress += TextBoxFIO_KeyPress;
            textBoxFIO.Validating += TextBoxFIO_Validating;
            textBoxFIO.TextChanged += TextBoxFIO_TextChanged;
            textBoxFIO.Leave += TextBoxFIO_Leave;

            // Для поля Логин - те же ограничения, что и в LoginForm
            textBoxLogin.MaxLength = 50; // VARCHAR(50)
            textBoxLogin.KeyPress += TextBoxLogin_KeyPress;
            textBoxLogin.Validating += TextBoxLogin_Validating;

            // Для поля Пароль - те же ограничения, что и в LoginForm
            textBoxPassword.MaxLength = 50; // VARCHAR(50)
            textBoxPassword.KeyPress += TextBoxPassword_KeyPress;
            textBoxPassword.Validating += TextBoxPassword_Validating;

            // Для поля поиска по логину - те же ограничения, что и для логина
            textBoxSearch.MaxLength = 50; // VARCHAR(50) для логина
            textBoxSearch.KeyPress += TextBoxSearch_KeyPress;
            textBoxSearch.Validating += TextBoxSearch_Validating;
        }

        // ИСПРАВЛЕННЫЙ KeyPress для ФИО - разрешает первые два пробела, запрещает третий
        private void TextBoxFIO_KeyPress(object sender, KeyPressEventArgs e)
        {
            System.Windows.Forms.TextBox textBox = sender as System.Windows.Forms.TextBox;

            // Разрешаем Backspace
            if (e.KeyChar == (char)Keys.Back)
                return;

            // Если пользователь пытается ввести пробел
            if (e.KeyChar == ' ')
            {
                // Подсчитываем текущее количество пробелов
                int currentSpaceCount = textBox.Text.Count(c => c == ' ');

                // Разрешаем, если пробелов меньше 2
                if (currentSpaceCount < 2)
                {
                    return; // Разрешаем ввод пробела
                }
                else
                {
                    // Если уже 2 пробела - запрещаем третий
                    e.Handled = true;
                    System.Media.SystemSounds.Beep.Play();
                    return;
                }
            }

            // Для дефиса - разрешаем всегда
            if (e.KeyChar == '-')
                return;

            // Для русских букв - разрешаем
            if (IsRussianLetter(e.KeyChar))
                return;

            // Все остальные символы запрещены
            e.Handled = true;
        }

        // Обработчик TextChanged для форматирования (ТОЛЬКО ЗАГЛАВНЫХ БУКВ, НЕ ТРОГАЕМ ПРОБЕЛЫ)
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
                    // Форматируем заглавные буквы, но НЕ удаляем пробелы
                    string formattedText = FormatFIOInRealTime(text, cursorPosition);

                    if (formattedText != text)
                    {
                        textBox.Text = formattedText;

                        // Корректируем позицию курсора
                        if (cursorPosition <= formattedText.Length)
                        {
                            textBox.SelectionStart = cursorPosition;
                        }
                        else
                        {
                            textBox.SelectionStart = formattedText.Length;
                        }
                    }
                }
            }
            finally
            {
                isFormattingFIO = false;
            }
        }

        // Форматирование ФИО в реальном времени (только заглавные буквы)
        private string FormatFIOInRealTime(string text, int cursorPosition)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            char[] chars = text.ToCharArray();

            // Правило 1: Первая буква всегда заглавная
            if (chars.Length > 0 && IsRussianLetter(chars[0]))
            {
                chars[0] = char.ToUpper(chars[0]);
            }

            // Правило 2: После пробела или дефиса буква должна быть заглавной
            for (int i = 1; i < chars.Length; i++)
            {
                if ((chars[i - 1] == ' ' || chars[i - 1] == '-') && IsRussianLetter(chars[i]))
                {
                    chars[i] = char.ToUpper(chars[i]);
                }
                // Правило 3: Остальные буквы должны быть строчными
                else if (IsRussianLetter(chars[i]))
                {
                    if (i > 0 && !IsWordSeparator(chars[i - 1]))
                    {
                        chars[i] = char.ToLower(chars[i]);
                    }
                }
            }

            return new string(chars);
        }

        // Проверка, является ли символ разделителем слов
        private bool IsWordSeparator(char c)
        {
            return c == ' ' || c == '-';
        }

        // Валидация при потере фокуса
        private void TextBoxFIO_Validating(object sender, CancelEventArgs e)
        {
            string text = textBoxFIO.Text.Trim();

            // Проверяем длину
            if (text.Length > 100)
            {
                MessageBox.Show("ФИО не может превышать 100 символов!",
                    "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxFIO.Focus();
                textBoxFIO.SelectAll();
                e.Cancel = true;
                return;
            }

            // Проверяем, что введены только русские буквы, пробелы и дефисы
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

        // Метод для форматирования ФИО при потере фокуса
        private void TextBoxFIO_Leave(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBoxFIO.Text))
            {
                string formattedFIO = FormatFIO(textBoxFIO.Text);
                if (formattedFIO != textBoxFIO.Text)
                {
                    textBoxFIO.Text = formattedFIO;
                }
            }
        }

        // Метод для форматирования ФИО (полное форматирование)
        private string FormatFIO(string fio)
        {
            if (string.IsNullOrWhiteSpace(fio))
                return fio;

            // Разделяем строку на слова
            string[] words = fio.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Обрабатываем каждое слово
            for (int i = 0; i < words.Length; i++)
            {
                // Первая буква заглавная, остальные строчные
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpper(words[i][0]) +
                              (words[i].Length > 1 ? words[i].Substring(1).ToLower() : "");
                }
            }

            // Собираем обратно в строку с пробелами
            return string.Join(" ", words);
        }

        // Контроль ввода для Логина (те же ограничения, что и в LoginForm)
        private void TextBoxLogin_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Если нажата управляющая клавиша (Backspace, Delete, Ctrl+V, Ctrl+C и т.д.), разрешаем
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            // Проверяем символ на соответствие разрешенным
            if (!IsValidCharacter(e.KeyChar))
            {
                e.Handled = true; // Отменяем ввод символа
            }
        }

        private void TextBoxLogin_Validating(object sender, CancelEventArgs e)
        {
            string text = textBoxLogin.Text.Trim();

            // Проверяем длину
            if (text.Length > 50)
            {
                MessageBox.Show("Логин не может превышать 50 символов!",
                    "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxLogin.Focus();
                textBoxLogin.SelectAll();
                e.Cancel = true;
                return;
            }

            if (!string.IsNullOrEmpty(text))
            {
                // Проверяем на наличие пробелов
                if (text.Contains(" "))
                {
                    MessageBox.Show("Логин не должен содержать пробелов!",
                        "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxLogin.Focus();
                    textBoxLogin.SelectAll();
                    e.Cancel = true;
                    return;
                }

                // Проверяем допустимые символы (те же что и в LoginForm)
                if (ContainsInvalidCharacters(text))
                {
                    MessageBox.Show("Логин может содержать только:\n" +
                                  "• Английские буквы (A-Z, a-z)\n" +
                                  "• Цифры (0-9)\n" +
                                  "• Специальные символы: " + allowedSpecialChars,
                                  "Недопустимые символы",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxLogin.Focus();
                    textBoxLogin.SelectAll();
                    e.Cancel = true;
                }
            }
        }

        // Контроль ввода для Пароля (те же ограничения, что и в LoginForm)
        private void TextBoxPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Если нажата управляющая клавиша (Backspace, Delete, Ctrl+V, Ctrl+C и т.д.), разрешаем
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            // Проверяем символ на соответствие разрешенным
            if (!IsValidCharacter(e.KeyChar))
            {
                e.Handled = true; // Отменяем ввод символа
            }
        }

        private void TextBoxPassword_Validating(object sender, CancelEventArgs e)
        {
            string text = textBoxPassword.Text;

            // Проверяем длину
            if (text.Length > 50)
            {
                MessageBox.Show("Пароль не может превышать 50 символов!",
                    "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxPassword.Focus();
                textBoxPassword.SelectAll();
                e.Cancel = true;
                return;
            }

            if (!string.IsNullOrEmpty(text))
            {
                // Проверяем на наличие пробелов
                if (text.Contains(" "))
                {
                    MessageBox.Show("Пароль не должен содержать пробелов!",
                        "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxPassword.Focus();
                    textBoxPassword.SelectAll();
                    e.Cancel = true;
                    return;
                }

                // Проверяем допустимые символы (те же что и в LoginForm)
                if (ContainsInvalidCharacters(text))
                {
                    MessageBox.Show("Пароль может содержать только:\n" +
                                  "• Английские буквы (A-Z, a-z)\n" +
                                  "• Цифры (0-9)\n" +
                                  "• Специальные символы: " + allowedSpecialChars,
                                  "Недопустимые символы",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxPassword.Focus();
                    textBoxPassword.SelectAll();
                    e.Cancel = true;
                }
            }
        }

        // Контроль ввода для поля поиска (те же ограничения, что и для логина)
        private void TextBoxSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Если нажата управляющая клавиша (Backspace, Delete, Ctrl+V, Ctrl+C и т.д.), разрешаем
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            // Проверяем символ на соответствие разрешенным
            if (!IsValidCharacter(e.KeyChar))
            {
                e.Handled = true; // Отменяем ввод символа
            }
        }

        private void TextBoxSearch_Validating(object sender, CancelEventArgs e)
        {
            string text = textBoxSearch.Text.Trim();

            // Проверяем длину
            if (text.Length > 50)
            {
                MessageBox.Show("Поисковый запрос не может превышать 50 символов!",
                    "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxSearch.Focus();
                textBoxSearch.SelectAll();
                e.Cancel = true;
                return;
            }

            if (!string.IsNullOrEmpty(text))
            {
                // Проверяем на наличие пробелов
                if (text.Contains(" "))
                {
                    MessageBox.Show("Поисковый запрос не должен содержать пробелов!",
                        "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxSearch.Focus();
                    textBoxSearch.SelectAll();
                    e.Cancel = true;
                    return;
                }

                // Проверяем допустимые символы (те же что и в LoginForm)
                if (ContainsInvalidCharacters(text))
                {
                    MessageBox.Show("Поисковый запрос может содержать только:\n" +
                                  "• Английские буквы (A-Z, a-z)\n" +
                                  "• Цифры (0-9)\n" +
                                  "• Специальные символы: " + allowedSpecialChars,
                                  "Недопустимые символы",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxSearch.Focus();
                    textBoxSearch.SelectAll();
                    e.Cancel = true;
                }
            }
        }

        // Метод проверки символа (такой же как в LoginForm)
        private bool IsValidCharacter(char c)
        {
            // Проверяем, является ли символ русской буквой
            if ((c >= 'а' && c <= 'я') || (c >= 'А' && c <= 'Я'))
            {
                return false; // Русские буквы не разрешены
            }

            // Проверяем, является ли символ пробелом
            if (c == ' ')
            {
                return false; // Пробелы не разрешены
            }

            // Разрешаем английские буквы
            if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
            {
                return true;
            }

            // Разрешаем цифры
            if (c >= '0' && c <= '9')
            {
                return true;
            }

            // Разрешаем специальные символы из списка
            if (allowedSpecialChars.Contains(c))
            {
                return true;
            }

            // Все остальные символы запрещены
            return false;
        }

        // Вспомогательные методы для проверки символов
        private bool IsRussianLetter(char c)
        {
            return (c >= 'а' && c <= 'я') || (c >= 'А' && c <= 'Я') || c == 'ё' || c == 'Ё';
        }

        // Проверка на недопустимые символы (те же что и в LoginForm)
        private bool ContainsInvalidCharacters(string text)
        {
            foreach (char c in text)
            {
                if (!IsValidCharacter(c))
                {
                    return true;
                }
            }
            return false;
        }

        // Инициализация поиска и фильтрации
        private void InitializeSearchAndFilter()
        {
            textBoxSearch.MaxLength = 50; // VARCHAR(50) для логина
            textBoxSearch.TextChanged += textBoxSearch_TextChanged;

            // Настройка comboBoxRoleSort
            comboBoxRoleSort.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxRoleSort.SelectedIndexChanged += comboBoxRoleSort_SelectedIndexChanged;
        }

        //Отображение кнопок
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

        //отображение DataGridView (ОБНОВЛЕНО ПО АНАЛОГИИ С OrdersReportForm)
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

            // Цвет шапки как в OrdersReportForm (зеленый)
            Color headerBackColor = Color.FromArgb(97, 173, 123);

            // Цвет выделения как в OrdersReportForm (светло-зеленый)
            Color selectionColor = Color.FromArgb(233, 242, 236);

            // Настройка шапки - КАК В ORDERSREPORTFORM
            dataGridViewUsers.ColumnHeadersDefaultCellStyle.BackColor = headerBackColor;
            dataGridViewUsers.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dataGridViewUsers.ColumnHeadersDefaultCellStyle.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            dataGridViewUsers.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewUsers.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridViewUsers.ColumnHeadersDefaultCellStyle.Padding = new Padding(0, 3, 0, 3);
            dataGridViewUsers.ColumnHeadersHeight = 45;
            dataGridViewUsers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            // Настройка строк - КАК В ORDERSREPORTFORM
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

            // Настройка сетки
            dataGridViewUsers.GridColor = Color.Gray;
            dataGridViewUsers.BorderStyle = BorderStyle.Fixed3D;
            dataGridViewUsers.CellBorderStyle = DataGridViewCellBorderStyle.Single;

            // Добавляем подсказку

            // Создаем колонки
            dataGridViewUsers.Columns.Clear();

            // ID пользователя (скрытая)
            DataGridViewTextBoxColumn colId = new DataGridViewTextBoxColumn();
            colId.Name = "id_user";
            colId.DataPropertyName = "id_user";
            colId.Visible = false;
            colId.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridViewUsers.Columns.Add(colId);

            // ФИО
            DataGridViewTextBoxColumn colFIO = new DataGridViewTextBoxColumn();
            colFIO.Name = "FIO";
            colFIO.HeaderText = "ФИО";
            colFIO.DataPropertyName = "FIO";
            colFIO.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colFIO.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridViewUsers.Columns.Add(colFIO);

            // Логин
            DataGridViewTextBoxColumn colLogin = new DataGridViewTextBoxColumn();
            colLogin.Name = "login";
            colLogin.HeaderText = "Логин";
            colLogin.DataPropertyName = "login";
            colLogin.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colLogin.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridViewUsers.Columns.Add(colLogin);

            // ID роли (скрытая)
            DataGridViewTextBoxColumn colRoleId = new DataGridViewTextBoxColumn();
            colRoleId.Name = "id_role";
            colRoleId.DataPropertyName = "id_role";
            colRoleId.Visible = false;
            colRoleId.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridViewUsers.Columns.Add(colRoleId);

            // Роль
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

                    // Если выбран текущий пользователь
                    if (userId == currentAdminId)
                    {
                        // Отключаем кнопку удаления, но сохраняем зеленый цвет
                        buttonDelete.Enabled = false;
                        // Сохраняем зеленые цвета как у других кнопок
                        buttonDelete.FlatAppearance.BorderColor = Color.DarkGray; // Серый бордер
                        buttonDelete.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen; // Зеленый при наведении (хоть и неактивно)
                        buttonDelete.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;
                    }
                    else
                    {
                        // Включаем кнопку удаления
                        buttonDelete.Enabled = true;
                        // Возвращаем стандартный черный бордер
                        buttonDelete.FlatAppearance.BorderColor = Color.Black;
                        buttonDelete.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
                        buttonDelete.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;
                    }
                }
            }
            else
            {
                // Если ничего не выбрано
                buttonDelete.Enabled = false;
                buttonDelete.FlatAppearance.BorderColor = Color.DarkGray;
                buttonDelete.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            }
        }

        // Загрузка ролей для фильтрации
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

                // Добавляем элемент "Все роли" в начало
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

        // Основной метод загрузки пользователей с фильтрацией
        private void LoadUsers(string searchText = "", int roleId = 0)
        {
            try
            {
                // Базовый запрос
                string query = @"
                    SELECT u.id_user, u.FIO, u.login, 
                           u.id_role, r.role_name 
                    FROM users u 
                    LEFT JOIN roles r ON u.id_role = r.id_role 
                    WHERE 1=1";

                // Добавляем условия фильтрации
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query += " AND u.login LIKE @search";
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

        // Обработчик поиска
        private void textBoxSearch_TextChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        // Обработчик фильтрации по ролям
        private void comboBoxRoleSort_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxRoleSort.SelectedIndex >= 0)
            {
                ApplyFilters();
            }
        }

        // Применение фильтров
        private void ApplyFilters()
        {
            string searchText = textBoxSearch.Text.Trim();
            int selectedRoleId = 0;

            if (comboBoxRoleSort.SelectedValue != null && comboBoxRoleSort.SelectedIndex > 0)
            {
                selectedRoleId = Convert.ToInt32(comboBoxRoleSort.SelectedValue);
            }

            LoadUsers(searchText, selectedRoleId);
        }

        // МЕТОДЫ ДЛЯ ПОКАЗА/СКРЫТИЯ ПАНЕЛИ
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

        //переключатель из режима редактирования в режим просмотра
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

        //очистка ввода
        private void ClearEditFields()
        {
            textBoxFIO.Text = "";
            textBoxLogin.Text = "";
            textBoxPassword.Text = "";
            if (comboBoxRole != null)
                comboBoxRole.SelectedIndex = -1;
        }

        // Метод для заполнения полей при редактировании
        private void LoadEditFields()
        {
            if (!isNewUser && currentUserId > 0)
            {
                DataRow[] rows = usersTable.Select($"id_user = {currentUserId}");
                if (rows.Length > 0)
                {
                    DataRow row = rows[0];

                    // ФОРМАТИРУЕМ ФИО ПРИ ЗАГРУЗКЕ
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

        //загружает список ролей из БД для редактирования
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

        //хэширование (такой же метод как в LoginForm)
        private string ComputeSHA256Hash(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);

                System.Text.StringBuilder builder = new System.Text.StringBuilder();
                foreach (byte b in hash)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Метод для подсчета администраторов, исключая определенного пользователя (id_role = 3)
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

        // Обработчики кнопок
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
                MessageBox.Show("Вы не можете удалить свою собственную учетную запись!\n" +
                               "Для удаления вашего профиля обратитесь к другому администратору.",
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

            DialogResult result = MessageBox.Show(
                $"Вы уверены, что хотите удалить пользователя '{userName}'?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

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
            // Проверяем валидацию перед сохранением
            if (!ValidateChildren(ValidationConstraints.Enabled))
            {
                MessageBox.Show("Пожалуйста, исправьте ошибки ввода!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Дополнительная проверка длины полей
            if (textBoxFIO.Text.Trim().Length > 100)
            {
                MessageBox.Show("ФИО не может превышать 100 символов!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxFIO.Focus();
                textBoxFIO.SelectAll();
                return;
            }

            if (textBoxLogin.Text.Trim().Length > 50)
            {
                MessageBox.Show("Логин не может превышать 50 символов!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxLogin.Focus();
                textBoxLogin.SelectAll();
                return;
            }

            if (textBoxPassword.Text.Length > 50)
            {
                MessageBox.Show("Пароль не может превышать 50 символов!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxPassword.Focus();
                textBoxPassword.SelectAll();
                return;
            }

            // Валидация
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

            // ДОПОЛНИТЕЛЬНАЯ ПРОВЕРКА ПЕРЕД СОХРАНЕНИЕМ
            string fio = textBoxFIO.Text.Trim();

            // Проверяем количество пробелов
            int spaceCount = fio.Count(c => c == ' ');
            if (spaceCount > 2)
            {
                MessageBox.Show("ФИО должно содержать не более 2 пробелов!\n" +
                              "Формат: Фамилия Имя Отчество",
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxFIO.Focus();
                textBoxFIO.SelectAll();
                return;
            }

            // Проверяем, что нет двойных пробелов
            if (fio.Contains("  "))
            {
                MessageBox.Show("ФИО не должно содержать двойных пробелов!\n" +
                              "Исправьте ввод.",
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxFIO.Focus();
                textBoxFIO.SelectAll();
                return;
            }

            // Проверяем, что есть ровно три слова (для стандартного формата)
            string[] words = fio.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length != 3)
            {
                MessageBox.Show("ФИО должно содержать ровно три слова!\n" +
                              "Формат: Фамилия Имя Отчество",
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxFIO.Focus();
                textBoxFIO.SelectAll();
                return;
            }

            try
            {
                // Автоматически форматируем ФИО перед сохранением
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

        // Метод для преобразования сообщения об ошибке MySQL в понятный текст
        private string GetUserFriendlyDuplicateError(string mysqlError, string login)
        {
            if (mysqlError.Contains("users.login"))
            {
                return $"Пользователь с логином '{login}' уже существует!\nПожалуйста, выберите другой логин.";
            }
            else if (mysqlError.Contains("PRIMARY") || mysqlError.Contains("id_user"))
            {
                return "Ошибка дублирования ID пользователя. Обратитесь к администратору.";
            }
            else if (mysqlError.Contains("users.FIO"))
            {
                return "Пользователь с таким ФИО уже существует.";
            }
            else
            {
                return $"Запись уже существует в базе данных.\nЛогин '{login}' уже занят.";
            }
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

        // Обработчик сброса фильтров
        private void btnResetFilter_Click(object sender, EventArgs e)
        {
            textBoxSearch.Text = "";
            comboBoxRoleSort.SelectedIndex = 0;
        }
    }
}