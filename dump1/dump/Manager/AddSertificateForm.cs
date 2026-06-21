using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Word = Microsoft.Office.Interop.Word;

namespace dump
{
    /// <summary>
    /// Форма выдачи подарочных сертификатов.
    /// Предоставляет функционал для создания, сохранения и печати сертификатов в Word.
    /// </summary>
    public partial class AddSertificateForm : Form
    {
        private bool isLockDialogOpen = false;

        /// <summary>
        /// Конструктор формы выдачи сертификатов.
        /// Инициализирует компоненты и настраивает внешний вид.
        /// </summary>
        public AddSertificateForm()
        {
            InitializeComponent();
            InitializeForm();
            LoadPricesToComboBox();

            this.FormClosing += AddSertificateForm_FormClosing;
            InactivityManager.RegisterForm(this);
            InactivityManager.OnLockRequest += LockSystem;
        }

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
        private void AddSertificateForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Visible = false;
                ManagerForm manager = new ManagerForm();
                manager.Show();
            }
        }

        /// <summary>
        /// Инициализирует компоненты формы: настройка даты, маски телефона, стилей кнопок.
        /// </summary>
        private void InitializeForm()
        {
            dtpIssueDate.Value = DateTime.Now;
            dtpIssueDate.Enabled = false;
            dtpIssueDate.Format = DateTimePickerFormat.Custom;
            dtpIssueDate.CustomFormat = "dd.MM.yyyy HH:mm:ss";

            mtxtPhone.Mask = "+7 (999) 000-00-00";
            mtxtPhone.TextMaskFormat = MaskFormat.IncludePromptAndLiterals;
            mtxtPhone.Font = new Font("Times New Roman", 20);
            mtxtPhone.BeepOnError = true;
            mtxtPhone.ValidatingType = typeof(int);

            txtLastName.KeyPress += TextBox_KeyPress_NoSpaces;
            txtFirstName.KeyPress += TextBox_KeyPress_NoSpaces;
            txtMiddleName.KeyPress += TextBox_KeyPress_NoSpaces;

            txtLastName.KeyPress += TextBox_KeyPress_RussianOnly;
            txtFirstName.KeyPress += TextBox_KeyPress_RussianOnly;
            txtMiddleName.KeyPress += TextBox_KeyPress_RussianOnly;

            txtLastName.TextChanged += TextBox_TextChanged_CapitalizeFirst;
            txtFirstName.TextChanged += TextBox_TextChanged_CapitalizeFirst;
            txtMiddleName.TextChanged += TextBox_TextChanged_CapitalizeFirst;

            btnIssue.FlatStyle = FlatStyle.Flat;
            btnIssue.FlatAppearance.BorderSize = 1;
            btnIssue.FlatAppearance.BorderColor = Color.Black;
            btnIssue.BackColor = Color.DarkSeaGreen;
            btnIssue.ForeColor = Color.Black;
            btnIssue.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            btnIssue.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;
            btnIssue.Click += BtnIssue_Click;

            btnIssue.MouseDown += (s, e) => btnIssue.FlatAppearance.BorderColor = Color.DarkBlue;
            btnIssue.MouseUp += (s, e) => btnIssue.FlatAppearance.BorderColor = Color.Black;
            btnIssue.MouseLeave += (s, e) => btnIssue.FlatAppearance.BorderColor = Color.Black;
        }

        /// <summary>
        /// Обработчик нажатия клавиш - запрещает ввод пробелов.
        /// </summary>
        private void TextBox_KeyPress_NoSpaces(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ' ')
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Загружает предустановленные цены в выпадающий список.
        /// </summary>
        private void LoadPricesToComboBox()
        {
            try
            {
                cmbPrice.Items.Clear();
                cmbPrice.Items.Add("1000");
                cmbPrice.Items.Add("1500");
                cmbPrice.Items.Add("2000");
                cmbPrice.Items.Add("2500");
                cmbPrice.Items.Add("3000");
                cmbPrice.Items.Add("3500");
                cmbPrice.Items.Add("4000");
                cmbPrice.Items.Add("4500");
                cmbPrice.Items.Add("5000");

                if (cmbPrice.Items.Count > 0)
                    cmbPrice.SelectedIndex = 0;

                cmbPrice.DropDownStyle = ComboBoxStyle.DropDownList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки цен: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Обработчик нажатия клавиш - разрешает ввод только русских букв и дефиса.
        /// </summary>
        private void TextBox_KeyPress_RussianOnly(object sender, KeyPressEventArgs e)
        {
            char c = e.KeyChar;
            bool isRussian = (c >= 'а' && c <= 'я') || (c >= 'А' && c <= 'Я') || c == 'ё' || c == 'Ё';

            if (!char.IsControl(e.KeyChar) && !isRussian && c != '-')
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Обработчик изменения текста - автоматически делает первую букву заглавной.
        /// </summary>
        private void TextBox_TextChanged_CapitalizeFirst(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;

            string text = textBox.Text;
            if (string.IsNullOrEmpty(text)) return;

            int cursorPos = textBox.SelectionStart;

            string firstChar = text[0].ToString().ToUpper();
            string rest = text.Length > 1 ? text.Substring(1) : "";
            string newText = firstChar + rest;

            if (text != newText)
            {
                textBox.Text = newText;
                textBox.SelectionStart = cursorPos > 0 ? cursorPos : 1;
            }
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Выдать сертификат".
        /// Выполняет валидацию и сохранение сертификата.
        /// </summary>
        private void BtnIssue_Click(object sender, EventArgs e)
        {
            if (!ValidateFields())
                return;

            SaveCertificateToDatabase();
        }

        /// <summary>
        /// Проверяет корректность заполнения всех полей формы.
        /// </summary>
        /// <returns>True если все поля заполнены корректно, иначе False.</returns>
        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("Введите фамилию!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLastName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                MessageBox.Show("Введите имя!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFirstName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtMiddleName.Text))
            {
                MessageBox.Show("Введите отчество!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMiddleName.Focus();
                return false;
            }

            if (!mtxtPhone.MaskCompleted)
            {
                MessageBox.Show("Введите корректный номер телефона!\nФормат: +7 (999) 000-00-00",
                    "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                mtxtPhone.Focus();
                return false;
            }

            if (cmbPrice.SelectedItem == null)
            {
                MessageBox.Show("Выберите цену сертификата!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbPrice.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Сохраняет сертификат в базу данных.
        /// </summary>
        private void SaveCertificateToDatabase()
        {
            try
            {
                string lastName = txtLastName.Text.Trim();
                string firstName = txtFirstName.Text.Trim();
                string middleName = txtMiddleName.Text.Trim();
                string fullPhone = mtxtPhone.Text;
                decimal price = decimal.Parse(cmbPrice.SelectedItem.ToString());
                DateTime issueDate = dtpIssueDate.Value;
                int statusId = 1;

                string query = @"
                    INSERT INTO certificates 
                    (last_name, first_name, middle_name, phone_number, price, date, id_status_certificate) 
                    VALUES 
                    (@lastName, @firstName, @middleName, @phoneNumber, @price, @date, @statusId)";

                using (MySqlConnection connection = SettingsBD.GetConnection())
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@lastName", lastName);
                        cmd.Parameters.AddWithValue("@firstName", firstName);
                        cmd.Parameters.AddWithValue("@middleName", middleName);
                        cmd.Parameters.AddWithValue("@phoneNumber", fullPhone);
                        cmd.Parameters.AddWithValue("@price", price);
                        cmd.Parameters.AddWithValue("@date", issueDate);
                        cmd.Parameters.AddWithValue("@statusId", statusId);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            long newId = cmd.LastInsertedId;

                            MessageBox.Show($"Сертификат №{newId} успешно выдан!\n\nСтатус: АКТИВЕН\nСрок действия: 1 год",
     "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            DialogResult createWordResult = MessageBox.Show(
                                "Создать сертификат в Word?",
                                "Создание сертификата",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);

                            if (createWordResult == DialogResult.Yes)
                            {
                                CreateWordCertificateWithDialog(newId, lastName, firstName, middleName, fullPhone, price, issueDate);
                            }

                            ClearForm();

                            DialogResult result = MessageBox.Show("Выдать еще один сертификат?",
                                "Продолжить", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                            if (result == DialogResult.No)
                            {
                                this.Hide();
                                ManagerForm manager = new ManagerForm();
                                manager.Show();
                            }
                            else
                            {
                                txtLastName.Focus();
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex) when (ex.Number == 1265 || ex.Number == 1406)
            {
                MessageBox.Show($"Ошибка: поле телефон слишком короткое в базе данных.\n" +
                               $"Нужно увеличить длину поля phone_number в таблице certificates.\n\n" +
                               $"Выполните SQL: ALTER TABLE certificates MODIFY phone_number VARCHAR(18);",
                    "Ошибка базы данных", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении сертификата: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Создаёт сертификат в Word с диалогом выбора места сохранения.
        /// </summary>
        private void CreateWordCertificateWithDialog(long certificateId, string lastName, string firstName,
            string middleName, string phone, decimal price, DateTime issueDate)
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();

                saveDialog.Filter = "Документ Word (*.docx)|*.docx|Все файлы (*.*)|*.*";
                saveDialog.FilterIndex = 1;

                saveDialog.FileName = $"Сертификат_№{certificateId}_{lastName}_{DateTime.Now:yyyyMMdd_HHmmss}.docx";

                saveDialog.Title = "ВЫБЕРИТЕ МЕСТО ДЛЯ СОХРАНЕНИЯ СЕРТИФИКАТА";

                saveDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                saveDialog.OverwritePrompt = true;
                saveDialog.CheckPathExists = true;
                saveDialog.ValidateNames = true;
                saveDialog.AddExtension = true;
                saveDialog.DefaultExt = "docx";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedPath = saveDialog.FileName;
                    CreateWordCertificate(selectedPath, certificateId, lastName, firstName, middleName, phone, price, issueDate);
                }
                else
                {
                    MessageBox.Show("Создание сертификата отменено.", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании сертификата Word: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Создаёт документ Word с сертификатом по указанному пути.
        /// </summary>
        private void CreateWordCertificate(string filePath, long certificateId, string lastName, string firstName,
            string middleName, string phone, decimal price, DateTime issueDate)
        {
            Word.Application wordApp = null;
            Word.Document doc = null;

            try
            {
                wordApp = new Word.Application();
                wordApp.Visible = true;

                doc = wordApp.Documents.Add();
                doc.Activate();

                Word.Selection selection = wordApp.Selection;

                doc.PageSetup.TopMargin = wordApp.CentimetersToPoints(1.5f);
                doc.PageSetup.BottomMargin = wordApp.CentimetersToPoints(1.5f);
                doc.PageSetup.LeftMargin = wordApp.CentimetersToPoints(2);
                doc.PageSetup.RightMargin = wordApp.CentimetersToPoints(2);

                doc.PageSetup.PageHeight = wordApp.CentimetersToPoints(29.7f);
                doc.PageSetup.PageWidth = wordApp.CentimetersToPoints(21);

                // ===== ВЕРХНИЙ КОЛОНТИТУЛ =====
                selection.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                selection.Font.Name = "Times New Roman";
                selection.Font.Size = 14;
                selection.Font.Bold = 1;
                selection.TypeText("ПОДАРОЧНЫЙ СЕРТИФИКАТ");
                selection.TypeParagraph();

                selection.Font.Size = 12;
                selection.Font.Bold = 0;
                selection.TypeText("На услуги ресторана");
                selection.TypeParagraph();
                selection.TypeParagraph();

                // ===== НОМЕР СЕРТИФИКАТА =====
                selection.Font.Size = 14;
                selection.Font.Bold = 1;
                selection.TypeText($"№ {certificateId}");
                selection.TypeParagraph();
                selection.TypeParagraph();

                // ===== ОСНОВНАЯ ИНФОРМАЦИЯ =====
                selection.Font.Size = 12;
                selection.Font.Bold = 0;
                selection.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;

                selection.TypeText("Настоящий сертификат подтверждает право на получение");
                selection.TypeParagraph();
                selection.TypeText("услуг ресторана на сумму:");
                selection.TypeParagraph();
                selection.TypeParagraph();

                // ===== СУММА =====
                selection.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                selection.Font.Size = 20;
                selection.Font.Bold = 1;
                selection.Font.Color = Word.WdColor.wdColorDarkRed;
                selection.TypeText($"{price.ToString("N0")} рублей");
                selection.TypeParagraph();
                selection.TypeParagraph();

                // Информация о владельце
                selection.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                selection.Font.Size = 11;
                selection.Font.Bold = 0;
                selection.Font.Color = Word.WdColor.wdColorBlack;

                selection.TypeText("Владелец сертификата:");
                selection.TypeParagraph();
                selection.Font.Bold = 1;
                selection.TypeText($"{lastName} {firstName} {middleName}");
                selection.TypeParagraph();
                selection.TypeParagraph();

                selection.Font.Bold = 0;
                selection.TypeText("Контактный телефон:");
                selection.TypeParagraph();
                selection.Font.Bold = 1;
                selection.TypeText(phone);
                selection.TypeParagraph();
                selection.TypeParagraph();

                selection.Font.Bold = 0;
                selection.TypeText("Дата выдачи:");
                selection.TypeParagraph();
                selection.Font.Bold = 1;
                selection.TypeText(issueDate.ToString("dd MMMM yyyy года", new System.Globalization.CultureInfo("ru-RU")));
                selection.TypeParagraph();
                selection.TypeParagraph();

                // ===== УСЛОВИЯ =====
                selection.Font.Bold = 0;
                selection.Font.Size = 9;
                selection.TypeText("Условия использования:");
                selection.TypeParagraph();

                string[] conditions = new string[]
                {
                    "• Действителен 1 год",
                    "• Не подлежит обмену на деньги",
                    "• При предъявлении назвать номер",
                    "• Используется одним лицом",
                    "• Не суммируется с другими акциями"
                };

                foreach (string condition in conditions)
                {
                    selection.TypeText(condition);
                    selection.TypeParagraph();
                }

                selection.TypeParagraph();

                // ===== ПОДПИСЬ =====
                selection.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
                selection.Font.Size = 11;
                selection.Font.Bold = 1;
                selection.TypeText("Директор ресторана");
                selection.TypeParagraph();
                selection.TypeText("_______________ /_________________/");
                selection.TypeParagraph();
                selection.TypeText("М.П.");
                selection.TypeParagraph();

                // ===== НИЖНИЙ КОЛОНТИТУЛ =====
                selection.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                selection.Font.Size = 8;
                selection.Font.Bold = 0;
                selection.Font.Italic = 1;
                selection.TypeText("Ресторан | Тел.: +7 (999) 123-45-67 | www.restaurant.ru");

                doc.SaveAs(filePath);

                MessageBox.Show($"✅ Сертификат сохранен!\n\nПуть: {filePath}",
                    "Сертификат создан", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании сертификата Word: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // НЕ ЗАКРЫВАЕМ WORD
            }
        }

        /// <summary>
        /// Очищает все поля формы для ввода нового сертификата.
        /// </summary>
        private void ClearForm()
        {
            txtLastName.Clear();
            txtFirstName.Clear();
            txtMiddleName.Clear();
            mtxtPhone.Clear();
            if (cmbPrice.Items.Count > 0)
                cmbPrice.SelectedIndex = 0;
            dtpIssueDate.Value = DateTime.Now;
        }

        /// <summary>
        /// Обработчик нажатия кнопки выхода (крестик).
        /// Скрывает текущую форму и открывает форму менеджера.
        /// </summary>
        private void PictureBox2_Click(object sender, EventArgs e)
        {
            this.Hide();
            ManagerForm manager = new ManagerForm();
            manager.Show();
        }

        private void AddSertificateForm_Load(object sender, EventArgs e)
        {
            txtLastName.Focus();
        }
    }
}