using CefSharp;
using OpenTalk.UI.Extensions;
using System.Windows.Forms;

namespace OpenTalk.UI.CefUnity
{
    public partial class CefScreen
    {
        /// <summary>
        /// 자바스크립트 다이얼로그 핸들러를 커스터마이징합니다.
        /// </summary>
        private class JsDialogHandler : IJsDialogHandler
        {
            private CefScreen m_Master;

            /// <summary>
            /// 커스텀 핸들러를 초기화합니다.
            /// </summary>
            /// <param name="master"></param>
            public JsDialogHandler(CefScreen master) => m_Master = master;

            /// <summary>
            /// 정말 페이지를 이탈하시겠습니까? 다이얼로그.
            /// </summary>
            /// <param name="chromiumWebBrowser"></param>
            /// <param name="browser"></param>
            /// <param name="messageText"></param>
            /// <param name="isReload"></param>
            /// <param name="callback"></param>
            /// <returns></returns>
            public bool OnBeforeUnloadDialog(IWebBrowser chromiumWebBrowser, IBrowser browser,
                string messageText, bool isReload, IJsDialogCallback callback)
            {
                if (!m_Master.m_DialogEvent.Broadcast(m_Master,
                    new CefDialogEventArgs(m_Master, CefDialogType.Confirm,
                    callback, messageText)))
                {
                    Form Form = m_Master.GetParent<Form>();
                    DialogResult Result = DialogResult.Yes;
                    string Title = "CefScreen";

                    if (Form.IsNotNull())
                        Title = Form.Text;

                    Result = Application.Tasks.Invoke(() => MessageBox.Show(messageText,
                        Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                        .WaitResult();

                    if (Result == DialogResult.Yes)
                        callback.Continue(true);

                    else callback.Continue(false);
                }

                return true;
            }

            /// <summary>
            /// 자바스크립트 다이얼로그를 요청합니다.
            /// </summary>
            /// <param name="chromiumWebBrowser"></param>
            /// <param name="browser"></param>
            /// <param name="originUrl"></param>
            /// <param name="dialogType"></param>
            /// <param name="messageText"></param>
            /// <param name="defaultPromptText"></param>
            /// <param name="callback"></param>
            /// <param name="suppressMessage"></param>
            /// <returns></returns>
            public bool OnJSDialog(IWebBrowser a, IBrowser b, string c,
                CefJsDialogType dialogType, string messageText, string defaultPromptText, 
                IJsDialogCallback callback, ref bool d)
            {
                CefDialogType requestType = CefDialogType.Alert;

                switch (dialogType)
                {
                    case CefJsDialogType.Alert:
                        requestType = CefDialogType.Alert;
                        break;

                    case CefJsDialogType.Confirm:
                        requestType = CefDialogType.Confirm;
                        break;

                    case CefJsDialogType.Prompt:
                        requestType = CefDialogType.Prompt;
                        break;

                    default:
                        break;
                }

                if (string.IsNullOrEmpty(defaultPromptText) ||
                    string.IsNullOrWhiteSpace(defaultPromptText))
                    defaultPromptText = null;

                if (!m_Master.m_DialogEvent.Broadcast(m_Master,
                    new CefDialogEventArgs(m_Master, requestType,
                    callback, messageText, defaultPromptText)))
                {
                    Form Form = m_Master.GetParent<Form>(true);
                    DialogResult Result = DialogResult.Yes;
                    string Title = "CefScreen";
                    
                    if (Form.IsNotNull())
                        Title = Form.Text;

                    switch (dialogType)
                    {
                        case CefJsDialogType.Alert:
                            Result = Application.Tasks.Invoke(() => MessageBox.Show(messageText,
                                Title, MessageBoxButtons.OK, MessageBoxIcon.Question))
                                .WaitResult();
                            break;

                        case CefJsDialogType.Confirm:
                            Result = Application.Tasks.Invoke(() => MessageBox.Show(messageText,
                                Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                                .WaitResult();
                            break;

                        case CefJsDialogType.Prompt:
                            Result = DialogResult.No;
                            break;

                        default:
                            break;
                    }

                    if (Result == DialogResult.Yes ||
                        Result == DialogResult.OK)
                        callback.Continue(true);

                    else callback.Continue(false);
                }

                return true;
            }

            public void OnDialogClosed(IWebBrowser chromiumWebBrowser, IBrowser browser) { }
            public void OnResetDialogState(IWebBrowser chromiumWebBrowser, IBrowser browser) { }
        }
    }
}
