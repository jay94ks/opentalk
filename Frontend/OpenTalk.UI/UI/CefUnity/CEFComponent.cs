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
    public class CEFComponent : Application.Component
    {
        private CefSettings m_Settings;
        private static int m_ActivationCounter = 0;
        private static object m_Synchronization = new object();

        /// <summary>
        /// CefSharp 연동 컴포넌트를 초기화합니다.
        /// </summary>
        public CEFComponent() : this(null) { }
        public CEFComponent(Application application)
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
        public static CEFComponent GetCEFComponent(Application application = null, bool allowCreate = true)
        {
            application = application != null ? 
                application : Application.RunningInstance;

            if (application != null)
            {
                lock (application)
                {
                    CEFComponent component = application.GetComponent<CEFComponent>();

                    if (component == null)
                        component = new CEFComponent(application);

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
                () => {
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
                }).Wait();

            base.OnActivated();
        }

        protected override void OnDeactivated()
        {
            Application.Tasks.Invoke(
                () => {
                    lock(m_Synchronization)
                    {
                        m_ActivationCounter--;

                        if (m_ActivationCounter > 0)
                            return;
                    }

                    try { Cef.Shutdown(); }
                    catch { }
                }).Wait();

            base.OnDeactivated();
        }
    }
}
