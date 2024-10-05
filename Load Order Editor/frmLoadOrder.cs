using Newtonsoft.Json;
using Starfield_Tools.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using YamlDotNet.Serialization;
using File = System.IO.File;

namespace Starfield_Tools
{
    public partial class frmLoadOrder : Form
    {
        private Rectangle dragBoxFromMouseDown;
        private int rowIndexFromMouseDown, rowIndexOfItemUnderMouseToDrop, GameVersion = 0;

        readonly Tools tools = new();
        private string StarfieldGamePath, LastProfile, CustomEXE;

        bool isModified = false, Profiles = false, GridSorted = false, LooseFiles = false, AutoUpdate = false;

        public frmLoadOrder(string parameter)
        {
            InitializeComponent();

            Tools.CheckGame();
            Rectangle resolution = Screen.PrimaryScreen.Bounds; // Resize window to 75% of screen width
            double screenWidth = resolution.Width;
            double screenHeight = resolution.Height;
            this.Width = (int)(screenWidth * 0.85);
            this.Height = (int)(screenHeight * 0.85);
            this.StartPosition = FormStartPosition.CenterScreen;

            this.KeyUp += new System.Windows.Forms.KeyEventHandler(KeyEvent); // Handle <enter> for search
            toolStripMenuInstall.Enabled = true;
            toolStripMenuUninstall.Enabled = true;

            string PluginsPath = Tools.StarfieldAppData + "\\Plugins.txt";

            menuStrip1.Font = Properties.Settings.Default.FontSize; // Get settings
            this.Font = Properties.Settings.Default.FontSize;
            StarfieldGamePath = Properties.Settings.Default.StarfieldGamePath;
            GameVersion = Properties.Settings.Default.GameVersion;
            CustomEXE = Properties.Settings.Default.CustomEXE;

            if (Properties.Settings.Default.AutoDelccc)
            {
                toolStripMenuAutoDelccc.Checked = true;
                sbarCCCOn();
                if (Delccc())
                    toolStripStatusDelCCC.Text = ("Starfield.ccc deleted");
            }
            else
                sbarCCCOff();

            switch (GameVersion)
            {
                case 0:
                    toolStripMenuSteam.Checked = true;
                    break;
                case 1:
                    toolStripMenuMS.Checked = true;
                    break;
                case 2:
                    toolStripMenuCustom.Checked = true;
                    break;
            }
            GameVersionDisplay();

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

            if (Properties.Settings.Default.TimeStamp)
            {
                timeStampToolStripMenuItem.Checked = true;
                dataGridView1.Columns["TimeStamp"].Visible = true;
            }
            else
            {
                timeStampToolStripMenuItem.Checked = false;
                dataGridView1.Columns["TimeStamp"].Visible = false;
            }

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

            if (Properties.Settings.Default.FileSize)
            {
                toolStripMenuFileSize.Checked = true;
                dataGridView1.Columns["FileSize"].Visible = true;
            }
            else
            {
                toolStripMenuFileSize.Checked = false;
                dataGridView1.Columns["FileSize"].Visible = false;
            }

            if (Properties.Settings.Default.LooseFiles)
            {
                looseFilesDisabledToolStripMenuItem.Text = "Loose Files Enabled";
                LooseFiles = true;
            }
            else
            {
                looseFilesDisabledToolStripMenuItem.Text = "Loose Files Disabled";
                LooseFiles = false;
            }

            if (Properties.Settings.Default.AutoUpdate)
            {
                autoUpdateModsToolStripMenuItem.Checked = true;
                AutoUpdate = true;
            }
            else
            {
                autoUpdateModsToolStripMenuItem.Checked = false;
                AutoUpdate = false;
            }

            frmStarfieldTools StarfieldTools = new(); // Check the catalog
            sbar4(StarfieldTools.CatalogStatus);
            if (StarfieldTools.CatalogStatus != null)
                if (StarfieldTools.CatalogStatus.Contains("Error"))
                    StarfieldTools.Show(); // Show catalog fixer if catalog broken

            InitDataGrid();
            cmbProfile.Enabled = Profiles;
            GetProfiles();

            // Do a 1-time backup of Plugins.txt if it doesn't exist
            if (!File.Exists(PluginsPath + ".bak"))
            {
                sbar2("Plugins.txt backed up to Plugins.txt.bak");
                File.Copy(PluginsPath, PluginsPath + ".bak");
            }

            if (AutoUpdate)
                AddRemove();

#if DEBUG
            testToolStripMenu.Visible = true;
#endif
        }

