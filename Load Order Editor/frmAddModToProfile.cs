using Starfield_Tools.Common;
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
    public partial class frmAddModToProfile : Form
    {
        readonly Tools tools = new();
        string ModName;
        public frmAddModToProfile(List<string> items, string modName) // List of profiles and mod name
        {
            InitializeComponent();

            foreach (string item in items)
            {
                checkedListBox1.Items.Add(item);
            }
            this.Text = "Add " + modName + " to Profile"; // Change form title to name of mod being applied
            ModName = modName;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(Properties.Settings.Default.ProfileFolder))
            {
                foreach (var item in checkedListBox1.CheckedItems)
                {
                    File.AppendAllText(Properties.Settings.Default.ProfileFolder + "\\" + item, "*" +ModName + Environment.NewLine); // Add mod and set to active
                }
            }
            else
                return;

            this.Close();
        }
    }
}
