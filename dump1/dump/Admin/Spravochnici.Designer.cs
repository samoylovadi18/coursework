namespace dump
{
    partial class Spravochnici
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Spravochnici));
            this.label1 = new System.Windows.Forms.Label();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.AddButton = new System.Windows.Forms.Button();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.tabPresent = new System.Windows.Forms.TabPage();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxFromPrice = new System.Windows.Forms.TextBox();
            this.textBoxPresentName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.button10 = new System.Windows.Forms.Button();
            this.button11 = new System.Windows.Forms.Button();
            this.button12 = new System.Windows.Forms.Button();
            this.dataGridViewPresents = new System.Windows.Forms.DataGridView();
            this.tabCategories = new System.Windows.Forms.TabPage();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxCategoryName = new System.Windows.Forms.TextBox();
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.dataGridViewCategories = new System.Windows.Forms.DataGridView();
            this.tabConrol1 = new System.Windows.Forms.TabControl();
            this.tabStatus = new System.Windows.Forms.TabPage();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxStatusName = new System.Windows.Forms.TextBox();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.dataGridViewStatus = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.tabPresent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPresents)).BeginInit();
            this.tabCategories.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewCategories)).BeginInit();
            this.tabConrol1.SuspendLayout();
            this.tabStatus.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewStatus)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Times New Roman", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.ForeColor = System.Drawing.Color.DarkSeaGreen;
            this.label1.Location = new System.Drawing.Point(310, -6);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(235, 40);
            this.label1.TabIndex = 21;
            this.label1.Text = "Справочники";
            // 
            // buttonDelete
            // 
            this.buttonDelete.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.buttonDelete.Location = new System.Drawing.Point(573, 541);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(170, 44);
            this.buttonDelete.TabIndex = 29;
            this.buttonDelete.Text = "Удалить";
            this.buttonDelete.UseVisualStyleBackColor = false;
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            // 
            // AddButton
            // 
            this.AddButton.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.AddButton.Location = new System.Drawing.Point(397, 541);
            this.AddButton.Name = "AddButton";
            this.AddButton.Size = new System.Drawing.Size(170, 44);
            this.AddButton.TabIndex = 27;
            this.AddButton.Text = "Добавить";
            this.AddButton.UseVisualStyleBackColor = false;
            this.AddButton.Click += new System.EventHandler(this.AddButton_Click);
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::dump.Properties.Resources.remove;
            this.pictureBox2.Location = new System.Drawing.Point(815, 1);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(41, 33);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 29;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Click += new System.EventHandler(this.pictureBox2_Click);
            // 
            // tabPresent
            // 
            this.tabPresent.Controls.Add(this.label6);
            this.tabPresent.Controls.Add(this.textBoxFromPrice);
            this.tabPresent.Controls.Add(this.textBoxPresentName);
            this.tabPresent.Controls.Add(this.label5);
            this.tabPresent.Controls.Add(this.button10);
            this.tabPresent.Controls.Add(this.button11);
            this.tabPresent.Controls.Add(this.button12);
            this.tabPresent.Controls.Add(this.dataGridViewPresents);
            this.tabPresent.Location = new System.Drawing.Point(4, 30);
            this.tabPresent.Name = "tabPresent";
            this.tabPresent.Size = new System.Drawing.Size(619, 429);
            this.tabPresent.TabIndex = 3;
            this.tabPresent.Text = "Подарки";
            this.tabPresent.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(29, 358);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(146, 21);
            this.label6.TabIndex = 32;
            this.label6.Text = "От какой суммы:";
            // 
            // textBoxFromPrice
            // 
            this.textBoxFromPrice.Location = new System.Drawing.Point(192, 350);
            this.textBoxFromPrice.Name = "textBoxFromPrice";
            this.textBoxFromPrice.Size = new System.Drawing.Size(403, 29);
            this.textBoxFromPrice.TabIndex = 31;
            // 
            // textBoxPresentName
            // 
            this.textBoxPresentName.Location = new System.Drawing.Point(192, 301);
            this.textBoxPresentName.Name = "textBoxPresentName";
            this.textBoxPresentName.Size = new System.Drawing.Size(403, 29);
            this.textBoxPresentName.TabIndex = 29;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(29, 309);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(157, 21);
            this.label5.TabIndex = 30;
            this.label5.Text = "Название подарка:";
            // 
            // button10
            // 
            this.button10.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.button10.Location = new System.Drawing.Point(1192, 711);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(170, 44);
            this.button10.TabIndex = 28;
            this.button10.Text = "Удалить";
            this.button10.UseVisualStyleBackColor = false;
            // 
            // button11
            // 
            this.button11.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.button11.Location = new System.Drawing.Point(1016, 711);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(170, 44);
            this.button11.TabIndex = 27;
            this.button11.Text = "Изменить";
            this.button11.UseVisualStyleBackColor = false;
            // 
            // button12
            // 
            this.button12.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.button12.Location = new System.Drawing.Point(840, 711);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(170, 44);
            this.button12.TabIndex = 26;
            this.button12.Text = "Добавить";
            this.button12.UseVisualStyleBackColor = false;
            // 
            // dataGridViewPresents
            // 
            this.dataGridViewPresents.AllowUserToAddRows = false;
            this.dataGridViewPresents.AllowUserToDeleteRows = false;
            this.dataGridViewPresents.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewPresents.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridViewPresents.BackgroundColor = System.Drawing.Color.White;
            this.dataGridViewPresents.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewPresents.Location = new System.Drawing.Point(33, 17);
            this.dataGridViewPresents.Name = "dataGridViewPresents";
            this.dataGridViewPresents.ReadOnly = true;
            this.dataGridViewPresents.Size = new System.Drawing.Size(562, 257);
            this.dataGridViewPresents.TabIndex = 0;
            // 
            // tabCategories
            // 
            this.tabCategories.Controls.Add(this.label4);
            this.tabCategories.Controls.Add(this.textBoxCategoryName);
            this.tabCategories.Controls.Add(this.button7);
            this.tabCategories.Controls.Add(this.button8);
            this.tabCategories.Controls.Add(this.button9);
            this.tabCategories.Controls.Add(this.dataGridViewCategories);
            this.tabCategories.Location = new System.Drawing.Point(4, 30);
            this.tabCategories.Name = "tabCategories";
            this.tabCategories.Size = new System.Drawing.Size(619, 429);
            this.tabCategories.TabIndex = 2;
            this.tabCategories.Text = "Категории";
            this.tabCategories.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(29, 309);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(174, 21);
            this.label4.TabIndex = 30;
            this.label4.Text = "Название категории:";
            // 
            // textBoxCategoryName
            // 
            this.textBoxCategoryName.Location = new System.Drawing.Point(209, 301);
            this.textBoxCategoryName.Name = "textBoxCategoryName";
            this.textBoxCategoryName.Size = new System.Drawing.Size(386, 29);
            this.textBoxCategoryName.TabIndex = 29;
            // 
            // button7
            // 
            this.button7.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.button7.Location = new System.Drawing.Point(1192, 711);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(170, 44);
            this.button7.TabIndex = 28;
            this.button7.Text = "Удалить";
            this.button7.UseVisualStyleBackColor = false;
            // 
            // button8
            // 
            this.button8.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.button8.Location = new System.Drawing.Point(1016, 711);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(170, 44);
            this.button8.TabIndex = 27;
            this.button8.Text = "Изменить";
            this.button8.UseVisualStyleBackColor = false;
            // 
            // button9
            // 
            this.button9.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.button9.Location = new System.Drawing.Point(840, 711);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(170, 44);
            this.button9.TabIndex = 26;
            this.button9.Text = "Добавить";
            this.button9.UseVisualStyleBackColor = false;
            // 
            // dataGridViewCategories
            // 
            this.dataGridViewCategories.AllowUserToAddRows = false;
            this.dataGridViewCategories.AllowUserToDeleteRows = false;
            this.dataGridViewCategories.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewCategories.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridViewCategories.BackgroundColor = System.Drawing.Color.White;
            this.dataGridViewCategories.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewCategories.Location = new System.Drawing.Point(33, 17);
            this.dataGridViewCategories.Name = "dataGridViewCategories";
            this.dataGridViewCategories.ReadOnly = true;
            this.dataGridViewCategories.Size = new System.Drawing.Size(562, 257);
            this.dataGridViewCategories.TabIndex = 0;
            // 
            // tabConrol1
            // 
            this.tabConrol1.Controls.Add(this.tabStatus);
            this.tabConrol1.Controls.Add(this.tabCategories);
            this.tabConrol1.Controls.Add(this.tabPresent);
            this.tabConrol1.Location = new System.Drawing.Point(120, 57);
            this.tabConrol1.Name = "tabConrol1";
            this.tabConrol1.SelectedIndex = 0;
            this.tabConrol1.Size = new System.Drawing.Size(627, 463);
            this.tabConrol1.TabIndex = 23;
            this.tabConrol1.Tag = "";
            // 
            // tabStatus
            // 
            this.tabStatus.Controls.Add(this.label3);
            this.tabStatus.Controls.Add(this.textBoxStatusName);
            this.tabStatus.Controls.Add(this.button4);
            this.tabStatus.Controls.Add(this.button5);
            this.tabStatus.Controls.Add(this.button6);
            this.tabStatus.Controls.Add(this.dataGridViewStatus);
            this.tabStatus.Location = new System.Drawing.Point(4, 30);
            this.tabStatus.Name = "tabStatus";
            this.tabStatus.Size = new System.Drawing.Size(619, 429);
            this.tabStatus.TabIndex = 1;
            this.tabStatus.Text = "Статусы заказов";
            this.tabStatus.UseVisualStyleBackColor = true;
            this.tabStatus.Click += new System.EventHandler(this.tabStatus_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(29, 309);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(152, 21);
            this.label3.TabIndex = 30;
            this.label3.Text = "Название статуса:";
            // 
            // textBoxStatusName
            // 
            this.textBoxStatusName.Location = new System.Drawing.Point(183, 301);
            this.textBoxStatusName.Name = "textBoxStatusName";
            this.textBoxStatusName.Size = new System.Drawing.Size(412, 29);
            this.textBoxStatusName.TabIndex = 29;
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.button4.Location = new System.Drawing.Point(1192, 711);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(170, 44);
            this.button4.TabIndex = 28;
            this.button4.Text = "Удалить";
            this.button4.UseVisualStyleBackColor = false;
            // 
            // button5
            // 
            this.button5.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.button5.Location = new System.Drawing.Point(1016, 711);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(170, 44);
            this.button5.TabIndex = 27;
            this.button5.Text = "Изменить";
            this.button5.UseVisualStyleBackColor = false;
            // 
            // button6
            // 
            this.button6.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.button6.Location = new System.Drawing.Point(840, 711);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(170, 44);
            this.button6.TabIndex = 26;
            this.button6.Text = "Добавить";
            this.button6.UseVisualStyleBackColor = false;
            // 
            // dataGridViewStatus
            // 
            this.dataGridViewStatus.AllowUserToAddRows = false;
            this.dataGridViewStatus.AllowUserToDeleteRows = false;
            this.dataGridViewStatus.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewStatus.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridViewStatus.BackgroundColor = System.Drawing.Color.White;
            this.dataGridViewStatus.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewStatus.Location = new System.Drawing.Point(33, 17);
            this.dataGridViewStatus.Name = "dataGridViewStatus";
            this.dataGridViewStatus.ReadOnly = true;
            this.dataGridViewStatus.Size = new System.Drawing.Size(562, 257);
            this.dataGridViewStatus.TabIndex = 0;
            // 
            // Spravochnici
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(856, 628);
            this.ControlBox = false;
            this.Controls.Add(this.buttonDelete);
            this.Controls.Add(this.AddButton);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.tabConrol1);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Spravochnici";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.Spravochnici_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.tabPresent.ResumeLayout(false);
            this.tabPresent.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPresents)).EndInit();
            this.tabCategories.ResumeLayout(false);
            this.tabCategories.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewCategories)).EndInit();
            this.tabConrol1.ResumeLayout(false);
            this.tabStatus.ResumeLayout(false);
            this.tabStatus.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewStatus)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.Button AddButton;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.TabPage tabPresent;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxFromPrice;
        private System.Windows.Forms.TextBox textBoxPresentName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.DataGridView dataGridViewPresents;
        private System.Windows.Forms.TabPage tabCategories;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxCategoryName;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.DataGridView dataGridViewCategories;
        private System.Windows.Forms.TabControl tabConrol1;
        private System.Windows.Forms.TabPage tabStatus;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxStatusName;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.DataGridView dataGridViewStatus;
    }
}