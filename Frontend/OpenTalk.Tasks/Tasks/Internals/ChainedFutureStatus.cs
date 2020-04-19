using System;
using System.Threading;

namespace OpenTalk.Tasks.Internals
{
    /// <summary>
    /// 선행 작업이 완료되면 실행되는 작업의 상태 관리자입니다.
    /// </summary>
    internal class ChainedFutureStatus
    {
        private Future m_Future;

        /// <summary>
        /// 선행 작업이 완료되면 실행되는 작업을 초기화합니다.
        /// </summary>
        /// <param name="Parent"></param>
        /// <param name="Callback"></param>
        public ChainedFutureStatus(Future Previous)
        {
            HasCancelIssued = false;
            this.Previous = Previous;
        }

        /// <summary>
        /// 취소가 요청되었는지 검사합니다.
        /// </summary>
        public bool HasCancelIssued { get; private set; }

        /// <summary>
        /// 이전에 수행하던 작업을 획득합니다.
        /// </summary>
        public Future Previous { get; private set; }

        /// <summary>
        /// 미래에 실행될 작업을 설정하거나 획득합니다.
        /// </summary>
        public Future Future {
            get { lock (this) return m_Future; }
            set { lock (this) m_Future = value; }
        }

        /// <summary>
        /// 이 체인 작업의 상태를 반환합니다.
        /// </summary>
        public FutureStatus Status {
            get {
                lock (this)
                {
                    if (Future != null)
                        return Future.Status;

                    if (HasCancelIssued)
                        return FutureStatus.Canceled;

                    return FutureStatus.Scheduled;
                }
            }
        }

        /// <summary>
        /// 작업이 종료될 때 까지 대기합니다.
        /// </summary>
        public bool Wait()
        {
            if (Previous.Wait())
            {
                while (true)
                {
                    lock (this)
                    {
                        if (Future != null)
                            break;
                    }

                    Thread.Yield();
                }

                return Future.Wait();
            }

            return false;
        }

        /// <summary>
        /// 작업이 종료될 때 까지 대기합니다.
        /// </summary>
        public bool Wait(int Milliseconds)
        {
            if (Milliseconds >= 0)
            {
                DateTime MarkedTime = DateTime.Now;

                if (Previous.Wait(Milliseconds))
                {
                    while (true)
                    {
                        lock (this)
                        {
                            if (Future != null)
                                break;
                        }

                        Thread.Yield();
                    }

                    return Future.Wait(Math.Max(0, (int)(Milliseconds -
                        (DateTime.Now - MarkedTime).TotalMilliseconds)));
                }

                return false;
            }

            return Wait();
        }

        /// <summary>
        /// 작업이 취소되면 실행됩니다.
        /// </summary>
        public void Cancel()
        {
            lock (this)
            {
                HasCancelIssued = true;

                if (Future != null)
                    Future.Cancel(Future);
            }
        }
    }
}
