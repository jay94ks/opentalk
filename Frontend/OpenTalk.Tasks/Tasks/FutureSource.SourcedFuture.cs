using OpenTalk.Helpers;
using System;
using System.Threading;

namespace OpenTalk.Tasks
{
    public partial class FutureSource
    {
        internal class SourcedFuture : Future
        {
            private TrickyManualEvent m_Event = new TrickyManualEvent(false);
            private FutureStatus m_State = FutureStatus.Running;
            private Exception m_Exception = null;
            private FutureSource m_Source;

            public SourcedFuture(FutureSource Source) => m_Source = Source;

            /// <summary>
            /// 작업의 상태를 확인합니다.
            /// </summary>
            public override FutureStatus Status {
                get {
                    lock (this)
                        return m_State;
                }
            }

            /// <summary>
            /// 작업이 성공적으로 끝난 것으로 설정합니다.
            /// </summary>
            public bool SetCompletion()
            {
                lock (this)
                {
                    if (IsCompleted)
                        return false;

                    m_State = FutureStatus.Succeed;
                }

                m_Event.Set();
                OnFinish();

                return true;
            }

            /// <summary>
            /// 작업이 실패된 것으로 설정합니다.
            /// </summary>
            public bool SetCompletion(Exception e)
            {
                lock (this)
                {
                    if (IsCompleted)
                        return false;

                    m_Exception = e;
                    m_State = FutureStatus.Faulted;
                }

                m_Event.Set();
                OnFinish();
                return true;
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
            /// 작업이 취소되면 실행됩니다.
            /// </summary>
            protected override void OnCancel()
            {
                lock (this)
                {
                    if (IsCompleted)
                        return;
                    
                    m_State = FutureStatus.Canceled;
                }

                m_Source?.RaiseCanceled();

                m_Event.Set();
                OnFinish();
            }
        }
    }

    public partial class FutureSource<ResultType>
    {
        internal class SourcedFuture : Future<ResultType>
        {
            private TrickyManualEvent m_Event = new TrickyManualEvent(false);
            private FutureStatus m_State = FutureStatus.Running;
            private Exception m_Exception = null;
            private ResultType m_Result = default(ResultType);
            private FutureSource<ResultType> m_Source;

            public SourcedFuture(FutureSource<ResultType> Source) => m_Source = Source;

            /// <summary>
            /// 작업의 상태를 확인합니다.
            /// </summary>
            public override FutureStatus Status {
                get {
                    lock (this)
                        return m_State;
                }
            }

            /// <summary>
            /// 작업의 결과를 획득합니다.
            /// </summary>
            public override ResultType Result {
                get {
                    if (!IsCompleted)
                        Wait();

                    if (IsCanceled)
                        throw new FutureImpossibleException(FutureImpossibleReason.Canceled);

                    if (IsFaulted)
                        throw new FutureImpossibleException(FutureImpossibleReason.Faulted);

                    return m_Result;
                }
            }

            /// <summary>
            /// 작업이 성공적으로 끝난 것으로 설정합니다.
            /// </summary>
            public bool SetCompletion(ResultType Result)
            {
                lock (this)
                {
                    if (IsCompleted)
                        return false;

                    m_Result = Result;
                    m_State = FutureStatus.Succeed;
                }

                m_Event.Set();
                OnFinish();

                return true;
            }

            /// <summary>
            /// 작업이 실패된 것으로 설정합니다.
            /// </summary>
            public bool SetCompletion(Exception e)
            {
                lock (this)
                {
                    if (IsCompleted)
                        return false;

                    m_Exception = e;
                    m_State = FutureStatus.Faulted;
                }

                m_Event.Set();
                OnFinish();
                return true;
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
            /// 작업이 취소되면 실행됩니다.
            /// </summary>
            protected override void OnCancel()
            {
                lock (this)
                {
                    if (IsCompleted)
                        return;

                    m_State = FutureStatus.Canceled;
                }

                m_Source?.RaiseCanceled();

                m_Event.Set();
                OnFinish();
            }
        }
    }
}
