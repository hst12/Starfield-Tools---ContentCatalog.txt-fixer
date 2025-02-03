namespace Starfield_Tools.Load_Order_Editor
{
    partial class frmAddModToProfile
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
            checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            btnOk = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            tableLayoutPanel1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // checkedListBox1
            // 
            checkedListBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            checkedListBox1.FormattingEnabled = true;
            checkedListBox1.Location = new System.Drawing.Point(3, 3);
            checkedListBox1.Name = "checkedListBox1";
            checkedListBox1.Size = new System.Drawing.Size(794, 391);
            checkedListBox1.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 0, 1);
            tableLayoutPanel1.Controls.Add(checkedListBox1, 0, 0);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.Size = new System.Drawing.Size(800, 450);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(btnOk);
            flowLayoutPanel1.Controls.Add(btnCancel);
            flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            flowLayoutPanel1.Location = new System.Drawing.Point(3, 400);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new System.Drawing.Size(794, 47);
            flowLayoutPanel1.TabIndex = 2;
            // 
            // btnOk
            // 
            btnOk.Location = new System.Drawing.Point(3, 3);
            btnOk.Name = "btnOk";
            btnOk.Size = new System.Drawing.Size(150, 46);
            btnOk.TabIndex = 0;
            btnOk.Text = "Ok";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Click += btnOk_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new System.Drawing.Point(159, 3);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(150, 46);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // frmAddModToProfile
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(tableLayoutPanel1);
            Name = "frmAddModToProfile";
            Text = "Add mod to the following profiles";
            tableLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.CheckedListBox checkedListBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
    }
}