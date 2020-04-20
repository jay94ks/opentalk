using OpenTalk.Tasks.Internals;
using System;

namespace OpenTalk.Tasks
{
    public abstract partial class Future
    {
        /// <summary>
        /// 지정된 작업이 완료되면 펑터를 실행하는 작업을 생성합니다.
        /// </summary>
        /// <param name="Task"></param>
        /// <param name="Callback"></param>
        /// <returns></returns>
        public Future Then(Action Functor)
        {
            ChainedFuture Future;

            lock (this)
            {
                if (IsCompleted || m_Chains == null)
                    return Run(() => Functor());

                Chain(Future = new ChainedFuture(this, (X) => Functor()));
            }

            return Future;
        }

        /// <summary>
        /// 지정된 작업이 완료되면 펑터를 실행하는 작업을 생성합니다.
        /// </summary>
        /// <param name="Task"></param>
        /// <param name="Callback"></param>
        /// <returns></returns>
        public Future Then(Action<Future> Functor)
        {
            ChainedFuture Future;

            lock (this)
            {
                if (IsCompleted || m_Chains == null)
                    return Run(() => Functor(this));

                Chain(Future = new ChainedFuture(this, Functor));
            }

            return Future;
        }

        /// <summary>
        /// 이 작업이 완료되면 실행될 작업을 등록합니다.
        /// 해당 작업이 이미 완료되었거나, null인 경우 Functor를 즉시 실행합니다.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Functor"></param>
        /// <returns></returns>
        public Future<ResultType> Then<ResultType>(Func<ResultType> Functor)
        {
            ChainedFuture<ResultType> Future;

            lock (this)
            {
                if (IsCompleted || m_Chains == null)
                    return Run(() => Functor());

                Chain(Future = new ChainedFuture<ResultType>(this, (X) => Functor()));
            }

            return Future;
        }

        /// <summary>
        /// 이 작업이 완료되면 실행될 작업을 등록합니다.
        /// 해당 작업이 이미 완료되었거나, null인 경우 Functor를 즉시 실행합니다.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Functor"></param>
        /// <returns></returns>
        public Future<ResultType> Then<ResultType>(Func<Future, ResultType> Functor)
        {
            ChainedFuture<ResultType> Future;

            lock (this)
            {
                if (IsCompleted || m_Chains == null)
                    return Run(() => Functor(this));

                Chain(Future = new ChainedFuture<ResultType>(this, Functor));
            }

            return Future;
        }
    }


    public abstract partial class Future<ResultType>
    {

        /// <summary>
        /// 지정된 작업이 완료되면 펑터를 실행하는 작업을 생성합니다.
        /// </summary>
        /// <param name="Task"></param>
        /// <param name="Callback"></param>
        /// <returns></returns>
        public Future Then(Action<Future<ResultType>> Functor)
        {
            ChainedFuture Future;

            lock (this)
            {
                if (IsCompleted || m_Chains == null)
                    return Run(() => Functor(this));

                Chain(Future = new ChainedFuture(this, (X) => Functor(this)));
            }

            return Future;
        }

        /// <summary>
        /// 이 작업이 완료되면 실행될 작업을 등록합니다.
        /// 해당 작업이 이미 완료되었거나, null인 경우 Functor를 즉시 실행합니다.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Functor"></param>
        /// <returns></returns>
        public Future<NewType> Then<NewType>(Func<Future<ResultType>, NewType> Functor)
        {
            ChainedFuture<NewType> Future;

            lock (this)
            {
                if (IsCompleted || m_Chains == null)
                    return Run(() => Functor(this));

                Chain(Future = new ChainedFuture<NewType>(
                    this, (X) => Functor(this)));
            }

            return Future;
        }
    }
}
