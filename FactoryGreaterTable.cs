using System;
using System.Text;
using ORM_1_21_.Attribute;
using ORM_1_21_.Linq;

namespace ORM_1_21_
{
    /// <summary>
    /// 
    /// </summary>
    internal class FactoryGreaterTable
    {
        public string SqlCreate<T>()
        {
            switch (Configure.Provider)
            {
                case ProviderName.MySql:
                    return UtilsCreateTableMySql.Create<T>();
                case ProviderName.MsSql:
                    return UtilsCreateTableMsSql.Create<T>();
                case ProviderName.Postgresql:
                    return UtilsCreateTablePostgres.Create<T>();
                case ProviderName.Sqlite:
                    return UtilsCreateTableSqlite.Create<T>();
                default:
                    return null;
            }
        }

        public static string GetDefaultValue(string def,Type type)
        {
            if (def != null)
            {
                return def;
            }

            if (type == typeof(long) )
            {
                return "NOT NULL DEFAULT '0'";
            }

            if (type == typeof(Nullable<>))
            {
                return "NULL";
            }
            if (type == typeof(int)
                || type.BaseType == typeof(Enum)
                || type == typeof(uint)
                || type == typeof(long)
                || type == typeof(short)
                ||type == typeof(bool)
                ||type == typeof(decimal)
                ||type == typeof(float)
                ||type == typeof(double))
            {
                return "NOT NULL DEFAULT '0'";
            }
         
            if (type == typeof(DateTime) )
            {
                return "NULL";
            }

            if (type == typeof(Guid))
            {
                return "NULL";
            }

            if (Utils.IsJsonType(type))
            {
                return "NULL";
            }


            return "NULL";

        }



    }
}