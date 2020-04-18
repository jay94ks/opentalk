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
using static OpenTalk.UI.CefUnity.CefComponent;

namespace OpenTalk.UI.CefUnity
{
    public partial class CefScreen : Screen
    {
        private ChromiumWebBrowser m_Browser;
        private Action m_TickAction;
        private Timer m_Timer;

        private EventHandlerCollection<CefDialogEventArgs> m_DialogEvent
            = new EventHandlerCollection<CefDialogEventArgs>();

        private StateTaskSource m_BrowserInitState = new StateTaskSource();
        private StateTaskSource m_BrowserLoadingState = new StateTaskSource();
        private StateTaskSource m_BrowserJSObjectState = new StateTaskSource();
        private StateTaskSource<string> m_RegistrationReady = new StateTaskSource<string>();
        private Dictionary<string, object> m_JSObjects = new Dictionary<string, object>();

        /// <summary>
        /// 매 페이지 로딩시마다 주입되는 코드입니다.
        /// 페이지 로드가 완료되고, 스크립팅 준비가 끝나면 
        /// 브라우져 내에서 __cefScreenRd 값을 true로 설정하고,
        /// __cefScreenCb 함수를 실행합니다.
        /// 
        /// 즉, 사용자는 __cefScreenRd 값이 true가 아닐 때, 
        /// __cefScreenCb 콜백을 설정함으로서 로딩 완료 여부를 추적할 수 있습니다.
        /// 
        /// 또, window.close 함수의 실제 동작을 "제거"합니다.
        /// </summary>
        private static readonly string SCRIPT_InvokeCallback 
            = "window.__cefScreenRd = true;" +
            "window.close = function() { };" +
            "if (window.__cefScreenCb != undefined) {" +
            "window.__cefScreenCb();" +
            "}";

        private static readonly string SCRIPT_InvokeInvisible
            = "window.__cefVisible = false;" +
            "if (window.__cefVisibleCb != undefined) {" +
            "window.__cefVisibleCb(false);" +
            "}";

        private static readonly string SCRIPT_InvokeVisible
            = "window.__cefVisible = true;" +
            "if (window.__cefVisibleCb != undefined) {" +
            "window.__cefVisibleCb(true);" +
            "}";

        /// <summary>
        /// Cef Screen을 초기화합니다.
        /// </summary>
        public CefScreen()
        {
            Router = new InterfaceRouter();
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
        /// 스크린 식별자입니다.
        /// </summary>
        internal string ScreenId { get; set; }

        /// <summary>
        /// 인터페이스 라우터입니다.
        /// </summary>
        public InterfaceRouter Router { get; private set; }

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
                m_TickAction = InitializeBrowserInstanceAsync;
            }

            base.OnLoad(e);
        }

