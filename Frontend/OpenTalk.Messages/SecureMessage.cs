using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Messages
{
    /// <summary>
    /// 잠금 모드와 관련된 메시지 객체입니다.
    /// </summary>
    public class SecureMessage : Message
    {
        /// <summary>
        /// 잠금을 설정할지 혹은 해제할지 여부입니다.
        /// 해제하려면 이 속성을 false로 설정하고, 
        /// HashedPassword 속성에 패스워드 해쉬값을 채우십시오.
        /// 
        /// 응답으로 사용되는 경우에, SetLocked가 true인 경우, 
        /// 잠금 모드가 설정된 상태임을 의미하며, 
        /// false인 경우 그렇지 않은 상태임을 의미합니다.
        /// </summary>
        [JsonProperty("set-lock")]
        public bool SetLocked { get; set; } = false;

        /// <summary>
        /// 요청이 거부된 경우 그 원인이 HashedPassword 인지 확인합니다.
        /// (응답인 경우에만 유효합니다)
        /// </summary>
        [JsonProperty("password-fault")]
        public bool IsPasswordInvalid { get; set; } = false;

        /// <summary>
        /// 해쉬 알고리즘으로, 해쉬된 패스워드입니다.
        /// 잠금을 설정하려면 이 칸을 비우고, SetLocked 속성을 true로 설정하십시오.
        /// 
        /// 응답으로 사용되는 경우에 이 필드는 사용되지 않습니다.
        /// </summary>
        [JsonProperty("password")]
        public string Password { get; set; } = null;
    }
}
