﻿using Microsoft.Win32;
using Narod.SteamGameFinder;
using SevenZipExtractor;
using Starfield_Tools.Common;
using Starfield_Tools.Load_Order_Editor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using YamlDotNet.Serialization;
using File = System.IO.File;

namespace Starfield_Tools
{

    public partial class frmLoadOrder : Form
    {
        public const byte Steam = 0, MS = 1, Custom = 2, SFSE = 3;
        public static string StarfieldGamePath;

        private Rectangle dragBoxFromMouseDown;
        private int rowIndexFromMouseDown, rowIndexOfItemUnderMouseToDrop, GameVersion = Steam;

        readonly Tools tools = new();

        private string LastProfile, tempstr;

        bool Profiles = false, GridSorted = false, AutoUpdate = false, ActiveOnly = false, AutoSort = false, isModified = false, LooseFiles;

        public frmLoadOrder(string parameter)
        {
            InitializeComponent();

#if DEBUG
            toolStripMenuProfiles.Visible = true;
#endif

            Tools.CheckGame(); // Exit if Starfield appdata folder not found

            string PluginsPath = Tools.StarfieldAppData + "\\Plugins.txt";

#pragma warning disable CS0168 // Variable is declared but never used
            try
            {
                string LooseFilesDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\My Games\\Starfield\\", // Check if loose files are enabled
filePath = LooseFilesDir + "StarfieldCustom.ini";
                var StarfieldCustomINI = File.ReadAllLines(filePath);
                foreach (var lines in StarfieldCustomINI)
                {
                    if (lines.Contains("bInvalidateOlderFiles"))
                    {
                        Properties.Settings.Default.LooseFiles = true;
                        Properties.Settings.Default.Save();
                        LooseFiles = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show(ex.Message, "Error opening StarfieldCustom.ini");
#endif
            }
#pragma warning restore CS0168 // Variable is declared but never used

            this.KeyUp += new System.Windows.Forms.KeyEventHandler(KeyEvent); // Handle <enter> for search

            progressBar1.Width = 400; // Set the width of the progress bar
            progressBar1.Height = 50; // Set the height of the progress bar

            // Calculate the position to center the progress bar
            int x = (this.ClientSize.Width - progressBar1.Width) / 2;
            int y = (this.ClientSize.Height - progressBar1.Height) / 2;

            // Set the location of the progress bar
            progressBar1.Location = new System.Drawing.Point(x, y);

            menuStrip1.Font = Properties.Settings.Default.FontSize; // Get settings
            this.Font = Properties.Settings.Default.FontSize;

            GameVersion = Properties.Settings.Default.GameVersion;

            if (GameVersion != MS)
            {
                StarfieldGamePath = Properties.Settings.Default.StarfieldGamePath;
                if (StarfieldGamePath == "")
                {
                    GetSteamGamePath(); // Detect game path
                    if (StarfieldGamePath == "")
                        StarfieldGamePath = tools.SetStarfieldGamePath();
                    Properties.Settings.Default.StarfieldGamePath = StarfieldGamePath;
                    Properties.Settings.Default.Save();
                }
            }
            else
            {
                if (Properties.Settings.Default.GamePathMS == "")
                    tools.SetStarfieldGamePathMS();
                StarfieldGamePath = Properties.Settings.Default.GamePathMS;
            }

            if (!File.Exists(StarfieldGamePath + "\\CreationKit.exe")) // Hide option to launch CK if not found
                creationKitToolStripMenuItem.Visible = false;

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

            // Detect other apps
            if (!File.Exists(StarfieldGamePath + "\\sfse_loader.exe"))
                gameVersionSFSEToolStripMenuItem.Visible = false;
            GameVersionDisplay();

            if (Properties.Settings.Default.MO2Path == "")
                mO2ToolStripMenuItem.Visible = false;

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

            // Setup other preferences

            switch (Properties.Settings.Default.DarkMode) // Theme
            {
                case 0: // Light
                    dataGridView1.EnableHeadersVisualStyles = true;
#pragma warning disable WFO5001 // 'System.Windows.Forms.Application.SetColorMode(System.Windows.Forms.SystemColorMode)' is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
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

            // Do a 1-time backup of Plugins.txt if it doesn't exist
            try
            {
                if (!File.Exists(PluginsPath + ".bak"))
                {
                    sbar2("Plugins.txt backed up to Plugins.txt.bak");
                    File.Copy(PluginsPath, PluginsPath + ".bak");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unable to find Plugins.txt");
            }

            // Do a 1-time backup of StarfieldCustom.ini if it doesn't exist
            tempstr = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                "\\My Games\\Starfield\\StarfieldCustom.ini";
            if (!File.Exists(tempstr + ".bak") && File.Exists(tempstr))
            {
                sbar2("StarfieldCustom.ini backed up to StarfieldCustom.ini.bak");
                File.Copy(tempstr, tempstr + ".bak");
            }

            frmStarfieldTools StarfieldTools = new(); // Check the catalog
            sbar4(StarfieldTools.CatalogStatus);
            if (StarfieldTools.CatalogStatus != null)
                if (StarfieldTools.CatalogStatus.Contains("Error"))
                    StarfieldTools.Show(); // Show catalog fixer if catalog broken

            cmbProfile.Enabled = Profiles;
            if (Profiles)
                GetProfiles();
            else
                InitDataGrid();
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

            if (LooseFiles || Properties.Settings.Default.LooseFiles)
            {
                looseFilesDisabledToolStripMenuItem.Checked = true;
                sbarCCC("Loose files enabled");
            }
            else
            {
                looseFilesDisabledToolStripMenuItem.Checked = false;
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

            activateNewModsToolStripMenuItem.Checked = Properties.Settings.Default.ActivateNew;

            if (Properties.Settings.Default.AutoReset)
                ResetDefaults();

            if (AutoUpdate)
            {
                int AddedMods = AddMissing(), RemovedMods = RemoveMissing();

                if (AddedMods + RemovedMods > 0)
                {
                    sbar4("Added: " + AddedMods + ", Removed: " + RemovedMods);
                    SavePlugins();
                    if (AutoSort)
                        RunLOOT(true);
                    InitDataGrid();
                }
            }

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
                "\\My Games\\Starfield\\StarfieldCustom.ini", Tools.CommonFolder + "StarfieldCustom.ini");
            return result;
        }
        private void InitDataGrid()
        {
            bool ModEnabled;
            int EnabledCount = 0, IndexCount = 1, i, esmCount = 0, espCount = 0, ba2Count, rowIndex;
            string loText, LOOTPath = Properties.Settings.Default.LOOTPath, PluginName, Description, ModFiles, ModVersion, AuthorVersion, ASafe, ModTimeStamp, ModID, URL,
                StatText = "", directory;
            List<string> CreationsPlugin = new();
            List<string> CreationsTitle = new();
            List<string> CreationsFiles = new();
            List<string> CreationsVersion = new();
            List<bool> AchievementSafe = new();
            List<long> TimeStamp = new();
            List<string> CreationsID = new();
            List<string> esmFiles = new();
            List<long> FileSize = new();
            long ModFileSize;
            DateTime start = new(1970, 1, 1, 0, 0, 0, 0);

            if (!File.Exists(Tools.GetCatalogPath()))
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

            string jsonFilePath = Tools.GetCatalogPath();
            string json = File.ReadAllText(jsonFilePath); // Read catalog file

            Tools.Configuration Groups = new();
            Tools.Configuration Url = new();

            if (toolStripMenuGroup.Checked && LOOTPath != "" && dataGridView1.Columns["Group"].Visible) // Read LOOT groups
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
                    MessageBox.Show(ex.Message, "Yaml decoding error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
#endif
                    sbar(ex.Message);
                }
            }

            try
            {
                var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Tools.Creation>>(json); // Read ContentCatalog.txt
                data.Remove("ContentCatalog");
                foreach (var kvp in data)
                {
                    try
                    {
                        foreach (var file in kvp.Value.Files)
                        {
                            if (file.EndsWith(".esm") || file.EndsWith(".esp")) // Look for .esm or .esp files
                                CreationsPlugin.Add(file);
                        }

                        CreationsTitle.Add(kvp.Value.Title);
                        CreationsVersion.Add(kvp.Value.Version);
                        CreationsFiles.Add(string.Join(", ", kvp.Value.Files));
                        AchievementSafe.Add(kvp.Value.AchievementSafe);
                        TimeStamp.Add(kvp.Value.Timestamp);
                        CreationsID.Add(kvp.Key);
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
                File.WriteAllText(Tools.GetCatalogPath(), json);
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

            var lines = File.ReadAllLines(loText);
            progressBar1.Maximum = lines.Length;
            progressBar1.Value = 0;
            progressBar1.Show();

            foreach (var line in lines) // Read Plugins.txt
            {
                progressBar1.Value++;
                PluginName = line;
                try
                {
                    if (!string.IsNullOrEmpty(PluginName) && !tools.BethFiles.Contains(PluginName))
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

                            for (i = 0; i < CreationsPlugin.Count; i++) // Parallel.For (0,  CreationsPlugin.Count, i=> 
                            {
                                if (CreationsPlugin[i][..CreationsPlugin[i].LastIndexOf('.')] + ".esm" == PluginName ||
                                    CreationsPlugin[i][..CreationsPlugin[i].LastIndexOf('.')] + ".esp" == PluginName)
                                {
                                    Description = CreationsTitle[i]; // Add Content Catalog description if available
                                    ModVersion = CreationsVersion[i];
                                    AuthorVersion = ModVersion[(ModVersion.IndexOf('.') + 1)..];
                                    ModVersion = start.AddSeconds(double.Parse(ModVersion[..ModVersion.IndexOf('.')])).Date.ToString("yyyy-MM-dd");

                                    ModFiles = CreationsFiles[i];
                                    ASafe = AchievementSafe[i] ? "Yes" : "";
                                    ModTimeStamp = Tools.ConvertTime(TimeStamp[i]).ToString();
                                    ModID = CreationsID[i];
                                    ModFileSize = FileSize[i] / 1024;
                                    URL = "https://creations.bethesda.net/en/starfield/details/" + ModID[3..];
                                    break;
                                }
                            }

                            rowIndex = this.dataGridView1.Rows.Add();
                            var row = this.dataGridView1.Rows[rowIndex];

                            // Populate datagrid from LOOT groups

                            if (LOOTPath != "" && Groups.groups != null && dataGridView1.Columns["Group"].Visible)
                            {
                                var group = Groups.plugins.FirstOrDefault(p => p.name == PluginName);
                                if (group != null)
                                {
                                    row.Cells["Group"].Value = group.group;
                                    if (group.url != null)
                                    {
                                        URL = group.url[0].link;
                                        Description = group.url[0].name;
                                    }
                                }
                            }

                            if (PluginName.StartsWith("sfbgs")) // Assume Bethesda plugin
                            {
                                if (row.Cells["Group"].Value == null)
                                    row.Cells["Group"].Value = "Bethesda Game Studios Creations";
                                else
                                    row.Cells["Group"].Value += " (Bethesda)";
                            }

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

            SetupColumns();

            // Get mod stats
            if (StarfieldGamePath != "")
            {
#pragma warning disable CS0168 // Variable is declared but never used
                try
                {
                    directory = StarfieldGamePath + @"\Data";

                    esmCount = Directory.EnumerateFiles(directory, "*.esm", SearchOption.TopDirectoryOnly).Count();
                    espCount = Directory.EnumerateFiles(directory, "*.esp", SearchOption.TopDirectoryOnly).Count();
                    ba2Count = Directory.EnumerateFiles(directory, "*.ba2", SearchOption.TopDirectoryOnly).Count();

                    StatText = "Total Mods: " + dataGridView1.RowCount + ", Creations Mods: " + CreationsPlugin.Count + ", Other: " +
                        (dataGridView1.RowCount - CreationsPlugin.Count) + ", Enabled: " + EnabledCount + ", esm files: " +
                        esmCount + " " + "Archives: " + ba2Count;

                    if (espCount > 0)
                        StatText += ", esp files: " + espCount;

                    if (dataGridView1.RowCount - CreationsPlugin.Count < 0)
                        sbar4("Catalog/Plugins mismatch - Run game to solve");
                }
                catch (Exception ex)
                {
                    sbar("Starfield path needs to be set for mod stats");
#if DEBUG
                    MessageBox.Show(ex.Message);
#endif
                }
#pragma warning restore CS0168 // Variable is declared but never used
            }
            else
                sbar("Starfield path needs to be set for mod stats");

            if (ActiveOnly)
            {
                sbar("Hiding inactive mods...");
                statusStrip1.Refresh();
                for (i = 0; i < dataGridView1.RowCount; i++)
                    if (!(bool)dataGridView1.Rows[i].Cells["ModEnabled"].Value && dataGridView1.RowCount > 0)
                        dataGridView1.Rows[i].Visible = false;
            }
            sbar(StatText);
            dataGridView1.EndEdit();

            progressBar1.Value = progressBar1.Maximum; // Ensure the progress bar is full at the end
            progressBar1.Hide();
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
            sbar3("Warning! - Plugins sorted - saving changes disabled - Refresh to enable saving");
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
            SavePlugins();
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
            SavePlugins();
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
            if (Tools.ConfirmAction("This will reset your current load order", "Enable all mods?"))
                EnableAll();
        }

        private void toolStripMenuDisableAll_Click(object sender, EventArgs e)
        {
            if (Tools.ConfirmAction("This will reset your current load order", "Disable all mods?"))
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
                SwitchProfile(SavePlugins.FileName);
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
                                           .Where(s => s.StartsWith('*'))
                                           .Select(s => $"New Profile added: {s.Replace("*", string.Empty).Replace("#", string.Empty)}")
                                           .Concat(currentProfile.Except(newProfile)
                                           .Where(s => s.StartsWith('*'))
                                           .Select(s => $"Previous Profile removed: {s.Replace("*", string.Empty).Replace("#", string.Empty)}"))
                                           .ToList();

                if (Difference.Count > 0)
                {
                    Form fpc = new frmProfileCompare(Difference);
                    fpc.Show();
                }
            }

#pragma warning disable CS0168 // Variable is declared but never used
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
#pragma warning restore CS0168 // Variable is declared but never used
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
            if (GameVersion != MS)
                StarfieldGamePath = tools.SetStarfieldGamePath();
            else
                StarfieldGamePath = tools.SetStarfieldGamePathMS();
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

            if (!CheckGamePath())
                return 0;

            directory += @"\Data";
            if (directory == @"\Data")
                return 0;

            esmespFiles = tools.GetPluginList(); // Add esm files

            try
            {
                foreach (var missingFile in Directory.EnumerateFiles(directory, "*.esp", SearchOption.TopDirectoryOnly)) // Add esp files
                {
                    esmespFiles.Add(missingFile[(missingFile.LastIndexOf('\\') + 1)..]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
                    rowIndex = this.dataGridView1.Rows.Add();
                    var row = this.dataGridView1.Rows[rowIndex];
                    if (FilesToAdd[i].Contains(".esm") && FilesToAdd[i] != null && Properties.Settings.Default.ActivateNew) // Activate the mod if the option is set in menu
                        row.Cells["ModEnabled"].Value = true;
                    else
                        row.Cells["ModEnabled"].Value = false;
                    row.Cells["PluginName"].Value = FilesToAdd[i];
                }
                dataGridView1.CurrentCell = dataGridView1.Rows[dataGridView1.RowCount - 1].Cells["PluginName"];
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

            if (!CheckGamePath())
                return 0;

            directory += @"\Data";

            esmespFiles = tools.GetPluginList(); // Add esm files

            try
            {
                foreach (var missingFile in Directory.EnumerateFiles(directory, "*.esp", SearchOption.TopDirectoryOnly)) // Add esp files
                {
                    esmespFiles.Add(missingFile[(missingFile.LastIndexOf('\\') + 1)..]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

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
            Properties.Settings.Default.WindowLocation = this.Location;
            Properties.Settings.Default.WindowSize = this.Size;

            if (isModified)
                SavePlugins();
            SaveSettings();
            /*Properties.Settings.Default.FormWidth = this.Width;
            Properties.Settings.Default.FormHeight = this.Height;
            Properties.Settings.Default.Save(); // Save settings*/
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

            if (!CheckGamePath())
                return;

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
                Form LoadScreen = new frmLoading("Extracting mod...");
                LoadScreen.Show();
                try
                {
                    sbar2("Installing mod...");
                    statusStrip1.Refresh();
                    using (ArchiveFile archiveFile = new(OpenMod.FileName))
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
                    LoadScreen.Close();
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
                        if (Tools.ConfirmAction("Overwrite archive " + destinationPath, "Replace archive?"))
                            File.Move(ModFile, destinationPath, true); // Overwrite
                        else
                            break;
                    }
                    else
                        File.Move(ModFile, destinationPath, true); // Overwrite
                }
                LoadScreen.Close();

                AddMissing();
                SavePlugins();
                if (AutoSort)
                    RunLOOT(true);
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
                Title = "Export Active Plugins",
                FileName = "Plugins.txt"
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

            if (!CheckGamePath())
                return;

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
            bool result;
            Properties.Settings.Default.GameVersion = GameVersion;
            SaveSettings();
            Form SS = new frmSplashScreen();
            if (GameVersion != MS)
                SS.Show();

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

        private void toolStripMenuViewOnCreations_Click(object sender, EventArgs e)
        {
            string url;

            url = (string)dataGridView1.CurrentRow.Cells["URL"].Value;
            if (url != "")
                Tools.OpenUrl(url);

            if (url == "")
                sbar3("No link for mod");
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

            if (File.Exists(@"C:\Program Files\LOOT\LOOT.exe") && LOOTPath == "") // Try detect LOOT if installed in default location
            {
                LOOTPath = @"C:\Program Files\LOOT\LOOT.exe";
                Properties.Settings.Default.LOOTPath = LOOTPath;
                Properties.Settings.Default.Save();
            }

            if (LOOTPath == "") // LOOT not found. Prompt for path
            {
                if (!SetLOOTPath())
                {
                    sbar2("LOOT path is required to run LOOT");
                    return;
                }
                else
                    LOOTPath = Properties.Settings.Default.LOOTPath;
            }

            if (GameVersion != MS)
                cmdLine = "--game Starfield";
            else
                cmdLine = cmdLine = "--game \"Starfield (MS Store)\"";

            if (ProfilesActive) // Temporary disable of profiles
            {
                Profiles = false;
                cmbProfile.Enabled = false;
                chkProfile.Checked = false;
            }

            if (LOOTMode)
                cmdLine += " --auto-sort";
            ProcessStartInfo startInfo = new()
            {
                FileName = LOOTPath,
                Arguments = "  " + cmdLine,
                WorkingDirectory = LOOTPath[..LOOTPath.LastIndexOf("LOOT.exe")]
            };

            using (Process process = Process.Start(startInfo)) // Freeze this app until LOOT closes
            {
                process.WaitForExit();
            }

            if (Properties.Settings.Default.AutoDelccc)
                Delccc();
            InitDataGrid();

            for (i = 0; i < tools.BethFiles.Count; i++)  // Remove base game files if LOOT added them
                for (j = 0; j < dataGridView1.Rows.Count; j++)
                    if ((string)dataGridView1.Rows[j].Cells["PluginName"].Value == tools.BethFiles[i])
                        dataGridView1.Rows.RemoveAt(j);

            if (ProfilesActive) // Re-enable profiles
            {
                Profiles = true;
                SavePlugins();
                cmbProfile.Enabled = true;
                chkProfile.Checked = true;

            }
            else
                SavePlugins();
        }
        private void toolStripMenuLoot_Click(object sender, EventArgs e)
        {
            RunLOOT(true);
        }

        private bool SetLOOTPath()
        {
            openFileDialog1.InitialDirectory = Properties.Settings.Default.LOOTPath;
            openFileDialog1.Filter = "Executable Files|*.exe";
            openFileDialog1.Title = "Set the path to the LOOT executable";
            openFileDialog1.FileName = "LOOT.exe";
            DialogResult LOOTPath = openFileDialog1.ShowDialog();
            if (LOOTPath == DialogResult.OK)
            {
                if (openFileDialog1.FileName != "")
                {
                    Properties.Settings.Default.LOOTPath = openFileDialog1.FileName;
                    Properties.Settings.Default.Save();
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
            if (!GameSwitchWarning())
                return;

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
                if (!Tools.FileCompare(Tools.CommonFolder + "StarfieldCustom.ini", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                    "\\My Games\\Starfield\\StarfieldCustom.ini")) // Check if StarfieldCustom.ini needs resetting
                {
                    File.Copy(Tools.CommonFolder + "StarfieldCustom.ini", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
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
            string pathToFile = Tools.GetCatalogPath();
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
        private bool GameSwitchWarning()
        {
            return (Tools.ConfirmAction("Do you want to proceed?", "Switching to a no mods profile is suggested before proceeding"));
        }
        private void toolStripMenuSteam_Click(object sender, EventArgs e)
        {
            if (!GameSwitchWarning())
                return;
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
            if (!GameSwitchWarning())
                return;
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
            //int Duplicates = RemoveDuplicates();

            //if (AutoSort && (ReturnStatus != "Plugins.txt is up to date" || Duplicates > 0))
            if (AutoSort && ReturnStatus != "Plugins.txt is up to date" )
                RunLOOT(true);

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
                looseFilesDisabledToolStripMenuItem.Checked = true;
                LooseFilesOnOff(true);
            }
            else
            {
                looseFilesDisabledToolStripMenuItem.Checked = false;
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
            if (!GameSwitchWarning())
                return;

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
        private int RemoveDuplicates()
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
            isModified = true;
            SavePlugins();
            sbar4("Duplicates removed: " + (ModCount - dataGridView1.RowCount).ToString());
            return (ModCount - dataGridView1.RowCount);

        }
        private void removeDuplicatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RemoveDuplicates();
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
                looseFilesDisabledToolStripMenuItem.Checked = true;
                LooseFiles = true;
                sbarCCC("Loose files enabled");
            }
            else
            {
                looseFilesDisabledToolStripMenuItem.Checked = false;
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
                MessageBox.Show("MO2 doesn't seem to be installed or path not configured.");
        }

        private void modOrganizer2PathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Executable Files|*.exe";
            openFileDialog1.Title = "Set the path to the MO2 executable";
            openFileDialog1.FileName = "ModOrganizer.exe";
            DialogResult MO2Path = openFileDialog1.ShowDialog();
            if (MO2Path == DialogResult.OK && openFileDialog1.FileName != "")
            {
                Properties.Settings.Default.MO2Path = openFileDialog1.FileName;
                mO2ToolStripMenuItem.Visible = true;
            }

        }

        private void ResetDefaults()
        {
            string LooseFilesDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\My Games\\Starfield\\", // Check if loose files are enabled
        filePath = LooseFilesDir + "StarfieldCustom.ini";
            int ChangeCount = 0;

            if (File.Exists(filePath)) // Disable loose files
            {
                ChangeCount += UndoVortexChanges(false);

                var lines = File.ReadAllLines(filePath).ToList();
                if (lines.Contains("bInvalidateOlderFiles=1"))
                {
                    LooseFilesOnOff(false);
                    ChangeCount++;
                }

                if (Delccc()) // Delete Starfield.ccc
                    ChangeCount++;

                if (ResetStarfieldCustomINI(false)) // Apply recommended settings
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
            if (Tools.ConfirmAction("Reset ini settings?", "Reset to recommended settings"))
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
            Properties.Settings.Default.ActivateNew = NewSetting;

            SaveSettings();
            SetMenus();
        }
        private void enableAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Tools.ConfirmAction("This will turn on a number of auto settings and reset ini settings", "Reset to recommended settings?"))
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
            fci.ShowDialog();
            string PluginsPath = Tools.StarfieldAppData + "\\Plugins.txt",
LooseFilesDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\My Games\\Starfield\\", // Check if loose files are enabled
filePath = LooseFilesDir + "StarfieldCustom.ini";
            LooseFiles = false;
            try
            {
                var StarfieldCustomINI = File.ReadAllLines(filePath);
                foreach (var lines in StarfieldCustomINI)
                {
                    if (lines.Contains("bInvalidateOlderFiles"))
                    {
                        Properties.Settings.Default.LooseFiles = true;
                        Properties.Settings.Default.Save();
                        LooseFiles = true;
                        break;
                    }
                }
            }
            catch
            {

            }
            if (LooseFiles)
                sbarCCC("Loose Files Enabled");
            else
                sbarCCC("Loose Files Disabled");
            looseFilesDisabledToolStripMenuItem.Checked = LooseFiles;
        }

        private void UpdateGameVersion(string gameVersion) // Display game version
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

        private void toolStripMenuVersion_Click(object sender, EventArgs e) // View version column
        {
            toolStripMenuVersion.Checked = !toolStripMenuVersion.Checked;
            if (toolStripMenuVersion.Checked)
                dataGridView1.Columns["Version"].Visible = true;
            else
                dataGridView1.Columns["Version"].Visible = false;
            Properties.Settings.Default.Version = toolStripMenuVersion.Checked;
        }

        private void toolStripMenuAuthorVersion_Click(object sender, EventArgs e) // View author column
        {
            toolStripMenuAuthorVersion.Checked = !toolStripMenuAuthorVersion.Checked;
            dataGridView1.Columns["AuthorVersion"].Visible = toolStripMenuAuthorVersion.Checked;
            Properties.Settings.Default.AuthorVersion = toolStripMenuAuthorVersion.Checked;
        }

        private void enableAchievementSafeOnlyToolStripMenuItem_Click(object sender, EventArgs e) // Experimental. Should probably remove
        {
            if (!Tools.ConfirmAction("Do you want to continue", "Warning - this will alter your current load order to achievement friendly mods only"))
                return;
            if (dataGridView1.Columns["Achievements"].Visible == false)
            {
                Properties.Settings.Default.Achievements = true;
                SaveSettings();
                SetupColumns();
            }
            RefreshDataGrid();
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
            SavePlugins();
        }

        private void SetAchievement(bool OnOff) // Experimental. Should probably remove
        {
            string jsonFilePath = Tools.GetCatalogPath(), json = File.ReadAllText(jsonFilePath);
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

            File.WriteAllText(Tools.GetCatalogPath(), json); // Write updated catalog
            //RefreshDataGrid();
        }
        private void disableAchievementFlagToolStripMenuItem_Click(object sender, EventArgs e) // Experimental. Should probably remove
        {
            SetAchievement(false);
        }

        private void enableAchievementFlagToolStripMenuItem_Click(object sender, EventArgs e) // Experimental. Should probably remove
        {
            SetAchievement(true);
        }

        private void openAllActiveModWebPagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int i;
            string url;

            if (Tools.ConfirmAction("Are you sure you want to open all mod web pages?", "This might take a while and a lot of memory"))
            {
                for (i = 0; i < dataGridView1.RowCount; i++)
                {
                    url = (string)dataGridView1.Rows[i].Cells["URL"].Value;
                    if ((bool)dataGridView1.Rows[i].Cells["ModEnabled"].Value == true && url != "")
                        Tools.OpenUrl(url);
                }
            }
        }

        private void timer2_Tick(object sender, EventArgs e) // Used for date / time display, ticks once per second.
        {
            DateTime now = DateTime.Now;
            sbar5(now.ToString("ddd, d MMM yyyy - hh:mm tt", CultureInfo.CurrentCulture.DateTimeFormat));
        }

        private void showTimeToolStripMenuItem_Click_1(object sender, EventArgs e) // Display date and time
        {
            timer2.Enabled = !timer2.Enabled;
            showTimeToolStripMenuItem.Checked = timer2.Enabled;
            Properties.Settings.Default.Showtime = showTimeToolStripMenuItem.Checked;
            if (!timer2.Enabled)
                sbar5("");
        }

        static void CreateZipFromFiles(List<string> files, string zipPath)
        {
            using (FileStream zipToOpen = new FileStream(zipPath, FileMode.Create))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                {
                    foreach (string file in files)
                    {
                        ZipArchiveEntry entry = archive.CreateEntryFromFile(file, Path.GetFileName(file));
                    }
                }
            }
        }

        private bool CheckGamePath() // Check if game path is set
        {
            if (StarfieldGamePath == "" || StarfieldGamePath == null)
                StarfieldGamePath = tools.SetStarfieldGamePath(); // Prompt user to set game path if not set
            if (StarfieldGamePath == "")
            {
                MessageBox.Show("Unable to continue without Starfield game path");
                return false;
            }
            else
                return true;
        }
        private void archiveModToolStripMenuItem_Click_1(object sender, EventArgs e) // Make a zip of a mod and copy it to specified folder
        {
            string ModName, ModFile;
            List<string> files = [];

            if (!CheckGamePath()) // Abort if game path not set
                return;

            ModName = (string)dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells["PluginName"].Value;
            ModName = ModName[..ModName.IndexOf('.')]; // Get current mod name

            string directoryPath = StarfieldGamePath + "\\Data";

            using FolderBrowserDialog folderBrowserDialog = new();

            folderBrowserDialog.Description = "Choose folder to archive the mod to";
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedFolderPath = folderBrowserDialog.SelectedPath;

                Form LoadScreen = new frmLoading("Archiving mod..."); // Show popup while archive process runs
                LoadScreen.Show();

                ModFile = directoryPath + "\\" + ModName; // Add esp, esm and archives to files list
                if (File.Exists(ModFile + ".esp"))
                    files.Add(ModFile + ".esp");

                if (File.Exists(ModFile + ".esm"))
                    files.Add(ModFile + ".esm");

                if (File.Exists(ModFile + " - textures.ba2"))
                    files.Add(ModFile + " - textures.ba2");

                if (File.Exists(ModFile + " - main.ba2"))
                    files.Add(ModFile + " - main.ba2");

                string zipPath = folderBrowserDialog.SelectedPath + "\\" + Path.GetFileName(ModFile) + ".zip"; // Choose pat to Zip it

                // Check if archive already exists, bail out on user cancel
                if (File.Exists(zipPath) && !Tools.ConfirmAction("Overwrite archive?", "Archive exists - " + Path.GetFileName(ModFile)))
                {
                    sbar3("Archive not created");
                    LoadScreen.Close();
                    return;
                }
                sbar3("Creating archive...");
                statusStrip1.Refresh();
                CreateZipFromFiles(files, zipPath); // Make zip
                sbar3(Path.GetFileName(ModFile) + " archived");
                LoadScreen.Close();
            }
        }

        private void enabledToolStripMenuItem_Click(object sender, EventArgs e) // WIP
        {

        }

        private void manageToolStripMenuItem_Click(object sender, EventArgs e) // WIP
        {
            Form ProfilesForm = new frmProfiles(cmbProfile.SelectedItem.ToString());
            ProfilesForm.Show();
        }

        private void checkArchivesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> BGSArchives = [];
            List<string> archives = [];
            List<string> plugins = [];
            List<string> orphaned = [];
            List<string> toDelete = [];

            if (StarfieldGamePath == "")
                return;

            using (StreamReader sr = new StreamReader(Tools.CommonFolder + "BGS Archives.txt")) // Read a list of standard game archives. Will need updating for future DLC
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    BGSArchives.Add(line);
                }
            }

            plugins = tools.GetPluginList() // Build a list of all plugins excluding base game files
    .Select(s => s.Substring(0, s.Length - 4).ToLower())
    .ToList();

            foreach (string file in Directory.EnumerateFiles(StarfieldGamePath + "\\Data", "*.ba2", SearchOption.TopDirectoryOnly)) // Build a list of all archives
            {
                tempstr = Path.GetFileName(file).ToLower();
                archives.Add(tempstr[..tempstr.LastIndexOf('.')]);
            }

            List<string> modArchives = archives.Except(BGSArchives) // Get the archive base names excluding BGS Archives
                .Select(s => s.ToLower()
                .Replace(" - main", string.Empty)
                .Replace(" - textures", string.Empty)
                .Replace(" - voices_en", string.Empty))
                .ToList();

            orphaned = modArchives.Except(plugins).ToList(); // Strip out esm files to get orphaned archives

            var suffixes = new List<string> { " - main.ba2", " - textures.ba2", " - voices_en.ba2", ".ba2" }; // Build a list of archives to delete with full path

            foreach (var item in orphaned)
            {
                tempstr = Path.Combine(StarfieldGamePath, "Data", item);

                foreach (var suffix in suffixes)
                {
                    var filePath = tempstr + suffix;
                    if (File.Exists(filePath))
                        toDelete.Add(filePath);
                }
            }

            if (toDelete.Count > 0)
            {
                Form Orphaned = new frmOrphaned(toDelete);
                Orphaned.Show();
            }
            else
                MessageBox.Show("No orphaned archives found");

        }

        private void activateNewModsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            activateNewModsToolStripMenuItem.Checked = !activateNewModsToolStripMenuItem.Checked;
            Properties.Settings.Default.ActivateNew = activateNewModsToolStripMenuItem.Checked;
        }

        private void GetSteamGamePath()
        {
            SteamGameLocator steamGameLocator = new SteamGameLocator();
            StarfieldGamePath = steamGameLocator.getGameInfoByFolder("Starfield").steamGameLocation;
        }

        private void toolStripMenuItemDeletePlugins_Click(object sender, EventArgs e)
        {
            if (!Tools.ConfirmAction("Are you sure you want to delete Plugins.txt?", "This will delete Plugins.txt and turn off most app settings"))
                return;
            ChangeSettings(false);
            File.Delete(Tools.StarfieldAppData + "\\Plugins.txt");
        }

        private void toolStripMenuAddToProfile_Click(object sender, EventArgs e)
        {
            List<string> profiles = new();

            foreach (var item in cmbProfile.Items)
            {
                profiles.Add(item.ToString());
            }
            profiles.Remove(cmbProfile.SelectedItem.ToString()); // Remove current profile from list

            frmAddModToProfile addMod = new(profiles, dataGridView1.CurrentRow.Cells["PluginName"].Value.ToString());
            addMod.Show(cmbProfile);
        }

        private void ResetWindowSize()
        {
            Rectangle resolution = Screen.PrimaryScreen.Bounds; // Resize window to 75% of screen width
            double screenWidth = resolution.Width;
            double screenHeight = resolution.Height;
            this.Width = (int)(screenWidth * 0.85);
            this.Height = (int)(screenHeight * 0.85);
            this.StartPosition = FormStartPosition.CenterScreen;
        }
        private void frmLoadOrder_Load(object sender, EventArgs e)
        {
            this.Location = Properties.Settings.Default.WindowLocation;
            this.Size = Properties.Settings.Default.WindowSize;
            if (this.Width < 500 || this.Height < 100)
                ResetWindowSize();
        }

        private void toolStripMenuResetWindow_Click(object sender, EventArgs e)
        {
            ResetWindowSize();
        }
    }
}
