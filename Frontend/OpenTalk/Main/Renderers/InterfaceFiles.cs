using OpenTalk.Themes;
using OpenTalk.UI.CefUnity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Main.Renderers
{
    /// <summary>
    /// 일반적인 리소스 파일들을 공급하는 렌더러입니다.
    /// </summary>
    class InterfaceFiles : CefContentRenderer
    {
        private string m_PathName;

        public InterfaceFiles() => m_PathName = null;
        private InterfaceFiles(string PathName) => m_PathName = PathName;

        /// <summary>
        /// 라우팅 되었을 때 해당 경로에 맞는 렌더러를 인스턴싱합니다.
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="PathName"></param>
        /// <returns></returns>
        public override CefContentRenderer OnRouted(CefScreen screen, string PathName)
            => new InterfaceFiles(PathName);

        /// <summary>
        /// 컨텐츠를 렌더링합니다.
        /// </summary>
        /// <param name="Method"></param>
        /// <param name="QueryString"></param>
        /// <param name="AllowCache"></param>
        /// <param name="OnlyFromCache"></param>
        /// <returns></returns>
        public override CefContent Render(string Method, 
            string QueryString, bool AllowCache, bool OnlyFromCache)
        {
            while (true)
            {
                if (string.IsNullOrEmpty(m_PathName) ||
                    string.IsNullOrWhiteSpace(m_PathName))
                    break;

                string PathName = Path.Combine(
                    Application.Environments.ExecPath,
                    "common", m_PathName.Replace('/', '\\'));

                try
                {
                    if (File.Exists(PathName))
                        return FromFile(new FileInfo(PathName));
                }
                catch { }


                if (Theme.CurrentTheme != null)
                {
                    PathName = Path.Combine(
                        Theme.CurrentTheme.Directory.FullName,
                        m_PathName.Replace('/', '\\'));

                    try
                    {
                        if (File.Exists(PathName))
                            return FromFile(new FileInfo(PathName));
                    }
                    catch { }
                }
                break;
            }

            return MakeError(404, "Not Found");
        }
    }
}
