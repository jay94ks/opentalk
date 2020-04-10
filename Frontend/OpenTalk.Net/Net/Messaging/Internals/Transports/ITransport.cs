using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Net.Messaging.Internals.Transports
{
    public enum ETransportState
    {
        Connecting,
        Connected,
        Refused,
        Unreachable,
        Closed
    }

    public interface ITransport
    {
        /// <summary>
        /// 이 전송 계층이 살아있는지 확인합니다.
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// 핸드쉐이크가 완료되었는지 검사합니다.
        /// </summary>
        bool IsHandshaked { get; }

        /// <summary>
        /// 재접속 시도시 사용해야하는 인증 토큰입니다.
        /// </summary>
        string Authorization { get; }

        /// <summary>
        /// 이 전송 계층의 상태를 확인합니다.
        /// </summary>
        ETransportState State { get; }

        /// <summary>
        /// 메시지를 수신합니다.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        string Receive(int timeout);

        /// <summary>
        /// 메시지를 송신합니다.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        bool Send(string message, int timeout);

        /// <summary>
        /// 이 전송 계층의 생존 여부를 확인합니다.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        bool Ping(int timeout);

        /// <summary>
        /// 이 전송계층의 연결을 파기합니다.
        /// </summary>
        void Close();
    }
}
