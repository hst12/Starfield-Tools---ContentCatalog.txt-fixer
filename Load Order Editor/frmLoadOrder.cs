using Newtonsoft.Json;
using Starfield_Tools.Common;
using Starfield_Tools.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using File = System.IO.File;

namespace Starfield_Tools
{


    public partial class frmLoadOrder : Form
    {
        readonly Tools tools = new();
        public string StarfieldGamePath = "", LoadScreenPic = "";

        bool isModified = false, Profiles = false, GameVersion = false, GridSorted = false;

        public frmLoadOrder()
        {
            InitializeComponent();

            if (!Tools.CheckGame())
                Application.Exit();

            if (!Directory.Exists(Tools.GetStarfieldPath())) // Check if Starfield is installed
            {
                MessageBox.Show("Unable to continue. Is Starfield installed correctly?", "Starfield not found in AppData directory", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                Application.Exit();
            }

            Rectangle resolution = Screen.PrimaryScreen.Bounds; // Resize window to 75% of screen width
            double screenWidth = resolution.Width;
            double screenHeight = resolution.Height;
            this.Width = (int)(screenWidth * 0.75);
            this.Height = (int)(screenHeight * 0.75);
            this.StartPosition = FormStartPosition.CenterScreen;

            this.KeyUp += new System.Windows.Forms.KeyEventHandler(KeyEvent); // Handle <enter> for search
            toolStripMenuInstall.Enabled = true;
            toolStripMenuUninstall.Enabled = true;

            string PluginsPath = Tools.GetStarfieldPath() + "\\Plugins.txt";

            menuStrip1.Font = Settings.Default.FontSize; // Get settings
            this.Font = Settings.Default.FontSize;
            StarfieldGamePath = Settings.Default.StarfieldGamePath;
            GameVersion = Settings.Default.GameVersion;
            if (!GameVersion) // 0=Steam, 1=MS game version
            {
                toolStripMenuSteam.Checked = true;
                toolStripMenuRunMS.Visible = false;
            }
            else
            {
                toolStripMenuMS.Checked = true;
                toolStripMenuRunSteam.Visible = false;
            }

            if (Settings.Default.Achievements)
            {
                toolStripMenuAchievements.Checked = true;
                dataGridView1.Columns["Achievements"].Visible = true;
            }
            else
            {
                toolStripMenuAchievements.Checked = false;
                dataGridView1.Columns["Achievements"].Visible = false;
            }

            if (Settings.Default.CreationsID)
            {
                toolStripMenuCreationsID.Checked = true;
                dataGridView1.Columns["CreationsID"].Visible = true;
            }
            else
            {
                dataGridView1.Columns["CreationsID"].Visible = false;
                toolStripMenuCreationsID.Checked = false;
            }

            if (Settings.Default.Files)
            {
                toolStripMenuFiles.Checked = true;
                dataGridView1.Columns["Files"].Visible = true;
            }
            else
            {
                dataGridView1.Columns["Files"].Visible = false;
                toolStripMenuFiles.Checked = false;
            }

            InitDataGrid();
            cmbProfile.Enabled = Profiles;
            GetProfiles();
            if (!File.Exists(PluginsPath + ".bak")) // Do a 1-time backup of Plugins.txt if it doesn't exist
            {
                sbar("Plugins.txt backed up to Plugins.txt.bak");
                File.Copy(PluginsPath, PluginsPath + ".bak");
            }
#if DEBUG
            testToolStripMenu.Visible = true;
#endif
        }

        private void RefreshDataGrid()
        {
            InitDataGrid();
            GetProfiles();
            GridSorted = false;
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

            toolStripStatusLabel1.ForeColor = DefaultForeColor;
            btnOK.Enabled = true;
            btnSave.Enabled = true;
            saveToolStripMenuItem.Enabled = true;

            if (!File.Exists(Tools.GetCatalog()))
            {
                MessageBox.Show("Missing ContentCatalog.txt");
                return;
            }

            dataGridView1.Rows.Clear();

            string jsonFilePath = Tools.GetCatalog();
            string json = System.IO.File.ReadAllText(jsonFilePath); // Read catalog


            List<string> CreationsPlugin = [];
            List<string> CreationsTitle = [];
            List<string> CreationsFiles = [];
            List<string> CreationsVersion = [];
            List<bool> AchievmentSafe = [];
            List<long> TimeStamp = [];
            List<string> CreationsID = [];

            int TitleCount = 0;
            int esmCount = 0;
            int espCount = 0;
            int ba2Count = 0;
            string StatText = "";
            double VersionCheck;
            List<string> esmFiles = [];

            try
            {
                //Dictionary<string, object> json_Dictionary = (new JavaScriptSerializer()).Deserialize<Dictionary<string, object>>(json);
                dynamic json_Dictionary = JsonConvert.DeserializeObject<dynamic>(json);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            try
            {
                var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Tools.Creation>>(json);
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
                        CreationsID.Add(kvp.Key.ToString());

                    }
                    catch (Exception ex)
                    {
                        sbar(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                sbar(ex.Message);
            }

            loText = Tools.GetStarfieldPath() + @"\plugins.txt";
            if (!File.Exists(loText))
            {
                MessageBox.Show(@"Missing Plugins.txt file

Click Ok and Ok again to create a blank Plugins.txt file or click Ok and Cancel to fix manually
Click File->Restore if you have a backup of your Plugins.txt file
Altenatively, run the game once to have it create a Plugins.txt file for you.", "Plugins.txt not found");
                return;
            }
            using (var reader = new StreamReader(loText))
            {
                string line, Description, ModFiles, ModVersion, ASafe, ModTimeStamp, ModID;

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
                                line = line[1..];
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
                                ModID = "";

                                for (int i = 0; i < CreationsPlugin.Count; i++)
                                {
                                    if (CreationsPlugin[i][..CreationsPlugin[i].IndexOf('.')] + ".esm" == line)
                                    {
                                        Description = CreationsTitle[i]; // Add Content Catalog description if available
                                        ModVersion = CreationsVersion[i];
                                        VersionCheck = double.Parse((ModVersion[..ModVersion.IndexOf('.')]));
                                        ModVersion = Tools.ConvertTime(VersionCheck).ToString() + ", v" + ModVersion[(ModVersion.IndexOf('.') + 1)..] + "\n";

                                        ModFiles = CreationsFiles[i];
                                        if (AchievmentSafe[i])
                                            ASafe = "Yes";
                                        else
                                            ASafe = "";
                                        ModTimeStamp = Tools.ConvertTime(TimeStamp[i]).ToString();
                                        ModID = CreationsID[i];
                                    }
                                }
                                dataGridView1.Rows.Add(ModEnabled, line, Description, ModVersion, ModTimeStamp, ASafe, ModFiles, ModID);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        sbar(ex.Message);
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
                foreach (var file in Directory.EnumerateFiles(directory, "*.ba2", SearchOption.TopDirectoryOnly))
                {
                    ba2Count++;
                }
                StatText = "Total Mods: " + dataGridView1.RowCount + ", Creations Mods: " + TitleCount.ToString() + ", Other: " +
                    (dataGridView1.RowCount - TitleCount).ToString() + ", Enabled: " + EnabledCount.ToString() + ", esm files: " +
                    esmCount.ToString() + " " + "Archives: " + ba2Count.ToString();

                if (espCount > 0)
                {
                    StatText += ", esp files: " + espCount.ToString();
                }

                sbar(StatText);
            }
            catch
            {
                sbar("Starfield path needs to be set for mod stats");
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
                    cmbProfile.Items.Add(profileName[(profileName.LastIndexOf('\\') + 1)..]);

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

            if (GridSorted)
                return;
            dataGridView1.CurrentCell = dataGridView1.Rows[0].Cells["ModEnabled"];

            using (StreamWriter writer = new(PluginFileName))
            {
                writer.Write("# This file is used by Starfield to keep track of your downloaded content.\r\n# Please do not modify this file.\r\n");
                for (int y = 0; y < dataGridView1.Rows.Count; y++)
                {
                    ModEnabled = (bool)dataGridView1.Rows[y].Cells["ModEnabled"].Value;
                    ModLine = (string)dataGridView1.Rows[y].Cells["PluginName"].Value;
                    if (ModEnabled)
                        writer.Write("*"); // Insert a * for enabled mods then write the mod filename
                    writer.WriteLine(ModLine);
                }
            }
            sbar("Plugins.txt saved");
            isModified = false;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            SaveLO(Tools.GetStarfieldPath() + @"\Plugins.txt");
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

            CurrentModEnabled = (bool)dataGridView1.Rows[y - 1].Cells["ModEnabled"].Value;
            CurrentModLine = (string)dataGridView1.Rows[y - 1].Cells["PluginName"].Value;
            CurrentDescription = (string)dataGridView1.Rows[y - 1].Cells["Description"].Value;

            NewModEnabled = (bool)dataGridView1.Rows[y].Cells["ModEnabled"].Value;
            NewModLine = (string)dataGridView1.Rows[y].Cells["PluginName"].Value;
            NewDescription = (string)dataGridView1.Rows[y].Cells["Description"].Value;


            dataGridView1.Rows[y].Cells["ModEnabled"].Value = CurrentModEnabled;
            dataGridView1.Rows[y].Cells["PluginName"].Value = CurrentModLine;
            dataGridView1.Rows[y].Cells["Description"].Value = CurrentDescription;

            dataGridView1.Rows[y - 1].Cells["ModEnabled"].Value = NewModEnabled;
            dataGridView1.Rows[y - 1].Cells["PluginName"].Value = NewModLine;
            dataGridView1.Rows[y - 1].Cells["Description"].Value = NewDescription;

            dataGridView1.Rows[y].Selected = false;
            dataGridView1.Rows[y - 1].Selected = true;

            dataGridView1.CurrentCell = dataGridView1.Rows[y - 1].Cells["ModEnabled"];


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

            CurrentModEnabled = (bool)dataGridView1.Rows[y + 1].Cells["ModEnabled"].Value;
            CurrentModLine = (string)dataGridView1.Rows[y + 1].Cells["PluginName"].Value;
            CurrentDescription = (string)dataGridView1.Rows[y + 1].Cells["Description"].Value;

            NewModEnabled = (bool)dataGridView1.Rows[y].Cells["ModEnabled"].Value;
            NewModLine = (string)dataGridView1.Rows[y].Cells["PluginName"].Value;
            NewDescription = (string)dataGridView1.Rows[y].Cells["Description"].Value;

            dataGridView1.Rows[y].Cells["ModEnabled"].Value = CurrentModEnabled;
            dataGridView1.Rows[y].Cells["PluginName"].Value = CurrentModLine;
            dataGridView1.Rows[y].Cells["Description"].Value = CurrentDescription;

            dataGridView1.Rows[y + 1].Cells["ModEnabled"].Value = NewModEnabled;
            dataGridView1.Rows[y + 1].Cells["PluginName"].Value = NewModLine;
            dataGridView1.Rows[y + 1].Cells["Description"].Value = NewDescription;

            dataGridView1.Rows[y].Selected = false;
            dataGridView1.Rows[y + 1].Selected = true;

            dataGridView1.CurrentCell = dataGridView1.Rows[y + 1].Cells["ModEnabled"];

        }
        private void btnDown_Click(object sender, EventArgs e)
        {
            MoveDown();
        }

        private void BackupPlugins()
        {
            string sourceFileName = Tools.GetStarfieldPath() + @"\Plugins.txt";
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

                sbar("Backup done");
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
            string sourceFileName = Tools.GetStarfieldPath() + @"\Plugins.txt.bak";
            string destFileName = Tools.GetStarfieldPath() + @"\Plugins.txt";

            try
            {
                // Copy the file
                File.Copy(sourceFileName, destFileName, true); // overwrite
                InitDataGrid();

                toolStripStatusLabel1.ForeColor = DefaultForeColor;
                sbar("Restore done");
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

            CurrentModEnabled = (bool)dataGridView1.Rows[y].Cells["ModEnabled"].Value;
            CurrentModLine = (string)dataGridView1.Rows[y].Cells["PluginName"].Value;
            CurrentDescription = (string)dataGridView1.Rows[y].Cells["Description"].Value;
            dataGridView1.Rows.Insert(0);

            dataGridView1.Rows[0].Cells["ModEnabled"].Value = CurrentModEnabled;
            dataGridView1.Rows[0].Cells["PluginName"].Value = CurrentModLine;
            dataGridView1.Rows[0].Cells["Description"].Value = CurrentDescription;

            dataGridView1.Rows.Remove(dataGridView1.CurrentRow);
            dataGridView1.CurrentCell = dataGridView1.Rows[0].Cells["ModEnabled"];
        }

        private void MoveBottom()
        {
            int y = dataGridView1.CurrentCell.RowIndex;
            if (y > dataGridView1.Rows.Count - 1) return;
            isModified |= true;
            bool CurrentModEnabled;
            string CurrentModLine;
            string CurrentDescription;

            CurrentModEnabled = (bool)dataGridView1.Rows[y].Cells["ModEnabled"].Value;
            CurrentModLine = (string)dataGridView1.Rows[y].Cells["PluginName"].Value;
            CurrentDescription = (string)dataGridView1.Rows[y].Cells["Description"].Value;

            dataGridView1.Rows.Insert(dataGridView1.Rows.Count);

            dataGridView1.Rows[^1].Cells["ModEnabled"].Value = CurrentModEnabled;
            dataGridView1.Rows[^1].Cells["PluginName"].Value = CurrentModLine;
            dataGridView1.Rows[^1].Cells["Description"].Value = CurrentDescription;

            dataGridView1.Rows.Remove(dataGridView1.CurrentRow);
            dataGridView1.CurrentCell = dataGridView1.Rows[^1].Cells["ModEnabled"];
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
            sbar("Warning! - Plugins sorted - saving changes disabled");
            toolStripStatusLabel1.ForeColor = Color.Red;
            btnOK.Enabled = false;
            btnSave.Enabled = false;
            saveToolStripMenuItem.Enabled = false;
            GridSorted = true;
        }

        private void DisableAll()
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                dataGridView1.Rows[i].Cells["ModEnabled"].Value = false;
            }
            sbar("All mods disabled");
            isModified = true;
        }

        private void EnableAll()
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                dataGridView1.Rows[i].Cells["ModEnabled"].Value = true;
            }
            sbar("All mods enabled");
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
                DataGridSring = dataGridView1.Rows[ModIndex].Cells["PluginName"].Value.ToString().ToLower();
                if (DataGridSring.Contains(TextBoxString))
                {
                    sbar("Found " + txtSearchBox.Text + " in " + dataGridView1.Rows[ModIndex].Cells["PluginName"].Value.ToString());
                    dataGridView1.CurrentCell = dataGridView1.Rows[ModIndex].Cells["PluginName"];
                    break;
                }
                else
                    sbar(txtSearchBox.Text + " not found ");
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Tools.ShowAbout();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveSettings();
            Application.Exit();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SavePlugings();
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

            Tools.OpenUrl("https://creations.bethesda.net/en/starfield/all?sort=latest_uploaded");
        }

        private void toolStripMenuNexus_Click(object sender, EventArgs e)
        {
            Tools.OpenUrl("https://www.nexusmods.com/starfield");
        }

        private void txtSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                SearchMod();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string ProfileFolder;

            dataGridView1.CurrentCell = dataGridView1.Rows[0].Cells["ModEnabled"];

            SaveFileDialog SavePlugins = new();
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
                Settings.Default.ProfileFolder = SavePlugins.FileName[..SavePlugins.FileName.LastIndexOf('\\')];
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
                File.Copy(ProfileName, Tools.GetStarfieldPath() + "\\Plugins.txt", true);
                Settings.Default.LastProfile = ProfileName[(ProfileName.LastIndexOf('\\') + 1)..];
                SaveSettings();
                isModified = false;
            }
            catch
            {
                sbar("Error switching profile");
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string ProfileFolder;

            ProfileFolder = Settings.Default.ProfileFolder;
            if (ProfileFolder == null || ProfileFolder == "")
                ProfileFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            OpenFileDialog OpenPlugins = new()
            {
                InitialDirectory = ProfileFolder,
                Filter = "Txt File|*.txt",
                Title = "Load Profile"
            };

            DialogResult result = OpenPlugins.ShowDialog();
            if (DialogResult.OK == result)
            {

                InitDataGrid();

            }
            if (OpenPlugins.FileName != "")
            {
                Settings.Default.ProfileFolder = OpenPlugins.FileName[..OpenPlugins.FileName.LastIndexOf('\\')];
                SwitchProfile(OpenPlugins.FileName);
                GetProfiles();
                Settings.Default.Save();
            }
        }

        private void toolStripMenuAdd_Click(object sender, EventArgs e)
        {
            string GameData;

            GameData = Settings.Default.StarfieldGamePath + "\\Data";
            OpenFileDialog GetMod = new()
            {
                Filter = "esm File|*.esm",
                Title = "Select mod",
                InitialDirectory = GameData
            };

            DialogResult result = GetMod.ShowDialog();
            if (DialogResult.OK == result)
            {
                sbar(GetMod.FileName);
                GameData = GetMod.FileName[GetMod.FileName.LastIndexOf('\\')..];
                dataGridView1.Rows.Add(true, GameData[1..]);
                dataGridView1.CurrentCell = dataGridView1.Rows[dataGridView1.RowCount - 1].Cells["PluginName"];
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
            StarfieldGamePath = tools.SetStarfieldGamePath();
            InitDataGrid();
        }

        private void toolStripMenuCleanup_Click(object sender, EventArgs e)
        {
            RemoveMissing();
        }

        private int AddMissing() // Look for .esm files to add to Plugins.txt returns no. of file added
        {
            int AddedFiles = 0;
            List<string> esmFiles = [];
            List<string> PluginFiles = [];
            List<string> BethFiles =
            // Exclude game files - will probably need updating after DLC release
            [
                "BlueprintShips-Starfield.esm","Constellation.esm","OldMars.esm","SFBGS003.esm","SFBGS006.esm","SFBGS007.esm","SFBGS008.esm","Starfield.esm"
            ];

            string directory = Settings.Default.StarfieldGamePath;
            if (directory == "" || directory == null)
                directory = tools.SetStarfieldGamePath();
            directory += @"\Data";
            if (directory == "\\Data")
            {
                MessageBox.Show("Can't continue without game installation path");
                return 0;
            }
            foreach (var missingFile in Directory.EnumerateFiles(directory, "*.esm", SearchOption.TopDirectoryOnly))
            {
                esmFiles.Add(missingFile[(missingFile.LastIndexOf('\\') + 1)..]);
            }
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                PluginFiles.Add((string)dataGridView1.Rows[i].Cells["PluginName"].Value);
            List<string> MissingFiles = esmFiles.Except(PluginFiles).ToList();

            List<string> FilesToAdd = MissingFiles.Except(BethFiles).ToList();  // Exclude BGS esm files
            if (FilesToAdd.Count > 0)
            {
                for (int i = 0; i < FilesToAdd.Count; i++)
                {
                    AddedFiles++;
                    dataGridView1.Rows.Add(true, FilesToAdd[i]);
                }
                dataGridView1.CurrentCell = dataGridView1.Rows[dataGridView1.RowCount - 1].Cells["PluginName"];
                sbar(AddedFiles.ToString() + " file(s) added");
                isModified = true;
            }
            else
                sbar("Nothing to add");
            return AddedFiles;
        }


        private int RemoveMissing() // Remove entries from Plugins.txt for missing .esm files. Returns number of removals
        {
            int RemovedFiles = 0;
            List<string> esmFiles = [];
            List<string> PluginFiles = [];

            List<string> BethFiles =
            // Exclude BGS files
            [
                "BlueprintShips-Starfield.esm","Constellation.esm","OldMars.esm","SFBGS003.esm","SFBGS006.esm","SFBGS007.esm","SFBGS008.esm","Starfield.esm"
            ];

            string directory = Settings.Default.StarfieldGamePath;
            if (directory == "" || directory == null)
                directory = tools.SetStarfieldGamePath();
            directory += @"\Data";
            if (directory == "\\Data")
            {
                MessageBox.Show("Can't continue without game installation path");
                return 0;
            }
            foreach (var missingFile in Directory.EnumerateFiles(directory, "*.esm", SearchOption.TopDirectoryOnly))
            {
                esmFiles.Add(missingFile[(missingFile.LastIndexOf('\\') + 1)..]);
            }
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                PluginFiles.Add((string)dataGridView1.Rows[i].Cells["PluginName"].Value);
            List<string> MissingFiles = PluginFiles.Except(esmFiles).ToList();

            List<string> FilesToRemove = MissingFiles.Except(BethFiles).ToList();
            if (FilesToRemove.Count > 0)
            {
                for (int i = 0; i < FilesToRemove.Count; i++)
                {
                    RemovedFiles++;

                    for (int j = 0; j < dataGridView1.Rows.Count; j++)
                        if ((string)dataGridView1.Rows[j].Cells["PluginName"].Value == FilesToRemove[i])
                            dataGridView1.Rows.RemoveAt(j);
                }
                sbar(RemovedFiles.ToString() + " file(s) removed");
                isModified = true;
            }
            else
                sbar("Nothing to remove");
            return RemovedFiles;
        }

        private void AddRemove()
        {
            int addedMods = 0, removedMods = 0;
            addedMods = AddMissing();
            removedMods = RemoveMissing();
            sbar(addedMods.ToString() + " Mods added, " + removedMods.ToString() + " Mods removed");
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

        private static void SaveSettings()
        {
            Settings.Default.Save();
        }

        private void cmbProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            SwitchProfile(Settings.Default.ProfileFolder + "\\" + (string)cmbProfile.SelectedItem);
            InitDataGrid();
        }

        private void InstallMod()
        {
            string ModPath;
            string ExtractPath = Path.GetTempPath() + "hstTools";

            OpenFileDialog OpenMod = new()
            {
                //OpenMod.InitialDirectory = ProfileFolder;
                Filter = "ZIP File|*.zip",
                Title = "Install Mod - Zip files only. Loose files not supported"
            };

            DialogResult result = OpenMod.ShowDialog();
            ModPath = OpenMod.FileName;

            if (OpenMod.FileName != "")
            {
                try
                {
                    sbar("Installing mod...");
                    ZipFile.ExtractToDirectory(ModPath, ExtractPath);

                }
                catch (Exception ex)
                {
                    sbar(ex.Message);

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
                SaveLO(Tools.GetStarfieldPath() + @"\Plugins.txt");
            }

        }
        private void toolStripMenuInstall_Click(object sender, EventArgs e)
        {
            InstallMod();
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
            SaveFileDialog ExportActive = new()
            {
                Filter = "Txt File|*.txt",
                Title = "Export Active Plugins"
            };

            DialogResult dlgResult = ExportActive.ShowDialog();
            if (dlgResult == DialogResult.OK)
            {
                using StreamWriter writer = new(ExportActive.FileName);
                for (i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if ((bool)dataGridView1.Rows[i].Cells["ModEnabled"].Value)
                    {
                        tempstr = (string)dataGridView1.Rows[i].Cells["PluginName"].Value;
                        writer.WriteLine(tempstr);
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
            Process.Start("explorer.exe", Tools.GetStarfieldPath());
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

            ModName = (string)dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells["PluginName"].Value;
            ModName = ModName[..ModName.IndexOf('.')];
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

                SaveLO(Tools.GetStarfieldPath() + @"\Plugins.txt");
            }
            else
                sbar("Un-install cancelled");
        }

        private void toolStripMenuUninstallContext_Click(object sender, EventArgs e)
        {
            UninstallMod();
        }

        private void EnableDisable()
        {
            if (GridSorted)
                return;
            isModified = true;
            DataGridViewRow currentRow = dataGridView1.CurrentRow;
            currentRow.Cells["ModEnabled"].Value = !(bool)(currentRow.Cells["ModEnabled"].Value);
            SaveOnDblClick();
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
                    Tools.StartStarfieldSteam();
            }
            else
                Tools.StartStarfieldSteam();
        }

        private void toolStripMenuRunMS_Click(object sender, EventArgs e)
        {
            if (isModified)
            {
                DialogResult = MessageBox.Show("Load order is modified. Cancel and save changes first or press OK to load game without saving", "Launch Game", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (DialogResult == DialogResult.OK)
                    Tools.StartStarfieldMS();
            }
            else
                Tools.StartStarfieldMS();

        }

        private void toolStripMenuInstallMod_Click(object sender, EventArgs e)
        {
            InstallMod();
        }

        private void toolStripMenuSteam_Click(object sender, EventArgs e)
        {
            toolStripMenuSteam.Checked = !toolStripMenuSteam.Checked;
            if (toolStripMenuSteam.Checked)
            {
                toolStripMenuRunMS.Visible = false;
                toolStripMenuRunSteam.Visible = true;
                toolStripMenuMS.Checked = false;
                GameVersion = false;
                Settings.Default.GameVersion = GameVersion;
            }
            else
                toolStripMenuMS.Checked = true;
            Settings.Default.GameVersion = GameVersion;
            SaveSettings();
        }

        private void SaveOnDblClick()
        {
            SavePlugings();
        }

        private void SavePlugings()
        {
            var dgCurrent = dataGridView1.CurrentCell;
            SaveLO(Tools.GetStarfieldPath() + @"\Plugins.txt");
            if (Profiles)
                SaveLO(Settings.Default.ProfileFolder + "\\" + cmbProfile.Text); // Save profile as well
            dataGridView1.CurrentCell = dgCurrent;
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            SavePlugings();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            if (GameVersion)
                Tools.StartStarfieldMS();
            else
                Tools.StartStarfieldSteam();
        }

        private void toolStripMenAddRemoveContext_Click(object sender, EventArgs e)
        {
            AddRemove();
        }

        private void toolStripMenuBGSStarfield_Click(object sender, EventArgs e)
        {
            Tools.OpenUrl("https://discord.com/channels/784542837596225567/1083043812949110825");
        }

        private void toolStripMenuBGSX_Click(object sender, EventArgs e)
        {
            Tools.OpenUrl("https://x.com/StarfieldGame");
        }

        private void toolStripMenuTestJson_Click(object sender, EventArgs e)
        {
            var modMetaData = new Tools.ModMetaData();
            string jsonPath = "Z:\\test.txt", json;

            modMetaData.ModName = "Test";
            modMetaData.SourceURL = "http://127.0.0.1";
            modMetaData.ModVersion = "1.0";
            json = Newtonsoft.Json.JsonConvert.SerializeObject(modMetaData, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(jsonPath, json);
        }

        private void toolStripMenuViewOnCreations_Click(object sender, EventArgs e)
        {
            string CreationsID = dataGridView1.CurrentRow.Cells["CreationsID"].Value.ToString();
            string url = "https://creations.bethesda.net/en/starfield/details/" + CreationsID[3..];

            if (CreationsID != "")
                Tools.OpenUrl(url);  // Open Creations web site
            else
                sbar("Not a Creations mod");
        }

        private void toolStripMenuMS_Click(object sender, EventArgs e)
        {
            toolStripMenuMS.Checked = !toolStripMenuMS.Checked;
            if (toolStripMenuMS.Checked)
            {
                toolStripMenuSteam.Checked = false;
                toolStripMenuRunMS.Visible = true;
                toolStripMenuRunSteam.Visible = false;

                GameVersion = true;
            }
            else
                toolStripMenuSteam.Checked = true;
            Settings.Default.GameVersion = GameVersion;
            SaveSettings();
        }

        private void toolStripMenuUninstall_Click(object sender, EventArgs e)
        {
            UninstallMod();
        }


        private void btnQuit_Click(object sender, EventArgs e)
        {
            SaveSettings();
            Application.Exit();
        }

        private void toolStripMenuGitHub_Click(object sender, EventArgs e)
        {
            Tools.OpenUrl("https://github.com/hst12/Starfield-Tools---ContentCatalog.txt-fixer");
        }

        private void toolStripMenuAchievements_Click(object sender, EventArgs e)
        {
            toolStripMenuAchievements.Checked = !toolStripMenuAchievements.Checked;
            if (toolStripMenuAchievements.Checked)
                dataGridView1.Columns["Achievements"].Visible = true;
            else
                dataGridView1.Columns["Achievements"].Visible = false;
            Settings.Default.Achievements = toolStripMenuAchievements.Checked;
        }

        private void toolStripMenuCreationsID_Click(object sender, EventArgs e)
        {
            toolStripMenuCreationsID.Checked = !toolStripMenuCreationsID.Checked;
            if (toolStripMenuCreationsID.Checked)
                dataGridView1.Columns["CreationsID"].Visible = true;
            else
                dataGridView1.Columns["CreationsID"].Visible = false;
            Settings.Default.CreationsID = toolStripMenuCreationsID.Checked;
        }

        private void toolStripMenuFiles_Click(object sender, EventArgs e)
        {
            toolStripMenuFiles.Checked = !toolStripMenuFiles.Checked;
            if (toolStripMenuFiles.Checked)
                dataGridView1.Columns["Files"].Visible = true;
            else
                dataGridView1.Columns["Files"].Visible = false;
            Settings.Default.Files = toolStripMenuFiles.Checked;
        }

        private void toolStripMenuLoadingScreen_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            openFileDialog1.Title = "Choose a loadscreen image";
            DialogResult LoadScreen = openFileDialog1.ShowDialog();
            if (LoadScreen == DialogResult.OK)
            {
                if (openFileDialog1.FileName != "")
                    Settings.Default.LoadScreenFilename = openFileDialog1.FileName;
            }
        }

        private void toolStripMenuLoot_Click(object sender, EventArgs e)
        {
            string LOOTPath = Settings.Default.LOOTPath;
            if (LOOTPath == "")
            {
                if (!SetLOOTPath())
                {
                    sbar("LOOT path is required to run LOOT");
                    return;
                }
            }

            if (LOOTPath != "")
            {
                Process.Start(LOOTPath, "--game Starfield --auto-sort");
                Thread.Sleep(2000);
                InitDataGrid();
            }
        }

        private bool SetLOOTPath()
        {
            openFileDialog1.Filter = "Executable Files|*.exe";
            openFileDialog1.Title = "Set the path to the LOOT executable";
            openFileDialog1.FileName = "LOOT.exe";
            DialogResult LOOTPath = openFileDialog1.ShowDialog();
            if (LOOTPath == DialogResult.OK)
            {
                if (openFileDialog1.FileName != "")
                {
                    Settings.Default.LOOTPath = openFileDialog1.FileName;
                    return true;
                }
                else return false;
            }
            else
                return false;
        }
        private void toolStripMenuLootPath_Click(object sender, EventArgs e)
        {
            SetLOOTPath();
        }

        private void sbar(string StatusBarMessage)
        {
            toolStripStatusLabel1.Text = StatusBarMessage;
        }
    }
}

