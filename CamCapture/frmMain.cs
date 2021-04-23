using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using DirectShowLib;
using SharpDX.MediaFoundation;

namespace CamCapture
{
    public partial class frmMain : Form
    {
        private Capture cap;
        public string[] cameraNames;
        Emgu.CV.VideoWriter vwr;
        bool inCapture;

        public frmMain()
        {
            InitializeComponent();
            this.cap = null;
            this.inCapture = false;
            this.vwr = null;

        }

        
        private void frmMain_Load(object sender, EventArgs e)
        {
            this.Text = String.Format("影像定時擷取器  {0}.{1}.{2} {3}:{4}"
                , ThisAssembly.Git.SemVer.Major
                , ThisAssembly.Git.SemVer.Minor
                , ThisAssembly.Git.SemVer.Patch
                , ThisAssembly.Git.Branch
                , ThisAssembly.Git.Commits);
            /*
            captures.Clear();
            try
            {
                for (int i = 0; i < 20; ++i)
                {
                    Capture c = new  Capture(Emgu.CV.CvEnum.CaptureType.Msmf);
                    captures.Add(c);
                }
            }
            catch { }
            /**/
            this.cameraNames = ListOfAttachedCameras();
            this.cbxCameras.Items.Clear();
            if (this.cameraNames != null)
            {
                for (int i = 0; i < this.cameraNames.Length; ++i)
                    this.cbxCameras.Items.Add(this.cameraNames[i]);

                this.cbxCameras.SelectedIndex = 0;
            }
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            disconnectCapture();
            Properties.Settings.Default.Save();
        }

        public static int GetCameraIndexForPartName(string partName)
        {
            var cameras = ListOfAttachedCameras();
            for (var i = 0; i < cameras.Count(); i++)
            {
                if (cameras[i].ToLower().Contains(partName.ToLower()))
                {
                    return i;
                }
            }
            return -1;
        }

        public static string[] ListOfAttachedCameras()
        {
            var cameras = new List<string>();
            var attributes = new MediaAttributes(1);
            attributes.Set(CaptureDeviceAttributeKeys.SourceType.Guid, CaptureDeviceAttributeKeys.SourceTypeVideoCapture.Guid);
            var devices = MediaFactory.EnumDeviceSources(attributes);
            for (var i = 0; i < devices.Count(); i++)
            {
                var friendlyName = devices[i].Get(CaptureDeviceAttributeKeys.FriendlyName);
                cameras.Add(friendlyName);
            }
            return cameras.ToArray();
        }

        private void cbxCameras_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox bx = (ComboBox)sender;
            selectCamera(bx.SelectedIndex);

        }

        private void selectCamera(int cameraIndex)
        {
            if(cameraIndex<0 || this.cameraNames == null || cameraIndex >= this.cameraNames.Length)
            {
                disconnectCapture();
                return;
            }  
            
            try
            {
                this.cap = new Capture(cameraIndex);
                this.cap.ImageGrabbed += VideoCapture_ImageGrabbed;
                this.cap.Start();
            }
            catch(Exception ex)
            {
                disconnectCapture();
                MessageBox.Show("Failed to link camear" + Environment.NewLine + ex.Message);
            }
        }
        private void VideoCapture_ImageGrabbed(object sender, EventArgs e)
        {
            /*
            if (fileChanged)
            {
                totalFrames = this.cap.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameCount);
                fps = this.cap.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps);
                int fourcc = Convert.ToInt32(videoCapture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FourCC));
                int frameHeight = Convert.ToInt32(videoCapture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight));
                int frameWidth = Convert.ToInt32(videoCapture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth));
                string destination = "C:\\Users\\ITNOA\\Desktop\\savedVideoDHS\\" + i + ".avi";
                videoWriter = new VideoWriter(destination, VideoWriter.Fourcc('I', 'Y', 'U', 'V'), fps, new Size(frameWidth, frameHeight), true);
                fileChanged = false;
            }
            /**/

            Mat m = new Mat();
            this.cap.Retrieve(m);
            picMain.Image = m.ToImage<Bgr, byte>().Bitmap;
            //videoWriter.Write(m);
        }

        private void disconnectCapture()
        {
            if(this.cap!=null)
            {
                this.cap.Dispose();
                this.cap = null;
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("立即結束此程式?", "影像資料定時錄製器", MessageBoxButtons.YesNo) != DialogResult.Yes)
                e.Cancel = true;
        }
    }
}

























































