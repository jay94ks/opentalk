using Newtonsoft.Json;
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
        private JsonExports m_Response;
        private bool m_SessionInvalidated;

        public StatusPolling(ScrMain MainScreen)
        {
            m_Response = new JsonExports();
            m_MainScreen = MainScreen;

            Program.OTK.Session.Authentication.Authenticated += OnAuthUpdated;
            Program.OTK.Session.Authentication.Deauthenticated += OnAuthUpdated;
        }

        private void OnAuthUpdated(Session session, 
            Session.Auth.Operation operation, SessionError errorCode)
        {
            /* 새로 로그인 했으면, 갱신 대상을 새로 추가합니다. */
            if (session.Authentication.Credential != null)
            {
                m_SessionInvalidated = false;

                /* 내 프로필, 친구 목록, 즐겨찾기를 갱신 대상으로 넣습니다.*/
                m_Response.Targets.Add("myinfo");
                m_Response.Targets.Add("friends");
                m_Response.Targets.Add("favorates");
            }

            else
            {
                m_SessionInvalidated = true;
            }
        }

        private class JsonExports
        {
            [JsonProperty("targets")]
            public List<string> Targets { get; set; } = new List<string>();
        }
        
        /// <summary>
        /// 어플리케이션 상태를 폴링하는 요청을 처리합니다.
        /// </summary>
        public override CefContent Render(string Method, 
            string QueryString, bool AllowCache, bool OnlyFromCache)
        {
            if (QueryString == "r=0")
            {
                /* 인터페이스 리로드 후 첫 요청인 경우. */
                if (Program.OTK.Session.Authentication.Credential != null)
                    OnAuthUpdated(Program.OTK.Session, Session.Auth.Operation.Authentication, SessionError.None);
            }

            if (m_SessionInvalidated)
            {
                m_SessionInvalidated = false;
                return MakeError();
            }

            lock (m_Response)
            {
                string Response = null;

                RemoveDuplicatedTargets();
                Response = JsonConvert.SerializeObject(m_Response);

                // 송신하고나서 리스트를 비웁니다.
                m_Response.Targets.Clear();

                return FromText(Response, "application/json");
            }
        }

        /// <summary>
        /// 응답에서 중복된 요소들을 정리합니다.
        /// </summary>
        private void RemoveDuplicatedTargets()
        {
            m_Response.Targets 
                = m_Response.Targets.Distinct().ToList();
        }
    }
}
