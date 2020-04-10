using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Server
{
    /// <summary>
    /// 서버 상태 모니터링용 포트입니다.
    /// </summary>
    public class MonitorSettings
    {
        public class Monitoring
        {
            [JsonProperty(PropertyName = "enabled")]
            public bool Enabled = true;

            [JsonProperty(PropertyName = "address")]
            public string Address = "0.0.0.0";

            [JsonProperty(PropertyName = "port")]
            public int PortNumber = 8800;
        }

        public class ConditionalReport
        {
            [JsonProperty(PropertyName = "enabled")]
            public bool Enabled = false;

            [JsonProperty(PropertyName = "base-uri")]
            public string BaseUri = "https://127.0.0.1/otk/mgmt";

            [JsonProperty(PropertyName = "path")]
            public string Path = "report";

            [JsonProperty(PropertyName = "query-string")]
            public string QueryString = "";

            [JsonProperty(PropertyName = "authorization")]
            public string Authorization = "server-key1";
        }

        public class PeriodicReport : ConditionalReport
        {
            [JsonProperty(PropertyName = "interval")]
            public int Interval = 1000 * 60 * 10;
        }

        /// <summary>
        /// 네이티브 포트입니다.
        /// </summary>
        [JsonProperty(PropertyName = "native")]
        public Monitoring Native { get; set; } = new Monitoring();

        /// <summary>
        /// 주기적으로 상태보고를 합니다.
        /// </summary>
        [JsonProperty(PropertyName = "periodic")]
        public PeriodicReport Report { get; set; } = new PeriodicReport();

        /// <summary>
        /// 서버가 시작될 때 상태보고를 합니다.
        /// </summary>
        [JsonProperty(PropertyName = "startup")]
        public ConditionalReport StartupReport { get; set; } = new ConditionalReport();

        /// <summary>
        /// 서버가 종료될 때 상태보고를 합니다.
        /// </summary>
        [JsonProperty(PropertyName = "shutdown")]
        public ConditionalReport ShutdownReport { get; set; } = new ConditionalReport();
    }
}
