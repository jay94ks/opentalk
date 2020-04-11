namespace OpenTalk
{
    partial class FrmMain
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

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.m_TrayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.m_TrayMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.m_TrayMenuOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.m_TrayMenuAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.m_TrayMenuLockMode = new System.Windows.Forms.ToolStripMenuItem();
            this.m_TrayMenuLogout = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.m_TrayMenuExit = new System.Windows.Forms.ToolStripMenuItem();
            this.m_ScreenSwitcher = new OpenTalk.UI.ScreenSwitcher();
            this.m_TrayMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_TrayIcon
            // 
            this.m_TrayIcon.ContextMenuStrip = this.m_TrayMenu;
            this.m_TrayIcon.Visible = true;
            this.m_TrayIcon.DoubleClick += new System.EventHandler(this.OnShowRequested);
            // 
            // m_TrayMenu
            // 
            this.m_TrayMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_TrayMenuOpen,
            this.m_TrayMenuAbout,
            this.m_TrayMenuLockMode,
            this.m_TrayMenuLogout,
            this.toolStripMenuItem1,
            this.m_TrayMenuExit});
            this.m_TrayMenu.Name = "m_TrayMenu";
            this.m_TrayMenu.Size = new System.Drawing.Size(151, 120);
            // 
            // m_TrayMenuOpen
            // 
            this.m_TrayMenuOpen.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.m_TrayMenuOpen.Name = "m_TrayMenuOpen";
            this.m_TrayMenuOpen.Size = new System.Drawing.Size(150, 22);
            this.m_TrayMenuOpen.Text = "열기";
            this.m_TrayMenuOpen.Click += new System.EventHandler(this.OnShowRequested);
            // 
            // m_TrayMenuAbout
            // 
            this.m_TrayMenuAbout.Name = "m_TrayMenuAbout";
            this.m_TrayMenuAbout.Size = new System.Drawing.Size(150, 22);
            this.m_TrayMenuAbout.Text = "오픈톡 정보";
            this.m_TrayMenuAbout.Click += new System.EventHandler(this.OnAboutRequested);
            // 
            // m_TrayMenuLockMode
            // 
            this.m_TrayMenuLockMode.Name = "m_TrayMenuLockMode";
            this.m_TrayMenuLockMode.Size = new System.Drawing.Size(150, 22);
            this.m_TrayMenuLockMode.Text = "잠금모드 설정";
            this.m_TrayMenuLockMode.Click += new System.EventHandler(this.OnLockModeRequested);
            // 
            // m_TrayMenuLogout
            // 
            this.m_TrayMenuLogout.Name = "m_TrayMenuLogout";
            this.m_TrayMenuLogout.Size = new System.Drawing.Size(150, 22);
            this.m_TrayMenuLogout.Text = "로그아웃";
            this.m_TrayMenuLogout.Click += new System.EventHandler(this.OnLogoutRequested);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(147, 6);
            // 
            // m_TrayMenuExit
            // 
            this.m_TrayMenuExit.Name = "m_TrayMenuExit";
            this.m_TrayMenuExit.Size = new System.Drawing.Size(150, 22);
            this.m_TrayMenuExit.Text = "종료";
            this.m_TrayMenuExit.Click += new System.EventHandler(this.OnExitRequested);
            // 
            // m_ScreenSwitcher
            // 
            this.m_ScreenSwitcher.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_ScreenSwitcher.Location = new System.Drawing.Point(0, 0);
            this.m_ScreenSwitcher.Name = "m_ScreenSwitcher";
            this.m_ScreenSwitcher.ScreenTypes = new System.Type[0];
            this.m_ScreenSwitcher.Size = new System.Drawing.Size(398, 549);
            this.m_ScreenSwitcher.TabIndex = 1;
            this.m_ScreenSwitcher.VisibleScreenType = null;
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(398, 549);
            this.Controls.Add(this.m_ScreenSwitcher);
            this.Name = "FrmMain";
            this.Text = "OpenTalk";
            this.m_TrayMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon m_TrayIcon;
        private System.Windows.Forms.ContextMenuStrip m_TrayMenu;
        private System.Windows.Forms.ToolStripMenuItem m_TrayMenuOpen;
        private System.Windows.Forms.ToolStripMenuItem m_TrayMenuAbout;
        private System.Windows.Forms.ToolStripMenuItem m_TrayMenuLockMode;
        private System.Windows.Forms.ToolStripMenuItem m_TrayMenuLogout;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem m_TrayMenuExit;
        private OpenTalk.UI.ScreenSwitcher m_ScreenSwitcher;
    }
}

