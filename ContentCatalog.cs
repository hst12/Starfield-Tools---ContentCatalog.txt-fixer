using System;

namespace Starfield_Tools
{
    

    internal class ContentCatalog
    {
        public string StarFieldPath { get; set; }

        public string GetStarfieldPath()
        {
            return (Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)) + @"\Starfield";
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

    }
}
