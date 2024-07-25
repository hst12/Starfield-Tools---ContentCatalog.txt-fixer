using Starfield_Tools.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace Starfield_Tools
{
    public partial class frmLoadOrder : Form
    {
        ContentCatalog CC = new ContentCatalog();

        bool isModified = false;

        public frmLoadOrder()
        {
            InitializeComponent();

            menuStrip1.Font = Properties.Settings.Default.FontSize;
            this.Font = Properties.Settings.Default.FontSize;
            string StarfieldPath = CC.GetStarfieldPath();
            InitDataGrid();

        }

        private void InitDataGrid()
        {
            bool ModEnabled;
            int EnabledCount = 0;
            string loText;

            if (!File.Exists(CC.GetCatalog()))
            {
                MessageBox.Show("Missing ContentCatalog.txt");
                return;
            }

            dataGridView1.Rows.Clear();

            string jsonFilePath = CC.GetCatalog();
            string json = File.ReadAllText(jsonFilePath); // Read catalog


            List<string> CreationsPlugin = new List<string>();
            List<string> CreationsTitle = new List<string>();
            List<string> CreationsFiles = new List<string>();
            List<string> CreationsVersion = new List<string>();
            List<bool> AchievmentSafe = new List<bool>();
            List<long> TimeStamp = new List<long>();

            int TitleCount = 0;
            int esmCount = 0;
            int espCount = 0;
            string StatText = "";
            double VersionCheck;
            List<string> esmFiles = new List<string>();

            try
            {
                Dictionary<string, object> json_Dictionary = (new JavaScriptSerializer()).Deserialize<Dictionary<string, object>>(json);
            }
            catch { }
            try
            {
                var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, ContentCatalog.Creation>>(json);
                data.Remove("ContentCatalog");
                foreach (var kvp in data)
                {
                    try
                    {
                        for (int i = 0; i < kvp.Value.Files.Length - 0; i++)
                        {
                            if (kvp.Value.Files[i].IndexOf(".esm") > 0) // Look for .esm files
                            {
                                CreationsPlugin.Add(kvp.Value.Files[i]);

                                TitleCount++;
                            }
                        }
                        CreationsTitle.Add(kvp.Value.Title); // Add Creations description to datagrid

                        CreationsVersion.Add(kvp.Value.Version);
                        CreationsFiles.Add(string.Join(", ", kvp.Value.Files));
                        AchievmentSafe.Add(kvp.Value.AchievementSafe);
                        TimeStamp.Add(kvp.Value.Timestamp);

                    }
                    catch (Exception ex)
                    {
                        toolStripStatusLabel1.Text = ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                toolStripStatusLabel1.Text = ex.Message;
            }

            loText = CC.GetStarfieldPath() + @"\plugins.txt";
            if (!File.Exists(loText))
            {
                MessageBox.Show(@"Missing Plugins.txt file

Click Ok and Ok again to create a blank Plugins.txt file or click Ok and Cancel to fix manually
Click Restore if you have a backup of your Plugins.txt file
Altenatively, run the game once to have it create a Plugins.txt file for you.", "Plugins.txt not found");
                return;
            }
            using (var reader = new StreamReader(loText))
            {
                string line, Description, ModFiles, ModVersion, ASafe, ModTimeStamp;

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
                                ASafe = "";
                                ModTimeStamp = "";

                                for (int i = 0; i < CreationsPlugin.Count; i++)
                                {
                                    if (CreationsPlugin[i].Substring(0, CreationsPlugin[i].IndexOf('.')) + ".esm" == line)
                                    {
                                        Description = CreationsTitle[i]; // Add Content Catalog description if available
                                        ModVersion = CreationsVersion[i];
                                        VersionCheck = double.Parse((ModVersion.Substring(0, ModVersion.IndexOf('.'))));
                                        ModVersion = CC.ConvertTime(VersionCheck).ToString() + " " + ModVersion.Substring(ModVersion.IndexOf('.') + 1) + "\n";

                                        ModFiles = CreationsFiles[i];
                                        if (AchievmentSafe[i])
                                            ASafe = "Yes";
                                        else
                                            ASafe = "";
                                        ModTimeStamp = CC.ConvertTime(TimeStamp[i]).ToString();
                                    }
                                }
                                dataGridView1.Rows.Add(ModEnabled, line, Description, ModVersion, ModFiles, ASafe, ModTimeStamp);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        toolStripStatusLabel1.Text = ex.Message;
                    }
                }
            }

            try
            {
                string directory = Properties.Settings.Default.StarfieldPath + @"\Data";
                foreach (var file in Directory.EnumerateFiles(directory, "*.esm", SearchOption.TopDirectoryOnly))
                {
                    esmCount++;
                    esmFiles.Add(file);
                }
                foreach (var file in Directory.EnumerateFiles(directory, "*.esp", SearchOption.TopDirectoryOnly))
                {
                    espCount++;
                }
                StatText = "Total Mods: " + dataGridView1.RowCount + ", Creations Mods: " + TitleCount.ToString() + ", Other: " + (dataGridView1.RowCount - TitleCount).ToString() + ", Enabled: " +
            EnabledCount.ToString();
                if (esmCount > 0)
                {
                    StatText += ", esm files: " + esmCount.ToString() + " ";

                }
                if (espCount > 0)
                {
                    StatText += ", esp files: " + espCount.ToString();
                }

                toolStripStatusLabel1.Text = StatText;
                /*foreach (var item in esmFiles)
                {
                    Console.WriteLine(item); // Do something here to automatically add missing .esm files to Plugins.txt
                }*/
            }
            catch
            {
                toolStripStatusLabel1.Text = "Starfield path needs to be set for mod stats";
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SaveLO(string PluginFileName)
        {

            bool ModEnabled;
            string ModLine;

            // Create or overwrite the file
            using (StreamWriter writer = new StreamWriter(PluginFileName))
            {
                writer.Write("# This file is used by Starfield to keep track of your downloaded content.\r\n# Please do not modify this file.\r\n");
                for (int y = 0; y < dataGridView1.Rows.Count; y++)
                {
                    ModEnabled = (bool)dataGridView1.Rows[y].Cells[0].Value;
                    ModLine = (string)dataGridView1.Rows[y].Cells[1].Value;
                    if (ModEnabled)
                        writer.Write("*"); // Write a * for enalbed mods then write the mod filename
                    writer.WriteLine(ModLine);
                }
            }
            toolStripStatusLabel1.Text = "Plugins.txt saved";
            isModified = false;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            SaveLO(CC.GetStarfieldPath() + @"\Plugins.txt");
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

        private void BackupPlugins()
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

        private void btnBackupPlugins_Click(object sender, EventArgs e)
        {
            BackupPlugins();
        }

        private void RestorePlugins()
        {
            string sourceFileName = CC.GetStarfieldPath() + @"\Plugins.txt.bak";
            string destFileName = CC.GetStarfieldPath() + @"\Plugins.txt";

            try
            {
                // Copy the file
                File.Copy(sourceFileName, destFileName, true); // overwrite
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
            toolStripStatusLabel1.Text = "Warning! - Plugins sorted - saving changes disabled";
            toolStripStatusLabel1.ForeColor = Color.Red;
            btnOK.Enabled = false;
        }

        private void DisableAll()
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                dataGridView1.Rows[i].Cells[0].Value = false;
            }
            toolStripStatusLabel1.Text = "All mods disabled";
            isModified = true;
        }

        private void EnableAll()
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                dataGridView1.Rows[i].Cells[0].Value = true;
            }
            toolStripStatusLabel1.Text = "All mods enabled";
            isModified = true;
        }

        private void FontSelect()
        {
            if (fontDialog1.ShowDialog() != DialogResult.Cancel)
            {
                this.Font = fontDialog1.Font;
                menuStrip1.Font = fontDialog1.Font;
            }
            this.CenterToScreen();
            Properties.Settings.Default.FontSize = this.Font;
        }

        private void SearchMod()
        {
            int ModIndex;
            string DataGridSring, TextBoxString;
            if (txtSearchBox.Text == "")
                return;
            TextBoxString = txtSearchBox.Text.ToLower(); // Do lower case only search

            for (ModIndex = 0; ModIndex < dataGridView1.RowCount; ModIndex++)
            {
                DataGridSring = dataGridView1.Rows[ModIndex].Cells[1].Value.ToString().ToLower();
                Console.WriteLine(DataGridSring);
                if (DataGridSring.Contains(TextBoxString))
                {
                    toolStripStatusLabel1.Text = "Found " + txtSearchBox.Text + " in " + dataGridView1.Rows[ModIndex].Cells[1].Value.ToString();
                    dataGridView1.CurrentCell = dataGridView1.Rows[ModIndex].Cells[1];
                    break;
                }
                else
                    toolStripStatusLabel1.Text = txtSearchBox.Text + " not found ";
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CC.ShowAbout();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveLO(CC.GetStarfieldPath() + @"\Plugins.txt");
        }

        private void toolStripMenuBackup_Click(object sender, EventArgs e)
        {
            BackupPlugins();
        }

        private void toolStripMenuRestore_Click(object sender, EventArgs e)
        {
            RestorePlugins();
        }

        private void toolStripMenuEnableAll_Click(object sender, EventArgs e)
        {
            EnableAll();
        }

        private void toolStripMenuDisableAll_Click(object sender, EventArgs e)
        {
            DisableAll();
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FontSelect();
        }

        private void toolStripMenuCreations_Click(object sender, EventArgs e)
        {
            Process.Start("https://creations.bethesda.net/en/starfield/all?sort=latest_uploaded");
        }

        private void toolStripMenuNexus_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.nexusmods.com/starfield");
        }

        private void txtSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                SearchMod();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string ProfileFolder;

            dataGridView1.CurrentCell = dataGridView1.Rows[0].Cells[0];

            SaveFileDialog SavePlugins = new SaveFileDialog();
            ProfileFolder = Properties.Settings.Default.ProfileFolder;
            if (ProfileFolder == null || ProfileFolder == "")
                ProfileFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            SavePlugins.InitialDirectory = ProfileFolder;
            SavePlugins.Filter = "Txt File|*.txt";
            SavePlugins.Title = "Save Profile";

            DialogResult result = SavePlugins.ShowDialog();
            if (DialogResult.OK == result)
            {
                SaveLO(SavePlugins.FileName);
            }
            if (SavePlugins.FileName != "")
            {
                Properties.Settings.Default.ProfileFolder = SavePlugins.FileName.Substring(SavePlugins.FileName.LastIndexOf('\\'));
                Properties.Settings.Default.Save();
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string ProfileFolder;

            ProfileFolder = Properties.Settings.Default.ProfileFolder;
            if (ProfileFolder == null || ProfileFolder == "")
                ProfileFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            OpenFileDialog OpenPlugins = new OpenFileDialog();
            OpenPlugins.InitialDirectory = ProfileFolder;
            OpenPlugins.Filter = "Txt File|*.txt";
            OpenPlugins.Title = "Load Profile";

            DialogResult result = OpenPlugins.ShowDialog();
            if (DialogResult.OK == result)
            {
                File.Copy(OpenPlugins.FileName, CC.GetStarfieldPath() + "\\Plugins.txt", true);
                InitDataGrid();
            }
            if (OpenPlugins.FileName != "")
            {
                Properties.Settings.Default.ProfileFolder = OpenPlugins.FileName.Substring(OpenPlugins.FileName.LastIndexOf('\\'));
                Properties.Settings.Default.Save();
            }
        }

        private void toolStripMenuAdd_Click(object sender, EventArgs e)
        {
            string SteamData;

            SteamData=Settings.Default.StarfieldPath +"\\Data";
            OpenFileDialog GetMod =new OpenFileDialog();
            GetMod.Filter = "esm File|*.esm";
            GetMod.Title = "Select mod";
            GetMod.InitialDirectory = SteamData;

            DialogResult result=GetMod.ShowDialog();
            if (DialogResult.OK == result)
            {
                toolStripStatusLabel1.Text = GetMod.FileName;
                SteamData = GetMod.FileName.Substring(GetMod.FileName.LastIndexOf('\\'));
                dataGridView1.Rows.Add(true, SteamData.Substring(1));
                dataGridView1.CurrentCell = dataGridView1.Rows[dataGridView1.RowCount-1].Cells[1];
            }
        }

        private void toolStripMenuDelete_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.RemoveAt(dataGridView1.CurrentRow.Index);
        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                dataGridView1.Rows.RemoveAt(dataGridView1.CurrentRow.Index);

        }
    }
}