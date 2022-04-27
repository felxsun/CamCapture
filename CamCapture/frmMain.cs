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

        //naviagtion
        private Rectangle boxHead;
        private MCvScalar navBoxCvColor;
        private Color navBoxColor;
        

        private string folderImg;
        private string folderVideo;

        private int intervalImg;
        private int durationRecord;
        private int durationVideo;

        private System.Timers.Timer pictureTimer;
        private System.Timers.Timer videoTimer;
        private System.Timers.Timer recordTimer;
        private System.Timers.Timer frameTimer;
        private System.Timers.Timer countDownTimer;

        private int countDownCounter=0;

        private Mat m;

        private DateTime startStamp;
        private DateTime initTime;
        private int pictureCount;
        private int pictureSecond;

        public frmMain()
        {
            InitializeComponent();
            this.cap = null;
            this.inCapture = false;
            this.vwr = null;

            Properties.Settings.Default.fps = DEFAULT_FPS;
            pictureTimer = new System.Timers.Timer();
            pictureTimer.Enabled = false;
            boxHead = Rectangle.Empty;
            navBoxColor = Color.Yellow;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            refreshSettings();

            this.Text = String.Format("影像定時擷取器  {0}.{1}.{2} {3}:{4}"
                , ThisAssembly.Git.BaseVersion.Major
                , ThisAssembly.Git.BaseVersion.Minor
                , ThisAssembly.Git.BaseVersion.Patch
                , ThisAssembly.Git.Branch
                , ThisAssembly.Git.Commits);
            
            this.cameraNames = ListOfAttachedCameras();
            this.cbxCameras.Items.Clear();

            this.pictureTimer = new System.Timers.Timer();
            this.recordTimer = new System.Timers.Timer();
            this.videoTimer = new System.Timers.Timer();
            this.frameTimer = new System.Timers.Timer();
            this.countDownTimer = new System.Timers.Timer();
            this.pictureTimer.Elapsed += OnPictureTimerEvent;
            this.recordTimer.Elapsed += onRecordTimerEvent;
            this.videoTimer.Elapsed += onVideoTimerEvent;
            this.frameTimer.Elapsed += onFrameTimerEvent;
            this.countDownTimer.Elapsed += onCountDownTimerEvent;

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

        public Rectangle getNavBoxHead(int iWidth, int iHeight, float bxTop, float bxWidth, float bxHeight)
        {
            try
            {
                int tTop = (int)(iHeight * bxTop);
                int tWidth = (int)(bxWidth * iWidth);
                int tHeight = (int)(bxHeight * iHeight);
                int tX = (int)((iWidth - tWidth) / 2);

                return new Rectangle(tX, tTop, tWidth, tHeight);
            }
            catch
            {
                return Rectangle.Empty;
            }
        }

        public MCvScalar genNavBoxColor(Color c)
        {
            return new MCvScalar( c.B,  c.G,c.R,0);
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
#if DEBUG
                tryAppendLog(DateTime.Now.ToString() + "\topen capture\n");
#endif
                this.cap = new Capture(cameraIndex);

#if DEBUG
                tryAppendLog(DateTime.Now.ToString() + "\tset NavBox\n");
#endif
                this.frameHeight = Convert.ToInt32(this.cap.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight));
                this.frameWidth = Convert.ToInt32(this.cap.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth));

                this.navBoxCvColor = this.genNavBoxColor(this.navBoxColor);
      
                this.boxHead = this.getNavBoxHead(
                    this.frameWidth
                    ,this.frameHeight
                    ,Properties.Settings.Default.boxTop
                    ,Properties.Settings.Default.boxWidth
                    ,Properties.Settings.Default.boxHeight
                    );
#if DEBUG
                tryAppendLog(DateTime.Now.ToString() + "\tset FPS\n");
#endif
                //WebCam 沒有 FPS
                this.cap.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps,24);
#if DEBUG
                tryAppendLog(DateTime.Now.ToString() + "\tget FPS\n");
#endif
                this.fps = (int)this.cap.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps);
