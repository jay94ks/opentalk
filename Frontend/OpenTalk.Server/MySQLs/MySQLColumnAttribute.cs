using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Server.MySQLs
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class MySQLColumnAttribute : Attribute
    {
        public MySQLColumnAttribute(string Name, int Order)
        {
            this.Name = Name;
            this.Order = Order;
        }

        /// <summary>
        /// 프라이머리 키인지 여부입니다.
        /// </summary>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// 이 컬럼의 이름입니다.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 이 컬럼이 배치되는 순서입니다.
        /// </summary>
        public int Order { get; private set; }

        /// <summary>
        /// 이 컬럼을 문자열로 전환할 때 사용될 변환기입니다.
        /// </summary>
        public Type Stringifier { get; set; }

        /// <summary>
        /// 이 컬럼 문자열을 객체로 전환할 때 사용할 변환기입니다.
        /// </summary>
        public Type Parser { get; set; }
    }
}
