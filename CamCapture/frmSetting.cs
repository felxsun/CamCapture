using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace CamCapture
{
    public partial class frmSetting : Form
    {
        public frmSetting()
        {
            InitializeComponent();
        }

        private void frmSetting_Load(object sender, EventArgs e)
        {
            this.txtFolderVideo.Text = 
                (Properties.Settings.Default.directoryVideo == "") ? "video" : Properties.Settings.Default.directoryVideo;

            this.txtFolderImage.Text =
                (Properties.Settings.Default.directoryPicture == "") ? "img" : Properties.Settings.Default.directoryPicture;

            try
            {
                this.numImageInterval.Value = Properties.Settings.Default.imageInterval;
            }
            catch
            {
                this.numImageInterval.Value = 1000;
            }

            try
            {
                this.numVideoDuration.Value = Properties.Settings.Default.videoDuration;
            }
            catch
            {
                this.numVideoDuration.Value = 600;
            }

            try
            {
                this.numRecordDuration.Value = Properties.Settings.Default.recordDuration;
            }
            catch
            {
                this.numRecordDuration.Value = 1800;
            }
        }

        private void frmSetting_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("結束並儲存設定", "定時影像錄製設定", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Properties.Settings.Default.directoryVideo = this.txtFolderVideo.Text;
                Properties.Settings.Default.directoryPicture = this.txtFolderImage.Text;
                Properties.Settings.Default.imageInterval = (int)this.numImageInterval.Value;
                Properties.Settings.Default.recordDuration = (int)this.numRecordDuration.Value;
                Properties.Settings.Default.videoDuration = (int)this.numVideoDuration.Value;
                Properties.Settings.Default.Save();
            }
            else
                e.Cancel = true;
        }

    }
}
