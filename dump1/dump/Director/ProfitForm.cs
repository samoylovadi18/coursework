using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Excel = Microsoft.Office.Interop.Excel;

namespace dump
{
    public partial class ProfitForm : Form
    {
        private DataTable profitData;
        private DateTime minDate = new DateTime(2024, 1, 1);
        private DateTime maxDate = new DateTime(2040, 12, 31);
        private CultureInfo russianCulture = new CultureInfo("ru-RU");

        public ProfitForm()
        {
            InitializeComponent();
            profitData = new DataTable();

            // Настройка кнопки "Сформировать отчёт"
            btnGenerate.FlatStyle = FlatStyle.Flat;
            btnGenerate.FlatAppearance.BorderSize = 1;
            btnGenerate.FlatAppearance.BorderColor = Color.Black;
            btnGenerate.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            btnGenerate.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            btnGenerate.MouseDown += (s, e) => btnGenerate.FlatAppearance.BorderColor = Color.DarkBlue;
            btnGenerate.MouseUp += (s, e) => btnGenerate.FlatAppearance.BorderColor = Color.Black;
            btnGenerate.MouseLeave += (s, e) => btnGenerate.FlatAppearance.BorderColor = Color.Black;

            // Настройка кнопки "Экспорт"
            btnExport.FlatStyle = FlatStyle.Flat;
            btnExport.FlatAppearance.BorderSize = 1;
            btnExport.FlatAppearance.BorderColor = Color.Black;
            btnExport.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            btnExport.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            btnExport.MouseDown += (s, e) => btnExport.FlatAppearance.BorderColor = Color.DarkBlue;
            btnExport.MouseUp += (s, e) => btnExport.FlatAppearance.BorderColor = Color.Black;
            btnExport.MouseLeave += (s, e) => btnExport.FlatAppearance.BorderColor = Color.Black;

            // Подписка на события
            btnGenerate.Click += btnGenerate_Click;
            btnExport.Click += btnExport_Click;
            pictureBoxBack.Click += PictureBoxBack_Click;
        }

        private void ProfitForm_Load(object sender, EventArgs e)
        {
            // Устанавливаем ограничения для DatePicker
            dtpStartDate.MinDate = minDate;
            dtpStartDate.MaxDate = DateTime.Now > maxDate ? maxDate : DateTime.Now;
            dtpEndDate.MinDate = minDate;
            dtpEndDate.MaxDate = DateTime.Now > maxDate ? maxDate : DateTime.Now;

            // Устанавливаем значения по умолчанию (текущий месяц)
            dtpEndDate.Value = DateTime.Now;
            dtpStartDate.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            // Настраиваем подписи
            labelStartDate.Text = "Период с:";
            labelEndDate.Text = "по:";

            // Настройка DataGridView
            SetupDataGridView();

            // Загружаем данные
            LoadProfitData();
        }

