using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Word = Microsoft.Office.Interop.Word;

namespace dump
{
    public partial class AddSertificateForm : Form
    {
        public AddSertificateForm()
        {
            InitializeComponent();
            InitializeForm();
            LoadPricesToComboBox();
        }

        private void InitializeForm()
        {
            // Настройка DateTimePicker
            dtpIssueDate.Value = DateTime.Now;
            dtpIssueDate.Enabled = false;
            dtpIssueDate.Format = DateTimePickerFormat.Custom;
            dtpIssueDate.CustomFormat = "dd.MM.yyyy HH:mm:ss";

            // НАСТРОЙКА MASKEDTEXTBOX ДЛЯ ТЕЛЕФОНА
            mtxtPhone.Mask = "+7 (999) 000-00-00";
            mtxtPhone.TextMaskFormat = MaskFormat.IncludePromptAndLiterals;
            mtxtPhone.Font = new Font("Times New Roman", 20);
            mtxtPhone.BeepOnError = true;
            mtxtPhone.ValidatingType = typeof(int);

            // ===== ЗАПРЕТ ПРОБЕЛОВ =====
            txtLastName.KeyPress += TextBox_KeyPress_NoSpaces;
            txtFirstName.KeyPress += TextBox_KeyPress_NoSpaces;
            txtMiddleName.KeyPress += TextBox_KeyPress_NoSpaces;

            // Настройка текстовых полей для ФИО (только русские буквы)
            txtLastName.KeyPress += TextBox_KeyPress_RussianOnly;
            txtFirstName.KeyPress += TextBox_KeyPress_RussianOnly;
            txtMiddleName.KeyPress += TextBox_KeyPress_RussianOnly;

            txtLastName.TextChanged += TextBox_TextChanged_CapitalizeFirst;
            txtFirstName.TextChanged += TextBox_TextChanged_CapitalizeFirst;
            txtMiddleName.TextChanged += TextBox_TextChanged_CapitalizeFirst;

            // Настройка кнопки "Выдать"
            btnIssue.FlatStyle = FlatStyle.Flat;
            btnIssue.FlatAppearance.BorderSize = 1;
            btnIssue.FlatAppearance.BorderColor = Color.Black;
            btnIssue.BackColor = Color.DarkSeaGreen;
            btnIssue.ForeColor = Color.Black;
            btnIssue.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            btnIssue.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;
            btnIssue.Click += BtnIssue_Click;

            // Настройка кнопки "Назад"
            if (pictureBox2 != null)
            {
                pictureBox2.Click += PictureBox2_Click;
                pictureBox2.Cursor = Cursors.Hand;
            }

            btnIssue.MouseDown += (s, e) => btnIssue.FlatAppearance.BorderColor = Color.DarkBlue;
            btnIssue.MouseUp += (s, e) => btnIssue.FlatAppearance.BorderColor = Color.Black;
            btnIssue.MouseLeave += (s, e) => btnIssue.FlatAppearance.BorderColor = Color.Black;
        }

        // ===== ЗАПРЕТ ПРОБЕЛОВ =====
        private void TextBox_KeyPress_NoSpaces(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ' ')
            {
                e.Handled = true;  // Пробел не будет введён
            }
        }

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

        private void TextBox_KeyPress_RussianOnly(object sender, KeyPressEventArgs e)
        {
            char c = e.KeyChar;
            bool isRussian = (c >= 'а' && c <= 'я') || (c >= 'А' && c <= 'Я') || c == 'ё' || c == 'Ё';

            // Разрешаем: управляющие символы (Backspace, Enter), русские буквы и дефис
            // ПРОБЕЛ ЗАПРЕЩЁН
            if (!char.IsControl(e.KeyChar) && !isRussian && c != '-')
            {
                e.Handled = true;
            }
        }

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

        private void BtnIssue_Click(object sender, EventArgs e)
        {
            if (!ValidateFields())
                return;

            SaveCertificateToDatabase();
        }

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

