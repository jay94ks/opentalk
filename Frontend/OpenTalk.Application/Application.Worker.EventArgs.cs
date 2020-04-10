namespace OpenTalk
{
    public abstract partial class Application
    {
        public sealed partial class Worker
        {
            /// <summary>
            /// Worker 이벤트입니다.
            /// </summary>
            public class EventArgs : Application.EventArgs
            {
                internal EventArgs(Worker worker, State state)
                    : base(worker.Application)
                {
                    Worker = worker;
                    State = state;
                }

                /// <summary>
                /// 작업자 인스턴스입니다.
                /// </summary>
                public Worker Worker { get; private set; }

                /// <summary>
                /// 작업자의 상태입니다.
                /// </summary>
                public State State { get; private set; }
            }
        }
    }
}
