using Microsoft.Win32;
using Starfield_Tools.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Starfield_Tools.Common
{

    internal class Tools
    {
        public static string CommonFolder { get; set; }
        public string StarFieldPath { get; set; }
        public string StarfieldGamePath { get; set; }
        public List<string> BethFiles { get; set; }
        public static string CatalogVersion { get; set; }
        public static string StarfieldAppData { get; set; }


        public Tools() // Constructor
        {
            CommonFolder = Environment.CurrentDirectory + "\\Common";
            try
            {
                BethFiles = new(File.ReadAllLines(CommonFolder + "\\BGS Exclude.txt"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "BGS Exclude file missing. Repair or re-install the app", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                Environment.Exit(1);
            }
            try
            {
                CatalogVersion = File.ReadAllText(CommonFolder + "\\Catalog Version.txt");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Catalog Version file missing. Repair or re-install the app", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                Environment.Exit(1);
            }
            try
            {
                StarfieldAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Starfield";
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Starfield AppData folder missing. Repair the game", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                Environment.Exit(1);
            }
        }

        public static string MakeHeaderBlank()
        {
            string HeaderString = "";

            try
            {
                HeaderString = File.ReadAllText(CommonFolder + "\\header.txt"); // Read the header from file
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Missing Header.txt file - unable to continue. Re-install or repair the tool");
                Application.Exit();
            }
            return HeaderString;
        }

        public static string MakeHeader()
        {
            string HeaderString = MakeHeaderBlank();
            HeaderString = HeaderString[..^5] + ",";
            return HeaderString;
        }
        public static void OpenUrl(string url)
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }

        public static DateTime ConvertTime(double TimeToConvert)
        {
            DateTime start = new(1970, 1, 1, 0, 0, 0, 0);
            try
            {
                start = start.AddSeconds(TimeToConvert);
            }
            catch
            {
                start = new(1970, 1, 1, 0, 0, 0, 0);
            }
            return start;
        }
        public static string GetCatalog()
        {
            return StarfieldAppData + @"\ContentCatalog.txt";
        }

        public class Group // LOOT
        {
            public string name { get; set; }
            public List<string> after { get; set; }
        }

        public class Plugin // LOOT
        {
            public string name { get; set; }
            public string group { get; set; }
            public List<string> after { get; set; }
            public List<string> inc { get; set; }
            public List<string> req { get; set; }
            public List<Url> url { get; set; }
        }

        public class Url // LOOT
        {
            public string link { get; set; }
            public string name { get; set; }
        }

        public class Configuration // LOOT
        {
            public List<Group> groups { get; set; }
            public List<Plugin> plugins { get; set; }
        }
        public class Creation // ContentCatalog.txt format
        {
            public bool AchievementSafe { get; set; }
            public string[] Files { get; set; }
            public long FilesSize { get; set; }
            public long Timestamp { get; set; }
            public string Title { get; set; }
            public string Version { get; set; }
        }

        public class ModMetaData // Testing
        {
            public string ModName { get; set; }
            public string[] ModFiles { get; set; }
            public string ModVersion { get; set; }
            public string SourceURL { get; set; }
            public string MainCategory { get; set; }
        }

        public static bool FileCompare(string file1, string file2)
        {
            // Check if the same file was referenced two times.
            if (file1 == file2)
            {
                return true;
            }

            // Open the two files.
            try
            {
                using FileStream fs1 = new(file1, FileMode.Open),
                                  fs2 = new(file2, FileMode.Open);
                // Check the file sizes. If they are not the same, the files are not the same.
                if (fs1.Length != fs2.Length)
                {
                    return false;
                }

                // Read and compare a byte from each file until either a non-matching set of bytes is found or until the end of file1 is reached.
                int file1byte, file2byte;
                do
                {
                    file1byte = fs1.ReadByte();
                    file2byte = fs2.ReadByte();
                }
                while ((file1byte == file2byte) && (file1byte != -1));

                // Return the success of the comparison.
                return file1byte == file2byte;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false; ;
            }


        }
        public static void CheckGame()
        {
            if (!Directory.Exists(StarfieldAppData)) // Check if Starfield is installed
            {
                MessageBox.Show("Unable to continue. Is Starfield installed correctly?", "Starfield not found in AppData directory", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                Environment.Exit(1);
            }
        }
        public static void ShowAbout()
        {
            Form AboutBox = new frmAbout();
            Rectangle resolution = Screen.PrimaryScreen.Bounds;
            int screenWidth = resolution.Width;
            int screenHeight = resolution.Height;
            AboutBox.Width = screenWidth / 2;
            AboutBox.Height = screenHeight / 2;
            AboutBox.StartPosition = FormStartPosition.CenterScreen;
            AboutBox.Show();
        }

        public string SetStarfieldGamePath()
        {
            using FolderBrowserDialog folderBrowserDialog = new();
            folderBrowserDialog.SelectedPath = Settings.Default.StarfieldGamePath;
            folderBrowserDialog.Description = "Choose path to the game installation folder";
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedFolderPath = folderBrowserDialog.SelectedPath;
                StarfieldGamePath = selectedFolderPath;
                Settings.Default.StarfieldGamePath = selectedFolderPath;
                Settings.Default.Save();
                return selectedFolderPath;
            }
            return "";
        }

        public static bool StartStarfieldCustom()
        {
            string cmdLine = Properties.Settings.Default.CustomEXE;
            if (cmdLine == null)
                return false;

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = cmdLine,
                    WorkingDirectory = Properties.Settings.Default.StarfieldGamePath,
                    UseShellExecute = false //
                };
                Process.Start(startInfo);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + cmdLine, "Error");
                return false;
            }
        }
        
        public static bool StartStarfieldSFSE()
        {
            string cmdLine = Properties.Settings.Default.StarfieldGamePath + "\\sfse_loader.exe";
            if (cmdLine == null)
                return false;

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = cmdLine,
                    WorkingDirectory = Properties.Settings.Default.StarfieldGamePath,
                    UseShellExecute = false //
                };
                Process.Start(startInfo);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + cmdLine, "Error");
                return false;
            }
        }
        public static bool StartStarfieldMS()
        {
            string cmdLine = @"shell:AppsFolder\BethesdaSoftworks.ProjectGold_3275kfvn8vcwc!Game";

            try
            {
                Process.Start(cmdLine);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + cmdLine, "Error");
                return false;
            }
        }
        public static bool StartGame(int GameVersion)
        {
            bool result = false;
            switch (GameVersion)
            {
                case 0:
                    result = StartStarfieldSteam();
                    break;
                case 1:
                    result = StartStarfieldMS();
                    break;
                case 2:
                    result = StartStarfieldCustom();
                    break;
                case 3:
                    result = StartStarfieldSFSE();
                    break;
            }
            return result;
        }

        public static bool StartStarfieldSteam()
        {
            const string userRoot = "HKEY_CURRENT_USER";
            const string subkey = @"Software\Valve\Steam";
            const string keyName = userRoot + "\\" + subkey;

            try
            {
                string stringValue = (string)Registry.GetValue(keyName, "SteamExe", ""); // Get Steam path from Registry
                var processInfo = new ProcessStartInfo(stringValue, "-applaunch 1716740");
                var process = Process.Start(processInfo);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unable to start game");
                return false;
            }
        }
    }
}
