using System.IO;
using System.Windows.Forms;

namespace Starfield_Tools
{
    public partial class frmAbout : Form
    {
        public frmAbout()
        {
            Tools ToolVersion = new Tools();
            InitializeComponent();
            string Readme = File.ReadAllText("Readme.txt");
            string AboutText = Application.ProductName + " " + ToolVersion.ToolVersion + "\n\n" + Readme;
            richTextBox1.Text = AboutText;
        }
    }
}
