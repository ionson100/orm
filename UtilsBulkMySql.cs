using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace ORM_1_21_
{
  internal  class UtilsBulkMySql
    {
        public static string GetSql<T>(IEnumerable<T> list, string fileCsv, string fieldterminator)
        {
            if (fileCsv != null)
            {
                return SqlFile(list, fileCsv, fieldterminator);
            }
            return SqlSimple(list);
        }

        private static string SqlFile<T>(IEnumerable<T> list, string fileCsv, string fieldterminator)
        {
            StringBuilder sql=new StringBuilder($"LOAD DATA INFILE '{fileCsv}'");
            sql.AppendLine($"INTO TABLE {AttributesOfClass<T>.TableName}");
            sql.AppendLine($"FIELDS TERMINATED BY '{fieldterminator}'");
            sql.AppendLine("ENCLOSED BY '\"'");
            sql.AppendLine("LINES TERMINATED BY '\n'");
            sql.AppendLine("IGNORE 1 ROWS");
            StringBuilder builder = new StringBuilder();
            bool isAddPk = AttributesOfClass<T>.PkAttribute.Generator != Generator.Native;

            StringBuilder rowHead = new StringBuilder();
            if (isAddPk)
            {
                rowHead.Append(AttributesOfClass<T>.PkAttribute.ColumnName)
                    .Append(";");
            }
            foreach (var map in AttributesOfClass<T>.CurrentTableAttributeDall)
            {
                rowHead.Append(map.ColumnName).Append(";");
            }

            builder.Append(rowHead.ToString().Substring(0, rowHead.ToString().LastIndexOf(';')))
                .Append(Environment.NewLine);



            foreach (var ob in list)
            {
                StringBuilder row = new StringBuilder();
                if (isAddPk)
                {
                    var o = AttributesOfClass<T>.GetValue.Value[AttributesOfClass<T>.PkAttribute.PropertyName](ob);
                    Type type = AttributesOfClass<T>.PropertyInfoList.Value[AttributesOfClass<T>.PkAttribute.PropertyName].PropertyType;
                    row.Append(GetValue(o, type)).Append(";");
                }
                foreach (var map in AttributesOfClass<T>.CurrentTableAttributeDall)
                {
                    var o = AttributesOfClass<T>.GetValue.Value[map.PropertyName](ob);
                    Type type = AttributesOfClass<T>.PropertyInfoList.Value[map.PropertyName].PropertyType;
                    if (type == typeof(Image) || type == typeof(byte[]))
                    {
                        continue;
                    }
                    string str = GetValue(o, type);
                    row.Append(str).Append(";");
                }

                string s = row.ToString().Substring(0, row.ToString().LastIndexOf(";", StringComparison.Ordinal)) + "\n";
                builder.Append(s);
            }
            File.WriteAllText(fileCsv, builder.ToString());
           

            return sql.ToString();
        }

        private static string SqlSimple<T>(IEnumerable<T> list)
        {
            StringBuilder builder=new StringBuilder($"INSERT INTO {AttributesOfClass<T>.TableName}");
            builder.Append(" ( ");

            bool isAddPk = AttributesOfClass<T>.PkAttribute.Generator != Generator.Native;

            StringBuilder rowHead = new StringBuilder();
            if (isAddPk == true)
            {
                rowHead.Append(AttributesOfClass<T>.PkAttribute.ColumnName)
                    .Append(",");
            }
            foreach (var map in AttributesOfClass<T>.CurrentTableAttributeDall)
            {
               
                if (map.TypeColumn == typeof(Image) || map.TypeColumn == typeof(byte[]))
                {
                    continue;
                }
                rowHead.Append(map.ColumnName).Append(",");
            }

            builder.Append(rowHead.ToString().Substring(0, rowHead.ToString().LastIndexOf(",", StringComparison.Ordinal))).Append(") VALUES");
            foreach (var ob in list)
            {
                StringBuilder row = new StringBuilder("(");
                if (isAddPk)
                {
                    var o = AttributesOfClass<T>.GetValue.Value[AttributesOfClass<T>.PkAttribute.PropertyName](ob);
                    Type type = AttributesOfClass<T>.PropertyInfoList.Value[AttributesOfClass<T>.PkAttribute.PropertyName].PropertyType;
                    row.Append(GetValue(o, type)).Append(",");
                }
                foreach (var map in AttributesOfClass<T>.CurrentTableAttributeDall)
                {
                    var o = AttributesOfClass<T>.GetValue.Value[map.PropertyName](ob);
                    Type type = AttributesOfClass<T>.PropertyInfoList.Value[map.PropertyName].PropertyType;
                    if (type == typeof(Image) || type == typeof(byte[]))
                    {
                        continue;
                    }
                    string str = GetValue(o, type);
                    row.Append(str).Append(",");
                }

                builder.AppendLine(row.ToString().Substring(0, row.ToString().LastIndexOf(',')) + "),");
            }
            var res = builder.ToString().Substring(0, builder.ToString().LastIndexOf(','));
           // File.WriteAllText("c:/999999999.txt",res);
            return res;
        }

        public static string GetValue(object o, Type type)
        {
            if (o == null)
            {
                return "null";
            }

          
            if (type == typeof(int)
                || type == typeof(decimal)
                || type == typeof(decimal)
                || type == typeof(long)
                || type == typeof(double)
                || type == typeof(float)
                || type == typeof(uint) 
                || type == typeof(sbyte)
                || type == typeof(short)
                || type == typeof(int?)
                || type == typeof(long?)
                || type == typeof(double?)
                || type == typeof(float?)
                || type == typeof(uint?)
                || type == typeof(sbyte?)
                || type == typeof(short?))
            {
                return o.ToString().Replace(",",".");
            }

            if (type == typeof(DateTime)|| type == typeof(DateTime?))
            {
                return $"'{((DateTime)o).ToString("yyyy-MM-dd HH:mm:ss.fff")}'";
            }

            if (type == typeof(System.Drawing.Image))
            {
                return "null";
            }

            if (type.IsEnum)
            {
                return Convert.ToInt32(o).ToString();
            }

            if (type == typeof(bool?) || type == typeof(bool))
            {
                if (Configure.Provider == ProviderName.Postgresql)
                {
                    return o.ToString();
                }
                bool v = Convert.ToBoolean(o);
                return v ? 0.ToString() : 1.ToString();
            }

            if (Utils.IsJsonType(type))
            {
                switch (Configure.Provider)
                {
                    case ProviderName.MsSql:
                        return $"'{Utils.ObjectToJson(o).Replace("'", "''")}'";
                    case ProviderName.MySql:
                        return $"'{Utils.ObjectToJson(o).Replace("\\","\\\\").Replace("'","''")}'";     
                    case ProviderName.Postgresql:
                        return $"'{Utils.ObjectToJson(o).Replace("'", "''")}'";
                    case ProviderName.Sqlite:
                        return $"'{Utils.ObjectToJson(o).Replace("'", "''")}'";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
               
            }

            switch (Configure.Provider)
            {
                case ProviderName.MsSql:
                    return $"'{o.ToString().Replace("'", "''")}'";
                case ProviderName.MySql:
                    return $"\"{o.ToString().Replace("\"", "\\\"")}\"";
                case ProviderName.Postgresql:
                    return $"'{o.ToString().Replace("'", "''")}'";
                case ProviderName.Sqlite:
                    return $"'{o.ToString().Replace("'", "''")}'";
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }

        public static string InsertFile<T>(string fileCsv, string fieldterminator)
        {
            StringBuilder sql = new StringBuilder($"LOAD DATA INFILE '{fileCsv}'");
            sql.AppendLine($"INTO TABLE {AttributesOfClass<T>.TableName}");
            sql.AppendLine($"FIELDS TERMINATED BY '{fieldterminator}'");
            sql.AppendLine("ENCLOSED BY '\"'");
            sql.AppendLine("LINES TERMINATED BY '\n'");
            sql.AppendLine("IGNORE 1 ROWS");
            return sql.ToString();
        }
    }
}
