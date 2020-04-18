using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.UI.CefUnity
{
    /// <summary>
    /// 렌더링된 컨텐트.
    /// </summary>
    public class CefContent
    {
        /// <summary>
        /// 응답의 문자 엔코딩을 획득하거나 설정합니다.
        /// </summary>
        public Encoding Charset { get; set; } = Encoding.UTF8;

        /// <summary>
        /// 응답 코드를 획득하거나 설정합니다.
        /// </summary>
        public virtual int StatusCode { get; set; } = 200;

        /// <summary>
        /// 상태 메시지를 획득하거나 설정합니다.
        /// </summary>
        public virtual string StatusMessage { get; set; } = "OK";

        /// <summary>
        /// 응답 컨텐츠의 MimeType을 획득하거나 설정합니다.
        /// </summary>
        public virtual string MimeType { get; set; } = "text/html";

        /// <summary>
        /// 응답 컨텐트 스트림을 획득하거나 설정합니다.
        /// </summary>
        public virtual Stream Content { get; set; } = null;

        /// <summary>
        /// 응답 헤더에 포함되어야 할 헤더들을 설정합니다.
        /// </summary>
        public virtual KeyValuePair<string, string>[] Headers { get; set; } = null;

        /// <summary>
        /// 응답 컨텐트 스트림을 파괴하지 말아야 할때 true를 설정합니다.
        /// </summary>
        public virtual bool NoDisposeContent { get; set; } = false;
    }
}
