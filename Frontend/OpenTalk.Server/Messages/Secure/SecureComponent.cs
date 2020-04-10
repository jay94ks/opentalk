using OpenTalk.Net.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Server.Messages.Secure
{
    /// <summary>
    /// 잠금 모드로 로그인 되었는지 아닌지에 대한 상태를 유지하는 컴포넌트입니다.
    /// </summary>
    public class SecureComponent : Connection.Component
    {
        protected override void OnInitialize()
        {
            base.OnInitialize();

            // 이 컴포넌트가 커넥션 객체에 부착된 초기에는
            // 항상 잠긴 상태입니다.

            lock (this)
            {
                Locked = true;
            }
        }

        /// <summary>
        /// 현재 잠금 모드가 설정되었는지 여부입니다.
        /// </summary>
        public bool Locked { get; private set; } = true;

        /// <summary>
        /// 잠금 모드를 설정합니다.
        /// </summary>
        public void Lock()
        {
            lock (this)
                Locked = true;
        }

        /// <summary>
        /// 잠금 모드를 해제합니다.
        /// </summary>
        /// <param name="HashedPassword"></param>
        public bool Unlock(string HashedPassword)
        {
            HttpComponent http = HttpComponent.GetHttpComponent(Connection.Server,
                Connection.Server.AuthorizationSettings.BaseUri);

            string TargetPath = MakeUnlockAuthentication(http);
            HttpResult Result = null;

            if ((Result = http.Get(TargetPath).WaitResult()).Success)
            {
                lock (this)
                    Locked = false;

                return true;
            }

            return false;
        }

        /// <summary>
        /// 잠금 모드 해제를 위한 인증을 수행하는 URL을 만듭니다.
        /// </summary>
        /// <param name="http"></param>
        /// <returns></returns>
        private string MakeUnlockAuthentication(HttpComponent http)
        {
            AuthorizationSettings.RequestTarget Authorizer
                = Connection.Server.AuthorizationSettings.Verify;

            return HttpHelper.CombinePath(
                Authorizer.Path, Authorizer.QueryStrings,
                "authkey=" + Connection.Authorization);
        }
    }
}
