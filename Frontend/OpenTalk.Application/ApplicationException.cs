using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk
{
    /// <summary>
    /// 어떤 객체의 동작이 현재 실행중인 어플리케이션을 대상으로할 때,
    /// 어플리케이션 객체가 준비된 상태가 아니거나,
    /// 어떤 인스턴스도 실행중이지 않을 때 발생되는 예외입니다.
    /// </summary>
    public class ApplicationException : InvalidOperationException
    {
        /// <summary>
        /// 어떤 객체의 동작이 현재 실행중인 어플리케이션을 대상으로할 때,
        /// 어플리케이션 객체가 준비된 상태가 아니거나,
        /// 어떤 인스턴스도 실행중이지 않을 때 발생되는 예외입니다.
        /// </summary>
        public ApplicationException()
            : base("Application state fault")
        {
        }

        /// <summary>
        /// 어떤 객체의 동작이 현재 실행중인 어플리케이션을 대상으로할 때,
        /// 어플리케이션 객체가 준비된 상태가 아니거나,
        /// 어떤 인스턴스도 실행중이지 않을 때 발생되는 예외입니다.
        /// </summary>
        public ApplicationException(Exception inner)
            : base("Application state fault", inner)
        {
        }

        /// <summary>
        /// 어떤 객체의 동작이 현재 실행중인 어플리케이션을 대상으로할 때,
        /// 어플리케이션 객체가 준비된 상태가 아니거나,
        /// 어떤 인스턴스도 실행중이지 않을 때 발생되는 예외입니다.
        /// </summary>
        public ApplicationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// 어떤 객체의 동작이 현재 실행중인 어플리케이션을 대상으로할 때,
        /// 어플리케이션 객체가 준비된 상태가 아니거나,
        /// 어떤 인스턴스도 실행중이지 않을 때 발생되는 예외입니다.
        /// </summary>
        public ApplicationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
