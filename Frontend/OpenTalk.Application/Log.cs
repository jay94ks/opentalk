using OpenTalk.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenTalk
{
    public abstract partial class Log
    {
        private static List<Log> m_Loggers = new List<Log>();
        private static Queue<KeyValuePair<DateTime, string>> m_WritePendings
            = new Queue<KeyValuePair<DateTime, string>>();

        private static Future m_TrickyWorker = null;
        private static bool m_AlwaysDirectOut = false;

        static Log()
        {
            // 기본 로깅 타겟을 등록합니다.
            lock (m_Loggers)
            {
                m_Loggers.Add(new Console());
                m_Loggers.Add(new File());
            }
        }

        /// <summary>
        /// 작업자 쓰레드가 매번 실행하게될 메서드입니다.
        /// </summary>
        private static void OnEachLoop()
        {
            DateTime writtenAt = DateTime.Now;
            string message = "";

            while (true)
            {
                lock (m_WritePendings)
                {
                    if (m_WritePendings.Count <= 0)
                    {
                        m_TrickyWorker = null;
                        break;
                    }

                    var Item = m_WritePendings.Dequeue();

                    writtenAt = Item.Key;
                    message = Item.Value;
                }

                lock (m_Loggers)
                {
                    foreach (Log log in m_Loggers)
                    {
                        try { log.Write(writtenAt, message); }
                        catch { }
                    }
                }

                Thread.Yield();
            }
        }

        /// <summary>
        /// 로그 메시지를 작성합니다.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public static void w(string format, params object[] args)
        {
            string message = args.Length <= 0 ?
                format : string.Format(format, args);

            if (!m_AlwaysDirectOut)
            {
                lock (m_WritePendings)
                {
                    m_WritePendings.Enqueue(
                        new KeyValuePair<DateTime, string>(
                            DateTime.Now, message));

                    if (m_TrickyWorker == null ||
                        m_TrickyWorker.IsCompleted)
                        m_TrickyWorker = Future.Run(OnEachLoop);
                }
            }

            else
            {
                DateTime now = DateTime.Now;
                lock (m_Loggers)
                {
                    foreach (Log log in m_Loggers)
                    {
                        try { log.Write(now, message); }
                        catch { }
                    }
                }
            }
        }

        /// <summary>
        /// 로깅 타겟을 등록합니다.
        /// </summary>
        /// <param name="logger"></param>
        public static void Add(Log logger)
        {
            lock (m_Loggers)
            {
                if (m_Loggers.Contains(logger))
                    return;

                m_Loggers.Add(logger);
            }
        }

        /// <summary>
        /// 로깅 타겟을 제거합니다.
        /// </summary>
        /// <param name="logger"></param>
        public static void Remove(Log logger)
        {
            lock (m_Loggers)
            {
                if (!m_Loggers.Contains(logger))
                    return;

                m_Loggers.Remove(logger);
            }
        }

        /// <summary>
        /// 로그 메시지를 기록합니다.
        /// </summary>
        /// <param name="message"></param>
        protected abstract void Write(DateTime writtenTime, string message);
    }
}
