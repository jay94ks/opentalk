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
using OpenTalk.Net.Textile;

namespace OpenTalk.Server
{
    public partial class Connection
    {
        private TextileClient m_Textile;
        private bool m_Authorized;

        /// <summary>
        /// 접속자를 캡슐화합니다.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="connection"></param>
        public Connection(Program server, TcpClient connection)
        {
            Server = server;
            m_Textile = new TextileClient(connection);

            m_Textile.StateChanged += OnStateChanged;
            m_Textile.ReceiveReady += OnReceiveReady;

            RemoteAddress = connection.RemoteAddress;
            Authorization = null;
            m_Authorized = false;

            OnStateChanged(m_Textile, m_Textile.State);
            if (m_Textile.HasReceivePending)
                OnReceiveReady(m_Textile, 0);
        }

        /// <summary>
        /// 메시지를 수신할 준비가 되면 실행됩니다.
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void OnReceiveReady(TextileClient client, int arg2)
        {
            // 메시지 수신 작업을 시작합니다.
            if (client.HasReceivePending)
                Server.InvokeByWorker(() => m_Textile.Receive(OnHandleMessage));
        }

        /// <summary>
        /// 수신된 메시지를 처리합니다.
        /// </summary>
        /// <param name="x"></param>
        private void OnHandleMessage(TextileClient.Message X)
        {
            // 인증 처리가 완료되면 전역 핸들러 쪽으로 모두 전달합니다.
            if (m_Authorized)
            {
                Server.HandleMessage(this, X.Label, X.Data);
            }

            else if (X.Label != null)
            {
                switch(X.Label.ToLower())
                {
                    case "authorization":
                        // 인증 요청.
                        Authorization = X.Data;
                        OnAuthorize();
                        break;
                }
            }
        }

        /// <summary>
        /// Textile 클라이언트의 상태가 변동되면 실행됩니다.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="state"></param>
        private void OnStateChanged(TextileClient client, TextileState state)
        {
            switch(state)
            {
                case TextileState.Disconnected:
                case TextileState.Unreachable:
                case TextileState.Refused:
                    lock (this)
                    {
                        Disconnected?.Invoke(this);
                    }

                    OnDeInitializeComponents();
                    OnUnauthorize();
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 이 연결의 생존 여부입니다.
        /// </summary>
        public bool IsAlive => 
            m_Textile.State == TextileState.Ready ||
            m_Textile.State == TextileState.Handshaking;

        /// <summary>
        /// 원격지 접속 IP와 Port입니다.
        /// </summary>
        public string RemoteAddress { get; private set; }

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
            => m_Textile.Send(new TextileClient.Message() {
                Label = label, Data = message
            });

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
                Authorization.QueryStrings, "authkey=" + this.Authorization))
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
                        RemoteAddress, Authorization);

                    Close();
                }
            }

            else
            {
                // 인증을 성공적으로 받았습니다.
                lock (this)
                {

                    m_Authorized = true;
                }
            }

            if (Retry)
            {
                Log.w("[Connection, {0}] Retrying to authorize the accessing token, '{1}'...",
                    RemoteAddress, Authorization);

                Thread.Sleep(100);
                Server.InvokeByWorker(OnAuthorize);
            }
        }

        /// <summary>
        /// 접속을 끊습니다.
        /// </summary>
        public void Close() => m_Textile.Close();

        /// <summary>
        /// 접속이 끊긴 연결에 대해 인증을 해제합니다.
        /// </summary>
        private void OnUnauthorize()
        {
            if (this.Authorization != null && m_Authorized)
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
                    Authorization.QueryStrings, "authkey=" + this.Authorization));
            }
            else
            {
                Log.w("[Connection, {0}] Not-authorized connection disconnected.",
                    RemoteAddress);
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
