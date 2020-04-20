using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Tasks
{
    public class FutureEventCombination : IFutureSource
    {
        private IFutureEventSource[] m_Sources;
        private FutureEventSource m_OutputSource;

        /// <summary>
        /// 하나 이상의 이벤트 원본을 종합,
        /// 최종 상태를 생성하는 작업 원본입니다.
        /// </summary>
        /// <param name="Sources"></param>
        public FutureEventCombination(params IFutureEventSource[] Sources)
        {
            m_Sources = Sources;
            m_OutputSource = new FutureEventSource();

            OnChanged(this, EventArgs.Empty);
            foreach (IFutureEventSource Source in Sources)
                Source.Changed += OnChanged;
        }

        /// <summary>
        /// 현재 이벤트 상태와 연관된 작업 객체입니다.
        /// </summary>
        public Future Future => m_OutputSource.Future;

        /// <summary>
        /// 사용자 정의 태그입니다.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// 상태 플래그가 변화되면 실행되는 이벤트입니다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnChanged(object sender, EventArgs e)
        {
            int CompletionCounts = 0;

            lock (this)
            {
                foreach (IFutureEventSource Source in m_Sources)
                {
                    if (Source.Future.IsCompleted)
                        ++CompletionCounts;
                }

                if (CompletionCounts >= m_Sources.Length)
                    m_OutputSource.Set();

                else m_OutputSource.Unset();
            }
        }
    }
}
