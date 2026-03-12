namespace dump
{
    partial class DirectorForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DirectorForm));
            this.label2 = new System.Windows.Forms.Label();
            this.buttonStatistics = new System.Windows.Forms.Button();
            this.panelStatistics = new System.Windows.Forms.Panel();
            this.buttonTopDish = new System.Windows.Forms.Button();
            this.buttonClientTop = new System.Windows.Forms.Button();
            this.buttonCertificates = new System.Windows.Forms.Button();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.ButtonReport = new System.Windows.Forms.Button();
            this.buttonProfit = new System.Windows.Forms.Button();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.buttonMenu = new System.Windows.Forms.Button();
            this.panelStatistics.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(251, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(301, 31);
            this.label2.TabIndex = 13;
            this.label2.Text = "Главное меню директора";
            // 
            // buttonStatistics
            // 
            this.buttonStatistics.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.buttonStatistics.Location = new System.Drawing.Point(42, 176);
            this.buttonStatistics.Name = "buttonStatistics";
            this.buttonStatistics.Size = new System.Drawing.Size(298, 87);
            this.buttonStatistics.TabIndex = 9;
            this.buttonStatistics.Text = "Статистика";
            this.buttonStatistics.UseVisualStyleBackColor = false;
            // 
            // panelStatistics
            // 
            this.panelStatistics.Controls.Add(this.buttonTopDish);
            this.panelStatistics.Controls.Add(this.buttonClientTop);
            this.panelStatistics.Controls.Add(this.buttonCertificates);
            this.panelStatistics.Controls.Add(this.pictureBox3);
            this.panelStatistics.Location = new System.Drawing.Point(-5, 2);
            this.panelStatistics.Name = "panelStatistics";
            this.panelStatistics.Size = new System.Drawing.Size(872, 640);
            this.panelStatistics.TabIndex = 16;
            // 
            // buttonTopDish
            // 
            this.buttonTopDish.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.buttonTopDish.Location = new System.Drawing.Point(290, 359);
            this.buttonTopDish.Name = "buttonTopDish";
            this.buttonTopDish.Size = new System.Drawing.Size(298, 87);
            this.buttonTopDish.TabIndex = 20;
            this.buttonTopDish.Text = "Топ 10 блюд по выручке";
            this.buttonTopDish.UseVisualStyleBackColor = false;
            this.buttonTopDish.Click += new System.EventHandler(this.buttonTopDish_Click);
            // 
            // buttonClientTop
            // 
            this.buttonClientTop.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.buttonClientTop.Location = new System.Drawing.Point(290, 266);
            this.buttonClientTop.Name = "buttonClientTop";
            this.buttonClientTop.Size = new System.Drawing.Size(298, 87);
            this.buttonClientTop.TabIndex = 19;
            this.buttonClientTop.Text = "Топ 10 клиентов по сумме заказа";
            this.buttonClientTop.UseVisualStyleBackColor = false;
            this.buttonClientTop.Click += new System.EventHandler(this.buttonClientTop_Click);
            // 
            // buttonCertificates
            // 
            this.buttonCertificates.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.buttonCertificates.Location = new System.Drawing.Point(290, 173);
            this.buttonCertificates.Name = "buttonCertificates";
            this.buttonCertificates.Size = new System.Drawing.Size(298, 87);
            this.buttonCertificates.TabIndex = 18;
            this.buttonCertificates.Text = "Статистика по сертификатам";
            this.buttonCertificates.UseVisualStyleBackColor = false;
            this.buttonCertificates.Click += new System.EventHandler(this.buttonCertificates_Click);
            // 
            // pictureBox3
            // 
            this.pictureBox3.Image = global::dump.Properties.Resources.remove;
            this.pictureBox3.Location = new System.Drawing.Point(828, 3);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(41, 33);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox3.TabIndex = 17;
            this.pictureBox3.TabStop = false;
            this.pictureBox3.Click += new System.EventHandler(this.pictureBox3_Click);
            // 
            // ButtonReport
            // 
            this.ButtonReport.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.ButtonReport.Location = new System.Drawing.Point(42, 269);
            this.ButtonReport.Name = "ButtonReport";
            this.ButtonReport.Size = new System.Drawing.Size(298, 87);
            this.ButtonReport.TabIndex = 21;
            this.ButtonReport.Text = "Отчёт по заказам за период";
            this.ButtonReport.UseVisualStyleBackColor = false;
            this.ButtonReport.Click += new System.EventHandler(this.ButtonReport_Click);
            // 
            // buttonProfit
            // 
            this.buttonProfit.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.buttonProfit.Location = new System.Drawing.Point(42, 362);
            this.buttonProfit.Name = "buttonProfit";
            this.buttonProfit.Size = new System.Drawing.Size(298, 87);
            this.buttonProfit.TabIndex = 22;
            this.buttonProfit.Text = "Отчёт по прибыли за период";
            this.buttonProfit.UseVisualStyleBackColor = false;
            this.buttonProfit.Click += new System.EventHandler(this.button1_Click);
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::dump.Properties.Resources.remove;
            this.pictureBox2.Location = new System.Drawing.Point(826, 2);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(41, 33);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 14;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Click += new System.EventHandler(this.pictureBox2_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::dump.Properties.Resources.admin1;
            this.pictureBox1.Location = new System.Drawing.Point(447, 147);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(357, 318);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 12;
            this.pictureBox1.TabStop = false;
            // 
            // buttonMenu
            // 
            this.buttonMenu.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.buttonMenu.Location = new System.Drawing.Point(42, 455);
            this.buttonMenu.Name = "buttonMenu";
            this.buttonMenu.Size = new System.Drawing.Size(298, 87);
            this.buttonMenu.TabIndex = 23;
            this.buttonMenu.Text = "Меню блюд";
            this.buttonMenu.UseVisualStyleBackColor = false;
            this.buttonMenu.Click += new System.EventHandler(this.buttonMenu_Click);
            // 
            // DirectorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(15F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(872, 644);
            this.ControlBox = false;
            this.Controls.Add(this.buttonMenu);
            this.Controls.Add(this.buttonProfit);
            this.Controls.Add(this.ButtonReport);
            this.Controls.Add(this.panelStatistics);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.buttonStatistics);
            this.Font = new System.Drawing.Font("Times New Roman", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(872, 644);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(872, 644);
            this.Name = "DirectorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = " ";
            this.Load += new System.EventHandler(this.DirectorForm_Load);
            this.panelStatistics.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button buttonStatistics;
        private System.Windows.Forms.Panel panelStatistics;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.Button buttonCertificates;
        private System.Windows.Forms.Button buttonClientTop;
        private System.Windows.Forms.Button buttonTopDish;
        private System.Windows.Forms.Button ButtonReport;
        private System.Windows.Forms.Button buttonProfit;
        private System.Windows.Forms.Button buttonMenu;
    }
}