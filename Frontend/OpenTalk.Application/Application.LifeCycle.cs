using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenTalk
{
    public abstract partial class Application
    {
        public class LifeCycle
        {
            private Application m_Application;

            private List<EventHandler<EventArgs>> m_EvePreInit;
            private List<EventHandler<EventArgs>> m_EveInit;
            private List<EventHandler<EventArgs>> m_EveDeInit;

            private TaskCompletionSource<EventArgs> m_TaskPreInit;
            private TaskCompletionSource<EventArgs> m_TaskInit;
            private TaskCompletionSource<EventArgs> m_TaskDeinit;

            /// <summary>
            /// 이벤트 컨테이너를 초기화합니다.
            /// </summary>
            internal LifeCycle(Application application)
            {
                m_Application = application;

                m_EvePreInit = new List<EventHandler<EventArgs>>();
                m_EveInit = new List<EventHandler<EventArgs>>();
                m_EveDeInit = new List<EventHandler<EventArgs>>();

                m_TaskPreInit = new TaskCompletionSource<EventArgs>();
                m_TaskInit = new TaskCompletionSource<EventArgs>();
                m_TaskDeinit = new TaskCompletionSource<EventArgs>();
            }

            /// <summary>
            /// 어플리케이션을 초기화하기 전, 필요한 작업들을 수행합니다.
            /// </summary>
            public event EventHandler<EventArgs> PreInitialize {
                add { lock (m_EvePreInit) m_EvePreInit.Add(value); }
                remove { lock (m_EvePreInit) m_EvePreInit.Remove(value); }
            }

            /// <summary>
            /// 어플리케이션을 초기화합니다.
            /// 주의: 이 이벤트는 주 쓰레드가 아닌 쓰레드에서 실행됩니다.
            /// </summary>
            public event EventHandler<EventArgs> Initialize {
                add { lock (m_EveInit) m_EveInit.Add(value); }
                remove { lock (m_EveInit) m_EveInit.Remove(value); }
            }

            /// <summary>
            /// 어플리케이션이 종료되기 전 실행됩니다.
            /// </summary>
            public event EventHandler<EventArgs> DeInitialize {
                add { lock (m_EveDeInit) m_EveDeInit.Add(value); }
                remove { lock (m_EveDeInit) m_EveDeInit.Remove(value); }
            }

            /// <summary>
            /// 어플리케이션의 사전 초기화가 완료되면 완료되는 Task 객체입니다.
            /// </summary>
            public Task<EventArgs> PreInitializeTask => m_TaskPreInit.Task;

            /// <summary>
            /// 어플리케이션의 초기화가 완료되면 완료되는 Task 객체입니다.
            /// </summary>
            public Task<EventArgs> InitializeTask => m_TaskPreInit.Task;

            /// <summary>
            /// 어플리케이션의 종료되기 직전에 완료되는 Task 객체입니다.
            /// </summary>
            public Task<EventArgs> DeInitializeTask => m_TaskPreInit.Task;

            /// <summary>
            /// PreInitialize 이벤트를 발생시킵니다.
            /// </summary>
            internal void InvokePreInitialize() => InvokeEvents(m_EvePreInit, m_TaskPreInit);

            /// <summary>
            /// Initialize 이벤트를 발생시킵니다.
            /// </summary>
            internal void InvokeInitialize() => InvokeEvents(m_EveInit, m_TaskInit);

            /// <summary>
            /// DeInitialize 이벤트를 발생시킵니다.
            /// </summary>
            internal void InvokeDeInitialize() => InvokeEvents(m_EveDeInit, m_TaskDeinit);

            /// <summary>
            /// 지정된 리스트에 포함된 이벤트 핸들러들을 실행시킵니다.
            /// </summary>
            /// <param name="Handlers"></param>
            private void InvokeEvents(
                List<EventHandler<EventArgs>> Handlers,
                TaskCompletionSource<EventArgs> TCS)
            {
                EventArgs EventArgs = new EventArgs(m_Application);

                lock (Handlers)
                {
                    foreach (EventHandler<EventArgs> Handler in Handlers.ToArray())
                        Handler(m_Application, EventArgs);
                }

                TCS.SetResult(EventArgs);
            }
        }
    }
}
