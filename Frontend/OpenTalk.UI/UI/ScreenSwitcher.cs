using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenTalk.UI
{
    /// <summary>
    /// 스크린 전환 컨트롤입니다.
    /// </summary>
    public class ScreenSwitcher : UserControl
    {
        private Dictionary<Type, Screen> m_Screens;
        private Screen m_VisibleScreen;
        private Type[] m_ScreenTypes;
        private Type m_ReservedScreen;

        /// <summary>
        /// 스크린 전환 컨트롤을 초기화합니다.
        /// </summary>
        public ScreenSwitcher()
        {
            m_ScreenTypes = new Type[0];
            m_VisibleScreen = null;
            m_Screens = new Dictionary<Type, Screen>();
        }

        /// <summary>
        /// 이 스크린 전환 컨트롤이 취급하는 스크린 타입들입니다.
        /// </summary>
        public Type[] ScreenTypes {
            get => m_ScreenTypes;
            set {
                m_ScreenTypes = value;
                InvalidateThreadSafety();
            }
        }

        /// <summary>
        /// 현재 보여지고 있는 스크린의 타입 정보입니다.
        /// </summary>
        public Type VisibleScreenType {
            get => m_VisibleScreen != null ? m_VisibleScreen.GetType() : null;
            set => SwitchScreen(VisibleScreenType);
        }

        /// <summary>
        /// 현재 보여지고 있는 스크린입니다.
        /// </summary>
        public Screen VisibleScreen => m_VisibleScreen;

        /// <summary>
        /// 현재 보여지고 있는 스크린이 변경되면 실행됩니다.
        /// </summary>
        public event EventHandler ScreenChanged;

        /// <summary>
        /// 지정된 스크린 타입을 이 전환 컨트롤에서 취급하는지 여부를 확인합니다.
        /// </summary>
        /// <param name="screenType"></param>
        /// <returns></returns>
        public bool HasScreen(Type screenType, bool includeSubclass = false)
        {
            if (m_ScreenTypes != null)
            {
                foreach(Type EachType in m_ScreenTypes)
                {
                    if (EachType == screenType || (includeSubclass && 
                        EachType.IsSubclassOf(screenType)))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 지정된 스크린으로 뷰를 전환합니다.
        /// </summary>
        /// <param name="type"></param>
        public bool SwitchScreen(Type type)
        {
            if (type != null)
            {
                if (m_Screens.ContainsKey(type))
                {
                    if (VisibleScreenType != type)
                    {
                        if (InvokeRequired)
                            Invoke(new Action(() => SetVisibleToScreen(type)));

                        else SetVisibleToScreen(type);
                    }

                    InvalidateThreadSafety();
                    return true;
                }

                else if (HasScreen(type))
                {
                    m_ReservedScreen = type;
                    InvalidateThreadSafety();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 지정된 스크린으로 뷰를 전환합니다.
        /// </summary>
        /// <param name="type"></param>
        private void SetVisibleToScreen(Type type)
        {
            foreach (Type each in m_Screens.Keys)
            {
                if (type != each)
                {
                    m_Screens[each].Visible = false;
                }
            }

            (m_VisibleScreen = m_Screens[type])
                .Visible = true;

            ScreenChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 컨트롤이 표시되어야 할 때 내부 스크린 컨트롤들을 초기화합니다.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (m_ScreenTypes != null && m_ScreenTypes.Length > 0)
            {
                RemoveUnusedScreens();

                // 새로 추가된 스크린 타입들을 초기화합니다.
                foreach (Type EachType in m_ScreenTypes)
                {
                    if (!m_Screens.ContainsKey(EachType) &&
                        EachType.IsSubclassOf(typeof(Screen)))
                    {
                        Screen NewInstance = (Screen)EachType
                            .GetConstructor(Type.EmptyTypes).Invoke(new object[0]);

                        NewInstance.Dock = DockStyle.Fill;
                        NewInstance.Visible = false;

                        m_Screens.Add(EachType, NewInstance);
                        Controls.Add(NewInstance);
                    }
                }

                if (m_ReservedScreen != null)
                {
                    SwitchScreen(m_ReservedScreen);
                    m_ReservedScreen = null;
                }
            }

            base.OnPaint(e);
        }

        /// <summary>
        /// 표시 영역을 무효화하고 새로 그립니다. (쓰레드 안전)
        /// </summary>
        private void InvalidateThreadSafety()
        {
            if (InvokeRequired)
                Invoke(new Action(() => Invalidate()));

            else Invalidate();
        }

        /// <summary>
        /// 더이상 사용하지 않는 스크린들을 정리합니다.
        /// </summary>
        private void RemoveUnusedScreens()
        {
            Queue<Type> UnusedScreens = new Queue<Type>();

            // 현재 인스턴싱된 스크린들 중에서 더이상 사용되지 않는 스크린들을 탐색합니다.
            foreach (Type InstancedType in m_Screens.Keys)
            {
                if (!HasScreen(InstancedType))
                    UnusedScreens.Enqueue(InstancedType);
            }

            // 더이상 사용되지 않는 스크린들을 정리합니다.
            while (UnusedScreens.Count > 0)
                RemoveScreenCompletely(UnusedScreens.Dequeue());
        }

        /// <summary>
        /// 지정된 타입의 스크린을 완전히 제거합니다.
        /// </summary>
        /// <param name="ScreenType"></param>
        private void RemoveScreenCompletely(Type ScreenType)
        {
            Screen Instance = m_Screens[ScreenType];

            m_Screens.Remove(ScreenType);
            Controls.Remove(Instance);

            if (m_VisibleScreen == Instance)
            {
                m_VisibleScreen = null;
                ScreenChanged?.Invoke(this, EventArgs.Empty);
            }

            try { Instance.Dispose(); }
            catch { }
        }
    }
}
