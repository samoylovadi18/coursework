using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dump
{
    public partial class LoginForm : Form
    {
        private bool isPasswordVisible = false;

        // Список разрешенных специальных символов (можно настроить)
        private string allowedSpecialChars = "!@#$%^&*()_-+=[]{}|;:'\",.<>?/`~";

        private void SetupUnderlineTextBox(TextBox textBox)
        {
            Panel underline = new Panel();
            underline.Height = 1;
            underline.Width = textBox.Width;
            underline.Location = new Point(textBox.Location.X, textBox.Location.Y + textBox.Height);
            underline.BackColor = Color.Gray;
            underline.Name = textBox.Name + "_Underline";

            // Добавляем на форму
            this.Controls.Add(underline);
            underline.BringToFront();

            // Обработчики событий для изменения цвета
            textBox.Enter += (s, e) => { underline.BackColor = Color.DarkSeaGreen; };
            textBox.Leave += (s, e) => { underline.BackColor = Color.Gray; };

            // Обновляем позицию линии если изменится размер или позиция TextBox
            textBox.LocationChanged += (s, e) =>
            {
                underline.Location = new Point(textBox.Location.X, textBox.Location.Y + textBox.Height);
            };

            textBox.SizeChanged += (s, e) =>
            {
                underline.Width = textBox.Width;
                underline.Location = new Point(textBox.Location.X, textBox.Location.Y + textBox.Height);
            };
        }

        public LoginForm()
        {
            InitializeComponent();

            // Устанавливаем ограничения на количество символов согласно структуре БД
            SetMaxLengthLimits();

            // Настраиваем валидацию ввода
            SetupInputValidation();

            // Пароль скрыт по умолчанию
            isPasswordVisible = false;
            Password.UseSystemPasswordChar = true;

            try
            {
                visible_password.Image = Image.FromFile("zac.png");
            }
            catch
            {
                // Если файл не найден, создаем простую иконку
                visible_password.Image = CreateSimpleEyeIcon(false);
            }

            visible_password.Font = new Font("Segoe UI Emoji", 13f);

            Enter.FlatStyle = FlatStyle.Flat;
            Enter.FlatAppearance.BorderSize = 1;
            Enter.FlatAppearance.BorderColor = Color.Black;
            Enter.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            Enter.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            // Добавляем обработчики для возврата border при отпускании мыши
            Enter.MouseDown += (s, e) =>
            {
                Enter.FlatAppearance.BorderColor = Color.DarkBlue; // Темнее при нажатии
            };

            Enter.MouseUp += (s, e) =>
            {
                Enter.FlatAppearance.BorderColor = Color.Black; // Возвращаем черный
            };

            Enter.MouseLeave += (s, e) =>
            {
                Enter.FlatAppearance.BorderColor = Color.Black; // Возвращаем черный при уходе мыши
            };

            Enter.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            Enter.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            SetupUnderlineTextBox(Login);
            SetupUnderlineTextBox(Password);
        }

        private Image CreateSimpleEyeIcon(bool open)
        {
            Bitmap bmp = new Bitmap(32, 32);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                using (Pen pen = new Pen(Color.Black, 2))
                {
                    if (open)
                    {
                        // Открытый глаз
                        g.DrawEllipse(pen, 8, 8, 16, 12);
                        g.FillEllipse(Brushes.Black, 14, 12, 4, 4);
                    }
                    else
                    {
                        // Закрытый глаз
                        g.DrawLine(pen, 8, 16, 24, 16);
                        g.DrawLine(pen, 8, 14, 24, 18);
                        g.DrawLine(pen, 8, 18, 24, 14);
                    }
                }
            }
            return bmp;
        }

        private void SetMaxLengthLimits()
        {
            // Согласно структуре базы данных:
            // login - VARCHAR(50) → максимум 50 символов
            Login.MaxLength = 50;           // VARCHAR(50)
            Password.MaxLength = 50;        // Ограничим пароль при вводе
        }

        private void SetupInputValidation()
        {
            // Настройка валидации для логина
            Login.KeyPress += TextBox_KeyPress;
            Login.TextChanged += Login_TextChanged;

            // Настройка валидации для пароля
            Password.KeyPress += TextBox_KeyPress;
            Password.TextChanged += Password_TextChanged;

            // Разрешаем все стандартные функции (копирование, вставку и т.д.)
            Login.ShortcutsEnabled = true;
            Password.ShortcutsEnabled = true;

            // Разрешаем контекстное меню (правая кнопка мыши)
            Login.ContextMenuStrip = new ContextMenuStrip();
            Password.ContextMenuStrip = new ContextMenuStrip();
        }

        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
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

                // Простой звуковой сигнал (необязательно)
                System.Media.SystemSounds.Beep.Play();
            }
        }

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

        private void Login_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string originalText = textBox.Text;

            // Фильтруем текст, удаляя недопустимые символы
            StringBuilder filteredText = new StringBuilder();
            foreach (char c in originalText)
            {
                if (IsValidCharacter(c) || char.IsControl(c))
                {
                    filteredText.Append(c);
                }
            }

            // Если текст изменился, обновляем TextBox
            if (filteredText.ToString() != originalText)
            {
                int selectionStart = textBox.SelectionStart;
                int selectionLength = textBox.SelectionLength;

                textBox.Text = filteredText.ToString();
                textBox.SelectionStart = Math.Max(0, selectionStart - (originalText.Length - filteredText.Length));
                textBox.SelectionLength = selectionLength;
            }

            // Проверяем длину логина
            if (textBox.Text.Length > textBox.MaxLength)
            {
                textBox.Text = textBox.Text.Substring(0, textBox.MaxLength);
                textBox.SelectionStart = textBox.MaxLength;
            }
        }

        private void Password_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string originalText = textBox.Text;

            // Фильтруем текст, удаляя недопустимые символы
            StringBuilder filteredText = new StringBuilder();
            foreach (char c in originalText)
            {
                if (IsValidCharacter(c) || char.IsControl(c))
                {
                    filteredText.Append(c);
                }
            }

            // Если текст изменился, обновляем TextBox
            if (filteredText.ToString() != originalText)
            {
                int selectionStart = textBox.SelectionStart;
                int selectionLength = textBox.SelectionLength;

                textBox.Text = filteredText.ToString();
                textBox.SelectionStart = Math.Max(0, selectionStart - (originalText.Length - filteredText.Length));
                textBox.SelectionLength = selectionLength;
            }

            // Проверяем длину пароля
            if (textBox.Text.Length > textBox.MaxLength)
            {
                textBox.Text = textBox.Text.Substring(0, textBox.MaxLength);
                textBox.SelectionStart = textBox.MaxLength;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnLog_Click(object sender, EventArgs e)
        {
            string login = Login.Text.Trim();
            string password = Password.Text;

            // Дополнительная валидация перед отправкой
            if (!ValidateLoginInput(login, password))
            {
                return;
            }

            try
            {
                using (var connection = SettingsBD.GetConnection())
                {
                    connection.Open();
                    // ИЗМЕНЕН ЗАПРОС: получаем ВСЕ данные пользователя
                    string query = @"SELECT u.id_user, u.FIO, u.login, u.id_role, r.role_name, u.password_hash 
                             FROM users u 
                             LEFT JOIN roles r ON u.id_role = r.id_role 
                             WHERE u.login = @login";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@login", login);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedHash = reader.GetString("password_hash");
                                string inputHash = HashPassword(password);

                                if (inputHash.Equals(storedHash, StringComparison.OrdinalIgnoreCase))
                                {
                                    // ПОЛУЧАЕМ ВСЕ ДАННЫЕ ПОЛЬЗОВАТЕЛЯ И СОХРАНЯЕМ В CurrentUser
                                    int userId = reader.GetInt32("id_user");
                                    string fio = reader.GetString("FIO");
                                    int roleId = reader.GetInt32("id_role");
                                    string roleName = reader.GetString("role_name");

                                    // Инициализируем статический класс
                                    CurrentUser.Initialize(userId, login, fio, roleId, roleName);

                                    // Открываем соответствующую форму в зависимости от роли
                                    OpenFormByRole(roleId);
                                    this.Hide();
                                }
                                else
                                {
                                    MessageBox.Show("Неверный пароль", "Информация",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Пользователь не найден", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при подключении к базе данных\n{ex.Message}",
                "Ошибка подключения",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateLoginInput(string login, string password)
        {
            // Проверка на пустые поля
            if (string.IsNullOrEmpty(login))
            {
                MessageBox.Show("Введите логин", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Login.Focus();
                return false;
            }

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите пароль", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Password.Focus();
                return false;
            }

            // Проверка длины логина
            if (login.Length < 3)
            {
                MessageBox.Show("Логин должен содержать не менее 3 символов", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Login.Focus();
                Login.SelectAll();
                return false;
            }

            if (login.Length > 50)
            {
                MessageBox.Show("Логин не должен превышать 50 символов", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Login.Focus();
                Login.SelectAll();
                return false;
            }

            // Проверка длины пароля
            if (password.Length < 3)
            {
                MessageBox.Show("Пароль должен содержать не менее 3 символов", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Password.Focus();
                Password.SelectAll();
                return false;
            }

            if (password.Length > 50)
            {
                MessageBox.Show("Пароль не должен превышать 50 символов", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Password.Focus();
                Password.SelectAll();
                return false;
            }

            // Дополнительная проверка на недопустимые символы (на всякий случай)
            if (ContainsInvalidCharacters(login) || ContainsInvalidCharacters(password))
            {
                MessageBox.Show("Логин и пароль могут содержать только:\n" +
                              "• Английские буквы (A-Z, a-z)\n" +
                              "• Цифры (0-9)\n" +
                              "• Специальные символы: " + allowedSpecialChars + "\n" +
                              "Запрещены: русские буквы и пробелы",
                              "Недопустимые символы",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

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

        private void OpenFormByRole(int roleId)
        {
            switch (roleId)
            {
                case 1: // Менеджер
                    ManagerForm managerForm = new ManagerForm();
                    managerForm.Show();
                    break;

                case 2: // Директор
                    DirectorForm directorForm = new DirectorForm();
                    directorForm.Show();
                    break;

                case 3: // Администратор
                    AdminForm adminForm = new AdminForm();
                    adminForm.Show();
                    break;

                default:
                    MessageBox.Show("Неизвестная роль пользователя", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        private string HashPassword(string password)
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

        private void lblPassword_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            isPasswordVisible = !isPasswordVisible;

            try
            {
                if (isPasswordVisible)
                {
                    Password.UseSystemPasswordChar = false;
                    visible_password.Image = Image.FromFile("otc.png"); // Открытый глаз
                }
                else
                {
                    Password.UseSystemPasswordChar = true;
                    visible_password.Image = Image.FromFile("zac.png"); // Закрытый глаз
                }
            }
            catch
            {
                // Если файлы не найдены, создаем простые иконки
                if (isPasswordVisible)
                {
                    Password.UseSystemPasswordChar = false;
                    visible_password.Image = CreateSimpleEyeIcon(true);
                }
                else
                {
                    Password.UseSystemPasswordChar = true;
                    visible_password.Image = CreateSimpleEyeIcon(false);
                }
            }

            Password.Focus();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show( "Вы действительно хотите закрыть приложение?","Подтверждение закрытия",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

            // Если пользователь нажал "Да", закрываем приложение
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        // Обработка горячих клавиш для формы
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Enter для входа
            if (keyData == Keys.Enter)
            {
                btnLog_Click(null, null);
                return true;
            }

            // Escape для выхода
            if (keyData == Keys.Escape)
            {
                // Полностью закрываем приложение
                Application.Exit();
                return true;
            }

            // Ctrl+A для выделения всего текста в активном поле
            if (keyData == (Keys.Control | Keys.A))
            {
                if (Login.Focused)
                {
                    Login.SelectAll();
                    return true;
                }
                if (Password.Focused)
                {
                    Password.SelectAll();
                    return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}