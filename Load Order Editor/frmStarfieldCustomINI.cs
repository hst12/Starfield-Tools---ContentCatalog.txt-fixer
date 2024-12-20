using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Starfield_Tools.Load_Order_Editor
{
    public partial class frmStarfieldCustomINI : Form
    {
        public frmStarfieldCustomINI()
        {
            InitializeComponent();
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\My Games\\Starfield\\StarfieldCustom.ini";
            var fileContent = File.ReadAllLines(filePath);

            //Set checkboxes
            foreach (var lines in fileContent)
            {
                if (lines.Contains("sIntroSequence"))
                    chkSkipIntro.Checked = true;
                if (lines.Contains("bInvalidateOlderFiles"))
                    chkLooseFiles.Checked = true;
                if (lines.Contains("bEnableMessageOfTheDay"))
                    chkMOTD.Checked = true;
                if (lines.Contains("uMainMenuDelayBeforeAllowSkip"))
                    chkMainMenuDelay.Checked = true;
                if (lines.Contains("bForcePhotoModeLoadScreen"))
                    chkUserPhotos.Checked = true;
                if (lines.Contains("bEnableLogging"))
                    chkPapyrusLogging.Checked = true;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\My Games\\Starfield\\StarfieldCustom.ini";

                List<string> INILines = new();
            INILines.Add(@"[General]
sIntroSequence=");

            if (chkMOTD.Checked)
                INILines.Add("bEnableMessageOfTheDay=0");

            if (chkMainMenuDelay.Checked)
                INILines.Add("uMainMenuDelayBeforeAllowSkip=0");

            if (chkUserPhotos.Checked)
                INILines.Add(@"
[Interface]
bForcePhotoModeLoadScreen=1");

            if (chkLooseFiles.Checked)
            {
                INILines.Add(@"
[Archive]
bInvalidateOlderFiles=");
            }

            if (chkPapyrusLogging.Checked)
            {
                INILines.Add(@"
[Papyrus]
bEnableLogging=1");
            }

            File.WriteAllLines(filePath, INILines);
            Properties.Settings.Default.LooseFiles = chkLooseFiles.Checked;
            Properties.Settings.Default.Save();
            this.Close();
        }
    }
}
