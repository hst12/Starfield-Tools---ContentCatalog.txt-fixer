using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Starfield_Tools
{
    public partial class frmStarfieldTools : Form
    {
        public frmStarfieldTools()
        {
            InitializeComponent();
            CheckCatalog();
            DisplayCatalog();
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

        private bool CheckCatalog()
        {
            string filePath = GetCatalog();
            // Read the file character by character
            StringBuilder cleanedContents = new StringBuilder();
            toolStripStatusLabel1.Text = "Checking...";
            richTextBox1.Text = "";
            int errorCount = 0,lineCount=0;
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
                        toolStripStatusLabel1.Text = errorCount.ToString() + " Error(s) found - Clean recommended";
                        return false;
                    }
                    else
                    {
                        toolStripStatusLabel1.Text = "Content Catalog looks OK";
                        DisplayCatalog();
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error: {e.Message}"+"\n\n Creating dummy file", "Error");
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

            toolStripStatusLabel1.Text="File cleaned and rewritten successfully";
            DisplayCatalog() ;
        }

        private void cmdClean_Click(object sender, EventArgs e)
        {
            if (!CheckCatalog()) CleanCatalog();
            else toolStripStatusLabel1.Text = "Catalog is OK. Cleaning not needed.";
         }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            string pathToFile = GetCatalog();

            // Launch Notepad and open the specified text file
            Process.Start( pathToFile);
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
            string AboutText = @"Starfield ContentCatalog.txt checker

Checks ContentCatalog.txt file automatically when launched.

Check button re-checks the file.

Clean button repairs the file. It may take a while with a large mod list.

Edit button opens the file for editing with your default text editor.

Explore button opens the folder with your plugin and catalog files.
You could manually edit the Plugins.txt file to enable or disable mods if needed.
A * character indicates that a mod is enabled.

You can alt-tab from the game to check the ContentCatalog file to see if corruption has occurred by pressing the Check button.

The launch Starfield button is hard coded to the default Steam installation path at the moment and is only intended for quick testing.

Quit the game if it's running before using the Clean or Edit buttons.
";
                
            MessageBox.Show(AboutText,"Starfield Tools");
        }

        private void btnExplore_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)+@"\Starfield");
        }

        private void btnEditPlugins_Click(object sender, EventArgs e)
        {
            string pathToFile = (Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
          @"\Starfield\Plugins.txt");

            // Launch Notepad and open the specified text file
            Process.Start(pathToFile);
        }

        private void btnBackup_Click(object sender, EventArgs e)
        {
            string sourceFileName = GetCatalog();
            string destFileName = sourceFileName + ".bak";

            try
            {
                // Copy the file
                File.Copy(sourceFileName, destFileName,true); // overwrite

                Console.WriteLine("File copied successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error");
            }
        
    }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            string destFileName = GetCatalog();
            string sourceFileName = destFileName+".bak";
            

            try
            {
                // Copy the file
                File.Copy(sourceFileName, destFileName, true); // overwrite

                Console.WriteLine("File copied successfully!");
                CheckCatalog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error");
            }
        }
    }
}
