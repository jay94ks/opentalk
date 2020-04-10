using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk
{
    public static class Extensions
    {
        /// <summary>
        /// 이 작업이 끝날때 까지 기다렸다가 그 결과를 획득합니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="anyTask"></param>
        /// <returns></returns>
        public static T WaitResult<T>(this Task<T> anyTask)
        {
            if (!anyTask.IsCompleted)
                anyTask.Wait();

            return anyTask.Result;
        }

        /// <summary>
        /// 이 작업이 성공하면 지정된 콜백을 실행하는 작업을 생성합니다.
        /// </summary>
        /// <param name="anyTask"></param>
        /// <param name="functor"></param>
        /// <returns></returns>
        public static Task IfDone(this Task anyTask, Action functor)
        {
            return anyTask.ContinueWith((X) =>
            {
                if (X.IsCanceled || X.IsFaulted)
                    return;

                functor();
            });
        }

        /// <summary>
        /// 이 작업이 성공하면 지정된 콜백을 실행하는 작업을 생성합니다.
        /// </summary>
        /// <param name="anyTask"></param>
        /// <param name="functor"></param>
        /// <returns></returns>
        public static Task IfDone<ResultType>(this Task<ResultType> anyTask, Action<ResultType> functor)
        {
            return anyTask.ContinueWith((X) =>
            {
                if (X.IsCanceled || X.IsFaulted)
                    return;

                functor(X.WaitResult());
            });
        }

        /// <summary>
        /// 이 작업이 실패하면 지정된 콜백을 실행하는 작업을 생성합니다.
        /// </summary>
        /// <param name="anyTask"></param>
        /// <param name="functor"></param>
        /// <returns></returns>
        public static Task IfFail(this Task anyTask, Action functor)
        {
            return anyTask.ContinueWith((X) =>
            {
                if (!X.IsCanceled && !X.IsFaulted)
                    return;

                functor();
            });
        }

        /// <summary>
        /// 이 작업이 실패하면 지정된 콜백을 실행하는 작업을 생성합니다.
        /// </summary>
        /// <param name="anyTask"></param>
        /// <param name="functor"></param>
        /// <returns></returns>
        public static Task IfFail<T>(this Task<T> anyTask, Action<Task<T>> functor)
        {
            return anyTask.ContinueWith((X) =>
            {
                if (!X.IsCanceled && !X.IsFaulted)
                    return;

                functor(anyTask);
            });
        }

        /// <summary>
        /// 이 작업이 끝나면 메시지 루프에서 지정된 작업을 수행하는 작업을 생성합니다.
        /// </summary>
        /// <param name="anyTask"></param>
        /// <param name="functor"></param>
        /// <returns></returns>
        public static Task ContinueOnMessageLoop(this Task anyTask, Action<Task> functor) 
            => anyTask.ContinueWith((oldTask) => Application.Tasks.Invoke(() => functor(oldTask)).Wait());
        
        /// <summary>
        /// 이 작업이 끝나면 메시지 루프에서 지정된 작업을 수행하는 작업을 생성합니다.
        /// </summary>
        /// <param name="anyTask"></param>
        /// <param name="functor"></param>
        /// <returns></returns>
        public static Task<T> ContinueOnMessageLoop<T>(this Task anyTask, Func<Task, T> functor)
            => anyTask.ContinueWith((oldTask) => Application.Tasks.Invoke(() => functor(oldTask)).WaitResult());

        /// <summary>
        /// lock 키워드로, 락을 걸고, functor를 실행합니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="This"></param>
        /// <param name="functor"></param>
        /// <returns></returns>
        public static U Locked<T, U>(this T This, Func<T, U> functor)
        {
            lock (This)
                return functor(This);
        }

        /// <summary>
        /// lock 키워드로, 락을 걸고, functor를 실행합니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="This"></param>
        /// <param name="functor"></param>
        /// <returns></returns>
        public static U Locked<T, U>(this T This, Func<U> functor)
        {
            lock (This)
                return functor();
        }


        /// <summary>
        /// lock 키워드로, 락을 걸고, functor를 실행합니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="This"></param>
        /// <param name="functor"></param>
        /// <returns></returns>
        public static T Locked<T>(this T This, Action<T> functor)
        {
            lock (This)
                functor(This);

            return This;
        }
    }
}
