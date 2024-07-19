using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;


namespace Starfield_Tools
{

    public partial class frmStarfieldTools : Form
    {

        private bool AutoCheck, AutoClean, AutoBackup, AutoRestore, ForceClean;

        ContentCatalog CC = new ContentCatalog();

        public frmStarfieldTools()
        {
            InitializeComponent();

            //#if DEBUG
            cmdDeleteStale.Enabled = true;
            cmdDeleteStale.Visible = true;
            //#endif

            // Retrieve settings
            AutoCheck = Properties.Settings.Default.AutoCheck;
            AutoClean = Properties.Settings.Default.AutoClean;
            AutoBackup = Properties.Settings.Default.AutoBackup;
            AutoRestore = Properties.Settings.Default.AutoRestore;
            CC.StarFieldPath = Properties.Settings.Default.StarfieldPath;
            ForceClean = Properties.Settings.Default.ForceClean;

            // Initialise Checkboxes
            chkAutoCheck.Checked = AutoCheck;
            chkAutoClean.Checked = AutoClean;
            chkAutoBackup.Checked = AutoBackup;
            chkAutoRestore.Checked = AutoRestore;
            chkForceClean.Checked = ForceClean;

            richTextBox2.Text = "";

            if (AutoCheck) // Check catalog status if enabled
            {
                if (!CheckCatalog()) // If not okay, then...
                {
                    if (AutoRestore) // Restore backup file if auto restore is on
                        RestoreCatalog();
                    else
                    if (AutoClean)
                        CleanCatalog();
                }
                else
                    toolStripStatusLabel1.Text = "Checks complete";
            }
            else toolStripStatusLabel1.Text = "Ready";

            if (AutoBackup)
                if (!CheckBackup()) // Backup if necessary
                    BackupCatalog();
            /*if (ForceClean)
                CleanCatalog();*/

            DisplayCatalog();
            richTextBox2.Text = "";
            Console.WriteLine("Int32 Min " + Int32.MinValue.ToString());
            Console.WriteLine("Int32 Max " + Int32.MaxValue.ToString());
            Console.WriteLine("Int64 Min " + Int64.MinValue.ToString());
            Console.WriteLine("Int64 Max " + Int64.MaxValue.ToString());
        }

        private bool CheckBackup()
        {
            string fileName1 = CC.GetCatalog();
            string fileName2 = fileName1 + ".bak";

            DateTime lastWriteTime1 = File.GetLastWriteTime(fileName1);
            DateTime lastWriteTime2 = File.GetLastWriteTime(fileName2);

            if (lastWriteTime1 == lastWriteTime2)
            {
                richTextBox2.Text += "Backup is up to date.\n";
                return true;
            }
            else
            {
                richTextBox2.Text += "Backup is out of date.\n";
                return false;
            }
        }

        private void DisplayCatalog()
        {
            try
            {
                richTextBox1.Text = File.ReadAllText(CC.GetCatalog());
            }
            catch { }
        }


        private bool CheckCatalog() // returns true if catalog good
        {
            toolStripStatusLabel1.Text = "Checking...";
            richTextBox1.Text = "";
            bool ErrorFound = false;
            int ErrorCount = 0;
            richTextBox2.Text += "Checking Catalog\n";
            double VersionCheck;
            long TimeStamp;
            richTextBox2.Text = "";

            try
            {
                string jsonFilePath = CC.GetCatalog();
                if (jsonFilePath == null)
                {
                    toolStripStatusLabel1.Text = "Start the game and enter the Creations menu or load a save to create a catalog file";
                    richTextBox2.Text = "Start the game and enter the Creations menu or load a save to create a catalog file";
                    return false;
                }
                string json = File.ReadAllText(jsonFilePath);
                string TestString = "";
                richTextBox2.Text = "";

                var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, ContentCatalog.Creation>>(json);

                ErrorFound = false;
                foreach (var kvp in data)
                {
                    TestString = kvp.Value.Version;
                    richTextBox2.Text += "Checking " + kvp.Value.Title+", "+ TestString + "\n";
                    VersionCheck = double.Parse((kvp.Value.Version.Substring(0, kvp.Value.Version.IndexOf('.'))));
                    TimeStamp = kvp.Value.Timestamp;
                    if (VersionCheck > kvp.Value.Timestamp && VersionCheck != 1)
                    {
                        ErrorCount++;
                        ErrorFound = true;
                        richTextBox2.Text += "Out of range version number detected in " + kvp.Value.Title + "\n";
                    }

                    for (int i = 0; i < TestString.Length; i++)
                    {
                        if (!char.IsDigit(TestString[i]) && TestString[i] != '.') // Check for numbers or . in Version
                        {
                            ErrorFound = true;
                            break;
                        }
                    }
                }

                if (!ErrorFound)
                {
                    toolStripStatusLabel1.Text = "Catalog ok";
                    richTextBox2.Text += "Catalog ok\n";
                    return true;
                }
                else
                {
                    toolStripStatusLabel1.Text = ErrorCount.ToString() + " Error(s) found";
                    richTextBox2.Text += ErrorCount.ToString() + " Error(s) found\n";
                    return false;
                }
            }

            catch
            {
                //MessageBox.Show("Start the game and enter the Creations menu to create a catalog file", "Missing Catalog File");
                //File.WriteAllText(CC.GetCatalog(), string.Empty);
                toolStripStatusLabel1.Text = "Start the game and enter the Creations menu or load a save to create a catalog file";
                richTextBox2.Text = "Start the game and enter the Creations menu or load a save to create a catalog file";
                return false;
            }
        }

