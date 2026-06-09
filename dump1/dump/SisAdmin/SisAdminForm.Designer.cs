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
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageBD = new System.Windows.Forms.TabPage();
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
            this.tabPageImport = new System.Windows.Forms.TabPage();
            this.tabPageCopy = new System.Windows.Forms.TabPage();
            this.tabPageSecure = new System.Windows.Forms.TabPage();
            this.btnSaveSecurity = new System.Windows.Forms.Button();
            this.btnCancelSecurity = new System.Windows.Forms.Button();
            this.chkAutoLock = new System.Windows.Forms.CheckBox();
            this.numInactivityTime = new System.Windows.Forms.NumericUpDown();
            this.lblInactivity = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnRestoreDB = new System.Windows.Forms.Button();
            this.lblWarning = new System.Windows.Forms.Label();
            this.txtLog = new System.Windows.Forms.RichTextBox();
            this.grpImport = new System.Windows.Forms.GroupBox();
            this.btnImport = new System.Windows.Forms.Button();
            this.btnBrowseImport = new System.Windows.Forms.Button();
            this.txtImportFilePath = new System.Windows.Forms.TextBox();
            this.lblFile = new System.Windows.Forms.Label();
            this.cmbTables = new System.Windows.Forms.ComboBox();
            this.lblTable = new System.Windows.Forms.Label();
            this.grpExport = new System.Windows.Forms.GroupBox();
            this.btnExport = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbExportTables = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.tabControl.SuspendLayout();
            this.tabPageBD.SuspendLayout();
            this.tabPageImport.SuspendLayout();
            this.tabPageCopy.SuspendLayout();
            this.tabPageSecure.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numInactivityTime)).BeginInit();
            this.grpImport.SuspendLayout();
            this.grpExport.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageBD);
            this.tabControl.Controls.Add(this.tabPageImport);
            this.tabControl.Controls.Add(this.tabPageCopy);
            this.tabControl.Controls.Add(this.tabPageSecure);
            this.tabControl.Location = new System.Drawing.Point(49, 75);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(760, 557);
            this.tabControl.TabIndex = 0;
            // 
            // tabPageBD
            // 
            this.tabPageBD.Controls.Add(this.lblStatus);
            this.tabPageBD.Controls.Add(this.btnSave);
            this.tabPageBD.Controls.Add(this.btnTestConnection);
            this.tabPageBD.Controls.Add(this.visible_password);
            this.tabPageBD.Controls.Add(this.txtPassword);
            this.tabPageBD.Controls.Add(this.labelPassword);
            this.tabPageBD.Controls.Add(this.txtUsername);
            this.tabPageBD.Controls.Add(this.labelUsername);
            this.tabPageBD.Controls.Add(this.txtDatabase);
            this.tabPageBD.Controls.Add(this.labelDatabase);
            this.tabPageBD.Controls.Add(this.txtServer);
            this.tabPageBD.Controls.Add(this.labelServer);
            this.tabPageBD.Location = new System.Drawing.Point(4, 30);
            this.tabPageBD.Name = "tabPageBD";
            this.tabPageBD.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageBD.Size = new System.Drawing.Size(752, 523);
            this.tabPageBD.TabIndex = 0;
            this.tabPageBD.Text = "База данных";
            this.tabPageBD.UseVisualStyleBackColor = true;
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
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
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
            this.btnTestConnection.Click += new System.EventHandler(this.btnTestConnection_Click);
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
            // tabPageImport
            // 
            this.tabPageImport.Controls.Add(this.grpExport);
            this.tabPageImport.Controls.Add(this.grpImport);
            this.tabPageImport.Location = new System.Drawing.Point(4, 30);
            this.tabPageImport.Name = "tabPageImport";
            this.tabPageImport.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageImport.Size = new System.Drawing.Size(752, 523);
            this.tabPageImport.TabIndex = 1;
            this.tabPageImport.Text = "Импорт/Экспорт";
            this.tabPageImport.UseVisualStyleBackColor = true;
            // 
            // tabPageCopy
            // 
            this.tabPageCopy.Controls.Add(this.btnRestoreDB);
            this.tabPageCopy.Controls.Add(this.txtLog);
            this.tabPageCopy.Controls.Add(this.lblWarning);
            this.tabPageCopy.Location = new System.Drawing.Point(4, 30);
            this.tabPageCopy.Name = "tabPageCopy";
            this.tabPageCopy.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageCopy.Size = new System.Drawing.Size(752, 523);
            this.tabPageCopy.TabIndex = 2;
            this.tabPageCopy.Text = "Резервное копирование/восстановление";
            this.tabPageCopy.UseVisualStyleBackColor = true;
            this.tabPageCopy.Click += new System.EventHandler(this.tabPageCopy_Click);
            // 
            // tabPageSecure
            // 
            this.tabPageSecure.Controls.Add(this.btnSaveSecurity);
            this.tabPageSecure.Controls.Add(this.btnCancelSecurity);
            this.tabPageSecure.Controls.Add(this.chkAutoLock);
            this.tabPageSecure.Controls.Add(this.numInactivityTime);
            this.tabPageSecure.Controls.Add(this.lblInactivity);
            this.tabPageSecure.Location = new System.Drawing.Point(4, 30);
            this.tabPageSecure.Name = "tabPageSecure";
            this.tabPageSecure.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSecure.Size = new System.Drawing.Size(752, 523);
            this.tabPageSecure.TabIndex = 3;
            this.tabPageSecure.Text = "Безопасность";
            this.tabPageSecure.UseVisualStyleBackColor = true;
            this.tabPageSecure.Click += new System.EventHandler(this.tabPageSecure_Click);
            // 
            // btnSaveSecurity
            // 
            this.btnSaveSecurity.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.btnSaveSecurity.Location = new System.Drawing.Point(453, 273);
            this.btnSaveSecurity.Name = "btnSaveSecurity";
            this.btnSaveSecurity.Size = new System.Drawing.Size(170, 44);
            this.btnSaveSecurity.TabIndex = 44;
            this.btnSaveSecurity.Text = "Сохранить";
            this.btnSaveSecurity.UseVisualStyleBackColor = false;
            // 
            // btnCancelSecurity
            // 
            this.btnCancelSecurity.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.btnCancelSecurity.Location = new System.Drawing.Point(453, 345);
            this.btnCancelSecurity.Name = "btnCancelSecurity";
            this.btnCancelSecurity.Size = new System.Drawing.Size(170, 44);
            this.btnCancelSecurity.TabIndex = 43;
            this.btnCancelSecurity.Text = "Отмена";
            this.btnCancelSecurity.UseVisualStyleBackColor = false;
            // 
            // chkAutoLock
            // 
            this.chkAutoLock.AutoSize = true;
            this.chkAutoLock.Font = new System.Drawing.Font("Times New Roman", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.chkAutoLock.Location = new System.Drawing.Point(141, 217);
            this.chkAutoLock.Name = "chkAutoLock";
            this.chkAutoLock.Size = new System.Drawing.Size(482, 35);
            this.chkAutoLock.TabIndex = 28;
            this.chkAutoLock.Text = "Включить автоматическую блокировку";
            this.chkAutoLock.UseVisualStyleBackColor = true;
            // 
            // numInactivityTime
            // 
            this.numInactivityTime.Location = new System.Drawing.Point(503, 152);
            this.numInactivityTime.Name = "numInactivityTime";
            this.numInactivityTime.Size = new System.Drawing.Size(120, 29);
            this.numInactivityTime.TabIndex = 27;
            // 
            // lblInactivity
            // 
            this.lblInactivity.AutoSize = true;
            this.lblInactivity.Font = new System.Drawing.Font("Times New Roman", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblInactivity.Location = new System.Drawing.Point(135, 152);
            this.lblInactivity.Name = "lblInactivity";
            this.lblInactivity.Size = new System.Drawing.Size(360, 31);
            this.lblInactivity.TabIndex = 26;
            this.lblInactivity.Text = "Время бездействия (секунды):";
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
            // btnRestoreDB
            // 
            this.btnRestoreDB.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.btnRestoreDB.Location = new System.Drawing.Point(194, 142);
            this.btnRestoreDB.Name = "btnRestoreDB";
            this.btnRestoreDB.Size = new System.Drawing.Size(361, 46);
            this.btnRestoreDB.TabIndex = 43;
            this.btnRestoreDB.Text = " Восстановить структуру БД";
            this.btnRestoreDB.UseVisualStyleBackColor = false;
            // 
            // lblWarning
            // 
            this.lblWarning.AutoSize = true;
            this.lblWarning.Location = new System.Drawing.Point(199, 104);
            this.lblWarning.Name = "lblWarning";
            this.lblWarning.Size = new System.Drawing.Size(356, 21);
            this.lblWarning.TabIndex = 0;
            this.lblWarning.Text = "ВНИМАНИЕ! Все данные будут потеряны!";
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(20, 288);
            this.txtLog.Name = "txtLog";
            this.txtLog.Size = new System.Drawing.Size(710, 193);
            this.txtLog.TabIndex = 1;
            this.txtLog.Text = "";
            // 
            // grpImport
            // 
            this.grpImport.Controls.Add(this.btnImport);
            this.grpImport.Controls.Add(this.btnBrowseImport);
            this.grpImport.Controls.Add(this.txtImportFilePath);
            this.grpImport.Controls.Add(this.lblFile);
            this.grpImport.Controls.Add(this.cmbTables);
            this.grpImport.Controls.Add(this.lblTable);
            this.grpImport.Location = new System.Drawing.Point(86, 19);
            this.grpImport.Margin = new System.Windows.Forms.Padding(5);
            this.grpImport.Name = "grpImport";
            this.grpImport.Padding = new System.Windows.Forms.Padding(5);
            this.grpImport.Size = new System.Drawing.Size(569, 211);
            this.grpImport.TabIndex = 29;
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
            // btnBrowseImport
            // 
            this.btnBrowseImport.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.btnBrowseImport.Location = new System.Drawing.Point(479, 65);
            this.btnBrowseImport.Name = "btnBrowseImport";
            this.btnBrowseImport.Size = new System.Drawing.Size(86, 29);
            this.btnBrowseImport.TabIndex = 43;
            this.btnBrowseImport.Text = "Обзор";
            this.btnBrowseImport.UseVisualStyleBackColor = false;
            // 
            // txtImportFilePath
            // 
            this.txtImportFilePath.Location = new System.Drawing.Point(112, 65);
            this.txtImportFilePath.Name = "txtImportFilePath";
            this.txtImportFilePath.Size = new System.Drawing.Size(361, 29);
            this.txtImportFilePath.TabIndex = 3;
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
            // grpExport
            // 
            this.grpExport.Controls.Add(this.btnExport);
            this.grpExport.Controls.Add(this.button2);
            this.grpExport.Controls.Add(this.textBox1);
            this.grpExport.Controls.Add(this.label2);
            this.grpExport.Controls.Add(this.cmbExportTables);
            this.grpExport.Controls.Add(this.label3);
            this.grpExport.Location = new System.Drawing.Point(86, 270);
            this.grpExport.Margin = new System.Windows.Forms.Padding(5);
            this.grpExport.Name = "grpExport";
            this.grpExport.Padding = new System.Windows.Forms.Padding(5);
            this.grpExport.Size = new System.Drawing.Size(569, 211);
            this.grpExport.TabIndex = 44;
            this.grpExport.TabStop = false;
            // 
            // btnExport
            // 
            this.btnExport.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.btnExport.Location = new System.Drawing.Point(112, 111);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(361, 46);
            this.btnExport.TabIndex = 43;
            this.btnExport.Text = "Экспортировать";
            this.btnExport.UseVisualStyleBackColor = false;
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.button2.Location = new System.Drawing.Point(479, 65);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(86, 29);
            this.button2.TabIndex = 43;
            this.button2.Text = "Обзор";
            this.button2.UseVisualStyleBackColor = false;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(112, 65);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(361, 29);
            this.textBox1.TabIndex = 3;

            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 73);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 21);
            this.label2.TabIndex = 2;
            this.label2.Text = "Файл:";
            // 
            // cmbExportTables
            // 
            this.cmbExportTables.FormattingEnabled = true;
            this.cmbExportTables.Location = new System.Drawing.Point(112, 20);
            this.cmbExportTables.Name = "cmbExportTables";
            this.cmbExportTables.Size = new System.Drawing.Size(361, 29);
            this.cmbExportTables.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 28);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(81, 21);
            this.label3.TabIndex = 0;
            this.label3.Text = "Таблица:";
            // 
            // SisAdminForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(854, 659);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tabControl);
            this.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SisAdminForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.SisAdminForm_Load);
            this.tabControl.ResumeLayout(false);
            this.tabPageBD.ResumeLayout(false);
            this.tabPageBD.PerformLayout();
            this.tabPageImport.ResumeLayout(false);
            this.tabPageCopy.ResumeLayout(false);
            this.tabPageCopy.PerformLayout();
            this.tabPageSecure.ResumeLayout(false);
            this.tabPageSecure.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numInactivityTime)).EndInit();
            this.grpImport.ResumeLayout(false);
            this.grpImport.PerformLayout();
            this.grpExport.ResumeLayout(false);
            this.grpExport.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageBD;
        private System.Windows.Forms.TabPage tabPageImport;
        private System.Windows.Forms.TabPage tabPageCopy;
        private System.Windows.Forms.TabPage tabPageSecure;
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
        private System.Windows.Forms.CheckBox chkAutoLock;
        private System.Windows.Forms.NumericUpDown numInactivityTime;
        private System.Windows.Forms.Label lblInactivity;
        private System.Windows.Forms.Button btnSaveSecurity;
        private System.Windows.Forms.Button btnCancelSecurity;
        private System.Windows.Forms.Button btnRestoreDB;
        private System.Windows.Forms.Label lblWarning;
        private System.Windows.Forms.RichTextBox txtLog;
        private System.Windows.Forms.GroupBox grpExport;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbExportTables;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox grpImport;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Button btnBrowseImport;
        private System.Windows.Forms.TextBox txtImportFilePath;
        private System.Windows.Forms.Label lblFile;
        private System.Windows.Forms.ComboBox cmbTables;
        private System.Windows.Forms.Label lblTable;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
    }
}