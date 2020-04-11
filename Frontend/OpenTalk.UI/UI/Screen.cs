using OpenTalk.UI.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenTalk.UI
{
    public class Screen : UserControl
    {
        private bool m_CachedVisibility = false;

        /// <summary>
        /// 사용자 인터페이스가 보이게 되면 OnInterfaceVisible 메서드와
        /// OnInterfaceInvisible 메서드를 알맞게 호출합니다.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            /*
                Visible 속성 값에 같은 값이 제공되어도
                모두 동일하게 이벤트가 발생되는 것이 확인되어,
                캐쉬값을 두고, 캐쉬값과 새 값이 다를 경우에만 실행합니다.
             */
            if (Visible != m_CachedVisibility)
            {
                if (Visible)
                    OnNowVisible(e);

                else OnNowInvisible(e);
                m_CachedVisibility = Visible;
            }
            
            base.OnVisibleChanged(e);
        }

        /// <summary>
        /// 상위 컨트롤 중에서 스크린 전환 컨트롤을 찾아 가져옵니다.
        /// </summary>
        public ScreenSwitcher Switcher => this.GetParent<ScreenSwitcher>();

        /// <summary>
        /// 이 화면이 보이게 되면 발생하는 이벤트입니다.
        /// </summary>
        public event EventHandler NowVisible;

        /// <summary>
        /// 이 화면이 보이지 않게 되면 발생하는 이벤트입니다.
        /// </summary>
        public event EventHandler NowInvisible;

        /// <summary>
        /// 사용자 인터페이스가 보이게 되면 실행됩니다.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnNowVisible(EventArgs e) => NowVisible?.Invoke(this, e);

        /// <summary>
        /// 사용자 인터페이스가 보이지 않게 되면 실행됩니다.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnNowInvisible(EventArgs e) => NowInvisible?.Invoke(this, e);
    }
}
