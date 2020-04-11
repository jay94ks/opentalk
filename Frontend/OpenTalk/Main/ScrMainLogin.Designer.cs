namespace OpenTalk.Main
{
    partial class ScrMainLogin
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
            this.m_LoginBox = new System.Windows.Forms.Panel();
            this.m_ChkAutoLogin = new System.Windows.Forms.CheckBox();
            this.m_ChkRemember = new System.Windows.Forms.CheckBox();
            this.linkLabel3 = new System.Windows.Forms.LinkLabel();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.button1 = new System.Windows.Forms.Button();
            this.m_TxtPassword = new OpenTalk.UI.TextBoxWithPlaceholder();
            this.m_TxtIdentifier = new OpenTalk.UI.TextBoxWithPlaceholder();
            this.m_LoginBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_LoginBox
            // 
            this.m_LoginBox.BackgroundImage = global::OpenTalk.Properties.Resources.login_bg;
            this.m_LoginBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.m_LoginBox.Controls.Add(this.m_ChkAutoLogin);
            this.m_LoginBox.Controls.Add(this.m_ChkRemember);
            this.m_LoginBox.Controls.Add(this.linkLabel3);
            this.m_LoginBox.Controls.Add(this.linkLabel2);
            this.m_LoginBox.Controls.Add(this.linkLabel1);
            this.m_LoginBox.Controls.Add(this.button1);
            this.m_LoginBox.Controls.Add(this.m_TxtPassword);
            this.m_LoginBox.Controls.Add(this.m_TxtIdentifier);
            this.m_LoginBox.Location = new System.Drawing.Point(88, 119);
            this.m_LoginBox.Name = "m_LoginBox";
            this.m_LoginBox.Size = new System.Drawing.Size(330, 275);
            this.m_LoginBox.TabIndex = 0;
            // 
            // m_ChkAutoLogin
            // 
            this.m_ChkAutoLogin.AutoSize = true;
            this.m_ChkAutoLogin.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.m_ChkAutoLogin.Location = new System.Drawing.Point(195, 148);
            this.m_ChkAutoLogin.Name = "m_ChkAutoLogin";
            this.m_ChkAutoLogin.Size = new System.Drawing.Size(88, 16);
            this.m_ChkAutoLogin.TabIndex = 7;
            this.m_ChkAutoLogin.Text = "자동 로그인";
            this.m_ChkAutoLogin.UseVisualStyleBackColor = true;
            // 
            // m_ChkRemember
            // 
            this.m_ChkRemember.AutoSize = true;
            this.m_ChkRemember.Location = new System.Drawing.Point(46, 148);
            this.m_ChkRemember.Name = "m_ChkRemember";
            this.m_ChkRemember.Size = new System.Drawing.Size(142, 16);
            this.m_ChkRemember.TabIndex = 6;
            this.m_ChkRemember.Text = "이메일/전화번호 기억";
            this.m_ChkRemember.UseVisualStyleBackColor = true;
            this.m_ChkRemember.CheckedChanged += new System.EventHandler(this.OnRememberAccount);
            // 
            // linkLabel3
            // 
            this.linkLabel3.AutoSize = true;
            this.linkLabel3.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.linkLabel3.Location = new System.Drawing.Point(44, 239);
            this.linkLabel3.Name = "linkLabel3";
            this.linkLabel3.Size = new System.Drawing.Size(173, 12);
            this.linkLabel3.TabIndex = 5;
            this.linkLabel3.TabStop = true;
            this.linkLabel3.Text = "이용약관 및 개인정보취급 약관";
            this.linkLabel3.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnGotoReadAgreements);
            // 
            // linkLabel2
            // 
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.linkLabel2.Location = new System.Drawing.Point(44, 209);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(81, 12);
            this.linkLabel2.TabIndex = 4;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "패스워드 분실";
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnGotoLostPassword);
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.linkLabel1.Location = new System.Drawing.Point(44, 190);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(85, 12);
            this.linkLabel1.TabIndex = 3;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "새 계정 만들기";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnGotoRegister);
            // 
            // button1
            // 
            this.button1.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.button1.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
            this.button1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Location = new System.Drawing.Point(193, 190);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(90, 31);
            this.button1.TabIndex = 2;
            this.button1.Text = "로그인";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.OnPerformLogin);
            // 
            // m_TxtPassword
            // 
            this.m_TxtPassword.Font = new System.Drawing.Font("굴림", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.m_TxtPassword.ForeColor = System.Drawing.Color.DarkGray;
            this.m_TxtPassword.Location = new System.Drawing.Point(46, 112);
            this.m_TxtPassword.Name = "m_TxtPassword";
            this.m_TxtPassword.Placeholder = "패스워드";
            this.m_TxtPassword.Size = new System.Drawing.Size(237, 27);
            this.m_TxtPassword.TabIndex = 1;
            this.m_TxtPassword.Text = "패스워드";
            // 
            // m_TxtIdentifier
            // 
            this.m_TxtIdentifier.Font = new System.Drawing.Font("굴림", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.m_TxtIdentifier.ForeColor = System.Drawing.Color.DarkGray;
            this.m_TxtIdentifier.Location = new System.Drawing.Point(46, 79);
            this.m_TxtIdentifier.Name = "m_TxtIdentifier";
            this.m_TxtIdentifier.Placeholder = "이메일 혹은 전화번호";
            this.m_TxtIdentifier.Size = new System.Drawing.Size(237, 27);
            this.m_TxtIdentifier.TabIndex = 0;
            this.m_TxtIdentifier.Text = "이메일 혹은 전화번호";
            // 
            // ScrMainLogin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.m_LoginBox);
            this.Name = "ScrMainLogin";
            this.Size = new System.Drawing.Size(529, 505);
            this.m_LoginBox.ResumeLayout(false);
            this.m_LoginBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel m_LoginBox;
        private UI.TextBoxWithPlaceholder m_TxtIdentifier;
        private UI.TextBoxWithPlaceholder m_TxtPassword;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.LinkLabel linkLabel3;
        private System.Windows.Forms.CheckBox m_ChkAutoLogin;
        private System.Windows.Forms.CheckBox m_ChkRemember;
    }
}
