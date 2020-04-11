using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenTalk.UI
{
    public class TextBoxWithPlaceholder : TextBox
    {
        private char m_PasswordChar = '\0';
        private Color m_Color;

        private string m_Placeholder = "";

        /// <summary>
        /// 플레이스 홀더입니다.
        /// </summary>
        public string Placeholder {
            get => m_Placeholder;
            set {
                m_Placeholder = value;
                Text = "";
                OnLostFocus(EventArgs.Empty);
            }
        }

        /// <summary>
        /// 이 텍스트 박스가 포커스를 얻으면 플레이스 홀더를 제거하고 
        /// 입력 준비를 합니다.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnGotFocus(EventArgs e)
        {
            PasswordChar = m_PasswordChar;
            ForeColor = m_Color;

            if (base.Text == Placeholder)
            {
                Text = "";
                Invalidate();
            }

            base.OnGotFocus(e);
        }

        /// <summary>
        /// 이 텍스트 박스가 포커스를 잃었을 때,
        /// 텍스트 박스가 빈칸이면 플레이스 홀더를 채웁니다.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLostFocus(EventArgs e)
        {
            if (string.IsNullOrEmpty(Text) ||
                string.IsNullOrWhiteSpace(Text))
            {
                m_PasswordChar = PasswordChar;
                m_Color = ForeColor;

                PasswordChar = '\0';
                base.Text = Placeholder;

                ForeColor = Color.DarkGray;
            }

            else if (base.Text == Placeholder)
            {
                PasswordChar = '\0';
                ForeColor = Color.DarkGray;
            }

            base.OnLostFocus(e);
        }
    }
}
