﻿namespace OpenTalk.Main
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
            this.cefScreen = new OpenTalk.UI.CefUnity.CefScreen();
            this.SuspendLayout();
            // 
            // cefScreen
            // 
            this.cefScreen.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cefScreen.Location = new System.Drawing.Point(0, 0);
            this.cefScreen.Name = "cefScreen";
            this.cefScreen.Size = new System.Drawing.Size(478, 552);
            this.cefScreen.TabIndex = 0;
            // 
            // ScrMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.cefScreen);
            this.Name = "ScrMain";
            this.Size = new System.Drawing.Size(478, 552);
            this.ResumeLayout(false);

        }

        #endregion

        private UI.CefUnity.CefScreen cefScreen;
    }
}
