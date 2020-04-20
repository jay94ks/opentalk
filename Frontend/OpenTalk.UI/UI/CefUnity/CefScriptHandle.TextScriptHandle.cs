using System;
using CefSharp;
using CefSharp.WinForms;

namespace OpenTalk.UI.CefUnity
{
    public abstract partial class CefScriptHandle
    {
        private class TextScriptHandle : CefScriptHandle
        {
            private string m_Script = null;

            /// <summary>
            /// 텍스트 스크립트를 초기화합니다.
            /// </summary>
            /// <param name="Script"></param>
            public TextScriptHandle(string Script) => m_Script = Script;

            /// <summary>
            /// 스크립트를 실제로 실행합니다.
            /// </summary>
            /// <param name="Screen"></param>
            /// <param name="Callback"></param>
            internal override void OnInvoke(CefScreen Screen, ChromiumWebBrowser Browser)
            {
                if (Browser != null && 
                    !string.IsNullOrEmpty(m_Script) &&
                    !string.IsNullOrWhiteSpace(m_Script))
                    Browser.ExecuteScriptAsync(m_Script);

                base.OnInvoke(Screen, Browser);
            }
        }
    }
}
