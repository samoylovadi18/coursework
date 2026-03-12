namespace dump
{
    partial class ProfitForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProfitForm));
            this.dtpStartDate = new System.Windows.Forms.DateTimePicker();
            this.labelStartDate = new System.Windows.Forms.Label();
            this.labelEndDate = new System.Windows.Forms.Label();
            this.dtpEndDate = new System.Windows.Forms.DateTimePicker();
            this.dgvProfit = new System.Windows.Forms.DataGridView();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.pictureBoxBack = new System.Windows.Forms.PictureBox();
            this.lblTotalProfit = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProfit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBack)).BeginInit();
            this.SuspendLayout();
            // 
            // dtpStartDate
            // 
            this.dtpStartDate.Location = new System.Drawing.Point(138, 89);
            this.dtpStartDate.Name = "dtpStartDate";
            this.dtpStartDate.Size = new System.Drawing.Size(200, 29);
            this.dtpStartDate.TabIndex = 0;
            // 
            // labelStartDate
            // 
            this.labelStartDate.AutoSize = true;
            this.labelStartDate.Location = new System.Drawing.Point(44, 97);
            this.labelStartDate.Name = "labelStartDate";
            this.labelStartDate.Size = new System.Drawing.Size(88, 21);
            this.labelStartDate.TabIndex = 1;
            this.labelStartDate.Text = "Период с:";
            // 
            // labelEndDate
            // 
            this.labelEndDate.AutoSize = true;
            this.labelEndDate.Location = new System.Drawing.Point(463, 97);
            this.labelEndDate.Name = "labelEndDate";
            this.labelEndDate.Size = new System.Drawing.Size(34, 21);
            this.labelEndDate.TabIndex = 3;
            this.labelEndDate.Text = "по:";
            // 
            // dtpEndDate
            // 
            this.dtpEndDate.Location = new System.Drawing.Point(503, 89);
            this.dtpEndDate.Name = "dtpEndDate";
            this.dtpEndDate.Size = new System.Drawing.Size(200, 29);
            this.dtpEndDate.TabIndex = 2;
            // 
            // dgvProfit
            // 
            this.dgvProfit.AllowUserToAddRows = false;
            this.dgvProfit.AllowUserToDeleteRows = false;
            this.dgvProfit.AllowUserToResizeColumns = false;
            this.dgvProfit.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvProfit.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dgvProfit.BackgroundColor = System.Drawing.Color.White;
            this.dgvProfit.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvProfit.Location = new System.Drawing.Point(48, 165);
            this.dgvProfit.Name = "dgvProfit";
            this.dgvProfit.Size = new System.Drawing.Size(934, 459);
            this.dgvProfit.TabIndex = 4;
            // 
            // btnExport
            // 
            this.btnExport.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.btnExport.Location = new System.Drawing.Point(790, 644);
            this.btnExport.Margin = new System.Windows.Forms.Padding(5);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(192, 47);
            this.btnExport.TabIndex = 9;
            this.btnExport.Text = "Экспорт в Excel";
            this.btnExport.UseVisualStyleBackColor = false;
            // 
            // btnGenerate
            // 
            this.btnGenerate.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.btnGenerate.Location = new System.Drawing.Point(575, 644);
            this.btnGenerate.Margin = new System.Windows.Forms.Padding(5);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(192, 47);
            this.btnGenerate.TabIndex = 8;
            this.btnGenerate.Text = "Сформировать отчёт";
            this.btnGenerate.UseVisualStyleBackColor = false;
            // 
            // pictureBoxBack
            // 
            this.pictureBoxBack.Image = global::dump.Properties.Resources.remove;
            this.pictureBoxBack.Location = new System.Drawing.Point(976, 9);
            this.pictureBoxBack.Name = "pictureBoxBack";
            this.pictureBoxBack.Size = new System.Drawing.Size(41, 33);
            this.pictureBoxBack.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxBack.TabIndex = 26;
            this.pictureBoxBack.TabStop = false;
            this.pictureBoxBack.Click += new System.EventHandler(this.pictureBoxBack_Click_1);
            // 
            // lblTotalProfit
            // 
            this.lblTotalProfit.AutoSize = true;
            this.lblTotalProfit.Font = new System.Drawing.Font("Times New Roman", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblTotalProfit.ForeColor = System.Drawing.Color.DarkSeaGreen;
            this.lblTotalProfit.Location = new System.Drawing.Point(357, 2);
            this.lblTotalProfit.Name = "lblTotalProfit";
            this.lblTotalProfit.Size = new System.Drawing.Size(314, 40);
            this.lblTotalProfit.TabIndex = 27;
            this.lblTotalProfit.Text = "Отчёт по прибыли";
            // 
            // ProfitForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1029, 763);
            this.ControlBox = false;
            this.Controls.Add(this.lblTotalProfit);
            this.Controls.Add(this.pictureBoxBack);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.dgvProfit);
            this.Controls.Add(this.labelEndDate);
            this.Controls.Add(this.dtpEndDate);
            this.Controls.Add(this.labelStartDate);
            this.Controls.Add(this.dtpStartDate);
            this.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.MaximumSize = new System.Drawing.Size(1045, 779);
            this.MinimumSize = new System.Drawing.Size(1045, 779);
            this.Name = "ProfitForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.ProfitForm_Load_1);
            ((System.ComponentModel.ISupportInitialize)(this.dgvProfit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBack)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DateTimePicker dtpStartDate;
        private System.Windows.Forms.Label labelStartDate;
        private System.Windows.Forms.Label labelEndDate;
        private System.Windows.Forms.DateTimePicker dtpEndDate;
        private System.Windows.Forms.DataGridView dgvProfit;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.PictureBox pictureBoxBack;
        private System.Windows.Forms.Label lblTotalProfit;
    }
}