using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Tasks
{
    /// <summary>
    /// Future 객체를 이용한 이벤트 객체입니다.
    /// </summary>
    public class FutureEventSource : IFutureSource
    {
        private FutureSource m_Source;
        private bool m_WasSet;

        /// <summary>
        /// Future 객체를 이용한 이벤트 객체입니다.
        /// </summary>
        public FutureEventSource()
        {
            m_Source = new FutureSource();
            m_WasSet = false;

            Sender = this;
        }

        /// <summary>
        /// 현재 이벤트 상태와 연관된 작업 객체입니다.
        /// </summary>
        public Future Future {
            get { lock (this) return m_Source.Future; }
        }

        /// <summary>
        /// 사용자 정의 태그입니다.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Changed 이벤트의 sender인자로 사용될 객체를 설정합니다.
        /// </summary>
        public object Sender { get; set; }

        /// <summary>
        /// 이 객체의 상태가 변하면 발생하는 이벤트입니다.
        /// </summary>
        public event EventHandler Changed;

        /// <summary>
        /// 이벤트의 실행 인자를 생성하는 팩토리 콜백입니다.
        /// </summary>
        public Func<FutureEventSource, EventArgs> EventArgs { get; set; }

        /// <summary>
        /// 이벤트를 설정합니다.
        /// </summary>
        public virtual void Set()
        {
            lock(this)
            {
                if (m_WasSet)
                    return;

                m_WasSet = true;
                m_Source.SetCompleted();

                Changed?.Invoke(Sender, EventArgs != null ?
                    EventArgs(this) : System.EventArgs.Empty);
            }
        }

        /// <summary>
        /// 이벤트를 해제합니다.
        /// </summary>
        public void Unset()
        {
            bool FlagChanged = false;

            lock (this)
            {
                if (m_WasSet)
                {
                    m_Source = new FutureSource();
                    FlagChanged = true;
                }

                m_WasSet = false;

                if (FlagChanged)
                {
                    Changed?.Invoke(Sender, EventArgs != null ?
                        EventArgs(this) : System.EventArgs.Empty);
                }
            }
        }
    }

    /// <summary>
    /// Future 객체를 이용한 이벤트 객체입니다.
    /// </summary>
    public class FutureEventSource<ResultType>
    {
        private FutureSource<ResultType> m_Source;
        private ResultType m_LatestResult;
        private bool m_WasSet, m_NoResult;

        public FutureEventSource()
        {
            m_Source = new FutureSource<ResultType>();
            m_NoResult = true;
            m_WasSet = false;
            Sender = this;
        }

        /// <summary>
        /// 현재 이벤트 상태와 연관된 작업 객체입니다.
        /// </summary>
        public Future<ResultType> Future {
            get { lock (this) return m_Source.Future; }
        }

        /// <summary>
        /// 사용자 정의 태그입니다.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Changed 이벤트의 sender인자로 사용될 객체를 설정합니다.
        /// </summary>
        public object Sender { get; set; }

        /// <summary>
        /// 이 객체의 상태가 변하면 발생하는 이벤트입니다.
        /// </summary>
        public event EventHandler Changed;

        /// <summary>
        /// 이벤트의 실행 인자를 생성하는 팩토리 콜백입니다.
        /// </summary>
        public Func<FutureEventSource<ResultType>, EventArgs> EventArgs { get; set; }

        /// <summary>
        /// 이벤트를 설정합니다.
        /// (Future 객체 상태가 취소된 것으로 설정됩니다)
        /// </summary>
        public void Set()
        {
            lock (this)
            {
                if (m_WasSet)
                {
                    if (m_NoResult)
                        return;

                    m_Source = new FutureSource<ResultType>();
                }

                m_NoResult = true;
                m_WasSet = true;

                m_Source.SetCanceled();
                Changed?.Invoke(Sender, EventArgs != null ?
                    EventArgs(this) : System.EventArgs.Empty);
            }
        }

        /// <summary>
        /// 이벤트를 설정합니다.
        /// </summary>
        public void Set(ResultType Result)
        {
            lock (this)
            {
                if (m_WasSet)
                {
                    if (!m_NoResult && (object)m_LatestResult == (object)Result)
                        return;

                    m_Source = new FutureSource<ResultType>();
                }

                m_NoResult = false;
                m_WasSet = true;

                m_Source.SetCompleted(Result);
                Changed?.Invoke(Sender, EventArgs != null ?
                    EventArgs(this) : System.EventArgs.Empty);
            }
        }

        /// <summary>
        /// 이벤트를 해제합니다.
        /// </summary>
        public void Unset()
        {
            bool FlagChanged = false;

            lock (this)
            {
                if (m_WasSet)
                {
                    m_Source = new FutureSource<ResultType>();
                    FlagChanged = true;
                }

                m_NoResult = true;
                m_WasSet = false;

                if (FlagChanged)
                {
                    Changed?.Invoke(Sender, EventArgs != null ?
                        EventArgs(this) : System.EventArgs.Empty);
                }
            }
        }
    }
}
