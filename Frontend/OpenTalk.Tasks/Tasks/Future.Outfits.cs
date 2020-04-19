using OpenTalk.Tasks.Internals;
using System;

namespace OpenTalk.Tasks
{
    /// <summary>
    /// 닷넷 Task보다 가벼운 Task를 구현합니다.
    /// </summary>
    public abstract partial class Future
    {
        /// <summary>
        /// 지정된 작업이 null 레퍼런스인지 아닌지 Assertion을 수행합니다.
        /// </summary>
        /// <param name="Task"></param>
        private static void Assert(Future Task)
        {
            if (Task != null)
                return;

            throw new NullReferenceException();
        }

        /// <summary>
        /// 지정된 작업이 null 레퍼런스인지 아닌지 Assertion을 수행합니다.
        /// </summary>
        /// <param name="Task"></param>
        private static void Assert(Future[] Tasks)
        {
            if (Tasks != null)
            {
                foreach (Future Task in Tasks)
                {
                    if (Task == null)
                        throw new NullReferenceException();
                }

                return;
            }

            throw new NullReferenceException();
        }

        /// <summary>
        /// 지정된 작업을 취소합니다. 이미 완료되어
        /// 취소할 수 없는 경우, false를 반환합니다.
        /// 
        /// 단, 이 메서드가 완전한 취소를 보장하지는 않습니다.
        /// </summary>
        /// <param name="Future"></param>
        /// <returns></returns>
        public static bool Cancel(Future Future)
        {
            lock (Future)
            {
                if (Future.IsCompleted)
                    return false;

                // 취소의 실제 동작은 해당 구현체에서 결정합니다.
                if (!Future.IsCanceled)
                    Future.OnCancel();
            }

            return true;
        }

        /// <summary>
        /// 지정된 펑터를 쓰레드 풀에서 실행하는 작업을 생성합니다.
        /// </summary>
        /// <param name="Functor"></param>
        /// <returns></returns>
        public static Future Run(Action Functor)
            => new ActionFuture(Functor);

        /// <summary>
        /// 지연시간이 무한인 작업, 즉, 취소를 제외하고
        /// 완료될 수 없는 작업을 생성합니다.
        /// </summary>
        /// <returns></returns>
        public static Future MakeInfinite() => new DelayedFuture(-1);

        /// <summary>
        /// 지정된 시간 뒤에 완료되는 작업을 생성합니다.
        /// </summary>
        /// <param name="Milliseconds"></param>
        /// <returns></returns>
        public static Future MakeDelay(int Milliseconds)
            => new DelayedFuture(Milliseconds);

        /// <summary>
        /// 지정된 펑터를 쓰레드 풀에서 실행하는 작업을 생성합니다.
        /// </summary>
        /// <param name="Functor"></param>
        /// <returns></returns>
        public static Future<ResultType> Run<ResultType>(Func<ResultType> Functor)
            => new ActionFuture<ResultType>(Functor);

        /// <summary>
        /// 이미 완료된 작업을 생성합니다.
        /// </summary>
        /// <typeparam name="ResultType"></typeparam>
        /// <param name="Result"></param>
        /// <returns></returns>
        public static Future<ResultType> FromResult<ResultType>(ResultType Result)
            => new SucceedFuture<ResultType>(Result);

        /// <summary>
        /// 특정한 조건이 충족되면 완료되는 작업을 생성합니다.
        /// </summary>
        /// <param name="Condition"></param>
        /// <returns></returns>
        public static Future IfMet(Func<bool> Condition)
            => (new FutureConditionSource(Condition)).Future;

        /// <summary>
        /// 특정한 조건이 충족되면 실행되는 작업을 생성합니다.
        /// </summary>
        /// <param name="Condition"></param>
        /// <param name="Functor"></param>
        /// <returns></returns>
        public static Future IfMet(Func<bool> Condition, Action Functor)
            => IfMet(Condition).Then(Functor);

        /// <summary>
        /// 특정한 조건이 충족되면 실행되는 작업을 생성합니다.
        /// </summary>
        /// <param name="Condition"></param>
        /// <param name="Functor"></param>
        /// <returns></returns>
        public static Future<ResultType> IfMet<ResultType>(Func<bool> Condition, Func<ResultType> Functor)
            => IfMet(Condition).Then(Functor);

        /// <summary>
        /// 이미 취소된 작업을 생성합니다.
        /// </summary>
        /// <returns></returns>
        public static Future MakeCanceled() => new CanceledFuture();

        /// <summary>
        /// 이미 취소된 작업을 생성합니다.
        /// </summary>
        /// <returns></returns>
        public static Future<ResultType> MakeCanceled<ResultType>() => new CanceledFuture<ResultType>();

        /// <summary>
        /// 이미 취소된 작업을 생성합니다.
        /// 예외 정보가 주어지지 않으면, NullReferenceException 예외가 셋팅됩니다.
        /// </summary>
        /// <returns></returns>
        public static Future MakeFaulted(Exception e)
            => new FaultedFuture(e != null ? e : new NullReferenceException());

