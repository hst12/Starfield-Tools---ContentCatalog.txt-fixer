﻿using Newtonsoft.Json;
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
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using File = System.IO.File;

namespace Starfield_Tools
{


    public partial class frmLoadOrder : Form
    {
        private Rectangle dragBoxFromMouseDown;
        private int rowIndexFromMouseDown;
        private int rowIndexOfItemUnderMouseToDrop;

        readonly Tools tools = new();
        public string StarfieldGamePath = "", LoadScreenPic = "", LastProfile;

        bool isModified = false, Profiles = false, GameVersion = false, GridSorted = false;

        public frmLoadOrder(string parameter)
        {
            InitializeComponent();

            if (!Tools.CheckGame())
                Application.Exit();

            if (!Directory.Exists(Tools.GetStarfieldAppData())) // Check if Starfield is installed
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

            string PluginsPath = Tools.GetStarfieldAppData() + "\\Plugins.txt";

            menuStrip1.Font = Properties.Settings.Default.FontSize; // Get settings
            this.Font = Properties.Settings.Default.FontSize;
            StarfieldGamePath = Properties.Settings.Default.StarfieldGamePath;
            GameVersion = Properties.Settings.Default.GameVersion;
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

            if (Properties.Settings.Default.AutoDelccc)
            {
                toolStripMenuAutoDelccc.Checked = true;
            }

            if (Properties.Settings.Default.ProflieOn)
            {
                toolStripMenuProfilesOn.Checked = Properties.Settings.Default.ProflieOn;
                if (toolStripMenuProfilesOn.Checked)
                {
                    Profiles = true;
                    chkProfile.Checked = true;
                }
                else Profiles = false;
            }

            // Setup columns

            if (Properties.Settings.Default.Achievements)
            {
                toolStripMenuAchievements.Checked = true;
                dataGridView1.Columns["Achievements"].Visible = true;
            }
            else
            {
                toolStripMenuAchievements.Checked = false;
                dataGridView1.Columns["Achievements"].Visible = false;
            }

            if (Properties.Settings.Default.CreationsID)
            {
                toolStripMenuCreationsID.Checked = true;
                dataGridView1.Columns["CreationsID"].Visible = true;
            }
            else
            {
                dataGridView1.Columns["CreationsID"].Visible = false;
                toolStripMenuCreationsID.Checked = false;
            }

            if (Properties.Settings.Default.Files)
            {
                toolStripMenuFiles.Checked = true;
                dataGridView1.Columns["Files"].Visible = true;
            }
            else
            {
                dataGridView1.Columns["Files"].Visible = false;
                toolStripMenuFiles.Checked = false;
            }

            if (Properties.Settings.Default.Group)
            {
                toolStripMenuGroup.Checked = true;
                dataGridView1.Columns["Group"].Visible = true;
            }
            else
            {
                toolStripMenuGroup.Checked = false;
                dataGridView1.Columns["Group"].Visible = false;
            }

            if (Properties.Settings.Default.Index)
            {
                toolStripMenuIndex.Checked = true;
                dataGridView1.Columns["Index"].Visible = true;
            }
            else
            {
                toolStripMenuIndex.Checked = false;
                dataGridView1.Columns["Index"].Visible = false;
            }

            InitDataGrid();
            cmbProfile.Enabled = Profiles;
            GetProfiles();

            // Do a 1-time backup of Plugins.txt if it doesn't exist
            if (!File.Exists(PluginsPath + ".bak"))
            {
                sbar("Plugins.txt backed up to Plugins.txt.bak");
                File.Copy(PluginsPath, PluginsPath + ".bak");
            }
            if (parameter != "")
                sbar(parameter);
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
            int EnabledCount = 0, IndexCount = 1;
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

            Tools.Configuration Groups = new();
            if (toolStripMenuGroup.Checked && Properties.Settings.Default.LOOTPath != "") // Read LOOT groups
            {
                try
                {
                    var deserializer = new DeserializerBuilder().Build();
                    var yamlContent = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\LOOT\games\Starfield\userlist.yaml");
                    Groups = deserializer.Deserialize<Tools.Configuration>(yamlContent);
                }
                catch (Exception ex)
                {
#if DEBUG
                    MessageBox.Show(ex.Message);
#endif
                    sbar(ex.Message);
                }
            }

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

            loText = Tools.GetStarfieldAppData() + @"\plugins.txt";
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
                string PluginName, Description, ModFiles, ModVersion, ASafe, ModTimeStamp, ModID;

                while ((PluginName = reader.ReadLine()) != null) // Read Plugins.txt
                {
                    try
                    {
                        if (PluginName != "")
                        {
                            if (PluginName[0] == '*') // * = Mod enabled
                            {
                                ModEnabled = true;
                                EnabledCount++;
                                PluginName = PluginName[1..];
                            }
                            else
                                ModEnabled = false;

                            if (PluginName[0] != '#') // Ignore comment
                            {
                                Description = "";
                                ModFiles = "";
                                ModVersion = "";
                                ASafe = "";
                                ModTimeStamp = "";
                                ModID = "";

                                for (int i = 0; i < CreationsPlugin.Count; i++)
                                {
                                    if (CreationsPlugin[i][..CreationsPlugin[i].IndexOf('.')] + ".esm" == PluginName)
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
                                /* Disable combobox code for the time being


                                    ComboBox GroupCombo = new();
                                    GroupCombo.Items.AddRange(Groups);
                                    ((DataGridViewComboBoxColumn)dataGridView1.Columns["Group"]).DataSource = GroupCombo.Items;
                                */

                                int rowIndex = this.dataGridView1.Rows.Add();
                                var row = this.dataGridView1.Rows[rowIndex];

                                // Populate datagrid
                                if (Properties.Settings.Default.LOOTPath != "" && Groups.groups != null)
                                {
                                    for (int i = 0; i < Groups.plugins.Count; i++)
                                        if (Groups.plugins[i].name == PluginName)
                                            row.Cells["Group"].Value = Groups.plugins[i].group;
                                }
                                else
                                {
#if DEBUG
                                    Debug.WriteLine(PluginName, "Null Group");
#endif
                                }
                                if (PluginName.StartsWith("sfbgs")) // Assume Bethesda plugin
                                    row.Cells["Group"].Value = "Bethesda";
                                row.Cells["ModEnabled"].Value = ModEnabled;
                                row.Cells["PluginName"].Value = PluginName;
                                row.Cells["Description"].Value = Description;
                                row.Cells["Version"].Value = ModVersion;
                                row.Cells["TimeStamp"].Value = ModTimeStamp;
                                row.Cells["Achievements"].Value = ASafe;
                                row.Cells["Files"].Value = ModFiles;
                                row.Cells["CreationsID"].Value = ModID;
                                row.Cells["Index"].Value = IndexCount++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                        sbar(ex.Message);
#if DEBUG
                        MessageBox.Show(ex.Message);
#endif
                    }
                }
            }

            // Get mod stats
            try
            {
                string directory = Properties.Settings.Default.StarfieldGamePath + @"\Data";
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
                    StatText += ", esp files: " + espCount.ToString();

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
            ProfileFolder = Properties.Settings.Default.ProfileFolder;
            if (ProfileFolder == null || ProfileFolder == "")
                ProfileFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            LastProfile ??= Properties.Settings.Default.LastProfile;
            try
            {
                foreach (var profileName in Directory.EnumerateFiles(ProfileFolder, "*.txt", SearchOption.TopDirectoryOnly))
                {
                    cmbProfile.Items.Add(profileName[(profileName.LastIndexOf('\\') + 1)..]);

                }
                int index = cmbProfile.Items.IndexOf(Properties.Settings.Default.LastProfile);
                if (index != -1)
                {
                    cmbProfile.SelectedIndex = index;
                    LastProfile = cmbProfile.Items[index].ToString(); ;
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
            SaveLO(Tools.GetStarfieldAppData() + @"\Plugins.txt");
            this.Close();
        }
        private void MoveUp()
        {
            int rowIndex = dataGridView1.CurrentCell.RowIndex;
            int colIndex = dataGridView1.SelectedCells[0].ColumnIndex;
            if (rowIndex < 1)
                return;
            if (rowIndex == 0)
                return; // Already at the top

            DataGridViewRow selectedRow = dataGridView1.Rows[rowIndex];
            dataGridView1.Rows.Remove(selectedRow);
            dataGridView1.Rows.Insert(rowIndex - 1, selectedRow);
            dataGridView1.ClearSelection();
            dataGridView1.Rows[rowIndex - 1].Selected = true;
            dataGridView1.Rows[rowIndex - 1].Cells[colIndex].Selected = true;
        }
        private void btnUp_Click(object sender, EventArgs e)
        {
            MoveUp();
        }

        private void MoveDown()
        {

            int rowIndex = dataGridView1.SelectedCells[0].OwningRow.Index;
            int colIndex = dataGridView1.SelectedCells[0].ColumnIndex;

            if (rowIndex == dataGridView1.Rows.Count - 1)
                return; // Already at the bottom

            DataGridViewRow selectedRow = dataGridView1.Rows[rowIndex];
            dataGridView1.Rows.Remove(selectedRow);
            dataGridView1.Rows.Insert(rowIndex + 1, selectedRow);
            dataGridView1.ClearSelection();
            dataGridView1.Rows[rowIndex + 1].Selected = true;
            dataGridView1.Rows[rowIndex + 1].Cells[colIndex].Selected = true;

        }
        private void btnDown_Click(object sender, EventArgs e)
        {
            MoveDown();
        }

        private void BackupPlugins()
        {
            string sourceFileName = Tools.GetStarfieldAppData() + @"\Plugins.txt";
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
            string sourceFileName = Tools.GetStarfieldAppData() + @"\Plugins.txt.bak";
            string destFileName = Tools.GetStarfieldAppData() + @"\Plugins.txt";

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
            int rowIndex = dataGridView1.SelectedCells[0].RowIndex;
            DataGridViewRow selectedRow = dataGridView1.Rows[rowIndex];
            int colIndex = dataGridView1.SelectedCells[0].ColumnIndex;

            dataGridView1.Rows.Remove(selectedRow);
            dataGridView1.Rows.Insert(0, selectedRow);
            dataGridView1.ClearSelection();
            //dataGridView1.Rows[0].Selected = true;
            dataGridView1.Rows[0].Cells[colIndex].Selected = true;

        }

        private void MoveBottom()
        {
            int rowIndex = dataGridView1.SelectedCells[0].RowIndex;
            int colIndex = dataGridView1.SelectedCells[0].ColumnIndex;
            DataGridViewRow selectedRow = dataGridView1.Rows[rowIndex];

            dataGridView1.Rows.Remove(selectedRow);
            dataGridView1.Rows.Insert(dataGridView1.Rows.Count, selectedRow);
            dataGridView1.ClearSelection();
            //dataGridView1.Rows[dataGridView1.Rows.Count - 1].Selected = true;
            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[colIndex].Selected = true;
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
                Properties.Settings.Default.ProfileFolder = SavePlugins.FileName[..SavePlugins.FileName.LastIndexOf('\\')];
                Properties.Settings.Default.Save();
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
                File.Copy(ProfileName, Tools.GetStarfieldAppData() + "\\Plugins.txt", true);
                Properties.Settings.Default.LastProfile = ProfileName[(ProfileName.LastIndexOf('\\') + 1)..];
                SaveSettings();
                isModified = false;
                //SavePlugings();
                InitDataGrid();
            }
            catch
            {
                sbar("Error switching profile");
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string ProfileFolder;

            ProfileFolder = Properties.Settings.Default.ProfileFolder;
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
                Properties.Settings.Default.ProfileFolder = OpenPlugins.FileName[..OpenPlugins.FileName.LastIndexOf('\\')];
                SwitchProfile(OpenPlugins.FileName);
                GetProfiles();
                Properties.Settings.Default.Save();
            }
        }

        private void toolStripMenuAdd_Click(object sender, EventArgs e)
        {
            string GameData;

            GameData = Properties.Settings.Default.StarfieldGamePath + "\\Data";
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
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    dataGridView1.Rows.RemoveAt(dataGridView1.CurrentRow.Index);
                    break;
                case Keys.S:
                    MoveDown();
                    break;
                case Keys.W:
                    MoveUp();
                    break;
                case Keys.A:
                    MoveTop();
                    break;
                case Keys.D:
                    MoveBottom();
                    break;
                case Keys.Space:
                    EnableDisable();
                    break;
            }
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
            List<string> BethFiles = tools.BethFiles;
            // Exclude game files - will probably need updating after DLC release

            string directory = Properties.Settings.Default.StarfieldGamePath;
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
                    //dataGridView1.Rows.Add(true, FilesToAdd[i]);
                    int rowIndex = this.dataGridView1.Rows.Add();
                    var row = this.dataGridView1.Rows[rowIndex];
                    row.Cells["ModEnabled"].Value = true;
                    row.Cells["PluginName"].Value = FilesToAdd[i];
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

            string directory = Properties.Settings.Default.StarfieldGamePath;
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

            List<string> FilesToRemove = MissingFiles.Except(tools.BethFiles).ToList();
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
            Properties.Settings.Default.Save();
        }

        private void cmbProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            SwitchProfile(Properties.Settings.Default.ProfileFolder + "\\" + (string)cmbProfile.SelectedItem);
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
                SavePlugings();
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
            Process.Start("explorer.exe", Tools.GetStarfieldAppData());
        }

        private void dataGridView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                contextMenuDatagrid.Show(this, new Point(e.X, e.Y));

            // Get the index of the item the mouse is below
            rowIndexFromMouseDown = dataGridView1.HitTest(e.X, e.Y).RowIndex;
            if (rowIndexFromMouseDown != -1)
            {
                // Remember the point where the mouse down occurred
                Size dragSize = SystemInformation.DragSize;
                dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width / 2), e.Y - (dragSize.Height / 2)), dragSize);
            }
            else
            {
                dragBoxFromMouseDown = Rectangle.Empty;
            }
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

