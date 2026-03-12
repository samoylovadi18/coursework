namespace dump
{
    partial class TopClientsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TopClientsForm));
            this.dgvTopClients = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.dtpStartDate = new System.Windows.Forms.DateTimePicker();
            this.lblEndDate = new System.Windows.Forms.Label();
            this.dtpEndDate = new System.Windows.Forms.DateTimePicker();
            this.lblStatuses = new System.Windows.Forms.Label();
            this.cmbStatus = new System.Windows.Forms.ComboBox();
            this.lblRecordsCount = new System.Windows.Forms.Label();
            this.lblTotalSum = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTopClients)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvTopClients
            // 
            this.dgvTopClients.AllowUserToAddRows = false;
            this.dgvTopClients.AllowUserToDeleteRows = false;
            this.dgvTopClients.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvTopClients.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dgvTopClients.BackgroundColor = System.Drawing.Color.White;
            this.dgvTopClients.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTopClients.Location = new System.Drawing.Point(46, 185);
            this.dgvTopClients.Margin = new System.Windows.Forms.Padding(5);
            this.dgvTopClients.Name = "dgvTopClients";
            this.dgvTopClients.ReadOnly = true;
            this.dgvTopClients.Size = new System.Drawing.Size(936, 460);
            this.dgvTopClients.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(42, 96);
            this.label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(136, 21);
            this.label1.TabIndex = 1;
            this.label1.Text = "Начальная дата:";
            // 
            // dtpStartDate
            // 
            this.dtpStartDate.Location = new System.Drawing.Point(188, 88);
            this.dtpStartDate.Margin = new System.Windows.Forms.Padding(5);
            this.dtpStartDate.Name = "dtpStartDate";
            this.dtpStartDate.Size = new System.Drawing.Size(208, 29);
            this.dtpStartDate.TabIndex = 2;
            // 
            // lblEndDate
            // 
            this.lblEndDate.AutoSize = true;
            this.lblEndDate.Location = new System.Drawing.Point(42, 135);
            this.lblEndDate.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lblEndDate.Name = "lblEndDate";
            this.lblEndDate.Size = new System.Drawing.Size(130, 21);
            this.lblEndDate.TabIndex = 3;
            this.lblEndDate.Text = "Конечная дата:";
            // 
            // dtpEndDate
            // 
            this.dtpEndDate.Location = new System.Drawing.Point(188, 127);
            this.dtpEndDate.Margin = new System.Windows.Forms.Padding(5);
            this.dtpEndDate.Name = "dtpEndDate";
            this.dtpEndDate.Size = new System.Drawing.Size(208, 29);
            this.dtpEndDate.TabIndex = 4;
            // 
            // lblStatuses
            // 
            this.lblStatuses.AutoSize = true;
            this.lblStatuses.Location = new System.Drawing.Point(641, 104);
            this.lblStatuses.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lblStatuses.Name = "lblStatuses";
            this.lblStatuses.Size = new System.Drawing.Size(147, 21);
            this.lblStatuses.TabIndex = 5;
            this.lblStatuses.Text = "Статусы заказов:";
            // 
            // cmbStatus
            // 
            this.cmbStatus.FormattingEnabled = true;
            this.cmbStatus.Location = new System.Drawing.Point(798, 96);
            this.cmbStatus.Margin = new System.Windows.Forms.Padding(5);
            this.cmbStatus.Name = "cmbStatus";
            this.cmbStatus.Size = new System.Drawing.Size(184, 29);
            this.cmbStatus.TabIndex = 6;
            // 
            // lblRecordsCount
            // 
            this.lblRecordsCount.AutoSize = true;
            this.lblRecordsCount.Location = new System.Drawing.Point(52, 689);
            this.lblRecordsCount.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lblRecordsCount.Name = "lblRecordsCount";
            this.lblRecordsCount.Size = new System.Drawing.Size(143, 21);
            this.lblRecordsCount.TabIndex = 11;
            this.lblRecordsCount.Text = "Всего записей: 0";
            // 
            // lblTotalSum
            // 
            this.lblTotalSum.AutoSize = true;
            this.lblTotalSum.Location = new System.Drawing.Point(52, 668);
            this.lblTotalSum.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lblTotalSum.Name = "lblTotalSum";
            this.lblTotalSum.Size = new System.Drawing.Size(153, 21);
            this.lblTotalSum.TabIndex = 13;
            this.lblTotalSum.Text = "Общая сумма: 0 ₽";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Times New Roman", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.ForeColor = System.Drawing.Color.DarkSeaGreen;
            this.label2.Location = new System.Drawing.Point(167, 9);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(734, 40);
            this.label2.TabIndex = 22;
            this.label2.Text = "ТОП 10 КЛИЕНТОВ ПО СУММЕ ЗАКАЗОВ";
            // 
            // btnExport
            // 
            this.btnExport.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.btnExport.Location = new System.Drawing.Point(790, 668);
            this.btnExport.Margin = new System.Windows.Forms.Padding(5);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(192, 47);
            this.btnExport.TabIndex = 24;
            this.btnExport.Text = "Экспорт в Excel";
            this.btnExport.UseVisualStyleBackColor = false;
            // 
            // btnGenerate
            // 
            this.btnGenerate.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.btnGenerate.Location = new System.Drawing.Point(588, 668);
            this.btnGenerate.Margin = new System.Windows.Forms.Padding(5);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(192, 47);
            this.btnGenerate.TabIndex = 23;
            this.btnGenerate.Text = "Сформировать отчёт";
            this.btnGenerate.UseVisualStyleBackColor = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::dump.Properties.Resources.remove;
            this.pictureBox1.Location = new System.Drawing.Point(965, 9);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(41, 33);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 25;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // TopClientsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1018, 748);
            this.ControlBox = false;
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblTotalSum);
            this.Controls.Add(this.lblRecordsCount);
            this.Controls.Add(this.cmbStatus);
            this.Controls.Add(this.lblStatuses);
            this.Controls.Add(this.dtpEndDate);
            this.Controls.Add(this.lblEndDate);
            this.Controls.Add(this.dtpStartDate);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dgvTopClients);
            this.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "TopClientsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.TopClientsForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvTopClients)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvTopClients;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dtpStartDate;
        private System.Windows.Forms.Label lblEndDate;
        private System.Windows.Forms.DateTimePicker dtpEndDate;
        private System.Windows.Forms.Label lblStatuses;
        private System.Windows.Forms.ComboBox cmbStatus;
        private System.Windows.Forms.Label lblRecordsCount;
        private System.Windows.Forms.Label lblTotalSum;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}