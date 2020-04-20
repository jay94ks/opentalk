using Newtonsoft.Json;
using OpenTalk.Components;
using OpenTalk.Net;
using OpenTalk.Net.Http;
using OpenTalk.Server.Framework;
using OpenTalk.Server.MySQLs;
using OpenTalk.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static OpenTalk.Components.IpcComponent;

namespace OpenTalk.Server
{
    public class Program : Application
    {
        private IpcComponent m_Ipc;
        private TcpListener[] m_Listeners = null;

        private List<Connection> m_Connections = new List<Connection>();
        private IMessageHandler[] m_MessageHandlers = null;

        private int m_WorkerRounds = 0;
        private bool m_InitializedAndReady = false;
        private bool m_ConnectionPending = false;

        private bool m_Registered = false;

        /// <summary>
        /// 오픈톡 서버 프로그램 진입점입니다.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args) => Run(new Program(), args);

        /// <summary>
        /// 서버 인스턴스에 접근합니다.
        /// </summary>
        static Program ServerInstance => RunningInstance as Program;

        /// <summary>
        /// 이 서버의 Unique ID입니다.
        /// </summary>
        public Guid UniqueId { get; private set; }

        /// <summary>
        /// 커넥션 리스너 설정값들입니다.
        /// </summary>
        public ListenerSettings ListenerSettings { get; private set; }

        /// <summary>
        /// 모니터링 포트 설정값들입니다.
        /// </summary>
        public MonitorSettings MonitorSettings { get; private set; }

        /// <summary>
        /// 일반적으로 사용되는 설정값들입니다.
        /// </summary>
        public CommonSettings CommonSettings { get; private set; }

        /// <summary>
        /// 인증 토큰을 확인하는데 사용되는 설정값들입니다.
        /// </summary>
        public AuthorizationSettings AuthorizationSettings { get; private set; }

        /// <summary>
        /// MySQL 접속 정보입니다.
        /// </summary>
        public MySqlSettings MySqlSettings { get; private set; }

        /// <summary>
        /// 메세징 모듈 설정입니다.
        /// </summary>
        public MessageModuleSettings MessageModuleSettings { get; private set; }

        /// <summary>
        /// 셋팅을 로드합니다.
        /// </summary>
        /// <typeparam name="SettingType"></typeparam>
        /// <param name="Name"></param>
        /// <returns></returns>
        private SettingType LoadSetting<SettingType>(params string[] Names)
            where SettingType : new()
        {
            string fileName = Path.Combine(Environments.SettingPath, 
                string.Join(".", Names) + ".json");
            SettingType outObject = default(SettingType);

            try
            {
                outObject = JsonConvert.DeserializeObject
                    <SettingType>(File.ReadAllText(fileName));

                Log.w("Loading '{0}.json' success.", string.Join(".", Names));
            }
            catch
            {
                File.WriteAllText(fileName, JsonConvert.SerializeObject(
                    outObject = new SettingType(), Formatting.Indented));

                Log.w("Loading '{0}.json' failure... defaulted.", string.Join(".", Names));
            }

            return outObject;
        }

        /// <summary>
        /// 서버 프로그램이 시작될 때, 설정을 로드합니다.
        /// </summary>
        protected override void PreInitialize()
        {
            Log.w("Initializing IPC channel...");
            m_Ipc = new IpcComponent(this, WorkingMode.BothWay, "opentalk-server");

            if (m_Ipc.Mode == WorkingMode.ServerOnly)
                m_Ipc.Message += OnIpcMessage;

            m_Ipc.Activate();
        }

