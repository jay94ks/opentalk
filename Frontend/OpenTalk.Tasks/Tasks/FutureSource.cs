using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Tasks
{
    /// <summary>
    /// 사용자 코드에 의해 상태가 제어되는 작업 객체를 만듭니다.
    /// </summary>
    public partial class FutureSource : IFutureSource
    {
        private SourcedFuture m_Future;
        private bool m_ExpectsCanceled;

        /// <summary>
        /// 사용자 코드에 의해 상태가 제어되는 작업 객체를 초기화합니다.
        /// </summary>
        public FutureSource()
        {
            m_ExpectsCanceled = false;
            m_Future = new SourcedFuture(this);
        }

        /// <summary>
        /// 작업 객체입니다.
        /// </summary>
        public Future Future => m_Future;

        /// <summary>
        /// 사용자 정의 태그입니다.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Future 객체에서 취소를 요청하면 실행되는 이벤트입니다.
        /// </summary>
        public event EventHandler Canceled;

        /// <summary>
        /// 취소 이벤트를 발생시킵니다.
        /// </summary>
        internal void RaiseCanceled()
        {
            if (!m_ExpectsCanceled)
                Canceled?.Invoke(m_Future, EventArgs.Empty);
        }

        /// <summary>
        /// 작업이 성공적으로 끝난 것으로 처리합니다. (예외 발생하지 않음)
        /// </summary>
        /// <returns></returns>
        public bool TrySetCompleted() => m_Future.SetCompletion();

        /// <summary>
        /// 작업이 취소되어 끝난 것으로 처리합니다. (예외 발생하지 않음)
        /// </summary>
        /// <returns></returns>
        public bool TrySetFaulted(Exception e) => m_Future.SetCompletion(e);

        /// <summary>
        /// 작업이 취소되어 끝난 것으로 처리합니다. (예외 발생하지 않음)
        /// </summary>
        public bool TrySetCanceled()
        {
            lock (this)
                m_ExpectsCanceled = true;

            return Future.Cancel(m_Future);
        }

        /// <summary>
        /// 작업이 성공적으로 끝난 것으로 처리합니다.
        /// </summary>
        public void SetCompleted()
        {
            if (!m_Future.SetCompletion())
            {
                if (m_Future.IsCanceled)
                    throw new FutureImpossibleException(FutureImpossibleReason.Canceled);

                else if (m_Future.IsFaulted)
                    throw new FutureImpossibleException(FutureImpossibleReason.Faulted);

                throw new FutureImpossibleException(FutureImpossibleReason.Completed);
            }
        }

        /// <summary>
        /// 작업이 취소되어 끝난 것으로 처리합니다.
        /// </summary>
        public void SetFaulted(Exception e)
        {
            if (!m_Future.SetCompletion(e))
            {
                if (m_Future.IsCanceled)
                    throw new FutureImpossibleException(FutureImpossibleReason.Canceled);

                else if (m_Future.IsFaulted)
                    throw new FutureImpossibleException(FutureImpossibleReason.Faulted);

                throw new FutureImpossibleException(FutureImpossibleReason.Completed);
            }
        }

        /// <summary>
        /// 작업이 취소되어 끝난 것으로 처리합니다.
        /// </summary>
        public void SetCanceled()
        {
            lock (this)
                m_ExpectsCanceled = true;

            if (!Future.Cancel(m_Future))
            {
                if (m_Future.IsCanceled)
                    throw new FutureImpossibleException(FutureImpossibleReason.Canceled);

                else if (m_Future.IsFaulted)
                    throw new FutureImpossibleException(FutureImpossibleReason.Faulted);

                throw new FutureImpossibleException(FutureImpossibleReason.Completed);
            }
        }
    }

    /// <summary>
    /// 사용자 코드에 의해 상태가 제어되는 작업 객체를 만듭니다.
    /// </summary>
    public partial class FutureSource<ResultType>
    {
        private SourcedFuture m_Future;
        private bool m_ExpectsCanceled;

        /// <summary>
        /// 사용자 코드에 의해 상태가 제어되는 작업 객체를 초기화합니다.
        /// </summary>
        public FutureSource()
        {
            m_ExpectsCanceled = false;
            m_Future = new SourcedFuture(this);
        }

        /// <summary>
        /// 작업 객체입니다.
        /// </summary>
        public Future<ResultType> Future => m_Future;

        /// <summary>
        /// Future 객체에서 취소를 요청하면 실행되는 이벤트입니다.
        /// </summary>
        public event EventHandler Canceled;

        /// <summary>
        /// 취소 이벤트를 발생시킵니다.
        /// </summary>
        internal void RaiseCanceled()
        {
            if (!m_ExpectsCanceled)
                Canceled?.Invoke(m_Future, EventArgs.Empty);
        }

        /// <summary>
        /// 작업이 성공적으로 끝난 것으로 처리합니다. (예외 발생하지 않음)
        /// </summary>
        /// <returns></returns>
        public bool TrySetCompleted(ResultType Result) => m_Future.SetCompletion(Result);

        /// <summary>
        /// 작업이 취소되어 끝난 것으로 처리합니다. (예외 발생하지 않음)
        /// </summary>
        /// <returns></returns>
        public bool TrySetFaulted(Exception e) => m_Future.SetCompletion(e);

        /// <summary>
        /// 작업이 취소되어 끝난 것으로 처리합니다. (예외 발생하지 않음)
        /// </summary>
        public bool TrySetCanceled()
        {
            lock (this)
                m_ExpectsCanceled = true;

            return Future.Cancel(m_Future);
        }

        /// <summary>
        /// 작업이 성공적으로 끝난 것으로 처리합니다.
        /// </summary>
        public void SetCompleted(ResultType Result)
        {
            if (!m_Future.SetCompletion(Result))
            {
                if (m_Future.IsCanceled)
                    throw new FutureImpossibleException(FutureImpossibleReason.Canceled);

                else if (m_Future.IsFaulted)
                    throw new FutureImpossibleException(FutureImpossibleReason.Faulted);

                throw new FutureImpossibleException(FutureImpossibleReason.Completed);
            }
        }

        /// <summary>
        /// 작업이 취소되어 끝난 것으로 처리합니다.
        /// </summary>
        public void SetFaulted(Exception e)
        {
            if (!m_Future.SetCompletion(e))
            {
                if (m_Future.IsCanceled)
                    throw new FutureImpossibleException(FutureImpossibleReason.Canceled);

                else if (m_Future.IsFaulted)
                    throw new FutureImpossibleException(FutureImpossibleReason.Faulted);

                throw new FutureImpossibleException(FutureImpossibleReason.Completed);
            }
        }

        /// <summary>
        /// 작업이 취소되어 끝난 것으로 처리합니다.
        /// </summary>
        public void SetCanceled()
        {
            lock (this)
                m_ExpectsCanceled = true;

            if (!Future.Cancel(m_Future))
            {
                if (m_Future.IsCanceled)
                    throw new FutureImpossibleException(FutureImpossibleReason.Canceled);

                else if (m_Future.IsFaulted)
                    throw new FutureImpossibleException(FutureImpossibleReason.Faulted);

                throw new FutureImpossibleException(FutureImpossibleReason.Completed);
            }
        }
    }
}
