namespace dump
{
    partial class CaptchaForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CaptchaForm));
            this.picCaptcha = new System.Windows.Forms.PictureBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.txtCaptcha = new System.Windows.Forms.TextBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnVerify = new System.Windows.Forms.Button();
            this.lblInstruction = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.picCaptcha)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // picCaptcha
            // 
            this.picCaptcha.Location = new System.Drawing.Point(30, 20);
            this.picCaptcha.Margin = new System.Windows.Forms.Padding(5);
            this.picCaptcha.Name = "picCaptcha";
            this.picCaptcha.Size = new System.Drawing.Size(363, 178);
            this.picCaptcha.TabIndex = 0;
            this.picCaptcha.TabStop = false;
            this.picCaptcha.Click += new System.EventHandler(this.picCaptcha_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // txtCaptcha
            // 
            this.txtCaptcha.Location = new System.Drawing.Point(46, 323);
            this.txtCaptcha.Name = "txtCaptcha";
            this.txtCaptcha.Size = new System.Drawing.Size(308, 29);
            this.txtCaptcha.TabIndex = 2;
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.btnRefresh.Location = new System.Drawing.Point(362, 322);
            this.btnRefresh.Margin = new System.Windows.Forms.Padding(5);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(104, 29);
            this.btnRefresh.TabIndex = 9;
            this.btnRefresh.Text = "Обновить";
            this.btnRefresh.UseVisualStyleBackColor = false;
            // 
            // btnVerify
            // 
            this.btnVerify.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.btnVerify.Location = new System.Drawing.Point(46, 375);
            this.btnVerify.Margin = new System.Windows.Forms.Padding(5);
            this.btnVerify.Name = "btnVerify";
            this.btnVerify.Size = new System.Drawing.Size(420, 57);
            this.btnVerify.TabIndex = 10;
            this.btnVerify.Text = "Проверить";
            this.btnVerify.UseVisualStyleBackColor = false;
            // 
            // lblInstruction
            // 
            this.lblInstruction.AutoSize = true;
            this.lblInstruction.Location = new System.Drawing.Point(46, 290);
            this.lblInstruction.Name = "lblInstruction";
            this.lblInstruction.Size = new System.Drawing.Size(245, 21);
            this.lblInstruction.TabIndex = 12;
            this.lblInstruction.Text = "Введите символы с картинки";
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::dump.Properties.Resources.remove;
            this.pictureBox2.Location = new System.Drawing.Point(482, 12);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(41, 33);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 13;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Click += new System.EventHandler(this.pictureBox2_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.picCaptcha);
            this.groupBox1.Location = new System.Drawing.Point(47, 44);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(419, 230);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // CaptchaForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(539, 463);
            this.ControlBox = false;
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.lblInstruction);
            this.Controls.Add(this.btnVerify);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.txtCaptcha);
            this.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "CaptchaForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CaptchaForm";
            this.Load += new System.EventHandler(this.CaptchaForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picCaptcha)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picCaptcha;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.TextBox txtCaptcha;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnVerify;
        private System.Windows.Forms.Label lblInstruction;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}