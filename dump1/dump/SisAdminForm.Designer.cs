namespace dump
{
    partial class SisAdminForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SisAdminForm));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnTestConnection = new System.Windows.Forms.Button();
            this.visible_password = new System.Windows.Forms.Button();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.labelPassword = new System.Windows.Forms.Label();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.labelUsername = new System.Windows.Forms.Label();
            this.txtDatabase = new System.Windows.Forms.TextBox();
            this.labelDatabase = new System.Windows.Forms.Label();
            this.txtServer = new System.Windows.Forms.TextBox();
            this.labelServer = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Location = new System.Drawing.Point(49, 75);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(760, 557);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.lblStatus);
            this.tabPage1.Controls.Add(this.btnSave);
            this.tabPage1.Controls.Add(this.btnTestConnection);
            this.tabPage1.Controls.Add(this.visible_password);
            this.tabPage1.Controls.Add(this.txtPassword);
            this.tabPage1.Controls.Add(this.labelPassword);
            this.tabPage1.Controls.Add(this.txtUsername);
            this.tabPage1.Controls.Add(this.labelUsername);
            this.tabPage1.Controls.Add(this.txtDatabase);
            this.tabPage1.Controls.Add(this.labelDatabase);
            this.tabPage1.Controls.Add(this.txtServer);
            this.tabPage1.Controls.Add(this.labelServer);
            this.tabPage1.Location = new System.Drawing.Point(4, 30);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(752, 523);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "База данных";
            this.tabPage1.UseVisualStyleBackColor = true;
            this.tabPage1.Click += new System.EventHandler(this.tabPage1_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 30);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(594, 402);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Импорт/Экспорт";
            this.tabPage2.UseVisualStyleBackColor = true;
            this.tabPage2.Click += new System.EventHandler(this.tabPage2_Click);
            // 
            // tabPage3
            // 
            this.tabPage3.Location = new System.Drawing.Point(4, 30);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(594, 402);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Резервное копирование";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // tabPage4
            // 
            this.tabPage4.Location = new System.Drawing.Point(4, 30);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(594, 402);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Безопасность";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Times New Roman", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblStatus.Location = new System.Drawing.Point(225, 286);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(104, 31);
            this.lblStatus.TabIndex = 34;
            this.lblStatus.Text = "Пароль:";
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.btnSave.Font = new System.Drawing.Font("Times New Roman", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnSave.Location = new System.Drawing.Point(230, 409);
            this.btnSave.Margin = new System.Windows.Forms.Padding(5);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(431, 52);
            this.btnSave.TabIndex = 33;
            this.btnSave.Text = "Сохранить";
            this.btnSave.UseVisualStyleBackColor = false;
            // 
            // btnTestConnection
            // 
            this.btnTestConnection.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.btnTestConnection.Font = new System.Drawing.Font("Times New Roman", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnTestConnection.Location = new System.Drawing.Point(230, 336);
            this.btnTestConnection.Margin = new System.Windows.Forms.Padding(5);
            this.btnTestConnection.Name = "btnTestConnection";
            this.btnTestConnection.Size = new System.Drawing.Size(431, 52);
            this.btnTestConnection.TabIndex = 32;
            this.btnTestConnection.Text = "Проверить подключение";
            this.btnTestConnection.UseVisualStyleBackColor = false;
            // 
            // visible_password
            // 
            this.visible_password.BackColor = System.Drawing.Color.White;
            this.visible_password.Font = new System.Drawing.Font("Times New Roman", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.visible_password.ForeColor = System.Drawing.SystemColors.ControlText;
            this.visible_password.Location = new System.Drawing.Point(667, 203);
            this.visible_password.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.visible_password.Name = "visible_password";
            this.visible_password.Size = new System.Drawing.Size(46, 47);
            this.visible_password.TabIndex = 31;
            this.visible_password.UseVisualStyleBackColor = false;
            // 
            // txtPassword
            // 
            this.txtPassword.Font = new System.Drawing.Font("Times New Roman", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtPassword.Location = new System.Drawing.Point(230, 211);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(431, 39);
            this.txtPassword.TabIndex = 30;
            // 
            // labelPassword
            // 
            this.labelPassword.AutoSize = true;
            this.labelPassword.Font = new System.Drawing.Font("Times New Roman", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelPassword.Location = new System.Drawing.Point(39, 219);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(104, 31);
            this.labelPassword.TabIndex = 29;
            this.labelPassword.Text = "Пароль:";
            // 
            // txtUsername
            // 
            this.txtUsername.Font = new System.Drawing.Font("Times New Roman", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtUsername.Location = new System.Drawing.Point(230, 160);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(431, 39);
            this.txtUsername.TabIndex = 28;
            // 
            // labelUsername
            // 
            this.labelUsername.AutoSize = true;
            this.labelUsername.Font = new System.Drawing.Font("Times New Roman", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelUsername.Location = new System.Drawing.Point(39, 168);
            this.labelUsername.Name = "labelUsername";
            this.labelUsername.Size = new System.Drawing.Size(175, 31);
            this.labelUsername.TabIndex = 27;
            this.labelUsername.Text = "Пользователь:";
            // 
            // txtDatabase
            // 
            this.txtDatabase.Font = new System.Drawing.Font("Times New Roman", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtDatabase.Location = new System.Drawing.Point(230, 112);
            this.txtDatabase.Name = "txtDatabase";
            this.txtDatabase.Size = new System.Drawing.Size(431, 39);
            this.txtDatabase.TabIndex = 26;
            // 
            // labelDatabase
            // 
            this.labelDatabase.AutoSize = true;
            this.labelDatabase.Font = new System.Drawing.Font("Times New Roman", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelDatabase.Location = new System.Drawing.Point(39, 120);
            this.labelDatabase.Name = "labelDatabase";
            this.labelDatabase.Size = new System.Drawing.Size(163, 31);
            this.labelDatabase.TabIndex = 25;
            this.labelDatabase.Text = "База данных:";
            // 
            // txtServer
            // 
            this.txtServer.Font = new System.Drawing.Font("Times New Roman", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtServer.Location = new System.Drawing.Point(231, 61);
            this.txtServer.Name = "txtServer";
            this.txtServer.Size = new System.Drawing.Size(431, 39);
            this.txtServer.TabIndex = 24;
            // 
            // labelServer
            // 
            this.labelServer.AutoSize = true;
            this.labelServer.Font = new System.Drawing.Font("Times New Roman", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelServer.Location = new System.Drawing.Point(40, 69);
            this.labelServer.Name = "labelServer";
            this.labelServer.Size = new System.Drawing.Size(102, 31);
            this.labelServer.TabIndex = 23;
            this.labelServer.Text = "Сервер:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Times New Roman", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.ForeColor = System.Drawing.Color.DarkSeaGreen;
            this.label1.Location = new System.Drawing.Point(336, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(192, 40);
            this.label1.TabIndex = 28;
            this.label1.Text = "Настройки";
            // 
            // SisAdminForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(854, 659);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tabControl1);
            this.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.Name = "SisAdminForm";
            this.Load += new System.EventHandler(this.SisAdminForm_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnTestConnection;
        private System.Windows.Forms.Button visible_password;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Label labelUsername;
        private System.Windows.Forms.TextBox txtDatabase;
        private System.Windows.Forms.Label labelDatabase;
        private System.Windows.Forms.TextBox txtServer;
        private System.Windows.Forms.Label labelServer;
        private System.Windows.Forms.Label label1;
    }
}