        /// <summary>
        /// 서버 프로그램을 초기화합니다.
        /// </summary>
        protected override void Initialize()
        {
            // 클라이언트 모드일 땐, IPC 채널로 실행 인자만 서버쪽에 전달하고 종료합니다.
            if (m_Ipc.Mode == WorkingMode.ClientOnly)
            {
                if (Arguments.Length > 0)
                {
                    if (Arguments[0] == "start")
                    {
                        Log.w("[ERROR] Server process already running. wanna restart?");
                        Log.w("[INSTRUCTION to RESTART] {0} stop && {0} start",
                            Path.GetFileName(typeof(Program).Assembly.Location));
                    }

                    else if (Arguments[0] == "stop")
                    {
                        Log.w("Stopping server process... wait...");
                        m_Ipc.Send(Arguments);
                        Thread.Sleep(5000);
                    }

                    else
                    {
                        Log.w("[ERROR] Unknown argument: {0}", Arguments[0]);
                    }
                }
                else
                {
                    Log.w("Usage: {0} [start|stop]",
                        Path.GetFileName(typeof(Program).Assembly.Location));
                }

                Invoke(() => ExitApp());
                return;
            }

            // 실행 인자가 지정되었습니다.
            else if (Arguments.Length > 0)
            {
                Arguments[0] = Arguments[0].ToLower();
                switch (Arguments[0])
                {
                    case "start":
                        Log.w("[WARNING] This process doesn't support forking mechanism like POSIX system do.");
                        break;

                    case "help":
                        Log.w("Usage: {0} [start|stop]",
                            Path.GetFileName(typeof(Program).Assembly.Location));

                        Invoke(() => ExitApp());
                        return;

                    default:
                        Log.w("{0}: Unknown command.", Arguments[0]);
                        ExitByFatalError();
                        return;
                }
            }

            Log.w("Initializing 'OpenTalk Textile' server...");
            Log.w("Loading server configurations...");

            CommonSettings = LoadSetting<CommonSettings>("common");
            ListenerSettings = LoadSetting<ListenerSettings>("listeners");
            MonitorSettings = LoadSetting<MonitorSettings>("monitors");
            AuthorizationSettings = LoadSetting<AuthorizationSettings>("authorization");
            MySqlSettings = LoadSetting<MySqlSettings>("mysql");
            MessageModuleSettings = LoadSetting<MessageModuleSettings>("message-modules");

            if (!LoadServerUniqueId())
            {
                ExitByFatalError();
                return;
            }

            SetThreadPoolCapacity();

            Log.w("Connecting MySQL database...");
            try { (new MySQLComponent(this)).Activate(); }
            catch (Exception e)
            {
                Log.w("[MySQL] Critical error: {0}", e.Message);
                ExitByFatalError();
                return;
            }

           
            if (!OnInitializeMessageHandlers())
            {
                ExitByFatalError();
                return;
            }

            Log.w("Preparing textile protocol listeners...");
            if (ListenerSettings.Primary == null)
            {
                Log.w("[Textile] Critical error: no primary listening configured!");
                ExitByFatalError();
                return;
            }

            if (ListenerSettings.Secondaries != null)
                m_Listeners = new TcpListener[ListenerSettings.Secondaries.Length + 1];

            m_Listeners = m_Listeners != null ? m_Listeners : new TcpListener[1];

            Log.w(" - Preparing texile listener #1 of {0} (primary, {1}:{2}).",
                m_Listeners.Length, ListenerSettings.Primary.Address,
                ListenerSettings.Primary.PortNumber);

            if (!PrepareTextileListener(0, ListenerSettings.Primary))
                return;

            for (int i = 1; i < m_Listeners.Length; i++)
            {
                ListenerSettings.Textile Secondary = ListenerSettings.Secondaries[i - 1];
                Log.w(" - Preparing texile listener #{0} of {1} (secondary, {2}:{3}).",
                    i, m_Listeners.Length, Secondary.Address, Secondary.PortNumber);

                if (!PrepareTextileListener(i, Secondary))
                    return;
            }

            
            if (!RegisterToWebGateway())
            {
                ExitByFatalError();
                return;
            }

            Log.w("Okay, server started successfully.");
            lock (this)
            {
                m_InitializedAndReady = true;

                // 초기화하는 도중에 접속자가 접속했다면,
                // 작업자에게 접속자 수락 작업을 지시합니다.
                if (m_ConnectionPending)
                {
                    InvokeByWorker(() =>
                    {
                        foreach (TcpListener listener in m_Listeners)
                        {
                            if (listener.CanReadImmediately)
                                OnAcceptReady(listener, -1);
                        }
                    });
                }
            }
        }

