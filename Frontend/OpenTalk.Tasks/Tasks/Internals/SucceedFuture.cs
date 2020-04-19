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
    /// <typeparam name="ResultType"></typeparam>
    internal class SucceedFuture : Future
    {
        /// <summary>
        /// 이미 완료된 미래 객체를 초기화합니다.
        /// </summary>
        public SucceedFuture() { }

        /// <summary>
        /// 이미 완료된 미래 객체는 항상 Succeed 상태를 가집니다.
        /// </summary>
        public override FutureStatus Status => FutureStatus.Succeed;

        /// <summary>
        /// 이미 완료된 미래 객체는 항상 대기하지 않고 바로 반환합니다.
        /// </summary>
        public override bool Wait() => true;

        /// <summary>
        /// 이미 완료된 미래 객체는 항상 대기하지 않고 바로 반환합니다.
        /// </summary>
        public override bool Wait(int Milliseconds) => true;

        /// <summary>
        /// 이미 완료된 미래 객체는 이 메서드가 절대 호출되지 않습니다.
        /// </summary>
        protected override void OnCancel() { }
    }

    /// <summary>
    /// 이미 완료된 미래 객체를 표현합니다.
    /// </summary>
    /// <typeparam name="ResultType"></typeparam>
    internal class SucceedFuture<ResultType> : Future<ResultType>
    {
        /// <summary>
        /// 이미 완료된 미래 객체를 초기화합니다.
        /// </summary>
        /// <param name="Result"></param>
        public SucceedFuture(ResultType Result) => this.Result = Result;

        /// <summary>
        /// 이미 완료된 미래 객체는 항상 Succeed 상태를 가집니다.
        /// </summary>
        public override FutureStatus Status => FutureStatus.Succeed;

        /// <summary>
        /// 이미 완료된 미래 객체는 항상 대기하지 않고 바로 반환합니다.
        /// </summary>
        public override bool Wait() => true;

        /// <summary>
        /// 이미 완료된 미래 객체는 항상 대기하지 않고 바로 반환합니다.
        /// </summary>
        public override bool Wait(int Milliseconds) => true;

        /// <summary>
        /// 이미 완료된 작업의 결과를 확인합니다.
        /// </summary>
        public override ResultType Result { get; }

        /// <summary>
        /// 이미 완료된 미래 객체는 이 메서드가 절대 호출되지 않습니다.
        /// </summary>
        protected override void OnCancel() { }
    }
}
