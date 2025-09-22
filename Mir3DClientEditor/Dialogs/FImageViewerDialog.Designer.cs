namespace Mir3DClientEditor.Dialogs
{
    partial class FImageViewerDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FImageViewerDialog));
            MainLayout = new SplitContainer();
            ActiveImage = new PictureBox();
            ButtonExportImage = new Button();
            ReplaceImageButton = new Button();
            BtnNextMipmap = new Button();
            LblCurrentImage = new Label();
            BtnPrevMipmap = new Button();
            ((System.ComponentModel.ISupportInitialize)MainLayout).BeginInit();
            MainLayout.Panel1.SuspendLayout();
            MainLayout.Panel2.SuspendLayout();
            MainLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)ActiveImage).BeginInit();
            SuspendLayout();
            // 
            // MainLayout
            // 
            MainLayout.Dock = DockStyle.Fill;
            MainLayout.FixedPanel = FixedPanel.Panel2;
            MainLayout.Location = new Point(0, 0);
            MainLayout.Name = "MainLayout";
            MainLayout.Orientation = Orientation.Horizontal;
            // 
            // MainLayout.Panel1
            // 
            MainLayout.Panel1.AutoScroll = true;
            MainLayout.Panel1.Controls.Add(ActiveImage);
            // 
            // MainLayout.Panel2
            // 
            MainLayout.Panel2.Controls.Add(ButtonExportImage);
            MainLayout.Panel2.Controls.Add(ReplaceImageButton);
            MainLayout.Panel2.Controls.Add(BtnNextMipmap);
            MainLayout.Panel2.Controls.Add(LblCurrentImage);
            MainLayout.Panel2.Controls.Add(BtnPrevMipmap);
            MainLayout.Size = new Size(727, 482);
            MainLayout.SplitterDistance = 439;
            MainLayout.TabIndex = 0;
            // 
            // ActiveImage
            // 
            ActiveImage.Location = new Point(0, 0);
            ActiveImage.Name = "ActiveImage";
            ActiveImage.Size = new Size(248, 168);
            ActiveImage.TabIndex = 0;
            ActiveImage.TabStop = false;
            // 
            // ButtonExportImage
            // 
            ButtonExportImage.Location = new Point(232, 7);
            ButtonExportImage.Name = "ButtonExportImage";
            ButtonExportImage.Size = new Size(108, 23);
            ButtonExportImage.TabIndex = 4;
            ButtonExportImage.Text = "Export Image";
            ButtonExportImage.UseVisualStyleBackColor = true;
            ButtonExportImage.Click += ButtonExportImage_Click;
            // 
            // ReplaceImageButton
            // 
            ReplaceImageButton.Location = new Point(118, 7);
            ReplaceImageButton.Name = "ReplaceImageButton";
            ReplaceImageButton.Size = new Size(108, 23);
            ReplaceImageButton.TabIndex = 3;
            ReplaceImageButton.Text = "Replace Image";
            ReplaceImageButton.UseVisualStyleBackColor = true;
            ReplaceImageButton.Click += ReplaceImageButton_Click;
            // 
            // BtnNextMipmap
            // 
            BtnNextMipmap.Location = new Point(80, 7);
            BtnNextMipmap.Name = "BtnNextMipmap";
            BtnNextMipmap.Size = new Size(32, 23);
            BtnNextMipmap.TabIndex = 2;
            BtnNextMipmap.Text = ">";
            BtnNextMipmap.UseVisualStyleBackColor = true;
            BtnNextMipmap.Click += BtnNextMipmap_Click;
            // 
            // LblCurrentImage
            // 
            LblCurrentImage.AutoSize = true;
            LblCurrentImage.Location = new Point(50, 11);
            LblCurrentImage.Name = "LblCurrentImage";
            LblCurrentImage.Size = new Size(24, 15);
            LblCurrentImage.TabIndex = 1;
            LblCurrentImage.Text = "1/1";
            // 
            // BtnPrevMipmap
            // 
            BtnPrevMipmap.Location = new Point(12, 7);
            BtnPrevMipmap.Name = "BtnPrevMipmap";
            BtnPrevMipmap.Size = new Size(32, 23);
            BtnPrevMipmap.TabIndex = 0;
            BtnPrevMipmap.Text = "<";
            BtnPrevMipmap.UseVisualStyleBackColor = true;
            BtnPrevMipmap.Click += BtnPrevMipmap_Click;
            // 
            // FImageViewerDialog
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(727, 482);
            Controls.Add(MainLayout);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "FImageViewerDialog";
            Text = "FImageViewerDialog";
            MainLayout.Panel1.ResumeLayout(false);
            MainLayout.Panel2.ResumeLayout(false);
            MainLayout.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)MainLayout).EndInit();
            MainLayout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)ActiveImage).EndInit();
            ResumeLayout(false);

        }

        #endregion

        private SplitContainer MainLayout;
        private PictureBox ActiveImage;
        private Button button1;
        private Button BtnPrevMipmap;
        private Button BtnNextMipmap;
        private Label LblCurrentImage;
        private Button ReplaceImageButton;
        private Button ButtonExportImage;
    }
}