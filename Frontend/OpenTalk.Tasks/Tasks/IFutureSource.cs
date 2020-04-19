namespace OpenTalk.Tasks
{
    /// <summary>
    /// 사용자 코드에 의해 상태가 제어되는 작업 객체를 만듭니다.
    /// </summary>
    public interface IFutureSource
    {
        /// <summary>
        /// 작업 객체입니다.
        /// </summary>
        Future Future { get; }

        /// <summary>
        /// 사용자 정의 태그입니다.
        /// </summary>
        object Tag { get; set; }
    }
}