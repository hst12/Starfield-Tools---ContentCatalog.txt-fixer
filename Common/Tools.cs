﻿using Microsoft.Win32;
using Starfield_Tools.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Security.Permissions;
using System.Windows.Forms;

namespace Starfield_Tools.Common // Various functions used by the app
{

    internal class Tools
    {

        public static string CommonFolder { get; set; }
        public static string DocumentationFolder { get; set; }
        public string StarFieldPath { get; set; }
        public string StarfieldGamePath { get; set; }
        public string StarfieldGamePathMS { get; set; }
        public List<string> BethFiles { get; set; }
        public static string CatalogVersion { get; set; }
        public static string StarfieldAppData { get; set; }
        public List<string> PluginList { get; set; }

        public Tools() // Constructor
        {
            CommonFolder = Environment.CurrentDirectory + "\\Common\\"; // Used to read misc txt files used by the app
            DocumentationFolder = Environment.CurrentDirectory + "\\Documentation";
            try
            {
                BethFiles = new(File.ReadAllLines(CommonFolder + "BGS Exclude.txt")); // Exclude these files from Plugin list
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "BGS Exclude file missing. Repair or re-install the app", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                Environment.Exit(1);
            }
            try
            {
                CatalogVersion = File.ReadAllText(CommonFolder + "Catalog Version.txt");
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

        public static string MakeHeaderBlank() // Used to build ContentCatalog.txt header
        {
            string HeaderString = "";

            try
            {
                HeaderString = File.ReadAllText(CommonFolder + "header.txt"); // Read the header from file
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Missing Header.txt file - unable to continue. Re-install or repair the tool");
                Application.Exit();
            }
            return HeaderString;
        }

        public static string MakeHeader() // Used to build ContentCatalog.txt header
        {
            string HeaderString = MakeHeaderBlank();
            HeaderString = HeaderString[..^5] + ",";
            return HeaderString;
        }
        public static void OpenUrl(string url) // Launch web browser from argument
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }

        public static DateTime ConvertTime(double TimeToConvert) // Convert catalog time format to human readable
        {
            DateTime start = new(1970, 1, 1, 0, 0, 0, 0);
            try
            {
                start = start.AddSeconds(TimeToConvert);
            }
            catch
            {
                start = new(1970, 1, 1, 0, 0, 0, 0); // Return 1970 if error converting
            }
            return start;
        }
        public static string GetCatalogPath()
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
            public string display { get; set; }
            public string group { get; set; }
            public List<string> after { get; set; }
            public List<string> inc { get; set; }
            public List<Req> req { get; set; }
            public List<Msg> msg { get; set; }
            public List<Url> url { get; set; }

        }

        public class Req // LOOT
        {
            public string name { get; set; }
            public string display { get; set; }
        }

        public class Msg // LOOT
        {
            public string type { get; set; }
            public string content { get; set; }
        }
        public class Url // LOOT
        {
            public string link { get; set; }
            public string name { get; set; }
        }
        public class Prelude// LOOT
        {
            public List<MessageAnchor> common { get; set; }
        }

        public class MessageAnchor// LOOT
        {
            public string type { get; set; }
            public string content { get; set; }
            public List<string> subs { get; set; }
            public string condition { get; set; }
        }

        public class Globals// LOOT
        {
            public string type { get; set; }
            public string content { get; set; }
            public List<string> subs { get; set; }
            public string condition { get; set; }
        }
        public class Configuration // LOOT
        {
            public Prelude prelude { get; set; }
            public List<string> bash_tags { get; set; }
            public List<Group> groups { get; set; }
            public List<Plugin> plugins { get; set; }
            public List<Plugin> common { get; set; }
            public List<Globals> globals { get; set; }
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

