using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTalk.Server.MySQLs
{
    public class BooleanAsYN : IMySQLColumnParser, IMySQLColumnStringifier
    {
        public object Parse(string Value, Type PreferedType)
        {
            if (string.IsNullOrEmpty(Value) ||
                string.IsNullOrWhiteSpace(Value))
                return Convert.ChangeType(false, PreferedType);

            return Convert.ChangeType(Value.ToLower() == "y", PreferedType);
        }

        public string Stringify(object Value)
        {
            if (Value == null)
                return "N";

            if (Value is bool)
                return (bool)Value ? "Y" : "N";

            return Value.ToString().Length > 0 ? "Y" : "N";
        }
    }
}
