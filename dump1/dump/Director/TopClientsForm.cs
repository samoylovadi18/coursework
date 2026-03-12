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
using Excel = Microsoft.Office.Interop.Excel;

namespace dump
{
    public partial class TopClientsForm : Form
    {
        private DataTable clientsData;
        private DataTable statusesData;
        private DateTime minDate = new DateTime(2024, 1, 1); // Минимальная дата - 1 января 2024 года
        private System.Windows.Forms.ToolTip toolTip1;

        public TopClientsForm()
        {
            InitializeComponent();

            toolTip1 = new System.Windows.Forms.ToolTip();

            // Настройка таблиц
            clientsData = new DataTable();
            statusesData = new DataTable();

            // Установка дат по умолчанию
            dtpStartDate.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1); // Первое число текущего месяца
            dtpEndDate.Value = DateTime.Now;

            // Установка ограничений на даты
            dtpStartDate.MinDate = minDate;
            dtpStartDate.MaxDate = DateTime.Now; // Нельзя выбрать дату больше сегодняшней
            dtpEndDate.MinDate = minDate;
            dtpEndDate.MaxDate = DateTime.Now; // Нельзя выбрать дату больше сегодняшней

            // Изначально скрываем лейблы с информацией
            HideSummaryLabels();
            
            // Подписка на события
            dtpStartDate.ValueChanged += DtpStartDate_ValueChanged;
            dtpEndDate.ValueChanged += DtpEndDate_ValueChanged;
            btnGenerate.Click += BtnGenerate_Click;
            btnExport.Click += BtnExport_Click;
            this.Load += TopClientsForm_Load;

            btnGenerate.FlatStyle = FlatStyle.Flat;
            btnGenerate.FlatAppearance.BorderSize = 1;
            btnGenerate.FlatAppearance.BorderColor = Color.Black;
            btnGenerate.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            btnGenerate.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            btnGenerate.MouseDown += (s, e) =>
            {
                btnGenerate.FlatAppearance.BorderColor = Color.DarkBlue;
            };
            btnGenerate.MouseUp += (s, e) =>
            {
                btnGenerate.FlatAppearance.BorderColor = Color.Black;
            };
            btnGenerate.MouseLeave += (s, e) =>
            {
                btnGenerate.FlatAppearance.BorderColor = Color.Black;
            };

            btnExport.FlatStyle = FlatStyle.Flat;
            btnExport.FlatAppearance.BorderSize = 1;
            btnExport.FlatAppearance.BorderColor = Color.Black;
            btnExport.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            btnExport.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            btnExport.MouseDown += (s, e) =>
            {
                btnExport.FlatAppearance.BorderColor = Color.DarkBlue;
            };
            btnExport.MouseUp += (s, e) =>
            {
                btnExport.FlatAppearance.BorderColor = Color.Black;
            };
            btnExport.MouseLeave += (s, e) =>
            {
                btnExport.FlatAppearance.BorderColor = Color.Black;
            };
        }

        private void HideSummaryLabels()
        {
            // Скрываем лейблы с информацией
            lblRecordsCount.Visible = false;
            lblTotalSum.Visible = false;
        }

        private void ShowSummaryLabels()
        {
            // Показываем лейблы с информацией
            lblRecordsCount.Visible = true;
            lblTotalSum.Visible = true;
        }

        private void DtpStartDate_ValueChanged(object sender, EventArgs e)
        {
            // Если начальная дата стала позже конечной, корректируем конечную
            if (dtpStartDate.Value > dtpEndDate.Value)
            {
                dtpEndDate.Value = dtpStartDate.Value;
            }
        }

        private void DtpEndDate_ValueChanged(object sender, EventArgs e)
        {
            // Если конечная дата стала раньше начальной, корректируем начальную
            if (dtpEndDate.Value < dtpStartDate.Value)
            {
                dtpStartDate.Value = dtpEndDate.Value;
            }
        }

        private void TopClientsForm_Load(object sender, EventArgs e)
        {
            // Настройка DataGridView
            SetupDataGridView();

            // Создаем пустую таблицу для отображения шапки
            CreateEmptyTable();

            // Загружаем статусы из БД при загрузке формы
            LoadStatuses();
        }

