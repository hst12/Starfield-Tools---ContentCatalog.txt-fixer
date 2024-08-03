using Starfield_Tools.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using File = System.IO.File;


namespace Starfield_Tools
{


    public partial class frmLoadOrder : Form
    {
        ContentCatalog CC = new ContentCatalog();
        public string StarfieldGamePath;

        bool isModified = false, Profiles = false;

        public frmLoadOrder()
        {
            InitializeComponent();
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(KeyEvent);

            toolStripMenuInstall.Enabled = true;
            toolStripMenuUninstall.Enabled = true;

            string PluginsPath = CC.GetStarfieldPath() + "\\Plugins.txt";
            if (!File.Exists(PluginsPath + ".bak")) // Do a 1-time backup of Plugins.txt
            {
                toolStripStatusLabel1.Text = "Plugins.txt backed up to Plugins.txt.bak";
                File.Copy(PluginsPath, PluginsPath + ".bak");
            }
            menuStrip1.Font = Settings.Default.FontSize;
            this.Font = Settings.Default.FontSize;
            StarfieldGamePath = Settings.Default.StarfieldGamePath;
            InitDataGrid();
            cmbProfile.Enabled = Profiles;
            GetProfiles();
        }

        private void RefreshDataGrid()
        {
            InitDataGrid();
            GetProfiles();
        }

        private void KeyEvent(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                RefreshDataGrid();
            }
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
            string json = System.IO.File.ReadAllText(jsonFilePath); // Read catalog


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
                                        ModVersion = CC.ConvertTime(VersionCheck).ToString() + ", v" + ModVersion.Substring(ModVersion.IndexOf('.') + 1) + "\n";

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
                string directory = Settings.Default.StarfieldGamePath + @"\Data";
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

