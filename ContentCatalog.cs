using System;
using System.Drawing;
using System.Windows.Forms;

namespace Starfield_Tools
{


    internal class ContentCatalog
    {
        public string StarFieldPath { get; set; }

        public string GetStarfieldPath()
        {
            return (Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)) + @"\Starfield";
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
    }
}
