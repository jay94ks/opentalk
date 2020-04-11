using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenTalk.UI.Extensions
{
    public static class ControlActs
    {
        /// <summary>
        /// 지정된 타입의 부모 컨트롤을 획득합니다.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="parentType"></param>
        /// <returns></returns>
        public static Control GetParent(this Control control, Type parentType, bool includeSubclass = false)
        {
            Control Parent = control.Parent;

            while (Parent != null)
            {
                if (Parent.GetType() == parentType || (includeSubclass &&
                    Parent.GetType().IsSubclassOf(parentType)))
                    return Parent;

                Parent = Parent.Parent;
            }

            return null;
        }

        /// <summary>
        /// 지정된 타입의 부모 컨트롤을 획득합니다.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="parentType"></param>
        /// <returns></returns>
        public static ControlType GetParent<ControlType>(this Control control, bool includeSubclass = false)
            where ControlType : Control
        {
            Control outControl = control.GetParent(typeof(ControlType), includeSubclass);

            if (outControl != null)
                return (ControlType)outControl;

            return default(ControlType);
        }
    }
}
