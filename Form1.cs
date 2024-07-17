using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using static Starfield_Tools.frmLoadOrder;

namespace Starfield_Tools
{

    public partial class frmStarfieldTools : Form
    {

        private bool AutoCheck, AutoClean, AutoBackup, AutoRestore, ForceClean;

        ContentCatalog CC = new ContentCatalog();

        public frmStarfieldTools()
        {
            InitializeComponent();

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
            richTextBox2.Text += "Checking Catalog\n";

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

                var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Creation>>(json);

                ErrorFound = false;
                foreach (var kvp in data)
                {
                    TestString = kvp.Value.Version;

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
                    toolStripStatusLabel1.Text = "Error(s) found";
                    return false;
                }
            }

            catch (Exception e)
            {
                //MessageBox.Show("Start the game and enter the Creations menu to create a catalog file", "Missing Catalog File");
                File.WriteAllText(CC.GetCatalog(), string.Empty);
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
                    toolStripStatusLabel1.Text = "File cleaned and rewritten successfully";
                    richTextBox2.Text += "Clean complete\n";
                    if (AutoBackup)
                        BackupCatalog();
                    DisplayCatalog();
                }
            }
        }

        private void cmdClean_Click(object sender, EventArgs e)
        {
            if (!CheckCatalog() || ForceClean)
                CleanCatalog();
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

            // Get Steam path from Registry
            string stringValue = (string)Registry.GetValue(keyName, "SteamExe", "");
            Console.WriteLine($"String value: {stringValue}");


            var processInfo = new ProcessStartInfo(stringValue, "-applaunch 1716740");
            // Launch Starfield and wait for exit
            var process = Process.Start(processInfo);
            toolStripStatusLabel1.Text = "Starfield launching";
        }

        private void btnStarfield_Click(object sender, EventArgs e)
        {
            SaveSettings();
            StartStarfield();
            Application.Exit();
            //toolStripStatusLabel1.Text = "Ready";
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            string AboutText = Application.ProductName + " " + Application.ProductVersion + "\n\n" + @"Starfield ContentCatalog.txt checker

Checks ContentCatalog.txt file automatically when launched.

The tool works auotmatically if you enable all the check boxes in the Auto Functions section.

Run it before going to the Creations menu and afer exiting the Creations menu
Quit the game and run the tool before loading a saved game if you've installed new mods or updated mods in the Creations menu.
No need to use the tool if you're just playing the game normally.

Check button re-checks the file.

Clean button repairs the file. It may take a while with a large mod list.

Backup button copies ContentCatalog.txt to ContentCatalog.txt.bak. A previous backup if it exists will be overwritten without warning.
Use the backup function before loading your saved game since this is when the corruption may occur.
It's best to exit the game after making modifications to your installed mods, updating mods or changing the load order.

Restore button copies ContentCatalog.txt.bak to ContentCatalog.txt

Edit buttons are for opening ContentCatalog.txt or Plugins.txt files for editing with your default text editor.

Explore button opens the folder with your plugin and catalog files.
You could manually edit the Plugins.txt file to enable or disable mods if needed.
A * character indicates that a mod is enabled.

Load Order button shows a list of mods and allows them to be turned on or off or moved up or down in the load order

You can alt-tab from the game to check the ContentCatalog file to see if corruption has occurred by pressing the Check button.

The launch Starfield button is for a Steam installation and is only intended for quick testing.

Quit the game if it's running before using the Clean or Edit buttons.
";

            MessageBox.Show(AboutText, "Starfield Tools");
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

            try
            {
                Process.Start(cmdLine);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + cmdLine, "Error");
            }
            SaveSettings();
            Application.Exit();
        }

        private void frmStarfieldTools_Activated(object sender, EventArgs e)
        {
            bool cmdLineAuto = false;
            bool cmdLineRun = false;


            foreach (var arg in Environment.GetCommandLineArgs())
            {
                Console.WriteLine($"Argument: {arg}");
                //MessageBox.Show(arg);
                if (String.Equals(arg, "-auto", StringComparison.OrdinalIgnoreCase))
                    cmdLineAuto = true;
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

        private bool RestoreCatalog()
        {
            string destFileName = CC.GetCatalog();
            string sourceFileName = destFileName + ".bak";

            try
            {
                // Copy the file
                File.Copy(sourceFileName, destFileName, true); // overwrite

                richTextBox2.Text += "Restore done\n";
                return true;
            }
            catch (Exception ex)
            {
                richTextBox2.Text += "Restore failed.\n";
                toolStripStatusLabel1.Text = "Restore failed";
                return false;
            }
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            RestoreCatalog();
            DisplayCatalog();
        }

        private void NewFix()
        {
            string jsonFilePath = CC.GetCatalog();

            string json = File.ReadAllText(jsonFilePath);
            string TestString = "";
            bool FixVersion;

            var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Creation>>(json);

            foreach (var kvp in data)
            {
                TestString = kvp.Value.Version;
                FixVersion = false;
                for (int i = 0; i < TestString.Length; i++)
                {
                    if (!char.IsDigit(TestString[i]) && TestString[i] != '.') // Check for numbers or . in Version
                    {
                        FixVersion = true;
                        break;
                    }
                }
                if (FixVersion)
                    kvp.Value.Version = "0.1"; // set version to 0.1
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
" + json.Substring(1); // to strip out a bracket char

            File.WriteAllText(jsonFilePath, json);
        }


    }
}
