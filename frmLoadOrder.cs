using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Starfield_Tools
{
    public partial class frmLoadOrder : Form
    {
        public frmLoadOrder()
        {
            InitializeComponent();
            

            string loText;
            bool ModEnabled;

            loText = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Starfield\plugins.txt";

            using (StreamReader reader = new StreamReader(loText))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Process each line here
                    if (line[0] == '*')
                    {
                        ModEnabled = true;
                        line = line.Substring(1);
                    }
                    else
                        ModEnabled = false;
                    if (line[0] != '#')
                        dataGridView1.Rows.Add(ModEnabled, line);
                }
            }
        }


        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Starfield\Plugins.txt";
            bool ModEnabled;
            string ModLine;

            // Create or overwrite the file
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                for (int y = 0; y < dataGridView1.Rows.Count; y++)
                {
                    ModEnabled = (bool)dataGridView1.Rows[y].Cells[0].Value;
                    ModLine = (string)dataGridView1.Rows[y].Cells[1].Value;
                    if (ModEnabled)
                    {
                        writer.Write("*");
                        writer.WriteLine(dataGridView1.Rows[y].Cells[1].Value);
                    }
                    else
                        writer.WriteLine(ModLine);

                }


            }
            this.Close();
        }
    }
}
