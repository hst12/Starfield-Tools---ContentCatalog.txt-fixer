using Microsoft.Win32;
using Starfield_Tools.Properties;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Starfield_Tools
{


    internal class Tools
    {
        public string StarFieldPath { get; set; }
        public string StarfieldGamePath { get; set; }

        public string GetStarfieldPath()
        {
            return (Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)) + @"\Starfield";
        }

        public void OpenUrl(string url)
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }

        public System.DateTime ConvertTime(double TimeToConvert)
        {
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            start = start.AddSeconds(TimeToConvert);
            return start;
        }

        public string GetCatalog()
        {
            return (GetStarfieldPath() + @"\ContentCatalog.txt");
        }

        public class Creation
        {
            public bool AchievementSafe { get; set; }
            public string[] Files { get; set; }
            public long FilesSize { get; set; }
            public long Timestamp { get; set; }
            public string Title { get; set; }
            public string Version { get; set; }
        }

        public class ModMetaData
        {
            public string ModName { get; set; }
            public string ModVersion { get; set; }
            public string SourceURL { get; set; }

        }

        public void ShowAbout()
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
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
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
                return ("");
            }
        }

        public void ShowSplashScreen()
        {
            Form SS = new frmSplashScreen();

            Rectangle resolution = Screen.PrimaryScreen.Bounds;
            int screenWidth = resolution.Width;
            int screenHeight = resolution.Height;
            SS.Width = screenWidth / 2;
            SS.Height = screenHeight / 2;
            SS.StartPosition = FormStartPosition.CenterScreen;
            SS.Show();
        }

        public void StartStarfieldMS()
        {
            string cmdLine = @"shell:AppsFolder\BethesdaSoftworks.ProjectGold_3275kfvn8vcwc!Game";

            try
            {
                Process.Start(cmdLine);
                ShowSplashScreen();
                Thread.Sleep(2000);
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + cmdLine, "Error");
            }
        }

        public void StartStarfieldSteam()
        {
            const string userRoot = "HKEY_CURRENT_USER";
            const string subkey = @"Software\Valve\Steam";
            const string keyName = userRoot + "\\" + subkey;

            // Get Steam path from Registry
            string stringValue = (string)Registry.GetValue(keyName, "SteamExe", "");

            var processInfo = new ProcessStartInfo(stringValue, "-applaunch 1716740");
            var process = Process.Start(processInfo);
            ShowSplashScreen();
            Thread.Sleep(4000);
            Application.Exit();
        }
    }
}
