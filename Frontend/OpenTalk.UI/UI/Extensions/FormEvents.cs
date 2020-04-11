using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenTalk.UI.Extensions
{
    public static class FormEvents
    {
        /// <summary>
        /// 사용자가 창을 닫고 싶어 하는지 여부를 검사합니다.
        /// </summary>
        /// <param name="Self"></param>
        /// <returns></returns>
        public static bool IsUserClosing(this FormClosingEventArgs Self)
            => Self.CloseReason == CloseReason.UserClosing;

        public static void CancelAnd(this CancelEventArgs Self, Action Functor)
        {
            Self.Cancel = true;
            Functor?.Invoke();
        }
    }
}
