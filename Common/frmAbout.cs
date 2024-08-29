using System.IO;
using System.Windows.Forms;

namespace Starfield_Tools
{
    public partial class frmAbout : Form
    {
        public frmAbout()
        {
            InitializeComponent();
            string Readme = File.ReadAllText("Common\\Readme.txt");
            string AboutText = Application.ProductName + " " + File.ReadAllText("Common\\App Version.txt") + "\n\n" + Readme;
            richTextBox1.Text = AboutText;
        }
    }
}
