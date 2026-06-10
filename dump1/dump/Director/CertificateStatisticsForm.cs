using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;

namespace dump
{
    public partial class CertificateStatisticsForm : Form
    {
        private DataTable certificatesStats;
        private DateTime lastClickTime = DateTime.MinValue;
        private DateTime minDate = new DateTime(2024, 1, 1);
        private System.Windows.Forms.ToolTip toolTip1;
        private bool isLockDialogOpen = false;

        public CertificateStatisticsForm()
        {
            InitializeComponent();
            InitializeCustomComponents();
            toolTip1 = new System.Windows.Forms.ToolTip();

            certificatesStats = new DataTable();

            datePickerStart.MinDate = minDate;
            datePickerStart.MaxDate = DateTime.Now;
            datePickerEnd.MinDate = minDate;
            datePickerEnd.MaxDate = DateTime.Now;

            datePickerEnd.Value = DateTime.Now;
            datePickerStart.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            btnGenerate.Click += btnGenerate_Click;
            btnExport.Click += BtnExport_Click;

            SetupDataGridView();
            CreateEmptyTable();
            SetupButtons();
            InactivityManager.RegisterForm(this);
            InactivityManager.OnLockRequest += LockSystem;
        }

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

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            InactivityManager.UnregisterForm();
            base.OnFormClosed(e);
        }

        private void InitializeCustomComponents()
        {
            if (datePickerStart == null)
            {
                datePickerStart = new DateTimePicker();
                datePickerStart.Location = new Point(150, 20);
                datePickerStart.Size = new Size(150, 22);
                datePickerStart.Format = DateTimePickerFormat.Short;
                this.Controls.Add(datePickerStart);
            }

            if (datePickerEnd == null)
            {
                datePickerEnd = new DateTimePicker();
                datePickerEnd.Location = new Point(350, 20);
                datePickerEnd.Size = new Size(150, 22);
                datePickerEnd.Format = DateTimePickerFormat.Short;
                this.Controls.Add(datePickerEnd);
            }

            if (labelStart == null)
            {
                labelStart = new Label();
                labelStart.Text = "Начало периода:";
                labelStart.Location = new Point(40, 22);
                labelStart.Size = new Size(100, 20);
                this.Controls.Add(labelStart);
            }

            if (labelEnd == null)
            {
                labelEnd = new Label();
                labelEnd.Text = "Конец периода:";
                labelEnd.Location = new Point(250, 22);
                labelEnd.Size = new Size(100, 20);
                this.Controls.Add(labelEnd);
            }
        }

        private void SetupButtons()
        {
            btnGenerate.FlatStyle = FlatStyle.Flat;
            btnGenerate.FlatAppearance.BorderSize = 1;
            btnGenerate.FlatAppearance.BorderColor = Color.Black;
            btnGenerate.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            btnGenerate.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            btnGenerate.MouseDown += (s, e) => btnGenerate.FlatAppearance.BorderColor = Color.DarkBlue;
            btnGenerate.MouseUp += (s, e) => btnGenerate.FlatAppearance.BorderColor = Color.Black;
            btnGenerate.MouseLeave += (s, e) => btnGenerate.FlatAppearance.BorderColor = Color.Black;

            btnExport.FlatStyle = FlatStyle.Flat;
            btnExport.FlatAppearance.BorderSize = 1;
            btnExport.FlatAppearance.BorderColor = Color.Black;
            btnExport.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            btnExport.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            btnExport.MouseDown += (s, e) => btnExport.FlatAppearance.BorderColor = Color.DarkBlue;
            btnExport.MouseUp += (s, e) => btnExport.FlatAppearance.BorderColor = Color.Black;
            btnExport.MouseLeave += (s, e) => btnExport.FlatAppearance.BorderColor = Color.Black;
        }

        private void CreateEmptyTable()
        {
            DataTable emptyTable = new DataTable();
            emptyTable.Columns.Add("Статус", typeof(string));
            emptyTable.Columns.Add("Количество", typeof(int));
            emptyTable.Columns.Add("Общая сумма", typeof(decimal));
            emptyTable.Columns.Add("Средняя сумма", typeof(decimal));
            emptyTable.Columns.Add("Мин. сумма", typeof(decimal));
            emptyTable.Columns.Add("Макс. сумма", typeof(decimal));

            dgvCertificates.DataSource = emptyTable;
        }

