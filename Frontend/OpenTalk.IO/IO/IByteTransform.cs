using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.IO
{
    public interface IByteTransform
    {
        /// <summary>
        /// 입력 버퍼를 구현체가 구현하는 알고리즘에 맞춰, 변조합니다.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        byte[] Transform(byte[] buffer, int offset, int size);
    }
}
