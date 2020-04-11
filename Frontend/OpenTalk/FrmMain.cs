using OpenTalk.Credentials;
using OpenTalk.Main;
using OpenTalk.UI.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenTalk
{
    public partial class FrmMain : Form
    {
        private Session m_Session;
        private Credential m_StoredCredential;
        
        /// <summary>
        /// 메인 폼 입니다.
        /// </summary>
        public FrmMain()
        {
            InitializeComponent();

            m_TrayIcon.Text = Text;
            m_TrayIcon.Icon = Icon;
            m_TrayIcon.Visible = true;

            m_ScreenSwitcher.ScreenTypes = new Type[] { typeof(ScrMainLogin) };

            m_Session = Program.OTK.Session;
            m_Session.Authentication.Authenticated += OnAuthenticationChanged;
            m_Session.Authentication.Deauthenticated += OnAuthenticationChanged;
            m_TrayMenuLockMode.Enabled = m_TrayMenuLogout.Enabled = false;
        }

        /// <summary>
        /// 사용자가 창을 닫으면, 폼을 닫지 않고 숨김 처리합니다.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.IsUserClosing())
                e.CancelAnd(Hide);

            base.OnFormClosing(e);
        }

        /// <summary>
        /// 창이 보여질 때 작업 표시줄의 트레이 아이콘을 숨겨주고,
        /// 창이 더이상 보이지 않을 때 트레이 아이콘을 보여줍니다.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            m_TrayIcon.Visible = !Visible;
            base.OnVisibleChanged(e);
        }

        /// <summary>
        /// 사용자가 트레이 아이콘을 더블클릭하거나 트레이 메뉴에서 열기를 누르면 실행됩니다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnShowRequested(object sender, EventArgs e) => Show();

        /// <summary>
        /// 오픈톡 정보 메뉴를 누르면 실행됩니다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAboutRequested(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 잠금 모드 설정을 요청받았습니다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLockModeRequested(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 로그아웃을 요청받았습니다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLogoutRequested(object sender, EventArgs ev) => BeginDeauthenticate();

        /// <summary>
        /// 사용자가 앱 종료를 요청했습니다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnExitRequested(object sender, EventArgs e) => Application.ExitApp();

        /// <summary>
        /// 이 창이 보여질 때 실행됩니다.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            Credential Credential = LoadAutoCredential();

            // 로그인 화면을 보여줍니다.
            m_ScreenSwitcher.SwitchScreen(typeof(ScrMainLogin));

            if (Credential != null)
            {
                if (Credential is GenericCredential)
                {
                    // 로그인 정보가 저장만 된 경우. (자동 로그인 아님)
                }

                else if (Credential is TokenizedCredential)
                {
                    // 자동 로그인 정보로 가공된 경우.
                    BeginAuthenticate(Credential);
                }
            }

            base.OnLoad(e);
        }

        /// <summary>
        /// 하드디스크에 저장된 자격증명 정보를 획득합니다.
        /// </summary>
        public Credential StoredCredential {
            get => m_StoredCredential;
            set {
                if (value != m_StoredCredential)
                {
                    string CredFile = Path.Combine(Application.Environments.SettingPath, "credential.otk");

                    if (value != null)
                    {
                        try
                        {
                            if (File.Exists(CredFile))
                                File.Delete(CredFile);

                            // TODO: 암호화/복호화
                            using (FileStream CredStream = new FileStream(CredFile, 
                                FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                BinaryWriter CredWriter = new BinaryWriter(CredStream, Encoding.UTF8, true);
                                Credential.Serialize(value, CredWriter);

                                CredWriter.Flush();
                                CredStream.Close();
                            }
                        }
                        catch { return; }
                    }
                    else
                    {
                        try
                        {
                            if (File.Exists(CredFile))
                                File.Delete(CredFile);
                        }
                        catch { }
                    }

                    m_StoredCredential = value;
                }
            }
        }

        /// <summary>
        /// 하드디스크에 저장된 인증 정보를 읽어옵니다.
        /// </summary>
        /// <returns></returns>
        private Credential LoadAutoCredential()
        {
            string CredFile = Path.Combine(Application.Environments.SettingPath, "credential.otk");
            Credential Credential = null;

            try
            {
                if (File.Exists(CredFile))
                {
                    // TODO: 암호화/복호화
                    using (FileStream CredStream = new FileStream(CredFile, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        BinaryReader CredReader = new BinaryReader(CredStream, Encoding.UTF8, true);
                        Credential.Deserialize(CredReader, out Credential);
                        CredStream.Close();
                    }
                }
            }

            catch { }

            m_StoredCredential = Credential;
            return Credential;
        }

        /// <summary>
        /// 에러 메시지를 출력합니다.
        /// </summary>
        /// <param name="Message"></param>
        /// <param name="Title"></param>
        public void ShowError(string Message, string Title = null)
        {
            if (string.IsNullOrEmpty(Title) ||
                string.IsNullOrWhiteSpace(Title))
            {
                Title = Text;
            }

            Application.Tasks.Invoke(() => MessageBox.Show(Message, Title,
                MessageBoxButtons.OK, MessageBoxIcon.Warning)).Wait();
        }

        /// <summary>
        /// 인증 작업을 시작합니다.
        /// </summary>
        /// <param name="Credential"></param>
        public bool BeginAuthenticate(Credential Credential)
        {
            Task AuthTask = null;

            try { AuthTask = m_Session.Authentication.Authenticate(Credential); }
            catch (SessionException e)
            {
                switch (e.ErrorCode)
                {
                    case SessionError.AuthAlready:
                        ShowError("이미 로그인 되어 있습니다.");
                        break;

                    case SessionError.AuthBusy:
                        ShowError("이미 로그인 시도를 하는 중입니다. " +
                            "잠시후 다시 시도해주십시오.");
                        break;

                    case SessionError.SessionBusy:
                        ShowError("현재 다른 요청을 처리하고 있어서, " +
                            "요청하신 작업을 수행할 수 없습니다.");
                        break;
                }

                return false;
            }

            AuthTask.ContinueOnMessageLoop(EndAuthenticate);
            return true;
        }

        /// <summary>
        /// 인증 작업이 완료되었을 때 실행되는 콜백입니다.
        /// </summary>
        /// <param name="obj"></param>
        private void EndAuthenticate(Task obj)
        {
            if (m_Session.Authentication.Credential == null)
            {
                switch (m_Session.ErrorCode)
                {
                    case SessionError.AuthNetworkError:
                        ShowError("현재 네트워크 상태가 불안정하여, " +
                            "로그인 서버에 연결 할 수 없었습니다.");
                        break;

                    case SessionError.AuthInvalidCredential:
                        ShowError("입력하신 이메일 혹은 전화번호, " +
                            "패스워드가 일치하지 않습니다. 확인 후 재시도해주십시오.");
                        break;

                    case SessionError.AuthExpiredCredential:
                        ShowError("자동 로그인 정보가 만료되었습니다. " +
                            "패스워드 인증으로 로그인하여 주십시오.");
                        break;

                    case SessionError.AuthDenied:
                        ShowError("알 수 없는 이유로, 서버가 로그인을 승인하지 않았습니다.");
                        break;

                    case SessionError.AuthServerError:
                        ShowError("현재 서버 점검중입니다. 잠시후 다시 시도해주십시오.");
                        break;

                    case SessionError.AuthResponseError:
                        ShowError("서버에서 예상치 못한 오류코드를 발송했습니다. 잠시후 다시 시도해주십시오.");
                        break;
                }
            }
        }

        /// <summary>
        /// 인증 해제 처리를 시작합니다.
        /// </summary>
        public void BeginDeauthenticate()
        {
            Task Task = null;

            try { Task = m_Session.Authentication.Deauthenticate(); }
            catch (SessionException e)
            {
                switch (e.ErrorCode)
                {
                    case SessionError.AuthAlready:
                        ShowError("로그인되지 않은 상태입니다.");
                        break;

                    case SessionError.AuthBusy:
                    case SessionError.SessionBusy:
                        ShowError("현재 다른 요청을 처리하고 있어서, " +
                            "요청하신 작업을 수행할 수 없습니다.");
                        break;
                }

                return;
            }

            Task.ContinueOnMessageLoop(EndDeauthentcate);
        }

        /// <summary>
        /// 인증 해제 처리가 완료되면 실행되는 콜백입니다.
        /// </summary>
        /// <param name="obj"></param>
        private void EndDeauthentcate(Task obj)
        {
            if (m_Session.Authentication.Credential == null)
            {
                switch (m_Session.ErrorCode)
                {
                    case SessionError.AuthNetworkError:
                        ShowError("현재 네트워크 상태가 불안정하여, " +
                            "로그인 서버에 연결 할 수 없었습니다.");
                        break;

                    case SessionError.AuthServerError:
                        ShowError("현재 서버 점검중입니다. 잠시후 다시 시도해주십시오.");
                        break;

                    case SessionError.AuthResponseError:
                        ShowError("서버에서 예상치 못한 오류코드를 발송했습니다. 잠시후 다시 시도해주십시오.");
                        break;
                }
            }
        }

        /// <summary>
        /// 사용자의 로그인 시도와 관련된 이벤트입니다.
        /// 로그인 상태에 따라 트레이 메뉴를 감추거나 보여줍니다.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="operation"></param>
        /// <param name="errorCode"></param>
        private void OnAuthenticationChanged(Session session, Session.Auth.Operation operation, SessionError errorCode)
        {
            Application.Tasks.Invoke(() =>
            {
                m_TrayMenuLockMode.Enabled = m_TrayMenuLogout.Enabled =
                    session.Authentication.Credential != null;
            }).Wait();
        }
    }
}
