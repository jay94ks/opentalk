using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Tasks
{
    /// <summary>
    /// 논리상 완료가 불가능한 작업인 경우,
    /// 그 원인을 설명합니다.
    /// </summary>
    public enum FutureImpossibleReason
    {
        Unknown,

        /// <summary>
        /// 이미 완료되었으므로 해당 동작은 유효하지 않습니다.
        /// </summary>
        Completed,

        /// <summary>
        /// 오류로 중단된 작업을 대상으로 해당 동작은 유효하지 않습니다.
        /// </summary>
        Faulted,

        /// <summary>
        /// 취소된 작업을 대상으로 해당 동작은 유효하지 않습니다.
        /// </summary>
        Canceled
    }

    /// <summary>
    /// 논리상 완료가 불가능한 작업이 인자로 주어지거나,
    /// 완료가 불가능한 작업을 대상으로 대기하는 등의 작업을 수행하려하면
    /// 발생하는 예외입니다.
    /// </summary>
    public class FutureImpossibleException : Exception
    {
        public FutureImpossibleException(
            FutureImpossibleReason Reason = FutureImpossibleReason.Unknown)
            => this.Reason = Reason;

        /// <summary>
        /// 이 예외가 발생한 원인을 추적합니다.
        /// </summary>
        public FutureImpossibleReason Reason { get; private set; }
    }
}
