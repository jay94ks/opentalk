using CefSharp;
using System;

namespace OpenTalk.UI.CefUnity
{
    public partial class CefComponent
    {
        /// <summary>
        /// otk 스키마 핸들러입니다.
        /// </summary>
        internal class OtkSchemeHandler : ResourceHandler
        {
            internal static readonly string m_Scheme = "otk://";
            private CefComponent m_Master;

            /// <summary>
            /// otk 스키마 핸들러를 초기화합니다.
            /// </summary>
            /// <param name="master"></param>
            /// <param name="browser"></param>
            /// <param name="frame"></param>
            /// <param name="schemeName"></param>
            /// <param name="request"></param>
            public OtkSchemeHandler(CefComponent Master) => m_Master = Master;

            /// <summary>
            /// 리소스를 핸들링합니다.
            /// </summary>
            /// <param name="request"></param>
            /// <param name="callback"></param>
            /// <returns></returns>
            public override CefReturnValue ProcessRequestAsync(
                IRequest request, ICallback callback)
            {
                while (true)
                {
                    string ScreenId = request.Url.ToLower();
                    
                    // request.Url ==> e.g. otk://r9jbqm9mz3v4u6dt8jwov0d5w4s85al6/

                    if (!ScreenId.StartsWith(m_Scheme))
                        break;

                    ScreenId = ScreenId.Substring(m_Scheme.Length).Split('/')[0];

                    if (string.IsNullOrEmpty(ScreenId) ||
                        string.IsNullOrWhiteSpace(ScreenId))
                    {
                        break;
                    }

                    lock (m_Master.m_CefScreens)
                    {
                        if (!m_Master.m_CefScreens.ContainsKey(ScreenId))
                            break;
                        
                        m_Master.m_CefScreens[ScreenId].HandleRequestAsync(this, request)
                            .ContinueOnMessageLoop((X) => callback.Continue());

                        return CefReturnValue.ContinueAsync;
                    }
                }

                callback.Dispose();
                return CefReturnValue.Cancel;
            }
        }
    }
}
