namespace Mir3DClientEditor.Dialogs
{
    partial class RePakFiles
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RePakFiles));
            topPanel = new Panel();
            RePakButton = new Button();
            RePakLabel = new Label();
            LoadExportsButton = new Button();
            LblStatus = new Label();
            RePakListView = new ListView();
            RePakAllButton = new Button();
            RePakProgressBar = new ProgressBar();
            topPanel.SuspendLayout();
            SuspendLayout();
            // 
            // topPanel
            // 
            topPanel.AutoSize = true;
            topPanel.Controls.Add(RePakProgressBar);
            topPanel.Controls.Add(RePakAllButton);
            topPanel.Controls.Add(RePakButton);
            topPanel.Controls.Add(RePakLabel);
            topPanel.Controls.Add(LoadExportsButton);
            topPanel.Controls.Add(LblStatus);
            topPanel.Dock = DockStyle.Top;
            topPanel.Location = new Point(0, 0);
            topPanel.Name = "topPanel";
            topPanel.Padding = new Padding(8);
            topPanel.Size = new Size(934, 57);
            topPanel.TabIndex = 4;
            // 
            // RePakButton
            // 
            RePakButton.AutoSize = true;
            RePakButton.Location = new Point(826, 15);
            RePakButton.Name = "RePakButton";
            RePakButton.Size = new Size(100, 25);
            RePakButton.TabIndex = 6;
            RePakButton.Text = "RePak Selected";
            RePakButton.UseVisualStyleBackColor = true;
            RePakButton.Click += RePakButton_Click;
            // 
            // RePakLabel
            // 
            RePakLabel.AutoSize = true;
            RePakLabel.Location = new Point(122, 21);
            RePakLabel.Name = "RePakLabel";
            RePakLabel.Size = new Size(0, 15);
            RePakLabel.TabIndex = 5;
            // 
            // LoadExportsButton
            // 
            LoadExportsButton.AutoSize = true;
            LoadExportsButton.Location = new Point(9, 15);
            LoadExportsButton.Name = "LoadExportsButton";
            LoadExportsButton.Size = new Size(100, 25);
            LoadExportsButton.TabIndex = 0;
            LoadExportsButton.Text = "Load Export/s";
            LoadExportsButton.UseVisualStyleBackColor = true;
            LoadExportsButton.Click += LoadExportsButton_Click;
            // 
            // LblStatus
            // 
            LblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            LblStatus.Location = new Point(2602, 26);
            LblStatus.Name = "LblStatus";
            LblStatus.Size = new Size(280, 23);
            LblStatus.TabIndex = 4;
            LblStatus.Text = "Select .pak files or a folder...";
            LblStatus.TextAlign = ContentAlignment.MiddleRight;
            // 
            // RePakListView
            // 
            RePakListView.BorderStyle = BorderStyle.None;
            RePakListView.Dock = DockStyle.Fill;
            RePakListView.GridLines = true;
            RePakListView.Location = new Point(0, 57);
            RePakListView.Name = "RePakListView";
            RePakListView.Size = new Size(934, 393);
            RePakListView.TabIndex = 5;
            RePakListView.UseCompatibleStateImageBehavior = false;
            RePakListView.View = View.Details;
            // 
            // RePakAllButton
            // 
            RePakAllButton.AutoSize = true;
            RePakAllButton.Location = new Point(720, 15);
            RePakAllButton.Name = "RePakAllButton";
            RePakAllButton.Size = new Size(100, 25);
            RePakAllButton.TabIndex = 7;
            RePakAllButton.Text = "RePak All";
            RePakAllButton.UseVisualStyleBackColor = true;
            RePakAllButton.Click += RePakAllButton_ClickAsync;
            // 
            // RePakProgressBar
            // 
            RePakProgressBar.Location = new Point(412, 17);
            RePakProgressBar.Name = "RePakProgressBar";
            RePakProgressBar.Size = new Size(302, 23);
            RePakProgressBar.TabIndex = 8;
            // 
            // RePakFiles
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(934, 450);
            Controls.Add(RePakListView);
            Controls.Add(topPanel);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "RePakFiles";
            Text = "Mir3D Client Editor - RePak";
            topPanel.ResumeLayout(false);
            topPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel topPanel;
        private Label RePakLabel;
        private Button LoadExportsButton;
        private Label LblStatus;
        private ListView RePakListView;
        private Button RePakButton;
        private Button RePakAllButton;
        private ProgressBar RePakProgressBar;
    }
}