using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.IO
{
    public sealed class PosixStyles
    {
        public static bool Mkdir(string Path)
        {
            try
            {
                if (!Directory.Exists(Path))
                {
                    Directory.CreateDirectory(Path);
                    return true;
                }
            }
            catch { }
            return false;
        }
    }
}