        /// <summary>
        /// 브라우져 인스턴스를 비동기적으로 초기화합니다.
        /// </summary>
        private void InitializeBrowserInstanceAsync()
        {
            CefComponent cefComponent = CefComponent.GetCEFComponent();

            if (cefComponent.IsNotNull())
            {
                if (m_Browser.IsNull() && !IsReallyDesignMode() &&
                    cefComponent.RegisterScreen(this))
                {
                    InitializeBrowserInstance();

                    lock (this)
                    {
                        m_TickAction = null;
                    }
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
            => m_RegistrationReady.Set(ScreenId = screenId);

        /// <summary>
        /// 스크린이 등록 해제되면 실행되는 메서드입니다.
        /// </summary>
        internal void OnScreenUnregistered()
        {
            m_RegistrationReady.Unset();
            ScreenId = null;
        }

        /// <summary>
        /// 지정된 인터페이스를 로드하고,
        /// 로딩이 시작되면 완료되는 작업을 반환합니다.
        /// 단, 그 성공/실패 여부는 추적할 수 없습니다.
        /// </summary>
        /// <param name="InterfaceId"></param>
        public Task LoadInterface(string InterfaceId)
        {
            Task Task = new CombinedTaskSource<object>(
                () =>
                {
                    Application.Tasks.Invoke(
                        () => m_Browser.Load(string.Format("otk://" + ScreenId + "/" + InterfaceId))
                    ).Wait();

                    return this;
                },
                m_BrowserInitState.Task, m_BrowserLoadingState.Task,
                m_RegistrationReady.Task).Task;

            return Task;
        }

        /// <summary>
        /// 지정된 객체를 자바스크립트 객체로 바인드시킵니다.
        /// 단, 그 실행완료 여부는 추적할 수 없습니다.
        /// </summary>
        /// <param name="ObjectName"></param>
        /// <param name="Object"></param>
        /// <returns></returns>
        public Task<bool> Bind(string ObjectName, object Object)
        {
            return m_BrowserJSObjectState.Task.ContinueOnMessageLoop((X) =>
            {
                lock (m_JSObjects)
                {
                    if (m_JSObjects.ContainsKey(ObjectName))
                        return false;

                    m_JSObjects.Add(ObjectName, Object);
                }

                ExecuteScript(string.Format(
                    "if (window.__cefBindCb != undefined) {" +
                    "window.__cefBindCb(\"{0}\", \"BIND\");" +
                    "}", ObjectName));

                return true;
            });
        }

        /// <summary>
        /// 지정된 객체 바인딩을 제거합니다.
        /// 단, 그 실행완료 여부는 추적할 수 없습니다.
        /// </summary>
        /// <param name="ObjectName"></param>
        /// <param name="Object"></param>
        /// <returns></returns>
        public Task<bool> Unbind(string ObjectName, object Object)
        {
            return m_BrowserJSObjectState.Task.ContinueOnMessageLoop((X) =>
            {
                lock (m_JSObjects)
                {
                    if (m_JSObjects.ContainsKey(ObjectName))
                        return false;

                    m_JSObjects.Add(ObjectName, Object);
                }

                ExecuteScript(string.Format(
                    "if (window.__cefBindCb != undefined) {" +
                    "window.__cefBindCb(\"{0}\", \"UNBIND\");" +
                    "}", ObjectName));

                return true;
            });
        }

        /// <summary>
        /// 지정된 자바스크립트를 브라우져 내에서 실행시킵니다.
        /// 단, 그 실행완료 여부는 추적할 수 없습니다.
        /// </summary>
        /// <param name="Scripts"></param>
        /// <returns></returns>
        public Task ExecuteScript(string Scripts)
        {
            return m_BrowserLoadingState.Task.ContinueOnMessageLoop(
                (X) => m_Browser.ExecuteScriptAsync(Scripts));
        }

        /// <summary>
        /// 지정된 자바스크립트 함수를 브라우져 내에서 실행시킵니다.
        /// 단, 그 실행완료 여부는 추적할 수 없습니다.
        /// </summary>
        /// <param name="Scripts"></param>
        /// <returns></returns>
        public Task ExecuteScript(string Function, params object[] Arguments)
        {
            return m_BrowserLoadingState.Task.ContinueOnMessageLoop(
                (X) => m_Browser.ExecuteScriptAsync(Function, Arguments));
        }

        /// <summary>
        /// CEF 브라우져 인스턴스를 초기화시킵니다.
        /// </summary>
        private void InitializeBrowserInstance()
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

            // 브라우져의 초기화 상태를 Task 객체화 합니다.
            m_Browser.IsBrowserInitializedChanged += (X, Y) =>
            {
                if (m_Browser.IsBrowserInitialized)
                    m_BrowserInitState.Set(m_Browser);

                else m_BrowserInitState.Unset();
            };

            // 메인 프레임 로드 시작/종료 이벤트를 Task 객체화합니다.
            m_Browser.FrameLoadStart += (X, Y) =>
            {
                if (Y.Frame.IsNotNull() && Y.Frame.IsMain)
                {
                    m_BrowserLoadingState.Unset();
                    m_BrowserJSObjectState.Unset();
                }
            };

            m_Browser.FrameLoadEnd += (X, Y) =>
            {
                if (Y.Frame.IsNotNull() && Y.Frame.IsMain)
                {
                    if (SCRIPT_InvokeCallback.Length > 0)
                    {
                        m_Browser.ExecuteScriptAsync(SCRIPT_InvokeCallback);
                        m_Browser.ExecuteScriptAsync(Visible ? SCRIPT_InvokeVisible : SCRIPT_InvokeInvisible);
                    }

                    m_BrowserLoadingState.Set(m_Browser);
                    m_BrowserJSObjectState.Set(true);

                    try
                    {
                        if (Debugger.IsAttached)
                            m_Browser.ShowDevTools();
                    }
                    catch { }
                }
            };

            m_Browser.JavascriptObjectRepository.ResolveObject += OnResolveJSObject;
            //m_Browser.life

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
        /// 자바스크립트 객체를 바인딩 해야 할 때 실행되는 메서드입니다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnResolveJSObject(object sender, CefSharp.Event.JavascriptBindingEventArgs e)
        {
            m_BrowserJSObjectState.Set(true);

            lock (m_JSObjects)
            {
                if (m_JSObjects.ContainsKey(e.ObjectName))
                {
                    e.ObjectRepository.Register(e.ObjectName, m_JSObjects[e.ObjectName], true);
                }
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
            ExecuteScript(SCRIPT_InvokeVisible);
            base.OnNowVisible(e);
        }

        /// <summary>
        /// 이 CEF 스크린이 보여지지 않게 되면 실행됩니다.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNowInvisible(EventArgs e)
        {
            ExecuteScript(SCRIPT_InvokeInvisible);
            base.OnNowInvisible(e);
        }
    }
}
