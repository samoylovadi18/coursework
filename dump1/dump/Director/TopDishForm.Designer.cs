namespace dump
{
    partial class TopDishForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TopDishForm));
            this.dateTimePickerStart = new System.Windows.Forms.DateTimePicker();
            this.dateTimePickerEnd = new System.Windows.Forms.DateTimePicker();
            this.comboBoxCategory = new System.Windows.Forms.ComboBox();
            this.dataGridViewTopDish = new System.Windows.Forms.DataGridView();
            this.labelTotalRevenue = new System.Windows.Forms.Label();
            this.labelTotalSold = new System.Windows.Forms.Label();
            this.buttonExport = new System.Windows.Forms.Button();
            this.lblEndDate = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTopDish)).BeginInit();
            this.SuspendLayout();
            // 
            // dateTimePickerStart
            // 
            this.dateTimePickerStart.Location = new System.Drawing.Point(186, 69);
            this.dateTimePickerStart.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.dateTimePickerStart.Name = "dateTimePickerStart";
            this.dateTimePickerStart.Size = new System.Drawing.Size(331, 29);
            this.dateTimePickerStart.TabIndex = 0;
            // 
            // dateTimePickerEnd
            // 
            this.dateTimePickerEnd.Location = new System.Drawing.Point(186, 111);
            this.dateTimePickerEnd.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.dateTimePickerEnd.Name = "dateTimePickerEnd";
            this.dateTimePickerEnd.Size = new System.Drawing.Size(331, 29);
            this.dateTimePickerEnd.TabIndex = 1;
            // 
            // comboBoxCategory
            // 
            this.comboBoxCategory.FormattingEnabled = true;
            this.comboBoxCategory.Location = new System.Drawing.Point(788, 111);
            this.comboBoxCategory.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.comboBoxCategory.Name = "comboBoxCategory";
            this.comboBoxCategory.Size = new System.Drawing.Size(199, 29);
            this.comboBoxCategory.TabIndex = 2;
            // 
            // dataGridViewTopDish
            // 
            this.dataGridViewTopDish.BackgroundColor = System.Drawing.Color.White;
            this.dataGridViewTopDish.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewTopDish.Location = new System.Drawing.Point(40, 177);
            this.dataGridViewTopDish.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.dataGridViewTopDish.Name = "dataGridViewTopDish";
            this.dataGridViewTopDish.Size = new System.Drawing.Size(947, 472);
            this.dataGridViewTopDish.TabIndex = 14;
            // 
            // labelTotalRevenue
            // 
            this.labelTotalRevenue.AutoSize = true;
            this.labelTotalRevenue.Location = new System.Drawing.Point(36, 681);
            this.labelTotalRevenue.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.labelTotalRevenue.Name = "labelTotalRevenue";
            this.labelTotalRevenue.Size = new System.Drawing.Size(53, 21);
            this.labelTotalRevenue.TabIndex = 15;
            this.labelTotalRevenue.Text = "label1";
            // 
            // labelTotalSold
            // 
            this.labelTotalSold.AutoSize = true;
            this.labelTotalSold.Location = new System.Drawing.Point(36, 707);
            this.labelTotalSold.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.labelTotalSold.Name = "labelTotalSold";
            this.labelTotalSold.Size = new System.Drawing.Size(53, 21);
            this.labelTotalSold.TabIndex = 16;
            this.labelTotalSold.Text = "label1";
            // 
            // buttonExport
            // 
            this.buttonExport.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.buttonExport.Location = new System.Drawing.Point(795, 681);
            this.buttonExport.Margin = new System.Windows.Forms.Padding(5);
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Size = new System.Drawing.Size(192, 47);
            this.buttonExport.TabIndex = 29;
            this.buttonExport.Text = "Экспорт в Excel";
            this.buttonExport.UseVisualStyleBackColor = false;
            // 
            // lblEndDate
            // 
            this.lblEndDate.AutoSize = true;
            this.lblEndDate.Location = new System.Drawing.Point(40, 119);
            this.lblEndDate.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lblEndDate.Name = "lblEndDate";
            this.lblEndDate.Size = new System.Drawing.Size(130, 21);
            this.lblEndDate.TabIndex = 31;
            this.lblEndDate.Text = "Конечная дата:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(40, 80);
            this.label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(136, 21);
            this.label1.TabIndex = 30;
            this.label1.Text = "Начальная дата:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(680, 119);
            this.label2.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 21);
            this.label2.TabIndex = 32;
            this.label2.Text = "Категория:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Times New Roman", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label3.ForeColor = System.Drawing.Color.DarkSeaGreen;
            this.label3.Location = new System.Drawing.Point(405, 9);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(265, 40);
            this.label3.TabIndex = 33;
            this.label3.Text = "ТОП 10 БЛЮД ";
            // 
            // TopDishForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1029, 740);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblEndDate);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonExport);
            this.Controls.Add(this.labelTotalSold);
            this.Controls.Add(this.labelTotalRevenue);
            this.Controls.Add(this.dataGridViewTopDish);
            this.Controls.Add(this.comboBoxCategory);
            this.Controls.Add(this.dateTimePickerEnd);
            this.Controls.Add(this.dateTimePickerStart);
            this.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1045, 779);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(1045, 779);
            this.Name = "TopDishForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.TopDishForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTopDish)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DateTimePicker dateTimePickerStart;
        private System.Windows.Forms.DateTimePicker dateTimePickerEnd;
        private System.Windows.Forms.ComboBox comboBoxCategory;
        private System.Windows.Forms.DataGridView dataGridViewTopDish;
        private System.Windows.Forms.Label labelTotalRevenue;
        private System.Windows.Forms.Label labelTotalSold;
        private System.Windows.Forms.Button buttonExport;
        private System.Windows.Forms.Label lblEndDate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }
}