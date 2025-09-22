namespace Mir3DClientEditor
{
    partial class FSearch
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FSearch));
            SearchText = new TextBox();
            SuspendLayout();
            // 
            // SearchText
            // 
            SearchText.BorderStyle = BorderStyle.None;
            SearchText.Dock = DockStyle.Fill;
            SearchText.Font = new Font("Segoe UI", 12F);
            SearchText.Location = new Point(0, 0);
            SearchText.Name = "SearchText";
            SearchText.Size = new Size(305, 22);
            SearchText.TabIndex = 0;
            // 
            // FSearch
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(305, 22);
            Controls.Add(SearchText);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "FSearch";
            Text = "Search";
            Load += FSearch_Load;
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private TextBox SearchText;
    }
}