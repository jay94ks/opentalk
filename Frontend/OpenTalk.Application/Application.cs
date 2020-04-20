using OpenTalk.Tasks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DApp = System.Windows.Forms.Application;
using DRandom = System.Random;

namespace OpenTalk
{
    public abstract partial class Application
    {
        private static DRandom m_Randomizer = new DRandom((int)DateTime.Now.Ticks);

        private Context m_Context;
        private List<Component> m_Components;

        /// <summary>
        /// 어플리케이션 인스턴스를 초기화합니다.
        /// </summary>
        public Application()
        {
            Events = new LifeCycle(this);
            m_Context = new Context(this);
            m_Components = new List<Component>();
        }

        /// <summary>
        /// g_RunningInstance 필드에 접근 할 때 보호 동작을 수행하는 락 객체입니다.
        /// </summary>
        private static FutureEventSource<Application> g_RunningInstanceFES
            = new FutureEventSource<Application>();

        /// <summary>
        /// 현재 실행중인 어플리케이션 인스턴스를 획득합니다.
        /// </summary>
        public static Application RunningInstance => g_RunningInstanceFES.Future.Result;

        /// <summary>
        /// 어플리케이션 인스턴스가 준비되면 완료되는 작업입니다.
        /// </summary>
        public static Future<Application> FutureInstance => g_RunningInstanceFES.Future;

        /// <summary>
        /// 어플리케이션 라이프사이클 이벤트들을 정의합니다.
        /// </summary>
        public LifeCycle Events { get; private set; }

        /// <summary>
        /// Application.Run 메서드를 호츌 할 때 전달된 실행 인자들입니다.
        /// </summary>
        public string[] Arguments { get; private set; }

        /// <summary>
        /// 어플리케이션 인스턴스를 실행시킵니다.
        /// 이미 실행중인 인스턴스가 있으면 ApplicationException 예외가 발생합니다.
        /// </summary>
        /// <param name="Instance"></param>
        public static void Run(Application Instance, params string[] Arguments)
        {
            //g_RunningInstanceFES
            
            lock (g_RunningInstanceFES)
            {
                // 이미 실행중인 인스턴스가 있는 경우,
                // ApplicationException 예외를 발생시킵니다.
                if (g_RunningInstanceFES.Future.IsCompleted)
                    throw new ApplicationException();

                Instance.Arguments = Arguments;
                g_RunningInstanceFES.Set(Instance);
            }

            Instance.m_Context.Run();
            g_RunningInstanceFES.Unset();
        }

        /// <summary>
        /// 어플리케이션을 종료합니다.
        /// </summary>
        public static void ExitApp()
        {
            if (Log.HasConsole)
                Environment.Exit(0);

            DApp.Exit();
        }

        /// <summary>
        /// 어플리케이션을 초기화하기 전, 필요한 작업들을 수행합니다.
        /// </summary>
        protected virtual void PreInitialize()
        {
        }

        /// <summary>
        /// 어플리케이션을 초기화합니다.
        /// 주의: 이 메서드는 주 쓰레드가 아닌 쓰레드에서 실행됩니다.
        /// </summary>
        protected abstract void Initialize();

        /// <summary>
        /// 어플리케이션이 종료되기 전 실행됩니다.
        /// </summary>
        protected virtual void DeInitialize()
        {
        }

        /// <summary>
        /// 이 어플리케이션 인스턴스를 타겟팅하는 
        /// 어떤 컴포넌트가 활성화되면 실행되는 메서드입니다.
        /// </summary>
        /// <param name="component"></param>
        protected virtual void OnComponentActivated(Component component)
        {
        }

        /// <summary>
        /// 이 어플리케이션 인스턴스를 타겟팅하는 
        /// 어떤 컴포넌트가 비활성화되면 실행되는 메서드입니다.
        /// </summary>
        /// <param name="component"></param>
        protected virtual void OnComponentDeactivated(Component component)
        {
        }
    }
}
