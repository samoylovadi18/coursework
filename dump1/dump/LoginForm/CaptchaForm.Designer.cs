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
            this.btnGenerate = new System.Windows.Forms.Button();
            this.btnVerify = new System.Windows.Forms.Button();
            this.lblTimer = new System.Windows.Forms.Label();
            this.lblInstruction = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picCaptcha)).BeginInit();
            this.SuspendLayout();
            // 
            // picCaptcha
            // 
            this.picCaptcha.Location = new System.Drawing.Point(63, 57);
            this.picCaptcha.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.picCaptcha.Name = "picCaptcha";
            this.picCaptcha.Size = new System.Drawing.Size(372, 216);
            this.picCaptcha.TabIndex = 0;
            this.picCaptcha.TabStop = false;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // txtCaptcha
            // 
            this.txtCaptcha.Location = new System.Drawing.Point(63, 311);
            this.txtCaptcha.Name = "txtCaptcha";
            this.txtCaptcha.Size = new System.Drawing.Size(260, 29);
            this.txtCaptcha.TabIndex = 2;
            // 
            // btnGenerate
            // 
            this.btnGenerate.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.btnGenerate.Location = new System.Drawing.Point(331, 311);
            this.btnGenerate.Margin = new System.Windows.Forms.Padding(5);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(104, 29);
            this.btnGenerate.TabIndex = 9;
            this.btnGenerate.Text = "Обновить";
            this.btnGenerate.UseVisualStyleBackColor = false;
            // 
            // btnVerify
            // 
            this.btnVerify.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.btnVerify.Location = new System.Drawing.Point(63, 363);
            this.btnVerify.Margin = new System.Windows.Forms.Padding(5);
            this.btnVerify.Name = "btnVerify";
            this.btnVerify.Size = new System.Drawing.Size(372, 57);
            this.btnVerify.TabIndex = 10;
            this.btnVerify.Text = "Проверить";
            this.btnVerify.UseVisualStyleBackColor = false;
            // 
            // lblTimer
            // 
            this.lblTimer.AutoSize = true;
            this.lblTimer.Location = new System.Drawing.Point(63, 434);
            this.lblTimer.Name = "lblTimer";
            this.lblTimer.Size = new System.Drawing.Size(53, 21);
            this.lblTimer.TabIndex = 11;
            this.lblTimer.Text = "label1";
            // 
            // lblInstruction
            // 
            this.lblInstruction.AutoSize = true;
            this.lblInstruction.Location = new System.Drawing.Point(63, 278);
            this.lblInstruction.Name = "lblInstruction";
            this.lblInstruction.Size = new System.Drawing.Size(245, 21);
            this.lblInstruction.TabIndex = 12;
            this.lblInstruction.Text = "Введите символы с картинки";
            // 
            // CaptchaForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(477, 504);
            this.ControlBox = false;
            this.Controls.Add(this.lblInstruction);
            this.Controls.Add(this.lblTimer);
            this.Controls.Add(this.btnVerify);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.txtCaptcha);
            this.Controls.Add(this.picCaptcha);
            this.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.Name = "CaptchaForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CaptchaForm";
            this.Load += new System.EventHandler(this.CaptchaForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picCaptcha)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picCaptcha;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.TextBox txtCaptcha;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Button btnVerify;
        private System.Windows.Forms.Label lblTimer;
        private System.Windows.Forms.Label lblInstruction;
    }
}