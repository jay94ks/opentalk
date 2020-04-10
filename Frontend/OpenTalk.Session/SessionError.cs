namespace OpenTalk
{
    public enum SessionError
    {
        /// <summary>
        /// 오류가 없습니다.
        /// </summary>
        None = 0,

        /// <summary>
        /// 원인을 알 수 없습니다.
        /// </summary>
        Unknown,

        /// <summary>
        /// 이미 실행중인 어떤 작업 때문에 요청한 작업을 수행할 수 없습니다.
        /// </summary>
        SessionBusy,

        /// <summary>
        /// 이미 로그인 시도를 하는 중입니다.
        /// </summary>
        AuthBusy,

        /// <summary>
        /// 이미 로그인된 세션입니다.
        /// </summary>
        AuthAlready,

        /// <summary>
        /// 강제로 인증이 해제되었습니다.
        /// (네트워크 오류, Textile 서버와 연결이 끊어짐 등...)
        /// </summary>
        AuthDeauthenticatedForcely,

        /// <summary>
        /// 인증 응답에 오류가 있어 인증에 성공했는지 실패했는지 확인할 수 없습니다.
        /// </summary>
        AuthResponseError,

        /// <summary>
        /// 자격증명 정보가 유효하지 않습니다.
        /// </summary>
        AuthInvalidCredential,

        /// <summary>
        /// 자격증명 정보가 만료되었습니다.
        /// </summary>
        AuthExpiredCredential,

        /// <summary>
        /// 알 수 없는 이유로 로그인이 거부되었습니다.
        /// </summary>
        AuthDenied,

        /// <summary>
        /// 인증 서버와 통신할 수 없었습니다.
        /// </summary>
        AuthNetworkError,

        /// <summary>
        /// 인증 서버와 통신할 수 없었습니다.
        /// </summary>
        AuthServerError,
    }
}
