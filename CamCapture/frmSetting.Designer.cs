
namespace CamCapture
{
    partial class frmSetting
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSetting));
            this.lblDirVideo = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtFolderVideo = new System.Windows.Forms.TextBox();
            this.txtFolderImage = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.numImageInterval = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.numVideoDuration = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.numRecordDuration = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numImageInterval)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numVideoDuration)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRecordDuration)).BeginInit();
            this.SuspendLayout();
            // 
            // lblDirVideo
            // 
            this.lblDirVideo.AutoSize = true;
            this.lblDirVideo.Location = new System.Drawing.Point(24, 20);
            this.lblDirVideo.Name = "lblDirVideo";
            this.lblDirVideo.Size = new System.Drawing.Size(65, 12);
            this.lblDirVideo.TabIndex = 0;
            this.lblDirVideo.Text = "影片子目錄";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 50);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "照片子目錄";
            // 
            // txtFolderVideo
            // 
            this.txtFolderVideo.Location = new System.Drawing.Point(95, 16);
            this.txtFolderVideo.Name = "txtFolderVideo";
            this.txtFolderVideo.Size = new System.Drawing.Size(239, 22);
            this.txtFolderVideo.TabIndex = 2;
            // 
            // txtFolderImage
            // 
            this.txtFolderImage.Location = new System.Drawing.Point(95, 47);
            this.txtFolderImage.Name = "txtFolderImage";
            this.txtFolderImage.Size = new System.Drawing.Size(239, 22);
            this.txtFolderImage.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(36, 87);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "照片間隔";
            // 
            // numImageInterval
            // 
            this.numImageInterval.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numImageInterval.Location = new System.Drawing.Point(95, 85);
            this.numImageInterval.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numImageInterval.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numImageInterval.Name = "numImageInterval";
            this.numImageInterval.Size = new System.Drawing.Size(83, 22);
            this.numImageInterval.TabIndex = 5;
            this.numImageInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numImageInterval.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(184, 87);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(18, 12);
            this.label3.TabIndex = 6;
            this.label3.Text = "ms";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(36, 115);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 7;
            this.label4.Text = "影片長度";
            // 
            // numVideoDuration
            // 
            this.numVideoDuration.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numVideoDuration.Location = new System.Drawing.Point(95, 113);
            this.numVideoDuration.Maximum = new decimal(new int[] {
            1800,
            0,
            0,
            0});
            this.numVideoDuration.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numVideoDuration.Name = "numVideoDuration";
            this.numVideoDuration.Size = new System.Drawing.Size(83, 22);
            this.numVideoDuration.TabIndex = 8;
            this.numVideoDuration.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numVideoDuration.Value = new decimal(new int[] {
            600,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(184, 115);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(17, 12);
            this.label5.TabIndex = 9;
            this.label5.Text = "秒";
            // 
            // numRecordDuration
            // 
            this.numRecordDuration.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numRecordDuration.Location = new System.Drawing.Point(95, 141);
            this.numRecordDuration.Maximum = new decimal(new int[] {
            18000,
            0,
            0,
            0});
            this.numRecordDuration.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numRecordDuration.Name = "numRecordDuration";
            this.numRecordDuration.Size = new System.Drawing.Size(83, 22);
            this.numRecordDuration.TabIndex = 10;
            this.numRecordDuration.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numRecordDuration.Value = new decimal(new int[] {
            1800,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(36, 143);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 11;
            this.label6.Text = "錄製時間";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(184, 143);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(17, 12);
            this.label7.TabIndex = 12;
            this.label7.Text = "秒";
            // 
            // frmSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(363, 177);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.numRecordDuration);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.numVideoDuration);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.numImageInterval);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtFolderImage);
            this.Controls.Add(this.txtFolderVideo);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblDirVideo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSetting";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "錄製參數設置";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmSetting_FormClosing);
            this.Load += new System.EventHandler(this.frmSetting_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numImageInterval)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numVideoDuration)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRecordDuration)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblDirVideo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtFolderVideo;
        private System.Windows.Forms.TextBox txtFolderImage;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numImageInterval;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numVideoDuration;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numRecordDuration;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
    }
}