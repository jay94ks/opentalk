namespace OpenTalk.Server.MySQLs
{
    public interface IMySQLColumnStringifier
    {
        /// <summary>
        /// 지정된 객체를 MySQL 컬럼 문자열 표현으로 전환합니다.
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        string Stringify(object Value);
    }
}
