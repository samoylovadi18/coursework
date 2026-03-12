namespace dump
{
    partial class CertificateStatisticsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CertificateStatisticsForm));
            this.dgvCertificates = new System.Windows.Forms.DataGridView();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.labelEnd = new System.Windows.Forms.Label();
            this.datePickerEnd = new System.Windows.Forms.DateTimePicker();
            this.labelStart = new System.Windows.Forms.Label();
            this.datePickerStart = new System.Windows.Forms.DateTimePicker();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCertificates)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvCertificates
            // 
            this.dgvCertificates.AllowUserToAddRows = false;
            this.dgvCertificates.AllowUserToDeleteRows = false;
            this.dgvCertificates.AllowUserToResizeColumns = false;
            this.dgvCertificates.BackgroundColor = System.Drawing.Color.White;
            this.dgvCertificates.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCertificates.Location = new System.Drawing.Point(39, 153);
            this.dgvCertificates.Margin = new System.Windows.Forms.Padding(5);
            this.dgvCertificates.Name = "dgvCertificates";
            this.dgvCertificates.Size = new System.Drawing.Size(939, 522);
            this.dgvCertificates.TabIndex = 4;
            // 
            // btnGenerate
            // 
            this.btnGenerate.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.btnGenerate.Location = new System.Drawing.Point(584, 702);
            this.btnGenerate.Margin = new System.Windows.Forms.Padding(5);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(192, 47);
            this.btnGenerate.TabIndex = 6;
            this.btnGenerate.Text = "Сформировать отчёт";
            this.btnGenerate.UseVisualStyleBackColor = false;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // btnExport
            // 
            this.btnExport.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.btnExport.Location = new System.Drawing.Point(786, 702);
            this.btnExport.Margin = new System.Windows.Forms.Padding(5);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(192, 47);
            this.btnExport.TabIndex = 7;
            this.btnExport.Text = "Экспорт в Excel";
            this.btnExport.UseVisualStyleBackColor = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::dump.Properties.Resources.remove;
            this.pictureBox1.Location = new System.Drawing.Point(976, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(41, 33);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 20;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Times New Roman", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.ForeColor = System.Drawing.Color.DarkSeaGreen;
            this.label1.Location = new System.Drawing.Point(273, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(485, 40);
            this.label1.TabIndex = 21;
            this.label1.Text = "Статистика по сертификатам";
            // 
            // labelEnd
            // 
            this.labelEnd.AutoSize = true;
            this.labelEnd.Location = new System.Drawing.Point(470, 90);
            this.labelEnd.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.labelEnd.Name = "labelEnd";
            this.labelEnd.Size = new System.Drawing.Size(34, 21);
            this.labelEnd.TabIndex = 28;
            this.labelEnd.Text = "по:";
            // 
            // datePickerEnd
            // 
            this.datePickerEnd.Location = new System.Drawing.Point(508, 80);
            this.datePickerEnd.Margin = new System.Windows.Forms.Padding(5);
            this.datePickerEnd.Name = "datePickerEnd";
            this.datePickerEnd.Size = new System.Drawing.Size(207, 29);
            this.datePickerEnd.TabIndex = 27;
            // 
            // labelStart
            // 
            this.labelStart.AutoSize = true;
            this.labelStart.Location = new System.Drawing.Point(37, 90);
            this.labelStart.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.labelStart.Name = "labelStart";
            this.labelStart.Size = new System.Drawing.Size(88, 21);
            this.labelStart.TabIndex = 26;
            this.labelStart.Text = "Период с:";
            // 
            // datePickerStart
            // 
            this.datePickerStart.Location = new System.Drawing.Point(135, 82);
            this.datePickerStart.Margin = new System.Windows.Forms.Padding(5);
            this.datePickerStart.Name = "datePickerStart";
            this.datePickerStart.Size = new System.Drawing.Size(207, 29);
            this.datePickerStart.TabIndex = 25;
            // 
            // CertificateStatisticsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1029, 763);
            this.ControlBox = false;
            this.Controls.Add(this.labelEnd);
            this.Controls.Add(this.datePickerEnd);
            this.Controls.Add(this.labelStart);
            this.Controls.Add(this.datePickerStart);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.dgvCertificates);
            this.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.MaximumSize = new System.Drawing.Size(1045, 779);
            this.MinimumSize = new System.Drawing.Size(1045, 779);
            this.Name = "CertificateStatisticsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.CertificateStatisticsForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCertificates)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.DataGridView dgvCertificates;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelEnd;
        private System.Windows.Forms.DateTimePicker datePickerEnd;
        private System.Windows.Forms.Label labelStart;
        private System.Windows.Forms.DateTimePicker datePickerStart;
    }
}