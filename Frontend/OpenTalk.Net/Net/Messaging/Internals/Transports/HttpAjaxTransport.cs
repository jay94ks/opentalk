using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Net.Messaging.Internals.Transports
{
    /// <summary>
    /// Http/Ajax를 이용, Textile 전송 계층을 구성합니다.
    /// </summary>
    internal class HttpAjaxTransport : ITransport
    {
        private HttpClient m_HttpClient = new HttpClient();
        private Task<string> m_Receiver;
        private Task<HttpResponseMessage> m_Sender;
        private Task<HttpResponseMessage> m_Ping;
        private bool m_Closed = false;

        /// <summary>
        /// Http/AJAX 전송 계층을 초기화합니다.
        /// </summary>
        /// <param name="serverUri"></param>
        public HttpAjaxTransport(Uri serverUri, string authorization)
        {
            m_HttpClient.BaseAddress = serverUri;
            m_HttpClient.DefaultRequestHeaders.Authorization
                = new AuthenticationHeaderValue("bearer", authorization);

            Authorization = authorization;
        }

        /// <summary>
        /// 이 전송계층이 살아있는지 확인합니다.
        /// </summary>
        public bool IsAlive => this.Locked(() => m_Closed);

        /// <summary>
        /// 핸드쉐이크가 완료되었는지 검사합니다.
        /// </summary>
        public bool IsHandshaked => true;

        /// <summary>
        /// 이 전송계층의 상태를 나타냅니다.
        /// </summary>
        public ETransportState State => IsAlive ? ETransportState.Connected : ETransportState.Closed;

        /// <summary>
        /// 재접속 토큰입니다.
        /// </summary>
        public string Authorization { get; private set; }

        /// <summary>
        /// 문자열을 수신합니다.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public string Receive(int timeout)
        {
            lock (this)
            {
                if (m_Closed)
                    return null;

                if (m_Receiver != null)
                    throw new InvalidOperationException();

                m_Receiver = m_HttpClient.GetStringAsync("recv");
            }

            m_Receiver.Wait();

            if (m_Receiver.IsCanceled || m_Receiver.IsFaulted)
            {
                lock (this)
                    m_Receiver = null;

                return null;
            }

            string outResult = m_Receiver.Result;

            lock(this)
            {
                if (m_Closed)
                    outResult = null;

                m_Receiver = null;
            }

            return outResult;
        }

        /// <summary>
        /// 문자열을 전송합니다.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool Send(string message, int timeout)
        {
            lock (this)
            {
                if (m_Closed)
                    return false;

                if (m_Sender != null)
                    throw new InvalidOperationException();

                m_Sender = m_HttpClient.PutAsync("send", new StringContent(
                    message, Encoding.UTF8, "text/plain"));
            }

            m_Sender.Wait();

            if (m_Sender.IsCanceled || m_Sender.IsFaulted)
            {
                lock (this)
                    m_Sender = null;

                return false;
            }

            HttpResponseMessage Response = m_Sender.Result;

            lock (this)
                m_Sender = null;

            return Response.IsSuccessStatusCode;
        }

        /// <summary>
        /// 생존 여부를 확인합니다.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool Ping(int timeout)
        {
            lock (this)
            {
                if (m_Closed)
                    return false;

                m_Ping = m_HttpClient.GetAsync("ping");
            }

            m_Ping.Wait();

            if (m_Ping.IsCanceled || m_Ping.IsFaulted)
            {
                lock (this)
                    m_Sender = null;

                return false;
            }

            HttpResponseMessage Response = m_Ping.Result;

            lock (this)
                m_Ping = null;

            return Response.IsSuccessStatusCode;
        }

        /// <summary>
        /// 이 연결을 파기합니다.
        /// </summary>
        public void Close()
        {
            lock(this)
            {
                if (m_Closed)
                    return;

                m_Closed = true;
            }

            m_HttpClient.DeleteAsync("close").Wait();
        }
    }
}
