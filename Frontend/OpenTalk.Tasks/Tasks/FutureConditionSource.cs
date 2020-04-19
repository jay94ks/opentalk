using OpenTalk.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Tasks
{
    /// <summary>
    /// 특정 조건이 충족되면 완료되는 작업을 만듭니다.
    /// </summary>
    public class FutureConditionSource : IFutureSource
    {
        private FutureSource m_Future;
        private Func<bool> m_Condition;
        private bool m_CancelRequested;

        /// <summary>
        /// 특정 조건이 충족되면 완료되는 작업을 만듭니다.
        /// </summary>
        public FutureConditionSource(Func<bool> Condition)
        {
            m_Condition = Condition;
            m_CancelRequested = false;

            (m_Future = new FutureSource())
                .Canceled += OnCancelRequested;

            TrickyPollingLoop.Poll(OnCondition, OnCompletion);
        }

        /// <summary>
        /// 현재 상태와 연관된 작업 객체입니다.
        /// </summary>
        public Future Future => m_Future.Future;

        /// <summary>
        /// 사용자 정의 태그입니다.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// 취소 요청이 들어오면 취소 플래그를 셋팅합니다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCancelRequested(object sender, EventArgs e)
        {
            lock (this)
                m_CancelRequested = true;
        }

        /// <summary>
        /// 조건이 충족되었는지 검사합니다.
        /// </summary>
        /// <returns></returns>
        private bool OnCondition()
        {
            lock (this)
            {
                if (m_CancelRequested)
                    return true;
            }

            return m_Condition();
        }

        /// <summary>
        /// 조건이 충족되면 Future 객체도 완료시킵니다.
        /// </summary>
        private void OnCompletion()
        {
            lock (this)
            {
                if (m_CancelRequested)
                    m_Future.TrySetCanceled();

                else m_Future.TrySetCompleted();
            }
        }
    }
}
