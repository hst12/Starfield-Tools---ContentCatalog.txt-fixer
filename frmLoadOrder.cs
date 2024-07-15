using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using System.Text.Json;
using System.Drawing;


namespace Starfield_Tools
{
    public partial class frmLoadOrder : Form
    {
        ContentCatalog CC = new ContentCatalog();

        bool isModified = false;

        public frmLoadOrder()
        {
            InitializeComponent();
            string StarfieldPath = CC.GetStarfieldPath();
            Font FontSize = new Font("Microsoft Sans Serif", 14);
            this.Font = FontSize;
            toolStripStatusLabel1.Font = FontSize;
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
            bool ModEnabled;
            int EnabledCount = 0;
            string loText;
            string jsonFilePath = CC.GetCatalog();

            string json = File.ReadAllText(jsonFilePath);

            List<string> CreationsPlugin = new List<string>();
            List<string> CreationsTitle = new List<string>();
            List<string> CreationsFiles=new List<string>();
            List<string> CreationsVersion=new List<string>();

            int TitleCount = 0;
            int esmCount = 0;
            int espCount = 0;
            string StatText = "";

            Dictionary<string, object> json_Dictionary = (new JavaScriptSerializer()).Deserialize<Dictionary<string, object>>(json);
            try
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, Creation>>(json);
                data.Remove("ContentCatalog");
                foreach (var kvp in data)
                {
                    try
                    {
                        for (int i = 0; i < kvp.Value.Files.Length - 0; i++)
                        {
                            //CreationsFiles.Add(kvp.Value.Files[i]); 
                            if (kvp.Value.Files[i].IndexOf(".esm") > 0) // Look for .esm files
                            {
                                CreationsPlugin.Add(kvp.Value.Files[i]);
                               
                                TitleCount++;
                            }
                        }
                        CreationsTitle.Add(kvp.Value.Title); // Add Creations description to datagrid
                        CreationsVersion.Add(kvp.Value.Version);
                        CreationsFiles.Add(string.Join(", ",  kvp.Value.Files));

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}");
                    }
                }
            }
            catch { }

