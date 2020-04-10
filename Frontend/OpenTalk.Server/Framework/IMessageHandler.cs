using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Server.Framework
{
    public interface IMessageHandler
    {
        /// <summary>
        /// 주어진 레이블에 맞춰, 메시지를 처리합니다.
        /// </summary>
        /// <param name="Label"></param>
        /// <param name="Message"></param>
        /// <returns></returns>
        bool HandleMessage(Connection Connection, string Label, string Message);
    }
}
