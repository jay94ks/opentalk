using OpenTalk.Themes;
using OpenTalk.UI.CefUnity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace OpenTalk.Main.Renderers
{
    class MainPage : CefContentRenderer
    {
        private ScrMain m_Main;

        public MainPage(ScrMain scrMain)
        {
            m_Main = scrMain;
        }

        public override CefContent Render(string Method, 
            string QueryString, bool AllowCache, bool OnlyFromCache)
        {
            switch(QueryString)
            {
                case "myinfo": /* 내 프로필 */
                    return FromText(RenderProfileListItem(
                        "", "테스트", "테스트입니다.", "test"));

                case "friends": /* 친구 목록. */
                    return FromText(RenderProfileListItem(
                        "", "테스트", "테스트입니다.", "test"));

                case "favorates": /* 즐겨찾기. */
                    return FromText(RenderProfileListItem(
                        "", "테스트", "테스트입니다.", "test"));

                case "":
                case null:
                    break;
            }

            return FromFile(new FileInfo(Path.Combine(
                Theme.CurrentTheme.Directory.FullName, "Main.html")));
        }

        /// <summary>
        /// 개별 프로필 아이템을 렌더링합니다.
        /// </summary>
        /// <param name="Icon"></param>
        /// <param name="Name"></param>
        /// <param name="Text"></param>
        /// <param name="Identifier"></param>
        /// <returns></returns>
        public static string RenderProfileListItem(string Icon, 
            string Name, string Text, string Identifier)
        {
            string Fmt = File.ReadAllText(Path.Combine(
                Theme.CurrentTheme.Directory.FullName, 
                "parts", "main", "each-profile.html"));
            
            return string.Format(Fmt, Icon, 
                HttpUtility.HtmlEncode(Name), 
                HttpUtility.HtmlEncode(Text), 
                Identifier);
        }
    }
}