        private void SetupDataGridView()
        {
            // Отключаем авто-генерацию колонок
            dgvProfit.AutoGenerateColumns = false;

            // Очищаем все существующие колонки
            dgvProfit.Columns.Clear();

            // Настройка для таблицы прибыли - КАК В OrdersReportForm
            dgvProfit.ReadOnly = true;
            dgvProfit.AllowUserToAddRows = false;
            dgvProfit.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvProfit.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProfit.MultiSelect = false;
            dgvProfit.RowHeadersVisible = false;
            dgvProfit.EnableHeadersVisualStyles = false;

            // Цвет шапки как в OrdersForm (зеленый)
            Color headerBackColor = Color.FromArgb(97, 173, 123);

            // Цвет выделения как в OrdersForm (светло-зеленый)
            Color selectionColor = Color.FromArgb(233, 242, 236);

            // Настройка шапки - КАК В ORDERSFORM (ЖИРНЫЙ ТЕКСТ)
            dgvProfit.ColumnHeadersDefaultCellStyle.BackColor = headerBackColor;
            dgvProfit.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvProfit.ColumnHeadersDefaultCellStyle.Font = new Font("Times New Roman", 12, FontStyle.Bold); // ЖИРНЫЙ
            dgvProfit.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvProfit.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvProfit.ColumnHeadersDefaultCellStyle.Padding = new Padding(0, 5, 0, 5); // Увеличил отступы
            dgvProfit.ColumnHeadersHeight = 60; // УВЕЛИЧИЛ ВЫСОТУ ШАПКИ С 45 ДО 60
            dgvProfit.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            // Настройка строк - КАК В ORDERSFORM
            dgvProfit.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            dgvProfit.DefaultCellStyle.Padding = new Padding(5);
            dgvProfit.DefaultCellStyle.BackColor = Color.White;
            dgvProfit.DefaultCellStyle.ForeColor = Color.Black;
            dgvProfit.DefaultCellStyle.SelectionBackColor = selectionColor;
            dgvProfit.DefaultCellStyle.SelectionForeColor = Color.Black;

            dgvProfit.RowsDefaultCellStyle.BackColor = Color.White;
            dgvProfit.RowsDefaultCellStyle.ForeColor = Color.Black;
            dgvProfit.RowsDefaultCellStyle.SelectionBackColor = selectionColor;
            dgvProfit.RowsDefaultCellStyle.SelectionForeColor = Color.Black;

            dgvProfit.RowTemplate.Height = 35;
            dgvProfit.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvProfit.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            // Настройка сетки
            dgvProfit.GridColor = Color.Gray;
            dgvProfit.BorderStyle = BorderStyle.Fixed3D;
            dgvProfit.CellBorderStyle = DataGridViewCellBorderStyle.Single;

            // СОЗДАЕМ КОЛОНКИ ВРУЧНУЮ С РУССКИМИ ЗАГОЛОВКАМИ

            // Колонка 1 - Дата
            DataGridViewTextBoxColumn colDate = new DataGridViewTextBoxColumn();
            colDate.Name = "date";
            colDate.HeaderText = "Дата";
            colDate.DataPropertyName = "Дата";
            colDate.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colDate.DefaultCellStyle.Format = "dd.MM.yyyy";
            colDate.Width = 100;
            colDate.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvProfit.Columns.Add(colDate);

            // Колонка 2 - Выручка
            DataGridViewTextBoxColumn colRevenue = new DataGridViewTextBoxColumn();
            colRevenue.Name = "revenue";
            colRevenue.HeaderText = "Выручка (₽)";
            colRevenue.DataPropertyName = "Выручка";
            colRevenue.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colRevenue.DefaultCellStyle.Format = "N2";
            colRevenue.DefaultCellStyle.ForeColor = Color.DarkGreen;
            colRevenue.DefaultCellStyle.SelectionForeColor = Color.DarkGreen;
            colRevenue.Width = 120;
            colRevenue.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvProfit.Columns.Add(colRevenue);

            // Колонка 3 - Себестоимость
            DataGridViewTextBoxColumn colCost = new DataGridViewTextBoxColumn();
            colCost.Name = "total_cost";
            colCost.HeaderText = "Себестоимость (₽)";
            colCost.DataPropertyName = "Себестоимость";
            colCost.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colCost.DefaultCellStyle.Format = "N2";
            colCost.DefaultCellStyle.ForeColor = Color.DarkRed;
            colCost.DefaultCellStyle.SelectionForeColor = Color.DarkRed;
            colCost.Width = 120;
            colCost.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvProfit.Columns.Add(colCost);

            // Колонка 4 - Прибыль
            DataGridViewTextBoxColumn colProfit = new DataGridViewTextBoxColumn();
            colProfit.Name = "profit";
            colProfit.HeaderText = "Прибыль (₽)";
            colProfit.DataPropertyName = "Прибыль";
            colProfit.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colProfit.DefaultCellStyle.Format = "N2";
            colProfit.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Bold);
            colProfit.Width = 120;
            colProfit.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvProfit.Columns.Add(colProfit);

