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
        private System.Timers.Timer frameTimer;

        private Mat m;

        private DateTime startStamp;
        private DateTime initTime;
        private int pictureCount;

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

            this.pictureTimer = new System.Timers.Timer();
            this.recordTimer = new System.Timers.Timer();
            this.videoTimer = new System.Timers.Timer();
            this.frameTimer = new System.Timers.Timer();
            this.pictureTimer.Elapsed += OnPictureTimerEvent;
            this.recordTimer.Elapsed += onRecordTimerEvent;
            this.videoTimer.Elapsed += onVideoTimerEvent;
            this.frameTimer.Elapsed += onFrameTimerEvent;

            if (this.cameraNames != null)
            {
                for (int i = 0; i < this.cameraNames.Length; ++i)
                    this.cbxCameras.Items.Add(this.cameraNames[i]);

                this.cbxCameras.SelectedIndex = 0;
            }

            this.m = new Mat();
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
            this.cap.Retrieve(m);
            picMain.Image = m.ToImage<Bgr, byte>().Bitmap;
            string duration = (DateTime.Now - this.startStamp).TotalSeconds.ToString("0");
            statusMessage.Text = (inCapture)? String.Format("錄製中,應錄{2}秒,己錄{0}秒,己擷取{1}照片",duration ,this.pictureCount,this.durationRecord) : "-";
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
            if (this.inCapture || MessageBox.Show("立即結束此程式?", "影像資料定時錄製器", MessageBoxButtons.YesNo) != DialogResult.Yes)
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
                inCapture = false;

                this.pictureTimer.Stop();
                this.recordTimer.Stop();
                this.videoTimer.Stop();
                this.frameTimer.Stop();

                
                if(this.vwr!=null)
                    this.vwr.Dispose();
                mnuSetting.Enabled = true;
                this.cbxCameras.Enabled = true;
                this.btnStart.Text = "開始錄製";
                this.vwr = null;

            }
            else if (initialFolder())
            {
                this.btnStart.Text = "停止錄製";
                this.mnuSetting.Enabled = false;
                this.cbxCameras.Enabled = false;

                //picture timer
                this.pictureTimer.Interval=this.intervalImg;
                this.pictureCount = 1;
                //total recoder timer
                this.recordTimer.Interval=this.durationRecord*1000;
                this.startStamp = DateTime.Now;
                //frame timer
                this.frameTimer.Interval = 1000.0f / this.fps;
                //video timer
                this.videoTimer.Interval = this.durationVideo * 1000;

                inCapture = true;
                this.pictureTimer.Start();
                this.recordTimer.Start();
                this.frameTimer.Start();
                this.videoTimer.Start();
                this.startStamp = DateTime.Now;
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
            throw new Exception("ToDo: RecordTimerEvent");

        }

        private void onVideoTimerEvent(Object source, ElapsedEventArgs e)
        {
            if (!inCapture)
                return;
            if (this.vwr != null)
                this.vwr = null;
        }

        private void onFrameTimerEvent(Object source, ElapsedEventArgs e)
        {
            if (!inCapture)
                return;
            if(this.vwr==null)
            {
                this.vwr = new VideoWriter(
                    this.folderVideo+"\\"+DateTime.Now.ToString("yyyyMMdd_hhmmss")+".mp4",
                    this.fourcc,
                    this.fps,
                    new Size(frameWidth, frameHeight),
                    true
                );
            }
            else 
                this.vwr.Write(m);
        }

        /// <summary>
        /// 開啟檔案管理員 顯示資料檔根目錄
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        //save video frmae
        delegate void addFrameCallBack(PictureBox bx, string fileName);
        private void addFrame(PictureBox bx, string fileName)
        {
            if (bx.InvokeRequired)
            {
                addFrameCallBack d = new addFrameCallBack(addFrame);
                this.Invoke(d, new object[] { bx, fileName });
            }
            else
                throw new Exception("ToDo: addFrame operation");
        }
    }
}

























































