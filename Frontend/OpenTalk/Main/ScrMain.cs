using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTalk.UI.CefUnity;
using System.Diagnostics;
using OpenTalk.UI.Extensions;
using OpenTalk.Tasks;
using OpenTalk.Main.Renderers;

namespace OpenTalk.Main
{
    public partial class ScrMain : CefScreen
    {
        private Future m_ProgressDelay;
        private bool m_FirstVisible;

        public ScrMain()
        {
            InitializeComponent();

            // 디버거가 붙은 상태라면 상태 메시지를 보여줍니다.
            m_FirstVisible = true;
            m_StatusMessage.Visible = Debugger.IsAttached;

            if (Router != null)
            {
                Router.Set("/", new MainPage());
                Router.Set("/status.json", new StatusPolling(this));
            }
        }

        /// <summary>
        /// 로딩이 시작되었습니다.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoadStart(EventArgs e)
        {
            lock(this)
            {
                if (m_ProgressDelay != null)
                    m_ProgressDelay.Cancel();

                m_ProgressDelay = null;
            }

            Invoke(new Action(() =>
            {
                m_LoadingProgress.Visible = true;
            }));

            base.OnLoadStart(e);
        }

        /// <summary>
        /// 로딩이 완료되었습니다.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoadEnd(EventArgs e)
        {
            lock (this)
            {
                if (m_ProgressDelay != null)
                    m_ProgressDelay.Cancel();

                m_ProgressDelay = Future.MakeDelay(1000)
                    .Then(() => Invoke(new Action(() =>
                    {
                        m_LoadingProgress.Visible = false;
                    })));
            }

            base.OnLoadEnd(e);
        }

        /// <summary>
        /// 이 스크린이 보이게되면 실행됩니다.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNowVisible(EventArgs e)
        {
            if (!m_FirstVisible)
                ReloadInterface();

            m_FirstVisible = false;
            base.OnNowVisible(e);
        }

        /// <summary>
        /// 창을 닫아 달라는 요청을 받으면, 메인 창을 숨깁니다.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnCloseRequested(CefScreenClosingEventArgs e)
        {
            Invoke(new Action(() => {
                FrmMain MainForm = this.GetParent<FrmMain>(true);

                if (MainForm != null)
                    MainForm.Hide();
            }));

            base.OnCloseRequested(e);
        }

        /// <summary>
        /// 다이얼로그를 보여달라는 요청을 받으면, 
        /// 상태 메시지 인디케이터에 메시지를 복사합니다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnDialogRequested(CefDialogEventArgs e)
        {
            if (e.DialogType != CefDialogType.Prompt)
            {
                // 프롬포트가 아닐 때만,
                // 메시지를 레이블로 카피합니다.
                Invoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(e.Message) ||
                        string.IsNullOrWhiteSpace(e.Message))
                        m_StatusMessage.Text = "디버깅 도구를 열려면 이 메시지를 더블클릭하십시오.";

                    else m_StatusMessage.Text = e.Message;
                }));
            }

            base.OnDialogRequested(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            m_StatusMessage.Text = "디버깅 도구를 열려면 이 메시지를 더블클릭하십시오.";
            base.OnLoad(e);
        }

        /// <summary>
        /// 상태 메시지를 더블클릭하면
        /// 개발자 도구를 보여줍니다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOpenDevelTools(object sender, EventArgs e)
        {
            ShowDevDialog();
        }
    }
}
