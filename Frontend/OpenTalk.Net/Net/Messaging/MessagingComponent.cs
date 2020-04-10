using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Net.Messaging
{
    public class MessagingComponent : Application.Component
    {
        private Uri m_SocketServer;

        /// <summary>
        /// 메시징 컴포넌트입니다.
        /// </summary>
        /// <param name="application"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public MessagingComponent(Application application, Uri socketServer)
        {
            m_SocketServer = socketServer;
            
        }

        protected override void OnActivated()
        {
        }

        protected override void OnDeactivated()
        {
        }

        private void OnSocketConnected()
        {
        }

        private void OnSocketReconnecting()
        {
        }

        private void OnSocketDisconnected()
        {
        }
    }
}
