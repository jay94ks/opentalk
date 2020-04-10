using OpenTalk.IO;
using System;
using System.Net;

namespace OpenTalk.Net
{
    /// <summary>
    /// 신뢰가능한 양방향 연결 소켓 인터페이스입니다.
    /// </summary>
    public interface IReliableSocket
        : IReadInterface, IWriteInterface
    {
        /// <summary>
        /// 사용자 정의 상태 객체입니다.
        /// </summary>
        object UserState { get; set; }

        /// <summary>
        /// 목적 호스트에 접속합니다. (비동기)
        /// 반환값은 비동기 작업을 성공적으로 시작했는지 여부입니다.
        /// </summary>
        bool Connect(IPAddress host, int port, object state = null);

        /// <summary>
        /// 목적 호스트에 접속합니다. (비동기)
        /// 반환값은 비동기 작업을 성공적으로 시작했는지 여부입니다.
        /// </summary>
        bool Connect(string host, int port, object state = null);

        /// <summary>
        /// 접속을 거부당하면 발생하는 이벤트입니다.
        /// </summary>
        event Action<IReliableSocket, object> Refused;

        /// <summary>
        /// 접속할 호스트를 찾을 수 없으면 발생하는 이벤트입니다.
        /// </summary>
        event Action<IReliableSocket, object> Unreachable;

        /// <summary>
        /// 연결이 준비되면 실행되는 이벤트입니다.
        /// </summary>
        event Action<IReliableSocket, object> Ready;

        /// <summary>
        /// 커넥션이 끊어지면 실행되는 이벤트입니다.
        /// 이 이벤트가 발생하기 전에는 연결이 끊어진 것이 아닙니다.
        /// </summary>
        event Action<IReliableSocket> Closed;

        /// <summary>
        /// Read/Write 채널을 모두 닫고, 소켓을 정리합니다.
        /// </summary>
        bool Close();
    }
}