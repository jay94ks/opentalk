using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Credentials
{
    /// <summary>
    /// OpenTalk 세션에 로그인하기 위한 자격증명 정보를 추상화합니다.
    /// </summary>
    public abstract class Credential
    {
        /// <summary>
        /// 서버로 로그인 요청을 보내기 위한 데이터를 요청할 때 실행되는 딜리게이트입니다.
        /// </summary>
        /// <typeparam name="ValueType"></typeparam>
        /// <param name="Label"></param>
        /// <param name="Value"></param>
        public delegate void Setter(string Label, string Value);

        /// <summary>
        /// 데이터를 서버로 전송해야 할 때 실행되는 메서드입니다.
        /// </summary>
        /// <param name="setter"></param>
        protected abstract void OnSet(Setter setter);

        /// <summary>
        /// 데이터를 서버로 전송해야 할 때 실행되는 메서드입니다.
        /// (세션 클래스에서 OnSet 메서드를 호출하기 위해서 실행합니다)
        /// </summary>
        /// <param name="setter"></param>
        internal void Set(Setter setter) => OnSet(setter);

        /// <summary>
        /// 인증 정보를 하드디스크에 저장할 때 실행되는 메서드입니다.
        /// </summary>
        /// <param name="writer"></param>
        public virtual void Serialize(BinaryWriter writer) { }

        /// <summary>
        /// 인증 정보를 하드디스크에서 복원할 때 실행되는 메서드입니다.
        /// </summary>
        /// <param name="reader"></param>
        public virtual void Deserialize(BinaryReader reader) { }
    }
}
