namespace Starfield_Tools.Load_Order_Editor
{
    partial class frmProfiles
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
            chkBaseProfile = new System.Windows.Forms.CheckedListBox();
            chkAdd = new System.Windows.Forms.CheckedListBox();
            btnOK = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            label3 = new System.Windows.Forms.Label();
            cmbDestination = new System.Windows.Forms.ComboBox();
            lblSource = new System.Windows.Forms.Label();
            tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            label2 = new System.Windows.Forms.Label();
            lstAdded = new System.Windows.Forms.ListBox();
            lstRemoved = new System.Windows.Forms.ListBox();
            label1 = new System.Windows.Forms.Label();
            chkResult = new System.Windows.Forms.ListBox();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            SuspendLayout();
            // 
            // chkBaseProfile
            // 
            chkBaseProfile.Dock = System.Windows.Forms.DockStyle.Fill;
            chkBaseProfile.FormattingEnabled = true;
            chkBaseProfile.Location = new System.Drawing.Point(3, 49);
            chkBaseProfile.Name = "chkBaseProfile";
            chkBaseProfile.Size = new System.Drawing.Size(280, 577);
            chkBaseProfile.TabIndex = 0;
            // 
            // chkAdd
            // 
            chkAdd.CheckOnClick = true;
            chkAdd.Dock = System.Windows.Forms.DockStyle.Fill;
            chkAdd.FormattingEnabled = true;
            chkAdd.Location = new System.Drawing.Point(289, 49);
            chkAdd.Name = "chkAdd";
            chkAdd.Size = new System.Drawing.Size(280, 577);
            chkAdd.TabIndex = 1;
            // 
            // btnOK
            // 
            btnOK.Location = new System.Drawing.Point(3, 632);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(150, 46);
            btnOK.TabIndex = 3;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.Location = new System.Drawing.Point(861, 632);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(150, 46);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 4;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tableLayoutPanel1.Controls.Add(chkBaseProfile, 0, 1);
            tableLayoutPanel1.Controls.Add(chkAdd, 1, 1);
            tableLayoutPanel1.Controls.Add(btnOK, 0, 2);
            tableLayoutPanel1.Controls.Add(label3, 2, 0);
            tableLayoutPanel1.Controls.Add(btnCancel, 3, 2);
            tableLayoutPanel1.Controls.Add(cmbDestination, 1, 0);
            tableLayoutPanel1.Controls.Add(lblSource, 0, 0);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 3, 1);
            tableLayoutPanel1.Controls.Add(label1, 3, 0);
            tableLayoutPanel1.Controls.Add(chkResult, 2, 1);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.Size = new System.Drawing.Size(1145, 681);
            tableLayoutPanel1.TabIndex = 5;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(575, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(78, 32);
            label3.TabIndex = 7;
            label3.Text = "Result";
            // 
            // cmbDestination
            // 
            cmbDestination.FormattingEnabled = true;
            cmbDestination.Location = new System.Drawing.Point(289, 3);
            cmbDestination.Name = "cmbDestination";
            cmbDestination.Size = new System.Drawing.Size(242, 40);
            cmbDestination.TabIndex = 10;
            cmbDestination.SelectedIndexChanged += cmbDestination_SelectedIndexChanged;
            // 
            // lblSource
            // 
            lblSource.AutoSize = true;
            lblSource.Location = new System.Drawing.Point(3, 0);
            lblSource.Name = "lblSource";
            lblSource.Size = new System.Drawing.Size(78, 32);
            lblSource.TabIndex = 11;
            lblSource.Text = "label1";
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(label2, 0, 1);
            tableLayoutPanel2.Controls.Add(lstAdded, 0, 0);
            tableLayoutPanel2.Controls.Add(lstRemoved, 0, 2);
            tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel2.Location = new System.Drawing.Point(861, 49);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 3;
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel2.Size = new System.Drawing.Size(281, 577);
            tableLayoutPanel2.TabIndex = 12;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(3, 272);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(114, 32);
            label2.TabIndex = 2;
            label2.Text = "Removed";
            // 
            // lstAdded
            // 
            lstAdded.Dock = System.Windows.Forms.DockStyle.Fill;
            lstAdded.FormattingEnabled = true;
            lstAdded.Location = new System.Drawing.Point(3, 3);
            lstAdded.Name = "lstAdded";
            lstAdded.Size = new System.Drawing.Size(275, 266);
            lstAdded.TabIndex = 3;
            // 
            // lstRemoved
            // 
            lstRemoved.Dock = System.Windows.Forms.DockStyle.Fill;
            lstRemoved.FormattingEnabled = true;
            lstRemoved.Location = new System.Drawing.Point(3, 307);
            lstRemoved.Name = "lstRemoved";
            lstRemoved.Size = new System.Drawing.Size(275, 267);
            lstRemoved.TabIndex = 4;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(861, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(84, 32);
            label1.TabIndex = 13;
            label1.Text = "Added";
            // 
            // chkResult
            // 
            chkResult.Dock = System.Windows.Forms.DockStyle.Fill;
            chkResult.FormattingEnabled = true;
            chkResult.Location = new System.Drawing.Point(575, 49);
            chkResult.Name = "chkResult";
            chkResult.Size = new System.Drawing.Size(280, 577);
            chkResult.TabIndex = 14;
            // 
            // frmProfiles
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1145, 681);
            Controls.Add(tableLayoutPanel1);
            Name = "frmProfiles";
            Text = "Profile Manager";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.CheckedListBox chkBaseProfile;
        private System.Windows.Forms.CheckedListBox chkAdd;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbDestination;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox lstAdded;
        private System.Windows.Forms.ListBox lstRemoved;
        private System.Windows.Forms.ListBox chkResult;
    }
}