namespace OpenTalk
{
    public abstract partial class Application
    {
        public sealed partial class Worker
        {
            /// <summary>
            /// 작업자 인스턴스를 지정하는 헬퍼입니다.
            /// </summary>
            public struct Specifier
            {
                private Worker m_Worker;
                private Application m_Application;

                /// <summary>
                /// 작업자 인스턴스를 지정합니다.
                /// </summary>
                /// <param name="worker"></param>
                public Specifier(Worker worker)
                {
                    m_Application = null;
                    m_Worker = worker;
                    Name = null;
                }

                /// <summary>
                /// 이름이 부여된 작업자 인스턴스를 지정합니다.
                /// </summary>
                /// <param name="name"></param>
                public Specifier(string name)
                {
                    m_Application = null;
                    m_Worker = null;
                    Name = name;
                }

                /// <summary>
                /// 이름이 부여된 작업자 인스턴스를 지정합니다.
                /// </summary>
                /// <param name="name"></param>
                public Specifier(Application application, string name)
                {
                    m_Application = application;
                    m_Worker = null;
                    Name = name;
                }

                public static implicit operator Worker(Specifier specifier) => specifier.Worker;
                public static implicit operator Specifier(string name) => new Specifier(name);
                public static implicit operator Specifier(Worker worker) => new Specifier(worker);

                /// <summary>
                /// 작업자의 이름입니다.
                /// </summary>
                public string Name { get; private set; }

                /// <summary>
                /// 작업자 인스턴스에 접근합니다.
                /// </summary>
                public Worker Worker {
                    get {
                        if (m_Worker != null)
                            return m_Worker;

                        if (m_Application != null && m_Application.Workers.Has(Name))
                            return m_Application.Workers[Name];

                        if (RunningInstance != null && RunningInstance.Workers.Has(Name))
                            return RunningInstance.Workers[Name];

                        return null;
                    }
                }
            }
        }
    }
}
