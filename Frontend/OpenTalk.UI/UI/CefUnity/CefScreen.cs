using CefSharp;
using CefSharp.WinForms;
using OpenTalk.Tasks;
using OpenTalk.UI.Extensions;
using OpenTalk.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static OpenTalk.UI.CefUnity.CefComponent;

namespace OpenTalk.UI.CefUnity
{
    public partial class CefScreen : Screen
    {
        private ChromiumWebBrowser m_Browser;

        private EventHandlerCollection<CefDialogEventArgs> m_DialogEvent
            = new EventHandlerCollection<CefDialogEventArgs>();

        /// <summary>
        /// Cef Screen을 초기화합니다.
        /// </summary>
        public CefScreen()
        {
            Cycles = new LifeCycle(this);
            Router = new InterfaceRouter();
            Scripting = new ScriptingManager(this);
        }

        /// <summary>
        /// 스크린 식별자입니다.
        /// </summary>
        internal string ScreenId { get; set; }

        /// <summary>
        /// 인터페이스 라우터입니다.
        /// </summary>
        public InterfaceRouter Router { get; private set; }

        /// <summary>
        /// 프로그래밍 가능한 라이프 사이클 이벤트들 입니다.
        /// </summary>
        public LifeCycle Cycles { get; private set; }

        /// <summary>
        /// 스크립트 바인딩 관리자입니다.
        /// </summary>
        public ScriptingManager Scripting { get; private set; }

        /// <summary>
        /// 이 스크린을 닫아 달라는 요청을 받으면 실행됩니다.
        /// </summary>
        public event EventHandler CloseRequested;

        /// <summary>
        /// 이 스크린을 닫아 달라는 요청을 받으면 실행됩니다.
        /// (ScriptExtension에서 호출함)
        /// </summary>
        internal void OnCloseRequested()
        {
            Future.RunForUI(() => CloseRequested?.Invoke(this, EventArgs.Empty));
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
            // CEF 컴포넌트가 초기화에 성공하면 브라우져 인스턴스를 준비시킵니다.
            Future.IfMet(() => GetCEFComponent() != null,
                () => Future.RunForUI(InitializeBrowserInstance));

            base.OnLoad(e);
        }

        /// <summary>
        /// 브라우져 인스턴스를 비동기적으로 초기화합니다.
        /// </summary>
        private void InitializeBrowserInstance()
        {
            CefComponent cefComponent = GetCEFComponent();

            if (cefComponent.IsNotNull())
            {
                if (m_Browser.IsNull() && !IsReallyDesignMode() &&
                    cefComponent.RegisterScreen(this))
                {
                    BootstrapBrowserInstance();
                }
            }
        }

        /// <summary>
        /// 이 컨트롤이 파괴될 때 스크린을 등록 해제합니다.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            CefComponent cefComponent = GetCEFComponent(null, false);

            if (disposing && cefComponent.IsNotNull())
                cefComponent.UnregisterScreen(this);

            base.Dispose(disposing);
        }

        /// <summary>
        /// 스크린이 등록되면 실행되는 메서드입니다.
        /// </summary>
        /// <param name="screenId"></param>
        internal void OnScreenRegistered(string screenId)
            => Cycles.Registration.Set(ScreenId = screenId);

        /// <summary>
        /// 스크린이 등록 해제되면 실행되는 메서드입니다.
        /// </summary>
        internal void OnScreenUnregistered()
        {
            Cycles.Registration.Unset();
            ScreenId = null;
        }

        /// <summary>
        /// 지정된 인터페이스를 로드하고,
        /// 로딩이 시작되면 완료되는 작업을 반환합니다.
        /// 단, 그 성공/실패 여부는 추적할 수 없습니다.
        /// </summary>
        /// <param name="InterfaceId"></param>
        public Future LoadInterface(string InterfaceId)
        {
            return Cycles.Ready.Then((X) =>
            {
                Future.RunForUI(() =>
                {
                    m_Browser.Load(string.Format(
                        "otk://" + ScreenId + "/" + InterfaceId));
                }).Wait();
            });
        }

