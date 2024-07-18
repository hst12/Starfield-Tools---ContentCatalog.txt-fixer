namespace Starfield_Tools
{
    partial class frmStarfieldTools
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmStarfieldTools));
            this.richTextBox2 = new System.Windows.Forms.RichTextBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.btnQuit = new System.Windows.Forms.Button();
            this.cmdClean = new System.Windows.Forms.Button();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnCheck = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnStarfield = new System.Windows.Forms.Button();
            this.btnExplore = new System.Windows.Forms.Button();
            this.btnEditPlugins = new System.Windows.Forms.Button();
            this.btnBackup = new System.Windows.Forms.Button();
            this.btnRestore = new System.Windows.Forms.Button();
            this.btnLoadOrder = new System.Windows.Forms.Button();
            this.btnAbout = new System.Windows.Forms.Button();
            this.txtSource = new System.Windows.Forms.Button();
            this.chkForceClean = new System.Windows.Forms.CheckBox();
            this.btnStarfieldStore = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.grpActions = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cmdStarFieldPath = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cmdDeleteStale = new System.Windows.Forms.Button();
            this.grpAuto = new System.Windows.Forms.GroupBox();
            this.chkAutoRestore = new System.Windows.Forms.CheckBox();
            this.chkAutoBackup = new System.Windows.Forms.CheckBox();
            this.chkAutoClean = new System.Windows.Forms.CheckBox();
            this.chkAutoCheck = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.statusStrip1.SuspendLayout();
            this.grpActions.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.grpAuto.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // richTextBox2
            // 
            this.richTextBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox2.Location = new System.Drawing.Point(3, 468);
            this.richTextBox2.Name = "richTextBox2";
            this.richTextBox2.ReadOnly = true;
            this.richTextBox2.Size = new System.Drawing.Size(1307, 149);
            this.richTextBox2.TabIndex = 1;
            this.richTextBox2.TabStop = false;
            this.richTextBox2.Text = "";
            // 
            // richTextBox1
            // 
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Location = new System.Drawing.Point(6, 6);
            this.richTextBox1.Margin = new System.Windows.Forms.Padding(6);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(1301, 453);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.TabStop = false;
            this.richTextBox1.Text = "";
            // 
            // btnQuit
            // 
            this.btnQuit.AutoSize = true;
            this.btnQuit.Location = new System.Drawing.Point(969, 34);
            this.btnQuit.Margin = new System.Windows.Forms.Padding(4);
            this.btnQuit.Name = "btnQuit";
            this.btnQuit.Size = new System.Drawing.Size(150, 50);
            this.btnQuit.TabIndex = 13;
            this.btnQuit.Text = "Quit";
            this.toolTip1.SetToolTip(this.btnQuit, "Quit");
            this.btnQuit.UseVisualStyleBackColor = true;
            this.btnQuit.Click += new System.EventHandler(this.btnQuit_Click);
            // 
            // cmdClean
            // 
            this.cmdClean.AutoSize = true;
            this.cmdClean.Location = new System.Drawing.Point(169, 35);
            this.cmdClean.Margin = new System.Windows.Forms.Padding(6);
            this.cmdClean.Name = "cmdClean";
            this.cmdClean.Size = new System.Drawing.Size(150, 50);
            this.cmdClean.TabIndex = 5;
            this.cmdClean.Text = "Clean";
            this.toolTip1.SetToolTip(this.cmdClean, "Strip out corrupt characters");
            this.cmdClean.UseVisualStyleBackColor = true;
            this.cmdClean.Click += new System.EventHandler(this.cmdClean_Click);
            // 
            // btnLoad
            // 
            this.btnLoad.AutoSize = true;
            this.btnLoad.Location = new System.Drawing.Point(10, 31);
            this.btnLoad.Margin = new System.Windows.Forms.Padding(4);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(305, 50);
            this.btnLoad.TabIndex = 8;
            this.btnLoad.Text = "Edit ContentCatalog.txt";
            this.toolTip1.SetToolTip(this.btnLoad, "Edit with default text editor");
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // btnCheck
            // 
            this.btnCheck.AutoSize = true;
            this.btnCheck.Location = new System.Drawing.Point(10, 35);
            this.btnCheck.Margin = new System.Windows.Forms.Padding(4);
            this.btnCheck.Name = "btnCheck";
            this.btnCheck.Size = new System.Drawing.Size(150, 50);
            this.btnCheck.TabIndex = 4;
            this.btnCheck.Text = "Check";
            this.toolTip1.SetToolTip(this.btnCheck, "Check for corruption");
            this.btnCheck.UseVisualStyleBackColor = true;
            this.btnCheck.Click += new System.EventHandler(this.btnCheck_Click);
            // 
            // btnStarfield
            // 
            this.btnStarfield.AutoSize = true;
            this.btnStarfield.Location = new System.Drawing.Point(14, 34);
            this.btnStarfield.Margin = new System.Windows.Forms.Padding(4);
            this.btnStarfield.Name = "btnStarfield";
            this.btnStarfield.Size = new System.Drawing.Size(199, 50);
            this.btnStarfield.TabIndex = 11;
            this.btnStarfield.Text = "Starfield (Steam)";
            this.toolTip1.SetToolTip(this.btnStarfield, "Launch Starfield -Steam only");
            this.btnStarfield.UseVisualStyleBackColor = true;
            this.btnStarfield.Click += new System.EventHandler(this.btnStarfield_Click);
            // 
            // btnExplore
            // 
            this.btnExplore.AutoSize = true;
            this.btnExplore.Location = new System.Drawing.Point(532, 31);
            this.btnExplore.Name = "btnExplore";
            this.btnExplore.Size = new System.Drawing.Size(150, 50);
            this.btnExplore.TabIndex = 10;
            this.btnExplore.Text = "Explore";
            this.toolTip1.SetToolTip(this.btnExplore, "Open folder containing Plugins.txt and ContentCatalog.txt");
            this.btnExplore.UseVisualStyleBackColor = true;
            this.btnExplore.Click += new System.EventHandler(this.btnExplore_Click);
            // 
            // btnEditPlugins
            // 
            this.btnEditPlugins.AutoSize = true;
            this.btnEditPlugins.Location = new System.Drawing.Point(322, 31);
            this.btnEditPlugins.Name = "btnEditPlugins";
            this.btnEditPlugins.Size = new System.Drawing.Size(204, 50);
            this.btnEditPlugins.TabIndex = 9;
            this.btnEditPlugins.Text = "Edit Plugins.txt";
            this.toolTip1.SetToolTip(this.btnEditPlugins, "Edit with default text editor");
            this.btnEditPlugins.UseVisualStyleBackColor = true;
            this.btnEditPlugins.Click += new System.EventHandler(this.btnEditPlugins_Click);
            // 
            // btnBackup
            // 
            this.btnBackup.AutoSize = true;
            this.btnBackup.Location = new System.Drawing.Point(544, 35);
            this.btnBackup.Name = "btnBackup";
            this.btnBackup.Size = new System.Drawing.Size(150, 50);
            this.btnBackup.TabIndex = 6;
            this.btnBackup.Text = "Backup";
            this.toolTip1.SetToolTip(this.btnBackup, "Backup ContentCatalog.txt to ContentCatalog.txt.bak");
            this.btnBackup.UseVisualStyleBackColor = true;
            this.btnBackup.Click += new System.EventHandler(this.btnBackup_Click);
            // 
            // btnRestore
            // 
            this.btnRestore.AutoSize = true;
            this.btnRestore.Location = new System.Drawing.Point(700, 35);
            this.btnRestore.Name = "btnRestore";
            this.btnRestore.Size = new System.Drawing.Size(150, 50);
            this.btnRestore.TabIndex = 7;
            this.btnRestore.Text = "Restore";
            this.toolTip1.SetToolTip(this.btnRestore, "Restore ContentCatlog.txt.bak to ContextCatlog.txt");
            this.btnRestore.UseVisualStyleBackColor = true;
            this.btnRestore.Click += new System.EventHandler(this.btnRestore_Click);
            // 
            // btnLoadOrder
            // 
            this.btnLoadOrder.AutoSize = true;
            this.btnLoadOrder.Location = new System.Drawing.Point(688, 30);
            this.btnLoadOrder.Name = "btnLoadOrder";
            this.btnLoadOrder.Size = new System.Drawing.Size(210, 50);
            this.btnLoadOrder.TabIndex = 11;
            this.btnLoadOrder.Text = "Load Order";
            this.toolTip1.SetToolTip(this.btnLoadOrder, "Load Order Editor");
            this.btnLoadOrder.UseVisualStyleBackColor = true;
            this.btnLoadOrder.Click += new System.EventHandler(this.btnLoadOrder_Click);
            // 
            // btnAbout
            // 
            this.btnAbout.AutoSize = true;
            this.btnAbout.Location = new System.Drawing.Point(655, 34);
            this.btnAbout.Margin = new System.Windows.Forms.Padding(6);
            this.btnAbout.Name = "btnAbout";
            this.btnAbout.Size = new System.Drawing.Size(150, 50);
            this.btnAbout.TabIndex = 12;
            this.btnAbout.Text = "About";
            this.toolTip1.SetToolTip(this.btnAbout, "Brief info");
            this.btnAbout.UseVisualStyleBackColor = true;
            this.btnAbout.Click += new System.EventHandler(this.btnAbout_Click);
            // 
            // txtSource
            // 
            this.txtSource.AutoSize = true;
            this.txtSource.Location = new System.Drawing.Point(812, 34);
            this.txtSource.Name = "txtSource";
            this.txtSource.Size = new System.Drawing.Size(150, 50);
            this.txtSource.TabIndex = 14;
            this.txtSource.Text = "Github";
            this.toolTip1.SetToolTip(this.txtSource, "Open GitHub page");
            this.txtSource.UseVisualStyleBackColor = true;
            this.txtSource.Click += new System.EventHandler(this.txtSource_Click);
            // 
            // chkForceClean
            // 
            this.chkForceClean.AutoSize = true;
            this.chkForceClean.Location = new System.Drawing.Point(749, 30);
            this.chkForceClean.Name = "chkForceClean";
            this.chkForceClean.Size = new System.Drawing.Size(161, 29);
            this.chkForceClean.TabIndex = 4;
            this.chkForceClean.Text = "Force Clean";
            this.toolTip1.SetToolTip(this.chkForceClean, "Not normally needed");
            this.chkForceClean.UseVisualStyleBackColor = true;
            this.chkForceClean.CheckedChanged += new System.EventHandler(this.chkForceClean_CheckedChanged);
            // 
            // btnStarfieldStore
            // 
            this.btnStarfieldStore.AutoSize = true;
            this.btnStarfieldStore.Location = new System.Drawing.Point(220, 34);
            this.btnStarfieldStore.Name = "btnStarfieldStore";
            this.btnStarfieldStore.Size = new System.Drawing.Size(199, 50);
            this.btnStarfieldStore.TabIndex = 16;
            this.btnStarfieldStore.Text = "Starfield (MS)";
            this.toolTip1.SetToolTip(this.btnStarfieldStore, "Launch Starfield - MS Store version");
            this.btnStarfieldStore.UseVisualStyleBackColor = true;
            this.btnStarfieldStore.Click += new System.EventHandler(this.btnStarfieldStore_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 1100);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 14, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1313, 42);
            this.statusStrip1.TabIndex = 5;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(131, 32);
            this.toolStripStatusLabel1.Text = "Starting up";
            // 
            // grpActions
            // 
            this.grpActions.AutoSize = true;
            this.grpActions.Controls.Add(this.cmdDeleteStale);
            this.grpActions.Controls.Add(this.btnRestore);
            this.grpActions.Controls.Add(this.btnBackup);
            this.grpActions.Controls.Add(this.btnCheck);
            this.grpActions.Controls.Add(this.cmdClean);
            this.grpActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpActions.Location = new System.Drawing.Point(6, 626);
            this.grpActions.Margin = new System.Windows.Forms.Padding(6);
            this.grpActions.Name = "grpActions";
            this.grpActions.Padding = new System.Windows.Forms.Padding(6);
            this.grpActions.Size = new System.Drawing.Size(1301, 121);
            this.grpActions.TabIndex = 1;
            this.grpActions.TabStop = false;
            this.grpActions.Text = "Actions";
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSize = true;
            this.groupBox1.Controls.Add(this.btnStarfieldStore);
            this.groupBox1.Controls.Add(this.cmdStarFieldPath);
            this.groupBox1.Controls.Add(this.txtSource);
            this.groupBox1.Controls.Add(this.btnAbout);
            this.groupBox1.Controls.Add(this.btnStarfield);
            this.groupBox1.Controls.Add(this.btnQuit);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(6, 973);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(6);
            this.groupBox1.Size = new System.Drawing.Size(1301, 121);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Other";
            // 
            // cmdStarFieldPath
            // 
            this.cmdStarFieldPath.AutoSize = true;
            this.cmdStarFieldPath.Location = new System.Drawing.Point(426, 34);
            this.cmdStarFieldPath.Name = "cmdStarFieldPath";
            this.cmdStarFieldPath.Size = new System.Drawing.Size(222, 50);
            this.cmdStarFieldPath.TabIndex = 15;
            this.cmdStarFieldPath.Text = "Set Starfield Path";
            this.cmdStarFieldPath.UseVisualStyleBackColor = true;
            this.cmdStarFieldPath.Click += new System.EventHandler(this.cmdStarFieldPath_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.AutoSize = true;
            this.groupBox2.Controls.Add(this.btnLoadOrder);
            this.groupBox2.Controls.Add(this.btnEditPlugins);
            this.groupBox2.Controls.Add(this.btnExplore);
            this.groupBox2.Controls.Add(this.btnLoad);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(3, 756);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(1307, 112);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Edit";
            // 
            // cmdDeleteStale
            // 
            this.cmdDeleteStale.Enabled = false;
            this.cmdDeleteStale.Location = new System.Drawing.Point(328, 35);
            this.cmdDeleteStale.Name = "cmdDeleteStale";
            this.cmdDeleteStale.Size = new System.Drawing.Size(210, 50);
            this.cmdDeleteStale.TabIndex = 12;
            this.cmdDeleteStale.Text = "Remove Unused";
            this.cmdDeleteStale.UseVisualStyleBackColor = true;
            this.cmdDeleteStale.Visible = false;
            this.cmdDeleteStale.Click += new System.EventHandler(this.cmdDeleteStale_Click);
            // 
            // grpAuto
            // 
            this.grpAuto.AutoSize = true;
            this.grpAuto.Controls.Add(this.chkForceClean);
            this.grpAuto.Controls.Add(this.chkAutoRestore);
            this.grpAuto.Controls.Add(this.chkAutoBackup);
            this.grpAuto.Controls.Add(this.chkAutoClean);
            this.grpAuto.Controls.Add(this.chkAutoCheck);
            this.grpAuto.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpAuto.Location = new System.Drawing.Point(3, 874);
            this.grpAuto.Name = "grpAuto";
            this.grpAuto.Size = new System.Drawing.Size(1307, 90);
            this.grpAuto.TabIndex = 6;
            this.grpAuto.TabStop = false;
            this.grpAuto.Text = "Auto Functions";
            // 
            // chkAutoRestore
            // 
            this.chkAutoRestore.AutoSize = true;
            this.chkAutoRestore.Location = new System.Drawing.Point(557, 31);
            this.chkAutoRestore.Name = "chkAutoRestore";
            this.chkAutoRestore.Size = new System.Drawing.Size(169, 29);
            this.chkAutoRestore.TabIndex = 3;
            this.chkAutoRestore.Text = "Auto Restore";
            this.chkAutoRestore.UseVisualStyleBackColor = true;
            this.chkAutoRestore.CheckedChanged += new System.EventHandler(this.chkAutoRestore_CheckedChanged);
            // 
            // chkAutoBackup
            // 
            this.chkAutoBackup.AutoSize = true;
            this.chkAutoBackup.Location = new System.Drawing.Point(368, 31);
            this.chkAutoBackup.Name = "chkAutoBackup";
            this.chkAutoBackup.Size = new System.Drawing.Size(166, 29);
            this.chkAutoBackup.TabIndex = 2;
            this.chkAutoBackup.Text = "Auto Backup";
            this.chkAutoBackup.UseVisualStyleBackColor = true;
            this.chkAutoBackup.CheckedChanged += new System.EventHandler(this.chkAutoBackup_CheckedChanged);
            // 
            // chkAutoClean
            // 
            this.chkAutoClean.AutoSize = true;
            this.chkAutoClean.Location = new System.Drawing.Point(195, 30);
            this.chkAutoClean.Name = "chkAutoClean";
            this.chkAutoClean.Size = new System.Drawing.Size(150, 29);
            this.chkAutoClean.TabIndex = 1;
            this.chkAutoClean.Text = "Auto Clean";
            this.chkAutoClean.UseVisualStyleBackColor = true;
            this.chkAutoClean.CheckedChanged += new System.EventHandler(this.chkAutoClean_CheckedChanged);
            // 
            // chkAutoCheck
            // 
            this.chkAutoCheck.AutoSize = true;
            this.chkAutoCheck.Location = new System.Drawing.Point(17, 31);
            this.chkAutoCheck.Name = "chkAutoCheck";
            this.chkAutoCheck.Size = new System.Drawing.Size(155, 29);
            this.chkAutoCheck.TabIndex = 0;
            this.chkAutoCheck.Text = "Auto Check";
            this.chkAutoCheck.UseVisualStyleBackColor = true;
            this.chkAutoCheck.CheckedChanged += new System.EventHandler(this.chkAutoCheck_CheckedChanged);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.richTextBox2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.grpAuto, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.richTextBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox2, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.grpActions, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 75F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1313, 1100);
            this.tableLayoutPanel1.TabIndex = 7;
            // 
            // frmStarfieldTools
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1313, 1142);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.statusStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "frmStarfieldTools";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Starfield Tools";
            this.Activated += new System.EventHandler(this.frmStarfieldTools_Activated);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.grpActions.ResumeLayout(false);
            this.grpActions.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.grpAuto.ResumeLayout(false);
            this.grpAuto.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnQuit;
        private System.Windows.Forms.Button cmdClean;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnCheck;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnStarfield;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.GroupBox grpActions;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnAbout;
        private System.Windows.Forms.Button btnExplore;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnEditPlugins;
        private System.Windows.Forms.Button btnRestore;
        private System.Windows.Forms.Button btnBackup;
        private System.Windows.Forms.GroupBox grpAuto;
        private System.Windows.Forms.CheckBox chkAutoBackup;
        private System.Windows.Forms.CheckBox chkAutoClean;
        private System.Windows.Forms.CheckBox chkAutoCheck;
        private System.Windows.Forms.CheckBox chkAutoRestore;
        private System.Windows.Forms.RichTextBox richTextBox2;
        private System.Windows.Forms.Button btnLoadOrder;
        private System.Windows.Forms.Button txtSource;
        private System.Windows.Forms.Button cmdStarFieldPath;
        private System.Windows.Forms.Button btnStarfieldStore;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.CheckBox chkForceClean;
        private System.Windows.Forms.Button cmdDeleteStale;
    }
}

