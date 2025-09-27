using System.Drawing;
using System.Windows.Forms;

namespace Mir3DClientEditor.Dialogs
{
    partial class ExportFiles
    {
        private System.ComponentModel.IContainer components = null;

        // Designer fields (must be fields, not locals)
        private Panel topPanel;
        private Panel bottomPanel;
        private Button BtnSelectPak;
        private Button BtnSelectFolder;
        private Label LblFilter;
        private TextBox TxtFilter;
        private Label LblStatus;

        private ListView FilesListView;
        private ColumnHeader ColFile;
        private ColumnHeader ColPak;
        private ColumnHeader ColSize;

        private Button ExportBtn;
        private ProgressBar progressBar1;
        private Label LblActiveFile;
        private Button BtnClose;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportFiles));
            topPanel = new Panel();
            BtnSelectPak = new Button();
            CreateStandalonePListButton = new Button();
            BtnSelectFolder = new Button();
            LblFilter = new Label();
            TxtFilter = new TextBox();
            LblStatus = new Label();
            ExportPListCheckBox = new CheckBox();
            bottomPanel = new Panel();
            ExportLabel = new Label();
            ExportBtn = new Button();
            progressBar1 = new ProgressBar();
            LblActiveFile = new Label();
            BtnClose = new Button();
            FilesListView = new ListView();
            ColFile = new ColumnHeader();
            ColPak = new ColumnHeader();
            ColSize = new ColumnHeader();
            topPanel.SuspendLayout();
            bottomPanel.SuspendLayout();
            SuspendLayout();
            // 
            // topPanel
            // 
            topPanel.AutoSize = true;
            topPanel.Controls.Add(BtnSelectPak);
            topPanel.Controls.Add(CreateStandalonePListButton);
            topPanel.Controls.Add(BtnSelectFolder);
            topPanel.Controls.Add(LblFilter);
            topPanel.Controls.Add(TxtFilter);
            topPanel.Controls.Add(LblStatus);
            topPanel.Dock = DockStyle.Top;
            topPanel.Location = new Point(0, 0);
            topPanel.Name = "topPanel";
            topPanel.Padding = new Padding(8);
            topPanel.Size = new Size(884, 48);
            topPanel.TabIndex = 2;
            // 
            // BtnSelectPak
            // 
            BtnSelectPak.AutoSize = true;
            BtnSelectPak.Location = new Point(8, 10);
            BtnSelectPak.Name = "BtnSelectPak";
            BtnSelectPak.Size = new Size(90, 25);
            BtnSelectPak.TabIndex = 0;
            BtnSelectPak.Text = "Select .pak...";
            BtnSelectPak.UseVisualStyleBackColor = true;
            BtnSelectPak.Click += BtnSelectPak_Click;
            // 
            // CreateStandalonePListButton
            // 
            CreateStandalonePListButton.Location = new Point(729, 14);
            CreateStandalonePListButton.Name = "CreateStandalonePListButton";
            CreateStandalonePListButton.Size = new Size(143, 23);
            CreateStandalonePListButton.TabIndex = 5;
            CreateStandalonePListButton.Text = "Create Standalone PList";
            CreateStandalonePListButton.UseVisualStyleBackColor = true;
            CreateStandalonePListButton.Click += CreateStandalonePListButton_Click;
            // 
            // BtnSelectFolder
            // 
            BtnSelectFolder.AutoSize = true;
            BtnSelectFolder.Location = new Point(108, 10);
            BtnSelectFolder.Name = "BtnSelectFolder";
            BtnSelectFolder.Size = new Size(100, 25);
            BtnSelectFolder.TabIndex = 1;
            BtnSelectFolder.Text = "Select Folder...";
            BtnSelectFolder.UseVisualStyleBackColor = true;
            BtnSelectFolder.Click += BtnSelectFolder_Click;
            // 
            // LblFilter
            // 
            LblFilter.AutoSize = true;
            LblFilter.Location = new Point(220, 14);
            LblFilter.Name = "LblFilter";
            LblFilter.Size = new Size(36, 15);
            LblFilter.TabIndex = 2;
            LblFilter.Text = "Filter:";
            // 
            // TxtFilter
            // 
            TxtFilter.Location = new Point(260, 10);
            TxtFilter.Name = "TxtFilter";
            TxtFilter.Size = new Size(320, 23);
            TxtFilter.TabIndex = 3;
            TxtFilter.TextChanged += TxtFilter_TextChanged;
            // 
            // LblStatus
            // 
            LblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            LblStatus.Location = new Point(1284, 10);
            LblStatus.Name = "LblStatus";
            LblStatus.Size = new Size(280, 23);
            LblStatus.TabIndex = 4;
            LblStatus.Text = "Select .pak files or a folder...";
            LblStatus.TextAlign = ContentAlignment.MiddleRight;
            // 
            // ExportPListCheckBox
            // 
            ExportPListCheckBox.AutoSize = true;
            ExportPListCheckBox.Location = new Point(8, 41);
            ExportPListCheckBox.Name = "ExportPListCheckBox";
            ExportPListCheckBox.Size = new Size(162, 19);
            ExportPListCheckBox.TabIndex = 5;
            ExportPListCheckBox.Text = "Create PList during Export";
            ExportPListCheckBox.UseVisualStyleBackColor = true;
            // 
            // bottomPanel
            // 
            bottomPanel.AutoSize = true;
            bottomPanel.Controls.Add(ExportPListCheckBox);
            bottomPanel.Controls.Add(ExportLabel);
            bottomPanel.Controls.Add(ExportBtn);
            bottomPanel.Controls.Add(progressBar1);
            bottomPanel.Controls.Add(LblActiveFile);
            bottomPanel.Controls.Add(BtnClose);
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Location = new Point(0, 495);
            bottomPanel.Name = "bottomPanel";
            bottomPanel.Padding = new Padding(8);
            bottomPanel.Size = new Size(884, 71);
            bottomPanel.TabIndex = 1;
            // 
            // ExportLabel
            // 
            ExportLabel.AutoSize = true;
            ExportLabel.Location = new Point(176, 41);
            ExportLabel.Name = "ExportLabel";
            ExportLabel.Size = new Size(0, 15);
            ExportLabel.TabIndex = 4;
            // 
            // ExportBtn
            // 
            ExportBtn.AutoSize = true;
            ExportBtn.Location = new Point(8, 10);
            ExportBtn.Name = "ExportBtn";
            ExportBtn.Size = new Size(75, 25);
            ExportBtn.TabIndex = 0;
            ExportBtn.Text = "Export";
            ExportBtn.UseVisualStyleBackColor = true;
            ExportBtn.Click += ExportBtn_Click;
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(90, 12);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(300, 20);
            progressBar1.TabIndex = 1;
            progressBar1.Visible = false;
            // 
            // LblActiveFile
            // 
            LblActiveFile.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            LblActiveFile.Location = new Point(1094, 10);
            LblActiveFile.Name = "LblActiveFile";
            LblActiveFile.Size = new Size(380, 23);
            LblActiveFile.TabIndex = 2;
            LblActiveFile.TextAlign = ContentAlignment.MiddleRight;
            // 
            // BtnClose
            // 
            BtnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            BtnClose.AutoSize = true;
            BtnClose.Location = new Point(1484, 10);
            BtnClose.Name = "BtnClose";
            BtnClose.Size = new Size(75, 25);
            BtnClose.TabIndex = 3;
            BtnClose.Text = "Close";
            BtnClose.UseVisualStyleBackColor = true;
            BtnClose.Click += BtnClose_Click;
            // 
            // FilesListView
            // 
            FilesListView.Columns.AddRange(new ColumnHeader[] { ColFile, ColPak, ColSize });
            FilesListView.Dock = DockStyle.Fill;
            FilesListView.FullRowSelect = true;
            FilesListView.GridLines = true;
            FilesListView.Location = new Point(0, 48);
            FilesListView.Name = "FilesListView";
            FilesListView.Size = new Size(884, 447);
            FilesListView.TabIndex = 0;
            FilesListView.UseCompatibleStateImageBehavior = false;
            FilesListView.View = View.Details;
            FilesListView.ColumnClick += FilesListView_ColumnClick;
            FilesListView.ItemActivate += FilesListView_ItemActivate;
            // 
            // ColFile
            // 
            ColFile.Text = "File";
            ColFile.Width = 520;
            // 
            // ColPak
            // 
            ColPak.Text = "Pak";
            ColPak.Width = 260;
            // 
            // ColSize
            // 
            ColSize.Text = "Size";
            ColSize.TextAlign = HorizontalAlignment.Right;
            ColSize.Width = 100;
            // 
            // ExportFiles
            // 
            ClientSize = new Size(884, 566);
            Controls.Add(FilesListView);
            Controls.Add(bottomPanel);
            Controls.Add(topPanel);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(900, 600);
            Name = "ExportFiles";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Mir3D Client Editor - Export";
            FormClosing += OnFormClosed;
            topPanel.ResumeLayout(false);
            topPanel.PerformLayout();
            bottomPanel.ResumeLayout(false);
            bottomPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
        #endregion

        private CheckBox ExportPListCheckBox;
        private Label ExportLabel;
        private Button CreateStandalonePListButton;
    }
}