                SavePlugings();
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
                Properties.Settings.Default.GameVersion = false;
                sbar("Game version set to Steam");
            }
            else
                toolStripMenuMS.Checked = true;
            Properties.Settings.Default.GameVersion = GameVersion;
            SaveSettings();
        }

        private void SaveOnDblClick()
        {
            SavePlugings();
        }

        private void SavePlugings()
        {
            var dgCurrent = dataGridView1.CurrentCell;
            SaveLO(Tools.GetStarfieldAppData() + @"\Plugins.txt");
            if (Profiles)
            {
                SaveLO(Properties.Settings.Default.ProfileFolder + "\\" + cmbProfile.Text); // Save profile as well
                toolStripStatusLabel1.Text += ", " + cmbProfile.Text + " profile saved";
            }
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
            if (CreationsID == null || CreationsID == "")
            {
                sbar("Not a Creations mod");
                return;
            }
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
                sbar("Game version set to MS Store");
                GameVersion = true;
            }
            else
                toolStripMenuSteam.Checked = true;
            Properties.Settings.Default.GameVersion = GameVersion;
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
            Properties.Settings.Default.Achievements = toolStripMenuAchievements.Checked;
        }

        private void toolStripMenuCreationsID_Click(object sender, EventArgs e)
        {
            toolStripMenuCreationsID.Checked = !toolStripMenuCreationsID.Checked;
            if (toolStripMenuCreationsID.Checked)
                dataGridView1.Columns["CreationsID"].Visible = true;
            else
                dataGridView1.Columns["CreationsID"].Visible = false;
            Properties.Settings.Default.CreationsID = toolStripMenuCreationsID.Checked;
        }

        private void toolStripMenuFiles_Click(object sender, EventArgs e)
        {
            toolStripMenuFiles.Checked = !toolStripMenuFiles.Checked;
            if (toolStripMenuFiles.Checked)
                dataGridView1.Columns["Files"].Visible = true;
            else
                dataGridView1.Columns["Files"].Visible = false;
            Properties.Settings.Default.Files = toolStripMenuFiles.Checked;
        }

        private void toolStripMenuLoadingScreen_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            openFileDialog1.Title = "Choose a loadscreen image";
            DialogResult LoadScreen = openFileDialog1.ShowDialog();
            if (LoadScreen == DialogResult.OK)
            {
                if (openFileDialog1.FileName != "")
                    Properties.Settings.Default.LoadScreenFilename = openFileDialog1.FileName;
            }
        }

        private void RunLOOT(bool LOOTMode) // True for autosort
        {
            string LOOTPath = Properties.Settings.Default.LOOTPath, cmdLine = "";
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
                if (LOOTMode)
                    cmdLine = " --auto-sort";
                Process.Start(LOOTPath, "--game Starfield" + cmdLine);
                Thread.Sleep(2000);
                InitDataGrid();
                if (Properties.Settings.Default.AutoDelccc)
                    Delccc();
            }

        }
        private void toolStripMenuLoot_Click(object sender, EventArgs e)
        {
            RunLOOT(true);
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
                    Properties.Settings.Default.LOOTPath = openFileDialog1.FileName;
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

        private void toolStripMenuLoot_Click_1(object sender, EventArgs e)
        {
            RunLOOT(false);
        }

        private void toolStripMenuEditPlugins_Click(object sender, EventArgs e)
        {
            string pathToFile = (Tools.GetStarfieldAppData() + @"\Plugins.txt");
            Process.Start("explorer", pathToFile);
        }

        private void toolStripMenuGroup_Click(object sender, EventArgs e)
        {
            toolStripMenuGroup.Checked = !toolStripMenuGroup.Checked;
            if (toolStripMenuGroup.Checked)
                dataGridView1.Columns["Group"].Visible = true;
            else
                dataGridView1.Columns["Group"].Visible = false;
            Properties.Settings.Default.Group = toolStripMenuGroup.Checked;
        }

        private void toolStripMenuLootAutoSort_Click(object sender, EventArgs e)
        {
            RunLOOT(true);
        }

        private void toolStripMenuExploreGameDocs_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\My Games\\Starfield");
        }

        private void Delccc()
        {
            try
            {
                string Starfieldccc = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\My Games\\Starfield\\Starfield.ccc";
                if (File.Exists(Starfieldccc))
                {
                    File.Delete(Starfieldccc);
                    sbar("Starfield.ccc deleted");
                }
                else
                    sbar("Starfield.ccc not found");

            }
            catch (Exception ex)
            {
                sbar("Error deleting Starfield.ccc " + ex.Message);
            }

        }
        private void toolStripMenuDeleteCCC_Click(object sender, EventArgs e)
        {
            Delccc();
        }

        private void toolStripMenuAutoDelccc_Click(object sender, EventArgs e)
        {
            toolStripMenuAutoDelccc.Checked = !toolStripMenuAutoDelccc.Checked;
            Properties.Settings.Default.AutoDelccc = toolStripMenuAutoDelccc.Checked; ;

        }

        private void toolStripMenuProfilesOn_Click(object sender, EventArgs e)
        {
            toolStripMenuProfilesOn.Checked = !toolStripMenuProfilesOn.Checked;
            Properties.Settings.Default.ProflieOn = toolStripMenuProfilesOn.Checked;
            if (toolStripMenuProfilesOn.Checked)
            {
                Profiles = true;
                chkProfile.Checked = true;
                GetProfiles();
            }
            else
            {
                Profiles = false;
                chkProfile.Checked = false;

            }
        }

        private void toolStripMenuLoadScreenPreview_Click(object sender, EventArgs e)
        {
            Tools.ShowSplashScreen();
        }

        private void toolStripMenuIndex_Click(object sender, EventArgs e)
        {
            toolStripMenuIndex.Checked = !toolStripMenuIndex.Checked;
            if (toolStripMenuIndex.Checked)
                dataGridView1.Columns["Index"].Visible = true;
            else
                dataGridView1.Columns["Index"].Visible = false;
            Properties.Settings.Default.Index = toolStripMenuIndex.Checked;
        }

        private void dataGridView1_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                // If the mouse moves outside the rectangle, start the drag
                if (dragBoxFromMouseDown != Rectangle.Empty && !dragBoxFromMouseDown.Contains(e.X, e.Y))
                {
                    DragDropEffects dropEffect = dataGridView1.DoDragDrop(dataGridView1.Rows[rowIndexFromMouseDown], DragDropEffects.Move);
                }
            }
        }

        private void dataGridView1_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void dataGridView1_DragDrop(object sender, DragEventArgs e)
        {
            // Convert screen coordinates to client coordinates
            Point clientPoint = dataGridView1.PointToClient(new Point(e.X, e.Y));
            rowIndexOfItemUnderMouseToDrop = dataGridView1.HitTest(clientPoint.X, clientPoint.Y).RowIndex;

            // If the drag operation was a move, remove and insert the row
            if (e.Effect == DragDropEffects.Move)
            {
                DataGridViewRow rowToMove = e.Data.GetData(typeof(DataGridViewRow)) as DataGridViewRow;
                dataGridView1.Rows.RemoveAt(rowIndexFromMouseDown);
                dataGridView1.Rows.Insert(rowIndexOfItemUnderMouseToDrop, rowToMove);
            }
        }
    }
}

