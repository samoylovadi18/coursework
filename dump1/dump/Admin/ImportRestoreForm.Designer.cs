namespace dump
{
    partial class ImportRestoreForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportRestoreForm));
            this.label1 = new System.Windows.Forms.Label();
            this.grpRestore = new System.Windows.Forms.GroupBox();
            this.btnRestore = new System.Windows.Forms.Button();
            this.grpImport = new System.Windows.Forms.GroupBox();
            this.btnImport = new System.Windows.Forms.Button();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.lblFile = new System.Windows.Forms.Label();
            this.cmbTables = new System.Windows.Forms.ComboBox();
            this.lblTable = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.grpRestore.SuspendLayout();
            this.grpImport.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Times New Roman", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.ForeColor = System.Drawing.Color.DarkSeaGreen;
            this.label1.Location = new System.Drawing.Point(23, 14);
            this.label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(562, 40);
            this.label1.TabIndex = 22;
            this.label1.Text = "Импорт и восстановление данных";
            // 
            // grpRestore
            // 
            this.grpRestore.Controls.Add(this.btnRestore);
            this.grpRestore.Location = new System.Drawing.Point(47, 83);
            this.grpRestore.Margin = new System.Windows.Forms.Padding(5);
            this.grpRestore.Name = "grpRestore";
            this.grpRestore.Padding = new System.Windows.Forms.Padding(5);
            this.grpRestore.Size = new System.Drawing.Size(569, 231);
            this.grpRestore.TabIndex = 23;
            this.grpRestore.TabStop = false;
            // 
            // btnRestore
            // 
            this.btnRestore.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.btnRestore.Location = new System.Drawing.Point(112, 82);
            this.btnRestore.Name = "btnRestore";
            this.btnRestore.Size = new System.Drawing.Size(361, 46);
            this.btnRestore.TabIndex = 42;
            this.btnRestore.Text = " Восстановить структуру БД";
            this.btnRestore.UseVisualStyleBackColor = false;
            // 
            // grpImport
            // 
            this.grpImport.Controls.Add(this.btnImport);
            this.grpImport.Controls.Add(this.btnBrowse);
            this.grpImport.Controls.Add(this.txtFilePath);
            this.grpImport.Controls.Add(this.lblFile);
            this.grpImport.Controls.Add(this.cmbTables);
            this.grpImport.Controls.Add(this.lblTable);
            this.grpImport.Location = new System.Drawing.Point(47, 353);
            this.grpImport.Margin = new System.Windows.Forms.Padding(5);
            this.grpImport.Name = "grpImport";
            this.grpImport.Padding = new System.Windows.Forms.Padding(5);
            this.grpImport.Size = new System.Drawing.Size(569, 211);
            this.grpImport.TabIndex = 24;
            this.grpImport.TabStop = false;
            // 
            // btnImport
            // 
            this.btnImport.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.btnImport.Location = new System.Drawing.Point(112, 111);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(361, 46);
            this.btnImport.TabIndex = 43;
            this.btnImport.Text = "Импортировать";
            this.btnImport.UseVisualStyleBackColor = false;
            // 
            // btnBrowse
            // 
            this.btnBrowse.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.btnBrowse.Location = new System.Drawing.Point(479, 65);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(86, 29);
            this.btnBrowse.TabIndex = 43;
            this.btnBrowse.Text = "Обзор";
            this.btnBrowse.UseVisualStyleBackColor = false;

            // 
            // txtFilePath
            // 
            this.txtFilePath.Location = new System.Drawing.Point(112, 65);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.Size = new System.Drawing.Size(361, 29);
            this.txtFilePath.TabIndex = 3;
            // 
            // lblFile
            // 
            this.lblFile.AutoSize = true;
            this.lblFile.Location = new System.Drawing.Point(9, 73);
            this.lblFile.Name = "lblFile";
            this.lblFile.Size = new System.Drawing.Size(55, 21);
            this.lblFile.TabIndex = 2;
            this.lblFile.Text = "Файл:";
            // 
            // cmbTables
            // 
            this.cmbTables.FormattingEnabled = true;
            this.cmbTables.Location = new System.Drawing.Point(112, 20);
            this.cmbTables.Name = "cmbTables";
            this.cmbTables.Size = new System.Drawing.Size(361, 29);
            this.cmbTables.TabIndex = 1;
            // 
            // lblTable
            // 
            this.lblTable.AutoSize = true;
            this.lblTable.Location = new System.Drawing.Point(9, 28);
            this.lblTable.Name = "lblTable";
            this.lblTable.Size = new System.Drawing.Size(81, 21);
            this.lblTable.TabIndex = 0;
            this.lblTable.Text = "Таблица:";
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::dump.Properties.Resources.remove;
            this.pictureBox2.Location = new System.Drawing.Point(595, 21);
            this.pictureBox2.Margin = new System.Windows.Forms.Padding(5);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(41, 33);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 21;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Click += new System.EventHandler(this.pictureBox2_Click_1);
            // 
            // ImportRestoreForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(649, 594);
            this.ControlBox = false;
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.grpImport);
            this.Controls.Add(this.grpRestore);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "ImportRestoreForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ImportRestoreForm";
            this.Load += new System.EventHandler(this.ImportRestoreForm_Load);
            this.grpRestore.ResumeLayout(false);
            this.grpImport.ResumeLayout(false);
            this.grpImport.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox grpRestore;
        private System.Windows.Forms.GroupBox grpImport;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Button btnRestore;
        private System.Windows.Forms.Label lblFile;
        private System.Windows.Forms.ComboBox cmbTables;
        private System.Windows.Forms.Label lblTable;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Button btnImport;
    }
}