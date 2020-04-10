using System;

namespace OpenTalk.IO
{
    public interface IWriteInterface
    {
        /// <summary>
        /// 이 출력 인터페이스의 출력 채널이 살아있는지 검사합니다.
        /// </summary>
        bool IsWriteAlive { get; }

        /// <summary>
        /// 이 출력 인터페이스가 중간 단계에서 버퍼링을 구현하는지 확인합니다.
        /// </summary>
        bool IsBufferedWrite { get; }

        /// <summary>
        /// 이 출력 인터페이스로 데이터를 즉시 출력할 수 있는지 확인합니다.
        /// </summary>
        bool CanWriteImmediately { get; }

        /// <summary>
        /// 이 출력 인터페이스가 출력 준비/닫힘 이벤트를 발생시키는지 확인합니다.
        /// </summary>
        bool RaisesWriteEvents { get; }

        /// <summary>
        /// 사용자 정의 출력 상태입니다.
        /// </summary>
        object UserWriteState { get; set; }

        /// <summary>
        /// 이 출력 인터페이스가 데이터를 출력할 수 있게되면 실행되는 이벤트입니다.
        /// 이 이벤트 동작의 각 인자는, 읽기 인터페이스와, 현재 가용 바이트 수입니다.
        /// 가용 바이트 수가 0보다 작은 경우는 가용 크기 계산을 지원하지 않는 경우입니다.
        /// </summary>
        event Action<IWriteInterface, int> WriteReady;

        /// <summary>
        /// 이 입력 인터페이스가 닫히면 발생하는 이벤트입니다.
        /// </summary>
        event Action<IWriteInterface> WriteClosed;

        /// <summary>
        /// 이 출력 인터페이스로 데이터를 출력합니다.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        int Write(byte[] buffer, int offset, int length);

        /// <summary>
        /// 이 출력 인터페이스를 닫습니다.
        /// </summary>
        /// <returns></returns>
        bool CloseWrite();
    }
}
