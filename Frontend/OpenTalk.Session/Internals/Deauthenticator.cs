using OpenTalk.Net.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Internals
{
    internal class Deauthenticator
    {
        private Session m_Session;
        private Action m_Success;
        private Action<SessionError> m_Failure;
        private TaskCompletionSource<Session> m_TCS;
        private HttpComponent m_Http;
        private bool m_Running;

        /// <summary>
        /// 로그인 동작을 1회 진행하는 객체입니다.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="credential"></param>
        public Deauthenticator(Session session,
            Action success, Action<SessionError> failure)
        {
            m_TCS = new TaskCompletionSource<Session>();
            m_Session = session;
            m_Success = success;
            m_Failure = failure;
            m_Running = false;
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
            lock (this)
            {
                if (m_Running)
                    return Task;

                if (m_Http == null)
                {
                    m_Http = m_Session.GetHttpComponent();
                }

                m_Running = true;
            }

            if (m_Http.Authorization != null)
            {
                m_Http.Delete("auth")
                    .ContinueWith(OnHttpCompleted);
            }

            else InvokeSuccess();
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
        private void OnHttpCompleted(Task<HttpResult> X)
        {
            HttpResult Result = X.Result;

            if (Result.HasNetworkError)
                InvokeFailbacks(SessionError.AuthNetworkError);

            else if (!Result.Success)
            {
                switch (Result.StatusCode)
                {
                    case 401: /* Unauthorized. */
                    case 403: /* Forbidden. */
                    case 404: /* Not Found. */
                        InvokeSuccess();
                        break;

                    case 500: /* Internal Server Error. */
                    case 503: /* Service Unavailable. */
                        InvokeFailbacks(SessionError.AuthServerError);
                        break;

                    case 501: /* Not Implemented. */
                    default:
                        InvokeFailbacks(SessionError.AuthResponseError);
                        break;
                }
            }

            else InvokeSuccess();
        }

        private void InvokeSuccess()
        {
            m_Success?.Invoke();
            m_TCS.SetResult(m_Session);
        }
    }
}
