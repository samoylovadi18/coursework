using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dump
{
    public partial class DirectorForm : Form
    {
        public DirectorForm()
        {
            InitializeComponent();

            // Подписываемся на события ТОЛЬКО для кнопок, которые НА ПАНЕЛИ
            buttonStatistics.Click += ButtonStatistics_Click;

            // Подписываемся на события для кнопок статистики

            // При загрузке формы проверяем, что панель статистики скрыта
            this.Load += DirectorForm_Load;

            // Настройка стилей для кнопок
            SetupButtonStyles();
            buttonMenu.FlatStyle = FlatStyle.Flat;
            buttonMenu.FlatAppearance.BorderSize = 1;
            buttonMenu.FlatAppearance.BorderColor = Color.Black;
            buttonMenu.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            buttonMenu.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            buttonMenu.MouseDown += (s, e) => buttonMenu.FlatAppearance.BorderColor = Color.DarkBlue;
            buttonMenu.MouseUp += (s, e) => buttonMenu.FlatAppearance.BorderColor = Color.Black;
            buttonMenu.MouseLeave += (s, e) => buttonMenu.FlatAppearance.BorderColor = Color.Black;
        }

        private void SetupButtonStyles()
        {
            SetupPanelButtonStyle(buttonCertificates);
            SetupPanelButtonStyle(buttonClientTop);
            SetupPanelButtonStyle(buttonTopDish);
            SetupPanelButtonStyle(buttonStatistics);
            SetupPanelButtonStyle(buttonProfit);
            SetupPanelButtonStyle(ButtonReport);
            
        }

        private void SetupPanelButtonStyle(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.Black;
            btn.FlatAppearance.MouseOverBackColor = Color.DarkSeaGreen;
            btn.FlatAppearance.MouseDownBackColor = Color.DarkSeaGreen;

            btn.MouseDown += (s, e) => btn.FlatAppearance.BorderColor = Color.DarkBlue;
            btn.MouseUp += (s, e) => btn.FlatAppearance.BorderColor = Color.Black;
            btn.MouseLeave += (s, e) => btn.FlatAppearance.BorderColor = Color.Black;
        }

        private void DirectorForm_Load(object sender, EventArgs e)
        {
            // Находим панель статистики
            Panel statisticsPanel = this.Controls["panelStatistics"] as Panel;
            if (statisticsPanel != null)
            {
                // Изначально панель скрыта
                statisticsPanel.Visible = false;

                // Убеждаемся, что на панели ТОЛЬКО нужные кнопки
                // (buttonCertificates, buttonClientTop, buttonTopDish)
                // ButtonRev там быть НЕ ДОЛЖНО
            }
        }

        // Обработчик нажатия на кнопку Statistics
        private void ButtonStatistics_Click(object sender, EventArgs e)
        {
            Panel statisticsPanel = this.Controls["panelStatistics"] as Panel;
            if (statisticsPanel == null) return;

            // Показываем панель статистики
            statisticsPanel.Visible = true;
            statisticsPanel.BringToFront();

            // Показываем кнопки на панели
            buttonCertificates.Visible = true;
            buttonClientTop.Visible = true;
            buttonTopDish.Visible = true;

            // ButtonRev НЕ ТРОГАЕМ - она на форме и всегда видна
        }

        // Закрытие панели через pictureBox4
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            CloseStatisticsPanel();
        }

        // Закрытие панели через pictureBox3
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            CloseStatisticsPanel();
        }

        private void CloseStatisticsPanel()
        {
            Panel statisticsPanel = this.Controls["panelStatistics"] as Panel;
            if (statisticsPanel != null)
            {
                statisticsPanel.Visible = false;
            }

            // Скрываем кнопки на панели
            buttonCertificates.Visible = false;
            buttonClientTop.Visible = false;
            buttonTopDish.Visible = false;

            // ButtonRev НЕ ТРОГАЕМ
        }

        // Выход
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            LoginForm login = new LoginForm();
            login.Show();
        }

        // Обработчики для кнопок статистики
        private void buttonCertificates_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            CertificateStatisticsForm certificate = new CertificateStatisticsForm();
            certificate.Show();
        }

        private void buttonClientTop_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            TopClientsForm topClients = new TopClientsForm();
            topClients.Show();
        }

        private void buttonTopDish_Click(object sender, EventArgs e)
        {
            TopDishForm topDish = new TopDishForm();
            topDish.Owner = this; // Устанавливаем владельца
            this.Hide(); // Прячем DirectorForm
            topDish.Show();
        }

        private void ButtonReport_Click(object sender, EventArgs e)
        {
            OrdersReportForm ordersReport = new OrdersReportForm();
            ordersReport.Owner = this; // Устанавливаем владельца
            this.Hide(); // Прячем DirectorForm
            ordersReport.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ProfitForm profit = new ProfitForm();
            profit.Owner = this;
            this.Hide();
            profit.Show();
        }

        private void buttonMenu_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            AdminMenu directorMenu = new AdminMenu("director");
            directorMenu.Show();
        }
    }
}