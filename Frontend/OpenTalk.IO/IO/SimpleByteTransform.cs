namespace OpenTalk.IO
{
    /// <summary>
    /// 단순 바이트 변조 알고리즘을 제공합니다.
    /// </summary>
    public class SimpleByteTransform : IByteTransform
    {
        private byte[] m_TransformKey = null;

        public SimpleByteTransform(byte[] TransformKey)
        {
            m_TransformKey = TransformKey;
        }

        public byte[] Transform(byte[] buffer, int offset, int size)
        {
            byte[] OutBytes = new byte[size];

            for(int i = 0; i < size; i++)
            {
                byte Transbyte = m_TransformKey[i % m_TransformKey.Length];
                OutBytes[i] = (byte)(buffer[i + offset] ^ Transbyte);
            }

            return OutBytes;
        }
    }
}
