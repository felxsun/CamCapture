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

namespace CamCapture
{
    public partial class frmMain : Form
    {
        private Capture cap;
        private List<DsDevice> videocaptures;
        public frmMain()
        {
            InitializeComponent();
            videocaptures = new List<DsDevice>();
            this.cap = null;
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

            var devices = new List<DsDevice>(DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice));
            

        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }
}

























































