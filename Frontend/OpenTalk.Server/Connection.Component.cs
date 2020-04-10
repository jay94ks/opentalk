using System;
using System.Collections.Generic;

namespace OpenTalk.Server
{
    public partial class Connection
    {
        private List<Component> m_Components = new List<Component>();

        /// <summary>
        /// 커넥션 컴포넌트를 추상화합니다.
        /// </summary>
        public abstract class Component
        {
            /// <summary>
            /// 이 컴포넌트를 갖고 있는 커넥션입니다.
            /// </summary>
            public Connection Connection { get; internal set; }

            /// <summary>
            /// 이 컴포넌트를 초기화합니다.
            /// </summary>
            internal void Initialize() => OnInitialize();

            /// <summary>
            /// 이 컴포넌트를 종료시킵니다.
            /// </summary>
            internal void DeInitialize() => OnDeInitialize();

            /// <summary>
            /// 이 컴포넌트를 초기화합니다.
            /// </summary>
            protected virtual void OnInitialize()
            {

            }

            /// <summary>
            /// 이 컴포넌트를 종료시킵니다.
            /// </summary>
            protected virtual void OnDeInitialize()
            {

            }
        }

        /// <summary>
        /// 모든 컴포넌트들을 파기합니다.
        /// </summary>
        private void OnDeInitializeComponents()
        {
            lock (m_Components)
            {
                foreach(Component Component in m_Components)
                {
                    Component.DeInitialize();
                    Component.Connection = null;
                }

                m_Components.Clear();
            }
        }

        /// <summary>
        /// 지정된 타입의 컴포넌트를 획득합니다.
        /// 해당 타입의 컴포넌트가 없을 때 동작은 
        /// 이 연결이 끊어지지 않은 경우엔, 새로 생성, 부착한 후 반환하며,
        /// 연결이 끊어졌을 땐 null을 반환합니다.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Component GetComponent(Type type)
        {
            lock (m_Components)
            {
                Component Component = m_Components.Find((X) => X.GetType() == type);

                if (Component != null)
                    return Component;

                if (IsAlive)
                {
                    m_Components.Add(Component = (Component)type
                        .GetConstructor(Type.EmptyTypes).Invoke(new object[0]));

                    Component.Connection = this;
                    Component.Initialize();
                }

                return Component;
            }
        }

        /// <summary>
        /// 지정된 타입의 컴포넌트를 파기하고 새로 부착합니다.
        /// 연결이 끊어졌을 땐 항상 null을 반환합니다.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Component ResetComponent(Type type)
        {
            lock (m_Components)
            {
                DestroyComponent(type);
                return GetComponent(type);
            }
        }

        /// <summary>
        /// 지정된 타입의 컴포넌트를 파기합니다.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool DestroyComponent(Type type)
        {
            lock (m_Components)
            {
                Component Component = m_Components.Find((X) => X.GetType() == type);

                if (Component != null)
                {
                    Component.DeInitialize();
                    Component.Connection = null;

                    m_Components.Remove(Component);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 지정된 타입의 컴포넌트를 획득합니다.
        /// 해당 타입의 컴포넌트가 없을 때 동작은 
        /// 이 연결이 끊어지지 않은 경우엔, 새로 생성, 부착한 후 반환하며,
        /// 연결이 끊어졌을 땐 null을 반환합니다.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ComponentType GetComponent<ComponentType>()
            where ComponentType : Component
        {
            object Component = GetComponent(typeof(ComponentType));
            return Component != null ? Component as ComponentType : null;
        }

        /// <summary>
        /// 지정된 타입의 컴포넌트를 파기하고 새로 부착합니다.
        /// 해당 타입의 컴포넌트가 없을 때 동작은 
        /// 이 연결이 끊어지지 않은 경우엔, 새로 생성, 부착한 후 반환하며,
        /// 연결이 끊어졌을 땐 null을 반환합니다.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ComponentType ResetComponent<ComponentType>()
            where ComponentType : Component
        {
            object Component = ResetComponent(typeof(ComponentType));
            return Component != null ? Component as ComponentType : null;
        }
    }
}