        private void GetProfiles()
        {
            string ProfileFolder;
            if (!Profiles)
                return;
            cmbProfile.Items.Clear();
            ProfileFolder = Settings.Default.ProfileFolder;
            if (ProfileFolder == null || ProfileFolder == "")
                ProfileFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            try
            {
                foreach (var profileName in Directory.EnumerateFiles(ProfileFolder, "*.txt", SearchOption.TopDirectoryOnly))
                {
                    cmbProfile.Items.Add(profileName.Substring(profileName.LastIndexOf('\\') + 1));

                }
                int index = cmbProfile.Items.IndexOf(Settings.Default.LastProfile);
                if (index != -1)
                {
                    cmbProfile.SelectedIndex = index; // Set the ComboBox to the found index
                }
            }
            catch { }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SaveLO(string PluginFileName)
        {

            bool ModEnabled;
            string ModLine;

            dataGridView1.CurrentCell = dataGridView1.Rows[0].Cells[0];

            using (StreamWriter writer = new StreamWriter(PluginFileName))
            {
                writer.Write("# This file is used by Starfield to keep track of your downloaded content.\r\n# Please do not modify this file.\r\n");
                for (int y = 0; y < dataGridView1.Rows.Count; y++)
                {
                    ModEnabled = (bool)dataGridView1.Rows[y].Cells[0].Value;
                    ModLine = (string)dataGridView1.Rows[y].Cells[1].Value;
                    if (ModEnabled)
                        writer.Write("*"); // Insert a * for enabled mods then write the mod filename
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
        private void MoveUp()
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
        private void btnUp_Click(object sender, EventArgs e)
        {
            MoveUp();
        }

        private void MoveDown()
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
        private void btnDown_Click(object sender, EventArgs e)
        {
            MoveDown();
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
                isModified = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Restore failed");
            }
        }

        private void MoveTop()
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

        private void MoveBottom()
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
        private void btnBottom_Click(object sender, EventArgs e)
        {
            MoveBottom();
        }

        private void btnTop_Click(object sender, EventArgs e)
        {
            MoveTop();
        }

        private void dataGridView1_Sorted(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "Warning! - Plugins sorted - saving changes disabled";
            toolStripStatusLabel1.ForeColor = Color.Red;
            btnOK.Enabled = false;
            saveToolStripMenuItem.Enabled = false;
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
            Settings.Default.FontSize = this.Font;
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
            ProfileFolder = Settings.Default.ProfileFolder;
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
                Settings.Default.ProfileFolder = SavePlugins.FileName.Substring(0, SavePlugins.FileName.LastIndexOf('\\'));
                Settings.Default.Save();
                GetProfiles();
                isModified = false;
            }
        }

        private void SwitchProfile(string ProfileName)
        {
            if (!Profiles)
                return;
            try
            {
                File.Copy(ProfileName, CC.GetStarfieldPath() + "\\Plugins.txt", true);
                Settings.Default.LastProfile = ProfileName.Substring(ProfileName.LastIndexOf('\\') + 1);
                SaveSettings();
                isModified = false;
            }
            catch
            {
                toolStripStatusLabel1.Text = "Error switching profile";
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string ProfileFolder;

            ProfileFolder = Settings.Default.ProfileFolder;
            if (ProfileFolder == null || ProfileFolder == "")
                ProfileFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            OpenFileDialog OpenPlugins = new OpenFileDialog();
            OpenPlugins.InitialDirectory = ProfileFolder;
            OpenPlugins.Filter = "Txt File|*.txt";
            OpenPlugins.Title = "Load Profile";

            DialogResult result = OpenPlugins.ShowDialog();
            if (DialogResult.OK == result)
            {

                InitDataGrid();

            }
            if (OpenPlugins.FileName != "")
            {
                Settings.Default.ProfileFolder = OpenPlugins.FileName.Substring(0, OpenPlugins.FileName.LastIndexOf('\\'));
                SwitchProfile(OpenPlugins.FileName);
                GetProfiles();
                Settings.Default.Save();
            }
        }

        private void toolStripMenuAdd_Click(object sender, EventArgs e)
        {
            string GameData;

            GameData = Settings.Default.StarfieldGamePath + "\\Data";
            OpenFileDialog GetMod = new OpenFileDialog();
            GetMod.Filter = "esm File|*.esm";
            GetMod.Title = "Select mod";
            GetMod.InitialDirectory = GameData;

            DialogResult result = GetMod.ShowDialog();
            if (DialogResult.OK == result)
            {
                toolStripStatusLabel1.Text = GetMod.FileName;
                GameData = GetMod.FileName.Substring(GetMod.FileName.LastIndexOf('\\'));
                dataGridView1.Rows.Add(true, GameData.Substring(1));
                dataGridView1.CurrentCell = dataGridView1.Rows[dataGridView1.RowCount - 1].Cells[1];
                isModified = true;
            }
        }

        private void DeleteLine()
        {
            dataGridView1.Rows.RemoveAt(dataGridView1.CurrentRow.Index);
            isModified = true;
        }

        private void toolStripMenuDelete_Click(object sender, EventArgs e)
        {
            DeleteLine();
        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                dataGridView1.Rows.RemoveAt(dataGridView1.CurrentRow.Index);

        }

        private void toolStripMenuStats_Click(object sender, EventArgs e)
        {
            RefreshDataGrid();
        }

        private void toolStripMenuScanMods_Click(object sender, EventArgs e)
        {
            AddMissing();
        }

        private void toolStripMenuSetPath_Click(object sender, EventArgs e)
        {
            StarfieldGamePath = CC.SetStarfieldGamePath();
        }

        private void toolStripMenuCleanup_Click(object sender, EventArgs e)
        {
            RemoveMissing();
        }

        private int AddMissing() // Look for .esm files to add to Plugins.txt returns no. of file added
        {
            int AddedFiles = 0;
            List<string> esmFiles = new List<string>();
            List<string> PluginFiles = new List<string>();
            List<string> BethFiles = new List<string> // Exclude game files - will probably need updating after DLC release
            {
                "BlueprintShips-Starfield.esm","Constellation.esm","OldMars.esm","SFBGS003.esm","SFBGS006.esm","SFBGS007.esm","SFBGS008.esm","Starfield.esm"
            };

            string directory = Settings.Default.StarfieldGamePath;
            if (directory == "" || directory == null)
                directory = CC.SetStarfieldGamePath();
            directory = directory + @"\Data";
            if (directory == "\\Data")
            {
                MessageBox.Show("Can't continue without game installation path");
                return 0;
            }
            foreach (var missingFile in Directory.EnumerateFiles(directory, "*.esm", SearchOption.TopDirectoryOnly))
            {
                esmFiles.Add(missingFile.Substring(missingFile.LastIndexOf('\\') + 1));
            }
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                PluginFiles.Add((string)dataGridView1.Rows[i].Cells[1].Value);
            List<string> MissingFiles = esmFiles.Except(PluginFiles).ToList();

            List<string> FilesToAdd = MissingFiles.Except(BethFiles).ToList();  // Exclude BGS esm files
            if (FilesToAdd.Count > 0)
            {
                for (int i = 0; i < FilesToAdd.Count; i++)
                {
                    AddedFiles++;
                    dataGridView1.Rows.Add(true, FilesToAdd[i]);
                }
                dataGridView1.CurrentCell = dataGridView1.Rows[dataGridView1.RowCount - 1].Cells[1];
                toolStripStatusLabel1.Text = AddedFiles.ToString() + " file(s) added";
                isModified = true;
            }
            else
                toolStripStatusLabel1.Text = "Nothing to add";
            return AddedFiles;
        }


        private int RemoveMissing() // Remove entries from Plugins.txt for missing .esm files. Returns number of removals
        {
            int RemovedFiles = 0;
            List<string> esmFiles = new List<string>();
            List<string> PluginFiles = new List<string>();

            List<string> BethFiles = new List<string>  // Exclude BGS files
            {
                "BlueprintShips-Starfield.esm","Constellation.esm","OldMars.esm","SFBGS003.esm","SFBGS006.esm","SFBGS007.esm","SFBGS008.esm","Starfield.esm"
            };

            string directory = Settings.Default.StarfieldGamePath;
            if (directory == "" || directory == null)
                directory = CC.SetStarfieldGamePath();
            directory = directory + @"\Data";
            if (directory == "\\Data")
            {
                MessageBox.Show("Can't continue without game installation path");
                return 0;
            }
            foreach (var missingFile in Directory.EnumerateFiles(directory, "*.esm", SearchOption.TopDirectoryOnly))
            {
                esmFiles.Add(missingFile.Substring(missingFile.LastIndexOf('\\') + 1));
            }
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                PluginFiles.Add((string)dataGridView1.Rows[i].Cells[1].Value);
            List<string> MissingFiles = PluginFiles.Except(esmFiles).ToList();

            List<string> FilesToRemove = MissingFiles.Except(BethFiles).ToList();
            if (FilesToRemove.Count > 0)
            {
                for (int i = 0; i < FilesToRemove.Count; i++)
                {
                    RemovedFiles++;

                    for (int j = 0; j < dataGridView1.Rows.Count; j++)
                        if ((string)dataGridView1.Rows[j].Cells[1].Value == FilesToRemove[i])
                            dataGridView1.Rows.RemoveAt(j);
                }
                toolStripStatusLabel1.Text = RemovedFiles.ToString() + " file(s) removed";
                isModified = true;
            }
            else
                toolStripStatusLabel1.Text = "Nothing to remove";
            return RemovedFiles;
        }

        private void AddRemove()
        {
            int addedMods = 0, removedMods = 0;
            addedMods = AddMissing();
            removedMods = RemoveMissing();
            toolStripStatusLabel1.Text = addedMods.ToString() + " Mods added, " + removedMods.ToString() + " Mods removed";
            if (addedMods + removedMods > 0)
                toolStripStatusLabel1.Text += " - Save changes to update Plugins.txt file";

        }

        private void toolStripMenuAutoClean_Click(object sender, EventArgs e)
        {
            AddRemove();
        }


        private void frmLoadOrder_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
        }

        private void SaveSettings()
        {
            Settings.Default.Save();
        }

        private void cmbProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            Console.WriteLine("changed");

            SwitchProfile(Settings.Default.ProfileFolder + "\\" + (string)cmbProfile.SelectedItem);
            InitDataGrid();
        }

        private void InstallMod()
        {
            string ModPath;
            string ExtractPath = Path.GetTempPath() + "hstTools";

            OpenFileDialog OpenMod = new OpenFileDialog();
            //OpenMod.InitialDirectory = ProfileFolder;
            OpenMod.Filter = "ZIP File|*.zip";
            OpenMod.Title = "Install Mod";

            DialogResult result = OpenMod.ShowDialog();
            ModPath = OpenMod.FileName;

            if (OpenMod.FileName != "")
            {
                try
                {
                    toolStripStatusLabel1.Text = "Installing mod...";
                    ZipFile.ExtractToDirectory(ModPath, ExtractPath);

                }
                catch (Exception ex)
                {
                    toolStripStatusLabel1.Text = ex.Message;

                }
                foreach (string ModFile in Directory.EnumerateFiles(ExtractPath, "*.esm", SearchOption.AllDirectories)) // Move .esm files
                {
                    string fileName = Path.GetFileName(ModFile);
                    string destinationPath = Path.Combine(StarfieldGamePath + "\\Data", fileName);

                    if (!File.Exists(destinationPath))
                        File.Move(ModFile, destinationPath);
                }

                foreach (string ModFile in Directory.EnumerateFiles(ExtractPath, "*.ba2", SearchOption.AllDirectories)) // Move archives
                {
                    string fileName = Path.GetFileName(ModFile);
                    string destinationPath = Path.Combine(StarfieldGamePath + "\\Data", fileName);

                    if (!File.Exists(destinationPath))
                        File.Move(ModFile, destinationPath);
                }

                AddMissing();
                SaveLO(CC.GetStarfieldPath() + @"\Plugins.txt");
            }

        }
        private void toolStripMenuInstall_Click(object sender, EventArgs e)
        {
            InstallMod();
        }

        private void ToggleProfiles()
        {
        }

        private void chkProfile_CheckedChanged(object sender, EventArgs e)
        {
            if (chkProfile.Checked)
            {
                Profiles = true;
                cmbProfile.Enabled = true;
                GetProfiles();
            }
            else
            {
                Profiles = false;
                cmbProfile.Enabled = false;
            }
        }

        private void toolStripMenuExportActive_Click(object sender, EventArgs e)
        {
            int i;
            string tempstr;
            SaveFileDialog ExportActive = new SaveFileDialog();

            ExportActive.Filter = "Txt File|*.txt";
            ExportActive.Title = "Export Active Plugins";

            DialogResult dlgResult = ExportActive.ShowDialog();
            if (dlgResult == DialogResult.OK)
            {
                using (StreamWriter writer = new StreamWriter(ExportActive.FileName))
                {
                    for (i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        if ((bool)dataGridView1.Rows[i].Cells[0].Value)
                        {
                            tempstr = (string)dataGridView1.Rows[i].Cells[1].Value;
                            writer.WriteLine(tempstr);
                        }
                    }
                }
            }
        }

        private void toolStripMenuDeleteLine_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.RemoveAt(dataGridView1.CurrentRow.Index);
        }

