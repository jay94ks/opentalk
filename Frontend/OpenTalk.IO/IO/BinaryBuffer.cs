using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenTalk.IO
{
    /// <summary>
    /// 통신 구현부 쪽에서 사용하는 유틸리티 클래스입니다.
    /// </summary>
    public class BinaryBuffer
    {
        private byte[] m_Sequence;
        private int m_Offset = 0, m_WriteOffset = 0;

        private int m_WaitSize;
        private Action<BinaryBuffer> m_Waiter;

        /// <summary>
        /// 이 버퍼를 비웁니다.
        /// </summary>
        public virtual void Clear()
        {
            m_Offset = m_WriteOffset = 0;
            m_WaitSize = 0;
            m_Waiter = null;
        }

        /// <summary>
        /// 이 버퍼에 저장된 바이트 시퀸스의 길이를 획득합니다.
        /// </summary>
        public virtual int Size => this.Locked(() => m_WriteOffset - m_Offset);

        /// <summary>
        /// 이 버퍼가 비어있는지 검사합니다.
        /// </summary>
        public virtual bool IsEmpty => this.Locked(() => m_WriteOffset - m_Offset <= 0);

        /// <summary>
        /// 지정된 크기 이상의 바이트 시퀸스가 채워질 때 실행될 콜백을 지정합니다.
        /// 해당 콜백은 호출된 이후, 제거됩니다.
        /// 
        /// 반환값은, 콜백이 즉시 실행된 경우 true이며, 예약 된 경우엔 false를 반환합니다.
        /// </summary>
        /// <param name="Size"></param>
        /// <param name="Callback"></param>
        /// <returns></returns>
        public virtual bool WaitBytes(int Size, Action<BinaryBuffer> Callback)
        {
            lock (this)
            {
                if (this.Size >= Size)
                {
                    m_Waiter = null;
                    m_WaitSize = 0;

                    Callback(this);
                    return true;
                }

                m_WaitSize = Size;
                m_Waiter = Callback;
            }

            return false;
        }

        /// <summary>
        /// 버퍼에 바이트 시퀸스를 채워넣습니다.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public virtual void Push(byte[] buffer, int offset, int length)
        {
            lock(this)
            {
                if (m_Sequence == null)
                {
                    m_Sequence = new byte[length];
                    m_Offset = m_WriteOffset = 0;
                }

                else
                {
                    if (m_Offset >= m_WriteOffset)
                        m_Offset = m_WriteOffset = 0;

                    Array.Resize(ref m_Sequence, m_WriteOffset + length);
                }

                Array.Copy(buffer, offset, m_Sequence,
                    m_WriteOffset, length);

                m_WriteOffset += length;

                if (Size > m_WaitSize)
                {
                    Action<BinaryBuffer> Waiter = m_Waiter;

                    m_Waiter = null;
                    m_WaitSize = 0;

                    Waiter?.Invoke(this);
                }
            }
        }

        /// <summary>
        /// 버퍼에 채워진 바이트 시퀸스 중에서 첫 n 바이트를 읽어옵니다.
        /// 단, 읽어온 시퀸스를 제거하진 않습니다.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public virtual int Peek(byte[] buffer, int offset, int length)
        {
            lock(this)
            {
                int available = m_WriteOffset - m_Offset;
                length = Math.Min(available, length);

                if (length > 0)
                {
                    Array.Copy(m_Sequence, m_Offset, 
                        buffer, offset, length);
                }
            }

            return length;
        }

        /// <summary>
        /// 버퍼에 채워진 바이트 시퀸스의 선두에서 n 바이트 만큼 제거합니다.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public virtual int Consume(int length)
        {
            lock(this)
            {
                m_Offset += length;

                if (m_Offset < 0)
                    m_Offset = 0;

                if (m_Offset >= m_WriteOffset)
                    m_Offset = m_WriteOffset = 0;
            }

            return m_Offset;
        }
    }
}
