using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Server.Framework
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageHandlerAttribute : Attribute
    {
        public MessageHandlerAttribute(int Priority)
            => this.Priority = Priority;

        /// <summary>
        /// 이 메시지 핸들러의 우선순위입니다.
        /// </summary>
        public int Priority { get; private set; }
    }
}
