using OpenTalk.Components;
using OpenTalk.Net.Http;
using OpenTalk.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static OpenTalk.Components.IpcComponent;
using DApp = System.Windows.Forms.Application;

namespace OpenTalk
{
    class Program : Application
    {
        private IpcComponent m_Ipc;
        private FrmMain m_MainForm;
        private Session m_Session;

        /// <summary>
        /// 세션 서버 주소입니다.
        /// </summary>
        internal static Uri g_GatewayUri = new Uri("http://127.0.0.1/otk/");

        /// <summary>
        /// 회원 가입 페이지 주소입니다.
        /// </summary>
        internal static Uri g_RegisterUri = new Uri("http://127.0.0.1/member/register.php");

        /// <summary>
        /// 패스워드 분실 페이지 주소입니다.
        /// </summary>
        internal static Uri g_LostPasswordUri = new Uri("http://127.0.0.1/member/lost_password.php");

        /// <summary>
        /// 약관 읽기 주소입니다.
        /// </summary>
        internal static Uri g_ReadAgreementsUri = new Uri("http://127.0.0.1/agreements.php");

        /// <summary>
        /// 오픈톡 인스턴스입니다.
        /// </summary>
        public static Program OTK => ((Program)FutureInstance.Result);

        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main(string[] Arguments)
        {
            DApp.EnableVisualStyles();
            DApp.SetCompatibleTextRenderingDefault(false);
            Run(new Program(), Arguments);
        }

        /// <summary>
        /// 오픈톡 세션 객체입니다.
        /// </summary>
        public Session Session => m_Session;

        /// <summary>
        /// 프로그램을 초기화합니다.
        /// </summary>
        protected override void PreInitialize()
        {
            Log.w("Initializing IPC channel...");
            m_Ipc = new IpcComponent(this, WorkingMode.BothWay, "opentalk");

            if (m_Ipc.Mode == WorkingMode.ServerOnly)
                m_Ipc.Message += OnIpcMessage;

            m_Ipc.Activate();
        }

        /// <summary>
        /// 어플리케이션 인스턴스를 초기화합니다.
        /// </summary>
        protected override void Initialize()
        {
            bool showMain = true;

            if (m_Ipc.Mode == WorkingMode.ClientOnly)
            {
                m_Ipc.Send(Arguments);
                Invoke(() => ExitApp());
                return;
            }

            foreach (string Arg in Arguments)
            {
                if (Arg == "boot-run")
                {
                    showMain = false;
                    break;
                }
            }

            m_Session = new Session(this, g_GatewayUri);
            Invoke(() =>
            {
                m_MainForm = new FrmMain();

                if (showMain)
                    m_MainForm.Show();
            }).Wait();
        }

        /// <summary>
        /// IPC 메시지가 도착했을 때 실행됩니다.
        /// </summary>
        /// <param name="obj"></param>
        private void OnIpcMessage(string[] arguments)
        {
            if (arguments.Length <= 0)
                arguments = new string[] { "show" };

            switch(arguments[0])
            {
                case "show":
                default:
                    Invoke(() => m_MainForm.Show()).Wait();
                    break;
            }
        }
    }
}
