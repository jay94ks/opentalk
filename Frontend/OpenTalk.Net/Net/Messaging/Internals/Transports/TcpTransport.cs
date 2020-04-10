using OpenTalk.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenTalk.Net.Messaging.Internals.Transports
{
    internal class TcpTransport : ITransport
    {
        private TcpClient m_TcpClient;
        private Uri m_ServerUri;
        private int m_PortNumber;

        private string m_Authorization;
        private bool m_Handshaked;

        private Queue<string> m_Receives;
        private Queue<string> m_Sends;

        private DateTime m_SocketTime;
        private AutoResetEvent m_ReceiveWait;
        private bool m_WriteReady;

        private Encoding m_Encoding;
        private Encoding m_PreferedEncoding;

        private int m_NextLength = 0;
        private MemoryStream m_MessageBuffer = null;

        private IByteTransform m_WriteTransform = null;
        private IByteTransform m_ReadTransform = null;

        private IByteTransform m_PreferedWriteTransform;

        /// <summary>
        /// Tcp 소켓을 이용, Textile 전송 계층을 구성합니다.
        /// 최초 문자열 엔코딩은 ASCII이며, 핸드쉐이크가 완료되었을 때,
        /// 클라이언트측에서 요청한 엔코딩으로 변경됩니다.
        /// </summary>
        public TcpTransport(Uri serverUri, 
            string authorization, Encoding preferedEncoding = null)
        {
            m_TcpClient = new TcpClient();
            m_ReceiveWait = new AutoResetEvent(false);

            m_Receives = new Queue<string>();
            m_Sends = new Queue<string>();

            m_Handshaked = false;
            m_Authorization = authorization;
            m_ServerUri = serverUri;
            m_PortNumber = 8000;

            if (!serverUri.IsDefaultPort)
                m_PortNumber = serverUri.Port;

            m_Encoding = Encoding.ASCII;
            m_PreferedEncoding = preferedEncoding != null ? 
                preferedEncoding :Encoding.UTF8;

            m_TcpClient.Ready += OnReady;
            m_TcpClient.Refused += OnRefused;
            m_TcpClient.Unreachable += OnUnreachable;

            m_TcpClient.ReadReady += OnReadReady;
            m_TcpClient.WriteReady += OnWriteReady;

            // 읽기 채널이 닫히면 쓰기 채널도 닫아버립니다.
            m_TcpClient.ReadClosed += (X) => m_TcpClient.CloseWrite();
            m_TcpClient.Closed += OnClosed;

            State = ETransportState.Connecting;
            if (!m_TcpClient.Connect(serverUri.Host, m_PortNumber))
                SetState(ETransportState.Unreachable);

            else m_SocketTime = DateTime.Now;
        }

        /// <summary>
        /// 이 전송계층이 살아있는지 확인합니다.
        /// </summary>
        public bool IsAlive => this.Locked(() =>
            State == ETransportState.Connected || 
            State == ETransportState.Connecting);

        /// <summary>
        /// 이 전송계층의 상태를 나타냅니다.
        /// </summary>
        public ETransportState State { get; private set; }

        /// <summary>
        /// 재접속 토큰입니다.
        /// </summary>
        public string Authorization => this.Locked(() => m_Authorization);

        /// <summary>
        /// 핸드쉐이크가 완료되었는지 검사합니다.
        /// </summary>
        public bool IsHandshaked => this.Locked(() => m_Handshaked);

        /// <summary>
        /// 접속을 끊습니다.
        /// </summary>
        public void Close()
        {
            if (m_TcpClient.IsReadAlive ||
                m_TcpClient.IsWriteAlive)
                m_TcpClient.Close();
        }

        /// <summary>
        /// 문자열을 수신합니다.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public string Receive(int timeout)
        {
            if (IsAlive)
            {
                string RetVal = null;
                DateTime Latest = DateTime.Now;

                while (IsAlive)
                {
                    lock (m_Receives)
                    {
                        if (m_Receives.Count > 0)
                        {
                            RetVal = m_Receives.Dequeue();
                            break;
                        }
                    }

                    // 0 보다 작은 값이 주어졌을 땐, 무한 대기를 수행하며,
                    if (timeout < 0)
                        m_ReceiveWait.WaitOne();

                    // 0 보다 큰 값이 주어졌을 땐, 지정된 시간동안만 대기합니다.
                    else if (timeout > 0)
                    {
                        m_ReceiveWait.WaitOne(timeout);

                        timeout -= (int)(DateTime.Now - Latest).TotalMilliseconds;
                        Latest = DateTime.Now;

                        // 타임아웃에 도달한 경우, 다음번 루프에서 
                        // 그 결과가 어찌되었든, 빠져나갑니다.
                        if (timeout < 0)
                            timeout = 0;
                    }

                    // 또, 0인 경우, 즉시 탈출합니다.
                    else break;
                }

                return RetVal;
            }

            return null;
        }

        /// <summary>
        /// 지정된 메시지를 전송합니다.
        /// (TcpSocket 전송계층에서 timeout은 사용되지 않습니다)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool Send(string message, int timeout)
        {
            if (IsAlive)
            {
                lock (m_Sends)
                {
                    m_Sends.Enqueue(message);

                    if (m_WriteReady)
                        OnWriteReady(m_TcpClient, -1);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Ping을 송신합니다.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool Ping(int timeout)
        {
            if (IsAlive)
                return Send(null, timeout);

            return false;
        }

        /// <summary>
        /// Tcp 통신이 사용가능해지면 실행됩니다.
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void OnReady(TcpClient arg1, object arg2)
        {
            Version OTNetVersion = typeof(TcpTransport)
                .Assembly.GetName().Version;

            SetState(ETransportState.Connected);
            RemarkSocketTime();

            /*
                각 메시지의 '[]' 사이의 문자는 아래 의미를 가집니다.
                
                S: 특성 데이터 변경 요청.
                U: 사용자 데이터 송/수신.
             */

            // 현재 엔코딩과 타깃 엔코딩이 다른 경우,
            if (m_PreferedEncoding.WebName != m_Encoding.WebName)
            {
                // 엔코딩 정보를 송신합니다.

                // 첫번째 송신 이후, 엔코딩이 변경되어,
                // 다음 메시지부턴 변경된 엔코딩으로 송신됩니다.
                Send("[S] Encoding: " + m_PreferedEncoding.WebName, 0);
            }

            // 클라이언트측 송신 변조 알고리즘은, Simple 이며, 현재 시간의 Tick값을 변조 키로 사용합니다.
            // 이 값 역시도, 지연 적용 대상으로, 엔코딩 송신이 완료되고, 관련 정보 송신이 끝난 후 적용됩니다.
            string Transkey = DateTime.Now.Ticks.ToString();

            Send("[S] Encryption: Simple, " + Transkey, 0);
            m_PreferedWriteTransform = new SimpleByteTransform(Encoding.UTF8.GetBytes(Transkey));

            // 클라이언트 타입과 버젼을 먼저 송신하고,
            Send("[S] Type: OpenTalk", 0);
            Send("[S] Version: " + string.Format("{0}.{1}.{2}",
                OTNetVersion.Major, OTNetVersion.Minor, OTNetVersion.Revision), 0);

            // 인증 키를 송신하고,
            Send("[S] Authorization: " + m_Authorization, 0);

            // 통신을 시작합니다.
            Send("[S] Initiate", 0);

            // 마지막으로 Ping을 한번 찍습니다.
            Send(null, 0);
        }

        /// <summary>
        /// 마지막 패킷 송신/수신 성공 시간을 기록합니다.
        /// </summary>
        private void RemarkSocketTime()
        {
            lock (this)
                m_SocketTime = DateTime.Now;
        }

        /// <summary>
        /// Tcp 연결이 거부되면 실행됩니다.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="arg2"></param>
        private void OnRefused(TcpClient X, object arg2)
        {
            SetState(ETransportState.Refused);
            OnClosed(X);
        }

        /// <summary>
        /// 대상 서버에 접속할 수 없었을 때 실행됩니다.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="arg2"></param>
        private void OnUnreachable(TcpClient X, object arg2)
        {
            SetState(ETransportState.Unreachable);
            OnClosed(X);
        }

        /// <summary>
        /// 연결이 끊어졌거나, 접속에 실패했을 때, 전송 계층의 상태를 설정합니다.
        /// </summary>
        /// <param name="state"></param>
        private void SetState(ETransportState state)
        {
            lock (this)
            {
                State = state;
            }
        }

        /// <summary>
        /// 데이터 수신 준비가 완료되면 실행되는 콜백입니다.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="size"></param>
        private void OnReadReady(IO.IReadInterface socket, int size)
        {
            byte[] LengthBytes = new byte[4];
            byte[] MessageBuffer = new byte[256];

            RemarkSocketTime();

            while (!socket.ReadBuffer.IsEmpty)
            {
                if (m_NextLength <= 0)
                {
                    // 4바이트 길이 바이트들이 수신 버퍼에 충분히 채워져 있다면,
                    if (socket.ReadBuffer.Peek(LengthBytes, 0, LengthBytes.Length) >= sizeof(int))
                    {
                        // 메시지 버퍼를 새로 만들고,
                        m_MessageBuffer = new MemoryStream();

                        // 수신할 메시지 길이를 읽어낸 뒤에,
                        m_NextLength = BitConverter.ToInt32(LengthBytes, 0);

                        // 버퍼에서 수신한 4바이트를 제거해줍니다.
                        socket.ReadBuffer.Consume(sizeof(int));
                        continue;
                    }

                    // 충분하지 않다면 다음번 ReadReady 이벤트를 기다립니다.
                    break;
                }

                else
                {
                    // 지금 당장 수신할 크기를 확정하고,
                    int LengthPiece = m_NextLength > MessageBuffer.Length ? 
                        MessageBuffer.Length : m_NextLength;

                    // 메시지 본문을 수신합니다.
                    LengthPiece = socket.ReadBuffer.Peek(MessageBuffer, 0, LengthPiece);
                    
                    // 수신 받은 본문이 확실히 있으면,
                    if (LengthPiece > 0)
                    {
                        // 버퍼에 채웁니다.
                        m_MessageBuffer.Write(MessageBuffer, 0, LengthPiece);
                        socket.ReadBuffer.Consume(LengthPiece);
                        m_NextLength -= LengthPiece;
                    }

                    // 현재 수신하던 메시지가 완전히 수신되었다면,
                    if (m_NextLength <= 0)
                    {
                        byte[] MessageBytes = m_MessageBuffer.ToArray();

                        // 메시지 본문을 복원합니다.
                        string Message = null;

                        // 변조 알고리즘이 지정되어 있다면 변조를 수행합니다.
                        if (m_ReadTransform != null)
                        {
                            MessageBytes = m_ReadTransform.Transform(
                                MessageBytes, 0, MessageBytes.Length);
                        }

                        Message = m_Encoding.GetString(MessageBytes);

                        // 그리고, 메시지 버퍼를 제거합니다.
                        m_MessageBuffer.Dispose();
                        m_MessageBuffer = null;

                        // 메시지가 '[' 로 시작해야 정상적인 메시지입니다.
                        if (Message.StartsWith("["))
                            HandleMessage(Message);
                    }

                    else break;
                }
            }
        }

        /// <summary>
        /// 수신된 메시지에 따라서 필요한 동작을 취합니다.
        /// </summary>
        /// <param name="Message"></param>
        private void HandleMessage(string Message)
        {
            string MessageType = Message.Substring(0, 3).ToUpper();
            Message = Message.Substring(3);

            if (MessageType == "[S]")
            {
                string[] Parsing = Message.TrimStart().Split(new char[] { ':' }, 2);

                // 메시지 타입과, 메시지 본문을 분리합니다.
                MessageType = Parsing[0].Trim().ToUpper();
                Message = Parsing.Length > 1 ? Parsing[1].Trim() : "";

                switch (MessageType)
                {
                    case "AUTHORIZATION": // --> 재접속시 사용해야 하는 인증 토큰 수신.
                        lock (this)
                            m_Authorization = Message;
                        break;

                    case "ENCRYPTION": // --> 서버측 송신 변조키를 수신했을 때.
                        lock (this)
                        {
                            Parsing = Message.Split(new char[] { ',' }, 2);

                            // 현재, 단순 변조만 지원합니다.
                            if (Parsing[0].ToUpper() == "Simple")
                            {
                                // 다음 패킷부터 적용합니다.
                                m_ReadTransform = new SimpleByteTransform(
                                    Encoding.UTF8.GetBytes(Parsing[1].Trim()));
                            }
                        }
                        break;

                    case "INITIATE":
                        lock (this)
                            m_Handshaked = true;

                        break;

                        // 그 외엔 모두 무시합니다.
                    default:
                        break;
                }
            }

            // 사용자 메시지인 경우.
            else if (MessageType == "[U]")
            {
                // 첫번째 문자가 공백인 경우, 그 공백 하나만 제거하고,
                // 온전히 수신된 것으로 처리합니다.
                if (Message.Length > 0 && Message[0] == ' ')
                    Message = Message.Substring(1);

                lock (m_Receives)
                {
                    m_Receives.Enqueue(Message);
                    m_ReceiveWait.Set();
                }
            }
        }

        /// <summary>
        /// 데이터 송신 준비가 완료되면 실행되는 콜백입니다.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="size"></param>
        private void OnWriteReady(IO.IWriteInterface socket, int unused)
        {
            int Counts = 0;
            RemarkSocketTime();

            lock (m_Sends)
            {
                if (m_Sends.Count <= 0)
                {
                    m_WriteReady = true;
                    return;
                }

                while (m_Sends.Count > 0 && Counts <= 5)
                {
                    string Message = m_Sends.Dequeue();

                    if (Message != null)
                    {
                        byte[] Data = m_Encoding.GetBytes(Message);
                        byte[] Length = null;

                        if (m_WriteTransform != null)
                            Data = m_WriteTransform.Transform(Data, 0, Data.Length);

                        Length = BitConverter.GetBytes(Data.Length);

                        // 길이 바이트는 항상 리틀 엔디안으로 송신합니다.
                        if (!BitConverter.IsLittleEndian)
                            Array.Reverse(Length);

                        // 길이 바이트부터 송신하고, 데이터 바이트를 송신합니다.
                        socket.Write(Length, 0, Length.Length);
                        socket.Write(Data, 0, Data.Length);
                    }
                    else
                    {
                        // null을 송신하는 경우는 Ping 패킷 뿐이기 때문에,
                        // 길이가 0인 문자열을 송신하는 동작으로 해석합니다.
                        socket.Write(new byte[] { 0, 0, 0, 0 }, 0, 4);
                    }

                    UpdateEncoding();
                    Counts++;
                }

                m_WriteReady = false;
            }
        }

        /// <summary>
        /// 엔코딩을 업데이트합니다.
        /// </summary>
        private void UpdateEncoding()
        {
            lock (this)
            {
                if (m_PreferedEncoding.WebName != m_Encoding.WebName)
                    m_Encoding = m_PreferedEncoding;

                else if (m_WriteTransform != m_PreferedWriteTransform)
                    m_WriteTransform = m_PreferedWriteTransform;
            }
        }

        /// <summary>
        /// 연결이 끊어지면, 모든 송수신 큐들을 정리합니다.
        /// </summary>
        /// <param name="obj"></param>
        private void OnClosed(TcpClient X)
        {
            if (State == ETransportState.Connected)
                SetState(ETransportState.Closed);

            lock (m_Sends)
            {
                m_Sends.Clear();
            }

            lock (m_Receives)
            {
                m_Receives.Clear();
                m_ReceiveWait.Set();
            }
        }

    }
}