        private void RefreshDataGrid()
        {
            InitDataGrid();
            GetProfiles();
            GridSorted = false;
            isModified = false;
            GameVersionDisplay();
            sbar3("");
        }

        private void KeyEvent(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
                RefreshDataGrid();
        }

        private bool CheckStarfieldCustom()
        {
            bool result = Tools.FileCompare(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                "\\My Games\\Starfield\\StarfieldCustom.ini", Tools.CommonFolder + "\\StarfieldCustom.ini");
            return result;
        }
        private void InitDataGrid()
        {
            bool ModEnabled;
            int EnabledCount = 0, IndexCount = 1, i;
            string loText;

            toolStripStatusTertiary.ForeColor = DefaultForeColor;
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

            if (toolStripMenuGroup.Checked && Properties.Settings.Default.LOOTPath != "" && dataGridView1.Columns["Group"].Visible) // Read LOOT groups
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
            List<long> FileSize = [];

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
                        for (i = 0; i < kvp.Value.Files.Length; i++)
                        {
                            if (kvp.Value.Files[i].IndexOf(".esm") > 0 || kvp.Value.Files[i].IndexOf(".esp") > 0) // Look for .esm or .esp files
                            {
                                CreationsPlugin.Add(kvp.Value.Files[i]);
                                TitleCount++;
                            }
                        }
                        CreationsTitle.Add(kvp.Value.Title);
                        CreationsVersion.Add(kvp.Value.Version);
                        CreationsFiles.Add(string.Join(", ", kvp.Value.Files));
                        AchievmentSafe.Add(kvp.Value.AchievementSafe);
                        TimeStamp.Add(kvp.Value.Timestamp);
                        CreationsID.Add(kvp.Key.ToString());
                        FileSize.Add(kvp.Value.FilesSize);
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
            catch (Exception ex)
            {
                sbar(ex.Message);
                json = Tools.MakeHeaderBlank();
                File.WriteAllText(Tools.GetCatalog(), json);

#if DEBUG
                MessageBox.Show(ex.Message);
#endif
            }

            loText = Tools.StarfieldAppData + @"\plugins.txt";
            if (!File.Exists(loText))
            {
                MessageBox.Show(@"Missing Plugins.txt file

Click Ok and Quit to create a blank Plugins.txt file or click Ok and Cancel to fix manually
Click File->Restore if you have a backup of your Plugins.txt file
Altenatively, run the game once to have it create a Plugins.txt file for you.", "Plugins.txt not found");
                return;
            }
            using (var reader = new StreamReader(loText))
            {
                string PluginName, Description, ModFiles, ModVersion, AuthorVersion, ASafe, ModTimeStamp, ModID, URL;
                long ModFileSize;

                while ((PluginName = reader.ReadLine()) != null) // Read Plugins.txt
                {
                    try
                    {
                        if (PluginName != "" && !tools.BethFiles.Contains(PluginName))
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
                                AuthorVersion = "";
                                ASafe = "";
                                ModTimeStamp = "";
                                ModID = "";
                                ModFileSize = 0;
                                URL = "";

                                for (i = 0; i < CreationsPlugin.Count; i++)
                                {
                                    if (CreationsPlugin[i][..CreationsPlugin[i].LastIndexOf('.')] + ".esm" == PluginName ||
                                        CreationsPlugin[i][..CreationsPlugin[i].LastIndexOf('.')] + ".esp" == PluginName)
                                    {
                                        Description = CreationsTitle[i]; // Add Content Catalog description if available
                                        ModVersion = CreationsVersion[i];
                                        VersionCheck = double.Parse((ModVersion[..ModVersion.IndexOf('.')]));
                                        AuthorVersion = ModVersion[(ModVersion.IndexOf('.') + 1)..];
                                        ModVersion = Tools.ConvertTime(VersionCheck).ToString();

                                        ModFiles = CreationsFiles[i];
                                        if (AchievmentSafe[i])
                                            ASafe = "Yes";
                                        else
                                            ASafe = "";
                                        ModTimeStamp = Tools.ConvertTime(TimeStamp[i]).ToString();
                                        ModID = CreationsID[i];
                                        ModFileSize = FileSize[i] / 1024;
                                        if (Groups.plugins != null && Groups.plugins[i].url != null && dataGridView1.Columns["URL"].Visible)
                                        {
                                            URL = Groups.plugins[i].url[0].link.ToString();
                                            Debug.WriteLine(PluginName + " " + URL);
                                        }
                                    }
                                }

                                int rowIndex = this.dataGridView1.Rows.Add();
                                var row = this.dataGridView1.Rows[rowIndex];
                                ;
                                // Populate datagrid from LOOT groups
                                if (Properties.Settings.Default.LOOTPath != "" && Groups.groups != null && dataGridView1.Columns["Group"].Visible)
                                    for (i = 0; i < Groups.plugins.Count; i++)
                                        if (Groups.plugins[i].name == PluginName)
                                            row.Cells["Group"].Value = Groups.plugins[i].group;

                                if (PluginName.StartsWith("sfbgs")) // Assume Bethesda plugin
                                    row.Cells["Group"].Value = "Bethesda";
                                row.Cells["ModEnabled"].Value = ModEnabled;
                                row.Cells["PluginName"].Value = PluginName;
                                row.Cells["Description"].Value = Description;
                                row.Cells["Version"].Value = ModVersion;
                                row.Cells["AuthorVersion"].Value = AuthorVersion;
                                if (dataGridView1.Columns["TimeStamp"].Visible)
                                    row.Cells["TimeStamp"].Value = ModTimeStamp;
                                if (dataGridView1.Columns["Achievements"].Visible)
                                    row.Cells["Achievements"].Value = ASafe;
                                if (dataGridView1.Columns["Files"].Visible)
                                    row.Cells["Files"].Value = ModFiles;
                                if (ModFileSize != 0 && dataGridView1.Columns["FileSize"].Visible)
                                    row.Cells["FileSize"].Value = ModFileSize;
                                //if (dataGridView1.Columns["CreationsID"].Visible)
                                row.Cells["CreationsID"].Value = ModID;
                                if (dataGridView1.Columns["Index"].Visible)
                                    row.Cells["Index"].Value = IndexCount++;
                                if (dataGridView1.Columns["URL"].Visible)
                                    row.Cells["URL"].Value = URL;

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
                Parallel.ForEach(Directory.EnumerateFiles(directory, "*.esm", SearchOption.TopDirectoryOnly), file =>
                {
                    esmCount++;
                    esmFiles.Add(file);
                });
                Parallel.ForEach(Directory.EnumerateFiles(directory, "*.esp", SearchOption.TopDirectoryOnly), file =>
                {
                    espCount++;
                });
                Parallel.ForEach(Directory.EnumerateFiles(directory, "*.ba2", SearchOption.TopDirectoryOnly), file =>
                {
                    ba2Count++;
                });
                StatText = "Total Mods: " + dataGridView1.RowCount + ", Creations Mods: " + TitleCount.ToString() + ", Other: " +
                    (dataGridView1.RowCount - TitleCount).ToString() + ", Enabled: " + EnabledCount.ToString() + ", esm files: " +
                    esmCount.ToString() + " " + "Archives: " + ba2Count.ToString();

                if (espCount > 0)
                    StatText += ", esp files: " + espCount.ToString();

                dataGridView1.EndEdit();
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
                    LastProfile = cmbProfile.Items[index].ToString();
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show(ex.Message);
#endif
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

            if (GridSorted)
                return;
            if (dataGridView1.Rows.Count > 0)
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
            sbar2("Plugins.txt saved");
            isModified = false;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {

            this.Close();
        }
        private void MoveUp()
        {
            int rowIndex = dataGridView1.CurrentCell.RowIndex;
            int colIndex = dataGridView1.CurrentCell.ColumnIndex;
            if (rowIndex == 0)
                return; // Already at the top

            DataGridViewRow selectedRow = dataGridView1.Rows[rowIndex];
            dataGridView1.Rows.Remove(selectedRow);
            dataGridView1.Rows.Insert(rowIndex - 1, selectedRow);
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
            string sourceFileName = Tools.StarfieldAppData + @"\Plugins.txt";
            string destFileName = sourceFileName + ".bak";

            if (isModified)
            {
                MessageBox.Show("Plugins have been modified\nClick Ok to save first or Cancel to revert", "Backkup not done");
                return;
            }

            try
            {
                File.Copy(sourceFileName, destFileName, true); // overwrite
                sbar2("Backup done");
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
            string sourceFileName = Tools.StarfieldAppData + @"\Plugins.txt.bak";
            string destFileName = Tools.StarfieldAppData + @"\Plugins.txt";

            try
            {
                // Copy the file
                File.Copy(sourceFileName, destFileName, true); // overwrite
                InitDataGrid();

                toolStripStatusTertiary.ForeColor = DefaultForeColor;
                sbar2("Restore done");
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
            dataGridView1.Rows[^1].Cells[colIndex].Selected = true;
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
            sbar3("Warning! - Plugins sorted - saving changes disabled");
            toolStripStatusTertiary.ForeColor = Color.Red;
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
            sbar2("All mods disabled");
            isModified = true;
        }

        private void EnableAll()
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                dataGridView1.Rows[i].Cells["ModEnabled"].Value = true;
            }
            sbar2("All mods enabled");
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
            int currentIndex = dataGridView1.CurrentCell.RowIndex;
            int nextIndex = currentIndex + 1;
            DataGridSring = dataGridView1.Rows[currentIndex].Cells["PluginName"].Value.ToString().ToLower();
            if (DataGridSring.Contains(TextBoxString))
            {
                if (nextIndex < dataGridView1.Rows.Count)
                {
                    DataGridViewRow nextRow = dataGridView1.Rows[nextIndex];
                    for (ModIndex = nextRow.Index; ModIndex < dataGridView1.RowCount; ModIndex++)
                    {
                        DataGridSring = dataGridView1.Rows[ModIndex].Cells["PluginName"].Value.ToString().ToLower();
                        if (DataGridSring.Contains(TextBoxString))
                        {
                            sbar2("Found " + txtSearchBox.Text + " in " + dataGridView1.Rows[ModIndex].Cells["PluginName"].Value.ToString());
                            dataGridView1.CurrentCell = dataGridView1.Rows[ModIndex].Cells["PluginName"];
                            return;
                        }
                    }
                }
            }
            for (ModIndex = 0; ModIndex < dataGridView1.RowCount; ModIndex++)
            {
                DataGridSring = dataGridView1.Rows[ModIndex].Cells["PluginName"].Value.ToString().ToLower();
                if (DataGridSring.Contains(TextBoxString))
                {
                    sbar2("Found " + txtSearchBox.Text + " in " + dataGridView1.Rows[ModIndex].Cells["PluginName"].Value.ToString());
                    dataGridView1.CurrentCell = dataGridView1.Rows[ModIndex].Cells["PluginName"];
                    break;
                }
                else
                    sbar2(txtSearchBox.Text + " not found ");
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

            if (dataGridView1.Rows.Count > 0)
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
                File.Copy(ProfileName, Tools.StarfieldAppData + "\\Plugins.txt", true);
                Properties.Settings.Default.LastProfile = ProfileName[(ProfileName.LastIndexOf('\\') + 1)..];
                SaveSettings();
                isModified = false;
                //SavePlugings();
                InitDataGrid();
            }
            catch
            {
                sbar2("Error switching profile");
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

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e) // Keyboard shortcuts
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
                case Keys.R:
                    Tools.StartGame(GameVersion);
                    break;
            }
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

        private int AddMissing() // Look for .esm or .esp files to add to Plugins.txt returns no. of file added
        {
            int AddedFiles = 0;
            List<string> esmespFiles = [];
            List<string> PluginFiles = [];
            List<string> BethFiles = tools.BethFiles;
            // Exclude game files - will probably need updating after updates

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
                esmespFiles.Add(missingFile[(missingFile.LastIndexOf('\\') + 1)..]);
            }
            foreach (var missingFile in Directory.EnumerateFiles(directory, "*.esp", SearchOption.TopDirectoryOnly))
            {
                esmespFiles.Add(missingFile[(missingFile.LastIndexOf('\\') + 1)..]);
            }
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                PluginFiles.Add((string)dataGridView1.Rows[i].Cells["PluginName"].Value);
            List<string> MissingFiles = esmespFiles.Except(PluginFiles).ToList();

            List<string> FilesToAdd = MissingFiles.Except(BethFiles).ToList();  // Exclude BGS esm files
            if (FilesToAdd.Count > 0)
            {
                for (int i = 0; i < FilesToAdd.Count; i++)
                {
                    AddedFiles++;
                    int rowIndex = this.dataGridView1.Rows.Add();
                    var row = this.dataGridView1.Rows[rowIndex];
                    if (FilesToAdd[i].Contains(".esm"))
                        row.Cells["ModEnabled"].Value = true;
                    else
                        row.Cells["ModEnabled"].Value = false;
                    row.Cells["PluginName"].Value = FilesToAdd[i];
                }
                dataGridView1.CurrentCell = dataGridView1.Rows[dataGridView1.RowCount - 1].Cells["PluginName"];
                sbar3(AddedFiles.ToString() + " file(s) added");
                isModified = true;
            }
            else
                sbar3("Nothing to add");
            return AddedFiles;
        }

        private int RemoveMissing() // Remove entries from Plugins.txt for missing .esm files. Returns number of removals
        {
            int RemovedFiles = 0;
            List<string> esmespFiles = [];
            List<string> PluginFiles = [];
            List<string> FilesToRemove = [];

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
                esmespFiles.Add(missingFile[(missingFile.LastIndexOf('\\') + 1)..]);
            }
            foreach (var missingFile in Directory.EnumerateFiles(directory, "*.esp", SearchOption.TopDirectoryOnly))
            {
                esmespFiles.Add(missingFile[(missingFile.LastIndexOf('\\') + 1)..]);
            }
            int i;
            for (i = 0; i < dataGridView1.Rows.Count; i++)
                PluginFiles.Add((string)dataGridView1.Rows[i].Cells["PluginName"].Value);

            List<string> MissingFiles = PluginFiles.Except(esmespFiles).ToList();

            for (i = 0; i < MissingFiles.Count; i++)
            {
                FilesToRemove.Add(MissingFiles[i]);
                RemovedFiles++;
            }

            for (i = 0; i < tools.BethFiles.Count; i++)  // Remove base game files
            {
                FilesToRemove.Add(tools.BethFiles[i]);
#if DEBUG
                Debug.WriteLine(FilesToRemove[i]);
#endif
            }

            if (FilesToRemove.Count > 0)
            {
                for (i = 0; i < FilesToRemove.Count; i++)
                {
                    for (int j = 0; j < dataGridView1.Rows.Count; j++)
                        if ((string)dataGridView1.Rows[j].Cells["PluginName"].Value == FilesToRemove[i])
                        {
                            dataGridView1.Rows.RemoveAt(j);
                            Debug.WriteLine("Removing " + FilesToRemove[i]);
                        }
                }
                sbar3(RemovedFiles.ToString() + " file(s) removed");
                isModified = true;
            }

            if (RemovedFiles == 0)
                sbar3("Nothing to remove");
            return RemovedFiles;
        }

        private void AddRemove()
        {
            int addedMods, removedMods;
            addedMods = AddMissing();
            removedMods = RemoveMissing();
            if (addedMods != 0 || removedMods != 0)
            {
                SavePlugings();
                InitDataGrid();
                isModified = false;
                sbar3(addedMods.ToString() + " Mods added, " + removedMods.ToString() + " Mods removed");
            }
            else
            {
                isModified = false;
                sbar3("Plugins.txt is up to date");
            }
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
                    sbar2("Installing mod...");
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
                sbar3("Mod installed");
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
            string tempstr = "", Group = "";
            List<string> ExportMods = new();
            SaveFileDialog ExportActive = new()
            {
                Filter = "Txt File|*.txt",
                Title = "Export Active Plugins"
            };

            DialogResult dlgResult = ExportActive.ShowDialog();
            if (dlgResult == DialogResult.OK)
            {

                for (i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if ((bool)dataGridView1.Rows[i].Cells["ModEnabled"].Value)
                    {
                        tempstr = (string)dataGridView1.Rows[i].Cells["Group"].Value;
                        if (tempstr != "" && tempstr != null && tempstr != Group)
                        {
                            Group = tempstr;
                            ExportMods.Add("\n# " + Group);
                        }
                        tempstr = "*" + (string)dataGridView1.Rows[i].Cells["PluginName"].Value;
                        ExportMods.Add(tempstr);
                    }
                }
                if (ExportMods.Count == 0)
                {
                    sbar3("Nothing to export");
                    return;
                }
                if (ExportMods != null)
                    if (ExportMods[0].StartsWith("\n# "))
                        ExportMods[0] = ExportMods[0][1..];
                tempstr = "# Exported active mod list from hst Starfield Tools";
                if (Profiles)
                    tempstr += " using profile " + cmbProfile.Text;

                using StreamWriter writer = new(ExportActive.FileName);
                writer.WriteLine(tempstr + "\n");
                for (i = 0; i < ExportMods.Count; i++)
                    writer.WriteLine(ExportMods[i]);
                sbar3("Export done");
                Process.Start("explorer.exe", (ExportActive.FileName));
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
            Process.Start("explorer.exe", Tools.StarfieldAppData);
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
            DialogResult DialogResult = MessageBox.Show(@"This will delete all files related to the '" + ModName + @"' mod", "Delete " + ModName + " - Are you sure?",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Stop);

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
                sbar3("Mod uninstalled");
            }
            else
                sbar2("Un-install cancelled");
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

        private void toolStripMenuRunMS_Click(object sender, EventArgs e)
        {
            RunGame();
        }

        private void toolStripMenuInstallMod_Click(object sender, EventArgs e)
        {
            InstallMod();
        }
        private void SaveOnDblClick()
        {
            SavePlugings();
        }

        private void SavePlugings()
        {
            var dgCurrent = dataGridView1.CurrentCell;
            SaveLO(Tools.StarfieldAppData + @"\Plugins.txt");
            if (Profiles)
            {
                SaveLO(Properties.Settings.Default.ProfileFolder + "\\" + cmbProfile.Text); // Save profile as well
                toolStripStatusSecondary.Text += ", " + cmbProfile.Text + " profile saved";
            }
            dataGridView1.CurrentCell = dgCurrent;
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            SavePlugings();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            RunGame();
        }
        private static void ShowSplashScreen()
        {
            Form SS = new frmSplashScreen();
            SS.Show();
        }
        private void RunGame()
        {
            Properties.Settings.Default.GameVersion = GameVersion;
            SaveSettings();
            bool result;
            Form SS = new frmSplashScreen();
            SS.Show();

            if (isModified)
            {
                DialogResult = MessageBox.Show("Load order is modified. Cancel and save changes first or press OK to load game without saving", "Launch Game", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (DialogResult == DialogResult.OK)
                {
                    if (Tools.StartGame(GameVersion))
                        result = true;
                    else
                        result = false;
                }
                else
                    result = false;
            }
            else
            {
                if (Tools.StartGame(GameVersion))
                    result = true;
                else
                    result = false;
            }
            if (!result)
            {
                timer1.Stop();
                SS.Close();
            }
            else
                timer1.Start();
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
            string CreationsID = "", OtherURL = "", url = "";
            if (dataGridView1.CurrentRow.Cells["CreationsID"].Value != null)
                CreationsID = dataGridView1.CurrentRow.Cells["CreationsID"].Value.ToString();
            if (dataGridView1.CurrentRow.Cells["URL"].Value != null)
                OtherURL = dataGridView1.CurrentRow.Cells["URL"].Value.ToString();

            if (CreationsID == "" && OtherURL == "")
            {
                sbar3("No link for mod");
                return;
            }

            if (CreationsID != "")
            {
                url = "https://creations.bethesda.net/en/starfield/details/" + CreationsID[3..];
                Tools.OpenUrl(url);  // Open Creations web site
            }
            else
                if (url != "")
                Tools.OpenUrl(url);
        }

        private void toolStripMenuUninstall_Click(object sender, EventArgs e)
        {
            UninstallMod();
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            SaveLO(Tools.StarfieldAppData + @"\Plugins.txt");
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
                    sbar2("LOOT path is required to run LOOT");
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
            toolStripStatusStats.Text = StatusBarMessage;
        }

        private void sbar2(string StatusBarMessage)
        {
            toolStripStatusSecondary.Text = StatusBarMessage;
        }

        private void sbar3(string StatusBarMessage)
        {
            toolStripStatusTertiary.Text = StatusBarMessage;
        }

        private void sbar4(string StatusBarMessage)
        {
            toolStripStatusLabel4.Text = StatusBarMessage;
        }

        private void sbarCCCOn()
        {
            toolStripStatusDelCCC.Text = "Auto delete Starfield.ccc: On";
        }

        private void sbarCCCOff()
        {
            toolStripStatusDelCCC.Text = "Auto delete Starfield.ccc: Off";
        }
        private void toolStripMenuLoot_Click_1(object sender, EventArgs e)
        {
            RunLOOT(false);
        }

        private void toolStripMenuEditPlugins_Click(object sender, EventArgs e)
        {
            string pathToFile = (Tools.StarfieldAppData + @"\Plugins.txt");
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

        private bool Delccc()
        {
            try
            {
                string Starfieldccc = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\My Games\\Starfield\\Starfield.ccc";
                if (File.Exists(Starfieldccc))
                {
                    File.Delete(Starfieldccc);
                    sbar3("Starfield.ccc deleted");
                    return true;
                }
                else
                {
                    sbar3("Starfield.ccc not found");
                    return false;
                }
            }
            catch (Exception ex)
            {
                sbar3("Error deleting Starfield.ccc " + ex.Message);
                return false;
            }

        }
        private void toolStripMenuDeleteCCC_Click(object sender, EventArgs e)
        {
            Delccc();
        }

        private void toolStripMenuAutoDelccc_Click(object sender, EventArgs e)
        {
            toolStripMenuAutoDelccc.Checked = !toolStripMenuAutoDelccc.Checked;
            if (toolStripMenuAutoDelccc.Checked)
                sbarCCCOn();
            else
                sbarCCCOff();
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
            ShowSplashScreen();
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

        private void toolStripMenuCustom_Click(object sender, EventArgs e)
        {
            string CustomEXEFolder;

            CustomEXEFolder = Properties.Settings.Default.CustomEXE;

            CustomEXEFolder = tools.StarfieldGamePath;

            OpenFileDialog OpenEXE = new()
            {
                InitialDirectory = CustomEXEFolder,
                Filter = "exe File|*.exe",
                Title = "Select custom game executable"
            };

            DialogResult result = OpenEXE.ShowDialog();
            if (DialogResult.OK == result)
            {
                if (OpenEXE.FileName != "")
                {
                    Properties.Settings.Default.CustomEXE = OpenEXE.FileName;
                    Properties.Settings.Default.Save();
                }
            }

            toolStripMenuCustom.Checked = !toolStripMenuCustom.Checked;
            if (toolStripMenuCustom.Checked)
            {
                sbar2("Game version set to Custom");
                GameVersion = 2;
                Properties.Settings.Default.GameVersion = GameVersion;
            }
            else
            {
                toolStripMenuSteam.Checked = false;
                toolStripMenuMS.Checked = false;
            }

            Properties.Settings.Default.GameVersion = GameVersion;
            SaveSettings();
        }

        private void toolStripMenuRunCustom_Click(object sender, EventArgs e)
        {
            RunGame();
        }

        private string CheckCatalog()
        {
            frmStarfieldTools StarfieldTools = new();
            StarfieldTools.Show();
            sbar2(StarfieldTools.CatalogStatus);
            return StarfieldTools.CatalogStatus;
        }
        private void toolStripMenuCatalog_Click(object sender, EventArgs e)
        {
            CheckCatalog();
        }

        private void toolStripMenuShortcuts_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("explorer.exe", Tools.CommonFolder + "\\Shortcuts.txt");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error opening Shortcuts.txt");
            }
        }

        private void GameVersionDisplay()
        {
            switch (GameVersion)
            {
                case 0:
                    sbar2("Game version - Steam");
                    break;
                case 1:
                    sbar2("Game version - MS");
                    break;
                case 2:
                    sbar2("Game version - Custom - " + Properties.Settings.Default.CustomEXE);
                    break;
            }
        }

        private void toolStripMenuResetStarfieldCustom_Click(object sender, EventArgs e)
        {
            DialogResult DialogResult = MessageBox.Show("This will restore your StarfieldCustom.ini to a basic version", "Are you sure?",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Stop);

            if (DialogResult == DialogResult.OK)
            {
                try
                {
                    File.Copy(Tools.CommonFolder + "\\StarfieldCustom.ini", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\My Games\\Starfield\\StarfieldCustom.ini", true);
                    sbar3("StarfieldCustom.ini restored");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error restoring StarfieldCustom.ini");
                }
            }
            else
                sbar3("StarfieldCustom.ini not modified");
        }

        private void editStarfieldCustominiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string pathToFile = (Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\My Games\\Starfield\\StarfieldCustom.ini");
            Process.Start("explorer", pathToFile);
        }

        private void editContentCatalogtxtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string pathToFile = Tools.GetCatalog();
            Process.Start("explorer", pathToFile);
        }

        private void btnCheckCatalog_Click(object sender, EventArgs e)
        {
            CheckCatalog();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            this.Close();
        }

        private void toolStripMenuRun_Click(object sender, EventArgs e)
        {
            RunGame();
        }

        private void toolStripMenuSteam_Click(object sender, EventArgs e)
        {
            toolStripMenuSteam.Checked = !toolStripMenuSteam.Checked;
            if (toolStripMenuSteam.Checked)
            {
                toolStripMenuMS.Checked = false;
                toolStripMenuCustom.Checked = false;
                GameVersion = 0;
                sbar2("Game version set to Steam");
                Properties.Settings.Default.GameVersion = GameVersion;
            }
        }

        private void toolStripMenuMS_Click(object sender, EventArgs e)
        {
            toolStripMenuMS.Checked = !toolStripMenuMS.Checked;
            if (toolStripMenuMS.Checked)
            {
                toolStripMenuSteam.Checked = false;
                toolStripMenuCustom.Checked = false;
                GameVersion = 1;
                sbar2("Game version set to MS");
                Properties.Settings.Default.GameVersion = GameVersion;
            }
        }

        private void toolStripMenuFileSize_Click(object sender, EventArgs e)
        {
            toolStripMenuFileSize.Checked = !toolStripMenuFileSize.Checked;
            if (toolStripMenuFileSize.Checked)
                dataGridView1.Columns["FileSize"].Visible = true;
            else
                dataGridView1.Columns["FileSize"].Visible = false;
            Properties.Settings.Default.FileSize = toolStripMenuFileSize.Checked;
        }

        private void compareStarfieldCustominiToBackupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CheckStarfieldCustom())
                sbar3("Same");
            else
                sbar3("Modified");
        }

        private void toolStripMenuExploreCommon_Click(object sender, EventArgs e)
        {
            string pathToFile = (Tools.CommonFolder);
            Process.Start("explorer", pathToFile);
            sbar3("Restart the application for any changes to take effect");
        }

        private void timeStampToolStripMenuItem_Click(object sender, EventArgs e)
        {
            timeStampToolStripMenuItem.Checked = !timeStampToolStripMenuItem.Checked;
            if (timeStampToolStripMenuItem.Checked)
                dataGridView1.Columns["TimeStamp"].Visible = true;
            else
                dataGridView1.Columns["TimeStamp"].Visible = false;
            Properties.Settings.Default.TimeStamp = timeStampToolStripMenuItem.Checked;
        }

        private void resetLoadScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.LoadScreenFilename = "";
            Properties.Settings.Default.Save();
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RefreshDataGrid();
        }

        private int CheckAndDeleteINI(string FileName)
        {
            string FolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\My Games\\Starfield\\";
            if (File.Exists(FolderPath + FileName))
            {
                File.Delete(FolderPath + FileName);
                return 1;
            }
            else
                return 0;
        }
        private void undoVortexChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int ChangeCount = 0;

            DialogResult DialogResult = MessageBox.Show("Are you sure?", "This will remove all changes made by Vortex",
    MessageBoxButtons.OKCancel, MessageBoxIcon.Stop);
            if (DialogResult == DialogResult.OK)
            {
                string FolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\My Games\\Starfield\\";
                if (File.Exists(FolderPath + "StarfieldCustom.ini.base"))
                {
                    File.Copy(FolderPath + "StarfieldCustom.ini.base", FolderPath + "StarfieldCustom.ini", true);
                    File.Delete(FolderPath + "StarfieldCustom.ini.base");
                    ChangeCount++;
                }
                if (File.Exists(FolderPath + "StarfieldPrefs.ini.base"))
                {
                    File.Copy(FolderPath + "StarfieldPrefs.ini.base", FolderPath + "StarfieldPrefs.ini", true);
                    File.Delete(FolderPath + "StarfieldPrefs.ini.base");
                    ChangeCount++;
                }
                ChangeCount += CheckAndDeleteINI("Starfield.ini");
                ChangeCount += CheckAndDeleteINI("Starfield.ini.baked");
                ChangeCount += CheckAndDeleteINI("StarfieldCustom.ini.baked");
                ChangeCount += CheckAndDeleteINI("StarfieldPrefs.ini.baked");
                if (Delccc())
                    ChangeCount++;
                sbar3(ChangeCount + " Change(s) made to Vortex created files");
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            AddRemove();
            if (isModified)
                SavePlugings();
        }

        private void looseFilesDisabledToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string LooseFilesDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\My Games\\Starfield\\",
                filePath = LooseFilesDir + "StarfieldCustom.ini";
            LooseFiles = !LooseFiles;
            if (LooseFiles)
            {
                looseFilesDisabledToolStripMenuItem.Text = "Loose Files Enabled";
                List<string> linesToAppend = new List<string>
        {
            "[Archive]",
            "bInvalidateOlderFiles=1",
            "sResourceDataDirsFinal="
        };

                File.AppendAllLines(filePath, linesToAppend);
                sbar3("Loose Files Enabled");
            }
            else
            {
                looseFilesDisabledToolStripMenuItem.Text = "Loose Files Disabled";
                string[] linesToRemove = { "[Archive]", "bInvalidateOlderFiles=1", "sResourceDataDirsFinal=" };

                // Read all lines from the file
                var lines = File.ReadAllLines(filePath).ToList();

                // Remove the specified lines
                foreach (var lineToRemove in linesToRemove)
                {
                    lines.RemoveAll(line => line.Trim() == lineToRemove);
                }

                // Write the updated lines back to the file
                File.WriteAllLines(filePath, lines);

                sbar3("Loose Files Disabled");
            }

            Properties.Settings.Default.LooseFiles = LooseFiles;

        }

        private void autoUpdateModsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AutoUpdate = !AutoUpdate;
            autoUpdateModsToolStripMenuItem.Checked = !autoUpdateModsToolStripMenuItem.Checked;
            Properties.Settings.Default.AutoUpdate = AutoUpdate;
        }
    }
}
