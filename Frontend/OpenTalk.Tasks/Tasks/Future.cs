using OpenTalk.Tasks.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Tasks
{

    /// <summary>
    /// 닷넷 Task보다 가벼운 Task를 구현합니다.
    /// </summary>
    public abstract partial class Future
    {
        /// <summary>
        /// 작업 체인입니다. 상속받은 자식 클래스에선 절대 직접 접근할 수 없습니다.
        /// </summary>
        private Queue<IChainedFuture> m_Chains = new Queue<IChainedFuture>();

        /// <summary>
        /// 완료 여부를 확인합니다.
        /// (성공/실패/취소 여부에 상관없이 완료되었는지 확인합니다)
        /// </summary>
        public bool IsCompleted => Status == FutureStatus.Succeed ||
            Status == FutureStatus.Canceled || Status == FutureStatus.Faulted;

        /// <summary>
        /// 취소 여부를 확인합니다.
        /// </summary>
        public bool IsCanceled => Status == FutureStatus.Canceled;

        /// <summary>
        /// 오류로 인해 완료되었습니다.
        /// </summary>
        public bool IsFaulted => Status == FutureStatus.Faulted;

        /// <summary>
        /// 사용자 정의 태그입니다.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// 오류로 인해 완료된 경우, 오류 정보를 포함하는 예외를 포함합니다.
        /// </summary>
        public virtual Exception Exception => null;

        /// <summary>
        /// 작업의 상태를 확인합니다.
        /// </summary>
        public abstract FutureStatus Status { get; }

        /// <summary>
        /// 작업이 종료될 때 까지 대기합니다.
        /// </summary>
        public abstract bool Wait();

        /// <summary>
        /// 작업이 종료될 때 까지 대기합니다.
        /// </summary>
        public abstract bool Wait(int Milliseconds);

        /// <summary>
        /// 작업이 취소되면 실행됩니다.
        /// </summary>
        protected abstract void OnCancel();

        /// <summary>
        /// 작업이 완료되면 실행됩니다.
        /// (취소된 경우에도 실행됩니다)
        /// </summary>
        protected virtual void OnFinish()
        {
            Queue<IChainedFuture> Chains = null;

            lock (this)
            {
                Chains = m_Chains;
                m_Chains = null;
            }

            if (Chains != null)
            {
                while (Chains.Count > 0)
                    Chains.Dequeue().Fire();
            }
        }

        /// <summary>
        /// 내부 연속 체인에 작업을 끼워넣습니다.
        /// </summary>
        /// <param name="Future"></param>
        /// <returns></returns>
        internal bool Chain(IChainedFuture Future)
        {
            lock (this)
            {
                if (m_Chains != null)
                {
                    m_Chains.Enqueue(Future);
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// 닷넷 Task보다 가벼운 Task를 구현합니다.
    /// </summary>
    /// <typeparam name="ResultType"></typeparam>
    public abstract class Future<ResultType> : Future
    {
        /// <summary>
        /// 작업의 결과를 확인합니다.
        /// 
        /// 완료되지 않은 상태에서 결과를 요청하면
        /// 결과를 대기하게 됩니다.
        /// 
        /// 취소되거나 오류가 발생한 작업을 대상으로 
        /// 이 속성에 접근하게되면 FutureImpossibleException 예외가 발생됩니다.
        /// </summary>
        public abstract ResultType Result { get; }
    }
}
