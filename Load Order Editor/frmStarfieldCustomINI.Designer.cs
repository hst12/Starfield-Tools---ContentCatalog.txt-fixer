namespace Starfield_Tools.Load_Order_Editor
{
    partial class frmStarfieldCustomINI
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
            btnOK = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            chkLooseFiles = new System.Windows.Forms.CheckBox();
            chkMOTD = new System.Windows.Forms.CheckBox();
            ckhPapyrusLogging = new System.Windows.Forms.CheckBox();
            chkUserPhotos = new System.Windows.Forms.CheckBox();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            chkSkipIntro = new System.Windows.Forms.CheckBox();
            chkMainMenuDelay = new System.Windows.Forms.CheckBox();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // btnOK
            // 
            btnOK.Dock = System.Windows.Forms.DockStyle.Left;
            btnOK.Location = new System.Drawing.Point(3, 3);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(130, 59);
            btnOK.TabIndex = 0;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Dock = System.Windows.Forms.DockStyle.Right;
            btnCancel.Location = new System.Drawing.Point(512, 3);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(136, 59);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // chkLooseFiles
            // 
            chkLooseFiles.AutoSize = true;
            chkLooseFiles.Location = new System.Drawing.Point(12, 12);
            chkLooseFiles.Name = "chkLooseFiles";
            chkLooseFiles.Size = new System.Drawing.Size(162, 36);
            chkLooseFiles.TabIndex = 2;
            chkLooseFiles.Text = "Loose Files";
            chkLooseFiles.UseVisualStyleBackColor = true;
            // 
            // chkMOTD
            // 
            chkMOTD.AutoSize = true;
            chkMOTD.Location = new System.Drawing.Point(12, 54);
            chkMOTD.Name = "chkMOTD";
            chkMOTD.Size = new System.Drawing.Size(259, 36);
            chkMOTD.TabIndex = 3;
            chkMOTD.Text = "Message of the Day";
            chkMOTD.UseVisualStyleBackColor = true;
            // 
            // ckhPapyrusLogging
            // 
            ckhPapyrusLogging.AutoSize = true;
            ckhPapyrusLogging.Location = new System.Drawing.Point(12, 222);
            ckhPapyrusLogging.Name = "ckhPapyrusLogging";
            ckhPapyrusLogging.Size = new System.Drawing.Size(222, 36);
            ckhPapyrusLogging.TabIndex = 4;
            ckhPapyrusLogging.Text = "Papyrus Logging";
            ckhPapyrusLogging.UseVisualStyleBackColor = true;
            // 
            // chkUserPhotos
            // 
            chkUserPhotos.AutoSize = true;
            chkUserPhotos.Location = new System.Drawing.Point(12, 180);
            chkUserPhotos.Name = "chkUserPhotos";
            chkUserPhotos.Size = new System.Drawing.Size(317, 36);
            chkUserPhotos.TabIndex = 5;
            chkUserPhotos.Text = "Player Photo Loadscreens";
            chkUserPhotos.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(btnOK, 0, 0);
            tableLayoutPanel1.Controls.Add(btnCancel, 1, 0);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 293);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new System.Drawing.Size(651, 65);
            tableLayoutPanel1.TabIndex = 6;
            // 
            // chkSkipIntro
            // 
            chkSkipIntro.AutoSize = true;
            chkSkipIntro.Location = new System.Drawing.Point(12, 96);
            chkSkipIntro.Name = "chkSkipIntro";
            chkSkipIntro.Size = new System.Drawing.Size(148, 36);
            chkSkipIntro.TabIndex = 7;
            chkSkipIntro.Text = "Skip Intro";
            chkSkipIntro.UseVisualStyleBackColor = true;
            // 
            // chkMainMenuDelay
            // 
            chkMainMenuDelay.AutoSize = true;
            chkMainMenuDelay.Location = new System.Drawing.Point(12, 138);
            chkMainMenuDelay.Name = "chkMainMenuDelay";
            chkMainMenuDelay.Size = new System.Drawing.Size(363, 36);
            chkMainMenuDelay.TabIndex = 8;
            chkMainMenuDelay.Text = "Main Menu Delay before skip";
            chkMainMenuDelay.UseVisualStyleBackColor = true;
            // 
            // frmStarfieldCustomINI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(651, 358);
            Controls.Add(chkMainMenuDelay);
            Controls.Add(chkSkipIntro);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(chkUserPhotos);
            Controls.Add(ckhPapyrusLogging);
            Controls.Add(chkMOTD);
            Controls.Add(chkLooseFiles);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmStarfieldCustomINI";
            Text = "StarfieldCustom.ini Settings (not working yet)";
            tableLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox chkLooseFiles;
        private System.Windows.Forms.CheckBox chkMOTD;
        private System.Windows.Forms.CheckBox ckhPapyrusLogging;
        private System.Windows.Forms.CheckBox chkUserPhotos;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.CheckBox chkSkipIntro;
        private System.Windows.Forms.CheckBox chkMainMenuDelay;
    }
}