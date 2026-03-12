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
    public partial class TopDishForm : Form
    {
        private DataTable dishesData;
        private System.Windows.Forms.ToolTip toolTip1;

        // Названия элементов для добавления на форму:
        // dateTimePickerStart - выбор начальной даты
        // dateTimePickerEnd - выбор конечной даты
        // comboBoxCategory - выбор категории
        // buttonGenerate - кнопка "Сформировать отчёт"
        // buttonExport - кнопка "Экспорт в Excel"
        // buttonReset - кнопка "Сброс"
        // dataGridViewTopDish - таблица для отображения результатов
        // labelTotalRevenue - общая выручка
        // labelTotalSold - общее количество проданных блюд

        public TopDishForm()
        {
            InitializeComponent();
            dishesData = new DataTable();
            toolTip1 = new System.Windows.Forms.ToolTip();

            // Установка дат по умолчанию
            dateTimePickerStart.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dateTimePickerEnd.Value = DateTime.Now;

            // Настройка кнопок в стиле OrdersReportForm
            SetupButtonStyles();

            // Подписка на события
            buttonGenerate.Click += ButtonGenerate_Click;
            buttonExport.Click += ButtonExport_Click;
            this.Load += TopDishForm_Load;
        }

        private void SetupButtonStyles()
        {
            // Настройка кнопки Generate
            buttonGenerate.FlatStyle = FlatStyle.Flat;
            buttonGenerate.FlatAppearance.BorderSize = 1;
            buttonGenerate.FlatAppearance.BorderColor = Color.Black;
            buttonGenerate.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            buttonGenerate.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            buttonGenerate.MouseDown += (s, e) =>
            {
                buttonGenerate.FlatAppearance.BorderColor = Color.DarkBlue;
            };
            buttonGenerate.MouseUp += (s, e) =>
            {
                buttonGenerate.FlatAppearance.BorderColor = Color.Black;
            };
            buttonGenerate.MouseLeave += (s, e) =>
            {
                buttonGenerate.FlatAppearance.BorderColor = Color.Black;
            };

            // Настройка кнопки Export
            buttonExport.FlatStyle = FlatStyle.Flat;
            buttonExport.FlatAppearance.BorderSize = 1;
            buttonExport.FlatAppearance.BorderColor = Color.Black;
            buttonExport.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            buttonExport.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            buttonExport.MouseDown += (s, e) =>
            {
                buttonExport.FlatAppearance.BorderColor = Color.DarkBlue;
            };
            buttonExport.MouseUp += (s, e) =>
            {
                buttonExport.FlatAppearance.BorderColor = Color.Black;
            };
            buttonExport.MouseLeave += (s, e) =>
            {
                buttonExport.FlatAppearance.BorderColor = Color.Black;
            };
        }

        private void TopDishForm_Load(object sender, EventArgs e)
        {
            // Настройка DataGridView
            SetupDataGridView();

            // Загружаем категории
            LoadCategories();

            // Скрываем лейблы при загрузке формы
            labelTotalRevenue.Visible = false;
            labelTotalSold.Visible = false;
        }

        private void SetupDataGridView()
        {
            dataGridViewTopDish.ReadOnly = true;
            dataGridViewTopDish.AllowUserToAddRows = false;
            dataGridViewTopDish.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewTopDish.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewTopDish.MultiSelect = false;
            dataGridViewTopDish.RowHeadersVisible = false;
            dataGridViewTopDish.EnableHeadersVisualStyles = false;

            // Цвет шапки как в OrdersReportForm (зеленый)
            Color headerBackColor = Color.FromArgb(97, 173, 123);

            // Цвет выделения как в OrdersReportForm (светло-зеленый)
            Color selectionColor = Color.FromArgb(233, 242, 236);

            // Настройка шапки - КАК В ORDERSREPORTFORM
            dataGridViewTopDish.ColumnHeadersDefaultCellStyle.BackColor = headerBackColor;
            dataGridViewTopDish.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dataGridViewTopDish.ColumnHeadersDefaultCellStyle.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            dataGridViewTopDish.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewTopDish.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridViewTopDish.ColumnHeadersDefaultCellStyle.Padding = new Padding(0, 3, 0, 3);
            dataGridViewTopDish.ColumnHeadersHeight = 45;
            dataGridViewTopDish.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            // Настройка строк - КАК В ORDERSREPORTFORM
            dataGridViewTopDish.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            dataGridViewTopDish.DefaultCellStyle.Padding = new Padding(5);
            dataGridViewTopDish.DefaultCellStyle.BackColor = Color.White;
            dataGridViewTopDish.DefaultCellStyle.ForeColor = Color.Black;
            dataGridViewTopDish.DefaultCellStyle.SelectionBackColor = selectionColor;
            dataGridViewTopDish.DefaultCellStyle.SelectionForeColor = Color.Black;

            dataGridViewTopDish.RowsDefaultCellStyle.BackColor = Color.White;
            dataGridViewTopDish.RowsDefaultCellStyle.ForeColor = Color.Black;
            dataGridViewTopDish.RowsDefaultCellStyle.SelectionBackColor = selectionColor;
            dataGridViewTopDish.RowsDefaultCellStyle.SelectionForeColor = Color.Black;

            dataGridViewTopDish.RowTemplate.Height = 35;
            dataGridViewTopDish.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridViewTopDish.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            // Настройка сетки
            dataGridViewTopDish.GridColor = Color.Gray;
            dataGridViewTopDish.BorderStyle = BorderStyle.Fixed3D;
            dataGridViewTopDish.CellBorderStyle = DataGridViewCellBorderStyle.Single;

            // Добавляем подсказку
            toolTip1.SetToolTip(dataGridViewTopDish, "Топ 10 блюд по выручке");

            // Создаем колонки вручную
            dataGridViewTopDish.Columns.Clear();

            // Колонка Блюдо
            DataGridViewTextBoxColumn colDishName = new DataGridViewTextBoxColumn();
            colDishName.Name = "dish_name";
            colDishName.HeaderText = "Блюдо";
            colDishName.DataPropertyName = "Блюдо";
            colDishName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colDishName.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridViewTopDish.Columns.Add(colDishName);

            // Колонка Категория
            DataGridViewTextBoxColumn colCategory = new DataGridViewTextBoxColumn();
            colCategory.Name = "category";
            colCategory.HeaderText = "Категория";
            colCategory.DataPropertyName = "Категория";
            colCategory.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colCategory.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridViewTopDish.Columns.Add(colCategory);

            // Колонка Количество продаж
            DataGridViewTextBoxColumn colQuantity = new DataGridViewTextBoxColumn();
            colQuantity.Name = "quantity";
            colQuantity.HeaderText = "Кол-во продаж";
            colQuantity.DataPropertyName = "Кол-во продаж";
            colQuantity.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colQuantity.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridViewTopDish.Columns.Add(colQuantity);

            // Колонка Общая выручка
            DataGridViewTextBoxColumn colRevenue = new DataGridViewTextBoxColumn();
            colRevenue.Name = "revenue";
            colRevenue.HeaderText = "Общая выручка";
            colRevenue.DataPropertyName = "Общая выручка";
            colRevenue.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colRevenue.DefaultCellStyle.Format = "N2";
            colRevenue.DefaultCellStyle.ForeColor = Color.DarkGreen;
            colRevenue.DefaultCellStyle.SelectionForeColor = Color.DarkGreen;
            colRevenue.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridViewTopDish.Columns.Add(colRevenue);
        }

        private void LoadCategories()
        {
            try
            {
                string query = "SELECT id_category, category_name FROM categories ORDER BY category_name";

                using (var connection = SettingsBD.GetConnection())
                {
                    connection.Open();
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        // Добавляем пункт "Все категории"
                        DataRow row = dt.NewRow();
                        row["id_category"] = 0;
                        row["category_name"] = "Все категории";
                        dt.Rows.InsertAt(row, 0);

                        comboBoxCategory.DisplayMember = "category_name";
                        comboBoxCategory.ValueMember = "id_category";
                        comboBoxCategory.DataSource = dt;
                        comboBoxCategory.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке категорий: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ButtonGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                if (dateTimePickerStart.Value > dateTimePickerEnd.Value)
                {
                    MessageBox.Show("Начальная дата не может быть больше конечной!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                LoadTopDishes();
                UpdateSummaryInfo();

                // Показываем лейблы после успешной загрузки данных
                if (dishesData != null && dishesData.Rows.Count > 0)
                {
                    labelTotalRevenue.Visible = true;
                    labelTotalSold.Visible = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadTopDishes()
        {
            int selectedCategory = Convert.ToInt32(comboBoxCategory.SelectedValue);

            string query = @"
                SELECT 
                    d.dish_name AS 'Блюдо',
                    c.category_name AS 'Категория',
                    SUM(od.quantity) AS 'Кол-во продаж',
                    SUM(od.quantity * od.price_at_order) AS 'Общая выручка'
                FROM order_dish od
                JOIN dishes d ON od.id_dish = d.id_dish
                JOIN categories c ON d.id_category = c.id_category
                JOIN orders o ON od.id_order = o.id_order
                WHERE o.id_status IN (4,5,6)"; // Только доставленные/готовые заказы

            if (selectedCategory != 0)
            {
                query += " AND d.id_category = @categoryId";
            }

            query += @" AND DATE(o.delivery_date) BETWEEN @startDate AND @endDate
                GROUP BY d.id_dish
                ORDER BY SUM(od.quantity * od.price_at_order) DESC
                LIMIT 10";

            using (var connection = SettingsBD.GetConnection())
            {
                connection.Open();
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@startDate", dateTimePickerStart.Value.Date);
                    cmd.Parameters.AddWithValue("@endDate", dateTimePickerEnd.Value.Date);

                    if (selectedCategory != 0)
                    {
                        cmd.Parameters.AddWithValue("@categoryId", selectedCategory);
                    }

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    dishesData.Clear();
                    adapter.Fill(dishesData);

                    dataGridViewTopDish.DataSource = dishesData;

                    // Применяем форматирование после установки DataSource
                    if (dataGridViewTopDish.Columns["Общая выручка"] != null)
                    {
                        dataGridViewTopDish.Columns["Общая выручка"].DefaultCellStyle.Format = "N2";
                        dataGridViewTopDish.Columns["Общая выручка"].DefaultCellStyle.ForeColor = Color.DarkGreen;
                        dataGridViewTopDish.Columns["Общая выручка"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    }

                    if (dataGridViewTopDish.Columns["Кол-во продаж"] != null)
                    {
                        dataGridViewTopDish.Columns["Кол-во продаж"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    }
                }
            }
        }

        private void UpdateSummaryInfo()
        {
            decimal totalRevenue = 0;
            int totalSold = 0;

            foreach (DataRow row in dishesData.Rows)
            {
                totalRevenue += Convert.ToDecimal(row["Общая выручка"]);
                totalSold += Convert.ToInt32(row["Кол-во продаж"]);
            }

            labelTotalRevenue.Text = $"Общая выручка: {totalRevenue:N2} ₽";
            labelTotalSold.Text = $"Всего продано: {totalSold} шт.";
        }

        private void ButtonReset_Click(object sender, EventArgs e)
        {
            dateTimePickerStart.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dateTimePickerEnd.Value = DateTime.Now;
            comboBoxCategory.SelectedIndex = 0;

            dishesData.Clear();
            dataGridViewTopDish.DataSource = null;

            // Создаем пустую таблицу с колонками
            DataTable emptyTable = new DataTable();
            emptyTable.Columns.Add("Блюдо", typeof(string));
            emptyTable.Columns.Add("Категория", typeof(string));
            emptyTable.Columns.Add("Кол-во продаж", typeof(int));
            emptyTable.Columns.Add("Общая выручка", typeof(decimal));
            dataGridViewTopDish.DataSource = emptyTable;

            // Скрываем лейблы при сбросе
            labelTotalRevenue.Visible = false;
            labelTotalSold.Visible = false;

            labelTotalRevenue.Text = "Общая выручка: 0 ₽";
            labelTotalSold.Text = "Всего продано: 0 шт.";
        }

        private void ButtonExport_Click(object sender, EventArgs e)
        {
            try
            {
                if (dishesData == null || dishesData.Rows.Count == 0)
                {
                    MessageBox.Show("Сначала сформируйте отчёт!", "Предупреждение",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Excel файлы (*.xlsx)|*.xlsx|Excel файлы (*.xls)|*.xls";
                saveDialog.FileName = $"Топ_блюд_{DateTime.Now:yyyyMMdd_HHmmss}";

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
                worksheet.Name = "Топ блюд";

                // Получаем название выбранной категории
                string categoryName = comboBoxCategory.Text;

                // ЗАГОЛОВОК
                Excel.Range titleRange = worksheet.Range["A1:D1"];
                titleRange.Merge();
                titleRange.Value = "ТОП 10 БЛЮД ПО ВЫРУЧКЕ";
                titleRange.Font.Bold = true;
                titleRange.Font.Size = 14;
                titleRange.Font.Name = "Times New Roman";
                titleRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                titleRange.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                titleRange.RowHeight = 30;

                // ПЕРИОД И КАТЕГОРИЯ
                Excel.Range periodRange = worksheet.Range["A2:D2"];
                periodRange.Merge();
                periodRange.Value = $"Период: {dateTimePickerStart.Value:dd.MM.yyyy} - {dateTimePickerEnd.Value:dd.MM.yyyy} | Категория: {categoryName}";
                periodRange.Font.Bold = true;
                periodRange.Font.Size = 11;
                periodRange.Font.Name = "Times New Roman";
                periodRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                periodRange.RowHeight = 25;

                // Пустая строка
                worksheet.Range["A3:D3"].RowHeight = 10;

                // Устанавливаем ширину колонок
                worksheet.Columns[1].ColumnWidth = 40; // Блюдо
                worksheet.Columns[2].ColumnWidth = 25; // Категория
                worksheet.Columns[3].ColumnWidth = 18; // Кол-во продаж
                worksheet.Columns[4].ColumnWidth = 25; // Общая выручка

                // Заголовки таблицы (зеленый фон как в OrdersReportForm)
                int headerRow = 4;
                string[] headers = { "Блюдо", "Категория", "Кол-во продаж", "Общая выручка" };

                for (int i = 0; i < headers.Length; i++)
                {
                    Excel.Range cell = (Excel.Range)worksheet.Cells[headerRow, i + 1];
                    cell.Value = headers[i];
                    cell.Font.Bold = true;
                    cell.Font.Size = 12;
                    cell.Font.Name = "Times New Roman";
                    cell.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(97, 173, 123));
                    cell.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    cell.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    cell.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                    cell.RowHeight = 30;
                    cell.WrapText = true;
                }

                // Данные
                for (int i = 0; i < dishesData.Rows.Count; i++)
                {
                    int rowNum = headerRow + 1 + i;

                    // Блюдо
                    worksheet.Cells[rowNum, 1] = dishesData.Rows[i]["Блюдо"].ToString();

                    // Категория
                    worksheet.Cells[rowNum, 2] = dishesData.Rows[i]["Категория"].ToString();

                    // Кол-во продаж
                    worksheet.Cells[rowNum, 3] = Convert.ToInt32(dishesData.Rows[i]["Кол-во продаж"]);

                    // Общая выручка
                    worksheet.Cells[rowNum, 4] = Convert.ToDecimal(dishesData.Rows[i]["Общая выручка"]);

                    // Форматирование
                    for (int j = 1; j <= 4; j++)
                    {
                        Excel.Range cell = (Excel.Range)worksheet.Cells[rowNum, j];
                        cell.Font.Name = "Times New Roman";
                        cell.Font.Size = 10;
                        cell.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                        cell.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;

                        if (j == 3)
                            cell.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                        else if (j == 4)
                        {
                            cell.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                            cell.Font.Color = System.Drawing.ColorTranslator.ToOle(Color.DarkGreen);
                        }
                        else
                            cell.HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
                    }

                    // Формат суммы
                    Excel.Range sumCell = (Excel.Range)worksheet.Cells[rowNum, 4];
                    sumCell.NumberFormat = "#,##0.00";

                    ((Excel.Range)worksheet.Rows[rowNum]).RowHeight = 25;
                }

                // Итоговая строка
                int totalRow = headerRow + 1 + dishesData.Rows.Count;

                Excel.Range totalLabelRange = worksheet.Range[$"A{totalRow}:B{totalRow}"];
                totalLabelRange.Merge();
                totalLabelRange.Value = "ИТОГО:";
                totalLabelRange.Font.Bold = true;
                totalLabelRange.Font.Size = 11;
                totalLabelRange.Font.Name = "Times New Roman";
                totalLabelRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                totalLabelRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;

                // Общее количество продаж
                int totalSold = 0;
                decimal totalRevenue = 0;
                foreach (DataRow row in dishesData.Rows)
                {
                    totalSold += Convert.ToInt32(row["Кол-во продаж"]);
                    totalRevenue += Convert.ToDecimal(row["Общая выручка"]);
                }

                Excel.Range totalSoldRange = worksheet.Range[$"C{totalRow}"];
                totalSoldRange.Value = totalSold;
                totalSoldRange.Font.Bold = true;
                totalSoldRange.Font.Name = "Times New Roman";
                totalSoldRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                totalSoldRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;

                Excel.Range totalRevenueRange = worksheet.Range[$"D{totalRow}"];
                totalRevenueRange.Value = totalRevenue;
                totalRevenueRange.Font.Bold = true;
                totalRevenueRange.Font.Name = "Times New Roman";
                totalRevenueRange.NumberFormat = "#,##0.00";
                totalRevenueRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                totalRevenueRange.Font.Color = System.Drawing.ColorTranslator.ToOle(Color.DarkGreen);
                totalRevenueRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;

                ((Excel.Range)worksheet.Rows[totalRow]).RowHeight = 25;

                // Настройка страницы
                worksheet.PageSetup.Orientation = Excel.XlPageOrientation.xlLandscape;
                worksheet.PageSetup.FitToPagesWide = 1;
                worksheet.PageSetup.Zoom = 100;

                workbook.SaveAs(filePath);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при создании Excel: {ex.Message}");
            }
            finally
            {
                if (worksheet != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);
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

        private void pictureBoxBack_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            if (this.Owner != null && !this.Owner.IsDisposed)
            {
                this.Owner.Show(); // Показываем родительскую форму
            }
        }
    }
}