        private void SetupDataGridView()
        {
            dgvTopClients.ReadOnly = true;
            dgvTopClients.AllowUserToAddRows = false;
            dgvTopClients.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvTopClients.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvTopClients.MultiSelect = false;
            dgvTopClients.RowHeadersVisible = false;
            dgvTopClients.EnableHeadersVisualStyles = false;

            // Цвет шапки как в CertificateStatisticsForm (зеленый)
            Color headerBackColor = Color.FromArgb(97, 173, 123);

            // Цвет выделения как в CertificateStatisticsForm (светло-зеленый)
            Color selectionColor = Color.FromArgb(233, 242, 236);

            // Настройка шапки - КАК В CERTIFICATESTATISTICSFORM
            dgvTopClients.ColumnHeadersDefaultCellStyle.BackColor = headerBackColor;
            dgvTopClients.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvTopClients.ColumnHeadersDefaultCellStyle.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            dgvTopClients.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvTopClients.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvTopClients.ColumnHeadersDefaultCellStyle.Padding = new Padding(0, 3, 0, 3);
            dgvTopClients.ColumnHeadersHeight = 45;
            dgvTopClients.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            // Настройка строк - КАК В CERTIFICATESTATISTICSFORM
            dgvTopClients.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            dgvTopClients.DefaultCellStyle.Padding = new Padding(5);
            dgvTopClients.DefaultCellStyle.BackColor = Color.White;
            dgvTopClients.DefaultCellStyle.ForeColor = Color.Black;
            dgvTopClients.DefaultCellStyle.SelectionBackColor = selectionColor;
            dgvTopClients.DefaultCellStyle.SelectionForeColor = Color.Black;

            dgvTopClients.RowsDefaultCellStyle.BackColor = Color.White;
            dgvTopClients.RowsDefaultCellStyle.ForeColor = Color.Black;
            dgvTopClients.RowsDefaultCellStyle.SelectionBackColor = selectionColor;
            dgvTopClients.RowsDefaultCellStyle.SelectionForeColor = Color.Black;

            dgvTopClients.RowTemplate.Height = 35;
            dgvTopClients.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvTopClients.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            // Настройка сетки
            dgvTopClients.GridColor = Color.Gray;
            dgvTopClients.BorderStyle = BorderStyle.Fixed3D;
            dgvTopClients.CellBorderStyle = DataGridViewCellBorderStyle.Single;

            // Добавляем подсказку
            toolTip1.SetToolTip(dgvTopClients, "Топ клиентов по сумме заказов");

            // Создаем колонки вручную (как в CertificateStatisticsForm)
            dgvTopClients.Columns.Clear();

            // Клиент
            DataGridViewTextBoxColumn colClient = new DataGridViewTextBoxColumn();
            colClient.Name = "Клиент";
            colClient.HeaderText = "Клиент";
            colClient.DataPropertyName = "Клиент";
            colClient.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colClient.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvTopClients.Columns.Add(colClient);

            // Телефон
            DataGridViewTextBoxColumn colPhone = new DataGridViewTextBoxColumn();
            colPhone.Name = "Телефон";
            colPhone.HeaderText = "Телефон";
            colPhone.DataPropertyName = "Телефон";
            colPhone.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colPhone.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvTopClients.Columns.Add(colPhone);

            // Кол-во заказов
            DataGridViewTextBoxColumn colOrdersCount = new DataGridViewTextBoxColumn();
            colOrdersCount.Name = "Кол-во заказов";
            colOrdersCount.HeaderText = "Кол-во заказов";
            colOrdersCount.DataPropertyName = "Кол-во заказов";
            colOrdersCount.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colOrdersCount.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvTopClients.Columns.Add(colOrdersCount);

            // Сумма на все заказы
            DataGridViewTextBoxColumn colTotalSum = new DataGridViewTextBoxColumn();
            colTotalSum.Name = "Сумма на все заказы";
            colTotalSum.HeaderText = "Сумма на все заказы";
            colTotalSum.DataPropertyName = "Сумма на все заказы";
            colTotalSum.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colTotalSum.DefaultCellStyle.Format = "N2";
            colTotalSum.DefaultCellStyle.ForeColor = Color.DarkGreen;
            colTotalSum.DefaultCellStyle.SelectionForeColor = Color.DarkGreen;
            colTotalSum.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvTopClients.Columns.Add(colTotalSum);
        }

