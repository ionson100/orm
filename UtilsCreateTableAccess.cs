using System;
using System.Drawing;
using System.Text;
using ORM_1_21_.Attribute;

namespace ORM_1_21_
{
    internal class UtilsCreateTableAccess
    {
        public static string Create<T>()
        {
            var builder = new StringBuilder();

            var tableName = AttributesOfClass<T>.TableName;
            tableName = Utils.ClearTrim(tableName);
            builder.AppendLine($"CREATE TABLE  [{tableName}] (");
            var pk = AttributesOfClass<T>.PkAttribute;


            builder.AppendLine(
                $" [{Utils.ClearTrim(pk.ColumnNameForRider)}] {GetTypePgPk(pk.TypeColumn, pk.Generator)}  ,");
            foreach (var map in AttributesOfClass<T>.CurrentTableAttributeDall)
            {
                if (map.TypeString == null)
                {
                    builder.AppendLine($" [{Utils.ClearTrim(map.ColumnNameForReader)}] {GetTypePg(map.TypeColumn)} {FactoryGreaterTable.GetDefaultValue(map.DefaultValue, map.TypeColumn)} ,");
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

           
            if (type == typeof(long) || type == typeof(long?)) return "LONG";
            if (type == typeof(uint)|| type == typeof(uint?)||type == typeof(int) || type.BaseType == typeof(Enum) || type == typeof(int?)) return "NUMERIC";
            if (type == typeof(bool) || type == typeof(bool?)) return "BIT";
            if (type == typeof(decimal) || type == typeof(decimal?)) return "DECIMAL(19,4)";
            if (type == typeof(float) || type == typeof(float?)) return "NUMERIC";

            if (type == typeof(double) || type == typeof(double?)) return "DOUBLE";

            if (type == typeof(DateTime) || type == typeof(DateTime?)) 
                return "TEXT(100)";

            if (type == typeof(Guid)) return "GUID";

            if (Utils.IsJsonType(type)) return "LONGTEXT";

            if (type == typeof(Image) || type == typeof(byte[])) return "LONGBINARY";


            return "LONGTEXT";
        }

        private static string GetTypePgPk(Type type, Generator generator)
        {
            if (generator == Generator.Assigned)
            {
                if (type == typeof(long) || type == typeof(long?)) return "LONG PRIMARY KEY";
                if (type == typeof(int) || type.BaseType == typeof(Enum) || type == typeof(int?)) return "NUMERIC PRIMARY KEY ";
                if (type == typeof(Guid)) return "GUID PRIMARY KEY";
                
            }

            if (generator == Generator.Native)
            {
               return "AUTOINCREMENT PRIMARY KEY";
            }

            return "NVARCHAR] (256)";
        }
    }
}