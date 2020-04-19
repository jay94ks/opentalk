using OpenTalk.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenTalk.Tasks.Internals
{
    /// <summary>
    /// 어떤 작업이 수행된 미래를 표현합니다.
    /// </summary>
    internal class ActionFuture : Future
    {
        private Action m_Action;
        private FutureStatus m_Status;
        private TrickyManualEvent m_Event;
        private Exception m_Exception;
        private bool m_WannaCancel;

        /// <summary>
        /// 어떤 작업이 수행된 미래를 표현하는 객체를 초기화합니다.
        /// </summary>
        /// <param name="Action"></param>
        public ActionFuture(Action Action)
        {
            m_Action = Action;
            m_Status = FutureStatus.Canceled;
            m_Event = new TrickyManualEvent(false);
            m_Exception = null;
            m_WannaCancel = false;

            ThreadPool.QueueUserWorkItem(OnExecuteFuture, this);
        }

        /// <summary>
        /// ActionFuture를 실제로 실행시킵니다.
        /// </summary>
        /// <param name="state"></param>
        private static void OnExecuteFuture(object state)
        {
            ActionFuture Future = state as ActionFuture;
            
            while (true)
            {
                lock (Future)
                {
                    Future.m_Status = FutureStatus.Scheduled;

                    if (Future.m_WannaCancel)
                    {
                        Future.m_Exception = null;
                        Future.m_Status = FutureStatus.Canceled;
                        break;
                    }

                    Future.m_Status = FutureStatus.Running;
                }

                if (Debugger.IsAttached)
                {
                    Future.m_Action();

                    lock (Future)
                    {
                        Future.m_Exception = null;
                        Future.m_Status = FutureStatus.Succeed;
                    }
                }
                else
                {
                    try
                    {
                        Future.m_Action();

                        lock (Future)
                        {
                            Future.m_Exception = null;
                            Future.m_Status = FutureStatus.Succeed;
                        }
                    }

                    catch (Exception e)
                    {
                        lock (Future)
                        {
                            Future.m_Exception = e;
                            Future.m_Status = FutureStatus.Faulted;
                        }
                    }
                }

                break;
            }

            Future.m_Event.Set();
            Future.OnFinish();
        }

        /// <summary>
        /// 오류가 발생한 경우, 오류를 나타내는 예외 정보를 반환합니다.
        /// </summary>
        public override Exception Exception {
            get { lock (this) return m_Exception; }
        }

        /// <summary>
        /// 이 작업의 실행 상태를 나타냅니다.
        /// </summary>
        public override FutureStatus Status {
            get { lock (this) return m_Status; }
        }

        /// <summary>
        /// 작업이 종료될 때 까지 대기합니다.
        /// </summary>
        public override bool Wait()
        {
            if (IsCompleted)
                return true;

            m_Event.WaitOne();
            return IsCompleted;
        }

        /// <summary>
        /// 작업이 종료될 때 까지 대기합니다.
        /// </summary>
        public override bool Wait(int Milliseconds)
        {
            if (Milliseconds < 0)
                return Wait();

            if (IsCompleted)
                return true;

            m_Event.WaitOne(Milliseconds);
            return IsCompleted;
        }

        /// <summary>
        /// 작업이 취소되어야 할 때 실행됩니다.
        /// </summary>
        protected override void OnCancel()
        {
            m_WannaCancel = true;
        }
    }

    /// <summary>
    /// 어떤 작업이 수행된 미래를 표현합니다.
    /// </summary>
    internal class ActionFuture<ResultType> : Future<ResultType>
    {
        private Func<ResultType> m_Action;
        private FutureStatus m_Status;
        private TrickyManualEvent m_Event;
        private ResultType m_Result;
        private Exception m_Exception;
        private bool m_WannaCancel;

        /// <summary>
        /// 어떤 작업이 수행된 미래를 표현하는 객체를 초기화합니다.
        /// </summary>
        /// <param name="Action"></param>
        public ActionFuture(Func<ResultType> Action)
        {
            m_Action = Action;
            m_Status = FutureStatus.Canceled;
            m_Event = new TrickyManualEvent(false);
            m_Result = default(ResultType);
            m_Exception = null;
            m_WannaCancel = false;

            ThreadPool.QueueUserWorkItem(OnExecuteFuture, this);
        }

        /// <summary>
        /// ActionFuture를 실제로 실행시킵니다.
        /// </summary>
        /// <param name="state"></param>
        private static void OnExecuteFuture(object state)
        {
            ActionFuture<ResultType> Future = state as ActionFuture<ResultType>;

            while (true)
            {
                lock (Future)
                {
                    Future.m_Status = FutureStatus.Scheduled;

                    if (Future.m_WannaCancel)
                    {
                        Future.m_Exception = null;
                        Future.m_Status = FutureStatus.Canceled;
                        break;
                    }

                    Future.m_Status = FutureStatus.Running;
                }

                try
                {
                    Future.m_Result = Future.m_Action();

                    lock (Future)
                    {
                        Future.m_Exception = null;
                        Future.m_Status = FutureStatus.Succeed;
                    }
                }

                catch (Exception e)
                {
                    lock (Future)
                    {
                        Future.m_Exception = e;
                        Future.m_Status = FutureStatus.Faulted;
                    }
                }

                break;
            }

            Future.m_Event.Set();
            Future.OnFinish();
        }
        
        /// <summary>
        /// 오류가 발생한 경우, 오류를 나타내는 예외 정보를 반환합니다.
        /// </summary>
        public override Exception Exception {
            get { lock (this) return m_Exception; }
        }

        /// <summary>
        /// 이 작업의 실행 상태를 나타냅니다.
        /// </summary>
        public override FutureStatus Status {
            get { lock (this) return m_Status; }
        }

        /// <summary>
        /// 작업의 결과를 확인합니다.
        /// 
        /// 완료되지 않은 상태에서 결과를 요청하면
        /// 결과를 대기하게 됩니다.
        /// </summary>
        public override ResultType Result {
            get {
                while (!IsCompleted)
                    Wait();

                if (IsFaulted)
                    throw new FutureImpossibleException(FutureImpossibleReason.Faulted);

                if (IsCanceled)
                    throw new FutureImpossibleException(FutureImpossibleReason.Canceled);

                return m_Result;
            }
        }

        /// <summary>
        /// 작업이 종료될 때 까지 대기합니다.
        /// </summary>
        public override bool Wait()
        {
            if (IsCompleted)
                return true;

            m_Event.WaitOne();
            return IsCompleted;
        }

        /// <summary>
        /// 작업이 종료될 때 까지 대기합니다.
        /// </summary>
        public override bool Wait(int Milliseconds)
        {
            if (IsCompleted)
                return true;

            m_Event.WaitOne(Milliseconds);
            return IsCompleted;
        }

        /// <summary>
        /// 작업이 취소되어야 할 때 실행됩니다.
        /// </summary>
        protected override void OnCancel()
        {
            m_WannaCancel = true;
        }
    }
}