        private void CleanCatalog()
        {
            if (!File.Exists(CC.GetCatalog()))
                File.WriteAllText(CC.GetCatalog(), string.Empty); // Create dummy catalog
            else
            {
                long fileSize = new FileInfo(CC.GetCatalog()).Length;

                if (fileSize > 0)
                {
                    NewFix();
                    //toolStripStatusLabel1.Text = "File cleaned and rewritten successfully";
                    //richTextBox2.Text += "Clean complete\n";
                    if (AutoBackup)
                        BackupCatalog();
                    DisplayCatalog();
                }
            }
        }

        private void cmdClean_Click(object sender, EventArgs e)
        {
            if (!CheckCatalog() || ForceClean)
            {
                CleanCatalog();
                //toolStripStatusLabel1.Text = errorCount.ToString() + " Errors found";
            }
            else
            {
                richTextBox2.Text += "Cleaning not needed\n";
                toolStripStatusLabel1.Text = "Catalog is OK. Cleaning not needed.";
                DisplayCatalog();
            }
        }

        private void SaveSettings()
        {
            // Save settings

            Properties.Settings.Default.AutoCheck = AutoCheck;
            Properties.Settings.Default.AutoClean = AutoClean;
            Properties.Settings.Default.AutoBackup = AutoBackup;
            Properties.Settings.Default.AutoRestore = AutoRestore;
            Properties.Settings.Default.StarfieldPath = CC.StarFieldPath;
            Properties.Settings.Default.ForceClean = ForceClean;
            Properties.Settings.Default.Save();
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            SaveSettings();
            Application.Exit();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            string pathToFile = CC.GetCatalog();

            // Launch Notepad and open the specified text file
            Process.Start(pathToFile);
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            CheckCatalog();
            //CheckBackup();
            DisplayCatalog();
        }

        private void StartStarfield()
        {
            const string userRoot = "HKEY_CURRENT_USER";
            const string subkey = @"Software\Valve\Steam";
            const string keyName = userRoot + "\\" + subkey;

            toolStripStatusLabel1.Text = "Starfield launching";
            // Get Steam path from Registry
            string stringValue = (string)Registry.GetValue(keyName, "SteamExe", "");
            Console.WriteLine($"String value: {stringValue}");


            var processInfo = new ProcessStartInfo(stringValue, "-applaunch 1716740");
            // Launch Starfield and wait for exit
            var process = Process.Start(processInfo);
        }

        private void btnStarfield_Click(object sender, EventArgs e)
        {
            SaveSettings();
            StartStarfield();
            Application.Exit();
        }


        private void btnAbout_Click(object sender, EventArgs e)
        {
            Form AboutBox = new frmAbout();
            AboutBox.Show();
        }

        private void btnExplore_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", CC.GetStarfieldPath());
        }

        private void btnEditPlugins_Click(object sender, EventArgs e)
        {
            string pathToFile = (CC.GetStarfieldPath() +
          @"\Plugins.txt");

            // Launch Notepad and open the specified text file
            Process.Start(pathToFile);
        }

        private void BackupCatalog()
        {
            if (!CheckCatalog())
            {
                richTextBox2.Text = "Catalog is corrupted. Backup not made.\n";
                //toolStripStatusLabel1.Text = "Catalog is corrupted. Backup not made.";
                if (AutoClean)
                    CleanCatalog();
                return;
            }

            if (!CheckBackup())
            {
                string sourceFileName = CC.GetCatalog();
                string destFileName = sourceFileName + ".bak";

                try
                {
                    // Copy the file
                    File.Copy(sourceFileName, destFileName, true); // overwrite
                    richTextBox2.Text += "Backup done\n";
                    toolStripStatusLabel1.Text = "Backup done";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Backup failed");
                }
            }
        }

        private void btnLoadOrder_Click(object sender, EventArgs e)
        {
            frmLoadOrder frmLO = new frmLoadOrder();
            frmLO.Show();
        }