#if DEBUG
                tryAppendLog(DateTime.Now.ToString() + "\tset video format as XVID\n");
#endif
                this.fourcc = VideoWriter.Fourcc('X', 'V', 'I', 'D');

#if DEBUG
                tryAppendLog(DateTime.Now.ToString() + "\tset frameCount\n");
#endif
                this.cap.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameCount, 0);
                this.initTime = DateTime.Now;

#if DEBUG
                tryAppendLog(DateTime.Now.ToString() + "\tset image grab event handler\n");
#endif
                this.cap.ImageGrabbed += VideoCapture_ImageGrabbed;
#if DEBUG
                tryAppendLog(DateTime.Now.ToString() + "\tstart to do capture\n");
#endif
                this.cap.Start();

#if DEBUG
                tryAppendLog(DateTime.Now.ToString() + "\tCapture started successfully\n");
#endif

            }
            catch (Exception ex)
            {
                disconnectCapture();
                MessageBox.Show("Failed to link camear" + Environment.NewLine + ex.Message);
            }
        }

        private void tryAppendLog(string msg)
        {
            int i;

            for(i=0; i<5; ++i)
            {
                try
                {
                    File.AppendAllText("trace.log", msg);
                    return;
                }
                catch
                {
                    ++i;
                }
            }

            MessageBox.Show("Failed to write log : " + msg);
        }

        private void VideoCapture_ImageGrabbed(object sender, EventArgs e)
        {
#if DEBUG
            File.AppendAllText("trace.log", DateTime.Now.ToString() + "\tgrab image\n");
#endif
            this.m = null;
            this.m = new Mat();
            try
            {
                this.cap.Retrieve(m);
            }
            catch (Exception ex)
            {
#if DEBUG
                File.AppendAllText("trace.log", DateTime.Now.ToString() + "\t image capture : "+ex.Message+"\n");
#endif
                MessageBox.Show("Fatal error : " + ex.Message);
            }

            try
            {
                Mat t = m.Clone();
                //frame
                CvInvoke.Rectangle(t, this.boxHead, this.navBoxCvColor);
                //countdown timer
                if (this.countDownCounter > 0)
                    CvInvoke.PutText(t
                        , this.countDownCounter.ToString()
                        , new Point(60, 60)
                        , Emgu.CV.CvEnum.FontFace.HersheySimplex
                        , 2.0
                        , new Bgr(Color.White).MCvScalar
                        , 4);

                picMain.Image = t.ToImage<Bgr, byte>().Bitmap;
            }
            catch (Exception ex)
            {
#if DEBUG
                File.AppendAllText("trace.log", DateTime.Now.ToString() + "\t Image Rander : " + ex.Message + "\n");
#endif
                MessageBox.Show("Fatal error : " + ex.Message);
            }

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

            this.boxHead = this.getNavBoxHead(
                    this.frameWidth
                    , this.frameHeight
                    , Properties.Settings.Default.boxTop
                    , Properties.Settings.Default.boxWidth
                    , Properties.Settings.Default.boxHeight
                    );
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (inCapture)
            {
                stopRecording();
            }
            else if (initialFolder())
            {
                this.btnStart.Text = "停止錄製";
                this.mnuSetting.Enabled = false;
                this.cbxCameras.Enabled = false;
                this.pictureSecond = DateTime.Now.Second;

                //picture timer
                //this.pictureTimer.Interval=this.intervalImg;
                this.pictureTimer.Interval = this.intervalImg/3; //2021_11_05_改3倍速檢測
                this.pictureCount = 1;
                //total recoder timer
                this.recordTimer.Interval=this.durationRecord*1000;
                this.startStamp = DateTime.Now;
                //frame timer
                this.frameTimer.Interval = 1000.0f / this.fps;
                //video timer
                this.videoTimer.Interval = this.durationVideo * 1000;
                //count down
                this.countDownTimer.Interval = 1000;
                this.countDownCounter = (int)Properties.Settings.Default.countDown;



                inCapture = true;
                this.pictureTimer.Start();
                this.recordTimer.Start();
                this.frameTimer.Start();
                this.videoTimer.Start();
                this.countDownTimer.Start();
                this.startStamp = DateTime.Now;
            }
        }

        private void stopRecording()
        {
            inCapture = false;

            this.pictureTimer.Stop();
            this.recordTimer.Stop();
            this.videoTimer.Stop();
            this.frameTimer.Stop();
            this.countDownTimer.Stop();


            if (this.vwr != null)
                this.vwr = null;

            enManuStrip(mnuFile);
            enCbxCamer(this.cbxCameras);

            
            setButtonText(btnStart,"開始錄製");
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
            //用以計秒
            int tSecod = DateTime.Now.Second;
            if (!inCapture || this.pictureSecond==tSecod) //換秒後才能再做
                return;

            string fileName=null;
            try
            {

                DateTime t = DateTime.Now;
                //savePicture(picMain, this.folderImg + "\\" + this.pictureCount.ToString("000") + "_" + t.ToString("yyyyMMdd_hhmmss")+".png");
                Bitmap ti = this.m.ToImage<Bgr, byte>().Bitmap;
                //2021 1021 : 變更counter為四位數
                fileName = this.folderImg + "\\" + this.pictureCount.ToString("0000") + "_" + t.ToString("yyyyMMdd_hhmmss") + ".png";
                ti.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
                this.pictureSecond = tSecod;
            }
            catch (Exception ex)
            {
                //避免中斷流程,改在status bar上顯示
                //同時刪除出錯的影像檔
                //MessageBox.Show("Failed on frame image");
                //throw new Exception("Failed on frame image");
                try
                {
                    if (fileName != null) File.Delete(fileName);
                }
                catch { }

                this.statusMessage.Text = ex.Message;
            }

            ++this.pictureCount;
        }

        private void onRecordTimerEvent(Object source, ElapsedEventArgs e)
        {
            stopRecording();
            MessageBox.Show("錄製完成");
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

            if (this.vwr == null)
            {
                try
                {
                    this.vwr = new VideoWriter(
                        this.folderVideo + "\\" + DateTime.Now.ToString("yyyyMMdd_hhmmss") + ".mp4",
                        this.fourcc,
                        this.fps,
                        new Size(frameWidth, frameHeight),
                        true
                    );
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed on new video fragment");
                    throw new Exception("Failed on new video fragment");
                }

            }

            int i = 0;
            while(i<10)
            {
                try
                {
                    this.vwr.Write(m);
                    m = null;
                    ++i;
                    return;
                }
                catch (Exception ex)
                {
                    if (i >= 5)
                    {
                        MessageBox.Show("Failed on write Frame");
                        throw new Exception("Failed on write Frame");
                    }
                }
            }
            
        }

        private void onCountDownTimerEvent(Object source, ElapsedEventArgs e)
        {
            if (!inCapture || this.countDownCounter<=0)
                return;
            --this.countDownCounter;
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

        //enable setting mnu
        delegate void enManuStripCallBack(MenuStrip ms);
        private void enManuStrip(MenuStrip ms)
        {
            if (ms.InvokeRequired)
            {
                enManuStripCallBack d = new enManuStripCallBack(enManuStrip);
                this.Invoke(d, new object[] { ms });
            }
            else
                mnuSetting.Enabled = true;
        }

        //enable setting mnu
        delegate void enCbxCamerCallBack(ComboBox bx);
        private void enCbxCamer(ComboBox bx)
        {
            if (bx.InvokeRequired)
            {
                enCbxCamerCallBack d = new enCbxCamerCallBack(enCbxCamer);
                this.Invoke(d, new object[] { bx });
            }
            else
                bx.Enabled = true;
        }

        //enable setting mnu
        delegate void setButtonTextCallBack(Button btn, string txt);
        private void setButtonText(Button btn, string txt)
        {
            if (btn.InvokeRequired)
            {
                setButtonTextCallBack d = new setButtonTextCallBack(setButtonText);
                this.Invoke(d, new object[] { btn, txt });
            }
            else
                btn.Text = txt;
        }
    }
}

























































