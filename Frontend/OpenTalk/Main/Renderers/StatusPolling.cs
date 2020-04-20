using OpenTalk.UI.CefUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Main.Renderers
{
    class StatusPolling : CefContentRenderer
    {
        private ScrMain m_MainScreen;

        public StatusPolling(ScrMain MainScreen)
            => m_MainScreen = MainScreen;

        /// <summary>
        /// 어플리케이션 상태를 폴링하는 요청을 처리합니다.
        /// </summary>
        public override CefContent Render(string Method, 
            string QueryString, bool AllowCache, bool OnlyFromCache)
        {
            return FromText("{}", "application/json");
        }
    }
}
