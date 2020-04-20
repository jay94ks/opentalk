namespace OpenTalk.UI.CefUnity
{
    public partial class CefScreen
    {
        private class ScriptingExtension
        {
            private CefScreen m_Master;

            /// <summary>
            /// 자바스크립트 통신 인터페이스입니다.
            /// </summary>
            /// <param name="Master"></param>
            public ScriptingExtension(CefScreen Master) => m_Master = Master;

            /// <summary>
            /// CefScreen을 닫길 원한답니다.
            /// (window.close에 이 메서드가 덧쒸워져 있습니다)
            /// </summary>
            public void Close()
            {
                m_Master.OnCloseRequested();
            }
        }
    }
}
