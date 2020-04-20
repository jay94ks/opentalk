using OpenTalk.Tasks;
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
        /// 네트워크 작업자에서 작업을 실행합니다.
        /// </summary>
        /// <param name="functor"></param>
        /// <returns></returns>
        public static Future Invoke(Action functor)
            => Application.FutureInstance.Then(functor);
    }
}
