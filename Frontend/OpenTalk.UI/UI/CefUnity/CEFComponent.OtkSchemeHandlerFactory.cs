using CefSharp;

namespace OpenTalk.UI.CefUnity
{
    public partial class CefComponent
    {
        /// <summary>
        /// otk:로 시작하는 커스텀 스키마 핸들러를 구현합니다.
        /// </summary>
        /// <param name="Master"></param>
        private class OtkSchemeHandlerFactory : ISchemeHandlerFactory
        {
            public static readonly string SchemeName = "otk";
            private CefComponent m_Master;

            /// <summary>
            /// otk:로 시작하는 커스텀 스키마 핸들러입니다.
            /// </summary>
            /// <param name="Master"></param>
            public OtkSchemeHandlerFactory(CefComponent Master) => m_Master = Master;

            /// <summary>
            /// 리소스 핸들러를 생성합니다.
            /// </summary>
            /// <param name="browser"></param>
            /// <param name="frame"></param>
            /// <param name="schemeName"></param>
            /// <param name="request"></param>
            /// <returns></returns>
            public IResourceHandler Create(IBrowser browser, IFrame frame, string schemeName, IRequest request) 
                => new OtkSchemeHandler(m_Master);
        }
    }
}
