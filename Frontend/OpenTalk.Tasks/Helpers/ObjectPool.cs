using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Helpers
{
    /// <summary>
    /// 객체 할당/해제 메커니즘을 구현합니다.
    /// </summary>
    public class ObjectPool<ObjectType>
    {
        private Queue<ObjectType> m_Queue;

        private Func<ObjectType> m_Constructor;
        private Action<ObjectType> m_Finalizer;

        private int m_PoolMax;

        /// <summary>
        /// 객체 풀을 초기화합니다.
        /// </summary>
        /// <param name="Constructor"></param>
        /// <param name="PoolMax">객체가 큐에 보관될 수 있는 최대 갯수입니다</param>
        public ObjectPool(Func<ObjectType> Constructor, int PoolMax = 64)
        {
            m_Queue = new Queue<ObjectType>();
            m_Constructor = Constructor;
            m_PoolMax = PoolMax;

            if (typeof(ObjectType).IsSubclassOf(typeof(IDisposable)))
                m_Finalizer = (X) => ((IDisposable)X).Dispose();

            else m_Finalizer = (X) => { };
        }

        /// <summary>
        /// 객체 풀의 최대 크기를 획득하거나 설정합니다.
        /// </summary>
        public int PoolMax {
            get {
                lock (m_Queue)
                    return m_PoolMax;
            }

            set {
                lock (m_Queue)
                {
                    m_PoolMax = value;

                    while (m_Queue.Count > m_PoolMax)
                        m_Finalizer(m_Queue.Dequeue());
                }
            }
        }

        /// <summary>
        /// 객체를 할당합니다.
        /// </summary>
        /// <returns></returns>
        public ObjectType Alloc()
        {
            lock(m_Queue)
            {
                if (m_Queue.Count > 0)
                    return m_Queue.Dequeue();
            }

            return m_Constructor();
        }

        /// <summary>
        /// 객체를 반납합니다.
        /// </summary>
        /// <param name="Object"></param>
        public void Free(ObjectType Object)
        {
            lock (m_Queue)
            {
                if (m_Queue.Count < m_PoolMax)
                    m_Queue.Enqueue(Object);

                else m_Finalizer(Object);
            }
        }
        
    }
}
