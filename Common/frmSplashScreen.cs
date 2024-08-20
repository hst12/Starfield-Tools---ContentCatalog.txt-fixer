using Starfield_Tools.Properties;
using System;
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

                Rectangle screen = Screen.PrimaryScreen.Bounds;
                var bitmap = new Bitmap(LoadScreen);
                float screenWidth = screen.Width * 0.75f;
                float screenHeight = screen.Height * 0.75f;

                // Calculate the scaling factor to maintain aspect ratio
                float scale = Math.Min(screenWidth / bitmap.Width, screenHeight / bitmap.Height);

                // Calculate the new dimensions
                int newWidth = (int)(bitmap.Width * scale);
                int newHeight = (int)(bitmap.Height * scale);

                // Set the form size to the new dimensions

                this.BackgroundImage = bitmap;
                this.ClientSize = new Size(newWidth, newHeight);
            }
            this.StartPosition = FormStartPosition.CenterScreen;

        }
    }
}
