namespace OpenTalk
{
    public abstract partial class Application
    {
        /// <summary>
        /// 어플리케이션 인스턴스를 참조하는 객체를 구현할 때 슈퍼 클래스로 사용하십시오.
        /// 현재 실행중인 어플리케이션 인스턴스로부터, 혹은 지정된 어플리케이션 인스턴스를 참조하는
        /// 객체를 구현할 때 필요한 생성자가 사전에 준비되어 있습니다.
        /// </summary>
        public class BaseObject
        {
            /// <summary>
            /// 객체를 초기화합니다.
            /// 
            /// 단, 현재 실행중인 어플리케이션 인스턴스가 없을 때 
            /// ApplicationException 예외를 일으킵니다.
            /// </summary>
            public BaseObject() : this(null) { }

            /// <summary>
            /// 객체를 초기화합니다.
            /// 
            /// 단, 현재 실행중인 어플리케이션 인스턴스가 없을 때 
            /// ApplicationException 예외를 일으킵니다.
            /// </summary>
            /// <param name="application"></param>
            public BaseObject(Application application)
            {
                Application = application != null ? 
                    application : RunningInstance;

                if (Application == null)
                    throw new ApplicationException();
            }

            /// <summary>
            /// 어플리케이션 인스턴스에 접근합니다.
            /// </summary>
            public Application Application { get; private set; }
        }
    }
}
