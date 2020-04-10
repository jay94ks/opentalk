using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Net.Messaging.Internals
{
    public class TextileSocket
    {
        private Dictionary<string, Action<string>> m_Handlers = null;

        /// <summary>
        /// 텍스틸 소켓을 초기화합니다.
        /// </summary>
        public TextileSocket()
        {
            m_Handlers = new Dictionary<string, Action<string>>();
        }
    }
}
