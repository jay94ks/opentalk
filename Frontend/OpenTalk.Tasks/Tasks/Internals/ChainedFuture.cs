using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenTalk.Tasks.Internals
{
    /// <summary>
    /// 선행 작업이 완료되면 실행되는 작업입니다.
    /// (선행 작업이 결과가 없는 작업인 경우)
    /// </summary>
    internal interface IChainedFuture
    {
        /// <summary>
        /// 선행 작업이 완료될 때 호출됩니다.
        /// </summary>
        void Fire();
    }

    /// <summary>
    /// 선행 작업이 완료되면 실행되는 작업입니다.
    /// (선행 작업이 결과가 없는 작업인 경우)
    /// </summary>
    internal class ChainedFuture : Future, IChainedFuture
    {
        private ChainedFutureStatus m_ChainStatus;
        private Action<Future> m_Callback;

        /// <summary>
        /// 선행 작업이 완료되면 실행되는 작업을 초기화합니다.
        /// </summary>
        /// <param name="Parent"></param>
        /// <param name="Callback"></param>
        public ChainedFuture(Future Previous, Action<Future> Callback)
        {
            m_ChainStatus = new ChainedFutureStatus(Previous);
            m_Callback = Callback;
        }

        /// <summary>
        /// 선행 작업이 완료될 때 호출됩니다.
        /// </summary>
        public void Fire()
        {
            lock (m_ChainStatus)
            {
                if (m_ChainStatus.Future != null)
                    return;

                if (m_ChainStatus.HasCancelIssued)
                    m_ChainStatus.Future = new CanceledFuture();

                else m_ChainStatus.Future = new ActionFuture(
                    () => m_Callback(m_ChainStatus.Previous));
            }
        }

        /// <summary>
        /// 이 체인 작업의 상태를 반환합니다.
        /// </summary>
        public override FutureStatus Status => m_ChainStatus.Status;

        /// <summary>
        /// 작업이 종료될 때 까지 대기합니다.
        /// </summary>
        public override bool Wait() => m_ChainStatus.Wait();

        /// <summary>
        /// 작업이 종료될 때 까지 대기합니다.
        /// </summary>
        public override bool Wait(int Milliseconds) => m_ChainStatus.Wait(Milliseconds);

        /// <summary>
        /// 작업이 취소되면 실행됩니다.
        /// </summary>
        protected override void OnCancel() => m_ChainStatus.Cancel();
    }


    /// <summary>
    /// 선행 작업이 완료되면 실행되는 작업입니다.
    /// (선행 작업이 결과가 없는 작업인 경우)
    /// </summary>
    internal class ChainedFuture<ResultType> : Future<ResultType>, IChainedFuture
    {
        private ChainedFutureStatus m_ChainStatus;
        private Func<Future, ResultType> m_Callback;

        /// <summary>
        /// 선행 작업이 완료되면 실행되는 작업을 초기화합니다.
        /// </summary>
        /// <param name="Parent"></param>
        /// <param name="Callback"></param>
        public ChainedFuture(Future Previous, Func<Future, ResultType> Callback)
        {
            m_ChainStatus = new ChainedFutureStatus(Previous);
            m_Callback = Callback;
        }

        /// <summary>
        /// 선행 작업이 완료될 때 호출됩니다.
        /// </summary>
        public void Fire()
        {
            lock (m_ChainStatus)
            {
                if (m_ChainStatus.Future != null)
                    return;

                if (m_ChainStatus.HasCancelIssued)
                    m_ChainStatus.Future = new CanceledFuture();

                else m_ChainStatus.Future = new ActionFuture<ResultType>(
                    () => m_Callback(m_ChainStatus.Previous));
            }

            m_ChainStatus.Future.Then(OnFinish);
        }

        /// <summary>
        /// 이 체인 작업의 상태를 반환합니다.
        /// </summary>
        public override FutureStatus Status => m_ChainStatus.Status;

        /// <summary>
        /// 이 체인 작업의 결과를 반환합니다.
        /// </summary>
        public override ResultType Result {
            get {
                if (m_ChainStatus.Wait())
                {
                    if (!IsCanceled && !IsFaulted && m_ChainStatus.Future != null)
                        return (m_ChainStatus.Future as ActionFuture<ResultType>).Result;
                }

                if (IsCanceled || m_ChainStatus.Future == null)
                    throw new FutureImpossibleException(FutureImpossibleReason.Canceled);

                throw new FutureImpossibleException(FutureImpossibleReason.Faulted);
            }
        }

        /// <summary>
        /// 작업이 종료될 때 까지 대기합니다.
        /// </summary>
        public override bool Wait() => m_ChainStatus.Wait();

        /// <summary>
        /// 작업이 종료될 때 까지 대기합니다.
        /// </summary>
        public override bool Wait(int Milliseconds) => m_ChainStatus.Wait(Milliseconds);

        /// <summary>
        /// 작업이 취소되면 실행됩니다.
        /// </summary>
        protected override void OnCancel() => m_ChainStatus.Cancel();
    }
}
