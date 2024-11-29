using Microsoft.Win32;
using Newtonsoft.Json;
using SevenZipExtractor;
using Starfield_Tools.Common;
using Starfield_Tools.Load_Order_Editor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using YamlDotNet.Serialization;
using File = System.IO.File;

namespace Starfield_Tools
{
    public partial class frmLoadOrder : Form
    {
        public const byte Steam = 0, MS = 1, Custom = 2, SFSE = 3;

        private Rectangle dragBoxFromMouseDown;
        private int rowIndexFromMouseDown, rowIndexOfItemUnderMouseToDrop, GameVersion = Steam;

        readonly Tools tools = new();
        private string StarfieldGamePath, LastProfile;

        bool Profiles = false, GridSorted = false, LooseFiles = false, AutoUpdate = false, ActiveOnly = false, AutoSort = false, isModified = false;

        public frmLoadOrder(string parameter)
        {
            InitializeComponent();

            Tools.CheckGame(); // Exit if Starfield appdata folder not found

            string PluginsPath = Tools.StarfieldAppData + "\\Plugins.txt",
 LooseFilesDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\My Games\\Starfield\\", // Check if loose files are enabled
filePath = LooseFilesDir + "StarfieldCustom.ini";

            Rectangle resolution = Screen.PrimaryScreen.Bounds; // Resize window to 75% of screen width
            double screenWidth = resolution.Width;
            double screenHeight = resolution.Height;
            this.Width = (int)(screenWidth * 0.85);
            this.Height = (int)(screenHeight * 0.85);
            this.StartPosition = FormStartPosition.CenterScreen;

            this.KeyUp += new System.Windows.Forms.KeyEventHandler(KeyEvent); // Handle <enter> for search

            menuStrip1.Font = Properties.Settings.Default.FontSize; // Get settings
            this.Font = Properties.Settings.Default.FontSize;

            GameVersion = Properties.Settings.Default.GameVersion;
            if (GameVersion != MS)
                StarfieldGamePath = Properties.Settings.Default.StarfieldGamePath;
            else
                StarfieldGamePath = Properties.Settings.Default.GamePathMS;

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
                case Steam:
                    toolStripMenuSteam.Checked = true;
                    break;
                case MS:
                    toolStripMenuMS.Checked = true;
                    break;
                case Custom:
                    toolStripMenuCustom.Checked = true;
                    break;
                case SFSE:
                    gameVersionSFSEToolStripMenuItem.Checked = true;
                    break;
            }
            GameVersionDisplay();

            if (Properties.Settings.Default.ProfileOn)
            {
                toolStripMenuProfilesOn.Checked = Properties.Settings.Default.ProfileOn;
                if (toolStripMenuProfilesOn.Checked)
                {
                    Profiles = true;
                    chkProfile.Checked = true;
                }
                else Profiles = false;
            }

            SetupColumns();

            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath).ToList();
                if (lines.Contains("bInvalidateOlderFiles=1"))
                    Properties.Settings.Default.LooseFiles = true;
                else
                    Properties.Settings.Default.LooseFiles = false;
            }

            // Setup other preferences

            switch (Properties.Settings.Default.DarkMode)
            {
                case 0: // Light
                    dataGridView1.EnableHeadersVisualStyles = true;
                    Application.SetColorMode(SystemColorMode.Classic);
                    lightToolStripMenuItem.Checked = true;
                    break;
                case 1: // Dark
                    dataGridView1.EnableHeadersVisualStyles = false;
                    dataGridView1.DefaultCellStyle.SelectionBackColor = Color.Green; // Background color of selected cells
                    dataGridView1.DefaultCellStyle.SelectionForeColor = Color.White; // Text color of selected cells
                    Application.SetColorMode(SystemColorMode.Dark);
                    darkToolStripMenuItem.Checked = true;
                    break;
                case 2: // System
                    Application.SetColorMode(SystemColorMode.System);
                    if (Application.SystemColorMode == SystemColorMode.Dark)
                    {
                        dataGridView1.EnableHeadersVisualStyles = false;
                        dataGridView1.DefaultCellStyle.SelectionBackColor = Color.Green; // Background color of selected cells
                        dataGridView1.DefaultCellStyle.SelectionForeColor = Color.White; // Text color of selected cells
                    }
                    systemToolStripMenuItem.Checked = true;
                    break;
            }

            SetMenus();

            frmStarfieldTools StarfieldTools = new(); // Check the catalog
            sbar4(StarfieldTools.CatalogStatus);
            if (StarfieldTools.CatalogStatus != null)
                if (StarfieldTools.CatalogStatus.Contains("Error"))
                    StarfieldTools.Show(); // Show catalog fixer if catalog broken

            cmbProfile.Enabled = Profiles;
            GetProfiles();
            if (!Profiles)
                InitDataGrid();

            // Do a 1-time backup of Plugins.txt if it doesn't exist
            if (!File.Exists(PluginsPath + ".bak"))
            {
                sbar2("Plugins.txt backed up to Plugins.txt.bak");
                File.Copy(PluginsPath, PluginsPath + ".bak");
            }

            if (AutoUpdate)
                sbar4(AddRemove());

            if (Properties.Settings.Default.AutoReset)
                ResetDefaults();

#if DEBUG
            testToolStripMenu.Visible = true;