        private void chkAutoCheck_CheckedChanged(object sender, EventArgs e)
        {
            AutoCheck = chkAutoCheck.Checked;
        }

        private void chkAutoClean_CheckedChanged(object sender, EventArgs e)
        {
            AutoClean = chkAutoClean.Checked;
        }

        private void chkAutoBackup_CheckedChanged(object sender, EventArgs e)
        {
            AutoBackup = chkAutoBackup.Checked;
        }

        private void chkAutoRestore_CheckedChanged(object sender, EventArgs e)
        {
            AutoRestore = chkAutoRestore.Checked;
        }

        private void txtSource_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://github.com/hst12/Starfield-Tools---ContentCatalog.txt-fixer");
        }

        private void cmdStarFieldPath_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.SelectedPath = CC.StarFieldPath;
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFolderPath = folderBrowserDialog.SelectedPath;
                    CC.StarFieldPath = selectedFolderPath;
                    Properties.Settings.Default.StarfieldPath = selectedFolderPath;
                }
            }
        }

        private void chkForceClean_CheckedChanged(object sender, EventArgs e)
        {
            ForceClean = chkForceClean.Checked;
        }

        private void btnStarfieldStore_Click(object sender, EventArgs e)
        {
            string cmdLine = @"shell:AppsFolder\BethesdaSoftworks.ProjectGold_3275kfvn8vcwc!Game";
            string altCmdLine = "cmd.exe /C start " + cmdLine;
            SaveSettings();

            try
            {
                Process.Start(cmdLine);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + cmdLine, "Error");
            }

            Application.Exit();
        }

        private void frmStarfieldTools_Activated(object sender, EventArgs e)
        {
            //bool cmdLineAuto = false;
            bool cmdLineRun = false;


            foreach (var arg in Environment.GetCommandLineArgs())
            {
                Console.WriteLine($"Argument: {arg}");
                //MessageBox.Show(arg);
                /*  if (String.Equals(arg, "-auto", StringComparison.OrdinalIgnoreCase))
                      cmdLineAuto = true;*/
                if (String.Equals(arg, "-run", StringComparison.OrdinalIgnoreCase))
                    cmdLineRun = true;
            }

            if (cmdLineRun)
            {
                SaveSettings();
                StartStarfield();
                Application.Exit();
            }
        }

        private void btnBackup_Click(object sender, EventArgs e)
        {
            BackupCatalog();
            DisplayCatalog();
        }

        private void cmdDeleteStale_Click(object sender, EventArgs e)
        {
            RemoveDeleteddEntries();
            DisplayCatalog();
        }

        private bool RestoreCatalog()
        {
            string destFileName = CC.GetCatalog();
            string sourceFileName = destFileName + ".bak";

            try
            {
                // Copy the file
                File.Copy(sourceFileName, destFileName, true); // overwrite

                richTextBox2.Text = "Restore done\n";
                toolStripStatusLabel1.Text = "Restore done";
                return true;
            }
            catch
            {
                richTextBox2.Text += "Restore failed.\n";
                toolStripStatusLabel1.Text = "Restore failed";
                return false;
            }
        }

        private void btnResetAll_Click(object sender, EventArgs e)
        {

            DialogResult result = MessageBox.Show("Do you want to continue?", "All version numbers will be reset. This will force all Creations to re-download", MessageBoxButtons.OKCancel);
            if (result != DialogResult.OK)
            {
                toolStripStatusLabel1.Text = "Version numbers not reset";
                return;
            }

            string jsonFilePath = CC.GetCatalog();

            string json = File.ReadAllText(jsonFilePath);

            var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, ContentCatalog.Creation>>(json);

            foreach (var kvp in data)
            {
                kvp.Value.Version = "1704067200.0"; // set version to 1704067200.0
            }

            data.Remove("ContentCatalog"); // remove messed up content catalog section

            json = Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);

            // Hack the Bethesda header back in
            json = @"{
  ""ContentCatalog"" : 
  {
    ""Description"" : ""This file holds a database of any Creations downloaded or installed, in JSON format"",
    ""Version"" : ""1.1""
  },
