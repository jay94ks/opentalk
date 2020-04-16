using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Net
{
    internal class NetWorker
    {
        /// <summary>
        /// 어플리케이션 인스턴스가 준비될 때, 작업자를 준비시킵니다.
        /// </summary>
        static NetWorker()
        {
            Application.RunningInstanceReady.ContinueWith(
                (X) => {
                    var Worker = new Application.Worker(X.Result);
                    TCS.SetResult(Worker);
                    Worker.Start();
                });
        }

        /// <summary>
        /// 작업자 인스턴스를 제공하는 TCS 객체입니다.
        /// </summary>
        private static TaskCompletionSource<Application.Worker> TCS
            = new TaskCompletionSource<Application.Worker>();

        /// <summary>
        /// 작업자 인스턴스입니다.
        /// </summary>
        public static Application.Worker Worker => TCS.Task.WaitResult();

        /// <summary>
        /// 네트워크 작업자에서 작업을 실행합니다.
        /// </summary>
        /// <param name="functor"></param>
        /// <returns></returns>
        public static Task Invoke(Action functor) => Worker.Invoke(functor);
    }
}
