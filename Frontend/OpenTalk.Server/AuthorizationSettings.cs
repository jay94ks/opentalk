using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Server
{
    public class AuthorizationSettings
    {
        public class RequestTarget
        {
            [JsonProperty("path")]
            public string Path = "";

            [JsonProperty("auth-key")]
            public string Authorization = "";

            [JsonProperty("query-strings")]
            public string QueryStrings = "";
        }

        [JsonProperty("base-uri")]
        public string BaseUri { get; set; } = "http://127.0.0.1/otk/mgmt/";

        [JsonProperty]
        public RequestTarget Registration { get; set; } = new RequestTarget()
        {
            Path = "registration",
            Authorization = "DEBUG",
            QueryStrings = ""
        };

        /// <summary>
        /// 해당 인증 토큰에 대한 인증을 수행하는 API입니다.
        /// 인증 요청: GET, 인증 해제: DELETE.
        /// </summary>
        [JsonProperty("authorize")]
        public RequestTarget Authorization { get; set; } = new RequestTarget()
        {
            Path = "authorization",
            Authorization = "",
            QueryStrings = ""
        };

        /// <summary>
        /// 해당 인증 토큰에 대해 패스워드 인증을 수행하는 API입니다.
        /// </summary>
        [JsonProperty("verification")]
        public RequestTarget Verify { get; set; } = new RequestTarget()
        {
            Path = "verify",
            Authorization = "",
            QueryStrings = ""
        };
    }
}
