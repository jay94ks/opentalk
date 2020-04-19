namespace OpenTalk.Tasks
{
    /// <summary>
    /// 작업 상태를 표현합니다.
    /// </summary>
    public enum FutureStatus
    {
        /// <summary>
        /// 작업이 생성되었을 뿐 아무 대기열에도 등록되지 않았습니다.
        /// </summary>
        Created,

        /// <summary>
        /// 작업이 스케쥴링되었습니다. 
        /// 곧 실행될 예정입니다.
        /// </summary>
        Scheduled,

        /// <summary>
        /// 작업이 실행되는 중입니다.
        /// </summary>
        Running,

        /// <summary>
        /// 작업이 완료되었습니다.
        /// </summary>
        Succeed,

        /// <summary>
        /// 작업이 취소되었습니다.
        /// </summary>
        Canceled,

        /// <summary>
        /// 작업을 수행하던 중 실패하였습니다.
        /// </summary>
        Faulted
    }
}