        public static bool FileCompare(string file1, string file2) // Compare two files. Return true if same
        {
            // Check if the same file was referenced two times.
            if (file1 == file2)
            {
                return true;
            }

            // Open the two files.
#pragma warning disable CS0168 // Variable is declared but never used
            try
            {
                var fileA = File.ReadAllText(file1);
                var fileB = File.ReadAllText(file2);
                if (fileA == fileB)
                    return true;
                else
                    return false;

            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show("Tools.FileCompare error: " + ex.Message);
#endif
                return false; ;
            }
#pragma warning restore CS0168 // Variable is declared but never used

        }
        public static void CheckGame() // Check if Starfield appdata folder exists
        {
            if (!Directory.Exists(StarfieldAppData))
            {
                MessageBox.Show("Unable to continue. Is Starfield installed correctly?", "Starfield AppData directory not found", MessageBoxButtons.OK, MessageBoxIcon.Stop);
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

        public string SetStarfieldGamePath() // Prompt for Starfield game path
        {

            MessageBox.Show("Please select the path to the game installation folder where Starfield.exe is located", "Select Game Path", MessageBoxButtons.OK, MessageBoxIcon.Information);

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = frmLoadOrder.StarfieldGamePath;
                if (!string.IsNullOrEmpty(frmLoadOrder.StarfieldGamePath))
                    openFileDialog.InitialDirectory = frmLoadOrder.StarfieldGamePath;
                else
                {
                    openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                }

                openFileDialog.InitialDirectory = Settings.Default.StarfieldGamePath;

                openFileDialog.Title = "Set the path to the Starfield executable - Starfield.exe";
                openFileDialog.FileName = "Starfield.exe";
                openFileDialog.Filter = "Starfield.exe|Starfield.exe";
                var Result = openFileDialog.ShowDialog();
                if (Result == DialogResult.OK)
                {
                    if (!File.Exists(openFileDialog.FileName))
                    {
                        MessageBox.Show("Starfield.exe not found in the selected path", "Please select the correct folder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return ("");
                    }
                    StarfieldGamePath = Path.GetDirectoryName(openFileDialog.FileName);
                    Settings.Default.StarfieldGamePath = StarfieldGamePath;
                    Settings.Default.Save();
                    return StarfieldGamePath;
                }
                return "";
            }
        }

        public string SetStarfieldGamePathMS()
        {
            string selectedPath = "";
            MessageBox.Show("Please select the path to the game installation folder where Starfield.exe is located", "Select Game Path", MessageBoxButtons.OK, MessageBoxIcon.Information);

            using (FolderBrowserDialog folderBrowserDialog = new())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedPath = folderBrowserDialog.SelectedPath;
                    StarfieldGamePathMS = selectedPath;
                    Settings.Default.GamePathMS = selectedPath;
                    Settings.Default.Save();
                    return selectedPath;
                }
                else
                    return "";
            }
        }
        public static bool StartStarfieldCustom() // Start game with custom exe
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

        public static bool StartStarfieldSFSE() // Start game with SFSE loader
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
        public static bool StartStarfieldMS() // Start game with MS Store version
        {
            //string cmdLine = @"shell:AppsFolder\BethesdaSoftworks.ProjectGold_3275kfvn8vcwc!Game";
            string cmdLine = Properties.Settings.Default.GamePathMS + "\\Starfield.exe";

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = cmdLine,
                    WorkingDirectory = Properties.Settings.Default.GamePathMS,
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
        public static bool StartGame(int GameVersion) // Figure out which version of the game to start
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

        public static bool StartStarfieldSteam() // Start game with Steam version
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

        public static bool ConfirmAction(string ActionText, string ActionTitle) // Return true for OK, false for cancel
        {
            if (frmLoadOrder.NoWarn)
                return true;
            DialogResult DialogResult = MessageBox.Show(ActionText, ActionTitle, MessageBoxButtons.OKCancel, MessageBoxIcon.Stop);

            if (DialogResult == DialogResult.OK)
                return true;
            else
                return false;
        }
        public List<string> GetPluginList() // Get list of plugins from Starfield appdata folder
        {
            PluginList = new();
            try
            {
                string[] files = Directory.GetFiles(frmLoadOrder.StarfieldGamePath + "\\Data", "*.esm", SearchOption.TopDirectoryOnly);
                foreach (string file in files)
                {
                    if (!BethFiles.Contains(Path.GetFileName(file)))
                        PluginList.Add(Path.GetFileName(file));
                }
                return PluginList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error reading plugins", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            return PluginList;
        }
    }
}
