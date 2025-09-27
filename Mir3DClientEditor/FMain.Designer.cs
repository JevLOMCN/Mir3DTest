namespace Mir3DClientEditor
{
    partial class FMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FMain));
            Menu = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            Menu_File_OpenGameFolder = new ToolStripMenuItem();
            Menu_OpenFile = new ToolStripMenuItem();
            Menu_Save = new ToolStripMenuItem();
            Menu_File_SaveAs = new ToolStripMenuItem();
            toolsToolStripMenuItem = new ToolStripMenuItem();
            decryptFolderToolStripMenuItem = new ToolStripMenuItem();
            syncClientTextsToolStripMenuItem = new ToolStripMenuItem();
            locateUPKNameToolStripMenuItem = new ToolStripMenuItem();
            exportToolStripMenuItem = new ToolStripMenuItem();
            compareToolStripMenuItem = new ToolStripMenuItem();
            rePakToolStripMenuItem = new ToolStripMenuItem();
            donateToolStripMenuItem = new ToolStripMenuItem();
            LblActiveFile = new ToolStripStatusLabel();
            statusStrip1 = new StatusStrip();
            MainEditor = new MainEditorControl();
            FilePathTextBox = new TextBox();
            Menu.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // Menu
            // 
            Menu.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, toolsToolStripMenuItem, donateToolStripMenuItem });
            Menu.Location = new Point(0, 0);
            Menu.Name = "Menu";
            Menu.Size = new Size(1272, 24);
            Menu.TabIndex = 0;
            Menu.Text = "Menu";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { Menu_File_OpenGameFolder, Menu_OpenFile, Menu_Save, Menu_File_SaveAs });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // Menu_File_OpenGameFolder
            // 
            Menu_File_OpenGameFolder.Name = "Menu_File_OpenGameFolder";
            Menu_File_OpenGameFolder.Size = new Size(173, 22);
            Menu_File_OpenGameFolder.Text = "Open Game Folder";
            Menu_File_OpenGameFolder.Click += Menu_File_OpenGameFolder_Click;
            // 
            // Menu_OpenFile
            // 
            Menu_OpenFile.Name = "Menu_OpenFile";
            Menu_OpenFile.Size = new Size(173, 22);
            Menu_OpenFile.Text = "Open";
            Menu_OpenFile.Click += Menu_OpenFile_Click;
            // 
            // Menu_Save
            // 
            Menu_Save.Name = "Menu_Save";
            Menu_Save.Size = new Size(173, 22);
            Menu_Save.Text = "Save";
            Menu_Save.Click += Menu_Save_Click;
            // 
            // Menu_File_SaveAs
            // 
            Menu_File_SaveAs.Name = "Menu_File_SaveAs";
            Menu_File_SaveAs.Size = new Size(173, 22);
            Menu_File_SaveAs.Text = "Save As";
            Menu_File_SaveAs.Click += Menu_File_SaveAs_Click;
            // 
            // toolsToolStripMenuItem
            // 
            toolsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { decryptFolderToolStripMenuItem, syncClientTextsToolStripMenuItem, locateUPKNameToolStripMenuItem, exportToolStripMenuItem, compareToolStripMenuItem, rePakToolStripMenuItem });
            toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            toolsToolStripMenuItem.Size = new Size(47, 20);
            toolsToolStripMenuItem.Text = "Tools";
            // 
            // decryptFolderToolStripMenuItem
            // 
            decryptFolderToolStripMenuItem.Name = "decryptFolderToolStripMenuItem";
            decryptFolderToolStripMenuItem.Size = new Size(180, 22);
            decryptFolderToolStripMenuItem.Text = "Decrypt Folder";
            // 
            // syncClientTextsToolStripMenuItem
            // 
            syncClientTextsToolStripMenuItem.Name = "syncClientTextsToolStripMenuItem";
            syncClientTextsToolStripMenuItem.Size = new Size(180, 22);
            syncClientTextsToolStripMenuItem.Text = "Sync Client Texts";
            syncClientTextsToolStripMenuItem.Click += SyncClientTextsToolStripMenuItem_Click;
            // 
            // locateUPKNameToolStripMenuItem
            // 
            locateUPKNameToolStripMenuItem.Name = "locateUPKNameToolStripMenuItem";
            locateUPKNameToolStripMenuItem.Size = new Size(180, 22);
            locateUPKNameToolStripMenuItem.Text = "Locate UPK Name";
            locateUPKNameToolStripMenuItem.Click += locateUPKNameToolStripMenuItem_Click;
            // 
            // exportToolStripMenuItem
            // 
            exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            exportToolStripMenuItem.Size = new Size(180, 22);
            exportToolStripMenuItem.Text = "Export";
            exportToolStripMenuItem.Click += exportToolStripMenuItem_Click;
            // 
            // compareToolStripMenuItem
            // 
            compareToolStripMenuItem.Name = "compareToolStripMenuItem";
            compareToolStripMenuItem.Size = new Size(180, 22);
            compareToolStripMenuItem.Text = "Compare";
            compareToolStripMenuItem.Click += compareToolStripMenuItem_Click;
            // 
            // rePakToolStripMenuItem
            // 
            rePakToolStripMenuItem.Name = "rePakToolStripMenuItem";
            rePakToolStripMenuItem.Size = new Size(180, 22);
            rePakToolStripMenuItem.Text = "RePak";
            rePakToolStripMenuItem.Click += rePakToolStripMenuItem_Click;
            // 
            // donateToolStripMenuItem
            // 
            donateToolStripMenuItem.Name = "donateToolStripMenuItem";
            donateToolStripMenuItem.Size = new Size(57, 20);
            donateToolStripMenuItem.Text = "Donate";
            donateToolStripMenuItem.Click += donateToolStripMenuItem_Click;
            // 
            // LblActiveFile
            // 
            LblActiveFile.Name = "LblActiveFile";
            LblActiveFile.Size = new Size(51, 17);
            LblActiveFile.Text = "File: N/a";
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { LblActiveFile });
            statusStrip1.Location = new Point(0, 649);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1272, 22);
            statusStrip1.TabIndex = 1;
            statusStrip1.Text = "statusStrip1";
            // 
            // MainEditor
            // 
            MainEditor.Dock = DockStyle.Fill;
            MainEditor.Location = new Point(0, 24);
            MainEditor.Name = "MainEditor";
            MainEditor.Size = new Size(1272, 625);
            MainEditor.TabIndex = 2;
            // 
            // FilePathTextBox
            // 
            FilePathTextBox.Location = new Point(215, 0);
            FilePathTextBox.Name = "FilePathTextBox";
            FilePathTextBox.ReadOnly = true;
            FilePathTextBox.Size = new Size(703, 23);
            FilePathTextBox.TabIndex = 4;
            // 
            // FMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1272, 671);
            Controls.Add(FilePathTextBox);
            Controls.Add(MainEditor);
            Controls.Add(statusStrip1);
            Controls.Add(Menu);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = Menu;
            Name = "FMain";
            Text = "Mir3D Client Editor";
            Load += FMain_Load;
            Menu.ResumeLayout(false);
            Menu.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private MenuStrip Menu;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem Menu_OpenFile;
        private ToolStripMenuItem Menu_Save;
        private ToolStripMenuItem toolsToolStripMenuItem;
        private ToolStripMenuItem decryptFolderToolStripMenuItem;
        private ToolStripStatusLabel LblActiveFile;
        private StatusStrip statusStrip1;
        private ToolStripMenuItem Menu_File_SaveAs;
        private ToolStripMenuItem Menu_File_OpenGameFolder;
        private MainEditorControl MainEditor;
        private ToolStripMenuItem donateToolStripMenuItem;
        private ToolStripMenuItem syncClientTextsToolStripMenuItem;
        private ToolStripMenuItem locateUPKNameToolStripMenuItem;
        private TextBox FilePathTextBox;
        private ToolStripMenuItem exportToolStripMenuItem;
        private ToolStripMenuItem compareToolStripMenuItem;
        private ToolStripMenuItem rePakToolStripMenuItem;
    }
}