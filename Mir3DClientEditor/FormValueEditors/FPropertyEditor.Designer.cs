namespace Mir3DClientEditor.FormValueEditors
{
    partial class FPropertyEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FPropertyEditor));
            GridEditor = new PropertyGridEditorControl();
            SuspendLayout();
            // 
            // GridEditor
            // 
            GridEditor.Dock = DockStyle.Fill;
            GridEditor.Location = new Point(0, 0);
            GridEditor.Name = "GridEditor";
            GridEditor.Size = new Size(882, 504);
            GridEditor.TabIndex = 0;
            // 
            // FPropertyEditor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(882, 504);
            Controls.Add(GridEditor);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "FPropertyEditor";
            Text = "FArrayEditor";
            ResumeLayout(false);

        }

        #endregion

        private PropertyGridEditorControl GridEditor;
    }
}