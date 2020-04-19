using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenTalk.Helpers
{
    /// <summary>
    /// 할당/해제 메커니즘이 적용된 ManualResetEvent 객체입니다.
    /// </summary>
    public class TrickyManualEvent
    {
        private static ObjectPool<ManualResetEvent> m_MREs
            = new ObjectPool<ManualResetEvent>(() => new ManualResetEvent(false));

        private int m_WaitLoops;
        private bool m_CurrentState;
        private ManualResetEvent m_Event;

        /// <summary>
        /// 할당/해제 메커니즘이 적용된 ManualResetEvent 객체입니다.
        /// TrickyManualEvent 객체의 갯수보다 적은 ManualResetEvent 객체로
        /// 동일한 동작을 구현합니다.
        /// 
        /// (대기 루프가 있어야 ManualResetEvent가 할당되는 방식입니다)
        /// </summary>
        /// <param name="InitialState"></param>
        public TrickyManualEvent(bool InitialState)
        {
            m_WaitLoops = 0;
            m_CurrentState = InitialState;
            m_Event = null;
        }

        /// <summary>
        /// 이벤트를 리셋합니다.
        /// </summary>
        /// <returns></returns>
        public bool Reset()
        {
            lock (this)
            {
                m_CurrentState = false;
                NotifyState();
            }

            return true;
        }

        /// <summary>
        /// 이벤트를 설정합니다.
        /// </summary>
        /// <returns></returns>
        public bool Set()
        {
            lock (this)
            {
                m_CurrentState = true;
                NotifyState();
            }

            return true;
        }

        /// <summary>
        /// 이벤트가 설정될 때 까지 대기합니다.
        /// </summary>
        /// <returns></returns>
        public bool WaitOne()
        {
            EnterWaitLoop();
            while (true)
            {
                lock (this)
                {
                    // 신호가 있을 땐 이벤트 객체를 반납합니다.
                    if (m_CurrentState)
                    {
                        ReleaseMRE();
                        return true;
                    }

                    AllocateMRE();
                }

                // 신호를 대기합니다.
                m_Event.WaitOne();
            }
        }

        /// <summary>
        /// 이벤트가 설정될 때 까지 대기합니다.
        /// </summary>
        /// <returns></returns>
        public bool WaitOne(int Milliseconds)
        {
            if (Milliseconds < 0)
                return WaitOne();

            else if (Milliseconds == 0)
            {
                lock (this)
                    return m_CurrentState;
            }

            else
            {
                DateTime Checkpoint = DateTime.Now;
                int LeftMilliseconds = Milliseconds;

                EnterWaitLoop();
                while (true)
                {
                    lock (this)
                    {
                        // 신호가 있을 땐 이벤트 객체를 반납합니다.
                        if (m_CurrentState)
                        {
                            ReleaseMRE();
                            return true;
                        }

                        // 타임 아웃에 도달한 경우,
                        // MRE를 반납하고 루프롤 종료합니다.
                        if (LeftMilliseconds <= 0)
                        {
                            ReleaseMRE();
                            break;
                        }

                        AllocateMRE();
                    }

                    // 최대 1초 간격으로 신호를 대기합니다.
                    m_Event.WaitOne(LeftMilliseconds < 1000 ? LeftMilliseconds : 1000);
                    LeftMilliseconds = Math.Max(0, (int)(Milliseconds -
                        (DateTime.Now - Checkpoint).TotalMilliseconds));
                    break;
                }
            }

            return false;
        }

        /// <summary>
        /// 상태 변화를 통보합니다.
        /// </summary>
        private void NotifyState()
        {
            if (m_Event != null)
            {
                if (m_CurrentState)
                    m_Event.Set();

                else m_Event.Reset();
            }
        }

        /// <summary>
        /// 대기 루프로 진입할 때 카운터를 증분시킵니다.
        /// </summary>
        private void EnterWaitLoop()
        {
            lock (this)
                m_WaitLoops++;
        }

        /// <summary>
        /// 이벤트 객체가 필요한 상태, 즉, 상태가 false인 상태에서
        /// 대기 루프에 진입하면 MRE를 할당받고, 수신상태를 false로 마크합니다.
        /// </summary>
        private void AllocateMRE()
        {
            if (m_Event == null)
                (m_Event = m_MREs.Alloc()).Reset();
        }

        /// <summary>
        /// 현재 상태 변화를 수신하는 루프가 하나라도 살아 있다면, MRE를 반납하지 않습니다.
        /// </summary>
        private void ReleaseMRE()
        {
            m_WaitLoops--;

            if (m_WaitLoops <= 0)
            {
                if (m_Event != null)
                {
                    m_MREs.Free(m_Event);
                    m_Event = null;
                }

                // 음수가 된 경우를 대비.
                m_WaitLoops = 0;
            }
        }
    }
}
