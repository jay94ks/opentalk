namespace OpenTalk
{
    public abstract partial class Application
    {
        /// <summary>
        /// 어플리케이션 이벤트용 인자의 기반 클래스입니다.
        /// </summary>
        public class EventArgs : System.EventArgs
        {
            internal EventArgs(Application application) 
                => Application = application;

            /// <summary>
            /// 어플리케이션 인스턴스입니다.
            /// </summary>
            public Application Application { get; private set; }
        }
    }
}
