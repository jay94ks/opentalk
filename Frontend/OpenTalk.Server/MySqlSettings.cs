using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Server
{
    public class MySqlSettings
    {
        public class Config
        {
            [JsonProperty("host")]
            public string Host { get; set; } = "127.0.0.1";

            [JsonProperty("port")]
            public int Port { get; set; } = 3306;

            [JsonProperty("user")]
            public string User { get; set; } = "root";

            [JsonProperty("password")]
            public string Password { get; set; } = "";

            [JsonProperty("scheme")]
            public string Scheme { get; set; } = "opentalk";
        }

        /// <summary>
        /// 데이터베이스 마스터 서버입니다.
        /// </summary>
        [JsonProperty("master")]
        public Config Master { get; set; } = new Config();

        /// <summary>
        ///  데이터베이스 마스터와의 연결 갯수입니다.
        /// </summary>
        [JsonProperty("master-instances")]
        public int MasterInstances { get; set; } = 3;

        /// <summary>
        /// 데이터베이스 Read-only 슬레이브 서버들입니다.
        /// </summary>
        [JsonProperty("slaves")]
        public Config[] Slaves { get; set; } = new Config[0];
    }
}
