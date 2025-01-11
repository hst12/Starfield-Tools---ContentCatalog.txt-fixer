using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Starfield_Tools.Load_Order_Editor
{
    public partial class frmProfiles : Form
    {
        public frmProfiles(string ProfileName)
        {
            InitializeComponent();

            string DestProfile = Properties.Settings.Default.ProfileFolder + "\\Default.txt", ProfileFolder;
            Rectangle resolution = Screen.PrimaryScreen.Bounds; // Resize window to 75% of screen width
            double screenWidth = resolution.Width;
            double screenHeight = resolution.Height;
            this.Width = (int)(screenWidth * 0.85);
            this.Height = (int)(screenHeight * 0.85);
            this.StartPosition = FormStartPosition.CenterScreen;

            lblSource.Text = "Source Profile: " + ProfileName;
            var f = File.ReadAllLines(Properties.Settings.Default.ProfileFolder + "\\" + ProfileName);
            foreach (var line in f)
            {
                if (line[0] != '#')
                {
                    if (line[0] == '*')
                    {
                        chkBaseProfile.Items.Add(line[1..]);
                        chkBaseProfile.SetItemChecked(chkBaseProfile.Items.Count - 1, true);
                        chkAdd.Items.Add(line[1..]);
                        chkAdd.SetItemChecked(chkAdd.Items.Count - 1, true);
                        chkResult.Items.Add(line[1..]);
                    }
                    else
                    {
                        chkBaseProfile.Items.Add(line);
                        chkAdd.Items.Add(line);
                    }
                }
            }

            cmbDestination.Items.Clear();
            ProfileFolder = Properties.Settings.Default.ProfileFolder;
            if (ProfileFolder == null || ProfileFolder == "")
                ProfileFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            try
            {
                foreach (var profileName in Directory.EnumerateFiles(ProfileFolder, "*.txt", SearchOption.TopDirectoryOnly))
                {
                    cmbDestination.Items.Add(profileName[(profileName.LastIndexOf('\\') + 1)..]);
                }
                int index = cmbDestination.Items.IndexOf(Properties.Settings.Default.LastProfile);
                if (index != -1)
                {
                    cmbDestination.SelectedIndex = index;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void chkBaseProfile_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            e.NewValue = e.CurrentValue;
        }

        private void chkResult_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            e.NewValue = e.CurrentValue;
        }

        private void cmbDestination_SelectedIndexChanged(object sender, EventArgs e)
        {
            //MessageBox.Show((string)cmbDestination.SelectedItem);
        }
    }
}
