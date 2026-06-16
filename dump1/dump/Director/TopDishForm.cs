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
using Font = System.Drawing.Font;
using DataTable = System.Data.DataTable;
using Action = System.Action;

namespace dump
{
    public partial class TopDishForm : Form
    {
        private DataTable dishesData;
        private System.Windows.Forms.ToolTip toolTip1;
        private bool isLockDialogOpen = false;
        private DateTime minDate = new DateTime(2024, 1, 1); // Минимальная дата

        public TopDishForm()
        {
            InitializeComponent();
            dishesData = new DataTable();
            toolTip1 = new System.Windows.Forms.ToolTip();

            // Настройка ограничений для дат
            dateTimePickerStart.MinDate = minDate;
            dateTimePickerStart.MaxDate = DateTime.Now;
            dateTimePickerEnd.MinDate = minDate;
            dateTimePickerEnd.MaxDate = DateTime.Now;

            dateTimePickerEnd.Value = DateTime.Now;
            dateTimePickerStart.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            SetupButtonStyles();

            // Подписываемся только на экспорт
            buttonExport.Click += ButtonExport_Click;

            this.Load += TopDishForm_Load;
            InactivityManager.RegisterForm(this);
            InactivityManager.OnLockRequest += LockSystem;

            // Добавляем обработчик закрытия формы
            this.FormClosing += TopDishForm_FormClosing;

            // Подписываемся на изменение дат для автоматической загрузки
            dateTimePickerStart.ValueChanged += DateTimePicker_ValueChanged;
            dateTimePickerEnd.ValueChanged += DateTimePicker_ValueChanged;

            // Подписываемся на изменение категории
            comboBoxCategory.SelectedIndexChanged += ComboBoxCategory_SelectedIndexChanged;
        }

        // ===================== АВТОМАТИЧЕСКАЯ ЗАГРУЗКА ПРИ ИЗМЕНЕНИИ =====================

        private void DateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            LoadTopDishesAutomatically();
        }

