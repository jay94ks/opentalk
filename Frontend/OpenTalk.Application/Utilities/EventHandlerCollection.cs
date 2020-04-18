using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Utilities
{
    /// <summary>
    /// 이벤트 핸들러들을 취합하는 유틸리티입니다.
    /// 이벤트의 구현 유무에 따라 동작을 달리 해야 하는 경우,
    /// 사용하십시오.
    /// </summary>
    /// <typeparam name="TEventArgs"></typeparam>
    public class EventHandlerCollection<TEventArgs>
        where TEventArgs : EventArgs
    {
        private List<EventHandler<TEventArgs>> m_Handlers
            = new List<EventHandler<TEventArgs>>();

        /// <summary>
        /// 이벤트가 구현되었는지 검사합니다.
        /// </summary>
        public bool HasImplemented
            => m_Handlers.Locked((X) => X.Count > 0);

        /// <summary>
        /// 이벤트를 발송합니다.
        /// </summary>
        /// <param name="Args"></param>
        public bool Broadcast(object Sender, TEventArgs Args)
        {
            EventHandler<TEventArgs>[] Handlers
                = m_Handlers.Locked((X) => X.ToArray());

            foreach (var Handler in Handlers)
                Handler?.Invoke(Sender, Args);

            return Handlers.Length > 0;
        }

        /// <summary>
        /// 이벤트 핸들러를 추가합니다.
        /// </summary>
        /// <param name="Handler"></param>
        public void Add(EventHandler<TEventArgs> Handler)
        {
            lock(m_Handlers)
            {
                m_Handlers.Add(Handler);
            }
        }

        /// <summary>
        /// 이벤트 핸들러를 제거합니다.
        /// </summary>
        /// <param name="Handler"></param>
        public bool Remove(EventHandler<TEventArgs> Handler)
        {
            lock (m_Handlers)
            {
                return m_Handlers.Remove(Handler);
            }
        }
    }

    /// <summary>
    /// 이벤트 핸들러들을 취합하는 유틸리티입니다.
    /// 이벤트의 구현 유무에 따라 동작을 달리 해야 하는 경우,
    /// 사용하십시오.
    /// </summary>
    /// <typeparam name="TEventArgs"></typeparam>
    public class EventHandlerCollection
        : EventHandlerCollection<EventArgs>
    {
    }
}
