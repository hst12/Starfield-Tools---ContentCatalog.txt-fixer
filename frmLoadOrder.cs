using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using System.Text.Json;


namespace Starfield_Tools
{
    public partial class frmLoadOrder : Form
    {
        ContentCatalog CC = new ContentCatalog();

        public frmLoadOrder()
        {
            InitializeComponent();
            InitDataGrid();
        }

        public class Creation
        {
            public bool AchievementSafe { get; set; }
            public string[] Files { get; set; }
            public long FilesSize { get; set; }
            public long Timestamp { get; set; }
            public string Title { get; set; }
            public string Version { get; set; }
        }

        private void InitDataGrid()
        {
            string loText;
            bool ModEnabled;

            string jsonFilePath = CC.GetCatalog();

            string json = File.ReadAllText(jsonFilePath);
            Dictionary<string, object> json_Dictionary = (new JavaScriptSerializer()).Deserialize<Dictionary<string, object>>(json);
            var data = JsonSerializer.Deserialize<Dictionary<string, Creation>>(json);
            data.Remove("ContentCatalog");

            List<string> CreationsPlugin = new List<string>();
            List<string> CreationsTitle = new List<string>();
            int TitleCount = 0;

            foreach (var kvp in data)
            {
                try
                {
                    CreationsPlugin.Add(string.Join(",", kvp.Value.Files));
                    CreationsTitle.Add(kvp.Value.Title);
                    TitleCount++;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");

                }
            }

            loText = CC.GetStarfieldPath() + @"\plugins.txt";

            using (StreamReader reader = new StreamReader(loText))
            {
                string line,Description,Files;

                while ((line = reader.ReadLine()) != null)
                {
                    try
                    {
                        if (line != "")
                        {
                            if (line[0] == '*')
                            {
                                ModEnabled = true;
                                line = line.Substring(1);
                            }
                            else
                                ModEnabled = false;

                            if (line[0] != '#')
                            {
                                Description = "";
                                Files = "";

                                for (int i = 0; i < CreationsPlugin.Count; i++)
                                {
                                    if (CreationsPlugin[i] == line)  
                                    {
                                        Description = CreationsTitle[i];
                                    }
                                }
                                dataGridView1.Rows.Add(ModEnabled, line,Description);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($"Error: {e.Message}");
                    }
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string filePath = CC.GetStarfieldPath() + @"\Plugins.txt";
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

        private void btnUp_Click(object sender, EventArgs e)
        {
            int y = dataGridView1.CurrentCell.RowIndex;
            if (y < 1) return;
            bool CurrentModEnabled;
            string CurrentModLine;
            bool NewModEnabled;
            string NewModLine;

            CurrentModEnabled = (bool)dataGridView1.Rows[y - 1].Cells[0].Value;
            CurrentModLine = (string)dataGridView1.Rows[y - 1].Cells[1].Value;
            NewModEnabled = (bool)dataGridView1.Rows[y].Cells[0].Value;
            NewModLine = (string)dataGridView1.Rows[y].Cells[1].Value;


            dataGridView1.Rows[y].Cells[0].Value = CurrentModEnabled;
            dataGridView1.Rows[y].Cells[1].Value = CurrentModLine;

            dataGridView1.Rows[y - 1].Cells[0].Value = NewModEnabled;
            dataGridView1.Rows[y - 1].Cells[1].Value = NewModLine;

            dataGridView1.Rows[y].Selected = false;
            dataGridView1.Rows[y - 1].Selected = true;

            dataGridView1.CurrentCell = dataGridView1.Rows[y - 1].Cells[0];

        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            int y = dataGridView1.CurrentCell.RowIndex;
            if (y > dataGridView1.Rows.Count - 2) return;
            bool CurrentModEnabled;
            string CurrentModLine;
            bool NewModEnabled;
            string NewModLine;

            CurrentModEnabled = (bool)dataGridView1.Rows[y + 1].Cells[0].Value;
            CurrentModLine = (string)dataGridView1.Rows[y + 1].Cells[1].Value;
            NewModEnabled = (bool)dataGridView1.Rows[y].Cells[0].Value;
            NewModLine = (string)dataGridView1.Rows[y].Cells[1].Value;


            dataGridView1.Rows[y].Cells[0].Value = CurrentModEnabled;
            dataGridView1.Rows[y].Cells[1].Value = CurrentModLine;

            dataGridView1.Rows[y + 1].Cells[0].Value = NewModEnabled;
            dataGridView1.Rows[y + 1].Cells[1].Value = NewModLine;

            dataGridView1.Rows[y].Selected = false;
            dataGridView1.Rows[y + 1].Selected = true;

            dataGridView1.CurrentCell = dataGridView1.Rows[y + 1].Cells[0];
        }


        private void btnBackupPlugins_Click(object sender, EventArgs e)
        {
            string sourceFileName = CC.GetStarfieldPath() + @"\Plugins.txt";
            string destFileName = sourceFileName + ".bak";

            try
            {
                // Copy the file
                File.Copy(sourceFileName, destFileName, true); // overwrite

                //toolStripStatusLabel1.Text = "Backup done";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Backup failed");
            }
        }

        private void btnRestorePlugins_Click(object sender, EventArgs e)
        {
            string sourceFileName = CC.GetStarfieldPath() + @"\Plugins.txt.bak";
            string destFileName = CC.GetStarfieldPath() + @"\Plugins.txt";

            try
            {
                // Copy the file
                File.Copy(sourceFileName, destFileName, true); // overwrite
                dataGridView1.Rows.Clear();
                InitDataGrid();

                //toolStripStatusLabel1.Text = "Restore done";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Restore failed");
            }
        }







        private void btnTop_Click(object sender, EventArgs e)
        {

        }
    }
}
