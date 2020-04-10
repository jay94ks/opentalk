using OpenTalk.Net.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk
{
    /// <summary>
    /// OpenTalk 세션 객체.
    /// 이 클래스는 세션을 구성하는 각 파트에 대한 Mediator입니다.
    /// </summary>
    public partial class Session : Application.BaseObject
    {
        private Auth m_Authenticator;
        private SessionError m_LatestError;
        private bool m_Locked;
        private Uri m_GatewayUri;

        /// <summary>
        /// OpenTalk 세션 객체를 초기화합니다.
        /// </summary>
        public Session(Uri gatewayUri) : this(null, gatewayUri) { }

        /// <summary>
        /// OpenTalk 세션 객체를 초기화합니다.
        /// </summary>
        public Session(Application application, Uri gatewayUri) 
            : base(application)
        {
            m_Locked = false;
            m_LatestError = SessionError.None;
            m_GatewayUri = gatewayUri;
            m_Authenticator = new Auth(this);
        }

        /// <summary>
        /// 게이트웨이 서버 URI를 획득합니다.
        /// </summary>
        internal Uri GatewayUri => m_GatewayUri;

        /// <summary>
        /// 가장 마지막에 발생했던 오류 코드를 반환합니다.
        /// </summary>
        public SessionError ErrorCode => this.Locked(() => m_LatestError);

        /// <summary>
        /// HttpComponent를 획득합니다.
        /// </summary>
        /// <returns></returns>
        internal HttpComponent GetHttpComponent() 
            => HttpComponent.GetHttpComponent(GatewayUri);

        /// <summary>
        /// 현재 세션 상태를 변경하는 어떤 동작을 이미 진행중인지 확인하고,
        /// 진행중이 아니라면 차폐할지 여부를 설정합니다.
        /// </summary>
        internal bool TakeBusy(bool throwException = true)
        {
            lock (this)
            {
                if (m_Locked)
                {
                    if (throwException)
                        throw new SessionException(SessionError.SessionBusy);

                    return false;
                }

                m_Locked = true;
            }

            return true;
        }

        /// <summary>
        /// 현재 세션 상태를 변경하는 모든 동작이 차폐되어 있는지 여부를 확인합니다.
        /// </summary>
        /// <returns></returns>
        internal bool IsBusy() => this.Locked(() => m_Locked);

        /// <summary>
        /// 현재 세션 상태를 변경하는 모든 동작을 차폐하는 플래그를 제거합니다.
        /// </summary>
        /// <param name="Value"></param>
        internal void FreeBusy() => this.Locked(() => m_Locked = false);

        /// <summary>
        /// 가장 마지막에 발생한 오류 코드를 설정합니다.
        /// </summary>
        /// <param name="errorCode"></param>
        internal void SetErrorCode(SessionError errorCode)
        {
            lock (this)
                m_LatestError = errorCode;
        }
    }
}
