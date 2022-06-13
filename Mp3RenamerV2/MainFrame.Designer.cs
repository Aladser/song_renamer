namespace Mp3RenamerV2
{
    partial class MainFrame
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainFrame));
            this.infoField = new System.Windows.Forms.RichTextBox();
            this.openFileMS = new System.Windows.Forms.ToolStripMenuItem();
            this.openFolderMS = new System.Windows.Forms.ToolStripMenuItem();
            this.checkTagsMS = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.openMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFolderMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkTagsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkNameMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkFileNameMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkAlbFileNameMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.очиститьПолеToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.проверкаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkStatusLabel = new System.Windows.Forms.Label();
            this.progressLabel = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // infoField
            // 
            this.infoField.Location = new System.Drawing.Point(0, 57);
            this.infoField.Name = "infoField";
            this.infoField.Size = new System.Drawing.Size(1096, 477);
            this.infoField.TabIndex = 3;
            this.infoField.Text = "";
            // 
            // openFileMS
            // 
            this.openFileMS.Name = "openFileMS";
            this.openFileMS.Size = new System.Drawing.Size(108, 22);
            this.openFileMS.Text = "Файл";
            this.openFileMS.Click += new System.EventHandler(this.openFileMenuItem_Click);
            // 
            // openFolderMS
            // 
            this.openFolderMS.Name = "openFolderMS";
            this.openFolderMS.Size = new System.Drawing.Size(108, 22);
            this.openFolderMS.Text = "Папку";
            this.openFolderMS.Click += new System.EventHandler(this.openFolderMenuItem_Click);
            // 
            // checkTagsMS
            // 
            this.checkTagsMS.Name = "checkTagsMS";
            this.checkTagsMS.Size = new System.Drawing.Size(105, 20);
            this.checkTagsMS.Text = "Проверить теги";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openMenuItem,
            this.checkTagsMenuItem,
            this.checkNameMenuItem,
            this.очиститьПолеToolStripMenuItem,
            this.проверкаToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1108, 24);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // openMenuItem
            // 
            this.openMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openFileMenuItem,
            this.openFolderMenuItem});
            this.openMenuItem.Name = "openMenuItem";
            this.openMenuItem.Size = new System.Drawing.Size(66, 20);
            this.openMenuItem.Text = "Открыть";
            // 
            // openFileMenuItem
            // 
            this.openFileMenuItem.Name = "openFileMenuItem";
            this.openFileMenuItem.Size = new System.Drawing.Size(106, 22);
            this.openFileMenuItem.Text = "файл";
            this.openFileMenuItem.Click += new System.EventHandler(this.openFileMenuItem_Click);
            // 
            // openFolderMenuItem
            // 
            this.openFolderMenuItem.Name = "openFolderMenuItem";
            this.openFolderMenuItem.Size = new System.Drawing.Size(106, 22);
            this.openFolderMenuItem.Text = "папку";
            this.openFolderMenuItem.Click += new System.EventHandler(this.openFolderMenuItem_Click);
            // 
            // checkTagsMenuItem
            // 
            this.checkTagsMenuItem.Enabled = false;
            this.checkTagsMenuItem.Name = "checkTagsMenuItem";
            this.checkTagsMenuItem.Size = new System.Drawing.Size(105, 20);
            this.checkTagsMenuItem.Text = "Проверить теги";
            this.checkTagsMenuItem.Click += new System.EventHandler(this.checkTagsMenuItem_Click);
            // 
            // checkNameMenuItem
            // 
            this.checkNameMenuItem.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.checkNameMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkFileNameMenuItem,
            this.checkAlbFileNameMenuItem});
            this.checkNameMenuItem.Enabled = false;
            this.checkNameMenuItem.Name = "checkNameMenuItem";
            this.checkNameMenuItem.Size = new System.Drawing.Size(104, 20);
            this.checkNameMenuItem.Text = "Проверить имя";
            // 
            // checkFileNameMenuItem
            // 
            this.checkFileNameMenuItem.Name = "checkFileNameMenuItem";
            this.checkFileNameMenuItem.Size = new System.Drawing.Size(180, 22);
            this.checkFileNameMenuItem.Text = "файла";
            this.checkFileNameMenuItem.Click += new System.EventHandler(this.checkFileNameMenuItem_Click);
            // 
            // checkAlbFileNameMenuItem
            // 
            this.checkAlbFileNameMenuItem.Name = "checkAlbFileNameMenuItem";
            this.checkAlbFileNameMenuItem.Size = new System.Drawing.Size(180, 22);
            this.checkAlbFileNameMenuItem.Text = "файла альбома";
            this.checkAlbFileNameMenuItem.Click += new System.EventHandler(this.checkAlbFileNameMenuItem_Click);
            // 
            // очиститьПолеToolStripMenuItem
            // 
            this.очиститьПолеToolStripMenuItem.Name = "очиститьПолеToolStripMenuItem";
            this.очиститьПолеToolStripMenuItem.Size = new System.Drawing.Size(101, 20);
            this.очиститьПолеToolStripMenuItem.Text = "Очистить поле";
            this.очиститьПолеToolStripMenuItem.Click += new System.EventHandler(this.clearInfoField_Click);
            // 
            // проверкаToolStripMenuItem
            // 
            this.проверкаToolStripMenuItem.Name = "проверкаToolStripMenuItem";
            this.проверкаToolStripMenuItem.Size = new System.Drawing.Size(12, 20);
            // 
            // checkStatusLabel
            // 
            this.checkStatusLabel.Location = new System.Drawing.Point(12, 24);
            this.checkStatusLabel.Name = "checkStatusLabel";
            this.checkStatusLabel.Size = new System.Drawing.Size(100, 23);
            this.checkStatusLabel.TabIndex = 5;
            this.checkStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // progressLabel
            // 
            this.progressLabel.Location = new System.Drawing.Point(996, 24);
            this.progressLabel.Name = "progressLabel";
            this.progressLabel.Size = new System.Drawing.Size(100, 23);
            this.progressLabel.TabIndex = 6;
            this.progressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MainFrame
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1108, 546);
            this.Controls.Add(this.progressLabel);
            this.Controls.Add(this.checkStatusLabel);
            this.Controls.Add(this.infoField);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainFrame";
            this.Text = "SongRenamer 1.02";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private RichTextBox infoField;
        private ToolStripMenuItem openFileMS;
        private ToolStripMenuItem openFolderMS;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem openMenuItem;
        private ToolStripMenuItem openFileMenuItem;
        private ToolStripMenuItem openFolderMenuItem;
        private ToolStripMenuItem checkTagsMenuItem;
        private ToolStripMenuItem checkTagsMS;
        private ToolStripMenuItem checkNameMenuItem;
        private ToolStripMenuItem checkFileNameMenuItem;
        private ToolStripMenuItem checkAlbFileNameMenuItem;
        private Label checkStatusLabel;
        private ToolStripMenuItem очиститьПолеToolStripMenuItem;
        private Label progressLabel;
        private ToolStripMenuItem проверкаToolStripMenuItem;
    }
}