        private void ComboBoxCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTopDishesAutomatically();
        }

        private void LoadTopDishesAutomatically()
        {
            try
            {
                DateTime startDate = dateTimePickerStart.Value.Date;
                DateTime endDate = dateTimePickerEnd.Value.Date;

                if (startDate > endDate)
                {
                    CreateEmptyTable();
                    labelTotalRevenue.Visible = false;
                    labelTotalSold.Visible = false;
                    return;
                }

                LoadTopDishes();
                UpdateSummaryInfo();

                if (dishesData != null && dishesData.Rows.Count > 0)
                {
                    labelTotalRevenue.Visible = true;
                    labelTotalSold.Visible = true;
                }
                else
                {
                    labelTotalRevenue.Visible = false;
                    labelTotalSold.Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateEmptyTable()
        {
            DataTable emptyTable = new DataTable();
            emptyTable.Columns.Add("Блюдо", typeof(string));
            emptyTable.Columns.Add("Категория", typeof(string));
            emptyTable.Columns.Add("Кол-во продаж", typeof(int));
            emptyTable.Columns.Add("Общая выручка", typeof(decimal));
            dataGridViewTopDish.DataSource = emptyTable;
        }

        // ===================== ОБРАБОТЧИК ЗАКРЫТИЯ ФОРМЫ =====================

        /// <summary>
        /// Обработчик закрытия формы - при нажатии на крестик переходим на DirectorForm
        /// </summary>
        private void TopDishForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Проверяем, что закрытие инициировано пользователем (крестик или Alt+F4)
            if (e.CloseReason == CloseReason.UserClosing)
            {
                // Отменяем закрытие формы
                e.Cancel = true;

                // Отписываемся от менеджера бездействия
                InactivityManager.UnregisterForm();

                // Скрываем текущую форму
                this.Visible = false;

                // Открываем форму директора
                DirectorForm director = new DirectorForm();
                director.Show();
            }
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

                System.Windows.Forms.TextBox txtPassword = new System.Windows.Forms.TextBox();
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

        private void CheckPasswordAndUnlock(System.Windows.Forms.TextBox txtPassword, Form lockDialog)
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

        private void SetupButtonStyles()
        {
            // Настройка только кнопки экспорта
            buttonExport.FlatStyle = FlatStyle.Flat;
            buttonExport.FlatAppearance.BorderSize = 1;
            buttonExport.FlatAppearance.BorderColor = Color.Black;
            buttonExport.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            buttonExport.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            buttonExport.MouseDown += (s, e) => buttonExport.FlatAppearance.BorderColor = Color.DarkBlue;
            buttonExport.MouseUp += (s, e) => buttonExport.FlatAppearance.BorderColor = Color.Black;
            buttonExport.MouseLeave += (s, e) => buttonExport.FlatAppearance.BorderColor = Color.Black;
        }

        private void TopDishForm_Load(object sender, EventArgs e)
        {
            SetupDataGridView();
            LoadCategories();
            labelTotalRevenue.Visible = false;
            labelTotalSold.Visible = false;

            // Загружаем данные при загрузке формы
            LoadTopDishesAutomatically();
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

            Color headerBackColor = Color.FromArgb(97, 173, 123);
            Color selectionColor = Color.FromArgb(233, 242, 236);

            dataGridViewTopDish.ColumnHeadersDefaultCellStyle.BackColor = headerBackColor;
            dataGridViewTopDish.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dataGridViewTopDish.ColumnHeadersDefaultCellStyle.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            dataGridViewTopDish.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewTopDish.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridViewTopDish.ColumnHeadersDefaultCellStyle.Padding = new Padding(0, 3, 0, 3);
            dataGridViewTopDish.ColumnHeadersHeight = 45;

            dataGridViewTopDish.DefaultCellStyle.Font = new Font("Times New Roman", 10, FontStyle.Regular);
            dataGridViewTopDish.DefaultCellStyle.Padding = new Padding(5);
            dataGridViewTopDish.DefaultCellStyle.BackColor = Color.White;
            dataGridViewTopDish.DefaultCellStyle.ForeColor = Color.Black;
            dataGridViewTopDish.DefaultCellStyle.SelectionBackColor = selectionColor;
            dataGridViewTopDish.DefaultCellStyle.SelectionForeColor = Color.Black;

            dataGridViewTopDish.RowTemplate.Height = 35;
            dataGridViewTopDish.GridColor = Color.Gray;
            dataGridViewTopDish.BorderStyle = BorderStyle.Fixed3D;

            dataGridViewTopDish.Columns.Clear();

            dataGridViewTopDish.Columns.Add(new DataGridViewTextBoxColumn { Name = "dish_name", HeaderText = "Блюдо", DataPropertyName = "Блюдо", DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft } });
            dataGridViewTopDish.Columns.Add(new DataGridViewTextBoxColumn { Name = "category", HeaderText = "Категория", DataPropertyName = "Категория", DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft } });
            dataGridViewTopDish.Columns.Add(new DataGridViewTextBoxColumn { Name = "quantity", HeaderText = "Кол-во продаж", DataPropertyName = "Кол-во продаж", DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dataGridViewTopDish.Columns.Add(new DataGridViewTextBoxColumn { Name = "revenue", HeaderText = "Общая выручка", DataPropertyName = "Общая выручка", DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Format = "N2", ForeColor = Color.DarkGreen } });
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
                MessageBox.Show($"Ошибка при загрузке категорий: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                WHERE o.id_status IN (4,5,6)";

            if (selectedCategory != 0)
                query += " AND d.id_category = @categoryId";

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
                        cmd.Parameters.AddWithValue("@categoryId", selectedCategory);

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    dishesData.Clear();
                    adapter.Fill(dishesData);
                    dataGridViewTopDish.DataSource = dishesData;
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
                    MessageBox.Show("Нет данных для экспорта!", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Excel файлы (*.xlsx)|*.xlsx";
                saveDialog.FileName = $"Топ_блюд_{DateTime.Now:yyyyMMdd_HHmmss}";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    ExportToExcelWithChart(saveDialog.FileName);

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
                MessageBox.Show($"Ошибка при экспорте в Excel: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToExcelWithChart(string filePath)
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

                string categoryName = comboBoxCategory.Text;

                // ЗАГОЛОВОК (расширен до колонки J)
                Excel.Range titleRange = worksheet.Range["A1:J1"];
                titleRange.Merge();
                titleRange.Value = "ТОП 10 БЛЮД ПО ВЫРУЧКЕ";
                titleRange.Font.Bold = true;
                titleRange.Font.Size = 16;
                titleRange.Font.Name = "Times New Roman";
                titleRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                titleRange.RowHeight = 35;

                // ПЕРИОД
                Excel.Range periodRange = worksheet.Range["A2:J2"];
                periodRange.Merge();
                periodRange.Value = $"Период: {dateTimePickerStart.Value:dd.MM.yyyy} - {dateTimePickerEnd.Value:dd.MM.yyyy} | Категория: {categoryName}";
                periodRange.Font.Bold = true;
                periodRange.Font.Size = 11;
                periodRange.Font.Name = "Times New Roman";
                periodRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                periodRange.RowHeight = 25;

                // ЗАГОЛОВКИ ТАБЛИЦЫ
                int dataStartRow = 4;
                string[] headers = { "№", "Блюдо", "Категория", "Кол-во продаж", "Общая выручка" };

                for (int i = 0; i < headers.Length; i++)
                {
                    Excel.Range cell = worksheet.Cells[dataStartRow, i + 1];
                    cell.Value = headers[i];
                    cell.Font.Bold = true;
                    cell.Font.Size = 12;
                    cell.Font.Name = "Times New Roman";
                    cell.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(97, 173, 123));
                    cell.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    cell.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    cell.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                }

                // ЗАПОЛНЯЕМ ДАННЫЕ
                for (int i = 0; i < dishesData.Rows.Count; i++)
                {
                    int rowNum = dataStartRow + 1 + i;
                    worksheet.Cells[rowNum, 1] = i + 1;
                    worksheet.Cells[rowNum, 2] = dishesData.Rows[i]["Блюдо"].ToString();
                    worksheet.Cells[rowNum, 3] = dishesData.Rows[i]["Категория"].ToString();

                    object quantityObj = dishesData.Rows[i]["Кол-во продаж"];
                    int quantity = (quantityObj != DBNull.Value) ? Convert.ToInt32(quantityObj) : 0;
                    worksheet.Cells[rowNum, 4] = quantity;

                    object revenueObj = dishesData.Rows[i]["Общая выручка"];
                    decimal revenue = (revenueObj != DBNull.Value) ? Convert.ToDecimal(revenueObj) : 0;
                    worksheet.Cells[rowNum, 5] = revenue;

                    for (int j = 1; j <= 5; j++)
                    {
                        Excel.Range cell = worksheet.Cells[rowNum, j];
                        cell.Font.Name = "Times New Roman";
                        cell.Font.Size = 10;
                        cell.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                        cell.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;

                        if (j == 4) cell.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                        else if (j == 5)
                        {
                            cell.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                            cell.NumberFormat = "#,##0.00 ₽";
                            cell.Font.Color = System.Drawing.ColorTranslator.ToOle(Color.DarkGreen);
                        }
                        else cell.HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
                    }
                }

                // ИТОГОВАЯ СТРОКА
                int totalRow = dataStartRow + dishesData.Rows.Count + 1;
                int totalSold = 0;
                decimal totalRevenue = 0;
                foreach (DataRow row in dishesData.Rows)
                {
                    object quantityObj = row["Кол-во продаж"];
                    totalSold += (quantityObj != DBNull.Value) ? Convert.ToInt32(quantityObj) : 0;

                    object revenueObj = row["Общая выручка"];
                    totalRevenue += (revenueObj != DBNull.Value) ? Convert.ToDecimal(revenueObj) : 0;
                }

                worksheet.Cells[totalRow, 3] = "ИТОГО:";
                worksheet.Cells[totalRow, 3].Font.Bold = true;
                ((Excel.Range)worksheet.Cells[totalRow, 3]).HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;

                worksheet.Cells[totalRow, 4] = totalSold;
                worksheet.Cells[totalRow, 4].Font.Bold = true;
                ((Excel.Range)worksheet.Cells[totalRow, 4]).HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                worksheet.Cells[totalRow, 5] = totalRevenue;
                worksheet.Cells[totalRow, 5].Font.Bold = true;
                ((Excel.Range)worksheet.Cells[totalRow, 5]).NumberFormat = "#,##0.00 ₽";
                ((Excel.Range)worksheet.Cells[totalRow, 5]).HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                worksheet.Cells[totalRow, 5].Font.Color = System.Drawing.ColorTranslator.ToOle(Color.DarkGreen);

                // НАСТРОЙКА ШИРИНЫ КОЛОНОК
                worksheet.Columns[1].ColumnWidth = 5;   // №
                worksheet.Columns[2].ColumnWidth = 35;  // Блюдо
                worksheet.Columns[3].ColumnWidth = 20;  // Категория
                worksheet.Columns[4].ColumnWidth = 15;  // Кол-во продаж
                worksheet.Columns[5].ColumnWidth = 20;  // Общая выручка
                worksheet.Columns[6].ColumnWidth = 5;   // Отступ
                worksheet.Columns[7].ColumnWidth = 5;   // Отступ
                worksheet.Columns[8].ColumnWidth = 5;   // Отступ
                worksheet.Columns[9].ColumnWidth = 5;   // Отступ
                worksheet.Columns[10].ColumnWidth = 5;  // Отступ

                // ============= СОЗДАЕМ ДИАГРАММУ =============
                if (dishesData.Rows.Count > 0)
                {
                    int firstDataRow = dataStartRow + 1;
                    int lastDataRow = totalRow - 1;

                    // Данные для диаграммы
                    Excel.Range xValues = worksheet.Range[$"B{firstDataRow}:B{lastDataRow}"];
                    Excel.Range yValues = worksheet.Range[$"E{firstDataRow}:E{lastDataRow}"];

                    // Диаграмма в правой части
                    Excel.ChartObjects chartObjects = (Excel.ChartObjects)worksheet.ChartObjects();
                    Excel.ChartObject chartObject = chartObjects.Add(800, 60, 550, 350);
                    Excel.Chart chart = chartObject.Chart;

                    chart.ChartType = Excel.XlChartType.xlColumnClustered;

                    chart.HasTitle = true;
                    chart.ChartTitle.Text = "Топ блюд по выручке";
                    chart.ChartTitle.Font.Name = "Times New Roman";
                    chart.ChartTitle.Font.Size = 14;
                    chart.ChartTitle.Font.Bold = true;

                    chart.HasLegend = true;
                    chart.Legend.Position = Excel.XlLegendPosition.xlLegendPositionTop;

                    // Подписи осей
                    chart.Axes(Excel.XlAxisType.xlCategory, Excel.XlAxisGroup.xlPrimary).HasTitle = true;
                    chart.Axes(Excel.XlAxisType.xlCategory, Excel.XlAxisGroup.xlPrimary).AxisTitle.Text = "Блюда";
                    chart.Axes(Excel.XlAxisType.xlCategory, Excel.XlAxisGroup.xlPrimary).AxisTitle.Font.Name = "Times New Roman";

                    chart.Axes(Excel.XlAxisType.xlValue, Excel.XlAxisGroup.xlPrimary).HasTitle = true;
                    chart.Axes(Excel.XlAxisType.xlValue, Excel.XlAxisGroup.xlPrimary).AxisTitle.Text = "Выручка (₽)";
                    chart.Axes(Excel.XlAxisType.xlValue, Excel.XlAxisGroup.xlPrimary).AxisTitle.Font.Name = "Times New Roman";

                    // Добавляем данные
                    Excel.SeriesCollection seriesCollection = (Excel.SeriesCollection)chart.SeriesCollection();
                    Excel.Series series = seriesCollection.NewSeries();

                    series.Name = "Выручка";
                    series.XValues = xValues;
                    series.Values = yValues;
                    series.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(97, 173, 123));

                    // Подписи на столбцах
                    series.HasDataLabels = true;
                    Excel.DataLabels dataLabels = series.DataLabels();
                    if (dataLabels != null)
                    {
                        dataLabels.NumberFormat = "#,##0.00 ₽";
                        dataLabels.Position = Excel.XlDataLabelPosition.xlLabelPositionOutsideEnd;
                        dataLabels.Font.Size = 8;
                        dataLabels.Font.Name = "Times New Roman";
                    }

                    // Оформление
                    chart.ChartArea.Font.Name = "Times New Roman";
                    chart.PlotArea.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.WhiteSmoke);

                    // Сетка
                    Excel.Axis axisY = (Excel.Axis)chart.Axes(Excel.XlAxisType.xlValue, Excel.XlAxisGroup.xlPrimary);
                    axisY.HasMajorGridlines = true;
                    axisY.MajorGridlines.Border.Color = System.Drawing.ColorTranslator.ToOle(Color.LightGray);
                }

                // НАСТРОЙКА СТРАНИЦЫ
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

        private void pictureBoxBack_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            if (this.Owner != null && !this.Owner.IsDisposed)
                this.Owner.Show();
        }

        private void buttonExportPdf_Click(object sender, EventArgs e)
        {

        }
    }
}