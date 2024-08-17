namespace Starfield_Tools
{
    partial class FormTreeView
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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Armour");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Zone79");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Replacers", new System.Windows.Forms.TreeNode[] { treeNode2 });
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Zone79");
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("New", new System.Windows.Forms.TreeNode[] { treeNode4 });
            System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("Clothing", new System.Windows.Forms.TreeNode[] { treeNode3, treeNode5 });
            System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("Consteallation");
            System.Windows.Forms.TreeNode treeNode8 = new System.Windows.Forms.TreeNode("Replacers", new System.Windows.Forms.TreeNode[] { treeNode7 });
            System.Windows.Forms.TreeNode treeNode9 = new System.Windows.Forms.TreeNode("Star Wars");
            System.Windows.Forms.TreeNode treeNode10 = new System.Windows.Forms.TreeNode("New", new System.Windows.Forms.TreeNode[] { treeNode9 });
            System.Windows.Forms.TreeNode treeNode11 = new System.Windows.Forms.TreeNode("NPCs", new System.Windows.Forms.TreeNode[] { treeNode8, treeNode10 });
            System.Windows.Forms.TreeNode treeNode12 = new System.Windows.Forms.TreeNode("Mods");
            System.Windows.Forms.TreeNode treeNode13 = new System.Windows.Forms.TreeNode("Skins");
            System.Windows.Forms.TreeNode treeNode14 = new System.Windows.Forms.TreeNode("Weapons", new System.Windows.Forms.TreeNode[] { treeNode12, treeNode13 });
            treeView1 = new System.Windows.Forms.TreeView();
            SuspendLayout();
            // 
            // treeView1
            // 
            treeView1.Location = new System.Drawing.Point(24, 25);
            treeView1.Name = "treeView1";
            treeNode1.Name = "Node5";
            treeNode1.Text = "Armour";
            treeNode2.Name = "Node13";
            treeNode2.Text = "Zone79";
            treeNode3.Name = "Node0";
            treeNode3.Text = "Replacers";
            treeNode4.Name = "Node12";
            treeNode4.Text = "Zone79";
            treeNode5.Name = "Node11";
            treeNode5.Text = "New";
            treeNode6.Name = "Node1";
            treeNode6.Text = "Clothing";
            treeNode7.Name = "Node9";
            treeNode7.Text = "Consteallation";
            treeNode8.Name = "Node7";
            treeNode8.Text = "Replacers";
            treeNode9.Name = "Node6";
            treeNode9.Text = "Star Wars";
            treeNode10.Name = "Node10";
            treeNode10.Text = "New";
            treeNode11.Name = "Node8";
            treeNode11.Text = "NPCs";
            treeNode12.Name = "Node4";
            treeNode12.Text = "Mods";
            treeNode13.Name = "Node3";
            treeNode13.Text = "Skins";
            treeNode14.Name = "Node2";
            treeNode14.Text = "Weapons";
            treeView1.Nodes.AddRange(new System.Windows.Forms.TreeNode[] { treeNode1, treeNode6, treeNode11, treeNode14 });
            treeView1.Size = new System.Drawing.Size(694, 522);
            treeView1.TabIndex = 0;
            // 
            // FormTreeView
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 608);
            Controls.Add(treeView1);
            Name = "FormTreeView";
            Text = "FormTreeView";
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TreeView treeView1;
    }
}