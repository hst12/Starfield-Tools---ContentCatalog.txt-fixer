using System.IO;
using System.Windows.Forms;

namespace Starfield_Tools
{
    public partial class frmAbout : Form
    {
        public frmAbout()
        {
            InitializeComponent();
            string Readme = File.ReadAllText("Readme.txt");
            string AboutText = Application.ProductName + " " + "\n\n" + Readme;
            richTextBox1.Text = AboutText;
        }
    }
}
