using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.UI.CefUnity
{
    public class CefScreenClosingEventArgs : CefEventArgs
    {
        public CefScreenClosingEventArgs()
        {
        }

        public CefScreenClosingEventArgs(CefScreen Screen) : base(Screen)
        {
        }


    }
}
