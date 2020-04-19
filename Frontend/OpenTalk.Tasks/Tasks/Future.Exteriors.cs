using OpenTalk.Tasks.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Tasks
{
	/// <summary>
    /// 외장 메서드들입니다. (Null Safe)
    /// </summary>
    public static class FutureExteriors
    {
        /// <summary>
        /// 지정된 작업을 취소합니다. 이미 완료되어
        /// 취소할 수 없는 경우, false를 반환합니다.
        /// 
        /// 단, 이 메서드가 완전한 취소를 보장하지는 않습니다.
        /// </summary>
        /// <param name="This"></param>
        /// <returns></returns>
        public static bool Cancel(this Future This) 
            => This == null || Future.Cancel(This);


        /// <summary>
        /// 이 작업이 취소되면 실행될 작업을 등록합니다.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Functor"></param>
        /// <returns></returns>
        public static Future WhenCanceled(this Future This, Action<Future> Functor)
        {
            if (This != null)
            {
                return Future.After(This, (X) =>
                {
                    if (X.IsCanceled)
                        Functor(X);
                });
            }

            return Future.Run(() => Functor(
                new FaultedFuture(new NullReferenceException())));
        }

        /// <summary>
        /// 이 작업이 실패하면 실행될 작업을 등록합니다.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Functor"></param>
        /// <returns></returns>
        public static Future WhenFaulted(this Future This, Action<Future> Functor)
        {
            if (This != null)
            {
                return Future.After(This, (X) =>
                {
                    if (X.IsFaulted)
                        Functor(X);
                });
            }

            return Future.Run(() => Functor(
                new FaultedFuture(new NullReferenceException())));
        }

        /// <summary>
        /// 이 작업이 취소되거나 실패하면 실행될 작업을 등록합니다.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Functor"></param>
        /// <returns></returns>
        public static Future WhenNegative(this Future This, Action<Future> Functor)
        {
            if (This != null)
            {
                FutureSource NewFuture = new FutureSource();
                Future ChainedFuture = null;

                lock (This)
                {
                    // 취소 요청이 들어오면, 연계 작업을 실행하는 것 자체를 취소합니다.
                    NewFuture.Canceled +=
                        (X, Y) => ChainedFuture.Cancel();

                    ChainedFuture = Future.After(This, (X) =>
                    {
                        if (X.IsFaulted || X.IsCanceled)
                        {
                            try { Functor(X); }
                            catch (Exception e)
                            {
                                NewFuture.TrySetFaulted(e);
                                return;
                            }

                            NewFuture.TrySetCompleted();
                        }

                        else NewFuture.TrySetCanceled();
                    });
                }

                return NewFuture.Future;
            }

            return Future.Run(() => Functor(
                new FaultedFuture(new NullReferenceException())));
        }

        /// <summary>
        /// 이 작업이 완료되면 실행될 작업을 등록합니다.
        /// 해당 작업이 이미 완료되었거나, null인 경우 Functor를 즉시 실행합니다.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Functor"></param>
        /// <returns></returns>
        public static Future Then(this Future This, Action Functor)
        {
            if (This != null)
                return Future.After(This, Functor);

            return Future.Run(Functor);
        }

        /// <summary>
        /// 이 작업이 완료되면 실행될 작업을 등록합니다.
        /// 해당 작업이 이미 완료되었거나, null인 경우 Functor를 즉시 실행합니다.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Functor"></param>
        /// <returns></returns>
        public static Future<ResultType> Then<ResultType>(this Future This, Func<ResultType> Functor)
        {
            if (This != null)
                return Future.After(This, Functor);

            return Future.Run(Functor);
        }

        /// <summary>
        /// 이 작업이 완료되면 실행될 작업을 등록합니다.
        /// 해당 작업이 이미 완료되었거나, null인 경우 Functor를 즉시 실행합니다.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Functor"></param>
        /// <returns></returns>
		public static Future Then(this Future This, Action<Future> Functor)
        {
            if (This != null)
                return Future.After(This, Functor);

            return Future.Run(() => Functor(
                new FaultedFuture(new NullReferenceException())));
        }

        /// <summary>
        /// 이 작업이 완료되면 실행될 작업을 등록합니다.
        /// 해당 작업이 이미 완료되었거나, null인 경우 Functor를 즉시 실행합니다.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Functor"></param>
        /// <returns></returns>
		public static Future<ResultType> Then<ResultType>(this Future This, Func<Future, ResultType> Functor)
        {
            if (This != null)
                return Future.After(This, Functor);

            return Future.Run(() => Functor(
                new FaultedFuture(new NullReferenceException())));
        }

        /// <summary>
        /// 이 작업이 완료되면 실행될 작업을 등록합니다.
        /// 해당 작업이 이미 완료되었거나, null인 경우 Functor를 즉시 실행합니다.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Functor"></param>
        /// <returns></returns>
		public static Future<ResultType> Then<ResultType, PrevType>(this Future<PrevType> This, Func<Future<PrevType>, ResultType> Functor)
        {
            if (This != null)
                return Future.After(This, Functor);

            return Future.Run(() => Functor(
                new FaultedFuture<PrevType>(new NullReferenceException())));
        }
    }
}
