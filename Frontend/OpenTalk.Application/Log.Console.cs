using System;
using System.Reflection;
using DConsole = System.Console;

namespace OpenTalk
{
    public abstract partial class Log
    {
        internal static bool HasConsole => Console.m_HasConsole;

        /// <summary>
        /// 콘솔 창에 메시지 로그를 출력합니다.
        /// </summary>
        private class Console : Log
        {
            internal static bool m_HasConsole = false;

            /// <summary>
            /// 정적 초기화 메서드입니다.
            /// </summary>
            static Console()
            {
                m_HasConsole = true;
                
                try
                {
                    if (DConsole.WindowHeight != 0)
                        m_HasConsole = true;

                    else m_HasConsole = false;
                }

                catch { m_HasConsole = false; }

            }

            public Console()
            {
                if (m_HasConsole)
                    WriteLogBanner();
            }

            /// <summary>
            /// 로그 파일의 최상단에 배너를 찍습니다.
            /// </summary>
            private void WriteLogBanner()
            {
                Assembly Assembly = Assembly.GetEntryAssembly();
                AssemblyName Info = Assembly.GetName();
                string Title = null;

                try { Title = Assembly.GetCustomAttribute<AssemblyTitleAttribute>().Title; }
                catch { }

                if (string.IsNullOrEmpty(Title) || string.IsNullOrWhiteSpace(Title))
                    Title = Info.Name;

                DConsole.ForegroundColor = ConsoleColor.Yellow;
                DConsole.WriteLine(Title + " - " + Info.Version.ToString());

                try
                {
                    string Copyright = Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;

                    if (!string.IsNullOrEmpty(Copyright) && !string.IsNullOrWhiteSpace(Copyright))
                    {
                        DConsole.ForegroundColor = ConsoleColor.White;
                        DConsole.WriteLine(Copyright);
                    }
                }
                catch { }

                DConsole.ForegroundColor = ConsoleColor.White;
                DConsole.WriteLine();
            }

            /// <summary>
            /// 콘솔 창에 메시지 로그를 출력합니다.
            /// </summary>
            protected override void Write(DateTime writtenTime, string message)
            {
                if (m_HasConsole)
                {
                    DConsole.ForegroundColor = ConsoleColor.Cyan;
                    DConsole.Write(writtenTime.ToShortDateString() + " ");

                    DConsole.ForegroundColor = ConsoleColor.Yellow;
                    DConsole.Write(writtenTime.ToShortTimeString() + " ");

                    DConsole.ForegroundColor = ConsoleColor.White;
                    DConsole.WriteLine(message);
                }
            }
        }
    }
}