            loText = CC.GetStarfieldPath() + @"\plugins.txt";
            if (!File.Exists(loText))
            {
                MessageBox.Show(@"Missing Plugins.txt file

Click Ok and Ok again to create a blank Plugins.txt file or click Ok and Cancel to fix manually
Click Restore if you have a backup of your Plugins.txt file", "Plugins.txt not found");
                return;
            }
            using (var reader = new StreamReader(loText))
            {
                string line, Description,ModFiles,ModVersion;

                while ((line = reader.ReadLine()) != null)
                {
                    try
                    {
                        if (line != "")
                        {
                            if (line[0] == '*') // * = Mod enabled
                            {
                                ModEnabled = true;
                                EnabledCount++;
                                line = line.Substring(1);
                            }
                            else
                                ModEnabled = false;

                            if (line[0] != '#') // Ignore comment
                            {
                                Description = "";
                                ModFiles = "";
                                ModVersion = "";
                                for (int i = 0; i < CreationsPlugin.Count; i++)
                                {
                                    if (CreationsPlugin[i].Substring(0, CreationsPlugin[i].IndexOf('.')) + ".esm" == line)
                                    {
                                        Description = CreationsTitle[i]; // Add Content Catalog description if available
                                        ModVersion = CreationsVersion[i];
                                        ModFiles = CreationsFiles[i];
                                    }
                                }
                                dataGridView1.Rows.Add(ModEnabled, line, Description,ModVersion,ModFiles);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($"Error: {e.Message}");
                    }
                }
            }

            try
            {
                string directory = Properties.Settings.Default.StarfieldPath + @"\Data";
                foreach (var file in Directory.EnumerateFiles(directory, "*.esm", SearchOption.TopDirectoryOnly))
                {
                    esmCount++;
                }
                foreach (var file in Directory.EnumerateFiles(directory, "*.esp", SearchOption.TopDirectoryOnly))
                {
                    espCount++;
                }
                StatText = "Total Mods: " + dataGridView1.RowCount + ", Creations Mods: " + TitleCount.ToString() + ", Enabled: " +
            EnabledCount.ToString();
                if (esmCount > 0)
                {
                    StatText += ", esm files found: " + esmCount.ToString()+" (includes some base game esm files) ";

                }
                if (espCount > 0)
                {
                    StatText += ", esp files found: " + espCount.ToString();
                }

                toolStripStatusLabel1.Text = StatText;
            }
            catch (Exception ex)
            {
                toolStripStatusLabel1.Text = "Starfield path needs to be set";
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
            isModified = true;

            bool CurrentModEnabled;
            string CurrentModLine;
            string CurrentDescription;
            bool NewModEnabled;
            string NewModLine;
            string NewDescription;

            CurrentModEnabled = (bool)dataGridView1.Rows[y - 1].Cells[0].Value;
            CurrentModLine = (string)dataGridView1.Rows[y - 1].Cells[1].Value;
            CurrentDescription = (string)dataGridView1.Rows[y - 1].Cells[2].Value;

            NewModEnabled = (bool)dataGridView1.Rows[y].Cells[0].Value;
            NewModLine = (string)dataGridView1.Rows[y].Cells[1].Value;
            NewDescription = (string)dataGridView1.Rows[y].Cells[2].Value;


            dataGridView1.Rows[y].Cells[0].Value = CurrentModEnabled;
            dataGridView1.Rows[y].Cells[1].Value = CurrentModLine;
            dataGridView1.Rows[y].Cells[2].Value = CurrentDescription;

            dataGridView1.Rows[y - 1].Cells[0].Value = NewModEnabled;
            dataGridView1.Rows[y - 1].Cells[1].Value = NewModLine;
            dataGridView1.Rows[y - 1].Cells[2].Value = NewDescription;

            dataGridView1.Rows[y].Selected = false;
            dataGridView1.Rows[y - 1].Selected = true;

            dataGridView1.CurrentCell = dataGridView1.Rows[y - 1].Cells[0];

        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            int y = dataGridView1.CurrentCell.RowIndex;
            if (y > dataGridView1.Rows.Count - 2) return;
            isModified = true;
            bool CurrentModEnabled;
            string CurrentModLine;
            string CurrentDescription;
            bool NewModEnabled;
            string NewModLine;
            string NewDescription;

            CurrentModEnabled = (bool)dataGridView1.Rows[y + 1].Cells[0].Value;
            CurrentModLine = (string)dataGridView1.Rows[y + 1].Cells[1].Value;
            CurrentDescription = (string)dataGridView1.Rows[y + 1].Cells[2].Value;

            NewModEnabled = (bool)dataGridView1.Rows[y].Cells[0].Value;
            NewModLine = (string)dataGridView1.Rows[y].Cells[1].Value;
            NewDescription = (string)dataGridView1.Rows[y].Cells[2].Value;

            dataGridView1.Rows[y].Cells[0].Value = CurrentModEnabled;
            dataGridView1.Rows[y].Cells[1].Value = CurrentModLine;
            dataGridView1.Rows[y].Cells[2].Value = CurrentDescription;

            dataGridView1.Rows[y + 1].Cells[0].Value = NewModEnabled;
            dataGridView1.Rows[y + 1].Cells[1].Value = NewModLine;
            dataGridView1.Rows[y + 1].Cells[2].Value = NewDescription;

            dataGridView1.Rows[y].Selected = false;
            dataGridView1.Rows[y + 1].Selected = true;

            dataGridView1.CurrentCell = dataGridView1.Rows[y + 1].Cells[0];
        }


        private void btnBackupPlugins_Click(object sender, EventArgs e)
        {
            string sourceFileName = CC.GetStarfieldPath() + @"\Plugins.txt";
            string destFileName = sourceFileName + ".bak";

            if (isModified)
            {
                MessageBox.Show("Plugins have been modified\nClick Ok to save first or Cancel to revert", "Backkup not done");
                return;
            }

            try
            {
                // Copy the file
                File.Copy(sourceFileName, destFileName, true); // overwrite

                toolStripStatusLabel1.Text = "Backup done";
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

                toolStripStatusLabel1.ForeColor = DefaultForeColor;
                toolStripStatusLabel1.Text = "Restore done";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Restore failed");
            }
        }

        private void btnTop_Click(object sender, EventArgs e)
        {
            int y = dataGridView1.CurrentCell.RowIndex;
            if (y < 1) return;
            isModified = true;
            bool CurrentModEnabled;
            string CurrentModLine;
            string CurrentDescription;

            CurrentModEnabled = (bool)dataGridView1.Rows[y].Cells[0].Value;
            CurrentModLine = (string)dataGridView1.Rows[y].Cells[1].Value;
            CurrentDescription = (string)dataGridView1.Rows[y].Cells[2].Value;
            dataGridView1.Rows.Insert(0);

            dataGridView1.Rows[0].Cells[0].Value = CurrentModEnabled;
            dataGridView1.Rows[0].Cells[1].Value = CurrentModLine;
            dataGridView1.Rows[0].Cells[2].Value = CurrentDescription;

            dataGridView1.Rows.Remove(dataGridView1.CurrentRow);
            dataGridView1.CurrentCell = dataGridView1.Rows[0].Cells[0];
        }

        private void btnBottom_Click(object sender, EventArgs e)
        {
            int y = dataGridView1.CurrentCell.RowIndex;
            if (y > dataGridView1.Rows.Count - 1) return;
            isModified |= true;
            bool CurrentModEnabled;
            string CurrentModLine;
            string CurrentDescription;

            CurrentModEnabled = (bool)dataGridView1.Rows[y].Cells[0].Value;
            CurrentModLine = (string)dataGridView1.Rows[y].Cells[1].Value;
            CurrentDescription = (string)dataGridView1.Rows[y].Cells[2].Value;

            dataGridView1.Rows.Insert(dataGridView1.Rows.Count);

            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[0].Value = CurrentModEnabled;
            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value = CurrentModLine;
            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[2].Value = CurrentDescription;

            dataGridView1.Rows.Remove(dataGridView1.CurrentRow);
            dataGridView1.CurrentCell = dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[0];
        }

        private void dataGridView1_Sorted(object sender, EventArgs e)
        {
            //MessageBox.Show("Plugins sorted - changes will be written to Plugins.txt if Ok button is pressed. Use Cancel button to revert", "Warning!");
            toolStripStatusLabel1.Text = "Warning! - Plugins sorted - changes will be written to Plugins.txt if Ok button is pressed. Use Cancel button to revert";
            toolStripStatusLabel1.ForeColor = Color.Red;
        }

        private void btnDisable_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                dataGridView1.Rows[i].Cells[0].Value = false;
            }
            toolStripStatusLabel1.Text = "All mods disabled";
            isModified = true;
        }


    }
}