using Starfield_Tools.Properties;
using System.Drawing;
using System.Windows.Forms;

namespace Starfield_Tools
{
    public partial class frmSplashScreen : Form
    {
        public frmSplashScreen()
        {
            InitializeComponent();
            string LoadScreen = Settings.Default.LoadScreenFilename;
            if (LoadScreen != null)
            {
                var bmp = new Bitmap(LoadScreen);
                this.BackgroundImage = null;
                this.BackgroundImage = bmp;
            }
            Rectangle resolution = Screen.PrimaryScreen.Bounds; // Resize window to 75% of screen width
            double screenWidth = resolution.Width;
            double screenHeight = resolution.Height;
            this.Width = (int)(screenWidth * 0.75);
            this.Height = (int)(screenHeight * 0.75);
            this.StartPosition = FormStartPosition.CenterScreen;
        }
    }
}
