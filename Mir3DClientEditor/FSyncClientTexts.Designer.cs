namespace Mir3DClientEditor
{
    partial class FSyncClientTexts
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FSyncClientTexts));
            TextLatestClientPath = new TextBox();
            ButtonSelectLatestClientPath = new Button();
            label1 = new Label();
            label2 = new Label();
            ButtonSelectOldClientPath = new Button();
            TextOldClientPath = new TextBox();
            ButtonSyncronizeTexts = new Button();
            SuspendLayout();
            // 
            // TextLatestClientPath
            // 
            TextLatestClientPath.Location = new Point(12, 27);
            TextLatestClientPath.Name = "TextLatestClientPath";
            TextLatestClientPath.Size = new Size(374, 23);
            TextLatestClientPath.TabIndex = 0;
            // 
            // ButtonSelectLatestClientPath
            // 
            ButtonSelectLatestClientPath.Location = new Point(392, 27);
            ButtonSelectLatestClientPath.Name = "ButtonSelectLatestClientPath";
            ButtonSelectLatestClientPath.Size = new Size(75, 23);
            ButtonSelectLatestClientPath.TabIndex = 1;
            ButtonSelectLatestClientPath.Text = "Select Folder";
            ButtonSelectLatestClientPath.UseVisualStyleBackColor = true;
            ButtonSelectLatestClientPath.Click += ButtonSelectLatestClientPath_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(99, 15);
            label1.TabIndex = 2;
            label1.Text = "Latest Client Path";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 66);
            label2.Name = "label2";
            label2.Size = new Size(87, 15);
            label2.TabIndex = 5;
            label2.Text = "Old Client Path";
            // 
            // ButtonSelectOldClientPath
            // 
            ButtonSelectOldClientPath.Location = new Point(392, 84);
            ButtonSelectOldClientPath.Name = "ButtonSelectOldClientPath";
            ButtonSelectOldClientPath.Size = new Size(75, 23);
            ButtonSelectOldClientPath.TabIndex = 4;
            ButtonSelectOldClientPath.Text = "Select Folder";
            ButtonSelectOldClientPath.UseVisualStyleBackColor = true;
            ButtonSelectOldClientPath.Click += ButtonSelectOldClientPath_Click;
            // 
            // TextOldClientPath
            // 
            TextOldClientPath.Location = new Point(12, 84);
            TextOldClientPath.Name = "TextOldClientPath";
            TextOldClientPath.Size = new Size(374, 23);
            TextOldClientPath.TabIndex = 3;
            // 
            // ButtonSyncronizeTexts
            // 
            ButtonSyncronizeTexts.Location = new Point(12, 131);
            ButtonSyncronizeTexts.Name = "ButtonSyncronizeTexts";
            ButtonSyncronizeTexts.Size = new Size(455, 37);
            ButtonSyncronizeTexts.TabIndex = 6;
            ButtonSyncronizeTexts.Text = "Copy texts";
            ButtonSyncronizeTexts.UseVisualStyleBackColor = true;
            ButtonSyncronizeTexts.Click += ButtonSyncronizeTexts_Click;
            // 
            // FSyncClientTexts
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(479, 180);
            Controls.Add(ButtonSyncronizeTexts);
            Controls.Add(label2);
            Controls.Add(ButtonSelectOldClientPath);
            Controls.Add(TextOldClientPath);
            Controls.Add(label1);
            Controls.Add(ButtonSelectLatestClientPath);
            Controls.Add(TextLatestClientPath);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "FSyncClientTexts";
            Text = "FSyncClientTexts";
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private TextBox TextLatestClientPath;
        private Button ButtonSelectLatestClientPath;
        private Label label1;
        private Label label2;
        private Button ButtonSelectOldClientPath;
        private TextBox TextOldClientPath;
        private Button ButtonSyncronizeTexts;
    }
}