using CefSharp;
using CefSharp.WinForms;
using OpenTalk.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.UI.CefUnity
{
    public partial class CefComponent : Application.Component
    {
        private CefSettings m_Settings;
        private static int m_ActivationCounter = 0;
        private static object m_Synchronization = new object();
        private Dictionary<string, CefScreen> m_CefScreens
            = new Dictionary<string, CefScreen>();

        /// <summary>
        /// CefSharp 연동 컴포넌트를 초기화합니다.
        /// </summary>
        public CefComponent() : this(null) { }
        public CefComponent(Application application)
            : base(application)
        {
            m_Settings = new CefSettings();

            m_Settings.CachePath = Path.Combine(
                Application.Environments.CachePath,
                "CEF");

            m_Settings.RootCachePath = m_Settings.CachePath;
            m_Settings.UserDataPath = Path.Combine(
                Application.Environments.UserDataPath,
                "CEF");

            m_Settings.RegisterScheme(new CefCustomScheme()
            {
                IsLocal = false,
                IsCorsEnabled = true,
                SchemeName = OtkSchemeHandlerFactory.SchemeName,
                SchemeHandlerFactory = new OtkSchemeHandlerFactory(this)
            });

            // 디버거가 붙어있지 않으면 로깅 자체를 꺼버립니다.
            if (!Debugger.IsAttached)
                m_Settings.LogSeverity = LogSeverity.Disable;

            PosixStyles.Mkdir(m_Settings.CachePath);
            PosixStyles.Mkdir(m_Settings.RootCachePath);
            PosixStyles.Mkdir(m_Settings.UserDataPath);
        }

        /// <summary>
        /// 지정된 어플리케이션 인스턴스에서 CEF 컴포넌트를 획득합니다.
        /// 부착된 것이 없으면 새로 생성합니다.
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        public static CefComponent GetCEFComponent(Application application = null, bool allowCreate = true)
        {
            if (Application.FutureInstance.IsCompleted)
            {
                application = application != null ?
                    application : Application.RunningInstance;
            }

            if (application != null)
            {
                lock (application)
                {
                    CefComponent component = application.GetComponent<CefComponent>();

                    if (component == null)
                        (component = new CefComponent(application)).Activate();

                    return component;
                }
            }

            return null;
        }

        /// <summary>
        /// CEF 컴포넌트가 활성화될 때 CEF를 초기화합니다.
        /// </summary>
        protected override void OnActivated()
        {
            Application.Tasks.Invoke(
                () =>
                {
                    lock (m_Synchronization)
                    {
                        if (m_ActivationCounter > 0)
                        {
                            ++m_ActivationCounter;
                            return;
                        }

                        ++m_ActivationCounter;
                    }

                    try { Cef.Initialize(m_Settings); }
                    catch { }
                });

            base.OnActivated();
        }

        /// <summary>
        /// CEF 컴포넌트가 비활성화될 때 CEF를 종료시킵니다.
        /// </summary>
        protected override void OnDeactivated()
        {
            Application.Tasks.Invoke(
                () =>
                {
                    lock (m_Synchronization)
                    {
                        m_ActivationCounter--;

                        if (m_ActivationCounter > 0)
                            return;
                    }

                    try { Cef.Shutdown(); }
                    catch { }
                });

            base.OnDeactivated();
        }

        /// <summary>
        /// CefScreen을 등록합니다.
        /// 등록된 CefScreen들은 otk://[SCREEN-ID]/[INTERFACE_ID] 형식으로 
        /// UI 렌더링을 수행할 수 있습니다.
        /// </summary>
        /// <param name="cefScreen"></param>
        internal bool RegisterScreen(CefScreen cefScreen)
        {
            lock(m_CefScreens)
            {
                if (m_CefScreens.ContainsValue(cefScreen))
                    return false;

                while (true)
                {
                    string ScreenId = Application.Random.MakeString(32, false);

                    if (m_CefScreens.ContainsKey(ScreenId))
                        continue;

                    m_CefScreens.Add(ScreenId, cefScreen);
                    cefScreen.OnScreenRegistered(ScreenId);
                    break;
                }
            }

            return true;
        }

        internal bool UnregisterScreen(CefScreen cefScreen)
        {
            lock (m_CefScreens)
            {
                if (cefScreen.ScreenId.IsNull() ||
                    !m_CefScreens.ContainsKey(cefScreen.ScreenId))
                    return false;

                m_CefScreens.Remove(cefScreen.ScreenId);
                cefScreen.OnScreenUnregistered();
            }

            return true;
        }
    }
}
