using OpenTalk.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DTcpClient = System.Net.Sockets.TcpClient;
using DTcpListener = System.Net.Sockets.TcpListener;

namespace OpenTalk.Net
{
    public class TcpListener
        : IReadInterface
    {
        private bool m_Listening;
        private DTcpListener m_TcpListener;
        private Queue<DTcpClient> m_AcceptedClients;
        private AutoResetEvent m_AcceptState;
        private IAsyncResult m_AcceptIAR;

        /// <summary>
        /// TCP 리스너 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public TcpListener(IPAddress address, int port)
        {
            m_Listening = false;
            m_TcpListener = new DTcpListener(address, port);
            m_AcceptedClients = new Queue<DTcpClient>();
            m_AcceptState = new AutoResetEvent(false);
        }

        /// <summary>
        /// Tcp 리스너를 시작시킵니다.
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            lock (this)
            {
                if (!m_Listening)
                {
                    try { m_TcpListener.Start(); }
                    catch
                    {
                        return false;
                    }

                    m_Listening = true;
                    m_AcceptState.Reset();

                    AcceptAsync();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Tcp 리스너를 중단시킵니다.
        /// </summary>
        public void Stop() => CloseRead();

        /// <summary>
        /// 비동기 접속 수락 작업을 시작합니다.
        /// </summary>
        private void AcceptAsync()
        {
            lock (this)
            {
                if (m_AcceptIAR != null)
                    return;

                try { m_AcceptIAR = m_TcpListener.BeginAcceptTcpClient(OnAcceptAsync, null); }
                catch
                {
                    Stop();
                }
            }
        }

        /// <summary>
        /// 비동기 접속 수락작업이 완료되면 실행됩니다.
        /// </summary>
        /// <param name="X"></param>
        private void OnAcceptAsync(IAsyncResult X)
        {
            DTcpClient tcpClient = null;

            try { tcpClient = m_TcpListener.EndAcceptTcpClient(X); }
            catch
            {
                AcceptAsync();
                return;
            }

            lock (m_AcceptedClients)
            {
                m_AcceptedClients.Enqueue(tcpClient);
                m_AcceptState.Set();
            }

            lock (this)
                m_AcceptIAR = null;

            AcceptAsync();
            ReadReady?.Invoke(this, -1);
        }

        /// <summary>
        /// Tcp 리스너가 살아있는지 검사합니다.
        /// </summary>
        public bool IsReadAlive => this.Locked(() => m_Listening);

        /// <summary>
        /// 이 Tcp 리스너가 내부적으로 버퍼링을 수행하는지 확인합니다.
        /// </summary>
        public bool IsBufferedRead => true;

        /// <summary>
        /// 지금 즉시 Tcp 클라이언트를 수락할 수 있는지 검사합니다.
        /// </summary>
        public bool CanReadImmediately => m_AcceptedClients.Locked((X) => X.Count > 0);

        /// <summary>
        /// Read 이벤트를 발생시키는지 확인합니다.
        /// </summary>
        public bool RaisesReadEvents => IsReadAlive;

        /// <summary>
        /// 사용자 정의 Read 상태 객체입니다.
        /// </summary>
        public object UserReadState { get; set; }

        /// <summary>
        /// Tcp 리스너의 Read 버퍼는 지원되지 않습니다.
        /// </summary>
        public BinaryBuffer ReadBuffer => throw new NotSupportedException();

        /// <summary>
        /// Tcp 클라이언트를 수락할 수 있을 때 실행되는 이벤트입니다.
        /// </summary>
        public event Action<TcpListener, int> ReadReady;

        /// <summary>
        /// Tcp 리스너가 닫히면 발생되는 이벤트입니다.
        /// </summary>
        public event Action<TcpListener> ReadClosed;

        /// <summary>
        /// Tcp 클라이언트를 수락할 수 있을 때 실행되는 이벤트입니다.
        /// </summary>
        event Action<IReadInterface, int> IReadInterface.ReadReady {
            add { ReadReady += value; }
            remove { ReadReady -= value; }
        }

        /// <summary>
        /// Tcp 리스너가 닫히면 발생되는 이벤트입니다.
        /// </summary>
        event Action<IReadInterface> IReadInterface.ReadClosed {
            add { ReadClosed += value; }
            remove { ReadClosed -= value; }
        }

        /// <summary>
        /// 현재 수락 대기중인 Tcp 클라이언트를 수락하거나,
        /// 접속자가 있을 때 까지 대기합니다.
        /// 
        /// Initiator는 내부 비동기 작업들이 시작되기 전 수행되어야 할 동작들을 지정합니다.
        /// </summary>
        /// <returns></returns>
        public TcpClient Accept(Action<TcpClient> Initiator = null)
        {
            while (IsReadAlive)
            {
                lock (m_AcceptedClients)
                {
                    if (m_AcceptedClients.Count > 0)
                    {
                        TcpClient WrappedClient = new TcpClient(
                            m_AcceptedClients.Dequeue());

                        Initiator?.Invoke(WrappedClient);
                        WrappedClient.Initiate();

                        return WrappedClient;
                    }
                }

                m_AcceptState.WaitOne();
                Thread.Yield();

                lock (m_AcceptState)
                {
                    if (m_AcceptedClients.Count > 0)
                        m_AcceptState.Set();
                }
            }

            return null;
        }

        /// <summary>
        /// Tcp 리스너를 닫습니다.
        /// </summary>
        /// <returns></returns>
        public bool CloseRead()
        {
            lock (this)
            {
                if (m_Listening)
                {
                    m_Listening = false;
                    lock (m_AcceptedClients)
                    {
                        while (m_AcceptedClients.Count > 0)
                        {
                            DTcpClient client = m_AcceptedClients.Dequeue();

                            try { client.Client.Disconnect(false); } catch { }
                            try { client.Client.Close(); } catch { }
                        }

                        m_AcceptState.Set();
                    }

                    try
                    {
                        if (m_TcpListener != null)
                            m_TcpListener.Stop();
                    }
                    catch { }

                    m_TcpListener = null;
                    ReadClosed?.Invoke(this);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Tcp 리스너의 Read 메서드는 지원되지 않습니다.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public int Read(byte[] buffer, int offset, int length)
        {
            throw new NotSupportedException();
        }
    }
}
