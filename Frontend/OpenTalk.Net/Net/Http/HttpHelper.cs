using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Net.Http
{
    public class HttpHelper
    {
        /// <summary>
        /// 지정된 경로 문자열에 쿼리 스트링과,
        /// 추가적으로 덧붙일 쿼리 문자열을 덧붙힙니다.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="QueryStrings"></param>
        /// <param name="AdditionalQueries"></param>
        /// <returns></returns>
        public static string CombinePath(string Path,
            string QueryStrings, params string[] AdditionalQueries)
        {
            string TargetPath = Path;

            if (QueryStrings != null &&
                !string.IsNullOrEmpty(QueryStrings) &&
                !string.IsNullOrWhiteSpace(QueryStrings))
            {
                QueryStrings = QueryStrings.Trim();

                if (TargetPath.Contains("?"))
                    TargetPath += "&" + QueryStrings;

                else TargetPath += "?" + QueryStrings;
            }

            if (TargetPath.Contains("?"))
                TargetPath += "&" + string.Join("&", AdditionalQueries);

            else TargetPath += "?" + string.Join("&", AdditionalQueries);

            return TargetPath;
        }
    }
}
