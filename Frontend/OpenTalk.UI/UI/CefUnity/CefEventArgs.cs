using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.UI.CefUnity
{
    public class CefEventArgs : EventArgs
    {
        /// <summary>
        /// 아무런 이벤트가 없지만, 발생시켜야만 할 때 사용합니다.
        /// </summary>
        public static new CefEventArgs Empty { get; private set; } 
            = new CefEventArgs();

        /// <summary>
        /// Cef 스크린 이벤트 인자의 기본 클래스입니다.
        /// </summary>
        /// <param name="Screen"></param>
        public CefEventArgs() { }

        /// <summary>
        /// Cef 스크린 이벤트 인자의 기본 클래스입니다.
        /// </summary>
        /// <param name="Screen"></param>
        public CefEventArgs(CefScreen Screen) => this.Screen = Screen;

        /// <summary>
        /// Cef 스크린 객체.
        /// </summary>
        public CefScreen Screen { get; private set; }
    }
}
