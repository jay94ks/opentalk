using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Server.Framework
{
    [MessageHandler(1)]
    public class TestMessageHandler : IMessageHandler
    {
        public bool HandleMessage(Connection Connection, string Label, string Message)
        {
            return false;
        }
    }
}
