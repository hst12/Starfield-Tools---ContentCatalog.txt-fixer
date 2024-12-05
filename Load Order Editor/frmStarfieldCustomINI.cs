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

            //Set checkboxes
            if (Properties.Settings.Default.LooseFiles)
                chkLooseFiles.Checked = true;

        }

        private void ChangeSetting(bool EnableDisable, string INISection, string INISetting, string INISetting2 = "sResourceDataDirsFinal=") // True for enabled
        {
            string LooseFilesDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\My Games\\Starfield\\",
                    filePath = LooseFilesDir + "StarfieldCustom.ini";
            bool LooseFiles = false;

            if (EnableDisable)
            {
                List<string> linesToAppend = [INISection, INISetting];
                File.AppendAllLines(filePath, linesToAppend);
                LooseFiles = true;
            }
            else
            {
                if (File.Exists(filePath))
                {
                    var lines = File.ReadAllLines(filePath).ToList();
                    if (lines.Contains(INISetting))
                    {
                        string[] linesToRemove = [INISection, INISetting, INISetting2];

                        foreach (var lineToRemove in linesToRemove)
                        {
                            lines.RemoveAll(line => line.Trim() == lineToRemove);
                        }

                        File.WriteAllLines(filePath, lines);
                        LooseFiles = false;
                    }
                }
            }
            Properties.Settings.Default.LooseFiles = LooseFiles;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            //string INISection, INISetting;

            ChangeSetting(chkLooseFiles.Checked, "[Archive]", "bInvalidateOlderFiles=");
            ChangeSetting(chkMOTD.Checked, "[General]", "bEnableMessageOfTheDay=");
            ChangeSetting(chkUserPhotos.Checked, "[Interface]", "bForcePhotoModeLoadScreen=");
            ChangeSetting(ckhPapyrusLogging.Checked, "[Papyrus]", "bEnableLogging=");
            this.Close();
        }
    }
}