        private void SetThreadPoolCapacity()
        {
            int Workers, CompletionWorkers;

            ThreadPool.GetMaxThreads(out Workers, out CompletionWorkers);

            Workers = Math.Max(Workers, CommonSettings.WorkerInstances);
            CompletionWorkers = Math.Max(CompletionWorkers, CommonSettings.WorkerInstances);

            ThreadPool.SetMaxThreads(Workers, CompletionWorkers);
        }

        protected override void DeInitialize()
        {
            UnregisterFromWebGateway();
            base.DeInitialize();
        }

        /// <summary>
        /// 서버의 절대 식별 ID를 로드합니다.
        /// </summary>
        /// <returns></returns>
        private bool LoadServerUniqueId()
        {
            string IdFile = Path.Combine(Environments.SettingPath, "unique-id.guid");

            try
            {
                if (File.Exists(IdFile))
                {
                    byte[] Bytes = File.ReadAllBytes(IdFile);
                    UniqueId = new Guid(Bytes);
                }
                else
                {
                    UniqueId = Guid.NewGuid();
                    File.WriteAllBytes(IdFile, UniqueId.ToByteArray());
                }
            }

            catch
            {
                Log.w("Failed to load/create a unique id of this server!");
                return false;
            }

            Log.w("[SERVER UNIQUE-ID] {0}", UniqueId.ToString());
            return true;
        }

        public void ExitByFatalError()
        {
            Log.w("Terminating server because of fatal error...");
            Invoke(() => ExitApp());
        }

        private class RegistrationData
        {
            [JsonProperty("token")]
            public string Token { get; set; } = null;

            [JsonProperty("unique_id")]
            public string UniqueId { get; set; } = null;

            [JsonProperty("port")]
            public int Port { get; set; } = 0;
        }

        /// <summary>
        /// 웹 게이트웨이 서버에 이 서버를 등록합니다.
        /// </summary>
        /// <returns></returns>
        private bool RegisterToWebGateway()
        {
            var Registration = AuthorizationSettings.Registration;

            HttpComponent http = HttpComponent.GetHttpComponent(
                this, AuthorizationSettings.BaseUri);

            HttpResult<RegistrationData> Result = null;
            Log.w("Registering this server to OpenTalk Web-gateway...");

            // 최초 인증 정보 셋팅.
            http.Authorization = Registration.Authorization;
            Result = http.PostJson<RegistrationData>(
                HttpHelper.CombinePath(Registration.Path, Registration.QueryStrings),
                new RegistrationData() {
                    UniqueId = UniqueId.ToString(),
                    Port = ListenerSettings.Primary.PortNumber
                }).WaitResult();

            if (Result.Success)
            {
                if (Result.HasParsingError)
                    return false;

                http.Authorization = Result.ResponseObject.Token;
                Log.w(" + Success: {0}, {1}", Result.ResponseObject.Token, UniqueId.ToString());
                m_Registered = true;
                return true;
            }

            Log.w(" - Failure: {0} -> {1} {2}", UniqueId.ToString(), Result.StatusCode, Result.StatusMessage);
            Thread.Sleep(1000);
            return false;
        }

        /// <summary>
        /// 웹 게이트웨이 서버에서 이 서버를 등록 해제합니다.
        /// </summary>
        /// <returns></returns>
        private bool UnregisterFromWebGateway()
        {
            var Registration = AuthorizationSettings.Registration;

            HttpComponent http = HttpComponent.GetHttpComponent(
                this, AuthorizationSettings.BaseUri);

            HttpResult<RegistrationData> Result = null;
            Log.w("Unregistering this server from OpenTalk Web-gateway...");

            Result = http.Delete<RegistrationData>(
                HttpHelper.CombinePath(Registration.Path, Registration.QueryStrings))
                .WaitResult();

            if (Result.Success)
            {
                if (Result.HasParsingError)
                    return false;

                http.Authorization = null;
                m_Registered = false;
                return true;
            }

            Thread.Sleep(1000);
            return false;
        }

        /// <summary>
        /// 특정한 작업을 작업자 인스턴스에서 실행합니다.
        /// 작업이 작업자 인스턴스에 분배되는 방식은 라운드 로빈입니다.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public Future InvokeByWorker(Action action)
        {
            return Future.Run(action);
        }

