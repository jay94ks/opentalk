namespace OpenTalk.Main
{
    partial class ScrMain
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.m_StatusMessage = new System.Windows.Forms.Label();
            this.m_LoadingProgress = new OpenTalk.UI.Forms.ColorizedProgressBar();
            this.SuspendLayout();
            // 
            // m_StatusMessage
            // 
            this.m_StatusMessage.AutoSize = true;
            this.m_StatusMessage.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_StatusMessage.Location = new System.Drawing.Point(0, 0);
            this.m_StatusMessage.Name = "m_StatusMessage";
            this.m_StatusMessage.Size = new System.Drawing.Size(131, 12);
            this.m_StatusMessage.TabIndex = 1;
            this.m_StatusMessage.Text = "[STATUS_MESSAGE]";
            this.m_StatusMessage.DoubleClick += new System.EventHandler(this.OnOpenDevelTools);
            // 
            // m_LoadingProgress
            // 
            this.m_LoadingProgress.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.m_LoadingProgress.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.m_LoadingProgress.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.m_LoadingProgress.HorizontalProgress = false;
            this.m_LoadingProgress.Location = new System.Drawing.Point(0, 549);
            this.m_LoadingProgress.MarqueeSpeed = 5;
            this.m_LoadingProgress.Name = "m_LoadingProgress";
            this.m_LoadingProgress.Progress = 50F;
            this.m_LoadingProgress.ProgressMaximum = 100F;
            this.m_LoadingProgress.ProgressMinimum = 0F;
            this.m_LoadingProgress.ProgressStyle = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.m_LoadingProgress.Size = new System.Drawing.Size(478, 3);
            this.m_LoadingProgress.TabIndex = 2;
            // 
            // ScrMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.m_StatusMessage);
            this.Controls.Add(this.m_LoadingProgress);
            this.Name = "ScrMain";
            this.Size = new System.Drawing.Size(478, 552);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label m_StatusMessage;
        private UI.Forms.ColorizedProgressBar m_LoadingProgress;
    }
}
