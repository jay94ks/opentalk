using OpenTalk.Tasks;

namespace OpenTalk.UI.CefUnity
{
    public partial class CefScreen
    {
        public class LifeCycle
        {
            private FutureEventCombination m_ReadyState;
            private CefScreen m_Master;

            internal LifeCycle(CefScreen Master)
            {
                m_Master = Master;

                Instance = new FutureSource();
                Initialization = new FutureEventSource();
                Registration = new FutureEventSource<string>();
                LoadState = new FutureEventSource();
                ScriptReady = new FutureEventSource();

                m_ReadyState = new FutureEventCombination(
                    Initialization, Registration);
            }

            /// <summary>
            /// 브라우저 인스턴스 상태 이벤트 원본입니다.
            /// </summary>
            internal FutureSource Instance { get; private set; }

            /// <summary>
            /// 초기화 상태 이벤트 원본입니다.
            /// </summary>
            internal FutureEventSource Initialization { get; private set; }

            /// <summary>
            /// 초기화 상태 이벤트 원본입니다.
            /// </summary>
            internal FutureEventSource<string> Registration { get; private set; }

            /// <summary>
            /// 브라우저 로딩 이벤트 원본입니다.
            /// </summary>
            internal FutureEventSource LoadState { get; private set; }

            /// <summary>
            /// 스크립트 준비 상태와 관련된 이벤트 원본입니다.
            /// </summary>
            internal FutureEventSource ScriptReady { get; private set; }

            /// <summary>
            /// 브라우져 준비 상태에 관련된 작업 객체입니다.
            /// </summary>
            public Future Ready => m_ReadyState.Future;

            /// <summary>
            /// 로딩 완료 여부에 관련된 작업 객체입니다.
            /// </summary>
            public Future Loading => LoadState.Future;

            /// <summary>
            /// 스크립팅 가능 여부에 관련된 작업 객체입니다.
            /// </summary>
            public Future Scripting => ScriptReady.Future;

        }
    }
}
