using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenTalk.UI.CefUnity
{
    /// <summary>
    /// 컨텐트 렌더러 입니다.
    /// </summary>
    public abstract class CefContentRenderer
    {
        /// <summary>
        /// 실존하는 파일로부터 컨텐츠를 렌더링합니다.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public static CefContent FromFile(FileInfo fileInfo)
        {
            if (fileInfo.Exists)
            {
                string MimeType = "text/html";

                switch (fileInfo.Extension.ToLower().Trim('.'))
                {
                    case "js": MimeType = "text/js"; break;
                    case "css": MimeType = "text/css"; break;
                    case "htm":
                    case "html": MimeType = "text/html"; break;
                    case "json": MimeType = "application/json"; break;
                    case "png": MimeType = "image/png"; break;
                    case "jpg": MimeType = "image/jpeg"; break;
                    case "gif": MimeType = "image/gif"; break;
                    default: MimeType = "application/octet-stream"; break;
                }

                try
                {
                    return new CefContent()
                    {
                        StatusCode = 200,
                        StatusMessage = "OK",
                        MimeType = MimeType,
                        Charset = MimeType.StartsWith("text") ? Encoding.UTF8 : null,
                        Content = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read)
                    };
                }
                catch { }
            }

            return MakeError();
        }

        /// <summary>
        /// 주어진 텍스트로 컨텐트를 만듭니다.
        /// </summary>
        /// <param name="Content"></param>
        /// <param name="MimeType"></param>
        /// <returns></returns>
        public static CefContent FromText(
            string Content, string MimeType = "text/html")
        {
            return new CefContent()
            {
                StatusCode = 200,
                StatusMessage = "OK",
                MimeType = MimeType,
                Charset = Encoding.UTF8,
                Content = new MemoryStream(Encoding.UTF8.GetBytes(Content))
            };
        }

        /// <summary>
        /// 바이트 배열로부터 컨텐트를 만듭니다.
        /// </summary>
        /// <param name="Bytes"></param>
        /// <param name="MimeType"></param>
        /// <returns></returns>
        public static CefContent FromBytes(
            byte[] Bytes, string MimeType = "application/octet-stream")
        {
            return new CefContent()
            {
                StatusCode = 200,
                StatusMessage = "OK",
                MimeType = MimeType,
                Charset = Encoding.UTF8,
                Content = new MemoryStream(Bytes)
            };
        }

        /// <summary>
        /// 에러 응답 컨텐트를 만듭니다.
        /// </summary>
        /// <param name="Content"></param>
        /// <param name="MimeType"></param>
        /// <returns></returns>
        public static CefContent MakeError(
            int StatusCode = 404, string StatusMessage = "Not Found",
            string Content = "No such contents available.", 
            string MimeType = "text/plain")
        {
            return new CefContent()
            {
                StatusCode = StatusCode,
                StatusMessage = StatusMessage,
                MimeType = MimeType,
                Charset = Encoding.UTF8,
                Content = Content != null ? new MemoryStream(
                    Encoding.UTF8.GetBytes(Content)) : null
            };
        }

        /// <summary>
        /// 라우팅 되었을 때 동작을 구현합니다.
        /// 새 인스턴스로 동작하도록 구성하려면, 새 인스턴스를 반환하고,
        /// 그렇지 않고, 동일한 인스턴스로 동작하도록 구성하려면 그대로 유지하십시오.
        /// </summary>
        /// <returns></returns>
        public virtual CefContentRenderer OnRouted(CefScreen screen, string PathName)
        {
            return this;
        }

        /// <summary>
        /// 요구사항에 맞춰 페이지를 렌더링합니다.
        /// (POST, PUT 요청 전용)
        /// </summary>
        /// <param name="Method"></param>
        /// <param name="PathString"></param>
        /// <param name="PostData"></param>
        /// <param name="AllowCache"></param>
        /// <param name="OnlyFromCache"></param>
        /// <returns></returns>
        public virtual CefContent Render(string Method, string QueryString,
            Dictionary<string, byte[]> PostData, bool AllowCache, bool OnlyFromCache)
            => Render(Method, QueryString, AllowCache, OnlyFromCache);

        /// <summary>
        /// 요구사항에 맞춰 페이지를 렌더링합니다.
        /// (GET, DELETE 요청 전용)
        /// </summary>
        /// <param name="Method"></param>
        /// <param name="PathString"></param>
        /// <param name="PostData"></param>
        /// <param name="AllowCache"></param>
        /// <param name="OnlyFromCache"></param>
        /// <returns></returns>
        public abstract CefContent Render(string Method, 
            string QueryString, bool AllowCache, bool OnlyFromCache);
    }
}
