using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DFile = System.IO.File;

namespace OpenTalk
{
    public abstract partial class Log
    {
        private class File : Log
        {
            private DateTime m_LatestTime;
            private string m_TargetFile;
            private string m_LineTerminator;

            /// <summary>
            /// 파일에 로그 메시지를 출력합니다.
            /// </summary>
            public File()
            {
                m_LatestTime = DateTime.Now;
                m_TargetFile = null;
                m_LineTerminator = "\n";

                if (Environment.OSVersion.Platform == PlatformID.Win32S ||
                    Environment.OSVersion.Platform == PlatformID.Win32NT ||
                    Environment.OSVersion.Platform == PlatformID.Win32Windows ||
                    Environment.OSVersion.Platform == PlatformID.WinCE)
                {
                    m_LineTerminator = "\r\n";
                }
            }

            /// <summary>
            /// 로그 메시지를 파일에 출력합니다.
            /// </summary>
            /// <param name="writtenTime"></param>
            /// <param name="message"></param>
            protected override void Write(DateTime writtenTime, string message)
            {
                message = string.Format("{0} {1} {2}{3}",
                    writtenTime.ToShortDateString(),
                    writtenTime.ToShortTimeString(),
                    message, m_LineTerminator);

                lock (this)
                {
                    UpdateTargetFileName(writtenTime);
                    DFile.AppendAllText(m_TargetFile, message);
                }
            }

            /// <summary>
            /// 지정된 시간에 맞게 타깃 파일 이름을 변경합니다.
            /// </summary>
            /// <param name="writtenTime"></param>
            private void UpdateTargetFileName(DateTime writtenTime)
            {
                if (m_LatestTime.Year != writtenTime.Year ||
                    m_LatestTime.Month != writtenTime.Month ||
                    m_LatestTime.Day != writtenTime.Day ||
                    m_TargetFile == null)
                {
                    int PostfixCount = 1;
                    string FileName = Path.Combine(
                        Application.Environments.LoggingPath, string.Format(
                        "{0}-{1:00}-{2:00}", writtenTime.Year, writtenTime.Month,
                        writtenTime.Day));

                    m_TargetFile = FileName + ".000.log";
                    while (DFile.Exists(m_TargetFile))
                    {
                        m_TargetFile = string.Format("{0}.{1:000}.log", FileName, PostfixCount);
                        PostfixCount++;
                    }

                    WriteLogBanner();
                    m_LatestTime = writtenTime;
                }
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
                catch {  }

                if (string.IsNullOrEmpty(Title) || string.IsNullOrWhiteSpace(Title))
                    Title = Info.Name;

                DFile.WriteAllText(m_TargetFile, Title + " - " + Info.Version.ToString() + m_LineTerminator);

                try
                {
                    string Copyright = Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
                    if (!string.IsNullOrEmpty(Copyright) && !string.IsNullOrWhiteSpace(Copyright))
                        DFile.AppendAllText(m_TargetFile, Copyright + m_LineTerminator);
                }
                catch { }

                DFile.AppendAllText(m_TargetFile, m_LineTerminator);
            }
        }
    }
}
