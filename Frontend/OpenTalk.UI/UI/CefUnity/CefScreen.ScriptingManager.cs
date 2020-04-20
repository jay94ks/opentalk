using CefSharp;
using CefSharp.Event;
using OpenTalk.Tasks;
using System;
using System.Collections.Generic;

namespace OpenTalk.UI.CefUnity
{
    public partial class CefScreen
    {
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
            "}" +
            "(async function() {" +
                "await CefSharp.BindObjectAsync(\"cefScreen\"); " +
                "window.close = function() { cefScreen.close(); }" +
            "})();";

        private static readonly string SCRIPT_InvokeInvisible
            = "if (window.__cefVisible == undefined || window.__cefVisible) {" +
                "window.__cefVisible = false;" +
                "if (window.__cefVisibleCb != undefined) {" +
                    "window.__cefVisibleCb(false);" +
                "}" +
            "}";

        private static readonly string SCRIPT_InvokeVisible
            = "if (window.__cefVisible == undefined || !window.__cefVisible) {" +
                "window.__cefVisible = true;" +
                "if (window.__cefVisibleCb != undefined) {" +
                    "window.__cefVisibleCb(false);" +
                "}" +
            "}";


        public class ScriptingManager
        {
            private CefScreen m_Master;
            private Dictionary<string, object> m_Bindings 
                = new Dictionary<string, object>();

            // 매 페이지 로딩시마다 실행시킬 스크립트들입니다.
            // (순차적으로 실행된다는 보장은 없습니다)
            private List<CefScriptHandle> m_Invokes = new List<CefScriptHandle>();

            /// <summary>
            /// 스크립팅 관리자를 초기화합니다.
            /// </summary>
            /// <param name="Master"></param>
            internal ScriptingManager(CefScreen Master)
            {
                (m_Master = Master).Cycles.Instance
                    .Future.Then(OnScriptingInit);

                m_Bindings["cefScreen"] = new ScriptingExtension(m_Master);
            }

            /// <summary>
            /// 매 페이지 마다 실행되는 스크립트들의 핸들을 가져옵니다.
            /// 이 컬렉션에 접근할 땐, 반드시 lock 키워드로 컬렉션을 잠그고,
            /// 접근하십시오. e.g. lock(Handles) Handles.Remove(someHandle);
            /// </summary>
            public IList<CefScriptHandle> Handles => m_Invokes;

            /// <summary>
            /// 스크립팅 관리자를 초기화합니다.
            /// </summary>
            private void OnScriptingInit()
            {
                m_Master.m_Browser.JavascriptObjectRepository
                    .ResolveObject += OnResolve;

                m_Master.m_Browser.FrameLoadStart += OnFrameLoadStart;
                m_Master.m_Browser.FrameLoadEnd += OnFrameLoadEnd;
            }

            /// <summary>
            /// 스크립트 객체 바인딩 요청을 처리합니다.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void OnResolve(object sender, JavascriptBindingEventArgs e)
            {
                lock (m_Bindings)
                {
                    if (m_Bindings.ContainsKey(e.ObjectName))
                    {
                        e.ObjectRepository.Register(e.ObjectName,
                            m_Bindings[e.ObjectName], true);
                    }
                }

                m_Master.Cycles.ScriptReady.Set();
            }

            /// <summary>
            /// 지정된 객체를 바인딩합니다.
            /// 바인딩을 수행한다고 해서 자바스크립트에 즉각 반영되지는 않습니다.
            /// </summary>
            /// <param name="Name"></param>
            /// <param name="Object"></param>
            /// <returns></returns>
            public bool Bind(string Name, object Object)
            {
                lock (m_Bindings)
                {
                    if (m_Bindings.ContainsKey(Name))
                        return false;

                    m_Bindings.Add(Name, Object);
                    m_Master.Cycles.Scripting.Then(
                        () => NoticeBinding(Name));
                }

                return true;
            }

            /// <summary>
            /// 지정된 객체의 바인딩을 제거합니다.
            /// 바인딩을 수행한다고 해서 자바스크립트에 즉각 반영되지는 않습니다.
            /// (언바인딩은 다음 페이지 로드부터 적용됩니다)
            /// </summary>
            /// <param name="Name"></param>
            /// <param name="Object"></param>
            /// <returns></returns>
            public bool Unbind(string Name, object Object)
            {
                lock (m_Bindings)
                {
                    if (!m_Bindings.ContainsKey(Name))
                        return false;

                    m_Bindings.Remove(Name);
                }

                return true;
            }

