using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OpenTalk.Net.Http
{
    public partial class HttpComponent
    {

        /// <summary>
        /// Http POST 요청을 보냅니다.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="TextContents"></param>
        /// <param name="UserState"></param>
        /// <param name="ContentType"></param>
        /// <returns></returns>
        public Task<HttpResult> PostJson(string Path, object Content, object UserState = null, string ContentType = "application/json")
        {
            return Post(Path, JsonConvert.SerializeObject(Content), UserState, ContentType);
        }

        /// <summary>
        /// Http POST 요청을 보냅니다.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="TextContents"></param>
        /// <param name="UserState"></param>
        /// <param name="ContentType"></param>
        /// <returns></returns>
        public Task<HttpResult<T>> PostJson<T>(string Path, object Content, object UserState = null, string ContentType = "application/json")
        {
            return Post<T>(Path, JsonConvert.SerializeObject(Content), UserState, ContentType);
        }

        /// <summary>
        /// Http PUT 요청을 보냅니다.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="TextContents"></param>
        /// <param name="UserState"></param>
        /// <param name="ContentType"></param>
        /// <returns></returns>
        public Task<HttpResult> PutJson(string Path, object Content, object UserState = null, string ContentType = "application/json")
        {
            return Put(Path, JsonConvert.SerializeObject(Content), UserState, ContentType);
        }

        /// <summary>
        /// Http PUT 요청을 보냅니다.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="TextContents"></param>
        /// <param name="UserState"></param>
        /// <param name="ContentType"></param>
        /// <returns></returns>
        public Task<HttpResult<T>> PutJson<T>(string Path, object Content, object UserState = null, string ContentType = "application/json")
        {
            return Put<T>(Path, JsonConvert.SerializeObject(Content), UserState, ContentType);
        }

        /// <summary>
        /// Http POST 요청을 보냅니다.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="TextContents"></param>
        /// <param name="UserState"></param>
        /// <param name="ContentType"></param>
        /// <returns></returns>
        public Task<HttpResult> PostXml(string Path, object Content, object UserState = null, string ContentType = "text/plain")
        {
            XmlSerializer serializer = new XmlSerializer(Content.GetType());
            using (MemoryStream memStream = new MemoryStream())
            {
                TextWriter textWriter = new StreamWriter(memStream, Encoding.UTF8);

                serializer.Serialize(textWriter, Content);
                textWriter.Flush();

                return Post(Path, Encoding.UTF8.GetString(memStream.ToArray()), UserState, ContentType);
            }
        }

        /// <summary>
        /// Http POST 요청을 보냅니다.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="TextContents"></param>
        /// <param name="UserState"></param>
        /// <param name="ContentType"></param>
        /// <returns></returns>
        public Task<HttpResult<T>> PostXml<T>(string Path, object Content, object UserState = null, string ContentType = "text/plain")
        {
            XmlSerializer serializer = new XmlSerializer(Content.GetType());
            using (MemoryStream memStream = new MemoryStream())
            {
                TextWriter textWriter = new StreamWriter(memStream, Encoding.UTF8);

                serializer.Serialize(textWriter, Content);
                textWriter.Flush();

                return Post<T>(Path, Encoding.UTF8.GetString(memStream.ToArray()), UserState, ContentType);
            }
        }

        /// <summary>
        /// Http PUT 요청을 보냅니다.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="TextContents"></param>
        /// <param name="UserState"></param>
        /// <param name="ContentType"></param>
        /// <returns></returns>
        public Task<HttpResult> PutXml(string Path, object Content, object UserState = null, string ContentType = "text/plain")
        {
            XmlSerializer serializer = new XmlSerializer(Content.GetType());
            using (MemoryStream memStream = new MemoryStream())
            {
                TextWriter textWriter = new StreamWriter(memStream, Encoding.UTF8);

                serializer.Serialize(textWriter, Content);
                textWriter.Flush();

                return Put(Path, Encoding.UTF8.GetString(memStream.ToArray()), UserState, ContentType);
            }
        }

        /// <summary>
        /// Http PUT 요청을 보냅니다.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="TextContents"></param>
        /// <param name="UserState"></param>
        /// <param name="ContentType"></param>
        /// <returns></returns>
        public Task<HttpResult<T>> PutXml<T>(string Path, object Content, object UserState = null, string ContentType = "text/plain")
        {
            XmlSerializer serializer = new XmlSerializer(Content.GetType());
            using (MemoryStream memStream = new MemoryStream())
            {
                TextWriter textWriter = new StreamWriter(memStream, Encoding.UTF8);

                serializer.Serialize(textWriter, Content);
                textWriter.Flush();

                return Put<T>(Path, Encoding.UTF8.GetString(memStream.ToArray()), UserState, ContentType);
            }
        }
    }
}
