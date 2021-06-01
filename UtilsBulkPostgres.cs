using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace ORM_1_21_
{
    internal class UtilsBulkPostgres
    {
        public static string GetSql<T>(IEnumerable<T> list, string fileCsv, string fieldterminator)
        {
            if (fileCsv != null)
                return SqlFile(list, fileCsv, fieldterminator);
            return SqlSimple(list);
        }

        private static string SqlFile<T>(IEnumerable<T> list, string fileCsv, string fieldterminator)
        {
            var sql = new StringBuilder();

            sql.Append($"COPY {AttributesOfClass<T>.TableName} FROM '{fileCsv}' DELIMITER '{fieldterminator}';");
            var builder = new StringBuilder();
            var isAddPk = AttributesOfClass<T>.PkAttribute.Generator != Generator.Native;

           

            foreach (var ob in list)
            {
                var row = new StringBuilder();
                if (isAddPk)
                {
                    var o = AttributesOfClass<T>.GetValue.Value[AttributesOfClass<T>.PkAttribute.PropertyName](ob);
                    var type = AttributesOfClass<T>.PropertyInfoList
                        .Value[AttributesOfClass<T>.PkAttribute.PropertyName].PropertyType;
                    row.Append(GetValueE(o, type)).Append($"{fieldterminator}");
                }

                foreach (var map in AttributesOfClass<T>.CurrentTableAttributeDall)
                {
                    var o = AttributesOfClass<T>.GetValue.Value[map.PropertyName](ob);
                    var type = AttributesOfClass<T>.PropertyInfoList.Value[map.PropertyName].PropertyType;
                    var str = GetValueE(o, type);
                    row.Append(str).Append($"{fieldterminator}");
                }

                var s = row.ToString()
                            .Substring(0, row.ToString().LastIndexOf(fieldterminator, StringComparison.Ordinal)) + "\n";
                builder.Append(s);
            }

            File.WriteAllText(fileCsv, builder.ToString());


            return sql.ToString();
        }

        private static string SqlSimple<T>(IEnumerable<T> list)
        {
            var builder = new StringBuilder($"INSERT INTO {AttributesOfClass<T>.TableName}");
            builder.Append(" ( ");

            var isAddPk = AttributesOfClass<T>.PkAttribute.Generator != Generator.Native;

            var rowHead = new StringBuilder();
            if (isAddPk)
                rowHead.Append($"\"{Utils.ClearTrim(AttributesOfClass<T>.PkAttribute.ColumnName)}\"").Append(",");
            foreach (var map in AttributesOfClass<T>.CurrentTableAttributeDall)
            {
                if (map.TypeColumn == typeof(Image) || map.TypeColumn == typeof(byte[])) continue;
                rowHead.Append($"\"{Utils.ClearTrim(map.ColumnName)}\"").Append(",");
            }

            builder.Append(rowHead.ToString()
                .Substring(0, rowHead.ToString().LastIndexOf(",", StringComparison.Ordinal))).Append(") VALUES");
            foreach (var ob in list)
            {
                var row = new StringBuilder("(");
                if (isAddPk)
                {
                    var o = AttributesOfClass<T>.GetValue.Value[AttributesOfClass<T>.PkAttribute.PropertyName](ob);
                    var type = AttributesOfClass<T>.PropertyInfoList
                        .Value[AttributesOfClass<T>.PkAttribute.PropertyName].PropertyType;
                    row.Append(UtilsBulkMySql.GetValue(o, type)).Append(",");
                }

                foreach (var map in AttributesOfClass<T>.CurrentTableAttributeDall)
                {
                    var o = AttributesOfClass<T>.GetValue.Value[map.PropertyName](ob);
                    var type = AttributesOfClass<T>.PropertyInfoList.Value[map.PropertyName].PropertyType;
                    if (type == typeof(Image) || type == typeof(byte[])) continue;
                    var str = UtilsBulkMySql.GetValue(o, type);
                    row.Append(str).Append(",");
                }

                builder.AppendLine(row.ToString().Substring(0, row.ToString().LastIndexOf(',')) + "),");
            }

            var res = builder.ToString().Substring(0, builder.ToString().LastIndexOf(','));
            return res;
        }

        public static string GetValueE(object o, Type type)
        {
            if (o == null) return "null";


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
                return o.ToString().Replace(",", ".");

            if (type == typeof(DateTime) || type == typeof(DateTime?))
                return $"'{((DateTime) o).ToString("yyyy-MM-dd HH:mm:ss.fff")}'";

            if (type == typeof(Image)) return "null";

            if (type.IsEnum) return Convert.ToInt32(o).ToString();

            if (type == typeof(bool?) || type == typeof(bool))
            {
                if (Configure.Provider == ProviderName.Postgresql) return o.ToString();
                var v = Convert.ToBoolean(o);
                return v ? 0.ToString() : 1.ToString();
            }

            if (Utils.IsJsonType(type)) return $"'{Utils.ObjectToJson(o)}'";
            return $"{o}";
        }

        public static string InsertFile<T>(string fileCsv, string fieldterminator)
        {
            return $"COPY {AttributesOfClass<T>.TableName} FROM '{fileCsv}' DELIMITER '{fieldterminator}';";
        }
    }
}