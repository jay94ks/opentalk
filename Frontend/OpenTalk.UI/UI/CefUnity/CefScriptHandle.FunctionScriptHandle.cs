using CefSharp;
using CefSharp.WinForms;

namespace OpenTalk.UI.CefUnity
{
    public abstract partial class CefScriptHandle
    {
        /// <summary>
        /// 특정한 자바스크립트 함수를 실행하는 스크립트 핸들입니다.
        /// </summary>
        private class FunctionScriptHandle : CefScriptHandle
        {
            private string m_Function;
            private object[] m_Arguments;

            public FunctionScriptHandle(string Function, params object[] Arguments)
            {
                m_Function = Function;
                m_Arguments = Arguments;
            }

            internal override void OnInvoke(CefScreen Screen, ChromiumWebBrowser Browser)
            {
                if (Browser != null &&
                    !string.IsNullOrEmpty(m_Function) &&
                    !string.IsNullOrWhiteSpace(m_Function))
                    Browser.ExecuteScriptAsync(m_Function, m_Arguments);

                base.OnInvoke(Screen, Browser);
            }
        }
    }
}
