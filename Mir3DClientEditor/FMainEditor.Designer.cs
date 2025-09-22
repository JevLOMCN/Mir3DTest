namespace Mir3DClientEditor
{
    partial class FMainEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FMainEditor));
            MainEditor = new MainEditorControl();
            SuspendLayout();
            // 
            // MainEditor
            // 
            MainEditor.Dock = DockStyle.Fill;
            MainEditor.Location = new Point(0, 0);
            MainEditor.Name = "MainEditor";
            MainEditor.Size = new Size(800, 450);
            MainEditor.TabIndex = 0;
            // 
            // FMainEditor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(MainEditor);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "FMainEditor";
            Text = "FMainEditor";
            ResumeLayout(false);

        }

        #endregion

        private MainEditorControl MainEditor;
    }
}