using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.IO;

namespace dump
{
    /// <summary>
    /// Форма статистики по сертификатам для директора.
    /// Предоставляет информацию о количестве, суммах и статусах сертификатов за выбранный период.
    /// </summary>
    public partial class CertificateStatisticsForm : Form
    {
        private DataTable certificatesStats;
        private DateTime minDate = new DateTime(2024, 1, 1);
        private System.Windows.Forms.ToolTip toolTip1;

        /// <summary>
        /// Конструктор формы статистики сертификатов.
        /// Инициализирует компоненты и настраивает внешний вид.
        /// </summary>
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

            // Шрифты для DateTimePicker
            datePickerStart.Font = new Font("Times New Roman", 14);
            datePickerEnd.Font = new Font("Times New Roman", 14);

            btnExport.Click += BtnExport_Click;

            SetupDataGridView();
            CreateEmptyTable();
            SetupButtons();

            datePickerStart.ValueChanged += DatePicker_ValueChanged;
            datePickerEnd.ValueChanged += DatePicker_ValueChanged;

            this.Load += CertificateStatisticsForm_Load;
            this.FormClosing += CertificateStatisticsForm_FormClosing;
        }

        /// <summary>
        /// Обработчик изменения даты в календарях.
        /// Обновляет статистику.
        /// </summary>
        private void DatePicker_ValueChanged(object sender, EventArgs e)
        {
            LoadStatistics();
        }

        private void CertificateStatisticsForm_Load(object sender, EventArgs e)
        {
            LoadStatistics();
        }

        /// <summary>
        /// Загружает статистику по сертификатам за выбранный период.
        /// </summary>
        private void LoadStatistics()
        {
            try
            {
                DateTime startDate = datePickerStart.Value.Date;
                DateTime endDate = datePickerEnd.Value.Date;

                if (startDate > endDate)
                {
                    CreateEmptyTable();
                    return;
                }

                LoadCertificateStatistics(startDate, endDate);

                if (certificatesStats.Rows.Count == 0)
                {
                    CreateEmptyTable();
                }
                else
                {
                    dgvCertificates.DataSource = certificatesStats;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Обработчик события закрытия формы.
        /// При закрытии формы пользователем скрывает её и открывает форму директора.
        /// </summary>
        private void CertificateStatisticsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Visible = false;
                DirectorForm director = new DirectorForm();
                director.Show();
            }
        }

        /// <summary>
        /// Инициализирует дополнительные пользовательские компоненты формы.
        /// </summary>
        private void InitializeCustomComponents()
        {
            if (datePickerStart == null)
            {
                datePickerStart = new DateTimePicker();
                datePickerStart.Location = new Point(150, 20);
                datePickerStart.Size = new Size(150, 22);
                datePickerStart.Format = DateTimePickerFormat.Short;
                datePickerStart.Font = new Font("Times New Roman", 14);
                this.Controls.Add(datePickerStart);
            }

            if (datePickerEnd == null)
            {
                datePickerEnd = new DateTimePicker();
                datePickerEnd.Location = new Point(350, 20);
                datePickerEnd.Size = new Size(150, 22);
                datePickerEnd.Format = DateTimePickerFormat.Short;
                datePickerEnd.Font = new Font("Times New Roman", 14);
                this.Controls.Add(datePickerEnd);
            }

            if (labelStart == null)
            {
                labelStart = new Label();
                labelStart.Text = "Начало периода:";
                labelStart.Location = new Point(40, 22);
                labelStart.Size = new Size(100, 20);
                labelStart.Font = new Font("Times New Roman", 14);
                this.Controls.Add(labelStart);
            }

            if (labelEnd == null)
            {
                labelEnd = new Label();
                labelEnd.Text = "Конец периода:";
                labelEnd.Location = new Point(250, 22);
                labelEnd.Size = new Size(100, 20);
                labelEnd.Font = new Font("Times New Roman", 14);
                this.Controls.Add(labelEnd);
            }
        }

        /// <summary>
        /// Настраивает стиль кнопки экспорта.
        /// </summary>
        private void SetupButtons()
        {
            btnExport.FlatStyle = FlatStyle.Flat;
            btnExport.FlatAppearance.BorderSize = 1;
            btnExport.FlatAppearance.BorderColor = Color.Black;
            btnExport.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            btnExport.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;
            btnExport.Font = new Font("Times New Roman", 14, FontStyle.Regular);

            btnExport.MouseDown += (s, e) => btnExport.FlatAppearance.BorderColor = Color.DarkBlue;
            btnExport.MouseUp += (s, e) => btnExport.FlatAppearance.BorderColor = Color.Black;
            btnExport.MouseLeave += (s, e) => btnExport.FlatAppearance.BorderColor = Color.Black;
        }

        /// <summary>
        /// Создаёт пустую таблицу для отображения, когда нет данных.
        /// </summary>
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

        /// <summary>
        /// Настраивает внешний вид DataGridView для отображения статистики.
        /// </summary>
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

            // ===== ЗЕЛЕНАЯ ШАПКА - TIMES NEW ROMAN 14PT BOLD =====
            dgvCertificates.ColumnHeadersDefaultCellStyle.BackColor = headerBackColor;
            dgvCertificates.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvCertificates.ColumnHeadersDefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Bold);
            dgvCertificates.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCertificates.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvCertificates.ColumnHeadersDefaultCellStyle.Padding = new Padding(0, 3, 0, 3);
            dgvCertificates.ColumnHeadersHeight = 50;

