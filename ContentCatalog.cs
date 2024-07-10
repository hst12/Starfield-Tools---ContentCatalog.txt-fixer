﻿using System;

namespace Starfield_Tools
{
    internal class ContentCatalog
    {
        public string GetStarfieldPath()
        {
            return (Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
          @"\Starfield");
        }

        public string GetCatalog()
        {
            return (GetStarfieldPath() + @"\ContentCatalog.txt");
        }

    }
}