        /// <summary>
        /// CEF 브라우져 인스턴스를 초기화시킵니다.
        /// </summary>
        private void BootstrapBrowserInstance()
        {
            m_Browser = new ChromiumWebBrowser("otk://" + ScreenId + "/")
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
            m_Browser.MenuHandler = new EmptyContextMenuHandler();

            // 브라우져의 초기화 상태를 Task 객체화 합니다.
            m_Browser.IsBrowserInitializedChanged += (X, Y) =>
            {
                if (m_Browser.IsBrowserInitialized)
                    Cycles.Initialization.Set();

                else Cycles.Initialization.Unset();
            };

            // 브라우저 인스턴스를 준비 상태로 만듭니다.
            Cycles.Instance.SetCompleted();
            m_Browser.FrameLoadEnd += (X, Y) =>
            {
                if (Y.Frame.IsNotNull() && Y.Frame.IsMain)
                {
                    try
                    {
                        if (Debugger.IsAttached)
                            m_Browser.ShowDevTools();
                    }
                    catch { }
                }
            };

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
        /// HTML UI 렌더링 요청을 받으면 실행됩니다.
        /// </summary>
        /// <param name="Request"></param>
        internal Task HandleRequestAsync(OtkSchemeHandler Context, IRequest Request)
        {
            return Task.Run(() => {
                CefContent Content = null;
                CefContentRenderer Renderer = null;
                string QueryString = "";

                string RequestedUri = Request.Url.Substring(
                    OtkSchemeHandler.m_Scheme.Length).Split(new char[] { '/' }, 2)[1];


                if (RequestedUri.Contains("?"))
                {
                    int Offset = RequestedUri.IndexOf('?');

                    QueryString = Offset > 0 ? RequestedUri.Substring(Offset + 1).Trim() : null;
                    RequestedUri = Uri.UnescapeDataString(RequestedUri.Substring(0, Offset)).Trim();
                }

                Renderer = Router.Route(this, RequestedUri);
                if (Renderer != null)
                {
                    string Method = (Request.Method != null ? Request.Method : "GET").ToUpper();
                    
                    if (Method != "POST" && Method != "PUT")
                    {
                        Dictionary<string, byte[]> FormData
                            = new Dictionary<string, byte[]>();

                        if (Request.PostData != null)
                        {
                            foreach(var Data in Request.PostData.Elements)
                            {
                                if (Data.File != null)
                                    FormData[Data.File] = Data.Bytes;
                            }
                        }
                        
                        Content = Renderer.Render(
                            Method, QueryString, FormData, Method == "GET",
                            Request.Flags.HasFlag(UrlRequestFlags.OnlyFromCache));
                    }

                    else Content = Renderer.Render(Method, QueryString, 
                        Method == "GET", Request.Flags.HasFlag(UrlRequestFlags.OnlyFromCache));
                }

                PutContentToContext(Context, Content != null ? 
                    Content : CefContentRenderer.MakeError());
            });
        }

        /// <summary>
        /// 그런 컨텐트 없음.
        /// </summary>
        /// <param name="Context"></param>
        /// <param name="Content"></param>
        private static void PutContentToContext(OtkSchemeHandler Context, CefContent Content)
        {
            Context.StatusCode = Content.StatusCode;
            Context.StatusText = Content.StatusMessage;

            if (Content.Charset != null)
                Context.Charset = Content.Charset.WebName;

            Context.MimeType = Content.MimeType;
            Context.Stream = Content.Content;

            Context.AutoDisposeStream = !Content.NoDisposeContent;

            if (Content.Headers != null &&
                Content.Headers.Length > 0)
            {
                foreach (var Header in Content.Headers)
                    Context.Headers.Set(Header.Key, Header.Value);
            }
        }

        /// <summary>
        /// 이 CEF 스크린이 보여지게 되면 실행됩니다.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNowVisible(EventArgs e)
        {
            Scripting.Invoke(SCRIPT_InvokeVisible);
            base.OnNowVisible(e);
        }

        /// <summary>
        /// 이 CEF 스크린이 보여지지 않게 되면 실행됩니다.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNowInvisible(EventArgs e)
        {
            Scripting.Invoke(SCRIPT_InvokeInvisible);
            base.OnNowInvisible(e);
        }
    }
}
