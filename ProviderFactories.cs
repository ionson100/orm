using System;
using System.Data;
using System.Data.Common;
using System.Reflection;


namespace ORM_1_21_
{
    internal static class ProviderFactories
    {
        private static DbProviderFactory _curFactory;



        internal static IDbDataParameter GetParameter()
        {
            return GetFactory().CreateParameter();
        }


        static DbProviderFactory GetFactory()
        {
            if (_curFactory == null)
            {
                switch (Configure.Provider)
                {
                    case ProviderName.MsSql:
                        _curFactory = DbProviderFactories.GetFactory("System.Data.SqlClient");
                        break;
                    case ProviderName.MySql:
                    {
                        var assa = Utils.Assembler.GetType("MySql.Data.MySqlClient.MySqlClientFactory");
                        _curFactory = (DbProviderFactory)assa.GetField("Instance").GetValue(null);
                        break;
                    }

                    case ProviderName.Postgresql:

                        var type = Utils.Assembler.GetType("Npgsql.NpgsqlFactory");
                        var field = type.GetField("Instance", BindingFlags.Static | BindingFlags.Public);

                        _curFactory= (DbProviderFactory)field.GetValue(null);
                      
                        break;
                    case ProviderName.Sqlite:
                        var type1 = Utils.Assembler.GetType("System.Data.SQLite.SQLiteFactory");
                        var field1 = type1.GetField("Instance", BindingFlags.Static | BindingFlags.Public);
                        _curFactory = (DbProviderFactory)field1.GetValue(null);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return _curFactory;
        }
        public static IDbConnection GetConnect()
        {

            return GetFactory().CreateConnection();

        }

        public static IDbCommand GetCommand()
        {
            return GetFactory().CreateCommand();
        }

        public static IDbDataAdapter GetDataAdapter()
        {
            return GetFactory().CreateDataAdapter();

        }

      
    }
}