using OpenTalk.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenTalk.Helpers
{
    internal class TrickyPollingLoop
    {
        private static Future m_Future = null;
        private static Queue<KeyValuePair<Func<bool>, Action>> m_PendingTasks
            = new Queue<KeyValuePair<Func<bool>, Action>>();

        private static Queue<KeyValuePair<Func<bool>, Action>> m_CurrentTasks
            = new Queue<KeyValuePair<Func<bool>, Action>>();

        private static Action PollingLoop => Debugger.IsAttached ?
                (Action)DoPollingWithDebugger : DoPolling;

        /// <summary>
        /// 폴링 타겟을 추가합니다.
        /// </summary>
        /// <param name="Target"></param>
        /// <param name="Completion"></param>
        public static void Poll(Func<bool> Target, Action Completion)
        {
            lock (m_PendingTasks)
            {
                m_PendingTasks.Enqueue(new KeyValuePair<Func<bool>, Action>(Target, Completion));

                if (m_Future == null ||
                    m_Future.IsCompleted)
                {
                    m_Future = Future.Run(PollingLoop);
                }
            }
        }

        /// <summary>
        /// 폴링을 수행합니다. (디버거 없음)
        /// </summary>
        private static void DoPolling()
        {
            SwapWorkingQueue();

            while (m_CurrentTasks.Count > 0)
            {
                var Task = m_CurrentTasks.Dequeue();

                try
                {
                    if (Task.Key())
                        Task.Value();

                    else lock (m_PendingTasks)
                            m_PendingTasks.Enqueue(Task);
                }
                catch { }

                Thread.Yield();
            }

            // 전체를 루프로 감싸는 대신 작업을 새로 시작시켜서
            // 쓰레드 풀이 차폐(Block)되는걸 막습니다.
            lock (m_PendingTasks)
            {
                m_Future = m_PendingTasks.Count > 0 ?
                    Future.Run(PollingLoop) : null;
            }
        }

        /// <summary>
        /// 폴링을 수행합니다.
        /// </summary>
        private static void DoPollingWithDebugger()
        {
            SwapWorkingQueue();

            while(m_CurrentTasks.Count > 0)
            {
                var Task = m_CurrentTasks.Dequeue();

                if (Task.Key())
                    Task.Value();

                else lock (m_PendingTasks)
                    m_PendingTasks.Enqueue(Task);

                Thread.Yield();
            }

            // 전체를 루프로 감싸는 대신 작업을 새로 시작시켜서
            // 쓰레드 풀이 차폐(Block)되는걸 막습니다.
            lock (m_PendingTasks)
            {
                m_Future = m_PendingTasks.Count > 0 ?
                    Future.Run(PollingLoop) : null;
            }
        }

        /// <summary>
        /// 대기 큐와 작업 큐를 스왑합니다.
        /// </summary>
        private static void SwapWorkingQueue()
        {
            lock (m_PendingTasks)
            {
                var Temp = m_CurrentTasks;
                m_CurrentTasks = m_PendingTasks;
                m_PendingTasks = Temp;
            }
        }
    }
}
