using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace OpenTalk.Server.MySQLs
{
    public static class MySQLTools
    {
        /// <summary>
        /// Sql 문자열을 포멧팅합니다.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string FormatSql(this string format, params object[] values)
        {
            for(int i = 0; i < values.Length; i++)
            {
                if (values[i] != null && values[i].GetType() == typeof(DateTime))
                {
                    DateTime dateTime = (DateTime)values[i];
                    values[i] = string.Format(
                        "{0:0000}-{1:00}-{2:00} {3:00}:{4:00}:{5:00}",
                        dateTime.Year, dateTime.Month, dateTime.Day,
                        dateTime.Hour, dateTime.Minute, dateTime.Second);
                }

                if (values[i] != null)
                    values[i] = MySqlHelper.EscapeString(values[i].ToString());
            }

            return values.Length > 0 ? string.Format(format, values) : format;
        }

        /// <summary>
        /// 지정된 커넥션과 테이블 타입을 이용, 레코드들을 읽어옵니다.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tableType"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static object[] SelectRecords(this MySqlConnection connection, Type tableType, string sql, int limits = -1)
        {
            MySqlCommand command = new MySqlCommand(sql, connection);
            List<object> records = new List<object>();

            FieldInfo[] fields = tableType.GetFields();
            PropertyInfo[] properties = tableType.GetProperties();

            MySqlDataReader reader = command.ExecuteReader();
            ConstructorInfo ctor = tableType.GetConstructor(Type.EmptyTypes);

            string[] fieldNames = new string[reader.FieldCount];

            for (int i = 0; i < reader.FieldCount; i++)
                fieldNames[i] = reader.GetName(i);

            while (reader.Read())
            {
                object record = ctor.Invoke(new object[0]);
                records.Add(record);

                FillRecordFromReader(tableType, reader, fieldNames, record);

                if (limits == 0) break;
                else if (limits > 0) limits--;
            }

            reader.Close();
            return records.ToArray();
        }

        /// <summary>
        /// 지정된 커넥션과 테이블 타입을 이용, 레코드 하나를 읽어옵니다.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tableType"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static object SelectRecord(this MySqlConnection connection, Type tableType, string sql)
        {
            object[] objects = SelectRecords(connection, tableType, sql, 1);

            if (objects.Length > 0)
                return objects[0];

            return null;
        }

        /// <summary>
        /// 지정된 커넥션과 테이블 타입을 이용, 레코드 하나를 읽어옵니다.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tableType"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static RecordType SelectRecord<RecordType>(this MySqlConnection connection, string sql)
        {
            object record = SelectRecord(connection, typeof(RecordType), sql);

            if (record != null)
                return (RecordType)record;

            return default(RecordType);
        }

        /// <summary>
        /// 지정된 커넥션과 테이블 타입을 이용, 레코드들을 읽어옵니다.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tableType"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static RecordType[] SelectRecords<RecordType>(this MySqlConnection connection, string sql, int limits = -1)
        {
            MySqlCommand command = new MySqlCommand(sql, connection);
            List<RecordType> records = new List<RecordType>();
            Type tableType = typeof(RecordType);

            FieldInfo[] fields = tableType.GetFields();
            PropertyInfo[] properties = tableType.GetProperties();

            MySqlDataReader reader = command.ExecuteReader();
            ConstructorInfo ctor = tableType.GetConstructor(Type.EmptyTypes);

            string[] fieldNames = new string[reader.FieldCount];

            for (int i = 0; i < reader.FieldCount; i++)
                fieldNames[i] = reader.GetName(i);

            while (reader.Read())
            {
                object record = ctor.Invoke(new object[0]);
                records.Add((RecordType) record);

                FillRecordFromReader(tableType, reader, fieldNames, record);

                if (limits == 0) break;
                else if (limits > 0) limits--;
            }

            reader.Close();
            return records.ToArray();
        }

        /// <summary>
        /// 레코드 데이터를 갱신합니다.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="table"></param>
        /// <param name="record"></param>
        /// <param name="scheme"></param>
        public static bool UpdateRecord(this MySqlConnection connection, string table, object record, string scheme = null)
        {
            Type recordType = record.GetType();

            List<KeyValuePair<int, string>> fields = new List<KeyValuePair<int, string>>();
            List<KeyValuePair<int, MemberInfo>> members = new List<KeyValuePair<int, MemberInfo>>();

            string primaryKey = null;
            string selector = null;
            string sql = "UPDATE ";

            CollectRecordFields(recordType, fields, members);

            if (scheme != null)
                sql += "`" + scheme + "`.";

            sql += "`" + table + "` SET ";

            for (int i = 0; i < members.Count; i++)
            {
                var attr = members[i].Value.GetCustomAttribute<MySQLColumnAttribute>();
                object value = null;

                if (attr.IsPrimaryKey)
                {
                    primaryKey = fields[i].Value;

                    if (members[i].Value is FieldInfo)
                        value = (members[i].Value as FieldInfo).GetValue(record);

                    else if (members[i].Value is PropertyInfo)
                        value = (members[i].Value as PropertyInfo).GetValue(record);

                    if (value != null)
                    {
                        if (attr.Stringifier != null)
                            selector = ((IMySQLColumnStringifier)attr.Stringifier
                                .GetConstructor(Type.EmptyTypes).Invoke(new object[0]))
                                .Stringify(value);

                        else
                        {
                            selector = value.ToString();
                        }
                    }
                }
                else
                {
                    if (value != null)
                    {
                        if (attr.Stringifier != null)
                            value = ((IMySQLColumnStringifier)attr.Stringifier
                                .GetConstructor(Type.EmptyTypes).Invoke(new object[0]))
                                .Stringify(value);

                        else
                        {
                            value = value.ToString();
                        }

                        value = "'" + MySqlHelper.EscapeString((string)value) + "'";
                    }

                    else value = "NULL";


                    sql += "`" + attr.Name + "` = " + value.ToString() + ",";
                }
            }

            if (selector == null)
                throw new NotSupportedException();

            sql = sql.TrimEnd(',') + " WHERE `" + primaryKey + "` = '" + MySqlHelper.EscapeString(selector) + "' LIMIT 1";
            return (new MySqlCommand(sql, connection)).ExecuteNonQuery() > 0;
        }

        /// <summary>
        /// 레코드 데이터를 갱신합니다.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="table"></param>
        /// <param name="record"></param>
        /// <param name="scheme"></param>
        public static bool DeleteRecord(this MySqlConnection connection, string table, object record, string scheme = null)
        {
            Type recordType = record.GetType();

            List<KeyValuePair<int, string>> fields = new List<KeyValuePair<int, string>>();
            List<KeyValuePair<int, MemberInfo>> members = new List<KeyValuePair<int, MemberInfo>>();

            string primaryKey = null;
            string selector = null;
            string sql = "DELETE FROM ";

            CollectRecordFields(recordType, fields, members);

            if (scheme != null)
                sql += "`" + scheme + "`.";

            sql += "`" + table + "` ";

            for (int i = 0; i < members.Count; i++)
            {
                var attr = members[i].Value.GetCustomAttribute<MySQLColumnAttribute>();
                object value = null;

                if (attr.IsPrimaryKey)
                {
                    primaryKey = fields[i].Value;

                    if (members[i].Value is FieldInfo)
                        value = (members[i].Value as FieldInfo).GetValue(record);

                    else if (members[i].Value is PropertyInfo)
                        value = (members[i].Value as PropertyInfo).GetValue(record);

                    if (value != null)
                    {
                        if (attr.Stringifier != null)
                            selector = ((IMySQLColumnStringifier)attr.Stringifier
                                .GetConstructor(Type.EmptyTypes).Invoke(new object[0]))
                                .Stringify(value);

                        else
                        {
                            selector = value.ToString();
                        }
                    }

                    break;
                }
            }

            if (selector == null)
                throw new NotSupportedException();

            sql += "WHERE `" + primaryKey + "` = '" + MySqlHelper.EscapeString(selector) + "' LIMIT 1";
            return (new MySqlCommand(sql, connection)).ExecuteNonQuery() > 0;
        }
        /// <summary>
        /// 지정된 레코드 객체를 지정된 테이블에 삽입하고 Last-Insert ID를 반환합니다.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="table"></param>
        /// <param name="record"></param>
        /// <returns></returns>
        public static long InsertRecord(this MySqlConnection connection, string table, object record, string scheme = null)
        {
            Type recordType = record.GetType();

            List<KeyValuePair<int, string>> fields = new List<KeyValuePair<int, string>>();
            List<KeyValuePair<int, MemberInfo>> members = new List<KeyValuePair<int, MemberInfo>>();

            MySqlCommand command = null;
            string sql = "INSERT INTO ";

            CollectRecordFields(recordType, fields, members);

            if (scheme != null)
                sql += "`" + scheme + "`.";

            sql += "`" + table + "` (";

            foreach (var kv in fields)
            {
                if (sql.EndsWith("`"))
                    sql += ", ";

                sql += "`" + kv.Value + "`";
            }

            sql += ") VALUES (";

            foreach (var kv in members)
            {
                object value = null;

                if (kv.Value is PropertyInfo)
                    value = (kv.Value as PropertyInfo).GetValue(record);

                else if (kv.Value is FieldInfo)
                    value = (kv.Value as FieldInfo).GetValue(record);

                Type stringifier = kv.Value.GetCustomAttribute
                    <MySQLColumnAttribute>().Stringifier;

                if (sql.EndsWith("'") || sql.EndsWith("NULL"))
                    sql += ", ";

                if (value == null)
                    sql += "NULL";

                else
                {
                    sql += "'";

                    if (stringifier != null)
                    {
                        MySqlHelper.EscapeString(((IMySQLColumnStringifier)stringifier
                            .GetConstructor(Type.EmptyTypes).Invoke(new object[0]))
                            .Stringify(value));
                    }
                    else
                    {
                        sql += kv.Value.ToString();
                    }

                    sql += "'";
                }
            }

            sql += ")";

            try { (command = new MySqlCommand(sql)).ExecuteNonQuery(); }
            catch { return -1; }

            return command.LastInsertedId;
        }

        /// <summary>
        /// 지정된 레코드 객체들을 지정된 테이블에 삽입합니다.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="table"></param>
        /// <param name="record"></param>
        /// <returns></returns>
        public static bool InsertRecords<RecordType>(this MySqlConnection connection, string table, RecordType[] record, string scheme = null)
        {
            if (record.Length <= 0)
                return true;

            Type recordType = typeof(RecordType);

            List<KeyValuePair<int, string>> fields = new List<KeyValuePair<int, string>>();
            List<KeyValuePair<int, MemberInfo>> members = new List<KeyValuePair<int, MemberInfo>>();

            MySqlCommand command = null;
            string sql = "INSERT INTO ";

            CollectRecordFields(recordType, fields, members);

            if (scheme != null)
                sql += "`" + scheme + "`.";

            sql += "`" + table + "` (";

            foreach (var kv in fields)
            {
                if (sql.EndsWith("`"))
                    sql += ", ";

                sql += "`" + kv.Value + "`";
            }

            sql += ") VALUES ";

            for (int i = 0; i < record.Length; i++)
            {
                if (sql.EndsWith(")"))
                    sql += ", ";

                sql += "(";

                foreach (var kv in members)
                {
                    object value = null;

                    if (kv.Value is PropertyInfo)
                        value = (kv.Value as PropertyInfo).GetValue(record[i]);

                    else if (kv.Value is FieldInfo)
                        value = (kv.Value as FieldInfo).GetValue(record[i]);

                    Type stringifier = kv.Value.GetCustomAttribute
                        <MySQLColumnAttribute>().Stringifier;

                    if (sql.EndsWith("'") || sql.EndsWith("NULL"))
                        sql += ", ";

                    if (value == null)
                        sql += "NULL";

                    else
                    {
                        sql += "'";

                        if (stringifier != null)
                        {
                            MySqlHelper.EscapeString(((IMySQLColumnStringifier)stringifier
                                .GetConstructor(Type.EmptyTypes).Invoke(new object[0]))
                                .Stringify(value));
                        }
                        else
                        {
                            sql += kv.Value.ToString();
                        }

                        sql += "'";
                    }
                }

                sql += ")";
            }

            try { (command = new MySqlCommand(sql)).ExecuteNonQuery(); }
            catch { return false; }

            return true;
        }


        /// <summary>
        /// 레코드 내에서 필드정보를 추출합니다.
        /// </summary>
        /// <param name="recordType"></param>
        /// <param name="fields"></param>
        /// <param name="members"></param>
        private static void CollectRecordFields(Type recordType, List<KeyValuePair<int, string>> fields, List<KeyValuePair<int, MemberInfo>> members)
        {
            foreach (MemberInfo member in recordType.GetMembers())
            {
                try
                {
                    MySQLColumnAttribute columnInfo = member.GetCustomAttribute<MySQLColumnAttribute>();

                    if (columnInfo != null)
                    {
                        fields.Add(new KeyValuePair<int, string>(columnInfo.Order, columnInfo.Name));
                        members.Add(new KeyValuePair<int, MemberInfo>(columnInfo.Order, member));
                    }
                }
                catch { }
            }

            fields.Sort((X, Y) => X.Key - Y.Key);
            members.Sort((X, Y) => X.Key - Y.Key);
        }

        /// <summary>
        /// 데이터 리더가 가르키는 현재 데이터로 지정된 레코드 객체를 채웁니다.
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="reader"></param>
        /// <param name="fieldNames"></param>
        /// <param name="record"></param>
        private static void FillRecordFromReader(Type tableType, MySqlDataReader reader, string[] fieldNames, object record)
        {
            for (int i = 0; i < fieldNames.Length; i++)
            {
                FieldInfo field = null;
                PropertyInfo property = null;
                Action<object, object> setter = null;
                object value = reader.GetValue(i);

                try
                {
                    field = FindColumnField(fieldNames[i], tableType);
                    setter = (X, Y) =>
                    {
                        Y = ParseInputAsString(Y, field, field.FieldType);
                        field.SetValue(X, Y);
                    };
                }
                catch
                {
                    try
                    {
                        property = FindColumnProperty(fieldNames[i], tableType);
                        setter = (X, Y) =>
                        {
                            Y = ParseInputAsString(Y, property, property.PropertyType);
                            property.SetValue(X, Y);
                        };
                    }
                    catch
                    {
                        continue;
                    }
                }

                if (setter != null)
                {
                    try
                    {
                        setter(record, value);
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// 지정된 테이블 타입에서 지정된 컬럼이름에 해당되는 필드를 획득합니다.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="tableType"></param>
        /// <returns></returns>
        private static FieldInfo FindColumnField(string columnName, Type tableType)
        {
            FieldInfo[] fields = tableType.GetFields();

            foreach (FieldInfo field in fields)
            {
                try
                {
                    MySQLColumnAttribute columnInfo = field.GetCustomAttribute<MySQLColumnAttribute>();

                    if (columnInfo != null && columnInfo.Name == columnName)
                        return field;
                }
                catch { }
            }

            throw new Exception();
        }

        /// <summary>
        /// 지정된 테이블 타입에서 지정된 컬럼이름에 해당되는 속성을 획득합니다.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="tableType"></param>
        /// <returns></returns>
        private static PropertyInfo FindColumnProperty(string columnName, Type tableType)
        {
            PropertyInfo[] properties = tableType.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                try
                {
                    MySQLColumnAttribute columnInfo = property.GetCustomAttribute<MySQLColumnAttribute>();

                    if (columnInfo != null && columnInfo.Name == columnName)
                        return property;
                }
                catch { }
            }

            throw new Exception();
        }

        /// <summary>
        /// 입력 객체를 지정된 타입에 맞춰서 변환합니다.
        /// 변환기가 지정되지 않았을 땐 원본 그대로 지정됩니다.
        /// </summary>
        /// <param name="Y"></param>
        /// <param name="member"></param>
        /// <param name="preferedType"></param>
        /// <returns></returns>
        private static object ParseInputAsString(object Y, MemberInfo member, Type preferedType)
        {
            Type parserType = member.GetCustomAttribute<MySQLColumnAttribute>().Parser;

            if (parserType != null)
            {
                Y = ((IMySQLColumnParser)parserType.GetConstructor(Type.EmptyTypes).Invoke(new object[0]))
                    .Parse(Y.ToString(), preferedType);
            }

            return Y;
        }
    }
}
