using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OpenTalk.Server
{
    public class ListenerSettings
    {
        /// <summary>
        /// Textile 프로토콜 서버 설정입니다.
        /// </summary>
        public class Textile
        {
            [JsonProperty(PropertyName = "address")]
            public string Address = "0.0.0.0";

            [JsonProperty(PropertyName = "port")]
            public int PortNumber = 8000;
        }

        /// <summary>
        /// Textile 프로토콜 메인 포트 설정입니다.
        /// </summary>
        [JsonProperty(PropertyName = "primary")]
        public Textile Primary { get; set; } = new Textile();

        /// <summary>
        /// Textile 프로토콜 보조 포트 설정들입니다.
        /// (멀티 포트 설정을 하려면 이 값을 채우십시오)
        /// </summary>
        [JsonProperty(PropertyName = "secondaries")]
        public Textile[] Secondaries { get; set; } = new Textile[0];
    }
}
