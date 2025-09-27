namespace GameLogin
{
    partial class ConfigForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigForm));
            ServerIPLabel = new System.Windows.Forms.Label();
            ServerPortLabel = new System.Windows.Forms.Label();
            ServerNameLabel = new System.Windows.Forms.Label();
            AccountNameLabel = new System.Windows.Forms.Label();
            ServerIPBox = new System.Windows.Forms.TextBox();
            ServerPortBox = new System.Windows.Forms.TextBox();
            AccountNameBox = new System.Windows.Forms.TextBox();
            ServerNameBox = new System.Windows.Forms.TextBox();
            SaveButton = new System.Windows.Forms.Button();
            Return_To_Login_Button = new System.Windows.Forms.Button();
            EnableAutoPatcherCheckBox = new System.Windows.Forms.CheckBox();
            KeepAutoPatcherBackupsCheckbox = new System.Windows.Forms.CheckBox();
            PatchURLBox = new System.Windows.Forms.TextBox();
            PatchURLLabel = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // ServerIPLabel
            // 
            ServerIPLabel.AutoSize = true;
            ServerIPLabel.BackColor = System.Drawing.Color.Transparent;
            ServerIPLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            ServerIPLabel.ForeColor = System.Drawing.Color.White;
            ServerIPLabel.Location = new System.Drawing.Point(36, 115);
            ServerIPLabel.Name = "ServerIPLabel";
            ServerIPLabel.Size = new System.Drawing.Size(24, 19);
            ServerIPLabel.TabIndex = 0;
            ServerIPLabel.Text = "IP:";
            // 
            // ServerPortLabel
            // 
            ServerPortLabel.AutoSize = true;
            ServerPortLabel.BackColor = System.Drawing.Color.Transparent;
            ServerPortLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            ServerPortLabel.ForeColor = System.Drawing.Color.White;
            ServerPortLabel.Location = new System.Drawing.Point(36, 161);
            ServerPortLabel.Name = "ServerPortLabel";
            ServerPortLabel.Size = new System.Drawing.Size(37, 19);
            ServerPortLabel.TabIndex = 1;
            ServerPortLabel.Text = "Port:";
            // 
            // ServerNameLabel
            // 
            ServerNameLabel.AutoSize = true;
            ServerNameLabel.BackColor = System.Drawing.Color.Transparent;
            ServerNameLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            ServerNameLabel.ForeColor = System.Drawing.Color.White;
            ServerNameLabel.Location = new System.Drawing.Point(36, 242);
            ServerNameLabel.Name = "ServerNameLabel";
            ServerNameLabel.Size = new System.Drawing.Size(90, 19);
            ServerNameLabel.TabIndex = 3;
            ServerNameLabel.Text = "Server Name:";
            // 
            // AccountNameLabel
            // 
            AccountNameLabel.AutoSize = true;
            AccountNameLabel.BackColor = System.Drawing.Color.Transparent;
            AccountNameLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            AccountNameLabel.ForeColor = System.Drawing.Color.White;
            AccountNameLabel.Location = new System.Drawing.Point(36, 204);
            AccountNameLabel.Name = "AccountNameLabel";
            AccountNameLabel.Size = new System.Drawing.Size(62, 19);
            AccountNameLabel.TabIndex = 2;
            AccountNameLabel.Text = "Account:";
            // 
            // ServerIPBox
            // 
            ServerIPBox.BackColor = System.Drawing.Color.FromArgb(36, 36, 38);
            ServerIPBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            ServerIPBox.Font = new System.Drawing.Font("Segoe UI", 15F);
            ServerIPBox.ForeColor = System.Drawing.Color.White;
            ServerIPBox.Location = new System.Drawing.Point(65, 111);
            ServerIPBox.Margin = new System.Windows.Forms.Padding(0);
            ServerIPBox.Name = "ServerIPBox";
            ServerIPBox.PlaceholderText = "127.0.0.1";
            ServerIPBox.Size = new System.Drawing.Size(185, 27);
            ServerIPBox.TabIndex = 4;
            // 
            // ServerPortBox
            // 
            ServerPortBox.BackColor = System.Drawing.Color.FromArgb(36, 36, 38);
            ServerPortBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            ServerPortBox.Font = new System.Drawing.Font("Segoe UI", 15F);
            ServerPortBox.ForeColor = System.Drawing.Color.White;
            ServerPortBox.Location = new System.Drawing.Point(79, 157);
            ServerPortBox.Margin = new System.Windows.Forms.Padding(0);
            ServerPortBox.Name = "ServerPortBox";
            ServerPortBox.PlaceholderText = "8000";
            ServerPortBox.Size = new System.Drawing.Size(78, 27);
            ServerPortBox.TabIndex = 5;
            ServerPortBox.KeyPress += ServerPortBox_KeyPress;
            // 
            // AccountNameBox
            // 
            AccountNameBox.BackColor = System.Drawing.Color.FromArgb(36, 36, 38);
            AccountNameBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            AccountNameBox.Font = new System.Drawing.Font("Segoe UI", 15F);
            AccountNameBox.ForeColor = System.Drawing.Color.White;
            AccountNameBox.Location = new System.Drawing.Point(103, 201);
            AccountNameBox.Margin = new System.Windows.Forms.Padding(0);
            AccountNameBox.Name = "AccountNameBox";
            AccountNameBox.Size = new System.Drawing.Size(147, 27);
            AccountNameBox.TabIndex = 6;
            // 
            // ServerNameBox
            // 
            ServerNameBox.BackColor = System.Drawing.Color.FromArgb(36, 36, 38);
            ServerNameBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            ServerNameBox.Font = new System.Drawing.Font("Segoe UI", 12F);
            ServerNameBox.ForeColor = System.Drawing.Color.White;
            ServerNameBox.Location = new System.Drawing.Point(129, 242);
            ServerNameBox.Margin = new System.Windows.Forms.Padding(0);
            ServerNameBox.Name = "ServerNameBox";
            ServerNameBox.PlaceholderText = "Legend_Eternal";
            ServerNameBox.Size = new System.Drawing.Size(121, 22);
            ServerNameBox.TabIndex = 7;
            // 
            // SaveButton
            // 
            SaveButton.BackColor = System.Drawing.Color.FromArgb(230, 80, 80);
            SaveButton.FlatAppearance.BorderSize = 0;
            SaveButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            SaveButton.Font = new System.Drawing.Font("Microsoft YaHei", 12F);
            SaveButton.ForeColor = System.Drawing.Color.White;
            SaveButton.Location = new System.Drawing.Point(42, 378);
            SaveButton.Name = "SaveButton";
            SaveButton.Size = new System.Drawing.Size(213, 30);
            SaveButton.TabIndex = 8;
            SaveButton.Text = "Save";
            SaveButton.UseVisualStyleBackColor = false;
            SaveButton.Click += SaveButton_Click;
            // 
            // Return_To_Login_Button
            // 
            Return_To_Login_Button.BackColor = System.Drawing.Color.FromArgb(230, 80, 80);
            Return_To_Login_Button.FlatAppearance.BorderSize = 0;
            Return_To_Login_Button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            Return_To_Login_Button.Font = new System.Drawing.Font("Microsoft YaHei", 12F);
            Return_To_Login_Button.ForeColor = System.Drawing.Color.White;
            Return_To_Login_Button.Location = new System.Drawing.Point(42, 414);
            Return_To_Login_Button.Name = "Return_To_Login_Button";
            Return_To_Login_Button.Size = new System.Drawing.Size(213, 30);
            Return_To_Login_Button.TabIndex = 9;
            Return_To_Login_Button.Text = "Return to Login";
            Return_To_Login_Button.UseVisualStyleBackColor = false;
            Return_To_Login_Button.Click += Return_To_Login_Button_Click;
            // 
            // EnableAutoPatcherCheckBox
            // 
            EnableAutoPatcherCheckBox.AutoSize = true;
            EnableAutoPatcherCheckBox.BackColor = System.Drawing.Color.Transparent;
            EnableAutoPatcherCheckBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            EnableAutoPatcherCheckBox.ForeColor = System.Drawing.Color.White;
            EnableAutoPatcherCheckBox.Location = new System.Drawing.Point(40, 311);
            EnableAutoPatcherCheckBox.Name = "EnableAutoPatcherCheckBox";
            EnableAutoPatcherCheckBox.Size = new System.Drawing.Size(148, 23);
            EnableAutoPatcherCheckBox.TabIndex = 10;
            EnableAutoPatcherCheckBox.Text = "Enable AutoPatcher";
            EnableAutoPatcherCheckBox.UseVisualStyleBackColor = false;
            // 
            // KeepAutoPatcherBackupsCheckbox
            // 
            KeepAutoPatcherBackupsCheckbox.AutoSize = true;
            KeepAutoPatcherBackupsCheckbox.BackColor = System.Drawing.Color.Transparent;
            KeepAutoPatcherBackupsCheckbox.Font = new System.Drawing.Font("Segoe UI", 10F);
            KeepAutoPatcherBackupsCheckbox.ForeColor = System.Drawing.Color.White;
            KeepAutoPatcherBackupsCheckbox.Location = new System.Drawing.Point(40, 340);
            KeepAutoPatcherBackupsCheckbox.Name = "KeepAutoPatcherBackupsCheckbox";
            KeepAutoPatcherBackupsCheckbox.Size = new System.Drawing.Size(192, 23);
            KeepAutoPatcherBackupsCheckbox.TabIndex = 11;
            KeepAutoPatcherBackupsCheckbox.Text = "Keep AutoPatcher Backups";
            KeepAutoPatcherBackupsCheckbox.UseVisualStyleBackColor = false;
            // 
            // PatchURLBox
            // 
            PatchURLBox.BackColor = System.Drawing.Color.FromArgb(36, 36, 38);
            PatchURLBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            PatchURLBox.Font = new System.Drawing.Font("Segoe UI", 12F);
            PatchURLBox.ForeColor = System.Drawing.Color.White;
            PatchURLBox.Location = new System.Drawing.Point(114, 277);
            PatchURLBox.Margin = new System.Windows.Forms.Padding(0);
            PatchURLBox.Name = "PatchURLBox";
            PatchURLBox.PlaceholderText = "https://";
            PatchURLBox.Size = new System.Drawing.Size(136, 22);
            PatchURLBox.TabIndex = 13;
            // 
            // PatchURLLabel
            // 
            PatchURLLabel.AutoSize = true;
            PatchURLLabel.BackColor = System.Drawing.Color.Transparent;
            PatchURLLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            PatchURLLabel.ForeColor = System.Drawing.Color.White;
            PatchURLLabel.Location = new System.Drawing.Point(36, 277);
            PatchURLLabel.Name = "PatchURLLabel";
            PatchURLLabel.Size = new System.Drawing.Size(75, 19);
            PatchURLLabel.TabIndex = 12;
            PatchURLLabel.Text = "Patch URL:";
            // 
            // ConfigForm
            // 
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            BackgroundImage = (System.Drawing.Image)resources.GetObject("$this.BackgroundImage");
            ClientSize = new System.Drawing.Size(300, 486);
            Controls.Add(PatchURLBox);
            Controls.Add(PatchURLLabel);
            Controls.Add(KeepAutoPatcherBackupsCheckbox);
            Controls.Add(EnableAutoPatcherCheckBox);
            Controls.Add(Return_To_Login_Button);
            Controls.Add(SaveButton);
            Controls.Add(ServerNameBox);
            Controls.Add(AccountNameBox);
            Controls.Add(ServerPortBox);
            Controls.Add(ServerIPBox);
            Controls.Add(ServerNameLabel);
            Controls.Add(AccountNameLabel);
            Controls.Add(ServerPortLabel);
            Controls.Add(ServerIPLabel);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ConfigForm";
            ShowIcon = false;
            FormClosing += ConfigForm_FormClosing;
            Load += ConfigForm_Load;
            MouseDown += Tab_MouseDown;
            MouseMove += Tab_MouseMove;
            MouseUp += Tab_MouseUp;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label ServerIPLabel;
        private System.Windows.Forms.Label ServerPortLabel;
        private System.Windows.Forms.Label ServerNameLabel;
        private System.Windows.Forms.Label AccountNameLabel;
        private System.Windows.Forms.TextBox ServerIPBox;
        private System.Windows.Forms.TextBox ServerPortBox;
        private System.Windows.Forms.TextBox AccountNameBox;
        private System.Windows.Forms.TextBox ServerNameBox;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.Button Return_To_Login_Button;
        private System.Windows.Forms.CheckBox EnableAutoPatcherCheckBox;
        private System.Windows.Forms.CheckBox KeepAutoPatcherBackupsCheckbox;
        private System.Windows.Forms.TextBox PatchURLBox;
        private System.Windows.Forms.Label PatchURLLabel;
    }
}