using System;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Starfield_Tools
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            //Application.Run(new frmStarfieldTools());
            Application.Run(new frmLoadOrder(""));
        }
    }
}