            // ===== ЯЧЕЙКИ - TIMES NEW ROMAN 14PT REGULAR =====
            dgvCertificates.DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Regular);
            dgvCertificates.DefaultCellStyle.Padding = new Padding(5);
            dgvCertificates.DefaultCellStyle.BackColor = Color.White;
            dgvCertificates.DefaultCellStyle.ForeColor = Color.Black;
            dgvCertificates.DefaultCellStyle.SelectionBackColor = selectionColor;
            dgvCertificates.DefaultCellStyle.SelectionForeColor = Color.Black;

            dgvCertificates.RowsDefaultCellStyle.BackColor = Color.White;
            dgvCertificates.RowsDefaultCellStyle.ForeColor = Color.Black;
            dgvCertificates.RowsDefaultCellStyle.SelectionBackColor = selectionColor;
            dgvCertificates.RowsDefaultCellStyle.SelectionForeColor = Color.Black;

            dgvCertificates.RowTemplate.Height = 40;
            dgvCertificates.GridColor = Color.Gray;
            dgvCertificates.BorderStyle = BorderStyle.Fixed3D;

            toolTip1.SetToolTip(dgvCertificates, "Статистика по сертификатам");

            dgvCertificates.Columns.Clear();

            // Колонка Статус
            DataGridViewTextBoxColumn colStatus = new DataGridViewTextBoxColumn();
            colStatus.Name = "Статус";
            colStatus.HeaderText = "Статус";
            colStatus.DataPropertyName = "Статус";
            colStatus.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colStatus.DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Regular);
            dgvCertificates.Columns.Add(colStatus);

            // Колонка Количество
            DataGridViewTextBoxColumn colCount = new DataGridViewTextBoxColumn();
            colCount.Name = "Количество";
            colCount.HeaderText = "Количество";
            colCount.DataPropertyName = "Количество";
            colCount.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colCount.DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Regular);
            dgvCertificates.Columns.Add(colCount);

            // Колонка Общая сумма
            DataGridViewTextBoxColumn colTotal = new DataGridViewTextBoxColumn();
            colTotal.Name = "Общая сумма";
            colTotal.HeaderText = "Общая сумма";
            colTotal.DataPropertyName = "Общая сумма";
            colTotal.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colTotal.DefaultCellStyle.Format = "N2";
            colTotal.DefaultCellStyle.ForeColor = Color.DarkGreen;
            colTotal.DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Bold);
            dgvCertificates.Columns.Add(colTotal);

            // Колонка Средняя сумма
            DataGridViewTextBoxColumn colAvg = new DataGridViewTextBoxColumn();
            colAvg.Name = "Средняя сумма";
            colAvg.HeaderText = "Средняя сумма";
            colAvg.DataPropertyName = "Средняя сумма";
            colAvg.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colAvg.DefaultCellStyle.Format = "N2";
            colAvg.DefaultCellStyle.ForeColor = Color.DarkGreen;
            colAvg.DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Regular);
            dgvCertificates.Columns.Add(colAvg);

            // Колонка Мин. сумма
            DataGridViewTextBoxColumn colMin = new DataGridViewTextBoxColumn();
            colMin.Name = "Мин. сумма";
            colMin.HeaderText = "Мин. сумма";
            colMin.DataPropertyName = "Мин. сумма";
            colMin.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colMin.DefaultCellStyle.Format = "N2";
            colMin.DefaultCellStyle.ForeColor = Color.DarkGreen;
            colMin.DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Regular);
            dgvCertificates.Columns.Add(colMin);

            // Колонка Макс. сумма
            DataGridViewTextBoxColumn colMax = new DataGridViewTextBoxColumn();
            colMax.Name = "Макс. сумма";
            colMax.HeaderText = "Макс. сумма";
            colMax.DataPropertyName = "Макс. сумма";
            colMax.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colMax.DefaultCellStyle.Format = "N2";
            colMax.DefaultCellStyle.ForeColor = Color.DarkGreen;
            colMax.DefaultCellStyle.Font = new Font("Times New Roman", 14, FontStyle.Regular);
            dgvCertificates.Columns.Add(colMax);
        }

        /// <summary>
        /// Загружает статистику сертификатов из базы данных за указанный период.
        /// </summary>
        /// <param name="startDate">Дата начала периода.</param>
        /// <param name="endDate">Дата окончания периода.</param>
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

        // ===================== ЭКСПОРТ В PDF =====================

        /// <summary>
        /// Обработчик нажатия кнопки экспорта.
        /// Экспортирует статистику в PDF-файл.
        /// </summary>
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
                saveDialog.Filter = "PDF файлы (*.pdf)|*.pdf";
                saveDialog.FileName = $"Статистика_сертификатов_{DateTime.Now:yyyyMMdd_HHmmss}";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    ExportToPdf(saveDialog.FileName);

                    DialogResult result = MessageBox.Show($"✅ PDF файл сохранен!\n{saveDialog.FileName}\n\nОткрыть файл?",
                        "Готово", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        Process.Start(saveDialog.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Экспортирует данные в PDF-файл с использованием печати.
        /// </summary>
        /// <param name="filePath">Путь к сохраняемому файлу.</param>
        private void ExportToPdf(string filePath)
        {
            PrintDocument printDoc = new PrintDocument();
            printDoc.PrinterSettings.PrinterName = "Microsoft Print to PDF";
            printDoc.PrinterSettings.PrintFileName = filePath;
            printDoc.PrinterSettings.PrintToFile = true;
            printDoc.DocumentName = Path.GetFileName(filePath);
            printDoc.PrintController = new StandardPrintController();

            printDoc.PrintPage += (s, ev) =>
            {
                Graphics g = ev.Graphics;
                Font titleFont = new Font("Times New Roman", 18, FontStyle.Bold);
                Font headerFont = new Font("Times New Roman", 13, FontStyle.Bold);
                Font regularFont = new Font("Times New Roman", 11, FontStyle.Regular);
                Font boldFont = new Font("Times New Roman", 11, FontStyle.Bold);
                Font smallFont = new Font("Times New Roman", 9, FontStyle.Regular);

                float y = 30;
                float leftMargin = 30;
                float pageWidth = ev.PageBounds.Width - 60;

                // ЗАГОЛОВОК
                SizeF titleSize = g.MeasureString("СТАТИСТИКА ПО СЕРТИФИКАТАМ", titleFont);
                g.DrawString("СТАТИСТИКА ПО СЕРТИФИКАТАМ", titleFont, Brushes.Black,
                    new PointF(leftMargin + (pageWidth - titleSize.Width) / 2, y));
                y += 35;

                // ПЕРИОД
                string periodText = $"Период: {datePickerStart.Value:dd.MM.yyyy} - {datePickerEnd.Value:dd.MM.yyyy}";
                SizeF periodSize = g.MeasureString(periodText, smallFont);
                g.DrawString(periodText, smallFont, Brushes.DarkBlue,
                    new PointF(leftMargin + (pageWidth - periodSize.Width) / 2, y));
                y += 30;

                // ЛИНИЯ
                g.DrawLine(new Pen(Color.LightGray, 1), leftMargin, y, leftMargin + pageWidth, y);
                y += 15;

                // СВОДНАЯ ИНФОРМАЦИЯ
                g.DrawString("СВОДНАЯ ИНФОРМАЦИЯ:", headerFont, Brushes.Black,
                    new PointF(leftMargin, y));
                y += 25;

                // Рассчитываем итоги
                int totalIssued = 0, totalUsed = 0, totalReturned = 0, totalActive = 0;
                decimal totalIssuedSum = 0, totalUsedSum = 0, totalReturnedSum = 0;

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

                // Сводная информация
                string[] summaryLines = {
                    $"Всего выпущено сертификатов: {totalIssued,4} шт. ({totalIssuedSum,12:N2} ₽)",
                    $"Из них использовано:        {totalUsed,4} шт. ({totalUsedSum,12:N2} ₽)",
                    $"Активных (неиспользованных): {totalActive,4} шт. ({(totalIssuedSum - totalUsedSum),12:N2} ₽)",
                    $"Возвращено сертификатов:    {totalReturned,4} шт. ({totalReturnedSum,12:N2} ₽)"
                };

                foreach (string line in summaryLines)
                {
                    g.DrawString(line, regularFont, Brushes.Black,
                        new PointF(leftMargin + 20, y));
                    y += 22;
                }

                y += 20;

                // ЛИНИЯ
                g.DrawLine(new Pen(Color.LightGray, 1), leftMargin, y, leftMargin + pageWidth, y);
                y += 15;

                // СТАТИСТИКА ПО СТАТУСАМ
                g.DrawString("СТАТИСТИКА ПО СТАТУСАМ:", headerFont, Brushes.Black,
                    new PointF(leftMargin, y));
                y += 28;

                // Ширины колонок
                float[] colWidths = { 120, 70, 110, 110, 90, 100 };
                float x = leftMargin;
                string[] headers = { "Статус", "Кол-во", "Общая сумма", "Средняя сумма", "Мин. сумма", "Макс. сумма" };

                // Заголовки таблицы
                for (int i = 0; i < headers.Length; i++)
                {
                    Rectangle rect = new Rectangle((int)x, (int)y, (int)colWidths[i], 32);
                    g.FillRectangle(new SolidBrush(Color.FromArgb(97, 173, 123)), rect);
                    g.DrawRectangle(Pens.Black, rect);

                    StringFormat sf = new StringFormat();
                    if (i == 0) sf.Alignment = StringAlignment.Near;
                    else sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;

                    g.DrawString(headers[i], headerFont, Brushes.Black, rect, sf);
                    x += colWidths[i];
                }
                y += 32;

                // Данные
                foreach (DataRow row in certificatesStats.Rows)
                {
                    x = leftMargin;

                    // Статус
                    Rectangle statusRect = new Rectangle((int)x, (int)y, (int)colWidths[0], 24);
                    g.DrawRectangle(Pens.Black, statusRect);
                    g.DrawString(row["Статус"].ToString(), regularFont, Brushes.Black, statusRect,
                        new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });
                    x += colWidths[0];

                    // Количество
                    Rectangle countRect = new Rectangle((int)x, (int)y, (int)colWidths[1], 24);
                    g.DrawRectangle(Pens.Black, countRect);
                    g.DrawString(row["Количество"].ToString(), regularFont, Brushes.Black, countRect,
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                    x += colWidths[1];

                    // Общая сумма
                    decimal totalSum = Convert.ToDecimal(row["Общая сумма"]);
                    Rectangle totalRect = new Rectangle((int)x, (int)y, (int)colWidths[2], 24);
                    g.DrawRectangle(Pens.Black, totalRect);
                    g.DrawString($"{totalSum:N2} ₽", regularFont, Brushes.DarkGreen, totalRect,
                        new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center });
                    x += colWidths[2];

                    // Средняя сумма
                    decimal avgSum = Convert.ToDecimal(row["Средняя сумма"]);
                    Rectangle avgRect = new Rectangle((int)x, (int)y, (int)colWidths[3], 24);
                    g.DrawRectangle(Pens.Black, avgRect);
                    g.DrawString($"{avgSum:N2} ₽", regularFont, Brushes.DarkGreen, avgRect,
                        new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center });
                    x += colWidths[3];

                    // Мин. сумма
                    decimal minSum = Convert.ToDecimal(row["Мин. сумма"]);
                    Rectangle minRect = new Rectangle((int)x, (int)y, (int)colWidths[4], 24);
                    g.DrawRectangle(Pens.Black, minRect);
                    g.DrawString($"{minSum:N2} ₽", regularFont, Brushes.DarkGreen, minRect,
                        new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center });
                    x += colWidths[4];

                    // Макс. сумма
                    decimal maxSum = Convert.ToDecimal(row["Макс. сумма"]);
                    Rectangle maxRect = new Rectangle((int)x, (int)y, (int)colWidths[5], 24);
                    g.DrawRectangle(Pens.Black, maxRect);
                    g.DrawString($"{maxSum:N2} ₽", regularFont, Brushes.DarkGreen, maxRect,
                        new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center });

                    y += 24;
                }

                // ИТОГО
                Rectangle totalLabelRect = new Rectangle((int)leftMargin, (int)y, (int)colWidths[0], 24);
                g.FillRectangle(new SolidBrush(Color.FromArgb(230, 255, 230)), totalLabelRect);
                g.DrawRectangle(Pens.Black, totalLabelRect);
                g.DrawString("ИТОГО:", boldFont, Brushes.Black, totalLabelRect,
                    new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });

                Rectangle totalCountRect = new Rectangle((int)(leftMargin + colWidths[0]), (int)y, (int)colWidths[1], 24);
                g.FillRectangle(new SolidBrush(Color.FromArgb(230, 255, 230)), totalCountRect);
                g.DrawRectangle(Pens.Black, totalCountRect);
                g.DrawString(totalIssued.ToString(), boldFont, Brushes.Black, totalCountRect,
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

                Rectangle totalSumRect = new Rectangle((int)(leftMargin + colWidths[0] + colWidths[1]), (int)y, (int)colWidths[2], 24);
                g.FillRectangle(new SolidBrush(Color.FromArgb(230, 255, 230)), totalSumRect);
                g.DrawRectangle(Pens.Black, totalSumRect);
                g.DrawString($"{totalIssuedSum:N2} ₽", boldFont, Brushes.DarkGreen, totalSumRect,
                    new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center });

                // Пустые ячейки
                for (int i = 3; i < 6; i++)
                {
                    Rectangle emptyRect = new Rectangle((int)(leftMargin + colWidths.Take(i).Sum()), (int)y, (int)colWidths[i], 24);
                    g.FillRectangle(new SolidBrush(Color.FromArgb(230, 255, 230)), emptyRect);
                    g.DrawRectangle(Pens.Black, emptyRect);
                }

                y += 30;

                // ДАТА
                g.DrawString($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm:ss}", smallFont, Brushes.Gray,
                    new PointF(leftMargin + (pageWidth - g.MeasureString($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm:ss}", smallFont).Width) / 2, y));

                ev.HasMorePages = false;
            };

            printDoc.Print();
        }

        /// <summary>
        /// Обработчик нажатия кнопки выхода (крестик).
        /// Скрывает текущую форму и открывает форму директора.
        /// </summary>
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            DirectorForm director = new DirectorForm();
            director.Show();
        }
    }
}