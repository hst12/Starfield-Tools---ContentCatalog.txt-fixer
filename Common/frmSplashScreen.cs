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
            Rectangle screen = Screen.PrimaryScreen.Bounds;
            float screenWidth = screen.Width * 0.75f;
            float screenHeight = screen.Height * 0.75f;
            if (LoadScreen != null && LoadScreen != "")
            {


                var bitmap = new Bitmap(LoadScreen);


                // Calculate the scaling factor to maintain aspect ratio
                float scale = Math.Min(screenWidth / bitmap.Width, screenHeight / bitmap.Height);

                // Calculate the new dimensions
                int newWidth = (int)(bitmap.Width * scale);
                int newHeight = (int)(bitmap.Height * scale);

                // Set the form size to the new dimensions

                this.BackgroundImage = bitmap;
                this.ClientSize = new Size(newWidth, newHeight);
            }
            else
            {
                this.Width = (int)(screen.Width * 0.75f);
                this.Height = (int)(screen.Height * 0.75f);
            }
            this.StartPosition = FormStartPosition.CenterScreen;

        }
    }
}
