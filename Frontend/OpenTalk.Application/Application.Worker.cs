using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenTalk
{
    public abstract partial class Application
    {
        /// <summary>
        /// 어플리케이션 작업자 쓰레드를 구현합니다.
        /// 어플리케이션 쓰레드는 메시지 루프 형태로 동작하는 작업자이며,
        /// 어플리케이션 종료에 맞춰, 자동으로 종료 처리를 수행합니다.
        /// </summary>
        public sealed partial class Worker : BaseObject
        {
            private Thread m_Thread;
            private Thread m_Comparison;

            private bool m_KeepRunning;
            private bool m_ReallyRunning;

            private List<Action> m_Loops;
            private Queue<Action> m_Actions;
            private ManualResetEvent m_Synchronizer;

            /// <summary>
            /// 작업자 인스턴스를 초기화합니다.
            /// 단, 현재 실행중인 어플리케이션 인스턴스가 없을 때,
            /// ApplicationException 예외가 발생합니다.
            /// </summary>
            public Worker() : this(WorkUnit.Task) { }

            /// <summary>
            /// 작업자 인스턴스를 초기화합니다.
            /// 단, 현재 실행중인 어플리케이션 인스턴스가 없을 때,
            /// ApplicationException 예외가 발생합니다.
            /// </summary>
            public Worker(WorkUnit workUnit) : this(null, workUnit) { }

            /// <summary>
            /// 작업자 인스턴스를 초기화합니다.
            /// 단, 현재 실행중인 어플리케이션 인스턴스가 없을 때,
            /// ApplicationException 예외가 발생합니다.
            /// </summary>
            public Worker(params Action[] loopCallbacks) : this(null, loopCallbacks) { }

            /// <summary>
            /// 작업자 인스턴스를 초기화합니다.
            /// </summary>
            public Worker(Application application) : this(application, WorkUnit.Task) { }

            /// <summary>
            /// 작업자 인스턴스를 초기화합니다.
            /// </summary>
            public Worker(Application application, WorkUnit workUnit)
                : base(application) => Initialize(workUnit);

            /// <summary>
            /// 작업자 인스턴스를 초기화합니다.
            /// </summary>
            public Worker(Application application, params Action[] loopCallbacks)
                : this(application, WorkUnit.Loop)
            {
                foreach (Action eachCallback in loopCallbacks)
                    AddLoop(eachCallback);
            }

            /// <summary>
            /// 작업자 인스턴스를 초기화합니다.
            /// </summary>
            /// <param name="application"></param>
            private void Initialize(WorkUnit workUnit)
            {
                m_KeepRunning = true;
                m_ReallyRunning = false;
                WorkingUnit = workUnit;

                m_Actions = new Queue<Action>();
                m_Loops = new List<Action>();

                m_Synchronizer = new ManualResetEvent(false);
                m_Thread = null;
            }

            /// <summary>
            /// 작업자의 동작 모드를 확인합니다.
            /// </summary>
            public WorkUnit WorkingUnit { get; private set; }

            /// <summary>
            /// 사용자 정의 상태입니다.
            /// </summary>
            public object UserState { get; set; }

            /// <summary>
            /// 이 속성을 확인하는 메서드가 작업자 쓰레드와 
            /// 동일한 쓰레드에서 실행중인지 검사합니다.
            /// </summary>
            public bool IsWorkerThread => IsAlive && m_Comparison == Thread.CurrentThread;

            /// <summary>
            /// 이 작업자 쓰레드가 살아 있는지 검사합니다.
            /// </summary>
            public bool IsAlive {
                get {
                    lock (this)
                    {
                        /*
                            작업자 쓰레드가 파괴되었거나, 
                            살아 있지 않거나, 살아 있지 않게될 예정인 경우엔
                            죽은 걸로 취급합니다.
                         */
                        return  m_Thread != null && 
                                m_Thread.IsAlive &&
                               !m_KeepRunning &&
                               !m_ReallyRunning;
                    }
                }
            }

            /// <summary>
            /// 작업자 쓰레드가 실행된 직후에 실행되는 이벤트입니다.
            /// (주의: 작업자 쓰레드 내부에서 호출됩니다)
            /// </summary>
            public event EventHandler<EventArgs> Started;

            /// <summary>
            /// 작업자 쓰레드가 종료될 때 실행되는 이벤트입니다.
            /// (주의: 작업자 쓰레드 내부에서 호출됩니다)
            /// </summary>
            public event EventHandler<EventArgs> Stopped;

            /// <summary>
            /// 큐 상태에 대한 이벤트입니다. (대기중인지, 작업을 처리하고 있는지에 관한 이벤트)
            /// (주의: 작업자 쓰레드 내부에서 호출됩니다)
            /// </summary>
            public event EventHandler<EventArgs> QueueEvent;

            /// <summary>
            /// 작업자 쓰레드를 시작시킵니다.
            /// </summary>
            /// <returns></returns>
            public bool Start()
            {
                lock (this)
                {
                    if (IsAlive)
                        return false;

                    switch(WorkingUnit)
                    {
                        case WorkUnit.Task:
                            m_Thread = new Thread(OnThreadMainForTask);
                            break;

                        case WorkUnit.Loop:
                            m_Thread = new Thread(OnThreadMainForLoop);
                            break;
                    }

                    // 어플리케이션 종료 이벤트 수신을 시작합니다.
                    Application.Events.DeInitialize += OnApplicationExiting;

                    m_KeepRunning = true;
                    m_ReallyRunning = false;

                    m_Synchronizer.Reset();
                    m_Thread.Start();
                }

                Thread.Yield();
                m_Synchronizer.WaitOne();
                return true;
            }

            /// <summary>
            /// 작업자 쓰레드를 종료시킵니다.
            /// </summary>
            /// <returns></returns>
            public bool Stop()
            {
                lock (this)
                {
                    if (!IsAlive)
                        return false;

                    // 어플리케이션 종료 이벤트를 더이상 수신하지 않습니다.
                    Application.Events.DeInitialize -= OnApplicationExiting;

                    m_Synchronizer.Reset();
                    m_KeepRunning = false;
                }

                Thread.Yield();
                m_Synchronizer.WaitOne();
                return true;
            }

            /// <summary>
            /// 어플리케이션이 종료되려고 할 때 실행되는 이벤트 핸들러입니다.
            /// (메인 쓰레드에서 호출됨)
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void OnApplicationExiting(object sender, Application.EventArgs e)
            {
                // 어플리케이션 종료 이벤트를 더이상 수신하지 않습니다.
                Application.Events.DeInitialize -= OnApplicationExiting;

                // 종료 절차를 시작합니다.
                lock (this)
                {
                    if (!IsAlive)
                        return;

                    m_Synchronizer.Reset();
                    m_KeepRunning = false;
                }

                Thread.Yield();
                m_Synchronizer.WaitOne();
            }

            /// <summary>
            /// 이 작업자에서 지정된 작업을 수행합니다.
            /// enforced 인자에 true가 지정되면, 작업자 쓰레드가 작동중이지 않은 경우에도 작업을 예약하며
            /// false가 지정되면, 작업자 쓰레드가 작동중이지 않은 경우에 InvalidOperation 예외가 발생합니다.
            /// </summary>
            /// <param name="functor"></param>
            /// <returns></returns>
            public Task Invoke(Action functor, bool enforced = false)
            {
                if (IsAlive || enforced)
                {
                    TaskCompletionSource<Worker> TCS
                        = new TaskCompletionSource<Worker>();

                    lock (m_Actions)
                    {
                        m_Actions.Enqueue(() =>
                        {
                            functor();
                            TCS.SetResult(this);
                        });
                    }

                    return TCS.Task;
                }

                throw new InvalidOperationException();
            }

            /// <summary>
            /// 이 작업자가 반복적으로 수행할 작업 루프를 등록합니다.
            /// 작업 모드가 Loop가 아닌 경우, 항상 실패합니다.
            /// </summary>
            /// <param name="callback"></param>
            /// <returns></returns>
            public bool AddLoop(Action callback)
            {
                lock (m_Loops)
                {
                    if (WorkingUnit == WorkUnit.Loop)
                    {
                        m_Loops.Add(callback);
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            /// 이 작업자에 등록된 작업 루프를 제거합니다.
            /// 작업 모드가 Loop가 아닌 경우, 항상 실패합니다.
            /// </summary>
            /// <param name="callback"></param>
            /// <returns></returns>
            public bool RemoveLoop(Action callback)
            {
                lock (m_Loops)
                {
                    if (WorkingUnit == WorkUnit.Loop)
                        return m_Loops.Remove(callback);
                }

                return false;
            }

            /// <summary>
            /// Loop 모드 작업자 쓰레드의 진입점입니다.
            /// </summary>
            private void OnThreadMainForLoop()
            {
                bool StateFlag = false;

                Initiate();

                while (!TestAndDeInitiate())
                    HandleTasks(true, ref StateFlag);
            }

            /// <summary>
            /// Task 모드 작업자 쓰레드의 진입점입니다.
            /// </summary>
            private void OnThreadMainForTask() 
            {
                bool StateFlag = false;

                Initiate();

                while (!TestAndDeInitiate())
                    HandleTasks(false, ref StateFlag);
            }

            /// <summary>
            /// 작업자 쓰레드 내에서 최초 초기화를 수행합니다.
            /// </summary>
            private void Initiate()
            {
                lock (this)
                {
                    m_ReallyRunning = true;
                    m_Comparison = Thread.CurrentThread;
                    Started?.Invoke(this, new EventArgs(this, State.Started));
                }

                m_Synchronizer.Set();
                Thread.Yield();
            }

            /// <summary>
            /// 작업자 쓰레드의 종료 조건을 검사하고,
            /// 조건이 충족된 경우, 종료 처리를 수행합니다.
            /// </summary>
            private bool TestAndDeInitiate()
            {
                lock (this)
                {
                    if (!m_KeepRunning)
                    {
                        m_ReallyRunning = false;
                        Stopped?.Invoke(this, new EventArgs(this, State.Stopped));

                        m_Synchronizer.Set();
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            /// 큐에 들어있는 작업을 처리합니다.
            /// </summary>
            /// <param name="handleLoops"></param>
            /// <param name="stateFlag"></param>
            private void HandleTasks(bool handleLoops, ref bool stateFlag)
            {
                Action Current = null;
                lock (m_Actions)
                {
                    Current = null;
                    if (m_Actions.Count > 0)
                        Current = m_Actions.Dequeue();
                }

                if (Current == null)
                {
                    if (handleLoops)
                    {
                        Action[] loopers = null;

                        lock (m_Loops)
                        {
                            if (m_Loops.Count <= 0)
                            {
                                Thread.Yield();
                                return;
                            }

                            loopers = m_Loops.ToArray();
                        }

                        foreach (Action eachLooper in loopers)
                        {
                            eachLooper();
                            Thread.Yield();
                        }

                        Thread.Sleep(1);
                    }
                    else
                    {
                        if (stateFlag)
                        {
                            stateFlag = false;
                            QueueEvent?.Invoke(this, new EventArgs(this, State.Waiting));
                        }

                        Thread.Sleep(0);
                    }
                }

                else
                {
                    if (!stateFlag)
                    {
                        stateFlag = true;
                        QueueEvent?.Invoke(this, new EventArgs(this, State.Running));
                    }

                    Current?.Invoke();
                }

                Thread.Yield();
            }
        }
    }
}
