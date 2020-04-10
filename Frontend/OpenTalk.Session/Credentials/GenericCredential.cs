
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Credentials
{
    /// <summary>
    /// 이메일 혹은 전화번호와 패스워드의 조합으로 구성된
    /// 일반적인 자격증명 정보입니다.
    /// </summary>
    public class GenericCredential : Credential
    {
        /// <summary>
        /// 이메일 혹은 전화번호입니다.
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// 패스워드입니다.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 데이터를 서버로 전송해야 할 때 실행되는 메서드입니다.
        /// </summary>
        /// <param name="setter"></param>
        protected override void OnSet(Setter setter)
        {
            setter("identifier", Identifier);
            setter("password", Password);
        }

        /// <summary>
        /// 인증 정보를 하드디스크에 저장할 때 실행되는 메서드입니다.
        /// </summary>
        /// <param name="writer"></param>
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(Identifier != null ? Identifier : "");
            writer.Write(Password != null ? Password : "");
        }

        /// <summary>
        /// 인증 정보를 하드디스크에서 복원할 때 실행되는 메서드입니다.
        /// </summary>
        /// <param name="reader"></param>
        public override void Deserialize(BinaryReader reader)
        {
            Identifier = reader.ReadString();
            Password = reader.ReadString();
        }
    }
}
