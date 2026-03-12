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

        public CertificateStatisticsForm()
        {
            InitializeComponent();

            // Инициализация компонентов вручную, если они не созданы в дизайнере
            InitializeCustomComponents();

            toolTip1 = new System.Windows.Forms.ToolTip();

            certificatesStats = new DataTable();

            // Установка ограничений на даты
            datePickerStart.MinDate = minDate;
            datePickerStart.MaxDate = DateTime.Now;
            datePickerEnd.MinDate = minDate;
            datePickerEnd.MaxDate = DateTime.Now;

            // Установка значений по умолчанию (текущий месяц)
            datePickerEnd.Value = DateTime.Now;
            datePickerStart.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            // Подписка на события
            btnGenerate.Click += btnGenerate_Click;
            btnExport.Click += BtnExport_Click;

            // Настройка DataGridView
            SetupDataGridView();

            // Создаем пустую таблицу с колонками для отображения шапки
            CreateEmptyTable();

            // Настройка кнопок
            SetupButtons();
        }

        private void InitializeCustomComponents()
        {
            // Создаем компоненты, если они еще не созданы в дизайнере
            if (datePickerStart == null)
            {
                datePickerStart = new DateTimePicker();
                datePickerStart.Location = new Point(150, 20); // Настройте позицию под ваш макет
                datePickerStart.Size = new Size(150, 22);
                datePickerStart.Format = DateTimePickerFormat.Short;
                this.Controls.Add(datePickerStart);
            }

            if (datePickerEnd == null)
            {
                datePickerEnd = new DateTimePicker();
                datePickerEnd.Location = new Point(350, 20); // Настройте позицию под ваш макет
                datePickerEnd.Size = new Size(150, 22);
                datePickerEnd.Format = DateTimePickerFormat.Short;
                this.Controls.Add(datePickerEnd);
            }

            if (labelStart == null)
            {
                labelStart = new Label();
                labelStart.Text = "Начало периода:";
                labelStart.Location = new Point(40, 22); // Настройте позицию под ваш макет
                labelStart.Size = new Size(100, 20);
                this.Controls.Add(labelStart);
            }

            if (labelEnd == null)
            {
                labelEnd = new Label();
                labelEnd.Text = "Конец периода:";
                labelEnd.Location = new Point(250, 22); // Настройте позицию под ваш макет
                labelEnd.Size = new Size(100, 20);
                this.Controls.Add(labelEnd);
            }
        }

        private void SetupButtons()
        {
            // Настройка кнопки Generate
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

            // Настройка кнопки Export
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
            dgvCertificates.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

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
            dgvCertificates.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvCertificates.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            dgvCertificates.GridColor = Color.Gray;
            dgvCertificates.BorderStyle = BorderStyle.Fixed3D;
            dgvCertificates.CellBorderStyle = DataGridViewCellBorderStyle.Single;

            toolTip1.SetToolTip(dgvCertificates, "Статистика по сертификатам");

            dgvCertificates.Columns.Clear();

            // Статус
            DataGridViewTextBoxColumn colStatus = new DataGridViewTextBoxColumn();
            colStatus.Name = "Статус";
            colStatus.HeaderText = "Статус";
            colStatus.DataPropertyName = "Статус";
            colStatus.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colStatus.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvCertificates.Columns.Add(colStatus);

            // Количество
            DataGridViewTextBoxColumn colCount = new DataGridViewTextBoxColumn();
            colCount.Name = "Количество";
            colCount.HeaderText = "Количество";
            colCount.DataPropertyName = "Количество";
            colCount.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colCount.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvCertificates.Columns.Add(colCount);

            // Общая сумма
            DataGridViewTextBoxColumn colTotalSum = new DataGridViewTextBoxColumn();
            colTotalSum.Name = "Общая сумма";
            colTotalSum.HeaderText = "Общая сумма";
            colTotalSum.DataPropertyName = "Общая сумма";
            colTotalSum.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colTotalSum.DefaultCellStyle.Format = "N2";
            colTotalSum.DefaultCellStyle.ForeColor = Color.DarkGreen;
            colTotalSum.DefaultCellStyle.SelectionForeColor = Color.DarkGreen;
            colTotalSum.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvCertificates.Columns.Add(colTotalSum);

            // Средняя сумма
            DataGridViewTextBoxColumn colAvgSum = new DataGridViewTextBoxColumn();
            colAvgSum.Name = "Средняя сумма";
            colAvgSum.HeaderText = "Средняя сумма";
            colAvgSum.DataPropertyName = "Средняя сумма";
            colAvgSum.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colAvgSum.DefaultCellStyle.Format = "N2";
            colAvgSum.DefaultCellStyle.ForeColor = Color.DarkGreen;
            colAvgSum.DefaultCellStyle.SelectionForeColor = Color.DarkGreen;
            colAvgSum.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvCertificates.Columns.Add(colAvgSum);

            // Мин. сумма
            DataGridViewTextBoxColumn colMinSum = new DataGridViewTextBoxColumn();
            colMinSum.Name = "Мин. сумма";
            colMinSum.HeaderText = "Мин. сумма";
            colMinSum.DataPropertyName = "Мин. сумма";
            colMinSum.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colMinSum.DefaultCellStyle.Format = "N2";
            colMinSum.DefaultCellStyle.ForeColor = Color.DarkGreen;
            colMinSum.DefaultCellStyle.SelectionForeColor = Color.DarkGreen;
            colMinSum.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvCertificates.Columns.Add(colMinSum);

            // Макс. сумма
            DataGridViewTextBoxColumn colMaxSum = new DataGridViewTextBoxColumn();
            colMaxSum.Name = "Макс. сумма";
            colMaxSum.HeaderText = "Макс. сумма";
            colMaxSum.DataPropertyName = "Макс. сумма";
            colMaxSum.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colMaxSum.DefaultCellStyle.Format = "N2";
            colMaxSum.DefaultCellStyle.ForeColor = Color.DarkGreen;
            colMaxSum.DefaultCellStyle.SelectionForeColor = Color.DarkGreen;
            colMaxSum.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvCertificates.Columns.Add(colMaxSum);
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

                    if (certificatesStats.Rows.Count == 0)
                    {
                        CreateEmptyTable();
                    }
                    else
                    {
                        dgvCertificates.DataSource = certificatesStats;
                    }
                }
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            // Проверяем, прошло ли меньше 1 секунды с последнего нажатия
            if ((DateTime.Now - lastClickTime).TotalSeconds < 1)
            {
                return; // Игнорируем повторное нажатие
            }

            lastClickTime = DateTime.Now; // Запоминаем время нажатия

            try
            {
                DateTime startDate = datePickerStart.Value.Date;
                DateTime endDate = datePickerEnd.Value.Date;

                // Проверка корректности дат
                if (startDate > endDate)
                {
                    MessageBox.Show("Дата начала не может быть позже даты окончания!",
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Загружаем статистику по сертификатам
                LoadCertificateStatistics(startDate, endDate);

                // Проверка на наличие данных
                if (certificatesStats.Rows.Count == 0)
                {
                    MessageBox.Show($"За выбранный период ({startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}) записей не найдено.",
                        "Информация",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                if (certificatesStats == null || certificatesStats.Rows.Count == 0)
                {
                    MessageBox.Show("Невозможно сформировать отчёт, так как нет данных за выбранный период.",
                        "Предупреждение",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Excel файлы (*.xlsx)|*.xlsx|Excel файлы (*.xls)|*.xls";
                saveDialog.FileName = $"Статистика_сертификатов_{DateTime.Now:yyyyMMdd_HHmmss}";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    ExportToExcel(saveDialog.FileName);

                    DialogResult result = MessageBox.Show($"✅ Файл успешно сохранен!\n{saveDialog.FileName}\n\nОткрыть файл?",
                        "Готово", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
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
                MessageBox.Show($"Ошибка при экспорте в Excel: {ex.Message}\n\nУбедитесь, что установлен Microsoft Excel.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                Excel.Range titleRange = worksheet.Range["A1:F1"];
                titleRange.Merge();
                titleRange.Value = "СТАТИСТИКА ПО СЕРТИФИКАТАМ";
                titleRange.Font.Bold = true;
                titleRange.Font.Size = 14;
                titleRange.Font.Name = "Times New Roman";
                titleRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                titleRange.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                titleRange.RowHeight = 30;

                // ПЕРИОД
                Excel.Range periodRange = worksheet.Range["A2:F2"];
                periodRange.Merge();
                periodRange.Value = $"Период: {datePickerStart.Value:dd.MM.yyyy} - {datePickerEnd.Value:dd.MM.yyyy}";
                periodRange.Font.Bold = true;
                periodRange.Font.Size = 12;
                periodRange.Font.Name = "Times New Roman";
                periodRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                periodRange.RowHeight = 25;

                // Пустая строка
                worksheet.Range["A3:F3"].RowHeight = 10;

                // СТАТИСТИКА ПО СТАТУСАМ - заголовок
                int tableStartRow = 4;
                Excel.Range statsTitleRange = worksheet.Range[$"A{tableStartRow}:F{tableStartRow}"];
                statsTitleRange.Merge();
                statsTitleRange.Value = "СТАТИСТИКА ПО СТАТУСАМ:";
                statsTitleRange.Font.Bold = true;
                statsTitleRange.Font.Size = 12;
                statsTitleRange.Font.Name = "Times New Roman";
                statsTitleRange.Font.Underline = true;
                statsTitleRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
                statsTitleRange.RowHeight = 25;

                // Устанавливаем ширину колонок
                worksheet.Columns[1].ColumnWidth = 28;
                worksheet.Columns[2].ColumnWidth = 15;
                worksheet.Columns[3].ColumnWidth = 22;
                worksheet.Columns[4].ColumnWidth = 22;
                worksheet.Columns[5].ColumnWidth = 20;
                worksheet.Columns[6].ColumnWidth = 20;

                worksheet.Range[$"A{tableStartRow + 1}:F100"].Font.Name = "Times New Roman";
                worksheet.Range[$"A{tableStartRow + 1}:F100"].Font.Size = 10;

                if (certificatesStats.Rows.Count > 0)
                {
                    int dataStartRow = tableStartRow + 1;

                    for (int i = 0; i < certificatesStats.Columns.Count; i++)
                    {
                        Excel.Range cell = (Excel.Range)worksheet.Cells[dataStartRow, i + 1];
                        cell.Value = certificatesStats.Columns[i].ColumnName;
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

                    for (int i = 0; i < certificatesStats.Rows.Count; i++)
                    {
                        ((Excel.Range)worksheet.Rows[dataStartRow + 1 + i]).RowHeight = 25;

                        for (int j = 0; j < certificatesStats.Columns.Count; j++)
                        {
                            Excel.Range cell = (Excel.Range)worksheet.Cells[dataStartRow + 1 + i, j + 1];

                            if (certificatesStats.Rows[i][j] != DBNull.Value)
                            {
                                if (j >= 2 && certificatesStats.Rows[i][j] is decimal)
                                {
                                    cell.Value = Convert.ToDecimal(certificatesStats.Rows[i][j]);
                                }
                                else
                                {
                                    cell.Value = certificatesStats.Rows[i][j];
                                }
                            }
                            else
                            {
                                cell.Value = "";
                            }

                            cell.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                            cell.Borders.Weight = Excel.XlBorderWeight.xlThin;
                            cell.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                            cell.WrapText = true;

                            if (j == 0)
                            {
                                cell.HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
                            }
                            else if (j == 1)
                            {
                                cell.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                            }
                            else
                            {
                                cell.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                                if (j >= 2)
                                {
                                    cell.NumberFormat = "#,##0.00";
                                }
                            }
                        }
                    }

                    int totalRow = dataStartRow + 1 + certificatesStats.Rows.Count;

                    Excel.Range totalLabelRange = worksheet.Range[$"A{totalRow}"];
                    totalLabelRange.Value = "ИТОГО:";
                    totalLabelRange.Font.Bold = true;
                    totalLabelRange.Font.Size = 11;
                    totalLabelRange.Font.Name = "Times New Roman";
                    totalLabelRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
                    totalLabelRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    totalLabelRange.Borders.Weight = Excel.XlBorderWeight.xlThin;

                    int totalCertificates = 0;
                    decimal totalSum_all = 0;

                    foreach (DataRow row in certificatesStats.Rows)
                    {
                        totalCertificates += Convert.ToInt32(row["Количество"]);
                        totalSum_all += row["Общая сумма"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Общая сумма"]);
                    }

                    Excel.Range totalCountRange = worksheet.Range[$"B{totalRow}"];
                    totalCountRange.Value = totalCertificates;
                    totalCountRange.Font.Bold = true;
                    totalCountRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                    totalCountRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    totalCountRange.Borders.Weight = Excel.XlBorderWeight.xlThin;

                    Excel.Range totalSumRange = worksheet.Range[$"C{totalRow}"];
                    totalSumRange.Value = totalSum_all;
                    totalSumRange.Font.Bold = true;
                    totalSumRange.NumberFormat = "#,##0.00";
                    totalSumRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                    totalSumRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    totalSumRange.Borders.Weight = Excel.XlBorderWeight.xlThin;

                    for (int j = 3; j < certificatesStats.Columns.Count; j++)
                    {
                        Excel.Range emptyCell = (Excel.Range)worksheet.Cells[totalRow, j + 1];
                        emptyCell.Value = "";
                        emptyCell.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                        emptyCell.Borders.Weight = Excel.XlBorderWeight.xlThin;
                    }

                    ((Excel.Range)worksheet.Rows[totalRow]).RowHeight = 30;
                    ((Excel.Range)worksheet.Rows[totalRow]).Font.Bold = true;
                }
                else
                {
                    Excel.Range noDataRange = worksheet.Range[$"A{tableStartRow + 1}:F{tableStartRow + 1}"];
                    noDataRange.Merge();
                    noDataRange.Value = "Нет данных за выбранный период";
                    noDataRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    noDataRange.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                    noDataRange.Font.Italic = true;
                    noDataRange.Font.Size = 12;
                    noDataRange.Font.Name = "Times New Roman";
                    noDataRange.RowHeight = 40;
                    noDataRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    noDataRange.Borders.Weight = Excel.XlBorderWeight.xlThin;
                }

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

        private void CertificateStatisticsForm_Load(object sender, EventArgs e)
        {

        }
    }
}