using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FormTimer = System.Windows.Forms.Timer;

namespace OpenTalk.Tasks.Internals
{
    /// <summary>
    /// UI 쓰레드, 즉 메인 쓰레드에서 동작하는 작업 객체를 표현합니다.
    /// </summary>
    internal class UIActionFuture : Future, IChainedFuture
    {
        private static FormTimer m_Timer = new FormTimer() { Interval = 10 };
        private static Queue<UIActionFuture> m_UIActions = new Queue<UIActionFuture>();
        private static Thread m_CurrentThread = null;

        /// <summary>
        /// 타이머를 시작시킵니다.
        /// </summary>
        static UIActionFuture()
        {
            m_CurrentThread = Thread.CurrentThread;
            m_Timer.Tick += OnTimerTick;
            m_Timer.Start();
        }

        private static void OnTimerTick(object sender, EventArgs e)
        {
            UIActionFuture Action = null;
            m_CurrentThread = Thread.CurrentThread;

            while (true)
            {
                lock(m_UIActions)
                {
                    if (m_UIActions.Count <= 0)
                        break;

                    Action = m_UIActions.Dequeue();
                }

                Action.Invoke();
                Thread.Yield();
            }
        }

        private FutureSource m_Future;
        private Action m_Action;
        private bool m_Queued, m_WannaCancel, m_Running;

        /// <summary>
        /// 즉시 실행되는 UI 작업을 생성합니다.
        /// </summary>
        /// <param name="Action"></param>
        public UIActionFuture(Action Action)
        {
            m_Action = Action;
            (m_Future = new FutureSource())
                .Future.Then(OnFinish);

            m_Queued = true;
            m_Running = false;
            m_WannaCancel = false;

            if (m_CurrentThread != Thread.CurrentThread)
            {
                lock (m_UIActions)
                    m_UIActions.Enqueue(this);
            }

            else Invoke();
        }

        /// <summary>
        /// 지정된 작업이 완료되면 실행되는 UI 작업을 생성합니다.
        /// </summary>
        /// <param name="Previous"></param>
        /// <param name="Action"></param>
        public UIActionFuture(Future Previous, Action Action)
        {
            m_Action = Action;
            (m_Future = new FutureSource())
                .Future.Then(OnFinish);

            m_Queued = false;
            m_Running = false;
            m_WannaCancel = false;

            // 이전 작업의 체인에 이 작업을 등록합니다.
            Previous.Chain(this);
        }

        /// <summary>
        /// 작업의 상태를 확인합니다.
        /// </summary>
        public override FutureStatus Status {
            get {
                lock(this)
                {
                    if (!m_Queued)
                        return FutureStatus.Scheduled;
                }

                return m_Future.Future.Status;
            }
        }

        /// <summary>
        /// 작업이 종료될 때 까지 대기합니다.
        /// </summary>
        public override bool Wait() => m_Future.Future.Wait();

        /// <summary>
        /// 작업이 종료될 때 까지 대기합니다.
        /// </summary>
        public override bool Wait(int Milliseconds) => m_Future.Future.Wait(Milliseconds);

        /// <summary>
        /// 이전 작업에서 이 작업을 계속 진행시키기 위해 호출합니다.
        /// </summary>
        public void Fire()
        {
            lock (this)
            {
                if (m_Queued)
                    return;

                if (m_WannaCancel)
                {
                    m_Future.TrySetCanceled();
                    return;
                }

                lock (m_UIActions)
                    m_UIActions.Enqueue(this);

                m_Queued = true;
            }
        }

        /// <summary>
        /// 실제로 이 작업을 실행시킵니다.
        /// </summary>
        public void Invoke()
        {
            lock (this)
            {
                m_Running = true;
                if (m_WannaCancel)
                {
                    m_Future.TrySetCanceled();
                    return;
                }
            }

            if (Debugger.IsAttached)
            {
                m_Action();
                m_Future.TrySetCompleted();
            }

            else
            {
                try
                {
                    m_Action();
                    m_Future.TrySetCompleted();
                }
                catch (Exception e)
                {
                    m_Future.TrySetFaulted(e);
                }
            }
        }

        /// <summary>
        /// 이 작업을 취소합니다.
        /// </summary>
        protected override void OnCancel()
        {
            // 이미 시작되었으면
            // 그냥 포기합니다.

            if (m_Running)
                return;

            if (!m_Queued)
            {
                if (!m_WannaCancel)
                {
                    m_WannaCancel = true;
                    m_Future.TrySetCanceled();
                }

                return;
            }

            m_WannaCancel = true;
        }
    }
}
