namespace OpenTalk
{
    public abstract partial class Application
    {
        public sealed partial class Worker
        {
            /// <summary>
            /// 작업자 인스턴스의 상태를 표현합니다.
            /// </summary>
            public enum State
            {
                /// <summary>
                /// 작업자 쓰레드가 시작되었습니다.
                /// </summary>
                Started,

                /// <summary>
                /// 작업을 대기하고 있습니다.
                /// </summary>
                Waiting,

                /// <summary>
                /// 작업을 수행하고 있습니다.
                /// </summary>
                Running,

                /// <summary>
                /// 작업자 쓰레드가 정지되었습니다.
                /// </summary>
                Stopped
            }
        }
    }
}
