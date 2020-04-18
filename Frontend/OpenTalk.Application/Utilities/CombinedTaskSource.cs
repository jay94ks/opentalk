using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Utilities
{
    /// <summary>
    /// 한개 혹은 그 이상의 작업들이 완료되면,
    /// 다음 작업을 진행시키는 객체입니다.
    /// </summary>
    public class CombinedTaskSource<ResultType>
    {
        private bool m_ShouldAsync;
        private int m_CompletionWaits;
        private Func<ResultType> m_Callback;
        private TaskCompletionSource<ResultType> m_TCS;

        /// <summary>
        /// 한개 혹은 그 이상의 작업들이 완료되면,
        /// 다음 작업을 진행시키는 객체입니다.
        /// </summary>
        /// <param name="Tasks"></param>
        public CombinedTaskSource(params Task[] Tasks)
            : this(() => default(ResultType), Tasks)
        {
        }

        /// <summary>
        /// 한개 혹은 그 이상의 작업들이 완료되면,
        /// 다음 작업을 진행시키는 객체입니다.
        /// </summary>
        /// <param name="Tasks"></param>
        public CombinedTaskSource(Func<ResultType> Callback, params Task[] Tasks)
        {
            m_ShouldAsync = true;
            m_Callback = Callback;
            m_CompletionWaits = Tasks.Length;
            m_TCS = new TaskCompletionSource<ResultType>();

            // 작업 완료시 연속적으로 실행할 작업 Functor를 등록하거나 즉시 실행시킵니다.
            foreach (Task Each in Tasks)
            {
                if (Each.IsCompleted || Each.IsCanceled)
                    OnTaskCompletion(Each);

                else Each.ContinueWith(OnTaskCompletion);
            }

            lock (this)
                m_ShouldAsync = false;
        }

        /// <summary>
        /// 이 작업 원본이 생성한 작업 객체입니다.
        /// </summary>
        public Task<ResultType> Task => m_TCS.Task;

        /// <summary>
        /// 개별 작업의 완료를 처리합니다.
        /// </summary>
        /// <param name="task"></param>
        private void OnTaskCompletion(Task task)
        {
            lock(this)
            {
                m_CompletionWaits--;

                if (m_CompletionWaits <= 0)
                {
                    if (m_ShouldAsync)
                    {
                        System.Threading.Tasks.Task.Run(
                            () => m_TCS.SetResult(m_Callback()));

                        return;
                    }

                    m_TCS.SetResult(m_Callback());
                }
            }
        }
    }
}
