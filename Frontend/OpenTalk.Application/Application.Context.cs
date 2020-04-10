using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using DApp = System.Windows.Forms.Application;
using DContext = System.Windows.Forms.ApplicationContext;
using DTimer = System.Windows.Forms.Timer;

namespace OpenTalk
{
    public abstract partial class Application
    {
        private class Context : DContext
        {
            private Application m_Application;

            private DTimer m_InvokeTimer;
            private Queue<Action> m_Invokes;
            private Thread m_ContextThread;

            private bool m_Initiated;
            private bool m_Exiting;
            private bool m_Running;

            /// <summary>
            /// 어플리케이션 컨텍스트를 초기화합니다.
            /// </summary>
            /// <param name="application"></param>
            public Context(Application application)
            {
                m_Application = application;
                m_ContextThread = null;

                m_Invokes = new Queue<Action>();
                m_Initiated = m_Exiting = false;

                (m_InvokeTimer = new DTimer() { Interval = 5 })
                    .Tick += OnTick;

                m_InvokeTimer.Stop();
                ThreadExit += OnThreadExit;

                m_Running = false;
                m_ContextThread = Thread.CurrentThread;

                if (Log.HasConsole)
                {
                    Console.CancelKeyPress += OnProcessExit;
                    AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
                }
            }

            /// <summary>
            /// 어플리케이션이 종료될 예정입니다. (콘솔 앱인 경우)
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void OnProcessExit(object sender, System.EventArgs e) => OnThreadExit(sender, e);

            /// <summary>
            /// 어플리케이션이 종료될 예정입니다.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void OnThreadExit(object sender, System.EventArgs e)
            {
                lock(this)
                {
                    if (m_Exiting || !m_Running)
                        return;

                    m_Exiting = true;
                    m_Running = false;
                }

                m_ContextThread = Thread.CurrentThread;
                m_Application.Events.InvokeDeInitialize();
                m_Application.DeInitialize();

                Component.DeactivateAll(m_Application);

                if (Log.HasConsole)
                {
                    Environment.Exit(0);
                }
            }

            /// <summary>
            /// 이 속성을 확인하는 메서드가 컨텍스트 쓰레드와 동일한 쓰레드에서 실행중인지 검사합니다.
            /// </summary>
            public bool IsContextThread => m_ContextThread == Thread.CurrentThread;

            /// <summary>
            /// 이 어플리케이션 인스턴스가 종료되고 있는 중인지 검사합니다.
            /// </summary>
            public bool IsExiting {
                get {
                    lock (this)
                        return m_Exiting;
                }
            }

            /// <summary>
            /// 어플리케이션 컨텍스트를 실행시킵니다.
            /// </summary>
            public void Run()
            {
                lock (this)
                    m_Running = true;

                m_InvokeTimer.Start();
                DApp.Run(this);
            }

            /// <summary>
            /// functor를 메시지 루프 내에서 실행시킵니다.
            /// 즉시 실행된 경우, true를 반환하며,
            /// 즉시 실행되지 않은 경우, false를 반환합니다.
            /// 
            /// 이 컨텍스트가 이미 종료되고 있는 중일 때엔, 
            /// 모든 펑터를 즉시 실행시킵니다.
            /// </summary>
            /// <param name="functor"></param>
            /// <param name="shouldEnqueue">이 값이 true인 경우, 지정된 펑터는 반드시 다음 틱에서 실행됩니다.</param>
            /// <returns></returns>
            public bool Invoke(Action functor, bool shouldEnqueue = false)
            {
                if (!IsExiting && (shouldEnqueue || !IsContextThread))
                {
                    lock (m_Invokes)
                        m_Invokes.Enqueue(functor);

                    return false;
                }

                functor();
                return true;
            }

            /// <summary>
            /// Invoke 타깃으로 등록된 이벤트를 메시지 루프 내에서 실행시킵니다.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void OnTick(object sender, System.EventArgs e)
            {
                m_ContextThread = Thread.CurrentThread;

                if (!m_Initiated)
                {
                    m_Application.PreInitialize();
                    m_Application.Events.InvokePreInitialize();

                    m_Initiated = true;
                    Task.Run(() =>
                    {
                        m_Application.Initialize();
                        m_Application.Events.InvokeInitialize();
                    });
                }

                lock (m_Invokes)
                {
                    while (m_Invokes.Count > 0)
                        m_Invokes.Dequeue().Invoke();
                }
            }
        }
    }
}