        private void toolStripMenuExploreData_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", StarfieldGamePath + "\\Data");
        }

        private void toolStripMenuExploreAppData_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", CC.GetStarfieldPath());
        }

        private void dataGridView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                contextMenuDatagrid.Show(this, new Point(e.X, e.Y));
        }

        private void toolStripMenuUp_Click(object sender, EventArgs e)
        {
            MoveUp();
        }

        private void toolStripMenuDown_Click(object sender, EventArgs e)
        {
            MoveDown();
        }

        private void toolStripMenuTop_Click(object sender, EventArgs e)
        {
            MoveTop();
        }

        private void toolStripMenuBottom_Click(object sender, EventArgs e)
        {
            MoveBottom();
        }

        private void toolStripMenuDelContext_Click(object sender, EventArgs e)
        {
            DeleteLine();
        }

        private void UninstallMod()
        {
            string ModName, ModFile;

            ModName = (string)dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[1].Value;
            ModName = ModName.Substring(0, ModName.IndexOf("."));
            DialogResult DialogResult = MessageBox.Show(@"This will delete all files related to the '" + ModName + @"' mod", "Delete mod. Are you sure?", MessageBoxButtons.OKCancel, MessageBoxIcon.Stop);


            if (DialogResult == DialogResult.OK)
            {
                isModified = true;
                dataGridView1.Rows.Remove(dataGridView1.CurrentRow);
                string directoryPath = StarfieldGamePath + "\\Data";

                ModFile = directoryPath + "\\" + ModName;
                if (File.Exists(ModFile + ".esm"))
                    File.Delete(ModFile + ".esm");
                if (File.Exists(ModFile + " - textures.ba2"))
                    File.Delete(ModFile + " - textures.ba2");
                if (File.Exists(ModFile + " - main.ba2"))
                    File.Delete(ModFile + " - main.ba2");

                SaveLO(CC.GetStarfieldPath() + @"\Plugins.txt");
            }
            else
                toolStripStatusLabel1.Text = "Un-install cancelled";
        }

        private void toolStripMenuUninstallContext_Click(object sender, EventArgs e)
        {
            UninstallMod();
        }

        private void EnableDisable()
        {
            isModified = true;
            DataGridViewRow currentRow = dataGridView1.CurrentRow;
            currentRow.Cells[0].Value = !(bool)(currentRow.Cells[0].Value);

        }

        private void toolStripMenuEnableDisable_Click(object sender, EventArgs e)
        {
            EnableDisable();
        }

        private void toolStripMenuRefresh_Click(object sender, EventArgs e)
        {
            RefreshDataGrid();
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            EnableDisable();
        }

        private void toolStripMenuRunSteam_Click(object sender, EventArgs e)
        {
            if (isModified)
            {
                DialogResult = MessageBox.Show("Load order is modified. Cancel and save changes first or press OK to load game without saving", "Launch Game", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (DialogResult == DialogResult.OK)
                    CC.StartStarfieldSteam();
            }
            else
                CC.StartStarfieldSteam();
        }

        private void toolStripMenuRunMS_Click(object sender, EventArgs e)
        {
            if (isModified)
            {
                DialogResult = MessageBox.Show("Load order is modified. Cancel and save changes first or press OK to load game without saving", "Launch Game", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (DialogResult == DialogResult.OK)
                    CC.StartStarfieldMS();
            }
            else
                CC.StartStarfieldMS();

        }

        private void toolStripMenuInstallMod_Click(object sender, EventArgs e)
        {
            InstallMod();
        }

        private void toolStripMenuUninstall_Click(object sender, EventArgs e)
        {
            UninstallMod();
        }
    }
}