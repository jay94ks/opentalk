using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Utilities
{
    public class StateTaskSource<StateType>
    {
        private TaskCompletionSource<StateType> m_TCS
            = new TaskCompletionSource<StateType>();

        private bool m_Set = false;

        /// <summary>
        /// 상태 객체에 따른 작업 소스입니다.
        /// </summary>
        public StateTaskSource()
        {
            m_Set = false;
        }

        /// <summary>
        /// 상태 객체에 따라 작업 완료 여부가 달라지는 작업 원본입니다.
        /// </summary>
        public StateTaskSource(StateType InitialState)
        {
            m_TCS.SetResult(InitialState);
            m_Set = true;
        }

        ~StateTaskSource()
        {
            if (!m_Set)
                m_TCS.SetCanceled();
        }

        /// <summary>
        /// 작업 객체를 획득합니다.
        /// </summary>
        public Task<StateType> Task => this.Locked(() => m_TCS.Task);

        /// <summary>
        /// 상태 객체를 설정합니다.
        /// </summary>
        /// <param name="State"></param>
        public void Set(StateType State)
        {
            lock (this)
            {
                Unset();

                m_TCS.SetResult(State);
                m_Set = true;
            }
        }

        /// <summary>
        /// 상태 객체를 제거합니다.
        /// </summary>
        public void Unset()
        {
            lock (this)
            {
                if (m_Set)
                {
                    m_TCS = new TaskCompletionSource<StateType>();
                    m_Set = false;
                }
            }
        }
    }

    public class StateTaskSource : StateTaskSource<object>
    {
        public StateTaskSource()
        {
        }

        public StateTaskSource(object InitialState) : base(InitialState)
        {
        }
    }
}