        private void CreateEmptyTable()
        {
            // Создаем пустую таблицу с нужными колонками
            DataTable emptyTable = new DataTable();
            emptyTable.Columns.Add("Клиент", typeof(string));
            emptyTable.Columns.Add("Телефон", typeof(string));
            emptyTable.Columns.Add("Кол-во заказов", typeof(int));
            emptyTable.Columns.Add("Сумма на все заказы", typeof(decimal));

            dgvTopClients.DataSource = emptyTable;
        }

        private void LoadStatuses()
        {
            try
            {
                // ИСПРАВЛЕННЫЙ ЗАПРОС - используем правильную таблицу order_statuses
                string query = "SELECT id_status, status_name FROM order_statuses ORDER BY id_status";

                using (var connection = SettingsBD.GetConnection())
                {
                    connection.Open();
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        statusesData.Clear();
                        adapter.Fill(statusesData);

                        // Добавляем пункт "Все статусы" в начало
                        DataRow allRow = statusesData.NewRow();
                        allRow["id_status"] = 0;
                        allRow["status_name"] = "Все статусы";
                        statusesData.Rows.InsertAt(allRow, 0);

                        // Настройка ComboBox
                        cmbStatus.DisplayMember = "status_name";
                        cmbStatus.ValueMember = "id_status";
                        cmbStatus.DataSource = statusesData;

                        cmbStatus.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке статусов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                // Проверка на корректность дат
                if (dtpStartDate.Value > dtpEndDate.Value)
                {
                    MessageBox.Show("Начальная дата не может быть больше конечной!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Проверка на минимальную дату
                if (dtpStartDate.Value < minDate || dtpEndDate.Value < minDate)
                {
                    MessageBox.Show($"Нельзя выбрать дату раньше {minDate:dd.MM.yyyy}!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                LoadTopClients();
                UpdateSummaryInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadTopClients()
        {
            int selectedStatus = Convert.ToInt32(cmbStatus.SelectedValue);

            string query;

            if (selectedStatus == 0) // Все статусы
            {
                query = @"
            SELECT 
                name_client AS 'Клиент',
                phone_number AS 'Телефон',
                COUNT(*) AS 'Кол-во заказов',
                SUM(total_amount) AS 'Сумма на все заказы'
            FROM orders
            WHERE DATE(delivery_date) BETWEEN @startDate AND @endDate
            GROUP BY phone_number, name_client
            ORDER BY SUM(total_amount) DESC
            LIMIT 10"; 
            }
            else // Конкретный статус
            {
                query = @"
            SELECT 
                name_client AS 'Клиент',
                phone_number AS 'Телефон',
                COUNT(*) AS 'Кол-во заказов',
                SUM(total_amount) AS 'Сумма на все заказы'
            FROM orders
            WHERE id_status = @status
            AND DATE(delivery_date) BETWEEN @startDate AND @endDate
            GROUP BY phone_number, name_client
            ORDER BY SUM(total_amount) DESC
            LIMIT 10"; 
            }


            using (var connection = SettingsBD.GetConnection())
            {
                connection.Open();
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@startDate", dtpStartDate.Value.Date);
                    cmd.Parameters.AddWithValue("@endDate", dtpEndDate.Value.Date);

                    if (selectedStatus != 0)
                    {
                        cmd.Parameters.AddWithValue("@status", selectedStatus);
                    }

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    clientsData.Clear();
                    adapter.Fill(clientsData);

                    // Если данных нет, показываем пустую таблицу с колонками
                    if (clientsData.Rows.Count == 0)
                    {
                        CreateEmptyTable();
                        HideSummaryLabels(); // Скрываем лейблы, если данных нет
                    }
                    else
                    {
                        dgvTopClients.DataSource = clientsData;
                        ShowSummaryLabels(); // Показываем лейблы, если данные есть
                    }
                }
            }
        }

        private void UpdateSummaryInfo()
        {
            // Обновляем счетчик записей
            lblRecordsCount.Text = $"Всего записей: {clientsData.Rows.Count}";

            // Вычисляем общую сумму
            decimal totalSum = 0;
            foreach (DataRow row in clientsData.Rows)
            {
                totalSum += Convert.ToDecimal(row["Сумма на все заказы"]);
            }
            lblTotalSum.Text = $"Общая сумма: {totalSum:N2} ₽";
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                if (clientsData == null || clientsData.Rows.Count == 0)
                {
                    MessageBox.Show("Сначала сформируйте отчет!", "Предупреждение",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Excel файлы (*.xlsx)|*.xlsx|Excel файлы (*.xls)|*.xls";
                saveDialog.FileName = $"Топ_клиентов_{DateTime.Now:yyyyMMdd_HHmmss}";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    ExportToExcel(saveDialog.FileName);

                    DialogResult result = MessageBox.Show($"✅ Файл успешно сохранен!\n{saveDialog.FileName}\n\nОткрыть файл?",
                        "Готово", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        // Открываем файл в программе по умолчанию (Excel)
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = saveDialog.FileName,
                            UseShellExecute = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в Excel: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                worksheet.Name = "Топ клиентов";

                // Получаем название выбранного статуса
                string statusName = "Все статусы";
                if (cmbStatus.SelectedIndex > 0)
                {
                    statusName = cmbStatus.Text;
                }

                // ЗАГОЛОВОК
                Excel.Range titleRange = worksheet.Range["A1:D1"];
                titleRange.Merge();
                titleRange.Value = "ТОП КЛИЕНТОВ ПО СУММЕ ЗАКАЗОВ";
                titleRange.Font.Bold = true;
                titleRange.Font.Size = 14;
                titleRange.Font.Name = "Times New Roman";
                titleRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                titleRange.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                titleRange.RowHeight = 30;

                // ПЕРИОД И СТАТУС
                Excel.Range periodRange = worksheet.Range["A2:D2"];
                periodRange.Merge();
                periodRange.Value = $"Период: {dtpStartDate.Value:dd.MM.yyyy} - {dtpEndDate.Value:dd.MM.yyyy} | Статус: {statusName}";
                periodRange.Font.Bold = true;
                periodRange.Font.Size = 12;
                periodRange.Font.Name = "Times New Roman";
                periodRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                periodRange.RowHeight = 25;

                // Пустая строка
                worksheet.Range["A3:D3"].RowHeight = 10;

                // Устанавливаем ширину колонок
                worksheet.Columns[1].ColumnWidth = 35; // Клиент
                worksheet.Columns[2].ColumnWidth = 20; // Телефон
                worksheet.Columns[3].ColumnWidth = 18; // Кол-во заказов
                worksheet.Columns[4].ColumnWidth = 25; // Сумма

                // Устанавливаем шрифт для всей таблицы
                worksheet.Range[$"A4:F100"].Font.Name = "Times New Roman";
                worksheet.Range[$"A4:F100"].Font.Size = 10;

                // Заголовки таблицы
                int headerRow = 4;
                string[] headers = { "Клиент", "Телефон", "Кол-во заказов", "Сумма на все заказы" };

                for (int i = 0; i < headers.Length; i++)
                {
                    Excel.Range cell = (Excel.Range)worksheet.Cells[headerRow, i + 1];
                    cell.Value = headers[i];
                    cell.Font.Bold = true;
                    cell.Font.Size = 11;
                    cell.Font.Name = "Times New Roman";
                    cell.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(97, 173, 123));
                    cell.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    cell.Borders.Weight = Excel.XlBorderWeight.xlThin;
                    cell.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    cell.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                    cell.RowHeight = 35;
                    cell.WrapText = true;
                }

                // Данные
                for (int i = 0; i < clientsData.Rows.Count; i++)
                {
                    int rowNum = headerRow + 1 + i;
                    ((Excel.Range)worksheet.Rows[rowNum]).RowHeight = 25;

                    // Клиент
                    Excel.Range clientCell = (Excel.Range)worksheet.Cells[rowNum, 1];
                    clientCell.Value = clientsData.Rows[i]["Клиент"].ToString();
                    clientCell.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    clientCell.Borders.Weight = Excel.XlBorderWeight.xlThin;
                    clientCell.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                    clientCell.WrapText = true;
                    clientCell.HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;

                    // Телефон
                    Excel.Range phoneCell = (Excel.Range)worksheet.Cells[rowNum, 2];
                    phoneCell.Value = clientsData.Rows[i]["Телефон"].ToString();
                    phoneCell.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    phoneCell.Borders.Weight = Excel.XlBorderWeight.xlThin;
                    phoneCell.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                    phoneCell.WrapText = true;
                    phoneCell.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                    // Кол-во заказов
                    Excel.Range countCell = (Excel.Range)worksheet.Cells[rowNum, 3];
                    countCell.Value = Convert.ToInt32(clientsData.Rows[i]["Кол-во заказов"]);
                    countCell.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    countCell.Borders.Weight = Excel.XlBorderWeight.xlThin;
                    countCell.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                    countCell.WrapText = true;
                    countCell.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;

                    // Сумма
                    Excel.Range sumCell = (Excel.Range)worksheet.Cells[rowNum, 4];
                    sumCell.Value = Convert.ToDecimal(clientsData.Rows[i]["Сумма на все заказы"]);
                    sumCell.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    sumCell.Borders.Weight = Excel.XlBorderWeight.xlThin;
                    sumCell.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                    sumCell.WrapText = true;
                    sumCell.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                    sumCell.NumberFormat = "#,##0.00";
                    sumCell.Font.Color = System.Drawing.ColorTranslator.ToOle(Color.DarkGreen);
                }

                // Итоговая строка
                int totalRow = headerRow + 1 + clientsData.Rows.Count;

                Excel.Range totalLabelRange = worksheet.Range[$"A{totalRow}:B{totalRow}"];
                totalLabelRange.Merge();
                totalLabelRange.Value = "ИТОГО:";
                totalLabelRange.Font.Bold = true;
                totalLabelRange.Font.Size = 11;
                totalLabelRange.Font.Name = "Times New Roman";
                totalLabelRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                totalLabelRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                totalLabelRange.Borders.Weight = Excel.XlBorderWeight.xlThin;
                totalLabelRange.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;

                // Общее количество заказов
                int totalOrders = 0;
                decimal totalSum = 0;
                foreach (DataRow row in clientsData.Rows)
                {
                    totalOrders += Convert.ToInt32(row["Кол-во заказов"]);
                    totalSum += Convert.ToDecimal(row["Сумма на все заказы"]);
                }

                Excel.Range totalOrdersRange = worksheet.Range[$"C{totalRow}"];
                totalOrdersRange.Value = totalOrders;
                totalOrdersRange.Font.Bold = true;
                totalOrdersRange.Font.Size = 11;
                totalOrdersRange.Font.Name = "Times New Roman";
                totalOrdersRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                totalOrdersRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                totalOrdersRange.Borders.Weight = Excel.XlBorderWeight.xlThin;
                totalOrdersRange.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;

                Excel.Range totalSumRange = worksheet.Range[$"D{totalRow}"];
                totalSumRange.Value = totalSum;
                totalSumRange.Font.Bold = true;
                totalSumRange.Font.Size = 11;
                totalSumRange.Font.Name = "Times New Roman";
                totalSumRange.NumberFormat = "#,##0.00";
                totalSumRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                totalSumRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                totalSumRange.Borders.Weight = Excel.XlBorderWeight.xlThin;
                totalSumRange.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                totalSumRange.Font.Color = System.Drawing.ColorTranslator.ToOle(Color.DarkGreen);

                ((Excel.Range)worksheet.Rows[totalRow]).RowHeight = 30;

                // Настройка страницы
                worksheet.PageSetup.Orientation = Excel.XlPageOrientation.xlLandscape;
                worksheet.PageSetup.FitToPagesWide = 1;
                worksheet.PageSetup.Zoom = 100;

                workbook.SaveAs(filePath);
            }
            finally
            {
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
    }
}