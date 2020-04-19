using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Tasks.Internals
{
    /// <summary>
    /// 이미 완료된 미래 객체를 표현합니다.
    /// </summary>
    internal class FaultedFuture : Future
    {
        /// <summary>
        /// 이미 완료된 미래 객체를 초기화합니다.
        /// </summary>
        /// <param name="Result"></param>
        public FaultedFuture(Exception e) => Exception = e;

        /// <summary>
        /// 이미 완료된 미래 객체는 항상 Succeed 상태를 가집니다.
        /// </summary>
        public override FutureStatus Status => FutureStatus.Faulted;

        /// <summary>
        /// 이 실패와 연관된 예외입니다.
        /// </summary>
        public override Exception Exception { get; }

        /// <summary>
        /// 이미 완료된 미래 객체는 항상 대기하지 않고 바로 반환합니다.
        /// </summary>
        public override bool Wait() => IsCompleted;

        /// <summary>
        /// 이미 완료된 미래 객체는 항상 대기하지 않고 바로 반환합니다.
        /// </summary>
        public override bool Wait(int Milliseconds) => IsCompleted;

        /// <summary>
        /// 이미 완료된 미래 객체는 이 메서드가 절대 호출되지 않습니다.
        /// </summary>
        protected override void OnCancel() { }
    }

    /// <summary>
    /// 이미 완료된 미래 객체를 표현합니다.
    /// </summary>
    /// <typeparam name="ResultType"></typeparam>
    internal class FaultedFuture<ResultType> : Future<ResultType>
    {
        /// <summary>
        /// 이미 완료된 미래 객체를 초기화합니다.
        /// </summary>
        /// <param name="Result"></param>
        public FaultedFuture(Exception e) => Exception = e;

        /// <summary>
        /// 이미 완료된 미래 객체는 항상 Succeed 상태를 가집니다.
        /// </summary>
        public override FutureStatus Status => FutureStatus.Faulted;

        /// <summary>
        /// 이 실패와 연관된 예외입니다.
        /// </summary>
        public override Exception Exception { get; }

        /// <summary>
        /// 이미 완료된 미래 객체는 항상 대기하지 않고 바로 반환합니다.
        /// </summary>
        public override bool Wait() => IsCompleted;

        /// <summary>
        /// 이미 완료된 미래 객체는 항상 대기하지 않고 바로 반환합니다.
        /// </summary>
        public override bool Wait(int Milliseconds) => IsCompleted;

        /// <summary>
        /// 이미 완료된 작업의 결과를 확인합니다.
        /// </summary>
        public override ResultType Result => throw new FutureImpossibleException(FutureImpossibleReason.Faulted);

        /// <summary>
        /// 이미 완료된 미래 객체는 이 메서드가 절대 호출되지 않습니다.
        /// </summary>
        protected override void OnCancel() { }
    }
}