            // Колонка 5 - Количество заказов
            DataGridViewTextBoxColumn colOrders = new DataGridViewTextBoxColumn();
            colOrders.Name = "orders_count";
            colOrders.HeaderText = "Кол-во заказов";
            colOrders.DataPropertyName = "Кол-во заказов";
            colOrders.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colOrders.Width = 100;
            colOrders.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvProfit.Columns.Add(colOrders);

            // Колонка 6 - Маржа
            DataGridViewTextBoxColumn colMargin = new DataGridViewTextBoxColumn();
            colMargin.Name = "margin";
            colMargin.HeaderText = "Маржа %";
            colMargin.DataPropertyName = "Маржа %";
            colMargin.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colMargin.DefaultCellStyle.Format = "F1";
            colMargin.Width = 80;
            colMargin.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvProfit.Columns.Add(colMargin);
        }

        // МЕТОД ДЛЯ ПРИНУДИТЕЛЬНОГО ПРИМЕНЕНИЯ СТИЛЕЙ ШАПКИ
        private void ApplyHeaderStyles()
        {
            if (dgvProfit.Columns.Count > 0)
            {
                // Убеждаемся, что визуальные стили отключены
                dgvProfit.EnableHeadersVisualStyles = false;

                // Применяем стиль шапки глобально
                dgvProfit.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(97, 173, 123);
                dgvProfit.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
                dgvProfit.ColumnHeadersDefaultCellStyle.Font = new Font("Times New Roman", 12, FontStyle.Bold);
                dgvProfit.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgvProfit.ColumnHeadersHeight = 60; // УВЕЛИЧИЛ ВЫСОТУ ШАПКИ

                // Принудительно применяем к каждой колонке
                foreach (DataGridViewColumn col in dgvProfit.Columns)
                {
                    col.HeaderCell.Style.BackColor = Color.FromArgb(97, 173, 123);
                    col.HeaderCell.Style.ForeColor = Color.Black;
                    col.HeaderCell.Style.Font = new Font("Times New Roman", 12, FontStyle.Bold);
                    col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    col.HeaderCell.Style.Padding = new Padding(0, 5, 0, 5);
                }

                // Перерисовываем
                dgvProfit.Refresh();
            }
        }

