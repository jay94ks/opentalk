using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Tasks.Internals
{
    internal class CombinedFuture : Future, IChainedFuture
    {
        private Future[] m_Futures;
        private int m_Counter;
        private bool m_WannaCancel;

        /// <summary>
        /// 지정된 모든 작업이 끝나면 완료됩니다.
        /// </summary>
        /// <param name="Futures"></param>
        public CombinedFuture(params Future[] Futures)
        {
            m_Futures = Futures;
            m_Counter = Futures.Length;
            m_WannaCancel = false;

            foreach (Future Each in Futures)
            {
                lock (Each)
                {
                    if (!Each.Chain(this))
                        Fire();
                }
            }
        }

        /// <summary>
        /// 작업이 완료될 때 마다 실행됩니다.
        /// </summary>
        public void Fire()
        {
            lock(this)
            {
                m_Counter--;

                if (m_WannaCancel)
                    return;

                if (m_Counter <= 0)
                    OnFinish();
            }
        }

        /// <summary>
        /// 이 작업의 상태를 나타냅니다.
        /// </summary>
        public override FutureStatus Status {
            get {
                lock (this)
                {
                    if (m_WannaCancel)
                        return FutureStatus.Canceled;

                    if (m_Counter > 0)
                        return FutureStatus.Scheduled;

                    return FutureStatus.Succeed;
                }
            }
        }

        /// <summary>
        /// 작업이 종료될 때 까지 대기합니다.
        /// </summary>
        public override bool Wait()
        {
            int index = 0;

            if (IsCompleted)
                return true;

            while (index < m_Futures.Length)
            {
                lock (this)
                {
                    if (m_WannaCancel)
                        break;
                }

                if (m_Futures[index].Wait(1000))
                    index++;
            }

            return IsCompleted;
        }

        /// <summary>
        /// 작업이 종료될 때 까지 대기합니다.
        /// </summary>
        public override bool Wait(int Milliseconds)
        {
            if (Milliseconds >= 0)
            {
                int index = 0;
                DateTime MarkedTime;

                if (IsCompleted)
                    return true;

                MarkedTime = DateTime.Now;
                while (index < m_Futures.Length)
                {
                    lock (this)
                    {
                        if (m_WannaCancel)
                            break;
                    }

                    if (m_Futures[index].Wait(
                        Milliseconds <= 100 ? Milliseconds : 100))
                        index++;

                    if (Milliseconds > 0)
                    {
                        Milliseconds -= Math.Max(0, (int)((DateTime.Now - 
                            MarkedTime).TotalMilliseconds));

                        MarkedTime = DateTime.Now;
                    }

                    else break;
                }

                return IsCompleted;
            }

            return Wait();
        }

        /// <summary>
        /// 작업이 취소되면 실행됩니다.
        /// </summary>
        protected override void OnCancel()
        {
            m_WannaCancel = true;
        }
    }
}
