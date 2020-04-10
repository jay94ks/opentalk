using System;
using System.Threading.Tasks;

namespace OpenTalk
{
    public abstract partial class Application
    {
        /// <summary>
        /// 현재 실행중인 인스턴스를 대상으로 하는 작업들을 수행합니다.
        /// </summary>
        public static class Tasks
        {
            /// <summary>
            /// 현재 실행중인 어플리케이션 메시지 루프에서 Functor를 실행하며,
            /// 그 Functor가 실행되면 완료되는 Task 객체를 반환합니다.
            /// 
            /// 현재 실행중인 인스턴스가 없을 때, 
            /// ApplicationException 예외가 발생합니다.
            /// </summary>
            /// <param name="functor"></param>
            /// <returns></returns>
            public static Task Invoke(Action functor)
            {
                if (RunningInstance != null)
                    return RunningInstance.Invoke(functor);

                throw new ApplicationException();
            }

            /// <summary>
            /// 어플리케이션 메시지 루프에서 Functor를 실행하며,
            /// 그 Functor가 실행되면 완료되는 Task 객체를 반환합니다.
            /// 
            /// 현재 실행중인 인스턴스가 없을 때, 
            /// ApplicationException 예외가 발생합니다.
            /// </summary>
            /// <param name="functor"></param>
            /// <returns></returns>
            public static Task<T> Invoke<T>(Func<T> functor)
            {
                if (RunningInstance != null)
                    return RunningInstance.Invoke(functor);

                throw new ApplicationException();
            }

            /// <summary>
            /// 지정된 작업자 쓰레드에서 Functor를 실행하며,
            /// 그 Functor가 실행되면 완료되는 Task 객체를 반환합니다.
            /// 
            /// 현재 실행중인 인스턴스가 없을 때, 
            /// ApplicationException 예외가 발생합니다.
            /// 
            /// 지정된 이름으로 설정된 작업자가 없는 경우, 
            /// KeyNotFoundException이 발생합니다.
            /// </summary>
            /// <param name="functor"></param>
            /// <returns></returns>
            public static Task Invoke(string workerName, Action functor)
            {
                if (RunningInstance != null)
                    return RunningInstance.Invoke(workerName, functor);

                throw new ApplicationException();
            }

            /// <summary>
            /// 지정된 작업자 쓰레드에서 Functor를 실행하며,
            /// 그 Functor가 실행되면 완료되는 Task 객체를 반환합니다.
            /// 
            /// 현재 실행중인 인스턴스가 없을 때, 
            /// ApplicationException 예외가 발생합니다.
            /// 
            /// 지정된 이름으로 설정된 작업자가 없는 경우, 
            /// KeyNotFoundException이 발생합니다.
            /// </summary>
            /// <param name="functor"></param>
            /// <returns></returns>
            public static Task<T> Invoke<T>(string workerName, Func<T> functor)
            {
                if (RunningInstance != null)
                    return RunningInstance.Invoke(workerName, functor);

                throw new ApplicationException();
            }
        }

        /// <summary>
        /// 어플리케이션 메시지 루프에서 Functor를 실행하며,
        /// 그 Functor가 실행되면 완료되는 Task 객체를 반환합니다.
        /// </summary>
        /// <param name="functor"></param>
        /// <returns></returns>
        public Task Invoke(Action functor)
        {
            if (functor == null)
                throw new ArgumentNullException();

            if (!m_Context.IsContextThread)
            {
                TaskCompletionSource<Application> TCS
                    = new TaskCompletionSource<Application>();

                m_Context.Invoke(() =>
                {
                    functor();
                    TCS.SetResult(this);
                });

                return TCS.Task;
            }

            functor();
            return Task.FromResult(this);
        }

        /// <summary>
        /// 어플리케이션 메시지 루프에서 Functor를 실행하며,
        /// 그 Functor가 실행되면 완료되는 Task 객체를 반환합니다.
        /// </summary>
        /// <param name="functor"></param>
        /// <returns></returns>
        public Task<T> Invoke<T>(Func<T> functor)
        {
            if (functor == null)
                throw new ArgumentNullException();

            if (!m_Context.IsContextThread)
            {
                TaskCompletionSource<T> TCS
                    = new TaskCompletionSource<T>();

                m_Context.Invoke(
                    () => TCS.SetResult(functor()));

                return TCS.Task;
            }

            return Task.FromResult(functor());
        }

        /// <summary>
        /// 지정된 작업자 쓰레드에서 Functor를 실행하며,
        /// 그 Functor가 실행되면 완료되는 Task 객체를 반환합니다.
        /// 
        /// 지정된 이름으로 설정된 작업자가 없는 경우, 
        /// KeyNotFoundException이 발생합니다.
        /// </summary>
        /// <param name="functor"></param>
        /// <returns></returns>
        public Task Invoke(string workerName, Action functor)
        {
            Worker worker = Workers[workerName];

            if (functor == null)
                throw new ArgumentNullException();

            if (!worker.IsWorkerThread)
                return worker.Invoke(functor);

            functor();
            return Task.FromResult(this);
        }

        /// <summary>
        /// 지정된 작업자 쓰레드에서 Functor를 실행하며,
        /// 그 Functor가 실행되면 완료되는 Task 객체를 반환합니다.
        /// 
        /// 지정된 이름으로 설정된 작업자가 없는 경우, 
        /// KeyNotFoundException이 발생합니다.
        /// </summary>
        /// <param name="functor"></param>
        /// <returns></returns>
        public Task<T> Invoke<T>(string workerName, Func<T> functor)
        {
            Worker worker = Workers[workerName];

            if (functor == null)
                throw new ArgumentNullException();

            if (!worker.IsWorkerThread)
            {
                TaskCompletionSource<T> TCS
                    = new TaskCompletionSource<T>();

                worker.Invoke(
                    () => TCS.SetResult(functor()));

                return TCS.Task;
            }

            return Task.FromResult(functor());
        }
    }
}
