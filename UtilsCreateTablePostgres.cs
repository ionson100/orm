using System;
using System.Drawing;
using System.Text;

namespace ORM_1_21_
{
    internal class UtilsCreateTablePostgres
    {
        public static string Create<T>()
        {
            var builder = new StringBuilder();

            var tableName = AttributesOfClass<T>.TableName;
            tableName = Utils.ClearTrim(tableName);
            builder.AppendLine($"CREATE TABLE IF NOT EXISTS \"{tableName}\" (");
            var pk = AttributesOfClass<T>.PkAttribute;


            builder.AppendLine(
                $" \"{Utils.ClearTrim(pk.ColumnNameForRider)}\" {GetTypePgPk(pk.TypeColumn, pk.Generator)}  PRIMARY KEY,");
            foreach (var map in AttributesOfClass<T>.CurrentTableAttributeDall)
            {
                if (map.TypeString == null)
                {
                    builder.AppendLine(

                        $" \"{Utils.ClearTrim(map.ColumnNameForReader)}\" {GetTypePg(map.TypeColumn)} {FactoryGreaterTable.GetDefaultValue(map.DefaultValue, map.TypeColumn)} ,");
                }
                else
                {
                    builder.AppendLine(

                        $" \"{Utils.ClearTrim(map.ColumnNameForReader)}\" {map.TypeString} ,");
                }
               
            }
              


            var str2 = builder.ToString();
            str2 = str2.Substring(0, str2.LastIndexOf(','));
            builder.Clear();
            builder.Append(str2);
            builder.AppendLine(");");

            var indexBuilder = new StringBuilder();

            //var add = false;
            foreach (var map in AttributesOfClass<T>.CurrentTableAttributeDall)
                if (map.IsIndex)
                {
                    var colName = Utils.ClearTrim(map.ColumnName); //.Trim(new[] {' ', '`', '[', ']', '\''}));
                   // add = true;
                    //indexBuilder.AppendLine($"\"{colName}\"").Append(",");
                    indexBuilder.AppendLine(
                        $"CREATE INDEX IF NOT EXISTS INDEX_{tableName}_{colName} ON \"{tableName}\" (\"{colName}\");");
                }

            //if (add)
            //{
            //    var index = indexBuilder.ToString().Substring(0, indexBuilder.ToString().LastIndexOf(',')).Trim() +
            //                ");";
            //    builder.AppendLine(index);
            //}
            builder.AppendLine(indexBuilder.ToString());


            return builder.ToString();
        }


        private static string GetTypePg(Type type)
        {
            if (type == typeof(long) || type == typeof(long?)) return "BIGINT";
            if (type == typeof(int) || type.BaseType == typeof(Enum) || type == typeof(int?)) return "INTEGER";
            if (type == typeof(bool) || type == typeof(bool?)) //real
                return "BOOLEAN";
            if (type == typeof(decimal) || type == typeof(decimal?)) return "decimal";
            if (type == typeof(float) || type == typeof(float?)) return "NUMERIC";

            if (type == typeof(double) || type == typeof(double?)) return "double precision";

            if (type == typeof(DateTime) || type == typeof(DateTime?)) return "TIMESTAMP";

            if (type == typeof(Guid)) return "UUID";

            if (Utils.IsJsonType(type)) return "TEXT";

            if (type == typeof(Image) || type == typeof(byte[])) return "BYTEA";


            return "VARCHAR(256)";
        }

        private static string GetTypePgPk(Type type, Generator generator)
        {
            if (generator == Generator.Assigned)
            {
                if (type == typeof(long) || type == typeof(long?)) return "BIGINT";
                if (type == typeof(int) || type.BaseType == typeof(Enum) || type == typeof(int?)) return "INTEGER";
                if (type == typeof(Guid)) return "UUID";
            }

            if (generator == Generator.Native)
            {
                if (type == typeof(long) || type == typeof(long?)) return "BIGSERIAL";
                if (type == typeof(int) || type.BaseType == typeof(Enum) || type == typeof(int?)) return "SERIAL";
                if (type == typeof(Guid)) return "UUID";
            }

            return "NVARCHAR] (256)";
        }
    }
}