            /// <summary>
            /// 자바 스크립트를 실행합니다.
            /// </summary>
            /// <param name="Script"></param>
            /// <param name="Permanently">매 페이지 로딩 시 마다 실행되어야 하는지 여부입니다.</param>
            /// <returns></returns>
            public Future<bool> Invoke(CefScriptHandle Handle, bool Permanently = false)
            {
                if (Permanently)
                {
                    lock (m_Invokes)
                        m_Invokes.Add(Handle);

                    if (m_Master.Cycles.Scripting.IsCompleted)
                        return Invoke(Handle, false).Then(() => true);

                    return Future.FromResult(true);
                }

                return m_Master.Cycles.Scripting
                    .Then(() =>
                    {
                        try { Handle.OnInvoke(m_Master, m_Master.m_Browser); }
                        catch { return false; }

                        return true;
                    });
            }

            /// <summary>
            /// 자바 스크립트를 실행합니다.
            /// </summary>
            /// <param name="Script"></param>
            /// <param name="Permanently">매 페이지 로딩 시 마다 실행되어야 하는지 여부입니다.</param>
            /// <returns></returns>
            public Future<bool> Invoke(string Script, bool Permanently = false)
                => Invoke(CefScriptHandle.FromString(Script), Permanently);

            /// <summary>
            /// 자바 스크립트 파일을 실행합니다.
            /// </summary>
            /// <param name="Script"></param>
            /// <param name="Permanently">매 페이지 로딩 시 마다 실행되어야 하는지 여부입니다.</param>
            /// <returns></returns>
            public Future<bool> InvokeFile(string Script, bool Permanently = false)
                => Invoke(CefScriptHandle.FromFile(Script), Permanently);

            /// <summary>
            /// 자바 스크립트 함수를 실행합니다.
            /// </summary>
            /// <param name="Script"></param>
            /// <param name="Permanently">매 페이지 로딩 시 마다 실행되어야 하는지 여부입니다.</param>
            /// <returns></returns>
            public Future<bool> InvokeFunction(string Function, params object[] Arguments)
                => Invoke(CefScriptHandle.FunctionCall(Function, Arguments));

            /// <summary>
            /// 자바스크립트를 실행합니다.
            /// </summary>
            /// <param name="Script"></param>
            /// <returns></returns>
            private bool InvokeRaw(string Script)
            {
                try { m_Master.m_Browser.ExecuteScriptAsync(Script); }
                catch
                {
                    return false;
                }

                return true;
            }

            /// <summary>
            /// 페이지 프레임이 로드되기 시작하면, 로딩 상태와 스크립트 상태를 리셋합니다.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void OnFrameLoadStart(object sender, CefSharp.FrameLoadStartEventArgs e)
            {
                if (e.Frame != null && e.Frame.IsMain)
                {
                    m_Master.Cycles.LoadState.Unset();
                    m_Master.Cycles.ScriptReady.Unset();
                }
            }

            /// <summary>
            /// 페이지 프레임의 로드가 완료되면, 사전 스크립트들을 모두 실행시키고, 
            /// 로딩 상태와 스크립트 상태를 설정합니다.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void OnFrameLoadEnd(object sender, CefSharp.FrameLoadEndEventArgs e)
            {
                if (e.Frame != null && e.Frame.IsMain)
                {
                    // 우선 바인딩 정보를 통보합니다.
                    lock (m_Bindings)
                    {
                        foreach (string Name in m_Bindings.Keys)
                            NoticeBinding(Name);
                    }

                    // 이후, 페이지 준비 콜백 스크립트를 호출하고,
                    InvokeRaw(SCRIPT_InvokeCallback);
                    InvokeRaw(m_Master.Visible ?
                        SCRIPT_InvokeVisible : SCRIPT_InvokeInvisible);

                    // 사전 실행 스크립트들을 모두 실행시킵니다.
                    lock (m_Invokes)
                    {
                        foreach (CefScriptHandle Script in m_Invokes)
                            Script.OnInvoke(m_Master, m_Master.m_Browser);
                    }

                    m_Master.Cycles.LoadState.Set();
                    m_Master.Cycles.ScriptReady.Set();
                }
            }

            /// <summary>
            /// 바인딩 정보를 자바스크립트 쪽에 통보합니다.
            /// </summary>
            /// <param name="Name"></param>
            private void NoticeBinding(string Name)
            {
                InvokeRaw("(async function() { " +
                    "await CefSharp.BindObjectAsync(\"" + Name + "\"); " +
                        "if (window.__cefBindCb != undefined) { " +
                            "window.__cefBindCb(\"" + Name + "\");" +
                        "}" +
                    "})();");
            }
        }
    }
}