        /// <summary>
        /// 이미 취소된 작업을 생성합니다.
        /// 예외 정보가 주어지지 않으면, NullReferenceException 예외가 셋팅됩니다.
        /// </summary>
        /// <returns></returns>
        public static Future<ResultType> MakeFaulted<ResultType>(Exception e)
            => new FaultedFuture<ResultType>(e != null ? e : new NullReferenceException());

        /// <summary>
        /// 지정된 모든 작업이 끝나면 완료되는 작업을 생성합니다.
        /// </summary>
        /// <param name="Tasks"></param>
        /// <param name="Functor"></param>
        /// <returns></returns>
        public static Future AfterAll(params Future[] Tasks)
        {
            Assert(Tasks);

            foreach(Future Task in Tasks)
            {
                if (!Task.IsCompleted)
                    return new CombinedFuture(Tasks);
            }

            return new SucceedFuture();
        }

        /// <summary>
        /// 지정된 작업이 완료되면 펑터를 실행하는 작업을 생성합니다.
        /// </summary>
        /// <param name="Task"></param>
        /// <param name="Callback"></param>
        /// <returns></returns>
        public static Future After(Future Task, Action Functor)
        {
            ChainedFuture Future;
            Assert(Task);

            lock (Task)
            {
                if (Task.IsCompleted || Task.m_Chains == null)
                    return Run(Functor);

                Task.Chain(Future = new ChainedFuture(Task, (X) => Functor()));
            }

            return Future;
        }

        /// <summary>
        /// 지정된 작업이 완료되면 펑터를 실행하는 작업을 생성합니다.
        /// </summary>
        /// <param name="Task"></param>
        /// <param name="Callback"></param>
        /// <returns></returns>
        public static Future After(Future Task, Action<Future> Functor)
        {
            ChainedFuture Future;
            Assert(Task);

            lock (Task)
            {
                if (Task.IsCompleted || Task.m_Chains == null)
                    return Run(() => Functor(Task));

                Task.Chain(Future = new ChainedFuture(Task, Functor));
            }

            return Future;
        }

        /// <summary>
        /// 지정된 작업이 완료되면 펑터를 실행하는 작업을 생성합니다.
        /// </summary>
        /// <param name="Task"></param>
        /// <param name="Callback"></param>
        /// <returns></returns>
        public static Future<ResultType> After<ResultType>(Future Task, Func<ResultType> Functor)
        {
            ChainedFuture<ResultType> Future;
            Assert(Task);

            lock (Task)
            {
                if (Task.IsCompleted || Task.m_Chains == null)
                    return Run(() => Functor());

                Task.Chain(Future = new ChainedFuture<ResultType>(Task, (X) => Functor()));
            }

            return Future;
        }

        /// <summary>
        /// 지정된 작업이 완료되면 펑터를 실행하는 작업을 생성합니다.
        /// </summary>
        /// <param name="Task"></param>
        /// <param name="Callback"></param>
        /// <returns></returns>
        public static Future<ResultType> After<ResultType>(Future Task, Func<Future, ResultType> Functor)
        {
            ChainedFuture<ResultType> Future;
            Assert(Task);

            lock (Task)
            {
                if (Task.IsCompleted || Task.m_Chains == null)
                    return Run(() => Functor(Task));

                Task.Chain(Future = new ChainedFuture<ResultType>(Task, Functor));
            }

            return Future;
        }

        /// <summary>
        /// 지정된 작업이 완료되면 펑터를 실행하는 작업을 생성합니다.
        /// </summary>
        /// <param name="Task"></param>
        /// <param name="Callback"></param>
        /// <returns></returns>
        public static Future After<PreviousType>(Future<PreviousType> Task, Action<Future<PreviousType>> Functor)
        {
            ChainedFuture Future;
            Assert(Task);

            lock (Task)
            {
                if (Task.IsCompleted || Task.m_Chains == null)
                    return Run(() => Functor(Task));

                Task.Chain(Future = new ChainedFuture(Task, 
                    (X) => Functor(X as Future<PreviousType>)));
            }

            return Future;
        }

        /// <summary>
        /// 지정된 작업이 완료되면 펑터를 실행하는 작업을 생성합니다.
        /// </summary>
        /// <param name="Task"></param>
        /// <param name="Callback"></param>
        /// <returns></returns>
        public static Future<ResultType> After<ResultType, PreviousType>(
            Future<PreviousType> Task, Func<Future<PreviousType>, ResultType> Functor)
        {
            ChainedFuture<ResultType> Future;
            Assert(Task);

            lock (Task)
            {
                if (Task.IsCompleted || Task.m_Chains == null)
                    return Run(() => Functor(Task));

                Task.Chain(Future = new ChainedFuture<ResultType>(Task, 
                    (X) => Functor(X as Future<PreviousType>)));
            }

            return Future;
        }

        /// <summary>
        /// 주어진 모든 작업들이 완료되기를 기다립니다.
        /// </summary>
        /// <param name="Futures"></param>
        public static void WaitAll(params Future[] Tasks) => AfterAll(Tasks).Wait();


    }
}
