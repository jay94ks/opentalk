namespace OpenTalk
{
    public abstract partial class Application
    {
        public sealed partial class Worker
        {
            /// <summary>
            /// 작업자 쓰레드의 작업 단위를 지정합니다.
            /// </summary>
            public enum WorkUnit
            {
                /// <summary>
                /// Task를 연속적으로 실행하는 작업자입니다.
                /// </summary>
                Task,

                /// <summary>
                /// 동일한 Task를 반복적으로 수행하는 작업자입니다.
                /// </summary>
                Loop
            }
        }
    }
}