                            MessageBox.Show($"Сертификат №{newId} успешно выдан!", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // СПРАШИВАЕМ, СОЗДАВАТЬ ЛИ СЕРТИФИКАТ В WORD
                            DialogResult createWordResult = MessageBox.Show(
                                "Создать сертификат в Word?",
                                "Создание сертификата",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);

                            if (createWordResult == DialogResult.Yes)
                            {
                                // ВЫЗЫВАЕМ МЕТОД С ВЫБОРОМ ПУТИ
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

        // ===== НОВЫЙ МЕТОД С ВЫБОРОМ ПУТИ СОХРАНЕНИЯ =====
        private void CreateWordCertificateWithDialog(long certificateId, string lastName, string firstName,
            string middleName, string phone, decimal price, DateTime issueDate)
        {
            try
            {
                // СОЗДАЕМ ДИАЛОГ ВЫБОРА ПУТИ СОХРАНЕНИЯ
                SaveFileDialog saveDialog = new SaveFileDialog();

                // Настройка фильтров
                saveDialog.Filter = "Документ Word (*.docx)|*.docx|Все файлы (*.*)|*.*";
                saveDialog.FilterIndex = 1;

                // Имя файла по умолчанию
                saveDialog.FileName = $"Сертификат_№{certificateId}_{lastName}_{DateTime.Now:yyyyMMdd_HHmmss}.docx";

                // Заголовок окна
                saveDialog.Title = "ВЫБЕРИТЕ МЕСТО ДЛЯ СОХРАНЕНИЯ СЕРТИФИКАТА";

                // Начальная папка - РАБОЧИЙ СТОЛ
                saveDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                // Настройки диалога
                saveDialog.OverwritePrompt = true;  // Спрашивать при перезаписи
                saveDialog.CheckPathExists = true;  // Проверять существование пути
                saveDialog.ValidateNames = true;    // Проверять имя файла
                saveDialog.AddExtension = true;     // Добавлять расширение автоматически
                saveDialog.DefaultExt = "docx";     // Расширение по умолчанию

                // ПОКАЗЫВАЕМ ДИАЛОГ И ЖДЕМ ВЫБОРА ПОЛЬЗОВАТЕЛЯ
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    // ПОЛУЧАЕМ ВЫБРАННЫЙ ПУТЬ
                    string selectedPath = saveDialog.FileName;

                    // СОЗДАЕМ СЕРТИФИКАТ ПО ВЫБРАННОМУ ПУТИ
                    CreateWordCertificate(selectedPath, certificateId, lastName, firstName, middleName, phone, price, issueDate);
                }
                else
                {
                    // Пользователь нажал Отмена
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

        // ===== ИЗМЕНЕННЫЙ МЕТОД ДЛЯ СОЗДАНИЯ СЕРТИФИКАТА (принимает путь) =====
        private void CreateWordCertificate(string filePath, long certificateId, string lastName, string firstName,
            string middleName, string phone, decimal price, DateTime issueDate)
        {
            Word.Application wordApp = null;
            Word.Document doc = null;

            try
            {
                // Создаем приложение Word
                wordApp = new Word.Application();
                wordApp.Visible = true; // Делаем видимым

                // Создаем новый документ
                doc = wordApp.Documents.Add();
                doc.Activate();

                // Получаем выделение для вставки текста
                Word.Selection selection = wordApp.Selection;

                // ===== НАСТРОЙКА ПАРАМЕТРОВ СТРАНИЦЫ ДЛЯ КОМПАКТНОГО РАЗМЕЩЕНИЯ =====
                doc.PageSetup.TopMargin = wordApp.CentimetersToPoints(1.5f);
                doc.PageSetup.BottomMargin = wordApp.CentimetersToPoints(1.5f);
                doc.PageSetup.LeftMargin = wordApp.CentimetersToPoints(2);
                doc.PageSetup.RightMargin = wordApp.CentimetersToPoints(2);

                // Устанавливаем размер бумаги A4
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

                // Сумма (крупно)
                selection.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                selection.Font.Size = 20;
                selection.Font.Bold = 1;
                selection.Font.Color = Word.WdColor.wdColorDarkRed;
                selection.TypeText($"{price.ToString("N0")} (");

                // Пропись суммы
                string priceText = NumberToWords(price);
                selection.TypeText(priceText);
                selection.TypeText(") рублей");
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

                // Сохраняем документ
                doc.SaveAs(filePath);

                MessageBox.Show($"✅ Сертификат сохранен!\n\nПуть: {filePath}",
                    "Сертификат создан", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании сертификата Word: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===== ПРЕОБРАЗОВАНИЕ ЧИСЛА В ПРОПИСЬ =====
        private string NumberToWords(decimal number)
        {
            int num = (int)number;
            if (num == 0)
                return "ноль";

            string words = "";

            if ((num / 1000000) > 0)
            {
                words += NumberToWords(num / 1000000) + " миллионов ";
                num %= 1000000;
            }

            if ((num / 1000) > 0)
            {
                words += NumberToWords(num / 1000) + " тысяч ";
                num %= 1000;
            }

            if ((num / 100) > 0)
            {
                words += NumberToWords(num / 100) + " сотен ";
                num %= 100;
            }

            if (num > 0)
            {
                if (words != "")
                    words += " ";

                var unitsMap = new[] { "ноль", "один", "два", "три", "четыре", "пять", "шесть", "семь", "восемь", "девять", "десять", "одиннадцать", "двенадцать", "тринадцать", "четырнадцать", "пятнадцать", "шестнадцать", "семнадцать", "восемнадцать", "девятнадцать" };
                var tensMap = new[] { "ноль", "десять", "двадцать", "тридцать", "сорок", "пятьдесят", "шестьдесят", "семьдесят", "восемьдесят", "девяносто" };

                if (num < 20)
                    words += unitsMap[num];
                else
                {
                    words += tensMap[num / 10];
                    if ((num % 10) > 0)
                        words += " " + unitsMap[num % 10];
                }
            }

            return words.Trim();
        }

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