        private void SetupDataGridView()
        {
            dgvCertificates.ReadOnly = true;
            dgvCertificates.AllowUserToAddRows = false;
            dgvCertificates.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCertificates.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCertificates.MultiSelect = false;
            dgvCertificates.RowHeadersVisible = false;
            dgvCertificates.EnableHeadersVisualStyles = false;

            Color headerBackColor = Color.FromArgb(97, 173, 123);
            Color selectionColor = Color.FromArgb(233, 242, 236);

            dgvCertificates.ColumnHeadersDefaultCellStyle.BackColor = headerBackColor;
            dgvCertificates.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvCertificates.ColumnHeadersDefaultCellStyle.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            dgvCertificates.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCertificates.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvCertificates.ColumnHeadersDefaultCellStyle.Padding = new Padding(0, 3, 0, 3);
            dgvCertificates.ColumnHeadersHeight = 45;

            dgvCertificates.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            dgvCertificates.DefaultCellStyle.Padding = new Padding(5);
            dgvCertificates.DefaultCellStyle.BackColor = Color.White;
            dgvCertificates.DefaultCellStyle.ForeColor = Color.Black;
            dgvCertificates.DefaultCellStyle.SelectionBackColor = selectionColor;
            dgvCertificates.DefaultCellStyle.SelectionForeColor = Color.Black;

            dgvCertificates.RowsDefaultCellStyle.BackColor = Color.White;
            dgvCertificates.RowsDefaultCellStyle.ForeColor = Color.Black;
            dgvCertificates.RowsDefaultCellStyle.SelectionBackColor = selectionColor;
            dgvCertificates.RowsDefaultCellStyle.SelectionForeColor = Color.Black;

            dgvCertificates.RowTemplate.Height = 35;
            dgvCertificates.GridColor = Color.Gray;
            dgvCertificates.BorderStyle = BorderStyle.Fixed3D;

            toolTip1.SetToolTip(dgvCertificates, "Статистика по сертификатам");

            dgvCertificates.Columns.Clear();

            dgvCertificates.Columns.Add(new DataGridViewTextBoxColumn { Name = "Статус", HeaderText = "Статус", DataPropertyName = "Статус", DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft } });
            dgvCertificates.Columns.Add(new DataGridViewTextBoxColumn { Name = "Количество", HeaderText = "Количество", DataPropertyName = "Количество", DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvCertificates.Columns.Add(new DataGridViewTextBoxColumn { Name = "Общая сумма", HeaderText = "Общая сумма", DataPropertyName = "Общая сумма", DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Format = "N2", ForeColor = Color.DarkGreen } });
            dgvCertificates.Columns.Add(new DataGridViewTextBoxColumn { Name = "Средняя сумма", HeaderText = "Средняя сумма", DataPropertyName = "Средняя сумма", DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Format = "N2", ForeColor = Color.DarkGreen } });
            dgvCertificates.Columns.Add(new DataGridViewTextBoxColumn { Name = "Мин. сумма", HeaderText = "Мин. сумма", DataPropertyName = "Мин. сумма", DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Format = "N2", ForeColor = Color.DarkGreen } });
            dgvCertificates.Columns.Add(new DataGridViewTextBoxColumn { Name = "Макс. сумма", HeaderText = "Макс. сумма", DataPropertyName = "Макс. сумма", DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Format = "N2", ForeColor = Color.DarkGreen } });
        }

        private void LoadCertificateStatistics(DateTime startDate, DateTime endDate)
        {
            string query = @"
                SELECT 
                    sc.name AS 'Статус',
                    COUNT(*) AS 'Количество',
                    SUM(price) AS 'Общая сумма',
                    AVG(price) AS 'Средняя сумма',
                    MIN(price) AS 'Мин. сумма',
                    MAX(price) AS 'Макс. сумма'
                FROM certificates c
                JOIN status_certificates sc ON c.id_status_certificate = sc.id_status_certificate
                WHERE DATE(c.date) BETWEEN @startDate AND @endDate
                GROUP BY c.id_status_certificate, sc.name
                ORDER BY sc.id_status_certificate";

            using (var connection = SettingsBD.GetConnection())
            {
                connection.Open();
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@startDate", startDate.Date);
                    cmd.Parameters.AddWithValue("@endDate", endDate.Date);

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    certificatesStats.Clear();
                    adapter.Fill(certificatesStats);
                }
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            if ((DateTime.Now - lastClickTime).TotalSeconds < 1)
                return;

            lastClickTime = DateTime.Now;

            try
            {
                DateTime startDate = datePickerStart.Value.Date;
                DateTime endDate = datePickerEnd.Value.Date;

                if (startDate > endDate)
                {
                    MessageBox.Show("Дата начала не может быть позже даты окончания!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                LoadCertificateStatistics(startDate, endDate);

                if (certificatesStats.Rows.Count == 0)
                {
                    CreateEmptyTable();
                    MessageBox.Show($"За выбранный период ({startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}) записей не найдено.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    dgvCertificates.DataSource = certificatesStats;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                if (certificatesStats == null || certificatesStats.Rows.Count == 0)
                {
                    MessageBox.Show("Нет данных для экспорта!", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Excel файлы (*.xlsx)|*.xlsx";
                saveDialog.FileName = $"Статистика_сертификатов_{DateTime.Now:yyyyMMdd_HHmmss}";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    ExportToExcel(saveDialog.FileName);

                    DialogResult result = MessageBox.Show($"✅ Файл успешно сохранен!\n{saveDialog.FileName}\n\nОткрыть файл?",
                        "Готово", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo { FileName = saveDialog.FileName, UseShellExecute = true });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в Excel: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToExcel(string filePath)
        {
            Excel.Application excelApp = null;
            Excel.Workbook workbook = null;
            Excel.Worksheet worksheet = null;

            try
            {
                excelApp = new Excel.Application();
                excelApp.DisplayAlerts = false;
                workbook = excelApp.Workbooks.Add();
                worksheet = (Excel.Worksheet)workbook.Worksheets[1];
                worksheet.Name = "Статистика сертификатов";

                // ЗАГОЛОВОК
                Excel.Range titleRange = worksheet.Range["A1:G1"];
                titleRange.Merge();
                titleRange.Value = "СТАТИСТИКА ПО СЕРТИФИКАТАМ";
                titleRange.Font.Bold = true;
                titleRange.Font.Size = 16;
                titleRange.Font.Name = "Times New Roman";
                titleRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                titleRange.RowHeight = 35;

                // ПЕРИОД
                Excel.Range periodRange = worksheet.Range["A2:G2"];
                periodRange.Merge();
                periodRange.Value = $"Период: {datePickerStart.Value:dd.MM.yyyy} - {datePickerEnd.Value:dd.MM.yyyy}";
                periodRange.Font.Bold = true;
                periodRange.Font.Size = 12;
                periodRange.Font.Name = "Times New Roman";
                periodRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                periodRange.RowHeight = 25;

                worksheet.Range["A3:G3"].RowHeight = 10;

                // СВОДНАЯ ИНФОРМАЦИЯ
                int summaryStartRow = 4;
                Excel.Range summaryTitleRange = worksheet.Range[$"A{summaryStartRow}:G{summaryStartRow}"];
                summaryTitleRange.Merge();
                summaryTitleRange.Value = "СВОДНАЯ ИНФОРМАЦИЯ:";
                summaryTitleRange.Font.Bold = true;
                summaryTitleRange.Font.Size = 12;
                summaryTitleRange.Font.Name = "Times New Roman";
                summaryTitleRange.Font.Underline = true;
                summaryTitleRange.RowHeight = 25;

                // Рассчитываем итоги
                int totalIssued = 0;
                int totalUsed = 0;
                int totalReturned = 0;
                int totalActive = 0;
                decimal totalIssuedSum = 0;
                decimal totalUsedSum = 0;
                decimal totalReturnedSum = 0;

                foreach (DataRow row in certificatesStats.Rows)
                {
                    string status = row["Статус"].ToString();
                    int count = Convert.ToInt32(row["Количество"]);
                    decimal sum = Convert.ToDecimal(row["Общая сумма"]);

                    if (status == "Активен")
                    {
                        totalActive = count;
                        totalIssued += count;
                        totalIssuedSum += sum;
                    }
                    else if (status == "Использован")
                    {
                        totalUsed = count;
                        totalIssued += count;
                        totalIssuedSum += sum;
                        totalUsedSum = sum;
                    }
                    else if (status == "Возвращён")
                    {
                        totalReturned = count;
                        totalReturnedSum = sum;
                    }
                }

                // Форматирование для денежных значений
                string currencyFormat = "#,##0.00";

                // Строка 1
                int summaryRow = summaryStartRow + 1;
                worksheet.Cells[summaryRow, 1] = "Всего выпущено сертификатов:";
                worksheet.Cells[summaryRow, 2] = totalIssued;
                worksheet.Cells[summaryRow, 3] = "шт.";
                worksheet.Cells[summaryRow, 4] = "Общая сумма выпущенных:";
                worksheet.Cells[summaryRow, 5] = totalIssuedSum;
                ((Excel.Range)worksheet.Cells[summaryRow, 5]).NumberFormat = currencyFormat;
                worksheet.Cells[summaryRow, 5].Font.Color = System.Drawing.ColorTranslator.ToOle(Color.DarkGreen);

                // Строка 2
                int summaryRow2 = summaryStartRow + 2;
                worksheet.Cells[summaryRow2, 1] = "Из них использовано:";
                worksheet.Cells[summaryRow2, 2] = totalUsed;
                worksheet.Cells[summaryRow2, 3] = "шт.";
                worksheet.Cells[summaryRow2, 4] = "Общая сумма использованных:";
                worksheet.Cells[summaryRow2, 5] = totalUsedSum;
                ((Excel.Range)worksheet.Cells[summaryRow2, 5]).NumberFormat = currencyFormat;
                worksheet.Cells[summaryRow2, 5].Font.Color = System.Drawing.ColorTranslator.ToOle(Color.DarkGreen);

                // Строка 3
                int summaryRow3 = summaryStartRow + 3;
                worksheet.Cells[summaryRow3, 1] = "Активных (неиспользованных):";
                worksheet.Cells[summaryRow3, 2] = totalActive;
                worksheet.Cells[summaryRow3, 3] = "шт.";
                worksheet.Cells[summaryRow3, 4] = "Общая сумма активных:";
                worksheet.Cells[summaryRow3, 5] = totalIssuedSum - totalUsedSum;
                ((Excel.Range)worksheet.Cells[summaryRow3, 5]).NumberFormat = currencyFormat;
                worksheet.Cells[summaryRow3, 5].Font.Color = System.Drawing.ColorTranslator.ToOle(Color.DarkGreen);

                // Строка 4
                int summaryRow4 = summaryStartRow + 4;
                worksheet.Cells[summaryRow4, 1] = "Возвращено сертификатов:";
                worksheet.Cells[summaryRow4, 2] = totalReturned;
                worksheet.Cells[summaryRow4, 3] = "шт.";
                worksheet.Cells[summaryRow4, 4] = "Общая сумма возвращённых:";
                worksheet.Cells[summaryRow4, 5] = totalReturnedSum;
                ((Excel.Range)worksheet.Cells[summaryRow4, 5]).NumberFormat = currencyFormat;
                worksheet.Cells[summaryRow4, 5].Font.Color = System.Drawing.ColorTranslator.ToOle(Color.Red);

                // Оформление сводной информации
                for (int i = summaryStartRow + 1; i <= summaryStartRow + 4; i++)
                {
                    worksheet.Rows[i].RowHeight = 22;
                    worksheet.Rows[i].Font.Name = "Times New Roman";
                    worksheet.Rows[i].Font.Size = 11;
                }

                // Пустая строка перед таблицей
                int tableStartRow = summaryStartRow + 6;

                // СТАТИСТИКА ПО СТАТУСАМ
                Excel.Range statsTitleRange = worksheet.Range[$"A{tableStartRow}:G{tableStartRow}"];
                statsTitleRange.Merge();
                statsTitleRange.Value = "СТАТИСТИКА ПО СТАТУСАМ:";
                statsTitleRange.Font.Bold = true;
                statsTitleRange.Font.Size = 12;
                statsTitleRange.Font.Name = "Times New Roman";
                statsTitleRange.Font.Underline = true;
                statsTitleRange.RowHeight = 25;

                // Ширина колонок
                worksheet.Columns[1].ColumnWidth = 25;
                worksheet.Columns[2].ColumnWidth = 12;
                worksheet.Columns[3].ColumnWidth = 15;
                worksheet.Columns[4].ColumnWidth = 22;
                worksheet.Columns[5].ColumnWidth = 22;
                worksheet.Columns[6].ColumnWidth = 20;
                worksheet.Columns[7].ColumnWidth = 20;

                int dataStartRow = tableStartRow + 1;
                string[] headers = { "Статус", "Количество", "Общая сумма", "Средняя сумма", "Мин. сумма", "Макс. сумма" };

                for (int i = 0; i < headers.Length; i++)
                {
                    Excel.Range cell = worksheet.Cells[dataStartRow, i + 1];
                    cell.Value = headers[i];
                    cell.Font.Bold = true;
                    cell.Font.Size = 11;
                    cell.Font.Name = "Times New Roman";
                    cell.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(97, 173, 123));
                    cell.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    cell.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    cell.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                    cell.RowHeight = 35;
                    cell.WrapText = true;
                }

                for (int i = 0; i < certificatesStats.Rows.Count; i++)
                {
                    int rowNum = dataStartRow + 1 + i;
                    worksheet.Rows[rowNum].RowHeight = 25;

                    for (int j = 0; j < certificatesStats.Columns.Count; j++)
                    {
                        Excel.Range cell = worksheet.Cells[rowNum, j + 1];

                        if (certificatesStats.Rows[i][j] != DBNull.Value)
                        {
                            cell.Value = certificatesStats.Rows[i][j];
                        }
                        else
                        {
                            cell.Value = "";
                        }

                        cell.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                        cell.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                        cell.WrapText = true;

                        if (j == 0)
                            cell.HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
                        else if (j == 1)
                            cell.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                        else
                        {
                            cell.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                            if (j >= 2)
                            {
                                ((Excel.Range)cell).NumberFormat = currencyFormat;
                            }
                        }
                    }
                }

                int totalRow = dataStartRow + certificatesStats.Rows.Count + 1;
                worksheet.Cells[totalRow, 1] = "ИТОГО:";
                worksheet.Cells[totalRow, 1].Font.Bold = true;
                worksheet.Cells[totalRow, 1].HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
                worksheet.Cells[totalRow, 1].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;

                worksheet.Cells[totalRow, 2] = totalIssued;
                worksheet.Cells[totalRow, 2].Font.Bold = true;
                worksheet.Cells[totalRow, 2].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                worksheet.Cells[totalRow, 2].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;

                worksheet.Cells[totalRow, 3] = totalIssuedSum;
                worksheet.Cells[totalRow, 3].Font.Bold = true;
                ((Excel.Range)worksheet.Cells[totalRow, 3]).NumberFormat = currencyFormat;
                worksheet.Cells[totalRow, 3].HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                worksheet.Cells[totalRow, 3].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;

                for (int j = 4; j <= 6; j++)
                {
                    worksheet.Cells[totalRow, j].Value = "";
                    worksheet.Cells[totalRow, j].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                }

                worksheet.Rows[totalRow].RowHeight = 30;
                worksheet.Rows[totalRow].Font.Bold = true;

                worksheet.PageSetup.Orientation = Excel.XlPageOrientation.xlLandscape;
                worksheet.PageSetup.FitToPagesWide = 1;
                worksheet.PageSetup.Zoom = 90;

                workbook.SaveAs(filePath);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при создании Excel: {ex.Message}");
            }
            finally
            {
                if (worksheet != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);
                if (workbook != null)
                {
                    workbook.Close(false);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                }
                if (excelApp != null)
                {
                    excelApp.Quit();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            DirectorForm director = new DirectorForm();
            director.Show();
        }

        private void CertificateStatisticsForm_Load(object sender, EventArgs e)
        {
        }
    }
}