namespace OpenTalk
{
    public partial class Session
    {
        public class Textile : BaseObject
        {
            internal Textile(Session session) 
                : base(session)
            {
                session.Authentication.Authenticated += OnAuthenticationChanged;
                session.Authentication.Deauthenticated += OnAuthenticationChanged;
            }

            /// <summary>
            /// 인증 상태가 변경되면 실행됩니다.
            /// </summary>
            /// <param name="session"></param>
            /// <param name="operation"></param>
            /// <param name="errorCode"></param>
            private void OnAuthenticationChanged(Session session, Auth.Operation operation, SessionError errorCode)
            {
                switch (operation)
                {
                    case Auth.Operation.Authentication:
                        // 인증 성공 메시지.
                        if (Session.Authentication.Credential != null)
                        {

                        }
                        break;

                    case Auth.Operation.Deauthentication:
                        // 사용자가 의도한 인증 만료 (로그아웃).
                        if (Session.Authentication.Credential == null)
                        {

                        }
                        break;

                    case Auth.Operation.Deauthentication | Auth.Operation.Enforced:
                        // 사용자가 의도치 않은 인증 만료 (종료 or 연결 끊김).
                        // 종료가 아닌 경우엔 재접속 시도를 합니다.
                        break;
                }
            }
        }
    }
}
