using OpenTalk.Server.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Server.Messages
{
    [MessageHandler(int.MaxValue)]
    public class FailbackHandler : IMessageHandler
    {
        /// <summary>
        /// 무슨 메시지든 간에 여기 도달하면 연결을 끊습니다.
        /// </summary>
        /// <param name="Connection"></param>
        /// <param name="Label"></param>
        /// <param name="Message"></param>
        /// <returns></returns>
        public bool HandleMessage(Connection Connection, string Label, string Message)
        {
            Connection.Close();
            return false;
        }
    }
}
