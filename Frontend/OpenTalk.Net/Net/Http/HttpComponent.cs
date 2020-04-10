using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OpenTalk.Net.Http
{
    /// <summary>
    /// Http 컴포넌트입니다.
    /// </summary>
    public partial class HttpComponent : Application.Component
    {
        private HttpClient m_HttpClient;
        private string m_Authorization;

        /// <summary>
        /// Http 컴포넌트입니다.
        /// </summary>
        public HttpComponent(string BaseUri)
            : this(null, BaseUri)
        {
        }

        /// <summary>
        /// Http 컴포넌트입니다.
        /// </summary>
        public HttpComponent(Uri BaseUri)
            : this(null, BaseUri)
        {
        }

        /// <summary>
        /// Http 컴포넌트입니다.
        /// </summary>
        public HttpComponent(Application application, string BaseUri)
            : this(application, new Uri(BaseUri))
        {
        }

        /// <summary>
        /// Http 컴포넌트입니다.
        /// </summary>
        public HttpComponent(Application application, Uri BaseUri)
            : base(application)
        {
            Version OTNetVersion = typeof(HttpComponent).Assembly.GetName().Version;
            string OSType = GenerateOSTypeString();

            m_HttpClient = new HttpClient();
            if (BaseUri != null)
            {
                m_HttpClient.BaseAddress = BaseUri;
            }

            try
            {
                m_HttpClient.DefaultRequestHeaders.Add("User-Agent",
                    string.Format("OpenTalk/{0}.{1}.{2}; OS={3}", OTNetVersion.Major,
                    OTNetVersion.Minor, OTNetVersion.Revision, OSType));
            }
            catch { }

            m_Authorization = null;
        }

        /// <summary>
        /// 현재 실행중인 어플리케이션 객체에서 지정된 BaseUri 문자열을 이용하여, HttpComponent를 획득합니다.
        /// noCreate가 true가 아닐때, 어플리케이션 객체에 알맞는 인스턴스가 없을 땐 새로 생성합니다.
        /// </summary>
        /// <param name="BaseUri"></param>
        /// <param name="noCreate"></param>
        /// <returns></returns>
        public static HttpComponent GetHttpComponent(string BaseUri, bool noCreate = false)
            => GetHttpComponent(null, BaseUri, noCreate);

        /// <summary>
        /// 현재 실행중인 어플리케이션 객체에서 지정된 BaseUri 문자열을 이용하여, HttpComponent를 획득합니다.
        /// noCreate가 true가 아닐때, 어플리케이션 객체에 알맞는 인스턴스가 없을 땐 새로 생성합니다.
        /// </summary>
        /// <param name="BaseUri"></param>
        /// <param name="noCreate"></param>
        /// <returns></returns>
        public static HttpComponent GetHttpComponent(Uri BaseUri, bool noCreate = false)
            => GetHttpComponent(null, BaseUri, noCreate);

        /// <summary>
        /// 어플리케이션 객체에서 지정된 BaseUri 문자열을 이용하여, HttpComponent를 획득합니다.
        /// noCreate가 true가 아닐때, 어플리케이션 객체에 알맞는 인스턴스가 없을 땐 새로 생성합니다.
        /// </summary>
        /// <param name="application"></param>
        /// <param name="BaseUri"></param>
        /// <returns></returns>
        public static HttpComponent GetHttpComponent(Application application, string BaseUri, bool noCreate = false)
            => GetHttpComponent(application, new Uri(BaseUri), noCreate);

        /// <summary>
        /// 어플리케이션 객체에서 지정된 BaseUri 문자열을 이용하여, HttpComponent를 획득합니다.
        /// noCreate가 true가 아닐때, 어플리케이션 객체에 알맞는 인스턴스가 없을 땐 새로 생성합니다.
        /// </summary>
        /// <param name="application"></param>
        /// <param name="BaseUri"></param>
        /// <returns></returns>
        public static HttpComponent GetHttpComponent(Application application, Uri BaseUri, bool noCreate = false)
        {
            HttpComponent outComponent = null;

            application = application != null ? 
                application : Application.RunningInstance;

            if (application == null)
                throw new ApplicationException();

            lock (application)
            {
                Application.Component component = application.GetComponent(typeof(HttpComponent),
                    (X) => {
                        HttpComponent current = X as HttpComponent;
                        return current != null && current.BaseUri != null &&
                            (current.BaseUri.Equals(BaseUri) ||
                             current.BaseUri.IsBaseOf(BaseUri));
                    });

                if (component != null)
                    outComponent = component as HttpComponent;

                else if (!noCreate)
                    (outComponent = new HttpComponent(application, BaseUri)).Activate();
            }

            return outComponent;
        }

        /// <summary>
        /// 이 Http 컴포넌트가 취급하는 모든 요청들의 기준 URI를 지정합니다.
        /// </summary>
        public Uri BaseUri {
            get => m_HttpClient.BaseAddress;
            set => m_HttpClient.BaseAddress = value;
        }

        /// <summary>
        /// Http 인증 정보입니다.
        /// </summary>
        public string Authorization {
            get {
                lock (this)
                    return m_Authorization;
            }

            set {
                lock (this)
                {
                    if (m_Authorization != value)
                    {
                        if (string.IsNullOrEmpty(value) ||
                            string.IsNullOrWhiteSpace(value))
                        {
                            m_HttpClient.DefaultRequestHeaders.Authorization = null;
                        }

                        else
                        {
                            m_HttpClient.DefaultRequestHeaders.Authorization
                                = new AuthenticationHeaderValue("bearer", value);
                        }

                        m_Authorization = value;
                    }
                }
            }
        }

        /// <summary>
        /// 운영체제 종류 문자열을 생성합니다.
        /// </summary>
        /// <returns></returns>
        private static string GenerateOSTypeString()
        {
            string OSType = null;

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.MacOSX: OSType = "MacOSX"; break;
                case PlatformID.Unix: OSType = "Unix"; break;
                case PlatformID.Win32NT: OSType = "Win32NT"; break;
                case PlatformID.Win32S: OSType = "Win32S"; break;
                case PlatformID.Win32Windows: OSType = "Win32W"; break;
                case PlatformID.WinCE: OSType = "WinCE"; break;
                default: OSType = "Unknown"; break;
            }

            return OSType;
        }

        /// <summary>
        /// Http GET 요청을 보냅니다.
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        public Task<HttpResult> Get(string Path, object UserState = null)
        {
            return InterpreteHttpResponse(
                Perform((X) => X.GetAsync(Path)), new HttpResult()
                { Path = Path, UserState = UserState });
        }

        /// <summary>
        /// Http GET 요청을 보냅니다.
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        public Task<HttpResult<T>> Get<T>(string Path, object UserState = null)
        {
            return InterpreteHttpResponse(
                Perform((X) => X.GetAsync(Path)), new HttpResult<T>()
                { Path = Path, UserState = UserState });
        }

        /// <summary>
        /// Http POST 요청을 보냅니다.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="TextContents"></param>
        /// <param name="UserState"></param>
        /// <param name="ContentType"></param>
        /// <returns></returns>
        public Task<HttpResult> Post(string Path, string TextContents, object UserState = null, string ContentType = "text/plain")
        {
            return InterpreteHttpResponse(
                Perform((X) => X.PostAsync(Path, new StringContent(TextContents, Encoding.UTF8, ContentType))),
                new HttpResult() { Path = Path, UserState = UserState });
        }

        /// <summary>
        /// Http POST 요청을 보냅니다.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="TextContents"></param>
        /// <param name="UserState"></param>
        /// <param name="ContentType"></param>
        /// <returns></returns>
        public Task<HttpResult<T>> Post<T>(string Path, string TextContents, object UserState = null, string ContentType = "text/plain")
        {
            return InterpreteHttpResponse(
                Perform((X) => X.PostAsync(Path, new StringContent(TextContents, Encoding.UTF8, ContentType))),
                new HttpResult<T>() { Path = Path, UserState = UserState });
        }

        /// <summary>
        /// Http POST 요청을 보냅니다.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="TextContents"></param>
        /// <param name="UserState"></param>
        /// <param name="ContentType"></param>
        /// <returns></returns>
        public Task<HttpResult> Post(string Path, byte[] BinaryContents, object UserState = null, string ContentType = null)
        {
            ByteArrayContent Contents = new ByteArrayContent(BinaryContents, 0, BinaryContents.Length);

            if (ContentType != null)
                Contents.Headers.ContentType = new MediaTypeHeaderValue(ContentType);

            return InterpreteHttpResponse(
                Perform((X) => X.PostAsync(Path, Contents)),
                new HttpResult() { Path = Path, UserState = UserState });
        }

        /// <summary>
        /// Http POST 요청을 보냅니다.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="TextContents"></param>
        /// <param name="UserState"></param>
        /// <param name="ContentType"></param>
        /// <returns></returns>
        public Task<HttpResult<T>> Post<T>(string Path, byte[] BinaryContents, object UserState = null, string ContentType = null)
        {
            ByteArrayContent Contents = new ByteArrayContent(BinaryContents, 0, BinaryContents.Length);

            if (ContentType != null)
                Contents.Headers.ContentType = new MediaTypeHeaderValue(ContentType);

            return InterpreteHttpResponse(
                Perform((X) => X.PostAsync(Path, Contents)),
                new HttpResult<T>() { Path = Path, UserState = UserState });
        }

        /// <summary>
        /// Http PUT 요청을 보냅니다.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="TextContents"></param>
        /// <param name="UserState"></param>
        /// <param name="ContentType"></param>
        /// <returns></returns>
        public Task<HttpResult> Put(string Path, string TextContents, object UserState = null, string ContentType = "text/plain")
        {
            return InterpreteHttpResponse(
                Perform((X) => X.PutAsync(Path, new StringContent(TextContents, Encoding.UTF8, ContentType))),
                new HttpResult() { Path = Path, UserState = UserState });
        }

        /// <summary>
        /// Http PUT 요청을 보냅니다.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="TextContents"></param>
        /// <param name="UserState"></param>
        /// <param name="ContentType"></param>
        /// <returns></returns>
        public Task<HttpResult<T>> Put<T>(string Path, string TextContents, object UserState = null, string ContentType = "text/plain")
        {
            return InterpreteHttpResponse(
                Perform((X) => X.PutAsync(Path, new StringContent(TextContents, Encoding.UTF8, ContentType))),
                new HttpResult<T>() { Path = Path, UserState = UserState });
        }

        /// <summary>
        /// Http PUT 요청을 보냅니다.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="TextContents"></param>
        /// <param name="UserState"></param>
        /// <param name="ContentType"></param>
        /// <returns></returns>
        public Task<HttpResult> Put(string Path, byte[] BinaryContents, object UserState = null, string ContentType = null)
        {
            ByteArrayContent Contents = new ByteArrayContent(BinaryContents, 0, BinaryContents.Length);

            if (ContentType != null)
                Contents.Headers.ContentType = new MediaTypeHeaderValue(ContentType);

            return InterpreteHttpResponse(
                Perform((X) => X.PutAsync(Path, Contents)),
                new HttpResult() { Path = Path, UserState = UserState });
        }

        /// <summary>
        /// Http PUT 요청을 보냅니다.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="TextContents"></param>
        /// <param name="UserState"></param>
        /// <param name="ContentType"></param>
        /// <returns></returns>
        public Task<HttpResult<T>> Put<T>(string Path, byte[] BinaryContents, object UserState = null, string ContentType = null)
        {
            ByteArrayContent Contents = new ByteArrayContent(BinaryContents, 0, BinaryContents.Length);

            if (ContentType != null)
                Contents.Headers.ContentType = new MediaTypeHeaderValue(ContentType);

            return InterpreteHttpResponse(
                Perform((X) => X.PutAsync(Path, Contents)),
                new HttpResult<T>() { Path = Path, UserState = UserState });
        }

        /// <summary>
        /// Http GET 요청을 보냅니다.
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        public Task<HttpResult> Delete(string Path, object UserState = null)
        {
            return InterpreteHttpResponse(
                Perform((X) => X.DeleteAsync(Path)), new HttpResult()
                { Path = Path, UserState = UserState });
        }

        /// <summary>
        /// Http GET 요청을 보냅니다.
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        public Task<HttpResult<T>> Delete<T>(string Path, object UserState = null)
        {
            return InterpreteHttpResponse(
                Perform((X) => X.DeleteAsync(Path)), new HttpResult<T>()
                { Path = Path, UserState = UserState });
        }

        /// <summary>
        /// HttpResponseMessage 객체를 해석하여, HttpResult를 채웁니다.
        /// </summary>
        /// <param name="Result"></param>
        /// <param name="Message"></param>
        /// <returns></returns>
        private static Task<HttpResult> InterpreteHttpResponse(
            Task<HttpResponseMessage> Message, HttpResult Result)
        {
            return Message.ContinueWith((X) =>
            {
                InterpreteHttpResultHeaders(X, Result);
                return Result;
            });
        }

        /// <summary>
        /// HttpResponseMessage 객체를 해석하여, HttpResult를 채웁니다.
        /// </summary>
        /// <param name="Result"></param>
        /// <param name="Message"></param>
        /// <returns></returns>
        private Task<HttpResult<T>> InterpreteHttpResponse<T>(
            Task<HttpResponseMessage> Message, HttpResult<T> Result)
        {
            return Message.ContinueWith((X) =>
            {
                InterpreteHttpResultHeaders(X, Result);
                Result.HasParsingError = true;

                if (!Result.HasNetworkError)
                {
                    if (Result.ResponseText != null)
                        Result.HasParsingError = !ParseHttpResponseAsText(Result);

                    else if (Result.ResponseBytes != null)
                        Result.HasParsingError = !ParseHttpResponseAsBinary(Result);
                }

                return Result;
            });
        }

        /// <summary>
        /// Bytes 응답 필드를 이용, 응답을 파싱, 객체를 생성하여 HttpResult를 채웁니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Result"></param>
        /// <returns></returns>
        private static bool ParseHttpResponseAsBinary<T>(HttpResult<T> Result)
        {
            return false;
        }

        /// <summary>
        /// Text 응답 필드를 이용, 응답을 파싱, 객체를 생성하여 HttpResult를 채웁니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Result"></param>
        /// <returns></returns>
        private static bool ParseHttpResponseAsText<T>(HttpResult<T> Result)
        {
            try
            {
                switch (Result.ResponseType)
                {
                    case HttpResult.ContentType.Json:
                        Result.ResponseObject = JsonConvert.DeserializeObject<T>(Result.ResponseText);
                        break;

                    case HttpResult.ContentType.Xml:
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(T));
                            using (TextReader reader = new StringReader(Result.ResponseText))
                            {
                                Result.ResponseObject = (T)serializer.Deserialize(reader);
                            }
                        }
                        break;

                    default:
                        Result.HasParsingError = true;
                        return false;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// HttpResponseMessage 객체를 해석하여, HttpResult를 채웁니다.
        /// </summary>
        /// <param name="CompletedTask"></param>
        /// <param name="Result"></param>
        private static void InterpreteHttpResultHeaders(Task<HttpResponseMessage> CompletedTask, HttpResult Result)
        {
            if (CompletedTask.IsFaulted || CompletedTask.IsCanceled)
            {
                Result.Success = false;
                Result.StatusCode = 0;
            }
            else
            {
                HttpResponseMessage message = CompletedTask.WaitResult();

                Result.Success = message.IsSuccessStatusCode;
                Result.StatusCode = (int)message.StatusCode;
                Result.StatusMessage = message.ReasonPhrase;

                Result.ResponseText = null;
                Result.ResponseBytes = null;

                if (message.Content != null)
                {
                    string ContentType = message.Content.Headers.ContentType.MediaType;
                    ContentType = ContentType != null ? ContentType.ToLower() : "";

                    if (!string.IsNullOrEmpty(ContentType) &&
                        !string.IsNullOrWhiteSpace(ContentType))
                    {
                        string[] ContentTypes = ContentType.Split(new char[] { '/' }, 2);

                        if (ContentTypes.Length > 1 &&
                            (ContentTypes[1] == "xml" || ContentTypes[1] == "json"))
                        {
                            ContentType = ContentTypes[1];
                        }

                        else ContentType = ContentTypes[0];
                    }

                    switch (ContentType)
                    {
                        case "text":
                            Result.ResponseType = HttpResult.ContentType.Text;
                            Result.ResponseText = message.Content.ReadAsStringAsync().WaitResult();
                            break;

                        case "json":
                            Result.ResponseType = HttpResult.ContentType.Json;
                            Result.ResponseText = message.Content.ReadAsStringAsync().WaitResult();
                            break;

                        case "xml":
                            Result.ResponseType = HttpResult.ContentType.Xml;
                            Result.ResponseText = message.Content.ReadAsStringAsync().WaitResult();
                            break;

                        default:
                            Result.ResponseType = HttpResult.ContentType.Binary;
                            Result.ResponseBytes = message.Content.ReadAsByteArrayAsync().WaitResult();
                            break;
                    }
                }

                else Result.ResponseText = "";
            }
        }

        /// <summary>
        /// Http 요청을 수행합니다.
        /// </summary>
        /// <param name="Functor"></param>
        /// <returns></returns>
        private Task<HttpResponseMessage> Perform(Func<HttpClient, Task<HttpResponseMessage>> Functor)
        {
            lock(this)
            {
                return Functor(m_HttpClient);
            }
        }
    }
}
