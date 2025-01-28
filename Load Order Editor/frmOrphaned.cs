using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Starfield_Tools.Load_Order_Editor
{
    public partial class frmOrphaned : Form
    {
        public frmOrphaned(List<string> orphaned)
        {
            InitializeComponent();
            foreach (var item in orphaned)
            {
                checkedListBox1.Items.Add(Path.GetFileName(item));
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            foreach (int i in Enumerable.Range(0, checkedListBox1.Items.Count))
            {
                checkedListBox1.SetItemChecked(i, true);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            foreach (var item in checkedListBox1.CheckedItems)
            {
                try
                {
                    File.Delete(frmLoadOrder.StarfieldGamePath + "\\Data\\" + item.ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting file: " + ex.Message);
                }
            }

        }
    }
}
