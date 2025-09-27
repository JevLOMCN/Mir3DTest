namespace Mir3DClientEditor.Dialogs
{
    partial class CompareFiles
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CompareFiles));
            topPanel = new Panel();
            CompareLabel = new Label();
            ComparePaksButton = new Button();
            CompareExportsButton = new Button();
            LblStatus = new Label();
            CompareListView = new ListView();
            topPanel.SuspendLayout();
            SuspendLayout();
            // 
            // topPanel
            // 
            topPanel.AutoSize = true;
            topPanel.Controls.Add(CompareLabel);
            topPanel.Controls.Add(ComparePaksButton);
            topPanel.Controls.Add(CompareExportsButton);
            topPanel.Controls.Add(LblStatus);
            topPanel.Dock = DockStyle.Top;
            topPanel.Location = new Point(0, 0);
            topPanel.Name = "topPanel";
            topPanel.Padding = new Padding(8);
            topPanel.Size = new Size(982, 49);
            topPanel.TabIndex = 3;
            // 
            // CompareLabel
            // 
            CompareLabel.AutoSize = true;
            CompareLabel.Location = new Point(235, 17);
            CompareLabel.Name = "CompareLabel";
            CompareLabel.Size = new Size(0, 15);
            CompareLabel.TabIndex = 5;
            // 
            // ComparePaksButton
            // 
            ComparePaksButton.AutoSize = true;
            ComparePaksButton.Location = new Point(12, 12);
            ComparePaksButton.Name = "ComparePaksButton";
            ComparePaksButton.Size = new Size(100, 25);
            ComparePaksButton.TabIndex = 0;
            ComparePaksButton.Text = "Compare .pak...";
            ComparePaksButton.UseVisualStyleBackColor = true;
            ComparePaksButton.Click += ComparePaksButton_Click;
            // 
            // CompareExportsButton
            // 
            CompareExportsButton.AutoSize = true;
            CompareExportsButton.Location = new Point(118, 12);
            CompareExportsButton.Name = "CompareExportsButton";
            CompareExportsButton.Size = new Size(111, 25);
            CompareExportsButton.TabIndex = 1;
            CompareExportsButton.Text = "Compare Export...";
            CompareExportsButton.UseVisualStyleBackColor = true;
            CompareExportsButton.Click += CompareExportsButton_Click;
            // 
            // LblStatus
            // 
            LblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            LblStatus.Location = new Point(2058, 18);
            LblStatus.Name = "LblStatus";
            LblStatus.Size = new Size(280, 23);
            LblStatus.TabIndex = 4;
            LblStatus.Text = "Select .pak files or a folder...";
            LblStatus.TextAlign = ContentAlignment.MiddleRight;
            // 
            // CompareListView
            // 
            CompareListView.BorderStyle = BorderStyle.None;
            CompareListView.Dock = DockStyle.Fill;
            CompareListView.FullRowSelect = true;
            CompareListView.GridLines = true;
            CompareListView.Location = new Point(0, 49);
            CompareListView.Name = "CompareListView";
            CompareListView.Size = new Size(982, 401);
            CompareListView.TabIndex = 4;
            CompareListView.UseCompatibleStateImageBehavior = false;
            CompareListView.View = View.Details;
            CompareListView.ItemActivate += CompareListView_ItemActivate;
            CompareListView.KeyDown += CompareListView_KeyDown;
            // 
            // CompareFiles
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(982, 450);
            Controls.Add(CompareListView);
            Controls.Add(topPanel);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "CompareFiles";
            Text = "Mir3D Client Editor - Compare";
            topPanel.ResumeLayout(false);
            topPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel topPanel;
        private Button ComparePaksButton;
        private Button CompareExportsButton;
        private Label LblStatus;
        private ListView CompareListView;
        private Label CompareLabel;
    }
}