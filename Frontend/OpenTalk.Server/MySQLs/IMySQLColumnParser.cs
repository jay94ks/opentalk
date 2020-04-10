using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Server.MySQLs
{
    public interface IMySQLColumnParser
    {
        /// <summary>
        /// MySQL 컬럼 문자열을 파싱합니다.
        /// </summary>
        /// <param name="Value"></param>
        /// <param name="PreferedType"></param>
        /// <returns></returns>
        object Parse(string Value, Type PreferedType);
    }
}
