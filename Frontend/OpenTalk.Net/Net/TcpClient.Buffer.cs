using OpenTalk.IO;
using System;

namespace OpenTalk.Net
{
    public partial class TcpClient
    {
        private class Buffer : BinaryBuffer
        {
            public override void Push(byte[] buffer, int offset, int length) 
                => throw new NotSupportedException();

            public void PushInternal(byte[] buffer, int offset, int length) 
                => base.Push(buffer, offset, length);
        }

    }
}
