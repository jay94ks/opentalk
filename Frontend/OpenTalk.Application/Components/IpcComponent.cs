using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;

namespace OpenTalk.Components
{
    /// <summary>
    /// IPC 채널을 관리하는 컴포넌트입니다.
    /// 오버라이드 하거나, 생성자를 통해 컴포넌트의 동작 모드를 설정하십시오.
    /// 주의: 이 컴포넌트는 인스턴싱되는 순간부터 IPC 채널을 생성하고 IPC 통신을 시작합니다.
    /// </summary>
    public partial class IpcComponent : Application.Component
    {
        private IpcServerChannel m_IpcServer = null;
        private IpcClientChannel m_IpcClient = null;

        private Messanger m_Messanger = null;

        /// <summary>
        /// IPC 채널을 어떻게 열지 지정합니다.
        /// </summary>
        public enum WorkingMode
        {
            /// <summary>
            /// 서버 모드 채널을 열지 못하면 실패한 것으로 간주합니다.
            /// </summary>
            ServerOnly,

            /// <summary>
            /// 클라이언트 모드 채널을 열지 못하면 실패한 것으로 간주합니다.
            /// </summary>
            ClientOnly,

            /// <summary>
            /// 서버 모드 채널을 열어보고, 실패하면 클라이언트 모드로 엽니다.
            /// </summary>
            BothWay,

            /// <summary>
            /// 요청받은 채널을 열 수 없었습니다.
            /// </summary>
            Failed
        }

        /// <summary>
        /// IPC 컴포넌트를 초기화합니다.
        /// </summary>
        /// <param name="PipeName">이 컴포넌트가 개설하고자 하는 RPC 채널의 이름입니다.</param>
        public IpcComponent(string PipeName)
        {
            Initialize(WorkingMode.BothWay, PipeName);
        }

        /// <summary>
        /// IPC 컴포넌트를 초기화합니다.
        /// </summary>
        /// <param name="Mode">이 컴포넌트가 개설하고자 하는 RPC 채널의 동작 모드를 지정합니다.</param>
        /// <param name="PipeName">이 컴포넌트가 개설하고자 하는 RPC 채널의 이름입니다.</param>
        public IpcComponent(WorkingMode Mode, string PipeName)
        {
            Initialize(Mode, PipeName);
        }

        /// <summary>
        /// IPC 컴포넌트를 초기화합니다.
        /// </summary>
        /// <param name="Mode">이 컴포넌트가 개설하고자 하는 RPC 채널의 동작 모드를 지정합니다.</param>
        /// <param name="PipeName">이 컴포넌트가 개설하고자 하는 RPC 채널의 이름입니다.</param>
        public IpcComponent(Application application, WorkingMode Mode, string PipeName)
            : base(application)
        {
            Initialize(Mode, PipeName);
        }

        /// <summary>
        /// IPC 채널의 모드를 확인합니다.
        /// 성공적으로 열린 경우엔 현재 개설된 채널의 모드를 나타냅니다.
        /// </summary>
        public WorkingMode Mode { get; private set; }

        /// <summary>
        /// IPC 채널의 이름입니다.
        /// </summary>
        public string PipeName { get; private set; }

        /// <summary>
        /// 메시지가 수신되면 실행되는 이벤트입니다.
        /// </summary>
        public event Action<string[]> Message;

        /// <summary>
        /// IPC 컴포넌트를 초기화합니다.
        /// </summary>
        /// <param name="Mode"></param>
        /// <param name="PipeName"></param>
        private bool Initialize(WorkingMode Mode, string PipeName)
        {
            this.PipeName = PipeName;

            switch (Mode)
            {
                case WorkingMode.ServerOnly:
                    try
                    {
                        ChannelServices.RegisterChannel(
                            m_IpcServer = new IpcServerChannel(PipeName),
                            false);

                        RemotingConfiguration.RegisterWellKnownServiceType(
                            typeof(Messanger), "management", WellKnownObjectMode.Singleton);

                        m_Messanger = new Messanger(this);
                        this.Mode = WorkingMode.ServerOnly;
                        return true;
                    }
                    catch
                    {
                        if (m_IpcServer != null)
                            ChannelServices.UnregisterChannel(m_IpcServer);

                        m_Messanger = null;
                        m_IpcServer = null;
                    }
                    break;

                case WorkingMode.ClientOnly:
                    try
                    {
                        ChannelServices.RegisterChannel(
                            m_IpcClient = new IpcClientChannel(),
                            false);

                        RemotingConfiguration.RegisterWellKnownClientType(
                            typeof(Messanger), "ipc://" + PipeName + "/management");

                        m_Messanger = new Messanger();
                        this.Mode = WorkingMode.ClientOnly;
                        return true;
                    }
                    catch
                    {
                        if (m_IpcClient != null)
                            ChannelServices.UnregisterChannel(m_IpcClient);

                        m_Messanger = null;
                        m_IpcClient = null;
                    }
                    break;

                case WorkingMode.BothWay:
                    if (Initialize(WorkingMode.ServerOnly, PipeName) ||
                        Initialize(WorkingMode.ClientOnly, PipeName))
                    {
                        return true;
                    }

                    break;
            }

            this.Mode = WorkingMode.Failed;
            return false;
        }

        /// <summary>
        /// 컴포넌트가 활성화될 때 수행할 동작들을 수행합니다.
        /// </summary>
        protected override void OnActivated()
        {
        }

        /// <summary>
        /// IPC 채널로 메시지를 보냅니다.
        /// </summary>
        /// <param name="message"></param>
        public void Send(params string[] message) => m_Messanger.Send(message);

        /// <summary>
        /// 제어 메시지가 IPC 채널로 수신되면 실행됩니다.
        /// </summary>
        /// <param name="message"></param>
        private void OnMessage(params string[] message)
        {
            Message?.Invoke(message);
        }
    }
}
