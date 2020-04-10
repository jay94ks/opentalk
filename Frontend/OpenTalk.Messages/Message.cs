using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Messages
{
    public class Message
    {
        /// <summary>
        /// 식별번호입니다.
        /// 
        /// 이 값은 요청/응답 기반 메시지들을 송/수신할 때,
        /// 서로를 식별하기 위한 값으로 사용됩니다.
        /// </summary>
        [JsonProperty("id")]
        public ulong Id { get; set; } = 0;

    }
}
