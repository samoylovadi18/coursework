namespace dump
{
    partial class OrdersReportForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OrdersReportForm));
            this.dtpStartDate = new System.Windows.Forms.DateTimePicker();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.dgvOrders = new System.Windows.Forms.DataGridView();
            this.btnExport = new System.Windows.Forms.Button();
            this.labelStartDate = new System.Windows.Forms.Label();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.dtpEndDate = new System.Windows.Forms.DateTimePicker();
            this.labelEndDate = new System.Windows.Forms.Label();
            this.buttonDetail = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOrders)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // dtpStartDate
            // 
            this.dtpStartDate.Location = new System.Drawing.Point(182, 81);
            this.dtpStartDate.Margin = new System.Windows.Forms.Padding(5);
            this.dtpStartDate.Name = "dtpStartDate";
            this.dtpStartDate.Size = new System.Drawing.Size(207, 29);
            this.dtpStartDate.TabIndex = 4;
            // 
            // btnGenerate
            // 
            this.btnGenerate.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.btnGenerate.Location = new System.Drawing.Point(580, 691);
            this.btnGenerate.Margin = new System.Windows.Forms.Padding(5);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(192, 47);
            this.btnGenerate.TabIndex = 5;
            this.btnGenerate.Text = "Сформировать отчёт";
            this.btnGenerate.UseVisualStyleBackColor = false;
            // 
            // dgvOrders
            // 
            this.dgvOrders.AllowUserToAddRows = false;
            this.dgvOrders.AllowUserToDeleteRows = false;
            this.dgvOrders.AllowUserToResizeColumns = false;
            this.dgvOrders.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvOrders.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dgvOrders.BackgroundColor = System.Drawing.Color.White;
            this.dgvOrders.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvOrders.Location = new System.Drawing.Point(54, 144);
            this.dgvOrders.Margin = new System.Windows.Forms.Padding(5);
            this.dgvOrders.Name = "dgvOrders";
            this.dgvOrders.ReadOnly = true;
            this.dgvOrders.Size = new System.Drawing.Size(933, 520);
            this.dgvOrders.TabIndex = 6;
            // 
            // btnExport
            // 
            this.btnExport.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.btnExport.Location = new System.Drawing.Point(795, 691);
            this.btnExport.Margin = new System.Windows.Forms.Padding(5);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(192, 47);
            this.btnExport.TabIndex = 7;
            this.btnExport.Text = "Экспорт в Excel";
            this.btnExport.UseVisualStyleBackColor = false;
            // 
            // labelStartDate
            // 
            this.labelStartDate.AutoSize = true;
            this.labelStartDate.Location = new System.Drawing.Point(50, 92);
            this.labelStartDate.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.labelStartDate.Name = "labelStartDate";
            this.labelStartDate.Size = new System.Drawing.Size(53, 21);
            this.labelStartDate.TabIndex = 8;
            this.labelStartDate.Text = "label1";
            // 
            // pictureBox3
            // 
            this.pictureBox3.Image = global::dump.Properties.Resources.remove;
            this.pictureBox3.Location = new System.Drawing.Point(1355, -5);
            this.pictureBox3.Margin = new System.Windows.Forms.Padding(5);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(68, 54);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox3.TabIndex = 18;
            this.pictureBox3.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::dump.Properties.Resources.remove;
            this.pictureBox1.Location = new System.Drawing.Point(976, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(41, 33);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 19;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Times New Roman", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.ForeColor = System.Drawing.Color.DarkSeaGreen;
            this.label1.Location = new System.Drawing.Point(370, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(295, 40);
            this.label1.TabIndex = 22;
            this.label1.Text = "Отчёт по заказам";
            // 
            // dtpEndDate
            // 
            this.dtpEndDate.Location = new System.Drawing.Point(555, 79);
            this.dtpEndDate.Margin = new System.Windows.Forms.Padding(5);
            this.dtpEndDate.Name = "dtpEndDate";
            this.dtpEndDate.Size = new System.Drawing.Size(207, 29);
            this.dtpEndDate.TabIndex = 23;
            // 
            // labelEndDate
            // 
            this.labelEndDate.AutoSize = true;
            this.labelEndDate.Location = new System.Drawing.Point(508, 87);
            this.labelEndDate.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.labelEndDate.Name = "labelEndDate";
            this.labelEndDate.Size = new System.Drawing.Size(53, 21);
            this.labelEndDate.TabIndex = 24;
            this.labelEndDate.Text = "label1";
            // 
            // buttonDetail
            // 
            this.buttonDetail.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.buttonDetail.Location = new System.Drawing.Point(369, 691);
            this.buttonDetail.Margin = new System.Windows.Forms.Padding(5);
            this.buttonDetail.Name = "buttonDetail";
            this.buttonDetail.Size = new System.Drawing.Size(192, 47);
            this.buttonDetail.TabIndex = 26;
            this.buttonDetail.Text = "Детальная инф.";
            this.buttonDetail.UseVisualStyleBackColor = false;
            // 
            // OrdersReportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1029, 763);
            this.ControlBox = false;
            this.Controls.Add(this.buttonDetail);
            this.Controls.Add(this.labelEndDate);
            this.Controls.Add(this.dtpEndDate);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.pictureBox3);
            this.Controls.Add(this.labelStartDate);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.dgvOrders);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.dtpStartDate);
            this.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.MaximumSize = new System.Drawing.Size(1045, 779);
            this.MinimumSize = new System.Drawing.Size(1045, 779);
            this.Name = "OrdersReportForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.RevenueForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvOrders)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.DateTimePicker dtpStartDate;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.DataGridView dgvOrders;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Label labelStartDate;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dtpEndDate;
        private System.Windows.Forms.Label labelEndDate;
        private System.Windows.Forms.Button buttonDetail;
    }
}