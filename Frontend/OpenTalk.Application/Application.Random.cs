using System;
using System.Text;

namespace OpenTalk
{
    public abstract partial class Application
    {
        /// <summary>
        /// 어플리케이션 전역 난수 발생기입니다.
        /// </summary>
        public sealed class Random
        {
            private static readonly string m_RandomTexts_CS
                = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            private static readonly string m_RandomTexts_ICS
                = "abcdefghijklmnopqrstuvwxyz0123456789";

            /// <summary>
            /// 지정된 바이트 배열을 난수로 채웁니다.
            /// </summary>
            /// <param name="Bytes"></param>
            public static void MakeBytes(byte[] Bytes) => m_Randomizer.Locked((X) => X.NextBytes(Bytes));

            /// <summary>
            /// 지정된 바이트 배열의 지정된 범위를 난수로 채웁니다.
            /// </summary>
            /// <param name="Buffer"></param>
            /// <param name="Offset"></param>
            /// <param name="Length"></param>
            public static int MakeBytes(byte[] Buffer, int Offset, int Count)
            {
                int BytesLeft = Count;
                byte[] Temp = new byte[BytesLeft > 1024 ? 1024 : BytesLeft];

                while (BytesLeft > 0)
                {
                    MakeBytes(Temp);

                    Array.Copy(Temp, 0, Buffer, Offset, Temp.Length);

                    BytesLeft -= Temp.Length;
                    Offset -= Temp.Length;

                    if (BytesLeft > 0)
                        Array.Resize(ref Temp,
                            BytesLeft > 1024 ? 1024 : BytesLeft);
                }

                return Count;
            }

            /// <summary>
            /// int 범위 안에서 난수를 생성합니다.
            /// </summary>
            /// <returns></returns>
            public static int Make() => m_Randomizer.Locked((X) => X.Next());

            /// <summary>
            /// 지정된 범위 안에서 난수를 생성합니다.
            /// </summary>
            /// <param name="Starts"></param>
            /// <param name="Ends"></param>
            /// <returns></returns>
            public static int Make(int Starts, int Ends)
                => m_Randomizer.Locked((X) => X.Next(Starts, Ends));

            /// <summary>
            /// -1.0 에서 1.0 범위에 속하는 실수를 생성합니다.
            /// </summary>
            /// <returns></returns>
            public static double MakeDouble()
                => m_Randomizer.Locked((X) => X.NextDouble() * 2.0 - 1.0);

            /// <summary>
            /// 지정된 범위 안에서 실수를 생성합니다.
            /// </summary>
            /// <param name="Starts"></param>
            /// <param name="Ends"></param>
            /// <returns></returns>
            public static double MakeDouble(double Starts, double Ends)
                => m_Randomizer.Locked((X) => (Ends - Starts) * X.NextDouble() + Starts);

            /// <summary>
            /// 지정된 길이의 랜덤 문자열을 생성합니다.
            /// (알파벳과 숫자만 포함됨)
            /// </summary>
            /// <param name="Characters"></param>
            /// <param name="Length"></param>
            /// <returns></returns>
            public static string MakeString(int Length, bool CaseSensitive = true)
                => MakeString(CaseSensitive ? m_RandomTexts_CS : m_RandomTexts_ICS, Length);

            /// <summary>
            /// 지정된 길이의 랜덤 문자열을 생성합니다.
            /// </summary>
            /// <param name="Characters"></param>
            /// <param name="Length"></param>
            /// <returns></returns>
            public static string MakeString(string Characters, int Length)
            {
                return m_Randomizer.Locked((X) =>
                {
                    StringBuilder sb = new StringBuilder(Length + 1, Length + 1);

                    while (Length-- > 0)
                        sb.Append(Characters[X.Next(0, Characters.Length)]);

                    return sb.ToString();
                });
            }
            
        }
    }
}
