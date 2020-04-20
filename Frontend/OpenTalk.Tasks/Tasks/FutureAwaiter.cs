using System;
using System.Runtime.CompilerServices;

namespace OpenTalk.Tasks
{
    /// <summary>
    /// C# 자체 문법인 await으로 Future를 대기할 수 있도록 Adapting합니다.
    /// </summary>
    public struct FutureAwaiter : ICriticalNotifyCompletion, INotifyCompletion
    {
        private Future Future;
        internal FutureAwaiter(Future Future) => this.Future = Future;

        /// <summary>
        /// 이 대기자가 완료되었는지 검사합니다.
        /// </summary>
        public bool IsCompleted => Future.IsCompleted;

        /// <summary>
        /// 이 대기자가 완료되면 결과를 반환합니다.
        /// </summary>
        public void GetResult() => Future.Wait();

        /// <summary>
        /// 연속으로 실행될 메서드를 등록합니다.
        /// </summary>
        /// <param name="continuation"></param>
        public void OnCompleted(Action continuation) => Future.Then(continuation);

        /// <summary>
        /// 연속으로 실행될 메서드를 등록합니다.
        /// </summary>
        /// <param name="continuation"></param>
        public void UnsafeOnCompleted(Action continuation) => Future.Then(continuation);
    }

    /// <summary>
    /// C# 자체 문법인 await으로 Future를 대기할 수 있도록 Adapting합니다.
    /// </summary>
    public struct FutureAwaiter<ResultType> : ICriticalNotifyCompletion, INotifyCompletion
    {
        private Future<ResultType> Future;
        internal FutureAwaiter(Future<ResultType> Future) => this.Future = Future;

        /// <summary>
        /// 이 대기자가 완료되었는지 검사합니다.
        /// </summary>
        public bool IsCompleted => Future.IsCompleted;

        /// <summary>
        /// 이 대기자가 완료되면 결과를 반환합니다.
        /// </summary>
        public ResultType GetResult() => Future.Result;

        /// <summary>
        /// 연속으로 실행될 메서드를 등록합니다.
        /// </summary>
        /// <param name="continuation"></param>
        public void OnCompleted(Action continuation) => Future.Then(continuation);

        /// <summary>
        /// 연속으로 실행될 메서드를 등록합니다.
        /// </summary>
        /// <param name="continuation"></param>
        public void UnsafeOnCompleted(Action continuation) => Future.Then(continuation);
    }
}
