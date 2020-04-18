using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.UI.CefUnity
{
    public enum CefDialogType
    {
        Alert,
        Confirm,
        Prompt
    }

    public class CefDialogEventArgs : CefEventArgs
    {
        private Action m_Positive;
        private Action m_Negative;
        private Action<string> m_PositiveWithInput;

        private bool m_State;

        /// <summary>
        /// CEF 다이얼로그 이벤트입니다.
        /// </summary>
        /// <param name="Screen"></param>
        /// <param name="DialogType"></param>
        /// <param name="Callback"></param>
        internal CefDialogEventArgs(CefScreen Screen,
            CefDialogType DialogType, IJsDialogCallback Callback,
            string Message, string DefaultUserInput = null) 
            : base(Screen)
        {
            m_State = false;

            if (DefaultUserInput != null)
                m_Positive = () => Callback.Continue(true, DefaultUserInput);

            else m_Positive = () => Callback.Continue(true);

            m_Negative = () => Callback.Continue(false);
            m_PositiveWithInput = (X) => Callback.Continue(true, X);

            this.Message = Message;
            this.DialogType = DialogType;
        }

        /// <summary>
        /// 이 이벤트가 처리되었는지 검사합니다.
        /// </summary>
        public bool IsHandled => this.Locked(() => m_State);

        /// <summary>
        /// 다이얼로그 타입입니다.
        /// </summary>
        public CefDialogType DialogType { get; private set; }

        /// <summary>
        /// 다이얼로그를 요청하면서 전달된 메시지입니다.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// 긍정합니다.
        /// </summary>
        public void Positive()
        {
            lock(this)
            {
                if (m_State)
                    throw new InvalidOperationException();

                m_State = true;
            }

            m_Positive?.Invoke();
        }

        /// <summary>
        /// 부정합니다.
        /// </summary>
        public void Negative()
        {
            lock (this)
            {
                if (m_State)
                    throw new InvalidOperationException();

                m_State = true;
            }

            m_Negative?.Invoke();
        }

        /// <summary>
        /// 긍정하되, 사용자 입력을 첨부합니다.
        /// </summary>
        /// <param name="userInput"></param>
        public void Positive(string userInput)
        {
            lock (this)
            {
                if (m_State)
                    throw new InvalidOperationException();

                m_State = true;
            }

            m_PositiveWithInput?.Invoke(userInput);
        }
    }
}
