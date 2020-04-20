using OpenTalk.UI.CefUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Main.Renderers
{
    class MainPage : CefContentRenderer
    {
        public override CefContent Render(string Method, 
            string QueryString, bool AllowCache, bool OnlyFromCache)
        {

            return FromText("hello!");
        }
    }
}
