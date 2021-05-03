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
using System.Threading;
using System.Timers;
using System.IO;

namespace CamCapture
{
    public partial class frmMain : Form
    {
        //DEFAULT Setting
        public const int DEFAULT_FPS = 24;
        private Capture cap;
        public string[] cameraNames;

        Emgu.CV.VideoWriter vwr;
        private bool inCapture;
        private int frameHeight;
        private int frameWidth;
        private int fps;
        private int fourcc;
        

        private string folderImg;
        private string folderVideo;
        private int intervalImg;
        private int durationRecord;
        private int durationVideo;

        private System.Timers.Timer pictureTimer;
        private System.Timers.Timer videoTimer;
        private System.Timers.Timer recordTimer;
        private DateTime startStamp;
        private DateTime initTime;
        private int pictureCount;
        private int videoCount; //control video


        public frmMain()
        {
            InitializeComponent();
            this.cap = null;
            this.inCapture = false;
            this.vwr = null;

            Properties.Settings.Default.fps = DEFAULT_FPS;

            
            pictureTimer = new System.Timers.Timer();
            pictureTimer.Enabled = false;
        }

        
        private void frmMain_Load(object sender, EventArgs e)
        {
            refreshSettings();

            this.Text = String.Format("影像定時擷取器  {0}.{1}.{2} {3}:{4}"
                , ThisAssembly.Git.SemVer.Major
                , ThisAssembly.Git.SemVer.Minor
                , ThisAssembly.Git.SemVer.Patch
                , ThisAssembly.Git.Branch
                , ThisAssembly.Git.Commits);
            
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
            disconnectCapture();
            if(cameraIndex<0 || this.cameraNames == null || cameraIndex >= this.cameraNames.Length)
                return;
            

            try
            {
                this.cap = new Capture(cameraIndex);

                this.frameHeight = Convert.ToInt32(this.cap.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight));
                this.frameWidth = Convert.ToInt32(this.cap.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth));
                //WebCam 沒有 FPS
                this.cap.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps,24);
                this.fps = (int)this.cap.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps);
                this.fourcc = VideoWriter.Fourcc('X', 'V', 'I', 'D');

                this.cap.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameCount, 0);
                this.initTime = DateTime.Now;


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
            Mat m = new Mat();
            this.cap.Retrieve(m);
            picMain.Image = m.ToImage<Bgr, byte>().Bitmap;
            if (inCapture)
                this.vwr.Write(m);

        }

        private void disconnectCapture()
        {
            if(this.cap!=null)
            {
                    this.cap.Stop();
                    this.cap.ImageGrabbed -= VideoCapture_ImageGrabbed;
                    this.cap.Dispose();
                    this.cap = null;
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("立即結束此程式?", "影像資料定時錄製器", MessageBoxButtons.YesNo) != DialogResult.Yes)
                e.Cancel = true;
        }

        private void settingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmSetting dlg = new frmSetting();
            dlg.ShowDialog();
            refreshSettings();
        }

        private void refreshSettings()
        {
            string appPath = Application.StartupPath;
            this.folderImg = appPath + "\\" + Properties.Settings.Default.directoryPicture;
            this.folderVideo = appPath + "\\" + Properties.Settings.Default.directoryVideo;
            this.intervalImg = Properties.Settings.Default.imageInterval;
            this.durationRecord = Properties.Settings.Default.recordDuration;
            this.durationVideo = Properties.Settings.Default.videoDuration;
            this.fps = Properties.Settings.Default.fps;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (inCapture)
            {
                this.pictureTimer.Stop();
                this.pictureTimer.Dispose();
                this.pictureTimer.Elapsed -= OnPictureTimerEvent;
                this.pictureTimer = null;

                inCapture = false;
                this.vwr.Dispose();
                mnuSetting.Enabled = true;
                this.btnStart.Text = "開始錄製";

            }
            else if (initialFolder())
            {
                string destination = "a1.avi";
                this.vwr = new VideoWriter(destination, this.fourcc, this.fps, new Size(frameWidth, frameHeight), true);
                inCapture = true;
                this.btnStart.Text = "停止錄製";
                this.mnuSetting.Enabled = false;

                //picture timer
                this.pictureTimer = new System.Timers.Timer(this.intervalImg);
                this.pictureTimer.Elapsed += OnPictureTimerEvent;
                this.pictureCount = 1;
                this.pictureTimer.Start();
                //total recoder timer
                this.recordTimer = new System.Timers.Timer(this.durationRecord*1000);
                this.startStamp = DateTime.Now;
                this.recordTimer.Start();
                //video timer



            }
        }

        //Check and initial image folder
        private bool initialFolder()
        {
            if( (Directory.Exists(this.folderImg) && Directory.GetFiles(this.folderImg).Length>0)
                ||
                (Directory.Exists(this.folderVideo) && Directory.GetFiles(this.folderVideo).Length>0)
            )
                if (MessageBox.Show("影像目錄存在且不為空目錄, 清空目錄後再繼續？", "檢查影像目錄", MessageBoxButtons.OKCancel) != DialogResult.OK)
                    return false;

            //clear and create folder
            try
            {

                if (Directory.Exists(this.folderImg))
                {
                    DirectoryInfo dr = new DirectoryInfo(this.folderImg);
                    foreach (FileInfo f in dr.GetFiles())
                        f.Delete();
                }
                else
                    Directory.CreateDirectory(this.folderImg);

                if (Directory.Exists(this.folderVideo))
                {
                    DirectoryInfo dr = new DirectoryInfo(this.folderVideo);
                    foreach (FileInfo f in dr.GetFiles())
                        f.Delete();
                }
                else
                    Directory.CreateDirectory(this.folderVideo);
            }
            catch(Exception ex)
            {
                MessageBox.Show("建立或清理影像檔目錄失敗"+Environment.NewLine+ex.Message);
                return false;
            }
            return true;
            
        }

        private void OnPictureTimerEvent(Object source, ElapsedEventArgs e)
        {
            if (!inCapture)
                return;

            DateTime t = DateTime.Now;
            savePicture(picMain, this.folderImg + "\\" + this.pictureCount.ToString("000") + "_" + t.ToString("yyyyMMdd_hhmmss")+".png");
            ++this.pictureCount;
        }

        private void onRecordTimerEvent(Object source, ElapsedEventArgs e)
        {
            if (!inCapture)
                return;

            this.inCapture = false;
            this.vwr.Dispose();

        }
        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Application.StartupPath);
        }

        //save image
        delegate void savePictureCallBack(PictureBox bx, string fileName);
        private void savePicture(PictureBox bx, string fileName)
        {
            if (bx.InvokeRequired)
            {
                savePictureCallBack d = new savePictureCallBack(savePicture);
                this.Invoke(d, new object[] { bx, fileName });
            }
            else
                picMain.Image.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}

























































