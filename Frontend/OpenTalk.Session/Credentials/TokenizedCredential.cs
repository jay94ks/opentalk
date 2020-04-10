using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Credentials
{
    /// <summary>
    /// 자동 로그인시에 사용되는 간접 자격증명 정보입니다.
    /// </summary>
    public class TokenizedCredential : Credential
    {
        /// <summary>
        /// 이메일 혹은 전화번호입니다.
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// 인증 토큰입니다.
        /// 마지막으로 로그인 했던 세션의 인증 토큰입니다.
        /// </summary>
        public string AuthenticationToken { get; set; }

        /// <summary>
        /// 복원 토큰입니다.
        /// 마지막으로 로그인 했던 시점에 서버에서 발급합니다.
        /// </summary>
        public string RestorationToken { get; set; }

        /// <summary>
        /// 데이터를 서버로 전송해야 할 때 실행되는 메서드입니다.
        /// </summary>
        /// <param name="setter"></param>
        protected override void OnSet(Setter setter)
        {
            setter("identifier", Identifier);
            setter("authentication", AuthenticationToken);
            setter("restoration", RestorationToken);
        }

        /// <summary>
        /// 인증 정보를 하드디스크에 저장할 때 실행되는 메서드입니다.
        /// </summary>
        /// <param name="writer"></param>
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(Identifier != null ? Identifier : "");
            writer.Write(AuthenticationToken != null ? AuthenticationToken : "");
            writer.Write(RestorationToken != null ? RestorationToken : "");
        }

        /// <summary>
        /// 인증 정보를 하드디스크에서 복원할 때 실행되는 메서드입니다.
        /// </summary>
        /// <param name="reader"></param>
        public override void Deserialize(BinaryReader reader)
        {
            Identifier = reader.ReadString();
            AuthenticationToken = reader.ReadString();
            RestorationToken = reader.ReadString();
        }
    }
}
