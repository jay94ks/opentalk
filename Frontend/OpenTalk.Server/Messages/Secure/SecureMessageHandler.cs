using Newtonsoft.Json;
using OpenTalk.Messages;
using OpenTalk.Server.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Server.Messages.Secure
{
    [MessageHandler(0)]
    public class SecureMessageHandler : IMessageHandler
    {
        /// <summary>
        /// 잠금 모드와 관련된 메시지들을 처리합니다.
        /// </summary>
        /// <param name="Connection"></param>
        /// <param name="Label"></param>
        /// <param name="Message"></param>
        /// <returns></returns>
        public bool HandleMessage(Connection Connection, string Label, string Message)
        {
            if (Label == "secure")
            {
                SecureComponent Secure = Connection.GetComponent<SecureComponent>();
                SecureMessage Request = null;
                string Password = null;

                if (Secure != null)
                {
                    // 메시지 본문을 파싱합니다.
                    try { Request = JsonConvert.DeserializeObject<SecureMessage>(Message); }
                    catch
                    {
                        return false;
                    }

                    // 해쉬된 패스워드를 복사하고
                    Password = Request.Password;

                    // 메시지 본문을 재활용합니다.
                    Request.IsPasswordInvalid = false;
                    Request.Password = null;

                    // 잠금 모드를 설정하려는 경우.
                    if (Request.SetLocked)
                        Secure.Lock();

                    // 잠금을 해제하려는 경우.
                    else if (!string.IsNullOrEmpty(Password) &&
                        !string.IsNullOrWhiteSpace(Password))
                        Request.IsPasswordInvalid = Secure.Unlock(Password);

                    // 해제하려 하는데 패스워드 해쉬가 비어있는 경우.
                    else Request.IsPasswordInvalid = true;

                    // 변경된 상태를 복사하고 응답을 전송합니다.
                    lock (Secure) Request.SetLocked = Secure.Locked;
                    Connection.Send(Label, JsonConvert.SerializeObject(Request));
                    return true;
                }
            }

            return false;
        }
    }
}
