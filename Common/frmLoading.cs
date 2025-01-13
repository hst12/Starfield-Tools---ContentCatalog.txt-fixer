using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Starfield_Tools.Common
{
    public partial class frmLoading : Form
    {
        public frmLoading(string msgText)
        {
            InitializeComponent();
            this.CenterToScreen();
            txtMessage.Text = msgText;
        }

    }
}
