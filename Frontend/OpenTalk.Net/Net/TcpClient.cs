using OpenTalk.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DTcpClient = System.Net.Sockets.TcpClient;

namespace OpenTalk.Net
{
    /// <summary>
    /// TcpClient.
    /// </summary>
    public partial class TcpClient
        : IReliableSocket
    {
        private DTcpClient m_TcpClient;
        private Buffer m_ReadBuf;
        private BinaryBuffer m_WriteBuf;

        private bool m_SocketAlive;
        private bool m_ShutRead, m_ShutWrite;
        
        private AutoResetEvent m_ReadState;

        private IAsyncResult m_ReceiveIAR;
        private IAsyncResult m_SendIAR;

        private bool m_Connecting;
        private string m_CachedRemoteAddress;

        /// <summary>
        /// 원격지 접속정보를 확인합니다.
        /// </summary>
        public string RemoteAddress {
            get {
                lock (this)
                {
                    if (m_TcpClient != null &&
                        m_CachedRemoteAddress == null)
                    {
                        try
                        {
                            m_CachedRemoteAddress = m_TcpClient.Client.RemoteEndPoint.ToString();
                        }
                        catch { m_CachedRemoteAddress = null; }
                    }
                }

                return m_CachedRemoteAddress != null ? m_CachedRemoteAddress : "(unknown)";
            }
        }
        
        /// <summary>
        /// 새 Tcp 클라이언트 객체를 초기화합니다.
        /// </summary>
        public TcpClient() => Initialize(new DTcpClient());

        /// <summary>
        /// 닷넷 TcpClient 객체로 Tcp 클라이언트 객체를 초기화합니다.
        /// </summary>
        /// <param name="tcpClient"></param>
        internal TcpClient(DTcpClient tcpClient)
        {
            Initialize(tcpClient);
            m_SocketAlive = true;
        }

        /// <summary>
        /// 닷넷 TcpClient 객체로 초기화된 경우,
        /// 내부 비동기 통신을 시작시킵니다.
        /// </summary>
        internal void Initiate()
        {
            lock (this)
            {
                this.Locked(() => RemoteAddress);

                ReceiveBytes();
                WriteReady?.Invoke(this, -1);
            }
        }

        /// <summary>
        /// Tcp 클라이언트를 초기화합니다.
        /// </summary>
        /// <param name="tcpClient"></param>
        private void Initialize(DTcpClient tcpClient)
        {
            m_TcpClient = tcpClient;
            m_ReadState = new AutoResetEvent(false);

            m_SocketAlive = false;
            m_ShutRead = m_ShutWrite = false;

            m_ReadBuf = new Buffer();
            m_WriteBuf = new BinaryBuffer();

            m_Connecting = false;

            m_ReceiveIAR = null;
            m_SendIAR = null;
        }

        /// <summary>
        /// 목적 호스트에 접속합니다. (비동기)
        /// 반환값은 비동기 작업을 성공적으로 시작했는지 여부입니다.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool Connect(IPAddress host, int port, object state = null)
        {
            if (m_TcpClient != null)
            {
                if (!m_SocketAlive && !m_Connecting)
                {
                    m_Connecting = true;

                    try
                    {
                        lock (this)
                        {
                            IAsyncResult IAR = m_TcpClient.BeginConnect(
                                host, port, OnConnected, state);

                            m_ReceiveIAR = m_SendIAR = IAR;
                        }

                        return true;
                    }
                    catch
                    {
                    }
                }

                return false;
            }

            throw new ObjectDisposedException("m_TcpClient");
        }

        private class DestinationHint
        {
            public int Port { get; set; }
            public object State { get; set; }
            public IPAddress[] Addresses { get; set; }
            public int Index { get; set; }
        }

        /// <summary>
        /// 목적 호스트에 접속합니다. (비동기)
        /// 반환값은 비동기 작업을 성공적으로 시작했는지 여부입니다.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool Connect(string host, int port, object state = null)
        {
            if (m_TcpClient != null)
            {
                if (!m_SocketAlive && !m_Connecting)
                {
                    m_Connecting = true;

                    try
                    {
                        IAsyncResult IAR = Dns.BeginGetHostAddresses(host, OnConnectDnsStage,
                            new DestinationHint()
                            {
                                Port = port,
                                State = state
                            });

                        m_ReceiveIAR = m_SendIAR = IAR;
                        return true;
                    }
                    catch
                    {
                    }
                }

                return false;
            }

            throw new ObjectDisposedException("m_TcpClient");
        }

        /// <summary>
        /// 호스트 명으로 접속을 시도하는 경우, DNS Resolve가 끝나면 실행됩니다.
        /// </summary>
        /// <param name="X"></param>
        private void OnConnectDnsStage(IAsyncResult X)
        {
            DestinationHint hint = X.AsyncState as DestinationHint;

            try
            {
                hint.Addresses = Dns.EndGetHostAddresses(X);
                hint.Index = 0;

                // 모든 호스트 레코드를 대상으로 접속 시도 합니다.
                TryConnectByHint(hint);
            }
            catch
            {
                m_Connecting = false;

                // 서버를 찾을 수 없었습니다.
                Unreachable?.Invoke(this, hint.State);
                return;
            }
        }

        /// <summary>
        /// 주어진 레코드를 기반으로 접속을 시도합니다.
        /// </summary>
        /// <param name="hint"></param>
        private void TryConnectByHint(DestinationHint hint)
        {
            // 레코드가 더 없으면,
            if (hint.Index >= hint.Addresses.Length)
            {
                m_Connecting = false;

                // 서버를 찾을 수 없었습니다.
                Unreachable?.Invoke(this, hint.State);
                return;
            }

            lock (this)
            {
                IAsyncResult IAR = m_TcpClient.BeginConnect(
                    hint.Addresses[hint.Index], hint.Port,
                    OnConnectedByHint, hint);

                hint.Index++;
                m_ReceiveIAR = m_SendIAR = null;
            }
        }

        /// <summary>
        /// 힌트를 기반으로 한 접속시도가 완료될때 처리입니다.
        /// </summary>
        /// <param name="ar"></param>
        private void OnConnectedByHint(IAsyncResult X)
        {
            DestinationHint hint = X.AsyncState as DestinationHint;

            try
            {
                m_TcpClient.EndConnect(X);

                lock (this)
                {
                    m_ReceiveIAR = m_SendIAR = null;
                }
            }
            catch
            {
                // 다음 레코드로 재시도합니다.
                TryConnectByHint(hint);
                return;
            }

            m_SocketAlive = true;
            m_Connecting = false;

            // 이벤트를 발생시킵니다.
            NetWorker.Worker.Invoke(() =>
            {
                this.Locked(() => RemoteAddress);
                Ready?.Invoke(this, hint.State);
                WriteReady?.Invoke(this, -1);
                ReceiveBytes();
            });
        }

        /// <summary>
        /// 비동기 접속이 완료되면 실행됩니다.
        /// </summary>
        /// <param name="X"></param>
        private void OnConnected(IAsyncResult X)
        {
            try
            {
                m_TcpClient.EndConnect(X);

                lock (this)
                {
                    m_ReceiveIAR = m_SendIAR = null;
                }
            }
            catch
            {
                m_Connecting = false;

                // 접속이 거부되었습니다.
                Refused?.Invoke(this, X.AsyncState);
                return;
            }

            m_SocketAlive = true;
            m_Connecting = false;

            // 이벤트를 발생시킵니다.
            NetWorker.Worker.Invoke(() =>
            {
                this.Locked(() => RemoteAddress);
                Ready?.Invoke(this, X.AsyncState);
                WriteReady?.Invoke(this, -1);
                ReceiveBytes();
            });
        }

        /// <summary>
        /// 비동기 수신 IAR을 시작시킵니다.
        /// </summary>
        private void ReceiveBytes()
        {
            lock (this)
            {
                if (m_ReceiveIAR != null)
                    return;

                byte[] ReceivedBytes = new byte[m_TcpClient.ReceiveBufferSize];

                try
                {
                    m_ReceiveIAR = m_TcpClient.Client.BeginReceive(
                        ReceivedBytes, 0, ReceivedBytes.Length,
                        SocketFlags.None, OnReceivedBytes, ReceivedBytes);
                }
                catch
                {
                    CloseRead();
                }
            }
        }

        /// <summary>
        /// 비동기 송신 IAR을 시작합니다.
        /// </summary>
        private void SendBytes()
        {
            lock (this)
            {
                if (m_SendIAR != null || m_WriteBuf.IsEmpty)
                    return;

                byte[] SentBytes = new byte[Math.Min(m_TcpClient.SendBufferSize, m_WriteBuf.Size)];

                m_WriteBuf.Peek(SentBytes, 0, SentBytes.Length);

                try
                {
                    m_SendIAR = m_TcpClient.Client.BeginSend(
                        SentBytes, 0, SentBytes.Length, SocketFlags.None,
                        OnSentBytes, SentBytes);
                }
                catch
                {
                    CloseWrite();
                }
            }
        }

        /// <summary>
        /// 비동기 수신 IAR이 완료되면 실행됩니다.
        /// </summary>
        /// <param name="X"></param>
        private void OnReceivedBytes(IAsyncResult X)
        {
            int ReceivedSize = -1;

            try {
                byte[] ReceivedBytes = (byte[]) X.AsyncState;
                ReceivedSize = m_TcpClient.Client.EndReceive(X);

                if (ReceivedSize > 0)
                {
                    m_ReadBuf.PushInternal(ReceivedBytes, 0, ReceivedSize);
                    m_ReadState.Set();
                }
                else
                {
                    CloseRead();
                    CloseWrite();
                    return;
                }

                lock (this)
                {
                    m_ReceiveIAR = null;
                }
            }

            catch (SocketException e)
            {
                switch(e.SocketErrorCode)
                {
                    case SocketError.Success:
                        break;

                    case SocketError.NotConnected:
                    case SocketError.Disconnecting:
                    case SocketError.NetworkReset:
                    case SocketError.HostDown:
                    case SocketError.NetworkDown:
                    case SocketError.ConnectionReset:
                    case SocketError.ConnectionAborted:
                    case SocketError.Shutdown:
                    case SocketError.OperationAborted:
                    case SocketError.NoRecovery:
                        CloseRead();
                        CloseWrite();
                        return;

                    case SocketError.TryAgain:
                    case SocketError.WouldBlock:
                        ReceiveBytes();
                        return;
                }
            }

            if (ReceivedSize > 0)
                ReadReady?.Invoke(this, m_ReadBuf.Size);

            ReceiveBytes();
        }

        /// <summary>
        /// 비동기 송신 IAR이 완료되면 실행됩니다.
        /// </summary>
        /// <param name="ar"></param>
        private void OnSentBytes(IAsyncResult X)
        {
            int SentSize = -1;

            try
            {
                byte[] SentBytes = (byte[])X.AsyncState;
                SentSize = m_TcpClient.Client.EndSend(X);

                if (SentSize > 0)
                {
                    m_WriteBuf.Consume(SentSize);
                }

                lock (this)
                {
                    m_SendIAR = null;
                }
            }
            catch (SocketException e)
            {
                switch (e.SocketErrorCode)
                {
                    case SocketError.Success:
                        break;

                    case SocketError.NotConnected:
                    case SocketError.Disconnecting:
                    case SocketError.NetworkReset:
                    case SocketError.HostDown:
                    case SocketError.NetworkDown:
                    case SocketError.ConnectionReset:
                    case SocketError.ConnectionAborted:
                    case SocketError.Shutdown:
                    case SocketError.OperationAborted:
                    case SocketError.NoRecovery:
                        CloseWrite();
                        return;

                    case SocketError.TryAgain:
                    case SocketError.WouldBlock:
                        SendBytes();
                        return;
                }
            }

            if (m_WriteBuf.IsEmpty)
                WriteReady?.Invoke(this, -1);

            else SendBytes();
        }

        /// <summary>
        /// 이 TcpClient의 Read 채널이 살아있는지 검사합니다.
        /// </summary>
        public bool IsReadAlive => this.Locked(() => m_SocketAlive && !m_ShutRead);

        /// <summary>
        /// 이 TcpClient의 Write 채널이 살아있는지 검사합니다.
        /// </summary>
        public bool IsWriteAlive => this.Locked(() => m_SocketAlive && !m_ShutWrite);

        /// <summary>
        /// 이 TcpClient가 내부적으로 Read 버퍼링을 수행하는지 여부를 검사합니다.
        /// </summary>
        public bool IsBufferedRead => IsReadAlive;

        /// <summary>
        /// 이 TcpClient가 내부적으로 Write 버퍼링을 수행하는지 여부를 검사합니다.
        /// </summary>
        public bool IsBufferedWrite => IsWriteAlive;

        /// <summary>
        /// 이 TcpClient로 데이터를 즉시 Read 할 수 있는지 확인합니다.
        /// </summary>
        public bool CanReadImmediately => !m_ReadBuf.IsEmpty;

        /// <summary>
        /// 이 TcpClient로 데이터를 즉시 Write 할 수 있는지 확인합니다.
        /// </summary>
        public bool CanWriteImmediately => IsWriteAlive;

        /// <summary>
        /// 이 TcpClient가 Read 이벤트들을 발생시키는지 검사합니다.
        /// </summary>
        public bool RaisesReadEvents => IsReadAlive;

        /// <summary>
        /// 이 TcpClient가 Write 이벤트들을 발생시키는지 검사합니다.
        /// </summary>
        public bool RaisesWriteEvents => IsWriteAlive;

        /// <summary>
        /// 사용자 정의 Read 상태 객체입니다.
        /// </summary>
        public object UserReadState { get; set; }

        /// <summary>
        /// 사용자 정의 Write 상태 객체입니다.
        /// </summary>
        public object UserWriteState { get; set; }

        /// <summary>
        /// 사용자 정의 상태 객체입니다.
        /// </summary>
        public object UserState { get; set; }

        /// <summary>
        /// 접속할 호스트를 찾을 수 없으면 발생하는 이벤트입니다.
        /// </summary>
        public event Action<TcpClient, object> Unreachable;

        /// <summary>
        /// 접속을 거부당하면 발생하는 이벤트입니다.
        /// </summary>
        public event Action<TcpClient, object> Refused;

        /// <summary>
        /// Tcp 연결이 준비되면 실행되는 이벤트입니다.
        /// </summary>
        public event Action<TcpClient, object> Ready;

        /// <summary>
        /// 블로킹 없이 Read 가능한 상태가 되면 발생하는 이벤트입니다.
        /// </summary>
        public event Action<IReadInterface, int> ReadReady;

        /// <summary>
        /// 블로킹 없이 Write 가능한 상태가 되면 발생하는 이벤트입니다.
        /// </summary>
        public event Action<IWriteInterface, int> WriteReady;

        /// <summary>
        /// Read 채널이 닫히면 실행되는 이벤트입니다.
        /// </summary>
        public event Action<IReadInterface> ReadClosed;

        /// <summary>
        /// Write 채널이 닫히면 실행되는 이벤트입니다.
        /// </summary>
        public event Action<IWriteInterface> WriteClosed;

        /// <summary>
        /// Tcp 커넥션이 끊어지면 실행되는 이벤트입니다.
        /// 이 이벤트가 발생하기 전에는 연결이 끊어진 것이 아닙니다.
        /// </summary>
        public event Action<TcpClient> Closed;

        /// <summary>
        /// Read 채널의 바이너리 버퍼를 획득합니다.
        /// </summary>
        public BinaryBuffer ReadBuffer => m_ReadBuf;

        /// <summary>
        /// 접속을 거부당하면 발생하는 이벤트입니다.
        /// </summary>
        event Action<IReliableSocket, object> IReliableSocket.Refused {
            add { Refused += value; }
            remove { Refused -= value; }
        }

        /// <summary>
        /// 접속할 호스트를 찾을 수 없으면 발생하는 이벤트입니다.
        /// </summary>
        event Action<IReliableSocket, object> IReliableSocket.Unreachable {
            add { Unreachable += value; }
            remove { Unreachable -= value; }
        }

        /// <summary>
        /// Tcp 연결이 준비되면 실행되는 이벤트입니다.
        /// </summary>
        event Action<IReliableSocket, object> IReliableSocket.Ready {
            add { Ready += value; }
            remove { Ready -= value; }
        }

        /// <summary>
        /// Tcp 커넥션이 끊어지면 실행되는 이벤트입니다.
        /// 이 이벤트가 발생하기 전에는 연결이 끊어진 것이 아닙니다.
        /// </summary>
        event Action<IReliableSocket> IReliableSocket.Closed {
            add { Closed += value; }
            remove { Closed -= value; }
        }

        /// <summary>
        /// Read 버퍼에서 데이터를 읽습니다.
        /// 즉시 수신이 가능한 상태가 아닌경우, 블로킹이 일어납니다.
        /// 
        /// 연결이 끊어졌을 때, 이 메서드는 0을 리턴하며, 
        /// 이 메서드의 반환값을 기준으로 연결 끊김 처리를 해선 안됩니다.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public int Read(byte[] buffer, int offset, int length)
        {
            while (IsReadAlive)
            {
                int bufferedSize = m_ReadBuf.Peek(buffer, offset, length);
                if (bufferedSize > 0)
                {
                    m_ReadBuf.Consume(bufferedSize);
                    return bufferedSize;
                }

                m_ReadState.WaitOne();
                Thread.Yield();

                if (!m_ReadBuf.IsEmpty)
                    m_ReadState.Set();
            }

            return 0;
        }

        /// <summary>
        /// Write 버퍼에 데이터를 씁니다.
        /// 
        /// 연결이 끊어졌을 때, 이 메서드는 0을 리턴하며, 
        /// 이 메서드의 반환값을 기준으로 연결 끊김 처리를 해선 안됩니다.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public int Write(byte[] buffer, int offset, int length)
        {
            if (IsWriteAlive)
            {
                m_WriteBuf.Push(buffer, offset, length);

                if (length > 0)
                    SendBytes();

                return length;
            }

            return 0;
        }

        /// <summary>
        /// Read 채널을 닫습니다.
        /// </summary>
        /// <returns></returns>
        public bool CloseRead()
        {
            lock(this)
            {
                if (m_SocketAlive && !m_ShutRead)
                {
                    m_ShutRead = true;

                    m_ReadBuf.Clear();
                    m_ReadState.Set();

                    try { m_TcpClient.Client.Shutdown(SocketShutdown.Receive); }
                    catch { }

                    ReadClosed?.Invoke(this);
                    HandleFullyClose();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Write 채널을 닫습니다.
        /// </summary>
        /// <returns></returns>
        public bool CloseWrite()
        {
            lock (this)
            {
                if (m_SocketAlive && !m_ShutWrite)
                {
                    m_ShutWrite = true;
                    m_WriteBuf.Clear();

                    try { m_TcpClient.Client.Shutdown(SocketShutdown.Send); }
                    catch { }

                    WriteClosed?.Invoke(this);
                    HandleFullyClose();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Read/Write 채널을 모두 닫고, 소켓을 정리합니다.
        /// </summary>
        public bool Close()
        {
            bool bRetVal = CloseRead();
            return CloseWrite() || bRetVal;
        }

        /// <summary>
        /// Read/Write 채널이 모두 닫혔으므로 소켓 전체를 닫습니다.
        /// </summary>
        private void HandleFullyClose()
        {
            if (m_ShutRead && m_ShutWrite)
            {
                if (m_SocketAlive)
                {
                    try { m_TcpClient.Client.Close(); }
                    catch { }

                    try { m_TcpClient.Close(); }
                    catch { }

                    m_TcpClient = null;
                }

                m_SocketAlive = false;
                Closed?.Invoke(this);
            }
        }
    }
}
