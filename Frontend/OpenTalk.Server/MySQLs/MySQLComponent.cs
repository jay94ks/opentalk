using MySql.Data.MySqlClient;
using OpenTalk.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenTalk.Server.MySQLs
{
    public class MySQLComponent : Application.Component
    {
        private Program m_Server;

        private MySqlConnection[] m_Masters;
        private MySqlConnection[] m_Slaves;

        public enum State
        {
            None,
            Owned,
            Lost
        }

        private State[] m_MasterStates;
        private State[] m_SlaveStates;

        private AutoResetEvent m_MasterEvent;
        private AutoResetEvent m_SlaveEvent;
        
        /// <summary>
        /// MySQL 컴포넌트를 초기화합니다.
        /// </summary>
        /// <param name="server"></param>
        public MySQLComponent(Program server)
            : base(server)
        {
            if (server.MySqlSettings.Master == null)
                throw new Exception("no master server configured!");

            m_Server = server;
            m_Masters = new MySqlConnection[
                Math.Max(1, server.MySqlSettings.MasterInstances)];
            m_MasterStates = new State[m_Masters.Length];

            m_Slaves = new MySqlConnection[
                server.MySqlSettings.Slaves != null ? 
                server.MySqlSettings.Slaves.Length : 0];
            m_SlaveStates = new State[m_Slaves.Length];

            m_MasterEvent = new AutoResetEvent(false);
            if (m_Slaves.Length > 0)
                m_SlaveEvent = new AutoResetEvent(false);

            for (int i = 0; i < m_Masters.Length; i++)
                Connect(i, true, server.MySqlSettings.Master);

            for (int i = 0; i < m_Slaves.Length; i++)
                Connect(i, false, server.MySqlSettings.Slaves[i]);
        }

        /// <summary>
        /// MySQL 컴포넌트를 획득합니다.
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        public static MySQLComponent GetMySQLComponent(Application application) 
            => application.GetComponent(typeof(MySQLComponent)) as MySQLComponent;

        /// <summary>
        /// MySQL 명령을 처리합니다.
        /// </summary>
        /// <param name="application"></param>
        /// <param name="wannaWrite"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static Future Perform(Application application, bool wannaWrite, Action<MySqlConnection> callback)
            => GetMySQLComponent(application).Perform(wannaWrite, callback);

        /// <summary>
        /// MySQL 명령을 처리합니다.
        /// </summary>
        /// <param name="wannaWrite"></param>
        /// <param name="callback"></param>
        public Future Perform(bool wannaWrite, Action<MySqlConnection> callback)
        {
            return m_Server.InvokeByWorker(() =>
            {
                MySqlConnection connection = Acquire(wannaWrite);
                State latestState = State.None;

                try
                {
                    callback(connection);
                }
                catch (MySqlException)
                {
                    if (connection.State == System.Data.ConnectionState.Broken ||
                        connection.State == System.Data.ConnectionState.Closed)
                    {
                        latestState = State.Lost;
                    }
                }

                Release(connection, latestState);
            });
        }

        /// <summary>
        /// 끊어진 것으로 표시된 접속들을 복구합니다.
        /// </summary>
        private void OnRecoverConnections()
        {
            int deadMasters = 0, deadSlaves = 0;

            lock (this)
            {
                lock (m_SlaveStates)
                {
                    for (int i = 0; i < m_SlaveStates.Length; i++)
                    {
                        if (m_SlaveStates[i] == State.Lost)
                        {
                            Log.w("[MySQL, slave #{0}] Recovering connection...", i);
                            try { m_Slaves[i].Close(); } catch { }
                            try { Connect(i, false, m_Server.MySqlSettings.Slaves[i]); }
                            catch (Exception e)
                            {
                                Log.w("[MySQL, slave #{0}] Failed to recover this connection: {1}", i, e.Message);
                                deadSlaves++;
                            }
                        }
                    }
                }

                lock (m_MasterStates)
                {
                    for (int i = 0; i < m_MasterStates.Length; i++)
                    {
                        if (m_MasterStates[i] == State.Lost)
                        {
                            Log.w("[MySQL, master #{0}] Recovering connection...", i);
                            try { m_Masters[i].Close(); } catch { }
                            try { Connect(i, true, m_Server.MySqlSettings.Master); }
                            catch (Exception e)
                            {
                                Log.w("[MySQL, master #{0}] Failed to recover this connection: {1}", i, e.Message);
                                deadMasters++;
                            }
                        }
                    }
                }

                if (m_SlaveStates.Length <= deadSlaves &&
                    m_MasterStates.Length <= deadMasters)
                {
                    Log.w("[MySQL] Fatal error: all connections are dead and unrecoverable!");
                    m_Server.ExitByFatalError();
                }
            }
        }

        /// <summary>
        /// MySQL 접속 객체를 획득합니다.
        /// </summary>
        /// <param name="wannaWrite"></param>
        /// <returns></returns>
        public MySqlConnection Acquire(bool wannaWrite = false)
        {
            if (wannaWrite)
            {
                while (true)
                {
                    lock (m_MasterStates)
                    {
                        for (int i = 0; i < m_MasterStates.Length; i++)
                        {
                            if (m_MasterStates[i] == State.None)
                            {
                                m_MasterStates[i] = State.Owned;
                                return m_Masters[i];
                            }
                        }
                    }

                    Thread.Yield();
                    m_MasterEvent.WaitOne();
                }
            }

            else if (m_Slaves.Length <= 0)
                return Acquire(true);

            while (true)
            {
                lock (m_SlaveStates)
                {
                    for (int i = 0; i < m_SlaveStates.Length; i++)
                    {
                        if (m_SlaveStates[i] == State.None)
                        {
                            m_SlaveStates[i] = State.Owned;
                            return m_Slaves[i];
                        }
                    }
                }

                Thread.Yield();
                m_SlaveEvent.WaitOne();
            }
        }

        /// <summary>
        /// MySQL 접속 객체를 반환합니다.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="state"></param>
        public void Release(MySqlConnection connection, State state = State.None)
        {
            for(int i = 0; i < m_Masters.Length; i++)
            {
                if (m_Masters[i] == connection)
                {
                    lock (m_MasterStates)
                    {
                        m_MasterStates[i] = state;
                        m_MasterEvent.Set();
                    }

                    if (state == State.Lost)
                    {
                        Log.w("[MySQL, master #{0}] Connection lost during executing query.", i);
                        m_Server.InvokeByWorker(OnRecoverConnections);
                    }
                    return;
                }
            }

            for (int i = 0; i < m_Slaves.Length; i++)
            {
                if (m_Slaves[i] == connection)
                {
                    lock (m_SlaveStates)
                    {
                        m_SlaveStates[i] = state;
                        m_SlaveEvent.Set();
                    }

                    if (state == State.Lost)
                    {
                        Log.w("[MySQL, slave #{0}] Connection lost during executing query.", i);
                        m_Server.InvokeByWorker(OnRecoverConnections);
                    }
                    return;
                }
            }
        }

        /// <summary>
        /// 데이터베이스 서버로 접속합니다.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="master"></param>
        /// <param name="config"></param>
        private void Connect(int index, bool master, MySqlSettings.Config config)
        {
            MySqlConnection Connection = null;
            string ConnString = null;

            try
            {
                ConnString = GenerateConnectionString(config);
                (Connection = new MySqlConnection(ConnString)).Open();
            }
            catch {
                throw new Exception("connecting failed to [" + ConnString + "].");
            }

            if (master)
            {
                m_Masters[index] = Connection;
                m_MasterStates[index] = State.None;
                Log.w(" - [MySQL, master #{0}] connected and ready.", index + 1);
            }
            else
            {
                m_Slaves[index] = Connection;
                m_SlaveStates[index] = State.None;
                Log.w(" - [MySQL, slave #{0}] connected and ready.", index + 1);
            }
        }

        /// <summary>
        /// MySQL 커넥션 문자열을 생성합니다.
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private static string GenerateConnectionString(MySqlSettings.Config config)
        {
            MySqlConnectionStringBuilder MCSB = new MySqlConnectionStringBuilder();

            MCSB.Server = config.Host;
            MCSB.Port = (uint)config.Port;
            MCSB.UserID = config.User;

            if (!string.IsNullOrEmpty(config.Password) &&
                !string.IsNullOrWhiteSpace(config.Password))
            {
                MCSB.Password = config.Password;
            }

            MCSB.Database = config.Scheme;
            MCSB.CharacterSet = "utf8";

            return MCSB.GetConnectionString(true);
        }
    }
}
