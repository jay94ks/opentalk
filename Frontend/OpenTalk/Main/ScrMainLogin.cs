using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTalk.UI.Extensions;
using OpenTalk.Credentials;
using System.Diagnostics;

namespace OpenTalk.Main
{
    public partial class ScrMainLogin : UI.Screen
    {
        public ScrMainLogin()
        {
            InitializeComponent();
            m_ChkAutoLogin.Visible = false;
            Program.OTK.Session.Authentication.Authenticated += OnAuthenticated;
        }

        /// <summary>
        /// 이 스크린이 보이게 되면 부모 폼에서
        /// 자격 증명을 받아와, 셋팅합니다.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNowVisible(EventArgs e)
        {
            FrmMain Main = this.GetParent<FrmMain>();

            if (Main != null &&
                Main.StoredCredential != null)
            {
                m_ChkRemember.Checked = true;

                if (Main.StoredCredential is GenericCredential)
                    m_TxtIdentifier.Text = (Main.StoredCredential as GenericCredential).Identifier;

                else if (Main.StoredCredential is TokenizedCredential)
                {
                    m_TxtIdentifier.Text = (Main.StoredCredential as TokenizedCredential).Identifier;
                    m_ChkAutoLogin.Checked = true;

                    Main.BeginAuthenticate(Main.StoredCredential);
                }
            }

            base.OnNowVisible(e);
        }

        /// <summary>
        /// 이 스크린의 크기가 변경되면, 로그인 박스 위치를 중앙으로 옮깁니다.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
            m_LoginBox.Location = new Point(
                (int)((Width - m_LoginBox.Width) * 0.5),
                (int)((Height - m_LoginBox.Height) * 0.5));

            base.OnResize(e);
        }

        /// <summary>
        /// 아이디 기억 체크박스를 체크하거나 해제하면 실행됩니다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRememberAccount(object sender, EventArgs e) => m_ChkAutoLogin.Visible = m_ChkRemember.Checked;

        private void OnGotoRegister(object sender, LinkLabelLinkClickedEventArgs e)
            => Process.Start(Program.g_RegisterUri.ToString());

        private void OnGotoLostPassword(object sender, LinkLabelLinkClickedEventArgs e)
            => Process.Start(Program.g_LostPasswordUri.ToString());

        private void OnGotoReadAgreements(object sender, LinkLabelLinkClickedEventArgs e)
            => Process.Start(Program.g_ReadAgrementsUri.ToString());

        /// <summary>
        /// 로그인을 수행합니다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPerformLogin(object sender, EventArgs e)
        {
            FrmMain Main = this.GetParent<FrmMain>();

            if (Main != null)
            {
                if (string.IsNullOrEmpty(m_TxtIdentifier.Text) ||
                    string.IsNullOrWhiteSpace(m_TxtIdentifier.Text) ||
                    m_TxtIdentifier.Text.Length <= 4)
                {
                    MessageBox.Show("이메일 혹은 전화번호가 너무 짧습니다!", "오류",
                         MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    return;
                }

                if (string.IsNullOrEmpty(m_TxtPassword.Text) ||
                    string.IsNullOrWhiteSpace(m_TxtPassword.Text) ||
                    m_TxtPassword.Text.Length <= 4)
                {
                    MessageBox.Show("패스워드가 너무 짧습니다!", "오류",
                         MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    return;
                }

                if (m_ChkRemember.Checked)
                {
                    // 패스워드는 저장하지 않습니다.
                    Main.StoredCredential = new GenericCredential() { Identifier = m_TxtIdentifier.Text };
                }
                else Main.StoredCredential = null;

                if (Main.BeginAuthenticate(new GenericCredential()
                    {
                        Identifier = m_TxtIdentifier.Text,
                        Password = m_TxtPassword.Text
                    }))
                {
                    m_LoginBox.Enabled = false;
                }
            }
        }

        /// <summary>
        /// 인증에 성공하거나 시도할 때 발생되는 이벤트입니다.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="operation"></param>
        /// <param name="errorCode"></param>
        private void OnAuthenticated(Session session, Session.Auth.Operation operation, SessionError errorCode)
        {
            FrmMain Main = this.GetParent<FrmMain>();

            if (Main != null)
            {
                Application.Tasks.Invoke(() =>
                {
                    if (session.Authentication.Credential != null &&
                        m_ChkAutoLogin.Checked)
                    {
                        // 자동로그인인 경우에만 저장합니다.
                        Main.StoredCredential = session.Authentication.RestorationCredential;
                    }

                    m_LoginBox.Enabled = true;
                });
            }
        }
    }
}
