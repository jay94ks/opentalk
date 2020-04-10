using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenTalk.IO;
using OpenTalk.Net;
using OpenTalk.Net.Http;

namespace OpenTalk.Server
{
    public partial class Connection
    {
        private TcpClient m_TcpClient;

        private bool m_WriteReady;
        private Queue<string> m_Sends;
        private bool m_HasAuthorized;

        /// <summary>
        /// 메시지 수신 버퍼입니다.
        /// </summary>
        private MemoryStream m_MessageBuffer = null;
        private int m_NextLength = 0;

        private Encoding m_Encoding;
        private IByteTransform m_WriteTransform = null;
        private IByteTransform m_ReadTransform = null;

        private IByteTransform m_PreferedWriteTransform;

        /// <summary>
        /// 접속자를 캡슐화합니다.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="connection"></param>
        public Connection(Program server, TcpClient connection)
        {
            Server = server;
            m_TcpClient = connection;
            m_Sends = new Queue<string>();

            m_Encoding = Encoding.ASCII;
            Authorization = null;
            m_HasAuthorized = false;

            m_TcpClient.ReadReady += OnReadReady;
            m_TcpClient.WriteReady += OnWriteReady;
            m_TcpClient.Closed += (X) =>
            {
                lock (this)
                {
                    Disconnected?.Invoke(this);
                    IsAlive = false; 
                }

                OnDeInitializeComponents();
                OnUnauthorize();
            };

            IsAlive = true;
        }

        /// <summary>
        /// 이 연결의 생존 여부입니다.
        /// </summary>
        public bool IsAlive { get; private set; }

        /// <summary>
        /// 원격지 접속 IP와 Port입니다.
        /// </summary>
        public string RemoteAddress => m_TcpClient.RemoteAddress;

        /// <summary>
        /// 서버 인스턴스에 접근합니다.
        /// </summary>
        public Program Server { get; private set; }

        /// <summary>
        /// 인증 토큰입니다.
        /// </summary>
        public string Authorization { get; private set; }

        /// <summary>
        /// 연결이 끊어질 때 발생하는 이벤트입니다.
        /// </summary>
        public event Action<Connection> Disconnected;

        /// <summary>
        /// 메시지를 송신합니다.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool Send(string label, string message)
        {
            return SendRaw("[U] " + label + ":" + message, 0);
        }

        /// <summary>
        /// 지정된 메시지를 전송합니다.
        /// (TcpSocket 전송계층에서 timeout은 사용되지 않습니다)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private bool SendRaw(string message, int timeout)
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

        private void OnReadReady(IO.IReadInterface socket, int arg2)
        {
            byte[] LengthBytes = new byte[4];
            byte[] MessageBuffer = new byte[256];

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
                        if (Message.StartsWith("[S]"))
                            OnHandleControlMessage(Message.Substring(3).TrimStart());

                        else if (Message.StartsWith("[U]"))
                            EnqueueMessageTask(Message.Substring(3));
                    }

