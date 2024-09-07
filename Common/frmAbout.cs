using Starfield_Tools.Common;
using System.IO;
using System.Windows.Forms;

namespace Starfield_Tools
{
    public partial class frmAbout : Form
    {
        readonly Tools tools = new();
        public frmAbout()
        {
            InitializeComponent();
            string Readme = File.ReadAllText(Tools.CommonFolder+"\\Readme.txt");
            string AboutText = Application.ProductName + " " + File.ReadAllText(Tools.CommonFolder+"\\App Version.txt") + "\n\n" + Readme;
            richTextBox1.Text = AboutText;
        }
    }
}
