using Newtonsoft.Json;
using OpenTalk.Credentials;
using OpenTalk.Net.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Internals
{
    internal class Authenticator
    {
        private Session m_Session;
        private Credential m_Credential;
        private Action<Credential, string> m_Success;
        private Action<SessionError> m_Failure;
        private TaskCompletionSource<Session> m_TCS;
        private HttpComponent m_Http;
        private bool m_Running;

        /// <summary>
        /// 로그인 동작을 1회 진행하는 객체입니다.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="credential"></param>
        public Authenticator(Session session, 
            Credential credential, Action<Credential, string> success, 
            Action<SessionError> failure)
        {
            m_TCS = new TaskCompletionSource<Session>();
            m_Session = session;
            m_Credential = credential;
            m_Success = success;
            m_Failure = failure;
            m_Running = false;
        }

        private class AuthData
        {
            [JsonProperty("identifier")]
            public string Identifier { get; set; }

            [JsonProperty("type")]
            public string AuthType { get; set; } = "generic";

            [JsonProperty("first_key")]
            public string KeyData1 { get; set; }

            [JsonProperty("second_key")]
            public string KeyData2 { get; set; }
        }

        /// <summary>
        /// Task 객체입니다.
        /// </summary>
        public Task<Session> Task => m_TCS.Task;

        /// <summary>
        /// 작업을 시작합니다.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public Task<Session> Run()
        {
            lock(this)
            {
                if (m_Running)
                    return Task;

                if (m_Http == null)
                {
                    m_Http = m_Session.GetHttpComponent();
                }

                m_Running = true;
            }

            AuthData authData = new AuthData();

            if ((m_Credential is GenericCredential) ||
                (m_Credential is TokenizedCredential))
            {
                Credential.Setter Setter = (Key, Value) =>
                {
                    switch (Key)
                    {
                        case "identifier":
                            authData.Identifier = Value;
                            break;

                        case "password":
                            authData.KeyData1 = Value;
                            authData.AuthType = "password";
                            break;

                        case "authentication":
                            authData.KeyData1 = Value;
                            authData.AuthType = "indirect";
                            break;

                        case "restoration":
                            authData.KeyData2 = Value;
                            authData.AuthType = "indirect";
                            break;

                    }
                };

                m_Credential.Set(Setter);
            }

            else
            {
                InvokeFailbacks(SessionError.AuthInvalidCredential);
                return Task;
            }

            m_Http.PostJson<AuthData>("auth", authData)
                .ContinueWith(OnHttpCompleted);

            return Task;
        }

        /// <summary>
        /// 실패한 경우, 실패처리를 수행합니다.
        /// </summary>
        /// <param name="errorCode"></param>
        private void InvokeFailbacks(SessionError errorCode)
        {
            m_Failure?.Invoke(errorCode);
            m_TCS.SetResult(m_Session);
        }

        /// <summary>
        /// Http 작업이 완료되면 실행됩니다.
        /// </summary>
        /// <param name="X"></param>
        private void OnHttpCompleted(Task<HttpResult<AuthData>> X)
        {
            HttpResult<AuthData> Result = X.Result;

            if (Result.HasNetworkError)
                InvokeFailbacks(SessionError.AuthNetworkError);

            else if (!Result.Success)
            {
                // 실패. (StatusCode != 200)
                switch(Result.StatusCode)
                {
                    case 401: /* Unauthorized. */
                        InvokeFailbacks(SessionError.AuthDenied);
                        break;

                    case 400: /* Bad Request. */
                    case 404: /* Not Found. */
                        InvokeFailbacks(SessionError.AuthInvalidCredential);
                        break;

                    case 500: /* Internal Server Error. */
                    case 503: /* Service Unavailable. */
                        InvokeFailbacks(SessionError.AuthServerError);
                        break;

                    case 501: /* Not Implemented. */
                        InvokeFailbacks(SessionError.AuthResponseError);
                        break;

                    case 403: /* Forbidden. */
                    default:
                        InvokeFailbacks(SessionError.AuthExpiredCredential);
                        break;
                }
            }

            else if (Result.HasParsingError)
                InvokeFailbacks(SessionError.AuthResponseError);

            else
            {
                AuthData authData = Result.ResponseObject;
                Credential restoration = new TokenizedCredential()
                {
                    Identifier = authData.Identifier,
                    AuthenticationToken = authData.KeyData1,
                    RestorationToken = authData.KeyData2
                };

                m_Success?.Invoke(restoration, authData.KeyData1);
                m_TCS.SetResult(m_Session);
            }
        }
    }
}
