using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Server.Framework
{
    public class MessageModuleSettings
    {
        /// <summary>
        /// 메시지 핸들러 모듈 DLL 파일들입니다.
        /// </summary>
        public string[] Modules { get; set; } = new string[0];
    }
}
