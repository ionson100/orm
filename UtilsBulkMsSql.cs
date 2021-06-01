using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;


namespace ORM_1_21_
{
   internal class UtilsBulkMsSql
    {
        public  static string GetSql<T>(IEnumerable<T> list,string fileCsv,string fieldterminator)
        {
            if (fileCsv != null)
            {
                return GetSqlFile(list, fileCsv, fieldterminator);
            }

            return GetSimpleSql2(list);
        }

        private static string GetSqlFile<T>(IEnumerable<T> list, string fileCsv, string fieldterminator)
        {
            var tableName = AttributesOfClass<T>.TableName;
            StringBuilder sql=new StringBuilder("bulk insert " + tableName);
            sql.AppendLine($"from '{fileCsv}'");
            sql.AppendLine(" with(");
            sql.AppendLine("FIRSTROW = 2,");
            sql.AppendLine($"FIELDTERMINATOR = '{fieldterminator}',");
            sql.AppendLine("ROWTERMINATOR = '\n'");
            sql.AppendLine(")");
            StringBuilder builder=new StringBuilder();
            bool isAddPk=AttributesOfClass<T>.PkAttribute.Generator != Generator.Native;

            StringBuilder rowHead = new StringBuilder();
            if (isAddPk == true)
            {
                rowHead.Append(AttributesOfClass<T>.PkAttribute.ColumnName.Trim(new []{'[',']','`'}))
                    .Append(";");
            }
            foreach (var map in AttributesOfClass<T>.CurrentTableAttributeDall)
            {
                rowHead.Append(map.ColumnName.Trim(new[] {'[', ']', '`'})).Append(";");
            }

            builder.Append(rowHead.ToString().Substring(0, rowHead.ToString().LastIndexOf(';')))
                .Append(Environment.NewLine);



            foreach (var ob in list)
            {
                StringBuilder row=new StringBuilder();
                if (isAddPk == true)
                {
                    row.Append(AttributesOfClass<T>.GetValue.Value[AttributesOfClass<T>.PkAttribute.PropertyName](ob))
                        .Append(";");
                }
                foreach (var keyValuePair in AttributesOfClass<T>.PropertyInfoList.Value)
                {
                   row.Append(AttributesOfClass<T>.GetValue.Value[keyValuePair.Value.Name](ob)).Append(";");
                }

                string s = row.ToString().Substring(0, row.ToString().LastIndexOf(",", StringComparison.Ordinal)) +"\n";
                builder.Append(s);
            }
            File.WriteAllText(fileCsv,builder.ToString());
            return sql.ToString();
        }

       

        private static string GetSimpleSql2<T>(IEnumerable<T> list)
        {
            bool isAddPk = AttributesOfClass<T>.PkAttribute.Generator != Generator.Native;
            StringBuilder builder = new StringBuilder("INSERT INTO");
            builder.Append(AttributesOfClass<T>.TableName);
            builder.Append("(");
            StringBuilder headBuilder = new StringBuilder();
            if (isAddPk == true)
            {
                headBuilder.Append(AttributesOfClass<T>.PkAttribute.ColumnName).Append(",");
            }
            foreach (var map in AttributesOfClass<T>.CurrentTableAttributeDall)
            {
                if (map.TypeColumn == typeof(Image) || map.TypeColumn == typeof(byte[]))
                {
                    continue;
                }
                headBuilder.Append(map.ColumnName).Append(",");
            }

            builder.Append(headBuilder.ToString().Substring(0, headBuilder.ToString().LastIndexOf(',')));
            builder.Append(") ");
         
            int i = 0;
            foreach (var ob in list)
            {
                i++;
                StringBuilder row = new StringBuilder("SELECT ");
                if (isAddPk == true)
                {
                    var o = AttributesOfClass<T>.GetValue.Value[AttributesOfClass<T>.PkAttribute.PropertyName](ob);
                    Type type = AttributesOfClass<T>.PropertyInfoList.Value[AttributesOfClass<T>.PkAttribute.PropertyName].PropertyType;
                    row.Append(UtilsBulkMySql.GetValue(o, type)).Append(",");
                }
                foreach (var map in AttributesOfClass<T>.CurrentTableAttributeDall)
                {
                    var o = AttributesOfClass<T>.GetValue.Value[map.PropertyName](ob);
                    Type type = AttributesOfClass<T>.PropertyInfoList.Value[map.PropertyName].PropertyType;
                    if (type == typeof(Image) || type == typeof(byte[]))
                    {
                        continue;
                    }
                    string str = UtilsBulkMySql.GetValue(o, type);
                    row.Append(str).Append(",");
                }
                builder.AppendLine(row.ToString().Substring(0, row.ToString().LastIndexOf(',')));
                builder.AppendLine("UNION ALL");
            }
            var res = builder.ToString().Substring(0, builder.ToString().LastIndexOf("UNION ALL", StringComparison.Ordinal));
            return "SET DATEFORMAT YMD;"+res;
        }

        public static string InsertFile<T>(string fileCsv, string fieldterminator)
        {
            StringBuilder sql = new StringBuilder("bulk insert " + AttributesOfClass<T>.TableName);
            sql.AppendLine($"from '{fileCsv}'");
            sql.AppendLine(" with(");
            sql.AppendLine("FIRSTROW = 2,");
            sql.AppendLine($"FIELDTERMINATOR = '{fieldterminator}',");
            sql.AppendLine("ROWTERMINATOR = '\n'");
            sql.AppendLine(")");
            return sql.ToString();
        }
    }
}
