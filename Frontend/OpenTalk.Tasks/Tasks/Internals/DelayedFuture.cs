using OpenTalk.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenTalk.Tasks.Internals
{
    internal class DelayedFuture : Future
    {
        private FutureSource m_Future;
        private Timer m_Timer;

        /// <summary>
        /// 지정된 시간이 지나면 완료되는 작업을 생성합니다.
        /// -1이 주어지면 무슨 일이 일어나도 완료되지 않는 작업이 됩니다.
        /// (취소하지 않는 한)
        /// </summary>
        /// <param name="Milliseconds"></param>
        public DelayedFuture(int Milliseconds)
        {
            m_Future = new FutureSource();
            m_Future.Future.Then(OnFinish);

            if (Milliseconds > 0)
            {
                m_Timer = new Timer(OnDelayExpired, null,
                    Milliseconds, Timeout.Infinite);
            }

            else if (Milliseconds == 0)
                m_Future.TrySetCompleted();
        }

        /// <summary>
        /// 딜레이 타이머가 만료되면 실행됩니다.
        /// </summary>
        /// <param name="state"></param>
        private void OnDelayExpired(object state)
        {
            lock (m_Future)
            {
                m_Future.TrySetCompleted();
                m_Timer?.Dispose();
                m_Timer = null;
            }
        }

        /// <summary>
        /// 이 작업의 실행 상태를 나타냅니다.
        /// </summary>
        public override FutureStatus Status => m_Future.Future.Status;

        /// <summary>
        /// 작업이 종료될 때 까지 대기합니다.
        /// </summary>
        public override bool Wait() => m_Future.Future.Wait();

        /// <summary>
        /// 작업이 종료될 때 까지 대기합니다.
        /// </summary>
        public override bool Wait(int Milliseconds) => m_Future.Future.Wait(Milliseconds);

        /// <summary>
        /// 지연 작업을 취소합니다.
        /// </summary>
        protected override void OnCancel()
        {
            lock (m_Future)
            {
                m_Future.TrySetCanceled();
                m_Timer?.Dispose();
                m_Timer = null;
            }
        }
    }
}
