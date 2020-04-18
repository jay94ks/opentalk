using CefSharp;
using CefSharp.WinForms;
using OpenTalk.UI.Extensions;
using OpenTalk.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenTalk.UI.CefUnity
{
    public class CefScreen : Screen
    {
        private ChromiumWebBrowser m_Browser;
        private Action m_TickAction;
        private Timer m_Timer;

        private EventHandlerCollection<CefDialogEventArgs> m_DialogEvent
            = new EventHandlerCollection<CefDialogEventArgs>();

        /// <summary>
        /// Cef Screen을 초기화합니다.
        /// </summary>
        public CefScreen()
        {
            (m_Timer = new Timer() { Interval = 100 })
                .Tick += (X, Y) =>
                {
                    Action Functor;

                    lock (this) {
                        Functor = m_TickAction;
                    }

                    Functor?.Invoke();
                };

            m_Timer.Start();
        }

        /// <summary>
        /// 이 스크린에 보조 컨트롤이 부착될 때 마다 브라우져 컨트롤을 뒤로 옮깁니다.
        /// (사용자 컨트롤이 항상 전면에 위치하도록)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnControlAdded(ControlEventArgs e)
        {
            lock (this)
            {
                try
                {
                    if (m_Browser.IsNotNull())
                        m_Browser.SendToBack();
                }
                catch { }
            }

            base.OnControlAdded(e);
        }

        /// <summary>
        /// 현재 이 코드를 비주얼 스튜디오 내에서 실행하는 중인지 확인합니다.
        /// (디자인 모드인지 아닌지)
        /// </summary>
        /// <returns></returns>
        private bool IsReallyDesignMode() {
            if (!DesignMode)
            {
                Process Self = null;

                // 최근, 일부 윈도우즈 버젼들에서,
                // 보안 권한 문제로 프로세스 정보를 받아오지 못하는
                // 경우가 종종 발생된다고 합니다.
                // 따라서 try ... catch ...로 감쌌습니다.

                try { Self = Process.GetCurrentProcess(); }
                catch { }

                if (Self != null)
                {
                    switch (Self.ProcessName.ToLower())
                    {
                        case "xdesproc":
                        case "devenv":
                            return true;

                        default:
                            break;
                    }
                }
            }

            return DesignMode;
        }

        /// <summary>
        /// CEF 스크린이 로드될 때 실행됩니다.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            lock (this)
            {
                if (m_Browser.IsNull() &&
                    !IsReallyDesignMode())
                {
                    m_Browser = new ChromiumWebBrowser(
                        new CefSharp.Web.HtmlString(""))
                    {
                        Dock = DockStyle.Fill
                    };

                    OnPreConfigureCef(m_Browser.BrowserSettings);
                    OnConfigureCef(m_Browser.BrowserSettings);
                    OnPostConfigureCef(m_Browser.BrowserSettings);

                    /*
                        디버깅시엔, 보안 설정이 고정됩니다.
                     */
                    if (!Debugger.IsAttached)
                    {
                        m_Browser.BrowserSettings.WebSecurity = CefState.Enabled;
                        m_Browser.BrowserSettings.ApplicationCache = CefState.Disabled;
                    }

                    m_Browser.JsDialogHandler = new JsDialogHandler(this);

                    // 컨트롤을 추가합니다.
                    Controls.Add(m_Browser);
                    
                    // 브라우저 컨트롤을 맨 뒤로 보냅니다.
                    while (true)
                    {
                        int Index = Controls.GetChildIndex(m_Browser);

                        if (Index < 0)
                            break;

                        if (Index != Controls.Count - 1)
                            m_Browser.SendToBack();

                        else break;
                    }
                }
            }

            base.OnLoad(e);
        }

        /// <summary>
        /// 자바스크립트 다이얼로그 핸들러를 커스터마이징합니다.
        /// </summary>
        private class JsDialogHandler : IJsDialogHandler
        {
            private CefScreen m_Master;

            /// <summary>
            /// 커스텀 핸들러를 초기화합니다.
            /// </summary>
            /// <param name="master"></param>
            public JsDialogHandler(CefScreen master) => m_Master = master;

            /// <summary>
            /// 정말 페이지를 이탈하시겠습니까? 다이얼로그.
            /// </summary>
            /// <param name="chromiumWebBrowser"></param>
            /// <param name="browser"></param>
            /// <param name="messageText"></param>
            /// <param name="isReload"></param>
            /// <param name="callback"></param>
            /// <returns></returns>
            public bool OnBeforeUnloadDialog(IWebBrowser chromiumWebBrowser, IBrowser browser,
                string messageText, bool isReload, IJsDialogCallback callback)
            {
                if (!m_Master.m_DialogEvent.Broadcast(m_Master,
                    new CefDialogEventArgs(m_Master, CefDialogType.Confirm,
                    callback, messageText)))
                {
                    Form Form = m_Master.GetParent<Form>();
                    DialogResult Result = DialogResult.Yes;
                    string Title = "CefScreen";

                    if (Form.IsNotNull())
                        Title = Form.Text;

                    Result = Application.Tasks.Invoke(() => MessageBox.Show(messageText,
                        Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                        .WaitResult();

                    if (Result == DialogResult.Yes)
                        callback.Continue(true);

                    else callback.Continue(false);
                }

                return true;
            }

            /// <summary>
            /// 자바스크립트 다이얼로그를 요청합니다.
            /// </summary>
            /// <param name="chromiumWebBrowser"></param>
            /// <param name="browser"></param>
            /// <param name="originUrl"></param>
            /// <param name="dialogType"></param>
            /// <param name="messageText"></param>
            /// <param name="defaultPromptText"></param>
            /// <param name="callback"></param>
            /// <param name="suppressMessage"></param>
            /// <returns></returns>
            public bool OnJSDialog(IWebBrowser a, IBrowser b, string c,
                CefJsDialogType dialogType, string messageText, string defaultPromptText, 
                IJsDialogCallback callback, ref bool d)
            {
                CefDialogType requestType = CefDialogType.Alert;

                switch (dialogType)
                {
                    case CefJsDialogType.Alert:
                        requestType = CefDialogType.Alert;
                        break;

                    case CefJsDialogType.Confirm:
                        requestType = CefDialogType.Confirm;
                        break;

                    case CefJsDialogType.Prompt:
                        requestType = CefDialogType.Prompt;
                        break;

                    default:
                        break;
                }

                if (string.IsNullOrEmpty(defaultPromptText) ||
                    string.IsNullOrWhiteSpace(defaultPromptText))
                    defaultPromptText = null;

                if (!m_Master.m_DialogEvent.Broadcast(m_Master,
                    new CefDialogEventArgs(m_Master, requestType,
                    callback, messageText, defaultPromptText)))
                {
                    Form Form = m_Master.GetParent<Form>();
                    DialogResult Result = DialogResult.Yes;
                    string Title = "CefScreen";
                    
                    if (Form.IsNotNull())
                        Title = Form.Text;

                    switch (dialogType)
                    {
                        case CefJsDialogType.Alert:
                            Result = Application.Tasks.Invoke(() => MessageBox.Show(messageText,
                                Title, MessageBoxButtons.OK, MessageBoxIcon.Question))
                                .WaitResult();
                            break;

                        case CefJsDialogType.Confirm:
                            Result = Application.Tasks.Invoke(() => MessageBox.Show(messageText,
                                Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                                .WaitResult();
                            break;

                        case CefJsDialogType.Prompt:
                            Result = DialogResult.No;
                            break;

                        default:
                            break;
                    }

                    if (Result == DialogResult.Yes ||
                        Result == DialogResult.OK)
                        callback.Continue(true);

                    else callback.Continue(false);
                }

                return true;
            }

            public void OnDialogClosed(IWebBrowser chromiumWebBrowser, IBrowser browser) { }
            public void OnResetDialogState(IWebBrowser chromiumWebBrowser, IBrowser browser) { }
        }

        /// <summary>
        /// 기본적으로 셋팅되는 값들을 설정합니다.
        /// </summary>
        /// <param name="Cef"></param>
        private void OnPreConfigureCef(IBrowserSettings Cef)
        {
            Cef.Javascript = CefState.Enabled;
            Cef.JavascriptDomPaste = CefState.Enabled;
        }

        /// <summary>
        /// 무슨 일이 있어도 바뀌어선 안되는 값들을 설정합니다.
        /// </summary>
        /// <param name="Cef"></param>
        private void OnPostConfigureCef(IBrowserSettings Cef)
        {
            /*
                Switcher에 부착된 CefScreen은 자바스크립트로 닫을 수 없습니다.
             */
            if (Switcher != null)
            {
                Cef.JavascriptCloseWindows = CefState.Disabled;
            }

            Cef.Javascript = CefState.Enabled;
            Cef.LocalStorage = CefState.Enabled;
            Cef.FileAccessFromFileUrls = CefState.Enabled;
            Cef.UniversalAccessFromFileUrls = CefState.Enabled;
            Cef.ImageLoading = CefState.Enabled;
            Cef.Databases = CefState.Enabled;
        }

        /// <summary>
        /// 브라우져 설정을 초기화해야 할 때 실행됩니다.
        /// </summary>
        /// <param name="settings"></param>
        protected virtual void OnConfigureCef(IBrowserSettings Cef)
        {
        }

        /// <summary>
        /// 이 CEF 스크린이 보여지게 되면 실행됩니다.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNowVisible(EventArgs e)
        {
            base.OnNowVisible(e);
        }

        /// <summary>
        /// 이 CEF 스크린이 보여지지 않게 되면 실행됩니다.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNowInvisible(EventArgs e)
        {
            base.OnNowInvisible(e);
        }
    }
}
