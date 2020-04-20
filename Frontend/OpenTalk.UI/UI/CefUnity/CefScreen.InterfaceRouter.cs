using System;
using System.Collections.Generic;

namespace OpenTalk.UI.CefUnity
{
    public partial class CefScreen
    {
        /// <summary>
        /// 전역 라우터 객체입니다.
        /// 여기에 설정된 컨텐트들은 모든 CefScreen에서 접근할 수 있습니다.
        /// </summary>
        public static InterfaceRouter GlobalRouter { get; private set; }
            = new InterfaceRouter();

        public class InterfaceRouter
        {
            private Dictionary<string, CefContentRenderer> m_Renderers
                = new Dictionary<string, CefContentRenderer>();

            private CefContentRenderer m_Failback;

            /// <summary>
            /// 모든 렌더러를 제거합니다.
            /// </summary>
            public void Clear()
            {
                lock (m_Renderers)
                    m_Renderers.Clear();
            }

            /// <summary>
            /// 지정된 경로에 컨텐트 렌더러를 설정합니다.
            /// 메인 페이지를 설정하려면 경로에 공백 혹은 슬래쉬, '/'를 입력하십시오.
            /// (설정은 할 수 있지만 뭐가 설정되어 있는지 확인은 할 수 없습니다)
            /// </summary>
            /// <param name="Path"></param>
            /// <param name="Renderer"></param>
            public void Set(string Path, CefContentRenderer Renderer)
            {
                Path = Path != null ? Path.Trim('/') : "";

                lock (m_Renderers)
                {
                    if (Renderer != null)
                        m_Renderers[Path] = Renderer;

                    else if (m_Renderers.ContainsKey(Path))
                        m_Renderers.Remove(Path);
                }
            }

            /// <summary>
            /// FailOver 렌더러를 등록합니다.
            /// </summary>
            /// <param name="Renderer"></param>
            /// <returns></returns>
            public bool FailOver(CefContentRenderer Renderer)
            {
                if (this != GlobalRouter)
                    return false;

                m_Failback = Renderer;
                return true;
            }

            /// <summary>
            /// 주어진 경로 문자열을 기반으로 컨텐트 렌더러를 획득합니다.
            /// </summary>
            /// <param name="requestedUri"></param>
            /// <returns></returns>
            internal CefContentRenderer Route(CefScreen screen, string requestedUri)
            {
                CefContentRenderer renderer = null;
                requestedUri = requestedUri.Trim('/');

                lock (m_Renderers)
                {
                    foreach(string targetUri in m_Renderers.Keys)
                    {
                        if (targetUri.Trim('/') == requestedUri)
                        {
                            renderer = m_Renderers[targetUri];
                            break;
                        }
                    }
                }

                if (renderer.IsNotNull())
                    renderer = renderer.OnRouted(screen, requestedUri);

                return renderer.IsNotNull() ? renderer :
                    (GlobalRouter != this ? GlobalRouter.Route(screen, requestedUri) : 
                    (m_Failback != null ? m_Failback.OnRouted(screen, requestedUri) : null));
            }
        }
    }
}
