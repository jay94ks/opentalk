using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.UI.CefUnity
{
    /// <summary>
    /// Cef에서 실행되는 자바 스크립트를 추상화합니다.
    /// </summary>
    public abstract partial class CefScriptHandle
    {
        /// <summary>
        /// 문자열로부터 스크립트 핸들을 생성합니다.
        /// </summary>
        /// <param name="Script"></param>
        /// <returns></returns>
        public static CefScriptHandle FromString(string Script)
            => new TextScriptHandle(Script);

        /// <summary>
        /// 지정된 경로로부터 스크립트 핸들을 생성합니다.
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        public static CefScriptHandle FromFile(string Path)
            => new FileScriptHandle(Path);

        /// <summary>
        /// 지정된 자바스크립트 함수를 실행하는 스크립트 핸들을 생성합니다.
        /// </summary>
        /// <param name="Function"></param>
        /// <param name="Arguments"></param>
        /// <returns></returns>
        public static CefScriptHandle FunctionCall(string Function, params object[] Arguments)
            => new FunctionScriptHandle(Function, Arguments);

        /// <summary>
        /// 스크립트 동작을 수행해야 할 때 실행됩니다.
        /// </summary>
        /// <param name="Callback"></param>
        internal virtual void OnInvoke(CefScreen Screen, ChromiumWebBrowser Browser) => OnInvoke(Screen);

        /// <summary>
        /// 스크립트 동작을 수행해야 할 때 실행됩니다.
        /// </summary>
        /// <param name="Screen"></param>
        protected virtual void OnInvoke(CefScreen Screen)
        {

        }
    }
}