        /// <summary>
        /// Textile 리스너를 초기화합니다.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="conf"></param>
        private bool PrepareTextileListener(int index, ListenerSettings.Textile conf)
        {
            try
            {
                m_Listeners[index] = new TcpListener(
                    IPAddress.Parse(conf.Address), conf.PortNumber);

                m_Listeners[index].ReadReady += OnAcceptReady;
                m_Listeners[index].Start();
            }

            catch (FormatException)
            {
                Log.w("[Textile, {0}] misconfiguration found: '{1}' is invalid IP address!",
                    index <= 0 ? "primary" : "#" + index, conf.Address);

                ExitByFatalError();
                return false;
            }

            catch
            {
                Log.w("[Textile, {0}] runtime error found: the port, '{1}' is already in use!",
                    index <= 0 ? "primary" : "#" + index, conf.PortNumber);

                ExitByFatalError();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Accept할 준비가 끝나면 실행되는 이벤트 메서드입니다.
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="arg2"></param>
        private void OnAcceptReady(TcpListener listener, int arg2)
        {
            lock(this)
            {
                // 초기화가 끝나기 전이라면, 
                // 접속자가 대기중이란 것만 표시하고
                // 실제 접속을 처리하지 않습니다.

                if (!m_InitializedAndReady)
                {
                    m_ConnectionPending = true;
                    return;
                }
            }

            while (listener.CanReadImmediately)
                listener.Accept(OnInitiateConnection);
        }

        /// <summary>
        /// 클라이언트를 수락하면 호출되는 콜백입니다.
        /// </summary>
        /// <param name="connection"></param>
        private void OnInitiateConnection(TcpClient tcpClient)
        {
            Connection connection = new Connection(this, tcpClient);

            // 연결이 끊어지면 커넥션 리스트에서 제거합니다.
            connection.Disconnected +=
                (X) => this.Locked(() => m_Connections.Remove(X));

            lock (m_Connections)
                m_Connections.Add(connection);
        }


        /// <summary>
        /// IPC 메시지가 수신되면 실행됩니다.
        /// </summary>
        /// <param name="messages"></param>
        private void OnIpcMessage(string[] messages)
        {
            if (messages.Length <= 0)
                return;

            messages[0] = messages[0].ToLower();
            switch (messages[0])
            {
                case "stop":
                    // 서버를 정지시킵니다.
                    Log.w("Stopping server...");
                    ExitApp();
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 메시지 핸들러들을 로드합니다.
        /// </summary>
        /// <returns></returns>
        private bool OnInitializeMessageHandlers()
        {
            List<Type> CollectedHandlerTypes = new List<Type>();
            Assembly[] ScanTargets = new Assembly[1 +
                (MessageModuleSettings != null && MessageModuleSettings.Modules != null ?
                MessageModuleSettings.Modules.Length : 0)];

            Type[] ServerCtorArgTypes = new Type[] { typeof(Program) };
            object[] ServerCtorArgs = new object[] { this };

            List<IMessageHandler> HandlerInstances = new List<IMessageHandler>();

            Log.w("Loading textile message handlers...");
            ScanTargets[0] = typeof(Program).Assembly;
            LoadMessageHandlerModules(ScanTargets);
            CollectMessageHandlerTypes(CollectedHandlerTypes, ScanTargets);

            if (CollectedHandlerTypes.Count > 0)
                Log.w(" + Instancing implementations...");
            else Log.w(" ! No implementations are found.");

            foreach (Type EachHandlerType in CollectedHandlerTypes)
            {
                ConstructorInfo DefaultCtor = null, ServerCtor = null;
                IMessageHandler Handler = null;

                try { ServerCtor = EachHandlerType.GetConstructor(ServerCtorArgTypes); }
                catch { }

                if (ServerCtor == null)
                {
                    try { DefaultCtor = EachHandlerType.GetConstructor(Type.EmptyTypes); }
                    catch { }
                }

                try
                {
                    if (ServerCtor != null)
                        Handler = (IMessageHandler)ServerCtor.Invoke(ServerCtorArgs);

                    else if (DefaultCtor != null)
                        Handler = (IMessageHandler)DefaultCtor.Invoke(new object[0]);
                }
                catch (Exception e)
                {
                    Log.w("   - Error, from '{0}': {1}", EachHandlerType.FullName, e.Message);
                    continue;
                }

                if (Handler != null)
                    HandlerInstances.Add(Handler);
            }

            if (HandlerInstances.Count > 0)
            {
                Log.w(" + Sorting message handlers by their priority...");
                HandlerInstances.Sort((A, B) =>
                {
                    return A.GetType().GetCustomAttribute<MessageHandlerAttribute>().Priority -
                            B.GetType().GetCustomAttribute<MessageHandlerAttribute>().Priority;
                });
            }

            m_MessageHandlers = HandlerInstances.ToArray();

            if (m_MessageHandlers.Length > 0)
                Log.w(" * {0} Message handlers are configured.", m_MessageHandlers.Length);
            else Log.w(" * No message handlers are configured.", m_MessageHandlers.Length);

            return true;
        }

        /// <summary>
        /// 메시지 핸들러 모듈들을 로드합니다.
        /// </summary>
        /// <param name="ScanTargets"></param>
        private void LoadMessageHandlerModules(Assembly[] ScanTargets)
        {
            for (int i = 1; i < ScanTargets.Length; i++)
            {
                string FileName = MessageModuleSettings.Modules[i - 1];

                if (string.IsNullOrEmpty(FileName) ||
                    string.IsNullOrWhiteSpace(FileName))
                    continue;

                if (!File.Exists(FileName))
                {
                    try
                    {
                        FileName = Path.Combine(Environments.ModulePath, FileName);
                        if (!File.Exists(FileName))
                        {
                            Log.w(" - {0}: Not found!", MessageModuleSettings.Modules[i - 1]);
                            continue;
                        }
                    }
                    catch
                    {
                        Log.w(" - {0}: Not found!", MessageModuleSettings.Modules[i - 1]);
                        continue;
                    }
                }

                Log.w(" + Loading {0}...", Path.GetFileName(FileName));
                ScanTargets[i] = Assembly.LoadFile(FileName);
            }
        }

        /// <summary>
        /// 지정된 모듈들에서 핸들러 구현들만 솎아냅니다.
        /// </summary>
        /// <param name="CollectedHandlerTypes"></param>
        /// <param name="ScanTargets"></param>
        private static void CollectMessageHandlerTypes(List<Type> CollectedHandlerTypes, Assembly[] ScanTargets)
        {
            Log.w(" + Collecting implementations from loaded modules...");
            foreach (Assembly EachModule in ScanTargets)
            {
                if (EachModule == null)
                    continue;

                int Count = CollectedHandlerTypes.Count;

                Log.w(" - Scanning {0}...", Path.GetFileName(EachModule.Location));
                foreach (Type EachType in EachModule.GetTypes())
                {
                    MessageHandlerAttribute HandlerAttribute = null;

                    try {
                        HandlerAttribute = EachType.GetCustomAttribute<MessageHandlerAttribute>();
                    }
                    catch { continue; }

                    if (HandlerAttribute != null)
                    {
                        //EachType.IsSubclassOf(typeof(IMessageHandler))
                        CollectedHandlerTypes.Add(EachType);
                    }
                }

                if (CollectedHandlerTypes.Count - Count > 0)
                {
                    Log.w("   ^- {0} Implementations are found.", CollectedHandlerTypes.Count - Count);
                }
            }
        }

        /// <summary>
        /// 주어진 메시지를 처리합니다.
        /// </summary>
        /// <param name="Connection"></param>
        /// <param name="Label"></param>
        /// <param name="Message"></param>
        internal void HandleMessage(Connection Connection, string Label, string Message)
        {
            if (m_MessageHandlers != null && m_MessageHandlers.Length > 0)
            {
                foreach (IMessageHandler handler in m_MessageHandlers)
                {
                    if (handler.HandleMessage(Connection, Label, Message))
                        return;
                }

                Log.w("[Message, from {0}, Label: {1}] couldn't be handled!",
                    Connection.RemoteAddress, Label);
            }
        }
    }
}
