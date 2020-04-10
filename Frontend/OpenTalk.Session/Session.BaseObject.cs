namespace OpenTalk
{
    public partial class Session
    {
        public class BaseObject
        {
            /// <summary>
            /// 세션과 관련된 객체를 초기화합니다.
            /// </summary>
            /// <param name="session"></param>
            protected BaseObject(Session session) => Session = session;

            /// <summary>
            /// 세션 객체를 획득합니다.
            /// </summary>
            public Session Session { get; private set; }
        }
    }
}
