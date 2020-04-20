using System;

namespace OpenTalk.Tasks
{
    public interface IFutureEventSource
    {
        /// <summary>
        /// 이벤트의 실행 인자를 생성하는 팩토리 콜백입니다.
        /// </summary>
        Func<IFutureEventSource, EventArgs> EventArgs { get; set; }

        /// <summary>
        /// 현재 이벤트 상태와 연관된 작업 객체입니다.
        /// </summary>
        Future Future { get; }

        /// <summary>
        /// 사용자 정의 태그입니다.
        /// </summary>
        object Tag { get; set; }

        /// <summary>
        /// Changed 이벤트의 sender인자로 사용될 객체를 설정합니다.
        /// </summary>
        object Sender { get; set; }

        /// <summary>
        /// 이 객체의 상태가 변하면 발생하는 이벤트입니다.
        /// </summary>
        event EventHandler Changed;

        /// <summary>
        /// 이벤트를 설정합니다.
        /// </summary>
        void Set();

        /// <summary>
        /// 이벤트를 해제합니다.
        /// </summary>
        void Unset();
    }

    public interface IFutureEventSource<ResultType> : IFutureEventSource
    {
        /// <summary>
        /// 이벤트를 설정합니다.
        /// </summary>
        void Set(ResultType Result);
    }
}