        private void LoadProfitData()
        {
            try
            {
                if (dtpStartDate.Value.Date > dtpEndDate.Value.Date)
                {
                    MessageBox.Show("Дата 'С' не может быть позже даты 'По'!",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DateTime startDate = dtpStartDate.Value.Date;
                DateTime endDate = dtpEndDate.Value.Date;

                profitData = LoadProfitFromDB(startDate, endDate);

                // ВАЖНО: очищаем DataSource перед установкой нового
                dgvProfit.DataSource = null;


                // Устанавливаем DataSource
                dgvProfit.DataSource = profitData;

                // ПРИНУДИТЕЛЬНО ПРИМЕНЯЕМ СТИЛИ ШАПКИ ПОСЛЕ УСТАНОВКИ DataSource
                ApplyHeaderStyles();

                // Обновляем DataGridView
                dgvProfit.Refresh();

                // Подсчитываем итоги
                CalculateTotals();
              
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DataTable LoadProfitFromDB(DateTime startDate, DateTime endDate)
        {
            DataTable dt = new DataTable();

            string query = @"
                SELECT 
                    DATE(o.created_at) as date,
                    COALESCE(SUM(od.quantity * od.price_at_order), 0) as revenue,
                    COALESCE(SUM(od.quantity * d.cost), 0) as total_cost,
                    COUNT(DISTINCT o.id_order) as orders_count
                FROM orders o
                LEFT JOIN order_dish od ON o.id_order = od.id_order
                LEFT JOIN dishes d ON od.id_dish = d.id_dish
                WHERE DATE(o.created_at) BETWEEN @startDate AND @endDate
                AND o.id_status IN (4,5,6)
                GROUP BY DATE(o.created_at)
                ORDER BY DATE(o.created_at)";

            using (var connection = SettingsBD.GetConnection())
            {
                connection.Open();
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@startDate", startDate);
                    cmd.Parameters.AddWithValue("@endDate", endDate);

                    using (var adapter = new MySqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }

            // Добавляем колонки для прибыли и маржи
            dt.Columns.Add("profit", typeof(decimal));
            dt.Columns.Add("margin", typeof(decimal));

            foreach (DataRow row in dt.Rows)
            {
                decimal revenue = Convert.ToDecimal(row["revenue"]);
                decimal cost = Convert.ToDecimal(row["total_cost"]);
                decimal profit = revenue - cost;
                decimal margin = revenue > 0 ? (profit / revenue) * 100 : 0;

                row["profit"] = profit;
                row["margin"] = margin;
            }

            // ПЕРЕИМЕНОВЫВАЕМ КОЛОНКИ В DATATABLE НА РУССКИЕ
            dt.Columns["date"].ColumnName = "Дата";
            dt.Columns["revenue"].ColumnName = "Выручка";
            dt.Columns["total_cost"].ColumnName = "Себестоимость";
            dt.Columns["orders_count"].ColumnName = "Кол-во заказов";
            dt.Columns["profit"].ColumnName = "Прибыль";
            dt.Columns["margin"].ColumnName = "Маржа %";

            return dt;
        }

        private void CalculateTotals()
        {
            try
            {

                decimal totalRevenue = 0;
                decimal totalCost = 0;
                int totalOrders = 0;

                foreach (DataRow row in profitData.Rows)
                {
                    totalRevenue += Convert.ToDecimal(row["Выручка"]);
                    totalCost += Convert.ToDecimal(row["Себестоимость"]);
                    totalOrders += Convert.ToInt32(row["Кол-во заказов"]);
                }

                decimal totalProfit = totalRevenue - totalCost;
                decimal avgMargin = totalRevenue > 0 ? (totalProfit / totalRevenue) * 100 : 0;

            }
            catch (Exception ex)
            {
                lblTotalProfit.Text = "Не удалось рассчитать итоги";
                Console.WriteLine("Ошибка в CalculateTotals: " + ex.Message);
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                if (profitData == null || profitData.Rows.Count == 0)
                {
                    MessageBox.Show("Нет данных для экспорта!", "Предупреждение",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Excel файлы (*.xlsx)|*.xlsx|CSV файлы (*.csv)|*.csv";
                saveDialog.FileName = $"Отчет_по_прибыли_{dtpStartDate.Value:yyyyMMdd}-{dtpEndDate.Value:yyyyMMdd}";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    if (saveDialog.FileName.EndsWith(".csv"))
                        ExportToCSV(saveDialog.FileName);
                    else
                        ExportToExcel(saveDialog.FileName);

                    DialogResult result = MessageBox.Show($"✅ Файл сохранен!\n{saveDialog.FileName}\n\nОткрыть файл?",
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
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToCSV(string filePath)
        {
            using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                sw.WriteLine($"Отчет по прибыли за период с {dtpStartDate.Value:dd.MM.yyyy} по {dtpEndDate.Value:dd.MM.yyyy}");
                sw.WriteLine("=================================================================");
                sw.WriteLine();
                sw.WriteLine("Дата;Выручка;Себестоимость;Прибыль;Кол-во заказов;Маржа %");

                foreach (DataRow row in profitData.Rows)
                {
                    sw.WriteLine($"{Convert.ToDateTime(row["Дата"]):dd.MM.yyyy};{Convert.ToDecimal(row["Выручка"]):F2};" +
                        $"{Convert.ToDecimal(row["Себестоимость"]):F2};{Convert.ToDecimal(row["Прибыль"]):F2};" +
                        $"{row["Кол-во заказов"]};{Convert.ToDecimal(row["Маржа %"]):F1}");
                }

                sw.WriteLine();
                sw.WriteLine("ИТОГИ:");
                decimal totalRevenue = 0, totalCost = 0;
                int totalOrders = 0;

                foreach (DataRow row in profitData.Rows)
                {
                    totalRevenue += Convert.ToDecimal(row["Выручка"]);
                    totalCost += Convert.ToDecimal(row["Себестоимость"]);
                    totalOrders += Convert.ToInt32(row["Кол-во заказов"]);
                }

                decimal totalProfit = totalRevenue - totalCost;
                decimal avgMargin = totalRevenue > 0 ? (totalProfit / totalRevenue) * 100 : 0;

                sw.WriteLine($"Общая выручка: {totalRevenue:F2}");
                sw.WriteLine($"Общая себестоимость: {totalCost:F2}");
                sw.WriteLine($"Общая прибыль: {totalProfit:F2}");
                sw.WriteLine($"Всего заказов: {totalOrders}");
                sw.WriteLine($"Средняя маржа: {avgMargin:F1}%");
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
                excelApp.Visible = false;
                excelApp.DisplayAlerts = false;

                workbook = excelApp.Workbooks.Add();
                worksheet = workbook.Worksheets[1];
                worksheet.Name = "Прибыль";

                // Заголовок
                worksheet.Cells[1, 1] = $"Отчет по прибыли за период с {dtpStartDate.Value:dd.MM.yyyy} по {dtpEndDate.Value:dd.MM.yyyy}";
                worksheet.Cells[1, 1].Font.Bold = true;
                worksheet.Cells[1, 1].Font.Size = 14;
                Excel.Range titleRange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, 6]];
                titleRange.Merge();
                titleRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                // Заголовки колонок - РУССКИЕ
                string[] headers = { "Дата", "Выручка (₽)", "Себестоимость (₽)", "Прибыль (₽)", "Кол-во заказов", "Маржа %" };
                for (int i = 0; i < headers.Length; i++)
                {
                    Excel.Range headerCell = worksheet.Cells[3, i + 1];
                    headerCell.Value = headers[i];
                    headerCell.Font.Bold = true;
                    headerCell.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(97, 173, 123));
                    headerCell.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    headerCell.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                }

                // Данные
                for (int row = 0; row < profitData.Rows.Count; row++)
                {
                    worksheet.Cells[row + 4, 1] = Convert.ToDateTime(profitData.Rows[row]["Дата"]).ToString("dd.MM.yyyy");

                    Excel.Range revenueCell = worksheet.Cells[row + 4, 2];
                    revenueCell.Value = Convert.ToDouble(profitData.Rows[row]["Выручка"]);
                    revenueCell.NumberFormat = "#,##0.00";
                    revenueCell.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;

                    Excel.Range costCell = worksheet.Cells[row + 4, 3];
                    costCell.Value = Convert.ToDouble(profitData.Rows[row]["Себестоимость"]);
                    costCell.NumberFormat = "#,##0.00";
                    costCell.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;

                    Excel.Range profitCell = worksheet.Cells[row + 4, 4];
                    profitCell.Value = Convert.ToDouble(profitData.Rows[row]["Прибыль"]);
                    profitCell.NumberFormat = "#,##0.00";
                    profitCell.Font.Bold = true;
                    profitCell.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;

                    worksheet.Cells[row + 4, 5] = profitData.Rows[row]["Кол-во заказов"].ToString();

                    Excel.Range marginCell = worksheet.Cells[row + 4, 6];
                    marginCell.Value = Convert.ToDouble(profitData.Rows[row]["Маржа %"]);
                    marginCell.NumberFormat = "0.0";
                    marginCell.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                    // Границы
                    Excel.Range dataRange = worksheet.Range[worksheet.Cells[row + 4, 1], worksheet.Cells[row + 4, 6]];
                    dataRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                }

                // Итоги
                int lastRow = profitData.Rows.Count + 5;
                decimal totalRevenue = 0, totalCost = 0;
                int totalOrders = 0;

                foreach (DataRow row in profitData.Rows)
                {
                    totalRevenue += Convert.ToDecimal(row["Выручка"]);
                    totalCost += Convert.ToDecimal(row["Себестоимость"]);
                    totalOrders += Convert.ToInt32(row["Кол-во заказов"]);
                }

                decimal totalProfit = totalRevenue - totalCost;
                decimal avgMargin = totalRevenue > 0 ? (totalProfit / totalRevenue) * 100 : 0;

                // Заголовок "ИТОГИ"
                Excel.Range totalTitleCell = worksheet.Cells[lastRow, 1];
                totalTitleCell.Value = "ИТОГИ:";
                totalTitleCell.Font.Bold = true;
                totalTitleCell.Font.Size = 12;
                Excel.Range totalTitleRange = worksheet.Range[worksheet.Cells[lastRow, 1], worksheet.Cells[lastRow, 2]];
                totalTitleRange.Merge();

                // Общая выручка
                worksheet.Cells[lastRow + 1, 1] = "Общая выручка:";
                worksheet.Cells[lastRow + 1, 1].Font.Bold = true;
                Excel.Range revenueCell2 = worksheet.Cells[lastRow + 1, 2];
                revenueCell2.Value = Convert.ToDouble(totalRevenue);
                revenueCell2.NumberFormat = "#,##0.00";
                revenueCell2.Font.Bold = true;

                // Общая себестоимость
                worksheet.Cells[lastRow + 2, 1] = "Общая себестоимость:";
                worksheet.Cells[lastRow + 2, 1].Font.Bold = true;
                Excel.Range costCell2 = worksheet.Cells[lastRow + 2, 2];
                costCell2.Value = Convert.ToDouble(totalCost);
                costCell2.NumberFormat = "#,##0.00";
                costCell2.Font.Bold = true;

                // Общая прибыль
                worksheet.Cells[lastRow + 3, 1] = "Общая прибыль:";
                worksheet.Cells[lastRow + 3, 1].Font.Bold = true;
                Excel.Range profitCell2 = worksheet.Cells[lastRow + 3, 2];
                profitCell2.Value = Convert.ToDouble(totalProfit);
                profitCell2.NumberFormat = "#,##0.00";
                profitCell2.Font.Bold = true;

                // Всего заказов
                worksheet.Cells[lastRow + 4, 1] = "Всего заказов:";
                worksheet.Cells[lastRow + 4, 1].Font.Bold = true;
                worksheet.Cells[lastRow + 4, 2] = totalOrders;

                // Средняя маржа
                worksheet.Cells[lastRow + 5, 1] = "Средняя маржа:";
                worksheet.Cells[lastRow + 5, 1].Font.Bold = true;
                Excel.Range avgCell = worksheet.Cells[lastRow + 5, 2];
                avgCell.Value = Convert.ToDouble(avgMargin);
                avgCell.NumberFormat = "0.0";
                avgCell.Font.Bold = true;

                worksheet.Columns.AutoFit();
                workbook.SaveAs(filePath);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при создании Excel: {ex.Message}");
            }
            finally
            {
                if (worksheet != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);
                if (workbook != null) { workbook.Close(false); System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook); }
                if (excelApp != null) { excelApp.Quit(); System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp); }
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private void PictureBoxBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            LoadProfitData();
        }

        private void pictureBoxBack_Click_1(object sender, EventArgs e)
        {
            this.Visible = false;
            if (this.Owner != null && !this.Owner.IsDisposed)
            {
                this.Owner.Show(); // Показываем родительскую форму
            }
        }

        private void ProfitForm_Load_1(object sender, EventArgs e)
        {

        }
    }
}