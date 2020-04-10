using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk
{
    public class SessionException : Exception
    {
        /// <summary>
        /// 세션 예외를 초기화합니다.
        /// </summary>
        public SessionException()
            : this(SessionError.Unknown, "")
        {
        }

        /// <summary>
        /// 세션 예외를 초기화합니다.
        /// </summary>
        public SessionException(string message)
            : this(SessionError.Unknown, message)
        {
        }

        /// <summary>
        /// 세션 예외를 초기화합니다.
        /// </summary>
        public SessionException(SessionError errorCode)
            : this(errorCode, "")
        {
        }

        /// <summary>
        /// 세션 예외를 초기화합니다.
        /// </summary>
        public SessionException(SessionError errorCode, string message)
            : base(message) => ErrorCode = errorCode;

        /// <summary>
        /// 에러 코드입니다.
        /// </summary>
        public SessionError ErrorCode { get; private set; }
    }
}
