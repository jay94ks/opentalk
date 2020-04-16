using OpenTalk.Credentials;
using OpenTalk.Internals;
using System;
using System.Threading.Tasks;

namespace OpenTalk
{
    public partial class Session
    {
        /// <summary>
        /// OpenTalk 세션의 인증 정보를 관리합니다.
        /// </summary>
        public class Auth : BaseObject
        {
            private Credential m_Credential;
            private Credential m_TryingCredential;
            private Credential m_RestorationCredential;
            private string m_AuthorizationToken;

            /// <summary>
            /// 어떤 동작인지 기술합니다.
            /// </summary>
            public enum Operation
            {
                Authentication,
                Deauthentication,
                Enforced
            }

            /// <summary>
            /// 인증 상태가 변경되면 실행되는 이벤트 핸들러입니다.
            /// </summary>
            /// <param name="session"></param>
            public delegate void Event(Session session, Operation operation, SessionError errorCode);

            /// <summary>
            /// Auth 객체를 초기화합니다.
            /// </summary>
            /// <param name="session"></param>
            internal Auth(Session session)
                : base(session)
            {
                m_Credential = null;
                m_TryingCredential = null;
                m_RestorationCredential = null;
                m_AuthorizationToken = null;
            }

            /// <summary>
            /// 이 세션에 로그인하기 위해 사용한 자격증명 정보를 획득합니다.
            /// </summary>
            public Credential Credential => this.Locked(() => m_Credential);

            /// <summary>
            /// 현재 로그인을 수행하는 중이라면 시도중인 자격증명 정보를 반환합니다.
            /// </summary>
            public Credential TryingCredential => this.Locked(() => m_TryingCredential);

            /// <summary>
            /// 로그인에 성공한 경우, 다음번에 자동로그인에 사용될 수 있는 자격증명 정보를 반환합니다.
            /// </summary>
            public Credential RestorationCredential => this.Locked(() => m_RestorationCredential);

            /// <summary>
            /// 인증에 성공하거나, 실패할 때 발생하는 이벤트입니다.
            /// </summary>
            public event Event Authenticated;

            /// <summary>
            /// 인증이 해제되거나, 인증 해제에 실패할 때 발생하는 이벤트입니다.
            /// </summary>
            public event Event Deauthenticated;

            /// <summary>
            /// 주어진 자격증명 정보로 로그인을 시도합니다.
            /// </summary>
            /// <param name="credential"></param>
            /// <returns></returns>
            public Task<Session> Authenticate(Credential credential)
            {
                lock (this)
                {
                    // 이미 로그인 된 경우.
                    if (m_Credential != null)
                        throw new SessionException(SessionError.AuthAlready);

                    // 아직 로그인 시도중인 경우.
                    if (m_TryingCredential != null)
                        throw new SessionException(SessionError.AuthBusy);

                    Session.TakeBusy();

                    m_AuthorizationToken = null;
                    m_RestorationCredential = null;
                    m_TryingCredential = credential;
                }

                return (new Authenticator(Session, credential,
                    OnAuthenticationSuccess, OnAuthenticationFailure)).Run();
            }

            /// <summary>
            /// 이 세션이 로그인되어 있다면 로그아웃 합니다.
            /// </summary>
            /// <returns></returns>
            public Task<Session> Deauthenticate()
            {
                lock (this)
                {
                    // 이미 로그아웃되어 있는 경우.
                    if (m_Credential == null)
                        throw new SessionException(SessionError.AuthAlready);

                    // 아직 로그인 시도중인 경우.
                    if (m_TryingCredential != null)
                        throw new SessionException(SessionError.AuthBusy);

                    Session.TakeBusy();
                }

                return (new Deauthenticator(Session, OnDeauthenticationSuccess, 
                    OnDeauthenticationFailure)).Run();
            }

            /// <summary>
            /// 이 세션은 이미 연결이 끊어져 인증이 해제되었습니다.
            /// </summary>
            internal void EnforceDeauthentication()
            {
                lock(this)
                {
                    if (m_Credential == null)
                        return;
                }

                UnsetAllAuthentications(SessionError.AuthDeauthenticatedForcely);
                Deauthenticated?.Invoke(Session, 
                    Operation.Deauthentication | Operation.Enforced, 
                    SessionError.AuthDeauthenticatedForcely);
            }

            /// <summary>
            /// 인증에 성공하면 실행되는 콜백입니다.
            /// </summary>
            /// <param name="obj"></param>
            private void OnAuthenticationSuccess(Credential restoration, string token)
            {
                lock(this)
                {
                    m_Credential = m_TryingCredential;
                    m_RestorationCredential = restoration;

                    m_TryingCredential = null;
                    m_AuthorizationToken = token;
                }

                Session.GetHttpComponent().Authorization = token;
                Session.SetErrorCode(SessionError.None);
                Session.FreeBusy();

                Authenticated?.Invoke(Session, Operation.Authentication, SessionError.None);
            }

            /// <summary>
            /// 인증에 실패하면 실행되는 콜백입니다.
            /// </summary>
            private void OnAuthenticationFailure(SessionError errorCode)
            {
                UnsetAllAuthentications(errorCode);
                Authenticated?.Invoke(Session, Operation.Authentication, errorCode);
            }

            /// <summary>
            /// 인증 해제에 성공하면 실행되는 콜백입니다.
            /// </summary>
            private void OnDeauthenticationSuccess()
            {
                UnsetAllAuthentications(SessionError.None);
                Deauthenticated?.Invoke(Session, Operation.Deauthentication, SessionError.None);
            }

            /// <summary>
            /// 인증 해제에 실패하면 실행되는 콜백입니다.
            /// </summary>
            private void OnDeauthenticationFailure(SessionError errorCode)
            {
                Session.SetErrorCode(errorCode);
                Session.FreeBusy();

                Deauthenticated?.Invoke(Session, Operation.Deauthentication, errorCode);
            }

            /// <summary>
            /// 인증 정보를 모두 소거합니다.
            /// </summary>
            /// <param name="errorCode"></param>
            private void UnsetAllAuthentications(SessionError errorCode)
            {
                lock (this)
                {
                    m_Credential = m_TryingCredential = null;
                    m_RestorationCredential = null;
                    m_AuthorizationToken = null;
                }

                Session.GetHttpComponent().Authorization = null;
                Session.SetErrorCode(errorCode);
                Session.FreeBusy();
            }

        }
    }
}
