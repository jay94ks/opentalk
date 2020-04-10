using System;

namespace OpenTalk.Components
{
    public partial class IpcComponent
    {
        private class Messanger : MarshalByRefObject
        {
            private IpcComponent m_Component;

            /// <summary>
            /// 클라이언트용 생성자.
            /// </summary>
            public Messanger()
            {
            }

            /// <summary>
            /// 서버용 생성자.
            /// </summary>
            /// <param name="ipcComponent"></param>
            public Messanger(IpcComponent component)
            {
                m_Component = component;
            }

            /// <summary>
            /// 문자열로 된 메시지 타입과 그 데이터를 송신합니다.
            /// </summary>
            /// <param name="Message"></param>
            public void Send(params string[] message)
                => m_Component?.OnMessage(message);
        }
    }
}