" + json.Substring(1); // to strip out a brace char

            File.WriteAllText(CC.GetCatalog(), json);
            DisplayCatalog();
            toolStripStatusLabel1.Text = "Version numbers reset";
            richTextBox2.Text = "";
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            RestoreCatalog();
            DisplayCatalog();
        }

        public string GetStarfieldPath()
        {
            return (Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)) + @"\Starfield";
        }

        private void RemoveDeleteddEntries() // Remove left over entries from catalog
        {
            List<string> esmFiles = new List<string>();
            string jsonFilePath = CC.GetCatalog();
            string json = File.ReadAllText(jsonFilePath); // Read catalog
            List<string> CreationsPlugin = new List<string>(); // filename of .esm
            List<string> CreationsTitle = new List<string>(); // Display title for .ems
            List<string> CreationsGUID = new List<string>(); // Creations GUID
            List<string> CreationsVersion = new List<string>(); // Version
            int RemovalCount = 0;
            int index;
            bool unusedMods = false;
            richTextBox2.Text = "";

            /*try
            {*/
            string filePath = GetStarfieldPath() + "\\Plugins.txt";
            string fileContent = File.ReadAllText(filePath);
            // Split the content into lines if necessary
            List<string> lines = fileContent.Split('\n').ToList();

            foreach (var file in lines)
            {
                if (file != "")
                {
                    if (file[0] != '#') // skip the comments
                        if (file[0] == '*') // Make a list of .esm files
                            esmFiles.Add(file.Substring(1)); // strip any *
                        else
                            esmFiles.Add(file);
                }
            }
            for (int i = 0; i < esmFiles.Count; i++)
            {
                esmFiles[i] = esmFiles[i].Trim();
            }
            /*}
            catch
            {
                toolStripStatusLabel1.Text = "Error loading plugins file";
            }*/

            Dictionary<string, object> json_Dictionary = (new JavaScriptSerializer()).Deserialize<Dictionary<string, object>>(json);
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
                                CreationsGUID.Add(kvp.Key);
                                CreationsTitle.Add(kvp.Value.Title);
                                richTextBox2.Text += "Scanning: " + kvp.Value.Title + "\n";
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}");
                    }
                }

                List<string> missingStrings = CreationsPlugin.Except(esmFiles).ToList();
                richTextBox1.Text = "";
                index = 0;

                if (missingStrings.Count > 0)
                {
                    for (index = 0; index < missingStrings.Count; index++)
                    {
                        for (int i = 0; i < CreationsGUID.Count; i++)
                        {
                            if (CreationsPlugin[i] == missingStrings[index])
                            {
                                richTextBox2.Text += "Removing " + CreationsGUID[i] + " " + CreationsTitle[i] + "\n";
                                data.Remove(CreationsGUID[i]);
                                unusedMods = true;
                                RemovalCount++;
                            }
                        }

                    }
                }
                if (unusedMods)
                    toolStripStatusLabel1.Text = RemovalCount.ToString() + " Unused mods removed from catalog";
                else
                    toolStripStatusLabel1.Text = "No unused mods found in catalog";
                json = Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);

                // Hack the Bethesda header back in
                json = @"{
  ""ContentCatalog"" : 
  {
    ""Description"" : ""This file holds a database of any Creations downloaded or installed, in JSON format"",
    ""Version"" : ""1.1""
  },
" + json.Substring(1); // to strip out a brace char

                File.WriteAllText(CC.GetCatalog(), json);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void NewFix()
        {
            string jsonFilePath = CC.GetCatalog();

            string json = File.ReadAllText(jsonFilePath);
            string TestString = "";
            bool FixVersion;
            int errorCount = 0;
            double VersionCheck;
            long TimeStamp;
            richTextBox2.Text = "";
            int VersionReplacementCount = 0;

            var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, ContentCatalog.Creation>>(json);

            foreach (var kvp in data)
            {
                TestString = kvp.Value.Version;
                FixVersion = false;
                richTextBox2.Text += "Checking "+ kvp.Value.Title + ", " + TestString + "\n";

                for (int i = 0; i < TestString.Length; i++)
                {

                    if (!char.IsDigit(TestString[i]) && TestString[i] != '.') // Check for numbers or . in Version
                    {
                        FixVersion = true;
                        break;
                    }
                }

                if (TestString != "1.1")
                {
                    VersionCheck = double.Parse((kvp.Value.Version.Substring(0, kvp.Value.Version.IndexOf('.'))));
                    TimeStamp = kvp.Value.Timestamp;
                    if (VersionCheck > kvp.Value.Timestamp)
                    {
                        kvp.Value.Version = "1704067200.0";
                        VersionReplacementCount++;
                    }
                }
                if (FixVersion)
                {
                    kvp.Value.Version = "1704067200.0"; // set version to 1704067200.0
                    errorCount++;
                }
            }
            data.Remove("ContentCatalog"); // remove messed up content catalog section

            json = Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);

            // Hack the Bethesda header back in
            json = @"{
  ""ContentCatalog"" : 
  {
    ""Description"" : ""This file holds a database of any Creations downloaded or installed, in JSON format"",
    ""Version"" : ""1.1""
  },
" + json.Substring(1); // to strip out a brace char

            File.WriteAllText(jsonFilePath, json);
            toolStripStatusLabel1.Text = VersionReplacementCount.ToString() + " Version replacements";
        }
    }
}
