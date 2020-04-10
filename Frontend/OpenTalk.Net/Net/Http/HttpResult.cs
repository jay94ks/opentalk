using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Net.Http
{
    /// <summary>
    /// Http 결과 객체입니다.
    /// </summary>
    public class HttpResult
    {
        public enum ContentType
        {
            Text, Json, Xml,
            Binary
        }

        /// <summary>
        /// Http 요청 경로입니다.
        /// </summary>
        public string Path { get; set; }
        
        /// <summary>
        /// 성공 여부입니다.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 실패 원인이 네트워크 오류인지 검사합니다.
        /// </summary>
        public bool HasNetworkError => !Success && StatusCode <= 0;

        /// <summary>
        /// Http 상태 코드입니다.
        /// 이는 서버에 도달해서, 성공 혹은 실패한 경우에만 0이 아닌 값이 채워집니다.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// 사용자 정의 상태 객체입니다.
        /// Http 요청을 보낸 메서드에서 설정합니다.
        /// </summary>
        public object UserState { get; set; }

        /// <summary>
        /// Http 응답 헤더에 포함된 상태 메시지입니다.
        /// e.g. HTTP/1.1 200 OK에서 'OK'.
        /// </summary>
        public string StatusMessage { get; set; }

        /// <summary>
        /// 응답 컨텐츠의 포멧을 결정합니다.
        /// </summary>
        public ContentType ResponseType { get; set; }

        /// <summary>
        /// 텍스트 계열 응답인 경우 채워집니다.
        /// </summary>
        public string ResponseText { get; set; }

        /// <summary>
        /// 바이너리 계열 응답인 경우 채워집니다.
        /// </summary>
        public byte[] ResponseBytes { get; set; }
    }

    /// <summary>
    /// Http 결과 객체입니다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HttpResult<T> : HttpResult
    {
        /// <summary>
        /// 파싱 오류가 있었는지 여부입니다.
        /// </summary>
        public bool HasParsingError { get; set; }

        /// <summary>
        /// 파싱된 결과 객체입니다.
        /// </summary>
        public T ResponseObject { get; set; }
    }
}
