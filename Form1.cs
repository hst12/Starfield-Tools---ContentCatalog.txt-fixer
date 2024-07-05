﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Starfield_Tools
{

    public partial class frmStarfieldTools : Form
    {

        //private bool AutoCheck = true, AutoClean = true, AutoBackup = true, AutoRestore = true;
        private bool AutoCheck, AutoClean, AutoBackup, AutoRestore;

        public frmStarfieldTools()
        {
            InitializeComponent();
            
            // Initialise Checkboxes
            // Retrieve settings
            AutoCheck = Properties.Settings.Default.AutoCheck;
            AutoClean = Properties.Settings.Default.AutoClean;
            AutoBackup = Properties.Settings.Default.AutoBackup;
            AutoRestore = Properties.Settings.Default.AutoRestore;

            AutoCheck = true;
            AutoClean = true;
            AutoBackup = true;
            AutoRestore = true;


            chkAutoCheck.Checked = AutoCheck;
            chkAutoClean.Checked = AutoClean;
            chkAutoBackup.Checked = AutoBackup;
            chkAutoRestore.Checked = AutoRestore;

            richTextBox2.Text = "";
            if (AutoCheck) // Check catalog status if enabled
            {
                if (!CheckCatalog()) // If not okay, then...
                {
                    if (AutoRestore) // Restore backup file if auto restore is on
                        if (!RestoreCatalog()) // Check restore status
                            if (AutoClean) // Clean if restore failed
                                CleanCatalog();
                }
                toolStripStatusLabel1.Text = "Checks complete";
            }
            else toolStripStatusLabel1.Text = "Ready";



            if (AutoBackup)
                if (!CheckBackup()) // Backup if necessary
                    BackupCatalog();

            DisplayCatalog();

        }

        private bool CheckBackup()
        {
            string fileName1 = GetCatalog();
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
            richTextBox1.Text = File.ReadAllText(GetCatalog());
        }

        private string GetCatalog()
        {
            return (Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
          @"\Starfield\ContentCatalog.txt");
        }

        private bool CheckCatalog() // returns true if catalog good
        {
            string filePath = GetCatalog();
            // Read the file character by character
            StringBuilder cleanedContents = new StringBuilder();
            toolStripStatusLabel1.Text = "Checking...";
            richTextBox1.Text = "";
            int errorCount = 0, lineCount = 0;

            richTextBox2.Text += "Checking Catalog\n";

            try
            {

                using (StreamReader reader = new StreamReader(filePath))
                {
                    int character;
                    while ((character = reader.Read()) != -1)
                    {
                        if (character == 'n' || character == '\r') lineCount++;
                        if ((character < 32 || character > 126) && (character != '\n' && character != '\r'))
                        {
                            errorCount++;
                            richTextBox1.Text += "Invalid character at line " + lineCount.ToString() + "\n";
                        }
                    }
                    if (errorCount > 0)
                    {
                        richTextBox2.Text = "Error(s) found\n";
                        toolStripStatusLabel1.Text = errorCount.ToString() + " Error(s) found - Restore or Clean recommended";
                        return false;
                    }
                    else
                    {
                        toolStripStatusLabel1.Text = "Content Catalog looks OK";
                        richTextBox2.Text += "Catalog OK\n";
                        DisplayCatalog();
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error: {e.Message}" + "\n\n Creating dummy file", "Error");
                File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
          @"\Starfield\ContentCatalog.txt", string.Empty);
                toolStripStatusLabel1.Text = "Dummy ContentCatalog.txt file created";
                return false;
            }

        }

        private void CleanCatalog()
        {
            string filePath = GetCatalog();
            richTextBox1.Text = "Checking...\n\n";
            toolStripStatusLabel1.Text = "Checking...";
            // Read the file character by character
            StringBuilder cleanedContents = new StringBuilder();

            using (StreamReader reader = new StreamReader(filePath))
            {
                int character;
                while ((character = reader.Read()) != -1)
                {
                    // Preserve newline characters
                    if (character == '\n' || character == '\r')
                    {
                        cleanedContents.Append((char)character);
                        richTextBox1.Text += (char)character;
                    }
                    else if (character < 32 && character > 126 && character != '\n' && character != '\r')
                        toolStripStatusLabel1.Text = "Error found\n";
                    else if (character >= 32 && character <= 126)
                    {
                        cleanedContents.Append((char)character); // Keep printable characters
                        richTextBox1.Text += (char)character;
                    }
                }
            }

            // Write the cleaned contents back to the file
            File.WriteAllText(filePath, cleanedContents.ToString());

            toolStripStatusLabel1.Text = "File cleaned and rewritten successfully";
            richTextBox2.Text += "Clean complete\n";
            if (AutoBackup)
                BackupCatalog();
            DisplayCatalog();
        }

        private void cmdClean_Click(object sender, EventArgs e)
        {
            if (!CheckCatalog())
                CleanCatalog();
            else
            {
                richTextBox2.Text += "Cleaning not needed\n";
                toolStripStatusLabel1.Text = "Catalog is OK. Cleaning not needed.";
            }
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            // Set a values

            Properties.Settings.Default.AutoCheck = AutoCheck;
            Properties.Settings.Default.AutoClean = AutoClean;
            Properties.Settings.Default.AutoBackup = AutoBackup;
            Properties.Settings.Default.AutoRestore = AutoRestore;
            Properties.Settings.Default.Save();

            Application.Exit();
        }


        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            string pathToFile = GetCatalog();

            // Launch Notepad and open the specified text file
            Process.Start(pathToFile);
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            CheckCatalog();
        }

        private void btnStarfield_Click(object sender, EventArgs e)
        {
            /*string steam64 = @"SOFTWARE\Wow6432Node\Valve\";

            string steam64path="";

            RegistryKey key64 = Registry.LocalMachine.OpenSubKey(steam64);


            if (key64 != null)
            {
                steam64path = key64.GetValue("InstallPath").ToString();
                // Use steam64path as needed
            }
            MessageBox.Show(steam64path.ToString());*/
            toolStripStatusLabel1.Text = "Launching Starfield";
            System.Diagnostics.Process.Start(@"C:\Program Files (x86)\Steam\steam.exe", "-applaunch 1716740");
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            string AboutText = Application.ProductName+" "+Application.ProductVersion+"\n\n"+ @"Starfield ContentCatalog.txt checker

Checks ContentCatalog.txt file automatically when launched.

Latest version works auotmatically.
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

You can alt-tab from the game to check the ContentCatalog file to see if corruption has occurred by pressing the Check button.

The launch Starfield button is hard coded to the default Steam installation path at the moment and is only intended for quick testing.

Quit the game if it's running before using the Clean or Edit buttons.
";

            MessageBox.Show(AboutText, "Starfield Tools");
        }

        private void btnExplore_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Starfield");
        }

        private void btnEditPlugins_Click(object sender, EventArgs e)
        {
            string pathToFile = (Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
          @"\Starfield\Plugins.txt");

            // Launch Notepad and open the specified text file
            Process.Start(pathToFile);
        }

        private void BackupCatalog()
        {
            if (!CheckCatalog())
            {
                richTextBox2.Text = "Catalog is corrupted. Backup not made.\n";
                toolStripStatusLabel1.Text = "Catalog is corrupted. Backup not made.";
                return;
            }

            if (!CheckBackup())
            {
                string sourceFileName = GetCatalog();
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
            frmLoadOrder frmLO=new frmLoadOrder();
            frmLO.Show();
        }

        private void btnBackup_Click(object sender, EventArgs e)
        {
            BackupCatalog();
        }

        private bool RestoreCatalog()
        {
            string destFileName = GetCatalog();
            string sourceFileName = destFileName + ".bak";

            try
            {
                // Copy the file
                File.Copy(sourceFileName, destFileName, true); // overwrite

                richTextBox2.Text += "Restore done\n";
                CheckCatalog();
                return true;
            }
            catch (Exception ex)
            {
                richTextBox2.Text += "Restore failed.\n";
                MessageBox.Show($"Error: {ex.Message}", "Error");
                return false;
            }
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            RestoreCatalog();
        }
    }
}