#endif
        }

        private void SetupColumns()
        {
            SetColumnVisibility(Properties.Settings.Default.TimeStamp, timeStampToolStripMenuItem, dataGridView1.Columns["TimeStamp"]);
            SetColumnVisibility(Properties.Settings.Default.Achievements, toolStripMenuAchievements, dataGridView1.Columns["Achievements"]);
            SetColumnVisibility(Properties.Settings.Default.CreationsID, toolStripMenuCreationsID, dataGridView1.Columns["CreationsID"]);
            SetColumnVisibility(Properties.Settings.Default.Files, toolStripMenuFiles, dataGridView1.Columns["Files"]);
            SetColumnVisibility(Properties.Settings.Default.Group, toolStripMenuGroup, dataGridView1.Columns["Group"]);
            SetColumnVisibility(Properties.Settings.Default.Index, toolStripMenuIndex, dataGridView1.Columns["Index"]);
            SetColumnVisibility(Properties.Settings.Default.FileSize, toolStripMenuFileSize, dataGridView1.Columns["FileSize"]);
            SetColumnVisibility(Properties.Settings.Default.URL, uRLToolStripMenuItem, dataGridView1.Columns["URL"]);
            SetColumnVisibility(Properties.Settings.Default.Version, toolStripMenuVersion, dataGridView1.Columns["Version"]);
            SetColumnVisibility(Properties.Settings.Default.AuthorVersion, toolStripMenuAuthorVersion, dataGridView1.Columns["AuthorVersion"]);
        }
        private void SetMenus()
        {
            toolStripMenuProfilesOn.Checked = Properties.Settings.Default.ProfileOn;
            compareProfilesToolStripMenuItem.Checked = Properties.Settings.Default.CompareProfiles;

            LooseFiles = Properties.Settings.Default.LooseFiles;
            if (LooseFiles)
            {
                looseFilesDisabledToolStripMenuItem.Text = "Loose Files Enabled";
                sbarCCC("Loose files enabled");
            }
            else
            {
                looseFilesDisabledToolStripMenuItem.Text = "Loose Files Disabled";
                sbarCCC("Loose files disabled");
            }

            autoSortToolStripMenuItem.Checked = Properties.Settings.Default.AutoSort;
            AutoSort = Properties.Settings.Default.AutoSort;

            activeOnlyToolStripMenuItem.Checked = Properties.Settings.Default.ActiveOnly;
            ActiveOnly = Properties.Settings.Default.ActiveOnly;

            autoUpdateModsToolStripMenuItem.Checked = Properties.Settings.Default.AutoUpdate;
            AutoUpdate = Properties.Settings.Default.AutoUpdate;

            toolStripMenuAutoDelccc.Checked = Properties.Settings.Default.AutoDelccc;

            autoResetToolStripMenuItem.Checked = Properties.Settings.Default.AutoReset;

            showTimeToolStripMenuItem.Checked = Properties.Settings.Default.Showtime;
            timer2.Enabled = Properties.Settings.Default.Showtime;
        }

        private static void SetColumnVisibility(bool condition, ToolStripMenuItem menuItem, DataGridViewColumn column)
        {
            menuItem.Checked = condition;
            column.Visible = condition;
        }
        private void RefreshDataGrid()
        {
            if (!Profiles)
                InitDataGrid();

            GetProfiles();
            GridSorted = false;
            isModified = false;
            GameVersionDisplay();
            toolStripStatusTertiary.ForeColor = DefaultForeColor;
            sbar3("Refresh complete");
            sbar4("");
        }

        private void KeyEvent(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
                RefreshDataGrid();
        }

        private static bool CheckStarfieldCustom()
        {
            bool result = Tools.FileCompare(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                "\\My Games\\Starfield\\StarfieldCustom.ini", Tools.CommonFolder + "\\StarfieldCustom.ini");
            return result;
        }
        private void InitDataGrid()
        {
            bool ModEnabled;
            int EnabledCount = 0, IndexCount = 1, i, TitleCount = 0, esmCount = 0, espCount = 0, ba2Count = 0, rowIndex;
            string loText, LOOTPath = Properties.Settings.Default.LOOTPath, PluginName, Description, ModFiles, ModVersion, AuthorVersion, ASafe, ModTimeStamp, ModID, URL,
                StatText = "", directory;
            List<string> CreationsPlugin = [];
            List<string> CreationsTitle = [];
            List<string> CreationsFiles = [];
            List<string> CreationsVersion = [];
            List<bool> AchievementSafe = [];
            List<long> TimeStamp = [];
            List<string> CreationsID = [];
            List<string> esmFiles = [];
            List<long> FileSize = [];
            long ModFileSize;
            DateTime start = new(1970, 1, 1, 0, 0, 0, 0);

            if (!File.Exists(Tools.GetCatalog()))
            {
                MessageBox.Show("Missing ContentCatalog.txt");
                return;
            }

            sbar("Loading...");
            sbar3("");
            statusStrip1.Refresh();

            btnSave.Enabled = true;
            saveToolStripMenuItem.Enabled = true;

            dataGridView1.Rows.Clear();
            dataGridView1.Refresh();

            SetColumnVisibility(false, toolStripMenuCreationsID, dataGridView1.Columns["CreationsID"]);
            SetColumnVisibility(false, uRLToolStripMenuItem, dataGridView1.Columns["URL"]);

            string jsonFilePath = Tools.GetCatalog();
            string json = File.ReadAllText(jsonFilePath); // Read catalog

            Tools.Configuration Groups = new();
            Tools.Configuration Url = new();

            if (toolStripMenuGroup.Checked && LOOTPath != "" && dataGridView1.Columns["Group"].Visible) // Read LOOT groups
            {
                try
                {
                    var deserializer = new DeserializerBuilder().Build();
                    var yamlContent = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\LOOT\games\Starfield\userlist.yaml");
                    //var yamlContent = File.ReadAllText(@"C:\Users\hst12\Documents\GitHub\starfield\masterlist.yaml");
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
                var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Tools.Creation>>(json); // Read ContentCatalog.txt
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
                        AchievementSafe.Add(kvp.Value.AchievementSafe);
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
#if DEBUG
                MessageBox.Show(ex.Message);
#endif
                sbar(ex.Message);
                json = Tools.MakeHeaderBlank();
                File.WriteAllText(Tools.GetCatalog(), json);

            }

            loText = Tools.StarfieldAppData + @"\Plugins.txt";
            if (!File.Exists(loText))
            {
                MessageBox.Show(@"Missing Plugins.txt file

Click Ok to create a blank Plugins.txt file
Click File->Restore if you have a backup of your Plugins.txt file
Alternatively, run the game once to have it create a Plugins.txt file for you.", "Plugins.txt not found");
                using StreamWriter writer = new(loText);
                writer.Write("# This file is used by Starfield to keep track of your downloaded content.\n# Please do not modify this file.\n");
                sbar("");
                return;

            }
            using (var reader = new StreamReader(loText))
            {

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
                                        AuthorVersion = ModVersion[(ModVersion.IndexOf('.') + 1)..];
                                        //ModVersion = start.AddSeconds(double.Parse((ModVersion[..ModVersion.IndexOf('.')]))). ToString();
                                        ModVersion = start.AddSeconds(double.Parse(ModVersion[..ModVersion.IndexOf('.')])).Date.ToString("yyyy-MM-dd");


                                        ModFiles = CreationsFiles[i];
                                        if (AchievementSafe[i])
                                            ASafe = "Yes";
                                        else
                                            ASafe = "";
                                        ModTimeStamp = Tools.ConvertTime(TimeStamp[i]).ToString();
                                        ModID = CreationsID[i];
                                        ModFileSize = FileSize[i] / 1024;
                                        URL = "https://creations.bethesda.net/en/starfield/details/" + ModID;
                                    }
                                }

                                rowIndex = this.dataGridView1.Rows.Add();
                                var row = this.dataGridView1.Rows[rowIndex];

                                // Populate datagrid from LOOT groups

                                if (LOOTPath != "" && Groups.groups != null && dataGridView1.Columns["Group"].Visible)
                                    for (i = 0; i < Groups.plugins.Count; i++)
                                        if (Groups.plugins[i].name == PluginName)
                                        {
                                            row.Cells["Group"].Value = Groups.plugins[i].group;
                                            if (Groups.plugins[i].url != null)
                                            {
                                                URL = Groups.plugins[i].url[0].link;
                                                Description = Groups.plugins[i].url[0].name;
                                            }
                                            break;
                                        }

                                if (PluginName.StartsWith("sfbgs")) // Assume Bethesda plugin
                                    if (row.Cells["Group"].Value == null)
                                        row.Cells["Group"].Value = "Bethesda Game Studios Creations";
                                    else
                                        row.Cells["Group"].Value += " (Bethesda)";

                                row.Cells["ModEnabled"].Value = ModEnabled;
                                row.Cells["PluginName"].Value = PluginName;
                                row.Cells["Description"].Value = Description;
                                if (dataGridView1.Columns["Version"].Visible)
                                    row.Cells["Version"].Value = ModVersion;
                                if (dataGridView1.Columns["AuthorVersion"].Visible)
                                    row.Cells["AuthorVersion"].Value = AuthorVersion;

                                if (dataGridView1.Columns["TimeStamp"].Visible)
                                    row.Cells["TimeStamp"].Value = ModTimeStamp;
                                if (dataGridView1.Columns["Achievements"].Visible)
                                    row.Cells["Achievements"].Value = ASafe;
                                if (dataGridView1.Columns["Files"].Visible)
                                    row.Cells["Files"].Value = ModFiles;
                                if (ModFileSize != 0 && dataGridView1.Columns["FileSize"].Visible)
                                    row.Cells["FileSize"].Value = ModFileSize;
                                row.Cells["CreationsID"].Value = ModID;
                                if (dataGridView1.Columns["Index"].Visible)
                                    row.Cells["Index"].Value = IndexCount++;
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

            SetupColumns();

            // Get mod stats
            try
            {
                directory = StarfieldGamePath + @"\Data";

                foreach (string file in Directory.EnumerateFiles(directory, "*.esm", SearchOption.TopDirectoryOnly))
                {
                    esmCount++;
                }

                foreach (string file in Directory.EnumerateFiles(directory, "*.esp", SearchOption.TopDirectoryOnly))
                {
                    espCount++;
                }

                foreach (string file in Directory.EnumerateFiles(directory, "*.ba2", SearchOption.TopDirectoryOnly))
                {
                    ba2Count++;
                }

                StatText = "Total Mods: " + dataGridView1.RowCount + ", Creations Mods: " + TitleCount.ToString() + ", Other: " +
                    (dataGridView1.RowCount - TitleCount).ToString() + ", Enabled: " + EnabledCount.ToString() + ", esm files: " +
                    esmCount.ToString() + " " + "Archives: " + ba2Count.ToString();

                if (espCount > 0)
                    StatText += ", esp files: " + espCount.ToString();

            }
            catch (Exception ex)
            {
                sbar("Starfield path needs to be set for mod stats");
#if DEBUG
                MessageBox.Show(ex.Message);
#endif
            }

            if (ActiveOnly)
            {
                sbar("Hiding inactive mods...");
                statusStrip1.Refresh();
                for (i = 0; i < dataGridView1.RowCount; i++)
                    if ((bool)dataGridView1.Rows[i].Cells["ModEnabled"].Value == false && dataGridView1.RowCount > 0)
                        dataGridView1.Rows[i].Visible = false;
            }
            sbar(StatText);
            dataGridView1.EndEdit();
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
                MessageBox.Show(ex.Message);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void SaveLO(string PluginFileName)
        {

            bool ModEnabled;
            string ModLine;
            int i;

            if (GridSorted)
            {
                MessageBox.Show("Save disabled");
                return;
            }

            using (StreamWriter writer = new(PluginFileName))
            {
                writer.Write("# This file is used by Starfield to keep track of your downloaded content.\n# Please do not modify this file.\n");
                for (i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    ModEnabled = (bool)dataGridView1.Rows[i].Cells["ModEnabled"].Value;
                    ModLine = (string)dataGridView1.Rows[i].Cells["PluginName"].Value;
                    if (ModEnabled)
                        writer.Write("*"); // Insert a * for enabled mods then write the mod filename
                    writer.WriteLine(ModLine);
                }
            }

            sbar2(Path.GetFileName(PluginFileName) + " saved");
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

            if (ActiveOnly)
                ActiveOnlyToggle();

            DataGridViewRow selectedRow = dataGridView1.Rows[rowIndex];
            dataGridView1.Rows.Remove(selectedRow);
            dataGridView1.Rows.Insert(rowIndex - 1, selectedRow);
            dataGridView1.Rows[rowIndex - 1].Selected = true;
            dataGridView1.Rows[rowIndex - 1].Cells[colIndex].Selected = true;
            isModified = true;
        }
        private void btnUp_Click(object sender, EventArgs e)
        {
            MoveUp();
        }

        private void MoveDown()
        {
            int rowIndex = dataGridView1.SelectedCells[0].OwningRow.Index;
            int colIndex = 1;

            if (rowIndex == dataGridView1.Rows.Count - 1)
                return; // Already at the bottom

            if (ActiveOnly)
                ActiveOnlyToggle();

            DataGridViewRow selectedRow = dataGridView1.Rows[rowIndex];
            dataGridView1.Rows.Remove(selectedRow);
            dataGridView1.Rows.Insert(rowIndex + 1, selectedRow);
            dataGridView1.ClearSelection();
            dataGridView1.Rows[rowIndex + 1].Selected = true;
            dataGridView1.Rows[rowIndex + 1].Cells[colIndex].Selected = true;
            isModified = true;
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
                MessageBox.Show("Plugins have been modified\nClick Ok to save first or Cancel to revert", "Backup not done");
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
                SavePlugins();
                sbar2("Restore done");
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
            int colIndex = 1;

            dataGridView1.Rows.Remove(selectedRow);
            dataGridView1.Rows.Insert(0, selectedRow);
            dataGridView1.ClearSelection();
            dataGridView1.Rows[0].Cells[colIndex].Selected = true;
            isModified = true;
        }

        private void MoveBottom()
        {
            int rowIndex = dataGridView1.SelectedCells[0].RowIndex;
            int colIndex = 1;
            DataGridViewRow selectedRow = dataGridView1.Rows[rowIndex];

            dataGridView1.Rows.Remove(selectedRow);
            dataGridView1.Rows.Insert(dataGridView1.Rows.Count, selectedRow);
            dataGridView1.ClearSelection();
            dataGridView1.Rows[^1].Cells[colIndex].Selected = true;
            isModified = true;
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
            //InitDataGrid();
        }

        private void EnableAll()
        {
            if (ActiveOnly)
                ActiveOnlyToggle();
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
            int ModIndex, currentIndex, nextIndex;
            string DataGridString, TextBoxString;
            if (txtSearchBox.Text == "")
                return;
            TextBoxString = txtSearchBox.Text.ToLower(); // Do lower case only search

            if (dataGridView1.CurrentCell != null)
                currentIndex = dataGridView1.CurrentCell.RowIndex;
            else
            {
                return;
            }
            nextIndex = currentIndex + 1;
            DataGridString = dataGridView1.Rows[currentIndex].Cells["PluginName"].Value.ToString().ToLower();
            if (DataGridString.Contains(TextBoxString))
            {
                if (nextIndex < dataGridView1.Rows.Count)
                {
                    DataGridViewRow nextRow = dataGridView1.Rows[nextIndex];
                    for (ModIndex = nextRow.Index; ModIndex < dataGridView1.RowCount; ModIndex++)
                    {
                        DataGridString = dataGridView1.Rows[ModIndex].Cells["PluginName"].Value.ToString().ToLower();
                        if (DataGridString.Contains(TextBoxString))
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
                DataGridString = dataGridView1.Rows[ModIndex].Cells["PluginName"].Value.ToString().ToLower();
                if (DataGridString.Contains(TextBoxString))
                {
                    sbar2("Found " + txtSearchBox.Text + " in " + dataGridView1.Rows[ModIndex].Cells["PluginName"].Value.ToString());
                    if (dataGridView1.Rows[ModIndex].Visible)
                        dataGridView1.CurrentCell = dataGridView1.Rows[ModIndex].Cells["PluginName"];
                    else
                        sbar2("Mod found but is inactive");
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
            SavePlugins();
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

            Tools.OpenUrl("https://creations.bethesda.net/en/starfield/all?platforms=PC&sort=latest_uploaded");
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
                isModified = true;
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
            if (Properties.Settings.Default.CompareProfiles)
            {
                var currentProfile = File.ReadAllLines(Tools.StarfieldAppData + "\\Plugins.txt").ToList();
                var newProfile = File.ReadAllLines(ProfileName).ToList();

                var Difference = newProfile.Except(currentProfile)
                                           .Where(s => s.StartsWith("*"))
                                           .Select(s => $"New Profile added: {s.Replace("*", string.Empty).Replace("#", string.Empty)}")
                                           .Concat(currentProfile.Except(newProfile)
                                           .Where(s => s.StartsWith("*"))
                                           .Select(s => $"Previous Profile removed: {s.Replace("*", string.Empty).Replace("#", string.Empty)}"))
                                           .ToList();

                if (Difference.Count > 0)
                {
                    Form fpc = new frmProfileCompare(Difference);
                    fpc.Show();
                }
            }

            try
            {
                File.Copy(ProfileName, Tools.StarfieldAppData + "\\Plugins.txt", true);
                Properties.Settings.Default.LastProfile = ProfileName[(ProfileName.LastIndexOf('\\') + 1)..];
                SaveSettings();
                isModified = false;
                /*if (AutoUpdate)
                    AddRemove();*/
                InitDataGrid();
            }
            catch (Exception ex)
            {
                sbar2("Error switching profile");
#if DEBUG
                MessageBox.Show(ex.Message);
#endif
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
            if (DialogResult.OK == result && OpenPlugins.FileName != "")
                if (Profiles)
                {
                    {
                        Properties.Settings.Default.ProfileFolder = OpenPlugins.FileName[..OpenPlugins.FileName.LastIndexOf('\\')];
                        SwitchProfile(OpenPlugins.FileName);
                        GetProfiles();
                        Properties.Settings.Default.Save();
                    }
                }
                else
                    SwitchProfile(OpenPlugins.FileName);
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
                    RunGame();
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
        }

        private void toolStripMenuCleanup_Click(object sender, EventArgs e)
        {
            RemoveMissing();
        }

        private int AddMissing() // Look for .esm or .esp files to add to Plugins.txt returns no. of file added
        {
            int AddedFiles = 0, rowIndex;
            List<string> esmespFiles = [];
            List<string> PluginFiles = [];
            List<string> BethFiles = tools.BethFiles;
            // Exclude game files - will probably need updating after updates

            string directory = StarfieldGamePath;
            if (directory == "" || directory == null)
                directory = tools.SetStarfieldGamePath();
            if (!File.Exists(directory + "\\Starfield.exe"))
            {
                MessageBox.Show("Can't continue without game installation path");
                return 0;
            }
            directory += @"\Data";

            /*            if (directory == "\\Data")
                        {
                            MessageBox.Show("Can't continue without game installation path");
                            return 0;
                        }*/
            foreach (var missingFile in Directory.EnumerateFiles(directory, "*.esm", SearchOption.TopDirectoryOnly))
            {
                esmespFiles.Add(missingFile[(missingFile.LastIndexOf('\\') + 1)..]);
            };

            foreach (var missingFile in Directory.EnumerateFiles(directory, "*.esp", SearchOption.TopDirectoryOnly))
            {
                esmespFiles.Add(missingFile[(missingFile.LastIndexOf('\\') + 1)..]);
            };

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                PluginFiles.Add((string)dataGridView1.Rows[i].Cells["PluginName"].Value);
            List<string> MissingFiles = esmespFiles.Except(PluginFiles).ToList();

            List<string> FilesToAdd = MissingFiles.Except(BethFiles).ToList();  // Exclude BGS esm files
            if (FilesToAdd.Count > 0)
            {
                for (int i = 0; i < FilesToAdd.Count; i++)
                {
                    AddedFiles++;
                    rowIndex = this.dataGridView1.Rows.Add();
                    var row = this.dataGridView1.Rows[rowIndex];
                    if (FilesToAdd[i].Contains(".esm") && FilesToAdd[i] != null)
                        row.Cells["ModEnabled"].Value = true;
                    else
                        row.Cells["ModEnabled"].Value = false;
                    row.Cells["PluginName"].Value = FilesToAdd[i];
                }
                dataGridView1.CurrentCell = dataGridView1.Rows[dataGridView1.RowCount - 1].Cells["PluginName"];
                //sbar3(AddedFiles.ToString() + " file(s) added");
                isModified = true;
            }
            return AddedFiles;
        }

        private int RemoveMissing() // Remove entries from Plugins.txt for missing .esm files. Returns number of removals
        {
            int RemovedFiles = 0, i, j;
            List<string> esmespFiles = [];
            List<string> PluginFiles = [];
            List<string> FilesToRemove = [];

            string directory = StarfieldGamePath;
            if (directory == "" || directory == null)
                directory = tools.SetStarfieldGamePath();

            if (!File.Exists(directory + "\\Starfield.exe"))
            {
                MessageBox.Show("Can't continue without game installation path");
                return 0;
            }

            directory += @"\Data";

            /*            if (directory == "\\Data")
                        {
                            MessageBox.Show("Can't continue without game installation path");
                            return 0;
                        }*/

            foreach (var missingFile in Directory.EnumerateFiles(directory, "*.esm", SearchOption.TopDirectoryOnly))
            {
                esmespFiles.Add(missingFile[(missingFile.LastIndexOf('\\') + 1)..]);
            };

            foreach (var missingFile in Directory.EnumerateFiles(directory, "*.esp", SearchOption.TopDirectoryOnly))
            {
                esmespFiles.Add(missingFile[(missingFile.LastIndexOf('\\') + 1)..]);
            };

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
            }

            if (FilesToRemove.Count > 0)
            {
                for (i = 0; i < FilesToRemove.Count; i++)
                {
                    for (j = 0; j < dataGridView1.Rows.Count; j++)
                        if ((string)dataGridView1.Rows[j].Cells["PluginName"].Value == FilesToRemove[i])
                            dataGridView1.Rows.RemoveAt(j);
                }
            }
            if (RemovedFiles > 0)
                isModified = true;
            return RemovedFiles;
        }

        private string AddRemove()
        {
            int addedMods, removedMods;
            string ReturnStatus;
            addedMods = AddMissing();
            removedMods = RemoveMissing();
            if (addedMods != 0 || removedMods != 0)
            {
                SavePlugins();
                ReturnStatus = addedMods.ToString() + " Mods added, " + removedMods.ToString() + " Mods removed";
            }
            else
                ReturnStatus = "Plugins.txt is up to date";
            return ReturnStatus;
        }

        private void toolStripMenuAutoClean_Click(object sender, EventArgs e)
        {
            AddRemove();
        }

        private void frmLoadOrder_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isModified)
                SavePlugins();
            SaveSettings();
        }

        private static void SaveSettings()
        {
            Properties.Settings.Default.Save();
        }

        private void cmbProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            SwitchProfile(Properties.Settings.Default.ProfileFolder + "\\" + (string)cmbProfile.SelectedItem);
        }

        private void InstallMod()
        {
            string ModPath, fileName, destinationPath;
            string ExtractPath = Path.GetTempPath() + "hstTools\\";

            if (Directory.Exists(ExtractPath))
                Directory.Delete(ExtractPath, true);

            OpenFileDialog OpenMod = new()
            {
                //OpenMod.InitialDirectory = ProfileFolder;
                Filter = "Archive Files (*.zip;*.7z;*.rar)|*.zip;*.7z;*.rar|All Files (*.*)|*.*",
                Title = "Install Mod - Loose files not supported"
            };

            DialogResult result = OpenMod.ShowDialog();
            ModPath = OpenMod.FileName;

            if (OpenMod.FileName != "")
            {
                try
                {
                    sbar2("Installing mod...");
                    statusStrip1.Refresh();
                    using (ArchiveFile archiveFile = new ArchiveFile(OpenMod.FileName))
                    {
                        foreach (Entry entry in archiveFile.Entries)
                        {
                            entry.Extract(ExtractPath + entry.FileName);
                            sbar2("Extracting " + entry.FileName);
                            statusStrip1.Refresh();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
                foreach (string ModFile in Directory.EnumerateFiles(ExtractPath, "*.esm", SearchOption.AllDirectories)) // Move .esm files
                {
                    fileName = Path.GetFileName(ModFile);
                    destinationPath = Path.Combine(StarfieldGamePath + "\\Data", fileName);

                    if (File.Exists(destinationPath))
                    {
                        if (Tools.ConfirmAction("Overwrite esm " + destinationPath, "Replace mod?"))
                            File.Move(ModFile, destinationPath, true);  // Overwrite
                        else
                            break;
                    }
                    else
                        File.Move(ModFile, destinationPath, true);  // Overwrite
                }

                foreach (string ModFile in Directory.EnumerateFiles(ExtractPath, "*.ba2", SearchOption.AllDirectories)) // Move archives
                {
                    fileName = Path.GetFileName(ModFile);
                    destinationPath = Path.Combine(StarfieldGamePath + "\\Data", fileName);

                    if (File.Exists(destinationPath))
                    {
                        if (Tools.ConfirmAction("Overwrite archive " + destinationPath, "Replace mod?"))
                            File.Move(ModFile, destinationPath, true); // Overwrite
                        else
                            break;
                    }
                    else
                        File.Move(ModFile, destinationPath, true); // Overwrite
                }

                AddMissing();
                SavePlugins();
                if (Directory.Exists(ExtractPath)) // Clean up any left over files
                    Directory.Delete(ExtractPath, true);
                //sbar3("Mod installed");
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
            string tempstr, Group = "";
            List<string> ExportMods = [];
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
                Process.Start("explorer.exe", ExportActive.FileName);
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

            if (Tools.ConfirmAction(@"This will delete all files related to the '" + ModName + @"' mod", "Delete " + ModName + " - Are you sure?"))
            {
                isModified = true;
                dataGridView1.Rows.Remove(dataGridView1.CurrentRow);
                string directoryPath = StarfieldGamePath + "\\Data";

                ModFile = directoryPath + "\\" + ModName;
                if (File.Exists(ModFile + ".esp"))
                {
                    File.Delete(ModFile + ".esp");
                    SavePlugins();
                    sbar3("esp uninstalled - esm and archive files skipped");
                    return;
                }
                if (File.Exists(ModFile + ".esm"))
                    File.Delete(ModFile + ".esm");

                if (File.Exists(ModFile + " - textures.ba2"))
                    File.Delete(ModFile + " - textures.ba2");
                if (File.Exists(ModFile + " - main.ba2"))
                    File.Delete(ModFile + " - main.ba2");

                SavePlugins();
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
            SavePlugins();
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

        private void SavePlugins()
        {
            SaveLO(Tools.StarfieldAppData + @"\Plugins.txt");
            if (Profiles && cmbProfile.Text != "")
            {
                if (!Tools.FileCompare(Tools.StarfieldAppData + @"\Plugins.txt", Properties.Settings.Default.ProfileFolder + "\\" + cmbProfile.Text))
                {
                    SaveLO(Properties.Settings.Default.ProfileFolder + "\\" + cmbProfile.Text); // Save profile as well if updated
                    toolStripStatusSecondary.Text += ", " + cmbProfile.Text + " profile saved";
                }
            }
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            SavePlugins();
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
            Form SS = new frmSplashScreen();
            if (GameVersion != MS)
                SS.Show();
            bool result;

            if (isModified)
                SavePlugins();

            result = Tools.StartGame(GameVersion);

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
            string CreationsID = "", OtherURL = "", url;
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
                if (OtherURL != "")
                Tools.OpenUrl(OtherURL);
        }

        private void toolStripMenuUninstall_Click(object sender, EventArgs e)
        {
            UninstallMod();
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            if (isModified)
                SavePlugins();
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
            bool ProfilesActive = Profiles;
            int i, j;
            string LOOTPath = Properties.Settings.Default.LOOTPath, cmdLine;
            if (GameVersion != MS)
                cmdLine = "--game Starfield";
            else
                cmdLine = cmdLine = "--game \"Starfield (MS Store)\"";

            if (ProfilesActive)
            {
                Profiles = false;
                cmbProfile.Enabled = false;
                chkProfile.Checked = false;
            }

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
                    cmdLine += " --auto-sort";
                ProcessStartInfo startInfo = new()
                {
                    FileName = LOOTPath,
                    Arguments = "  " + cmdLine,
                    WorkingDirectory = LOOTPath[..LOOTPath.LastIndexOf("LOOT.exe")]
                };

                //sbar4("App paused waiting for LOOT exit");
                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                }
                //sbar4("Loot exit - review the load order if necessary");

                if (Properties.Settings.Default.AutoDelccc)
                    Delccc();
                InitDataGrid();

                for (i = 0; i < tools.BethFiles.Count; i++)  // Remove base game files
                    for (j = 0; j < dataGridView1.Rows.Count; j++)
                        if ((string)dataGridView1.Rows[j].Cells["PluginName"].Value == tools.BethFiles[i])
                            dataGridView1.Rows.RemoveAt(j);

                if (ProfilesActive)
                {
                    Profiles = true;
                    SavePlugins();
                    cmbProfile.Enabled = true;
                    chkProfile.Checked = true;

                }
                else
                    SavePlugins();
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

        private void sbar5(string StatusMessage)
        {
            toolStripStatusLabel5.Text = StatusMessage;
        }

        private void sbarCCC(string sbarMessage)
        {
            toolStripStatusDelCCC.Text = sbarMessage;
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
                    return false;
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show(ex.Message);
#endif
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
            Properties.Settings.Default.ProfileOn = toolStripMenuProfilesOn.Checked;
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
                isModified = true;
                SavePlugins();
            }
        }

        private void toolStripMenuCustom_Click(object sender, EventArgs e)
        {
            string CustomEXEFolder;

            CustomEXEFolder = Properties.Settings.Default.CustomEXE;

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
                GameVersion = Custom;
                UpdateGameVersion("Custom");
                toolStripMenuSteam.Checked = false;
                toolStripMenuMS.Checked = false;
                gameVersionSFSEToolStripMenuItem.Checked = false;

                Properties.Settings.Default.GameVersion = GameVersion;
                SaveSettings();
            }
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
                Process.Start("explorer.exe", Tools.DocumentationFolder + "\\Shortcuts.txt");
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
                case Steam:
                    sbar2("Game version - Steam");
                    break;
                case MS:
                    sbar2("Game version - MS");
                    break;
                case Custom:
                    sbar2("Game version - Custom - " + Properties.Settings.Default.CustomEXE);
                    break;
                case SFSE:
                    sbar2("Game version - SFSE");
                    break;
            }
        }

        private bool ResetStarfieldCustomINI(bool ConfirmOverwrite)  // true for confirmation
        {
            if (ConfirmOverwrite)
            {
                DialogResult DialogResult = MessageBox.Show("This will overwrite your StarfieldCustom.ini to a recommended version", "Are you sure?",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Stop);
                if (DialogResult != DialogResult.OK)
                    return false;
            }

            try
            {
                if (!Tools.FileCompare(Tools.CommonFolder + "\\StarfieldCustom.ini", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                    "\\My Games\\Starfield\\StarfieldCustom.ini")) // Check if StarfieldCustom.ini needs resetting
                {
                    File.Copy(Tools.CommonFolder + "\\StarfieldCustom.ini", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                        "\\My Games\\Starfield\\StarfieldCustom.ini", true);
                    sbar3("StarfieldCustom.ini restored");
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error restoring StarfieldCustom.ini");
                return false;
            }
        }
        private void toolStripMenuResetStarfieldCustom_Click(object sender, EventArgs e)
        {
            ResetStarfieldCustomINI(true);
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
                gameVersionSFSEToolStripMenuItem.Checked = false;
                GameVersion = Steam;
                UpdateGameVersion("Steam");
            }
        }

        private void toolStripMenuMS_Click(object sender, EventArgs e)
        {
            toolStripMenuMS.Checked = !toolStripMenuMS.Checked;
            if (toolStripMenuMS.Checked)
            {
                toolStripMenuSteam.Checked = false;
                toolStripMenuCustom.Checked = false;
                gameVersionSFSEToolStripMenuItem.Checked = false;
                GameVersion = MS;
                UpdateGameVersion("MS");
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

        private static int CheckAndDeleteINI(string FileName)
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

        private int UndoVortexChanges(bool ConfirmPrompt)  // true to confirm
        {
            int ChangeCount = 0;

            if (ConfirmPrompt)
            {
                DialogResult DialogResult = MessageBox.Show("Are you sure?", "This will remove all changes made by Vortex",
        MessageBoxButtons.OKCancel, MessageBoxIcon.Stop);
                if (DialogResult != DialogResult.OK)
                    return 0;
            }

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
            ChangeCount += CheckAndDeleteINI("Starfield.ini.base");
            LooseFiles = false;
            LooseFilesOnOff(false);
            LooseFilesMenuUpdate();
            if (Delccc())
                ChangeCount++;
            if (ChangeCount > 0)
                sbar3(ChangeCount + " Change(s) made to Vortex created files");
            return ChangeCount;
        }
        private void undoVortexChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UndoVortexChanges(true);
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            string ReturnStatus = AddRemove();
            //SavePlugins();
            sbar3(ReturnStatus);
        }

        private void LooseFilesOnOff(bool EnableDisable) // True for enabled
        {
            string LooseFilesDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\My Games\\Starfield\\",
                    filePath = LooseFilesDir + "StarfieldCustom.ini";

            if (EnableDisable)
            {
                List<string> linesToAppend = ["[Archive]", "bInvalidateOlderFiles=1"];
                File.AppendAllLines(filePath, linesToAppend);
                LooseFiles = true;
                sbarCCC("Loose Files Enabled");
            }
            else
            {
                if (File.Exists(filePath))
                {
                    var lines = File.ReadAllLines(filePath).ToList();
                    if (lines.Contains("bInvalidateOlderFiles=1"))
                    {
                        string[] linesToRemove = ["[Archive]", "bInvalidateOlderFiles=1", "sResourceDataDirsFinal="];

                        foreach (var lineToRemove in linesToRemove)
                        {
                            lines.RemoveAll(line => line.Trim() == lineToRemove);
                        }

                        // Write the updated lines back to the file
                        File.WriteAllLines(filePath, lines);
                        LooseFiles = false;
                        sbarCCC("Loose Files Disabled");
                    }
                }
            }
        }

        private void looseFilesDisabledToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LooseFiles = !LooseFiles;
            if (LooseFiles)
            {
                looseFilesDisabledToolStripMenuItem.Text = "Loose Files Enabled";
                LooseFilesOnOff(true);
            }
            else
            {
                looseFilesDisabledToolStripMenuItem.Text = "Loose Files Disabled";
                LooseFilesOnOff(false);
            }
            Properties.Settings.Default.LooseFiles = LooseFiles;
        }

        private void autoUpdateModsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AutoUpdate = !AutoUpdate;
            autoUpdateModsToolStripMenuItem.Checked = !autoUpdateModsToolStripMenuItem.Checked;
            Properties.Settings.Default.AutoUpdate = AutoUpdate;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshDataGrid();
        }

        private void btnLoot_Click(object sender, EventArgs e)
        {
            RunLOOT(true);
        }

        private void gameVersionSFSEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists(StarfieldGamePath + "\\sfse_loader.exe"))
            {
                gameVersionSFSEToolStripMenuItem.Checked = !gameVersionSFSEToolStripMenuItem.Checked;
                if (gameVersionSFSEToolStripMenuItem.Checked)
                {
                    toolStripMenuSteam.Checked = false;
                    toolStripMenuMS.Checked = false;
                    toolStripMenuCustom.Checked = false;

                    GameVersion = SFSE;
                    UpdateGameVersion("SFSE");
                }
            }
            else
            {
                MessageBox.Show("SFSE doesn't seem to be installed or Starfield path not set", "Unable to switch to SFSE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                GameVersion = Steam;
                toolStripMenuSteam.Checked = true;
                toolStripMenuMS.Checked = false;
                toolStripMenuCustom.Checked = false;
                gameVersionSFSEToolStripMenuItem.Checked = false;
            }
        }

        private void removeDuplicatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> Plugins = [];
            string PluginName;
            int ModCount = dataGridView1.RowCount;
            string loText = Tools.StarfieldAppData + "\\Plugins.txt";
            using (var reader = new StreamReader(loText))
            {
                while ((PluginName = reader.ReadLine()) != null) // Read Plugins.txt
                {
                    Plugins.Add(PluginName);
                }
            }
            List<string> distinctList = Plugins.Distinct().ToList();
            File.WriteAllLines(loText, distinctList);
            InitDataGrid();
            SavePlugins();
            sbar4("Duplicates removed: " + (ModCount - dataGridView1.RowCount).ToString());
        }

        private void ActiveOnlyToggle()
        {
            activeOnlyToolStripMenuItem.Checked = !activeOnlyToolStripMenuItem.Checked;
            Properties.Settings.Default.ActiveOnly = activeOnlyToolStripMenuItem.Checked;
            ActiveOnly = activeOnlyToolStripMenuItem.Checked;
            sbar4("Loading...");
            statusStrip1.Refresh();
            if (!ActiveOnly)
            {
                for (int i = 0; i < dataGridView1.RowCount; i++)
                    dataGridView1.Rows[i].Visible = true;
                sbar4("All mods shown");
            }
            else
            {
                for (int i = 0; i < dataGridView1.RowCount; i++)
                    if ((bool)dataGridView1.Rows[i].Cells["ModEnabled"].Value == false && dataGridView1.RowCount > 0)
                        dataGridView1.Rows[i].Visible = false;
                sbar4("Active mods only");
            }
        }
        private void activeOnlyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ActiveOnlyToggle();
        }

        private void LooseFilesMenuUpdate()
        {
            if (LooseFiles)
            {
                looseFilesDisabledToolStripMenuItem.Text = "Loose Files Enabled";
                LooseFiles = true;
                sbarCCC("Loose files enabled");
            }
            else
            {
                looseFilesDisabledToolStripMenuItem.Text = "Loose Files Disabled";
                LooseFiles = false;
                sbarCCC("Loose files disabled");
            }
            Properties.Settings.Default.LooseFiles = LooseFiles;
        }

        private void vortexPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Executable Files|*.exe";
            openFileDialog1.Title = "Set the path to the Vortex executable";
            openFileDialog1.FileName = "Vortex.exe";
            DialogResult VortexPath = openFileDialog1.ShowDialog();
            if (VortexPath == DialogResult.OK && openFileDialog1.FileName != "")
                Properties.Settings.Default.VortexPath = openFileDialog1.FileName;
        }

        private void uRLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            uRLToolStripMenuItem.Checked = !uRLToolStripMenuItem.Checked;
            if (uRLToolStripMenuItem.Checked)
                dataGridView1.Columns["URL"].Visible = true;
            else
                dataGridView1.Columns["URL"].Visible = false;
            Properties.Settings.Default.URL = uRLToolStripMenuItem.Checked;
        }

        private void vortexToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string keyName = @"HKEY_CLASSES_ROOT\nxm\shell\open\command";
            string valueName = ""; // Default value

            // Read the registry value
            object value = Registry.GetValue(keyName, valueName, null);
            if (value != null)
            {
                int startIndex = value.ToString().IndexOf('"') + 1;
                int endIndex = value.ToString().IndexOf('"', startIndex);

                if (startIndex > 0 && endIndex > startIndex)
                {
                    string extracted = value.ToString()[startIndex..endIndex];
                    try
                    {
                        var result = Process.Start(extracted);
                        if (result != null)
                        {
                            SaveSettings();
                            Application.Exit();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else
                    MessageBox.Show("Vortex doesn't seem to be installed.");
            }
            else
                MessageBox.Show("Vortex doesn't seem to be installed.");
        }

        private void autoSortToolStripMenuItem_Click(object sender, EventArgs e)
        {
            autoSortToolStripMenuItem.Checked = !autoSortToolStripMenuItem.Checked;
            Properties.Settings.Default.AutoSort = autoSortToolStripMenuItem.Checked;
            AutoSort = Properties.Settings.Default.AutoSort;
        }

        private void documentationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Tools.OpenUrl("Documentation\\Index.htm");
        }

        private void compareProfilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            compareProfilesToolStripMenuItem.Checked = !compareProfilesToolStripMenuItem.Checked;
            Properties.Settings.Default.CompareProfiles = compareProfilesToolStripMenuItem.Checked;
            SaveSettings();
        }

        private void lightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lightToolStripMenuItem.Checked = !lightToolStripMenuItem.Checked;
            Application.SetColorMode(SystemColorMode.Classic);
            Properties.Settings.Default.DarkMode = 0;
            darkToolStripMenuItem.Checked = false;
            systemToolStripMenuItem.Checked = false;
        }

        private void darkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            darkToolStripMenuItem.Checked = !darkToolStripMenuItem.Checked;
            dataGridView1.EnableHeadersVisualStyles = false;
            Application.SetColorMode(SystemColorMode.Dark);
            Properties.Settings.Default.DarkMode = 1;
            lightToolStripMenuItem.Checked = false;
            systemToolStripMenuItem.Checked = false;
        }

        private void systemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            systemToolStripMenuItem.Checked = !systemToolStripMenuItem.Checked;
            Application.SetColorMode(SystemColorMode.System);
            Properties.Settings.Default.DarkMode = 2;
            lightToolStripMenuItem.Checked = false;
            darkToolStripMenuItem.Checked = false;
        }

        private void btnActiveOnly_Click(object sender, EventArgs e)
        {
            ActiveOnlyToggle();
        }

        private void mO2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string MO2Path = Properties.Settings.Default.MO2Path;
            if (MO2Path != "")
            {
                try
                {
                    var result = Process.Start(MO2Path);
                    if (result != null)
                    {
                        SaveSettings();
                        Application.Exit();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
                MessageBox.Show("MO2 doesn't seem to be installed.");
        }

        private void modOrganizer2PathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Executable Files|*.exe";
            openFileDialog1.Title = "Set the path to the MO2 executable";
            openFileDialog1.FileName = "ModOrganizer.exe";
            DialogResult MO2Path = openFileDialog1.ShowDialog();
            if (MO2Path == DialogResult.OK && openFileDialog1.FileName != "")
                Properties.Settings.Default.MO2Path = openFileDialog1.FileName;

        }

        private void ResetDefaults()
        {
            string LooseFilesDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\My Games\\Starfield\\", // Check if loose files are enabled
        filePath = LooseFilesDir + "StarfieldCustom.ini";
            int ChangeCount = 0;

            if (File.Exists(filePath))
            {
                ChangeCount += UndoVortexChanges(false);

                var lines = File.ReadAllLines(filePath).ToList();
                if (lines.Contains("bInvalidateOlderFiles=1"))
                {
                    LooseFilesOnOff(false);
                    ChangeCount++;
                }

                if (Delccc())
                    ChangeCount++;

                if (ResetStarfieldCustomINI(false))
                    ChangeCount++;
                if (ChangeCount > 0)
                    sbar3(ChangeCount.ToString() + " Change(s) made to ini files");
            }
            sbar5("Auto Reset");
        }
        private void autoResetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!autoResetToolStripMenuItem.Checked)
            {
                DialogResult DialogResult = MessageBox.Show("This will run every time the app is started - Are you sure?", "This will reset settings made by other mod managers.",
                                MessageBoxButtons.OKCancel, MessageBoxIcon.Stop);
                if (DialogResult != DialogResult.OK)
                    return;
            }

            autoResetToolStripMenuItem.Checked = !autoResetToolStripMenuItem.Checked;
            if (autoResetToolStripMenuItem.Checked)
                ResetDefaults();
            else
                sbar5("");

            Properties.Settings.Default.AutoReset = autoResetToolStripMenuItem.Checked;
            SaveSettings();
        }

        private void creationKitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            const string userRoot = "HKEY_CURRENT_USER";
            const string subkey = @"Software\Valve\Steam";
            const string keyName = userRoot + "\\" + subkey;

            string executable = StarfieldGamePath;
            if (executable != null)
            {
                try
                {
                    SaveSettings();
                    string stringValue = (string)Registry.GetValue(keyName, "SteamExe", ""); // Get Steam path from Registry
                    var processInfo = new ProcessStartInfo(stringValue, "-applaunch 2722710");
                    var process = Process.Start(processInfo);
                    Application.Exit();
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Starfield path not set");
            }
        }

        private void resetToVanillaStarfieldSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Tools.ConfirmAction("Reset ini settings?", "Reset to vanilla defaults"))
                ResetDefaults();
        }

        private void ChangeSettings(bool NewSetting)
        {
            Properties.Settings.Default.ProfileOn = NewSetting;
            Profiles = NewSetting;
            chkProfile.Checked = NewSetting;
            Properties.Settings.Default.AutoSort = NewSetting;
            AutoSort = NewSetting;
            AutoUpdate = Properties.Settings.Default.AutoUpdate;
            Properties.Settings.Default.AutoUpdate = NewSetting;
            Properties.Settings.Default.AutoReset = NewSetting;
            Properties.Settings.Default.AutoDelccc = NewSetting;
            Properties.Settings.Default.CompareProfiles = NewSetting;

            SaveSettings();
            SetMenus();
        }
        private void enableAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Tools.ConfirmAction("This will turn on a number of auto settings and reset ini settings", "Reset to vanilla defaults?"))
            {
                ChangeSettings(true);
                ResetDefaults();
            }
        }

        private void disableAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeSettings(false);
            sbar5("");
        }

        private void uIToEditStarfieldCustominiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmStarfieldCustomINI fci = new();
            fci.Show();
        }

        private void UpdateGameVersion(string gameVersion)
        {
            Properties.Settings.Default.GameVersion = GameVersion;
            SaveSettings();
            if (GameVersion != MS)
                StarfieldGamePath = Properties.Settings.Default.StarfieldGamePath;
            else
                StarfieldGamePath = Properties.Settings.Default.GamePathMS;
            RefreshDataGrid();
            sbar2("Game version set to " + gameVersion);
        }

        private void toolStripMenuVersion_Click(object sender, EventArgs e)
        {
            toolStripMenuVersion.Checked = !toolStripMenuVersion.Checked;
            if (toolStripMenuVersion.Checked)
                dataGridView1.Columns["Version"].Visible = true;
            else
                dataGridView1.Columns["Version"].Visible = false;
            Properties.Settings.Default.Version = toolStripMenuVersion.Checked;
        }

        private void toolStripMenuAuthorVersion_Click(object sender, EventArgs e)
        {
            toolStripMenuAuthorVersion.Checked = !toolStripMenuAuthorVersion.Checked;
            dataGridView1.Columns["AuthorVersion"].Visible = toolStripMenuAuthorVersion.Checked ? true : false;
            Properties.Settings.Default.AuthorVersion = toolStripMenuAuthorVersion.Checked;
        }

        private void enableAchievementSafeOnlyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool ActiveOnlyStatus = ActiveOnly;
            DisableAll();
            if (ActiveOnly)
                ActiveOnlyToggle();

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if ((string)dataGridView1.Rows[i].Cells["Achievements"].Value == "Yes")
                    dataGridView1.Rows[i].Cells["ModEnabled"].Value = true;
            }

            if (ActiveOnlyStatus)
                ActiveOnlyToggle();
            sbar2("All achievement friendly mods enabled");
            isModified = true;
        }

        private void SetAchievement(bool OnOff)
        {
            string jsonFilePath = Tools.GetCatalog(), json = File.ReadAllText(jsonFilePath);
            var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Tools.Creation>>(json);

            data.Remove("ContentCatalog");

            foreach (var kvp in data)
            {
                int selectedIndex = dataGridView1.SelectedRows[0].Index;
                if (dataGridView1.Rows[selectedIndex].Cells["CreationsID"].Value.ToString() == kvp.Key.ToString())
                {
                    kvp.Value.AchievementSafe = OnOff;
                    dataGridView1.Rows[selectedIndex].Cells["Achievements"].Value = OnOff ? "Yes" : "";
                }
            }

            json = Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);

            // Hack the Bethesda header back in
            json = Tools.MakeHeader() + json[1..];

            File.WriteAllText(Tools.GetCatalog(), json); // Write updated catalog
            //RefreshDataGrid();
        }
        private void disableAchievementFlagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetAchievement(false);
        }

        private void enableAchievementFlagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetAchievement(true);
        }

        private void openAllActiveModWebPagesToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void timer2_Tick(object sender, EventArgs e)
        {

            DateTime now = DateTime.Now;
            string formattedTime = now.ToString("hh:mm tt");
            sbar5(formattedTime);
        }

        private void showTimeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            timer2.Enabled = !timer2.Enabled;
            showTimeToolStripMenuItem.Checked = timer2.Enabled;
            Properties.Settings.Default.Showtime = showTimeToolStripMenuItem.Checked;
            if (!timer2.Enabled)
                sbar5("");
        }
    }
}
