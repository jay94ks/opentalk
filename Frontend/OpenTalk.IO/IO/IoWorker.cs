using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.IO
{
    internal class IOWorker
    {
        /// <summary>
        /// 어플리케이션 인스턴스가 준비될 때, 작업자를 준비시킵니다.
        /// </summary>
        static IOWorker()
        {
            Application.RunningInstanceReady.ContinueWith(
                (X) => TCS.SetResult(new Application.Worker(X.Result)));
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
    }
}
