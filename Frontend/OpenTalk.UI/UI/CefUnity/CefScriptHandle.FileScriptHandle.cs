using System.IO;
using System.Text;
using CefSharp;
using CefSharp.WinForms;

namespace OpenTalk.UI.CefUnity
{
    public abstract partial class CefScriptHandle
    {
        private class FileScriptHandle : CefScriptHandle
        {
            private string m_ScriptFile = null;

            /// <summary>
            /// 파일로부터 스크립트 핸들을 초기화합니다.
            /// </summary>
            /// <param name="ScriptFile"></param>
            public FileScriptHandle(string ScriptFile)
                => m_ScriptFile = ScriptFile;

            /// <summary>
            /// 스크립트를 실제로 실행합니다.
            /// </summary>
            /// <param name="Screen"></param>
            /// <param name="Callback"></param>
            internal override void OnInvoke(CefScreen Screen, ChromiumWebBrowser Browser)
            {
                if (Browser != null &&
                    !string.IsNullOrEmpty(m_ScriptFile) &&
                    !string.IsNullOrWhiteSpace(m_ScriptFile) &&
                    File.Exists(m_ScriptFile))
                    Browser.ExecuteScriptAsync(File.ReadAllText(m_ScriptFile, Encoding.UTF8));

                base.OnInvoke(Screen, Browser);
            }
        }
    }
}
