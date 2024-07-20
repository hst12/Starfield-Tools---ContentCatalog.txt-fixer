namespace Starfield_Tools
{
    partial class frmLoadOrder
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLoadOrder));
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.ModEnabled = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ModNamexx = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Descritpion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Version = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Files = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AS = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnBottom = new System.Windows.Forms.Button();
            this.btnTop = new System.Windows.Forms.Button();
            this.btnRestorePlugins = new System.Windows.Forms.Button();
            this.btnBackupPlugins = new System.Windows.Forms.Button();
            this.btnDown = new System.Windows.Forms.Button();
            this.btnUp = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnEnableAll = new System.Windows.Forms.Button();
            this.btnDisable = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView1.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ModEnabled,
            this.ModNamexx,
            this.Descritpion,
            this.Version,
            this.Files,
            this.AS});
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(4, 4);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(4);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 82;
            this.dataGridView1.RowTemplate.Height = 33;
            this.dataGridView1.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.ShowEditingIcon = false;
            this.dataGridView1.Size = new System.Drawing.Size(1623, 800);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.Sorted += new System.EventHandler(this.dataGridView1_Sorted);
            // 
            // ModEnabled
            // 
            this.ModEnabled.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.ModEnabled.HeaderText = "Enabled";
            this.ModEnabled.MinimumWidth = 10;
            this.ModEnabled.Name = "ModEnabled";
            this.ModEnabled.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.ModEnabled.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.ModEnabled.Width = 136;
            // 
            // ModNamexx
            // 
            this.ModNamexx.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.ModNamexx.HeaderText = "Plugin Name";
            this.ModNamexx.MinimumWidth = 10;
            this.ModNamexx.Name = "ModNamexx";
            this.ModNamexx.ReadOnly = true;
            this.ModNamexx.Width = 179;
            // 
            // Descritpion
            // 
            this.Descritpion.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.Descritpion.HeaderText = "Description";
            this.Descritpion.MinimumWidth = 10;
            this.Descritpion.Name = "Descritpion";
            this.Descritpion.Width = 165;
            // 
            // Version
            // 
            this.Version.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.Version.HeaderText = "Version";
            this.Version.MinimumWidth = 10;
            this.Version.Name = "Version";
            this.Version.ReadOnly = true;
            this.Version.Width = 130;
            // 
            // Files
            // 
            this.Files.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.Files.HeaderText = "Files";
            this.Files.MinimumWidth = 10;
            this.Files.Name = "Files";
            this.Files.ReadOnly = true;
            this.Files.Width = 103;
            // 
            // AS
            // 
            this.AS.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.AS.HeaderText = "Achievements";
            this.AS.MinimumWidth = 10;
            this.AS.Name = "AS";
            this.AS.ReadOnly = true;
            this.AS.Width = 192;
            // 
            // btnOK
            // 
            this.btnOK.AutoSize = true;
            this.btnOK.Location = new System.Drawing.Point(473, 69);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(150, 50);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "Ok";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.AutoSize = true;
            this.btnCancel.Location = new System.Drawing.Point(631, 69);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(150, 50);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnBottom
            // 
            this.btnBottom.AutoSize = true;
            this.btnBottom.Location = new System.Drawing.Point(475, 3);
            this.btnBottom.Name = "btnBottom";
            this.btnBottom.Size = new System.Drawing.Size(150, 50);
            this.btnBottom.TabIndex = 5;
            this.btnBottom.Text = "Bottom";
            this.btnBottom.UseVisualStyleBackColor = true;
            this.btnBottom.Click += new System.EventHandler(this.btnBottom_Click);
            // 
            // btnTop
            // 
            this.btnTop.AutoSize = true;
            this.btnTop.Location = new System.Drawing.Point(319, 3);
            this.btnTop.Name = "btnTop";
            this.btnTop.Size = new System.Drawing.Size(150, 50);
            this.btnTop.TabIndex = 4;
            this.btnTop.Text = "Top";
            this.btnTop.UseVisualStyleBackColor = true;
            this.btnTop.Click += new System.EventHandler(this.btnTop_Click);
            // 
            // btnRestorePlugins
            // 
            this.btnRestorePlugins.AutoSize = true;
            this.btnRestorePlugins.Location = new System.Drawing.Point(787, 3);
            this.btnRestorePlugins.Name = "btnRestorePlugins";
            this.btnRestorePlugins.Size = new System.Drawing.Size(150, 50);
            this.btnRestorePlugins.TabIndex = 7;
            this.btnRestorePlugins.Text = "Restore";
            this.btnRestorePlugins.UseVisualStyleBackColor = true;
            this.btnRestorePlugins.Click += new System.EventHandler(this.btnRestorePlugins_Click);
            // 
            // btnBackupPlugins
            // 
            this.btnBackupPlugins.AutoSize = true;
            this.btnBackupPlugins.Location = new System.Drawing.Point(631, 3);
            this.btnBackupPlugins.Name = "btnBackupPlugins";
            this.btnBackupPlugins.Size = new System.Drawing.Size(150, 50);
            this.btnBackupPlugins.TabIndex = 6;
            this.btnBackupPlugins.Text = "Backup";
            this.btnBackupPlugins.UseVisualStyleBackColor = true;
            this.btnBackupPlugins.Click += new System.EventHandler(this.btnBackupPlugins_Click);
            // 
            // btnDown
            // 
            this.btnDown.AutoSize = true;
            this.btnDown.Location = new System.Drawing.Point(164, 3);
            this.btnDown.Margin = new System.Windows.Forms.Padding(2);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(150, 50);
            this.btnDown.TabIndex = 3;
            this.btnDown.Text = "Down";
            this.btnDown.UseVisualStyleBackColor = true;
            this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
            // 
            // btnUp
            // 
            this.btnUp.AutoSize = true;
            this.btnUp.Location = new System.Drawing.Point(10, 3);
            this.btnUp.Margin = new System.Windows.Forms.Padding(2);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(150, 50);
            this.btnUp.TabIndex = 2;
            this.btnUp.Text = "Up";
            this.btnUp.UseVisualStyleBackColor = true;
            this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 948);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1603, 42);
            this.statusStrip1.TabIndex = 5;
            this.statusStrip1.Text = "Starting up";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(131, 32);
            this.toolStripStatusLabel1.Text = "Starting up";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnEnableAll);
            this.panel2.Controls.Add(this.btnDisable);
            this.panel2.Controls.Add(this.btnCancel);
            this.panel2.Controls.Add(this.btnRestorePlugins);
            this.panel2.Controls.Add(this.btnBackupPlugins);
            this.panel2.Controls.Add(this.btnOK);
            this.panel2.Controls.Add(this.btnBottom);
            this.panel2.Controls.Add(this.btnUp);
            this.panel2.Controls.Add(this.btnTop);
            this.panel2.Controls.Add(this.btnDown);
            this.panel2.Location = new System.Drawing.Point(3, 811);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1625, 134);
            this.panel2.TabIndex = 6;
            // 
            // btnEnableAll
            // 
            this.btnEnableAll.Location = new System.Drawing.Point(1099, 3);
            this.btnEnableAll.Name = "btnEnableAll";
            this.btnEnableAll.Size = new System.Drawing.Size(150, 50);
            this.btnEnableAll.TabIndex = 10;
            this.btnEnableAll.Text = "Enable All";
            this.btnEnableAll.UseVisualStyleBackColor = true;
            this.btnEnableAll.Click += new System.EventHandler(this.btnEnableAll_Click);
            // 
            // btnDisable
            // 
            this.btnDisable.AutoSize = true;
            this.btnDisable.Location = new System.Drawing.Point(943, 3);
            this.btnDisable.Name = "btnDisable";
            this.btnDisable.Size = new System.Drawing.Size(150, 50);
            this.btnDisable.TabIndex = 9;
            this.btnDisable.Text = "Disable All";
            this.btnDisable.UseVisualStyleBackColor = true;
            this.btnDisable.Click += new System.EventHandler(this.btnDisable_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.dataGridView1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1603, 948);
            this.tableLayoutPanel1.TabIndex = 7;
            // 
            // frmLoadOrder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1603, 990);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.statusStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "frmLoadOrder";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Load Order";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnDown;
        private System.Windows.Forms.Button btnUp;
        private System.Windows.Forms.Button btnRestorePlugins;
        private System.Windows.Forms.Button btnBackupPlugins;
        private System.Windows.Forms.Button btnBottom;
        private System.Windows.Forms.Button btnTop;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnDisable;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ModEnabled;
        private System.Windows.Forms.DataGridViewTextBoxColumn ModNamexx;
        private System.Windows.Forms.DataGridViewTextBoxColumn Descritpion;
        private System.Windows.Forms.DataGridViewTextBoxColumn Version;
        private System.Windows.Forms.DataGridViewTextBoxColumn Files;
        private System.Windows.Forms.DataGridViewTextBoxColumn AS;
        private System.Windows.Forms.Button btnEnableAll;
    }
}