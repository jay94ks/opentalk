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

    }
}
