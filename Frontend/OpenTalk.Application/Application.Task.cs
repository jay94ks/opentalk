using OpenTalk.Tasks;
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
            /// </summary>
            /// <param name="functor"></param>
            /// <returns></returns>
            public static Future Invoke(Action functor)
                => Future.RunForUI(functor);

            /// <summary>
            /// 어플리케이션 메시지 루프에서 Functor를 실행하며,
            /// 그 Functor가 실행되면 완료되는 Task 객체를 반환합니다.
            /// </summary>
            /// <param name="functor"></param>
            /// <returns></returns>
            public static Future<T> Invoke<T>(Func<T> functor)
                => Future.RunForUI(functor);
        }

        /// <summary>
        /// 어플리케이션 메시지 루프에서 Functor를 실행하며,
        /// 그 Functor가 실행되면 완료되는 Task 객체를 반환합니다.
        /// </summary>
        /// <param name="functor"></param>
        /// <returns></returns>
        public Future Invoke(Action functor)
                => Future.RunForUI(functor);

        /// <summary>
        /// 어플리케이션 메시지 루프에서 Functor를 실행하며,
        /// 그 Functor가 실행되면 완료되는 Task 객체를 반환합니다.
        /// </summary>
        /// <param name="functor"></param>
        /// <returns></returns>
        public Future<T> Invoke<T>(Func<T> functor)
                => Future.RunForUI(functor);
    }
}
