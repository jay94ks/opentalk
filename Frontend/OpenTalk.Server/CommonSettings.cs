using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Server
{
    public class CommonSettings
    {
        /// <summary>
        /// 작업자 객체 인스턴스의 수를 결정합니다.
        /// </summary>
        [JsonProperty("worker-instances")]
        public int WorkerInstances { get; set; } = 8;
    }
}
