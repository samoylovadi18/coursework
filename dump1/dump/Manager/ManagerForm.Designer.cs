namespace dump
{
    partial class ManagerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ManagerForm));
            this.label2 = new System.Windows.Forms.Label();
            this.buttonOrder = new System.Windows.Forms.Button();
            this.buttonCerts = new System.Windows.Forms.Button();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.buttonCurrentOrders = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.buttonUse = new System.Windows.Forms.Button();
            this.buttonIssue = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(251, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(308, 31);
            this.label2.TabIndex = 15;
            this.label2.Text = "Главное меню менеджера";
            // 
            // buttonOrder
            // 
            this.buttonOrder.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.buttonOrder.Location = new System.Drawing.Point(53, 178);
            this.buttonOrder.Name = "buttonOrder";
            this.buttonOrder.Size = new System.Drawing.Size(298, 87);
            this.buttonOrder.TabIndex = 9;
            this.buttonOrder.Text = "Формирование заказа";
            this.buttonOrder.UseVisualStyleBackColor = false;
            this.buttonOrder.Click += new System.EventHandler(this.buttonOrder_Click);
            // 
            // buttonCerts
            // 
            this.buttonCerts.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.buttonCerts.Location = new System.Drawing.Point(53, 271);
            this.buttonCerts.Name = "buttonCerts";
            this.buttonCerts.Size = new System.Drawing.Size(298, 87);
            this.buttonCerts.TabIndex = 10;
            this.buttonCerts.Text = "Управление сертификатами";
            this.buttonCerts.UseVisualStyleBackColor = false;
            this.buttonCerts.Click += new System.EventHandler(this.buttonCerts_Click);
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::dump.Properties.Resources.remove;
            this.pictureBox2.Location = new System.Drawing.Point(803, 2);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(41, 33);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 16;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Click += new System.EventHandler(this.pictureBox2_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::dump.Properties.Resources.admin1;
            this.pictureBox1.Location = new System.Drawing.Point(419, 158);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(357, 318);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 14;
            this.pictureBox1.TabStop = false;
            // 
            // buttonCurrentOrders
            // 
            this.buttonCurrentOrders.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.buttonCurrentOrders.Location = new System.Drawing.Point(53, 364);
            this.buttonCurrentOrders.Name = "buttonCurrentOrders";
            this.buttonCurrentOrders.Size = new System.Drawing.Size(298, 87);
            this.buttonCurrentOrders.TabIndex = 17;
            this.buttonCurrentOrders.Text = "Текущие заказы";
            this.buttonCurrentOrders.UseVisualStyleBackColor = false;
            this.buttonCurrentOrders.Click += new System.EventHandler(this.buttonCurrentOrders_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.pictureBox3);
            this.panel1.Location = new System.Drawing.Point(-3, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(872, 644);
            this.panel1.TabIndex = 18;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // pictureBox3
            // 
            this.pictureBox3.Image = global::dump.Properties.Resources.remove;
            this.pictureBox3.Location = new System.Drawing.Point(803, 2);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(41, 33);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox3.TabIndex = 19;
            this.pictureBox3.TabStop = false;
            this.pictureBox3.Click += new System.EventHandler(this.pictureBox3_Click);
            // 
            // buttonUse
            // 
            this.buttonUse.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.buttonUse.Location = new System.Drawing.Point(279, 317);
            this.buttonUse.Name = "buttonUse";
            this.buttonUse.Size = new System.Drawing.Size(298, 87);
            this.buttonUse.TabIndex = 20;
            this.buttonUse.Text = "Использовать";
            this.buttonUse.UseVisualStyleBackColor = false;
            this.buttonUse.Click += new System.EventHandler(this.buttonUse_Click);
            // 
            // buttonIssue
            // 
            this.buttonIssue.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.buttonIssue.Location = new System.Drawing.Point(279, 224);
            this.buttonIssue.Name = "buttonIssue";
            this.buttonIssue.Size = new System.Drawing.Size(298, 87);
            this.buttonIssue.TabIndex = 19;
            this.buttonIssue.Text = "Выдать";
            this.buttonIssue.UseVisualStyleBackColor = false;
            this.buttonIssue.Click += new System.EventHandler(this.buttonIssue_Click);
            // 
            // ManagerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(15F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(856, 628);
            this.ControlBox = false;
            this.Controls.Add(this.buttonUse);
            this.Controls.Add(this.buttonIssue);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.buttonCurrentOrders);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.buttonCerts);
            this.Controls.Add(this.buttonOrder);
            this.Font = new System.Drawing.Font("Times New Roman", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(872, 644);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(872, 644);
            this.Name = "ManagerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.ManagerForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button buttonOrder;
        private System.Windows.Forms.Button buttonCerts;
        private System.Windows.Forms.Button buttonCurrentOrders;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.Button buttonUse;
        private System.Windows.Forms.Button buttonIssue;
    }
}