                    else break;
                }
            }
        }

        private Task EnqueueMessageTask(string Message)
            => Server.InvokeByWorker(() => OnHandleMessage(Message));

        private void OnWriteReady(IO.IWriteInterface socket, int arg2)
        {
            int Counts = 0;

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

                    if (m_WriteTransform != m_PreferedWriteTransform)
                        m_WriteTransform = m_PreferedWriteTransform;

                    Counts++;
                }

                m_WriteReady = false;
            }
        }

        private void OnHandleControlMessage(string Message)
        {
            string[] Parsing = Message.TrimStart().Split(new char[] { ':' }, 2);

            // 메시지 타입과, 메시지 본문을 분리합니다.
            string MessageType = Parsing[0].Trim().ToUpper();
            Message = Parsing.Length > 1 ? Parsing[1].Trim() : "";

            switch (MessageType)
            {
                case "AUTHORIZATION": // --> 인증 토큰 수신.
                    lock (this)
                        Authorization = Message;

                    Log.w("[Connection, {0}] Handshake: 'Authorization: {1}'.",
                        m_TcpClient.RemoteAddress, Message);
                    break;

                case "ENCODING": // --> 엔코딩은 클라이언트가 요청한 대로 서비스합니다.
                    try
                    {
                        lock (this)
                            m_Encoding = Encoding.GetEncoding(Parsing[1].Trim());

                        Log.w("[Connection, {0}] Handshake: 'Encoding: {1}'.",
                            m_TcpClient.RemoteAddress, Parsing[1]);
                    }
                    catch { }
                    break;

                case "ENCRYPTION": // --> 클라측 송신 변조키를 수신했을 때.
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

                        Log.w("[Connection, {0}] Handshake: 'Encryption: {1}'.",
                            m_TcpClient.RemoteAddress, Message);
                    }
                    break;

                case "INITIATE":
                    Log.w("[Connection, {0}] Initiating handshake...",
                        m_TcpClient.RemoteAddress);

                    Initiate();
                    break;

                // 그 외엔 모두 무시합니다.
                default:
                    break;
            }
        }

        private void Initiate()
        {
            Version OTNetVersion = typeof(Connection).Assembly.GetName().Version;
            string Transkey = DateTime.Now.Ticks.ToString();

            SendRaw("[S] Encryption: Simple, " + Transkey, 0);
            m_PreferedWriteTransform = new SimpleByteTransform(Encoding.UTF8.GetBytes(Transkey));

            SendRaw("[S] Type: OpenTalk.Server", 0);
            SendRaw("[S] Version: " + string.Format("{0}.{1}.{2}",
                OTNetVersion.Major, OTNetVersion.Minor,
                OTNetVersion.Revision), 0);

            // 인증 키의 유효성을 검증하고, 핸드쉐이크를 재개합니다.
            // 유효하지 않는 경우, 아무 메시지 없이 연결을 끊습니다.

            if (string.IsNullOrEmpty(Authorization) ||
                string.IsNullOrWhiteSpace(Authorization))
            {
                Log.w("[Connection, {0}] is malformed connection, so kicked.",
                    m_TcpClient.RemoteAddress);

                m_TcpClient.Close();
                return;
            }

            Log.w("[Connection, {0}] Authorizing the accessing token, '{1}'...",
                m_TcpClient.RemoteAddress, Authorization);

            Server.InvokeByWorker(OnAuthorize);
        }

        private class AuthorizationResponse
        {
            [JsonProperty("restore-key")]
            public string RestoreKey;
        }

        /// <summary>
        /// 인증 토큰에 대해 검증을 요청합니다.
        /// </summary>
        private void OnAuthorize()
        {
            HttpComponent http = HttpComponent.GetHttpComponent(
                Server, Server.AuthorizationSettings.BaseUri);

            var Authorization = Server.AuthorizationSettings.Authorization;

            lock (this)
            {
                if (!IsAlive)
                    return;
            }

            http.Get<AuthorizationResponse>(
                HttpHelper.CombinePath(Authorization.Path,
                Authorization.QueryStrings, "authkey=" + Authorization))
                .ContinueWith(OnHandleAuthorizeResponse);
        }

        /// <summary>
        /// 인증 토컨에 대한 검증 응답을 처리합니다.
        /// </summary>
        /// <param name="X"></param>
        private void OnHandleAuthorizeResponse(Task<HttpResult<AuthorizationResponse>> X)
        {
            HttpResult<AuthorizationResponse> Result = X.Result;
            bool Retry = false;

            // 서버 내부 네트워크 오류가 발생했다면 100ms 간격으로 인증을 재시도합니다.
            if (Result.HasNetworkError)
                Retry = true;

            else if (!Result.Success || Result.HasParsingError)
            {
                // 요청에 성공하지 못했거나 파싱 에러가 발생했는데,
                // 요청에 성공 했었다면 재시도 해보고,
                // 아니라면, 연결을 닫습니다.

                if (Result.Success)
                    Retry = true;

                else
                {
                    Log.w("[Connection, {0}] '{1}' is invalid access token, so, kicked.",
                        m_TcpClient.RemoteAddress, Authorization);

                    Kick();
                }
            }

            else
            {
                lock (this)
                    m_HasAuthorized = true;

                if (Result.ResponseObject.RestoreKey == null)
                    Result.ResponseObject.RestoreKey = Authorization;

                Log.w("[Connection, {0}] Authorized for '{1}' successfully.",
                    m_TcpClient.RemoteAddress, Authorization);

                // 재접속 시도용 인증 키를 송신하고,
                SendRaw("[S] Authorization: " + Result.ResponseObject.RestoreKey, 0);

                // 통신을 시작합니다.
                SendRaw("[S] Initiate", 0);

                // 마지막으로 Ping을 한번 찍습니다.
                SendRaw(null, 0);
            }

            if (Retry)
            {
                Log.w("[Connection, {0}] Retrying to authorize the accessing token, '{1}'...",
                    m_TcpClient.RemoteAddress, Authorization);

                Thread.Sleep(100);
                Server.InvokeByWorker(OnAuthorize);
            }
        }

        /// <summary>
        /// 접속을 끊습니다.
        /// </summary>
        public void Kick()
        {
            m_TcpClient.Close();
        }

        /// <summary>
        /// 접속이 끊긴 연결에 대해 인증을 해제합니다.
        /// </summary>
        private void OnUnauthorize()
        {
            if (m_HasAuthorized)
            {
                HttpComponent http = HttpComponent.GetHttpComponent(
                    Server, Server.AuthorizationSettings.BaseUri);

                var Authorization = Server.AuthorizationSettings.Authorization;

                lock (this)
                {
                    if (!IsAlive)
                        return;
                }

                // 응답을 굳이 해석할 필요가 없습니다.
                // (죽은 연결이기 때문)
                http.Delete<AuthorizationResponse>(
                    HttpHelper.CombinePath(Authorization.Path,
                    Authorization.QueryStrings, "authkey=" + Authorization));
            }
            else
            {
                Log.w("[Connection, {0}] Not-authorized connection disconnected.",
                    m_TcpClient.RemoteAddress);
            }
        }

        /// <summary>
        /// 수신한 메시지를 처리합니다.
        /// </summary>
        /// <param name="Message"></param>
        private void OnHandleMessage(string Message)
        {
            int collon = Message.IndexOf(':');

            string label = collon > 0 ? Message.Substring(0, collon).Trim() : null;
            Message = collon > 0 ? Message.Substring(collon + 1) : Message;

            Server.HandleMessage(this, label, Message);
        }
    }
}
