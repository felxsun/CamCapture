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
    }
}

























































