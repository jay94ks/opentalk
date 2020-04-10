using System;
using System.Collections.Generic;

namespace OpenTalk
{
    /// <summary>
    /// 어플리케이션 컴포넌트입니다.
    /// 본 클래스는 어플리케이션 인스턴스와 함께 동작하는 객체를 구현합니다.
    /// </summary>
    public abstract partial class Application
    {
        /// <summary>
        /// 지정된 타입의 컴포넌트가 이 어플리케이션에 존재하는지 검사합니다.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Component GetComponent(Type type)
        {
            lock (m_Components)
            {
                return m_Components.Find((X) => X.GetType() == type || X.GetType().IsSubclassOf(type));
            }
        }

        /// <summary>
        /// 지정된 타입의 컴포넌트가 이 어플리케이션에 존재하는지 검사합니다.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Component GetComponent(Type type, Func<Component, bool> predicate)
        {
            lock (m_Components)
            {
                return m_Components.Find((X) => (X.GetType() == type || X.GetType().IsSubclassOf(type)) && predicate(X));
            }
        }

        /// <summary>
        /// 지정된 타입의 컴포넌트가 이 어플리케이션에 존재하는지 검사합니다.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Component GetComponent(Func<Component, bool> predicate)
        {
            lock (m_Components)
            {
                return m_Components.Find((X) => predicate(X));
            }
        }

        /// <summary>
        /// 지정된 타입의 모든 컴포넌트를 획득합니다.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Component[] GetComponents(Type type)
        {
            List<Component> collection = new List<Component>();

            lock (m_Components)
            {
                foreach (Component component in m_Components)
                {
                    if (component.GetType() == type ||
                        component.GetType().IsSubclassOf(type))
                    {
                        collection.Add(component);
                    }
                }
            }

            return collection.ToArray();
        }

        /// <summary>
        /// 지정된 타입의 모든 컴포넌트를 획득합니다.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Component[] GetComponents(Type type, Func<Component, bool> predicate)
        {
            List<Component> collection = new List<Component>();

            lock (m_Components)
            {
                foreach (Component component in m_Components)
                {
                    if ((component.GetType() == type ||
                        component.GetType().IsSubclassOf(type)) &&
                        predicate(component))
                    {
                        collection.Add(component);
                    }
                }
            }

            return collection.ToArray();
        }

        /// <summary>
        /// 지정된 타입의 모든 컴포넌트를 획득합니다.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Component[] GetComponents(Func<Component, bool> predicate)
        {
            List<Component> collection = new List<Component>();

            lock (m_Components)
            {
                foreach (Component component in m_Components)
                {
                    if (predicate(component))
                        collection.Add(component);
                }
            }

            return collection.ToArray();
        }

        /// <summary>
        /// 이 어플리케이션 인스턴스에 컴포넌트를 추가합니다.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        private bool AddComponent(Component component)
        {
            lock (m_Components)
            {
                if (m_Components.Contains(component))
                    return false;

                m_Components.Add(component);
            }

            return true;
        }

        /// <summary>
        /// 이 어플리케이션 인스턴스에서 컴포넌트를 제거합니다.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        private bool RemoveComponent(Component component)
        {
            lock (m_Components)
            {
                return m_Components.Remove(component);
            }
        }

        /// <summary>
        /// 어플리케이션 컴포넌트를 구현합니다.
        /// (어플리케이션 인스턴스에 기생하는 싱글톤 클래스를 구현할 때 사용합니다)
        /// </summary>
        public abstract class Component : BaseObject
        {
            private bool m_Activated = false;

            /// <summary>
            /// 현재 실행중인 어플리케이션 인스턴스를 타겟팅하는
            /// 컴포넌트를 초기화합니다.
            /// 
            /// 단, 현재 실행중인 인스턴스가 없을 때
            /// ApplicationException 예외가 발생합니다.
            /// </summary>
            public Component()
            {
            }

            /// <summary>
            /// 어플리케이션 컴포넌트를 초기화합니다.
            /// </summary>
            public Component(Application application)
                : base(application)
            {
            }

            /// <summary>
            /// 컴포넌트의 활성 여부를 검사하거나 설정합니다.
            /// (내부적으로 Activate, Deactivate 메서드를 알맞게 호출합니다)
            /// </summary>
            public bool IsComponentActive {
                get {
                    lock (this)
                        return m_Activated;
                }

                set {
                    lock (this)
                    {
                        if (value != m_Activated)
                        {
                            if (value)
                                Activate();

                            else Deactivate();
                        }
                    }
                }
            }

            /// <summary>
            /// 이 컴포넌트를 활성화시킵니다.
            /// </summary>
            public bool Activate()
            {
                // 종료중이라면 활성화를 즉시 실패시킵니다.
                if (Application.m_Context.IsExiting)
                    return false;

                lock (Application)
                {
                    lock(this)
                    {
                        if (m_Activated)
                            return false;

                        m_Activated = true;
                    }

                    if (Application.AddComponent(this))
                    {
                        Application.Invoke(() =>
                        {
                            Application.OnComponentActivated(this);
                            OnActivated();
                        })
                        .Wait();
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            /// 이 컴포넌트를 비활성화시킵니다.
            /// </summary>
            /// <returns></returns>
            public bool Deactivate()
            {
                lock(Application)
                {
                    lock (this)
                    {
                        if (!m_Activated)
                            return false;

                        m_Activated = false;
                    }

                    if (Application.RemoveComponent(this))
                    {
                        Application.Invoke(() =>
                        {
                            OnDeactivated();
                            Application.OnComponentDeactivated(this);
                        })

                        .Wait();
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            /// 주어진 어플리케이션 인스턴스 내의 모든 컴포넌트를 비활성화시킵니다.
            /// </summary>
            /// <param name="application"></param>
            internal static void DeactivateAll(Application application)
            {
                Component[] allComponents = null;

                while (true)
                {
                    lock (application.m_Components)
                    {
                        allComponents = application.m_Components.ToArray();
                        application.m_Components.Clear();

                        if (allComponents.Length <= 0)
                            break;
                    }

                    foreach (Component component in allComponents)
                    {
                        lock(component)
                        {
                            component.m_Activated = false;
                        }

                        application.Invoke(() =>
                        {
                            component.OnDeactivated();
                            application.OnComponentDeactivated(component);
                        })

                        .Wait();
                    }
                }
            }

            /// <summary>
            /// 컴포넌트가 활성화되면 메인 쓰레드에서 실행됩니다.
            /// </summary>
            protected virtual void OnActivated()
            {
            }

            /// <summary>
            /// 컴포넌트가 비활성화되면 메인 쓰레드에서 실행됩니다.
            /// </summary>
            protected virtual void OnDeactivated()
            {
            }
        }
    }
}
