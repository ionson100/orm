using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using ORM_1_21_.Attribute;

namespace ORM_1_21_
{
  internal  class UtilsCreateTableMsSql
    {
        public static string Create<T>()
        {
            var tableName = AttributesOfClass<T>.TableName;
            tableName = tableName.Replace("[", "").Replace("]", "");
            StringBuilder builder = new StringBuilder($"IF not exists (select 1 from information_schema.tables where table_name = '{tableName}')");
            builder.AppendLine($"CREATE TABLE [dbo].[{tableName}](");
            var pk = AttributesOfClass<T>.PkAttribute;
            if (pk.Generator == Generator.Native)
            {

                builder.AppendLine($"[{pk.ColumnNameForRider}] {GetTypeMsSQl(pk.TypeColumn)} IDENTITY(1,1) NOT NULL PRIMARY KEY,");
            }
            if (pk.Generator == Generator.Assigned)
            {

                builder.AppendLine($"[{pk.ColumnNameForRider}] uniqueIdentifier default (newId()) primary key,");
            }


            foreach (MapColumnNameAttribute map in AttributesOfClass<T>.CurrentTableAttributeDall)
            {
                var typeUser = map.TypeString;
                if (typeUser == null)
                {
                    builder.AppendLine($" [{map.ColumnNameForReader}] {GetTypeMsSQl(map.TypeColumn)} {FactoryGreaterTable.GetDefaultValue(map.DefaultValue, map.TypeColumn)},");
                }
                else
                {
                    builder.AppendLine($" [{map.ColumnNameForReader}] {typeUser} {FactoryGreaterTable.GetDefaultValue(map.DefaultValue, map.TypeColumn)},");
                }
                
            }
            StringBuilder indexBuilder = new StringBuilder($"CREATE INDEX [INDEX_{tableName}] ON [{tableName}] (");

            bool add = false;
            foreach (MapColumnNameAttribute map in AttributesOfClass<T>.CurrentTableAttributeDall)
            {

                if (map.IsIndex)
                {
                    add = true;
                    indexBuilder.AppendLine(map.ColumnName).Append(",");
                }

            }
            string res = builder.ToString().Trim(new[] { ' ', ',' }) + ");";
            if (add == true)
            {
                string index = indexBuilder.ToString().Substring(0, indexBuilder.ToString().LastIndexOf(',')).Trim() + ");";

                res += Environment.NewLine + index;
            }



            return res;
        }
        private static string GetTypeMsSQl(Type type)
        {
            if (type == typeof(long) || type == typeof(long?))
            {
                return "[bigint]";
            }
            if (type == typeof(int) || type.BaseType == typeof(Enum) || type == typeof(int?))
            {
                return "[INT]";
            }
            if (type == typeof(bool) || type == typeof(bool?))//real
            {
                return "[BIT]";
            }
            if (type == typeof(decimal) || type == typeof(decimal?))
            {
                return "[decimal]";
            }
            if (type == typeof(float) || type == typeof(float?))
            {
                return "[float]";
            }

            if (type == typeof(double) || type == typeof(double?))
            {
                return "[float]";
            }

            if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                return "[DATETIME]";
            }

            if (type == typeof(Guid))
            {
                return "[uniqueidentifier]";
            }

            if (Utils.IsJsonType(type))
            {
                return "[nvarchar] (max)";
            }

            if (type == typeof(Image) || type == typeof(byte[]))
            {
                return "varbinary(MAX)";
            }
            return "[NVARCHAR] (256)";
        }
    }
}
