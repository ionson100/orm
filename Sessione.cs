using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using ORM_1_21_.Linq;

namespace ORM_1_21_
{
    ///<summary>
    ///</summary>
    public sealed partial class Sessione : ISession, IServiceSessions
    {
        List<IDbCommand> _dbCommands=new List<IDbCommand>();
        IDbCommand IServiceSessions.CommandForLinq
        {
            get
            {
                var com = ProviderFactories.GetCommand();
                com.Connection = _connect;
                return com;
            }
        }

        object IServiceSessions.Locker { get; } = new object();

        Logger IServiceSessions.Logger { get; set; }

        //Dictionary<int, BoxCache> IServiceSessions.CacheFirstLevel { get; } = new Dictionary<int, BoxCache>();

        /// <summary>
        ///     Удаление объекта из базы
        /// </summary>
        /// <typeparam name="T">Тип удаляемого объекта</typeparam>
        /// <param name="item">Удаляемый объект</param>
        public int Delete<T>(T item) where T : class
        {
            if (!Utils.IsPersistent(item)) return 0;
            var com = ProviderFactories.GetCommand();
            com.Connection = _connect;
            AttributesOfClass<T>.CreateDeleteCommand(com, item);
            try
            {
                NotificBefore(item, ActionMode.Delete);
                OpenConnectAndTransaction(com);
                return com.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Configure.SendError(Utils.GetStringSql(com), ex);
                return 0;
            }
            finally
            {
                ComDisposable(com);
                NotificAfter(item, ActionMode.Delete);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> GetListMonster<T>(IDataReader reader) where T : class
        {
            return AttributesOfClass<T>.GetEnumerableObjects(reader);
        }

        /// <summary>
        ///     Сохранение обьекта в базе равно как вставка и изменение
        /// </summary>
        /// <typeparam name="T">Тип обьекта</typeparam>
        /// <param name="item">сохраняемый объект</param>
        public int Save<T>(T item) where T : class
        {
            if (item == null) throw new ArgumentException("Объект для сохранения равен Null");
            return SaveNew(item);
        }

        /// <summary>
        ///     Получение объекта по первичному ключу
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="id">Значение первичного ключа</param>
        /// <returns>Полученый объект, в случае отсутствия  в базe - NULL</returns>
        public T Get<T>(object id) where T : class
        {
            if (id == null) throw new ArgumentException("Объект первичного ключа, равен равен Null");
            return GetReal<T>(id);
        }

        /// <summary>
        ///     запрос на выборку с параметрами
        /// </summary>
        /// <param name="sqlWhere">запрос на выборку, начиная с where  с праметрами</param>
        /// <param name="obj">список параметров в той последовательности в которой они идут в запросе.</param>
        /// <typeparam name="T">Тип сущности</typeparam>
        /// <returns></returns>
        public IEnumerable<T> GetList<T>(string sqlWhere, params object[] obj) where T : class
        {
            var sqlAll = AttributesOfClass<T>.SimpleSqlSelect + AttributesOfClass<T>.AddSqlWhere(sqlWhere);
            var com = ProviderFactories.GetCommand();
            com.CommandText = sqlAll;
            AddParam( com, obj);
            com.Connection = _connect;
            IEnumerable<T> res;
            try
            {
                //var res1 = GetCache<T>(true, com);
                // if (res1 != null) return (IEnumerable<T>) res1;
                OpenConnectAndTransaction(com);
                res = AttributesOfClass<T>.GetEnumerableObjects(com.ExecuteReader());
                //SetCache<T>(true, com, res);
            }
            catch (Exception ex)
            {
                Configure.SendError(Utils.GetStringSql(com), ex);
                return null;
            }
            finally
            {
                ComDisposable(com);
            }

            return res;
        }

        /// <summary>
        ///     Создание таблицы
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void TableCreate<T>() where T : class
        {
            var ss = new FactoryGreaterTable().SqlCreate<T>();
            var com = ProviderFactories.GetCommand();
            com.Connection = _connect;

            com.CommandText = ss;
            try
            {
                OpenConnectAndTransaction(com);
                com.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Configure.SendError(Utils.GetStringSql(com), ex);
            }
            finally
            {
                ComDisposable(com);
                WriteLogFile(ss);
            }
        }

        /// <summary>
        ///     Получение IDbCommand,Закрывать соединение вручную
        /// </summary>
        /// <returns></returns>
        public IDbCommand GeDbCommand()
        {
            return ProviderFactories.GetCommand();
        }

        /// <summary>
        ///     Удаление таблицы
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void DropTable<T>() where T : class
        {
            var com = ProviderFactories.GetCommand();
            com.Connection = _connect;

            com.CommandText = $"DROP TABLE {AttributesOfClass<T>.TableName}";
            try
            {
                OpenConnectAndTransaction(com);
                com.ExecuteNonQuery();
                WriteLogFile(com.CommandText);
            }
            catch (Exception ex)
            {
                Configure.SendError(Utils.GetStringSql(com), ex);
            }
            finally
            {
                ComDisposable(com);
            }
        }

        /// <summary>
        ///     Проверка на существование таблицы
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool TableExists<T>() where T : class
        {
            var com = ProviderFactories.GetCommand();
            try
            {
                if (Configure.Provider == ProviderName.Postgresql)
                {
                    com.Connection = _connect;
                    var tableName = Utils.ClearTrim(AttributesOfClass<T>.TableName);
                    com.CommandText =
                        $"SELECT count(*) FROM pg_tables WHERE   tablename  = '{tableName}';";

                    OpenConnectAndTransaction(com);
                    long res =(long) com.ExecuteScalar();
                    return res != 0;
                }
                else
                {
                    com = ProviderFactories.GetCommand();
                    com.Connection = _connect;
                    com.CommandText = $"select 1 from {AttributesOfClass<T>.TableName};";
                    OpenConnectAndTransaction(com);
                    com.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                ComDisposable(com);
            }
        }

        /// <summary>
        ///     Пожарынй шланг данных
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="objects">параметры</param>
        /// <returns></returns>
        public IDataReader ExecuteReader(string sql,object[] objects)
        {
            var com = ProviderFactories.GetCommand();
            com.Connection = _connect;

            com.CommandText = sql;
            AddParam(com,objects);
            //try
            //{
            OpenConnectAndTransaction(com);
            return com.ExecuteReader();
            // }
            // finally
            // {
            //     //ComDisposable(com);
            // }
        }

       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="timeOut"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public IDataReader ExecuteReaderT(string sql, int timeOut = -1, params object[] obj)
        {
            var com = ProviderFactories.GetCommand();
            com.Connection = _connect;
            com.CommandText = sql;
            if (timeOut != -1)
            {
                com.CommandTimeout = timeOut;
            }
            AddParam(com, obj);
            //try
            //{
            OpenConnectAndTransaction(com);
            return com.ExecuteReader();
            // }
            // finally
            // {
            //     //ComDisposable(com);
            // }
        }

        /// <summary>
        ///     получение DataTable
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataTable GetDataTable(string sql,int timeout=-1)
        {
            var table = new DataTable();

            var com = ProviderFactories.GetCommand();
            com.Connection = _connect;
            if (timeout != -1)
            {
                com.CommandTimeout = timeout;
            }

            com.CommandText = sql;
            try
            {
                OpenConnectAndTransaction(com);
                var reader = com.ExecuteReader();
                table.BeginLoadData();
                table.Load(reader);
                table.EndLoadData();
                WriteLogFile(com.CommandText);
                return table;
            }
            catch (Exception ex)
            {
                Configure.SendError(com.CommandText, ex);
                return null;
            }
            finally
            {
                ComDisposable(com);
            }
        }

        /// <summary>
        /// Список таблиц в базе
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public List<string> GetTableNames()
        {
            IEnumerable<object> lResult;
            var com = ProviderFactories.GetCommand();

            var index = 2;

            switch (Configure.Provider)
            {
                case ProviderName.MsSql:
                {
                    com.CommandText = "SELECT * FROM information_schema.tables";
                    index = 2;
                }
                    break;
                case ProviderName.MySql:
                {
                    index = 0;
                    var strs = Configure.ConnectionString.Split(';');
                    foreach (var str in strs)
                        if (str.ToUpper().Contains("DATABASE"))
                        {
                            var nameBase = str.Split('=')[1].Trim();
                            com.CommandText =
                                $"SELECT table_name FROM information_schema.tables WHERE table_schema = '{nameBase}';";
                        }
                }
                    break;
                case ProviderName.Postgresql:
                {
                    index = 0;
                    com.CommandText = "SELECT table_name FROM information_schema.tables where table_schema='public'";
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            com.Connection = _connect;

            var result = new List<string>();
            try
            {
                OpenConnectAndTransaction(com);
                var reader = com.ExecuteReader();
                while (reader.Read()) result.Add(reader.GetString(index));
                reader.Close();
            }
            catch (Exception ex)
            {
                Configure.SendError(Utils.GetStringSql(com), ex);
            }
            finally
            {
                ComDisposable(com);
            }

            return result;
        }

        /// <summary>
        /// Создает базу данных
        /// </summary>
        /// <param name="baseName">название базы для Mysql  , путь до базы для Postgesql,Sqlite,MSSql</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public int CreateBase(string baseName)
        {
            var com = ProviderFactories.GetCommand();
            com.Connection = _connect;


            try
            {
                switch (Configure.Provider)
                {
                    case ProviderName.MsSql:
                        if (File.Exists(baseName))
                            return 0;
                        var bName = Path.GetFileName(baseName).Substring(0, length: Path.GetFileName(baseName).IndexOf('.'));
                        com.CommandText = $"CREATE DATABASE {bName} ON(NAME = {bName}, FILENAME = '{baseName}')";
                        break;
                    case ProviderName.MySql:
                        com.CommandText = $"CREATE DATABASE [IF NOT EXISTS] {baseName}";
                        break;
                    case ProviderName.Postgresql:
                        com.CommandText = $"CREATE DATABASE {baseName};";
                        return -1;
                    case ProviderName.Sqlite:
                        if (File.Exists(baseName) == false)
                        {
                            com.Connection.GetType().GetMethod("CreateFile", BindingFlags.Static | BindingFlags.Public)
                                ?.Invoke(com.Connection, new object[] {baseName});
                            return -1;
                        }

                        return 0;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                OpenConnectAndTransaction(com);
                return com.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Configure.SendError(com.CommandText, ex);
                return 0;
            }
            finally
            {
                ComDisposable(com);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="list"></param>
        /// <param name="fileCsv"></param>
        /// <param name="fieldterminator"></param>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void InsertBulk<T>(IEnumerable<T> list, string fileCsv = null, string fieldterminator = ";", int timeOut = -1)
        {
            var com = ProviderFactories.GetCommand();
            com.Connection = _connect;
            switch (Configure.Provider)
            {
                case ProviderName.MsSql:
                    com.CommandText = UtilsBulkMsSql.GetSql(list, fileCsv, fieldterminator);
                    break;
                case ProviderName.MySql:
                    com.CommandText = UtilsBulkMySql.GetSql(list, fileCsv, fieldterminator);
                    break;
                case ProviderName.Postgresql:
                    com.CommandText = UtilsBulkPostgres.GetSql(list, fileCsv, fieldterminator);
                    break;
                case ProviderName.Sqlite:
                    com.CommandText = UtilsBulkMySql.GetSql(list, fileCsv, fieldterminator);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            try
            {
                OpenConnectAndTransaction(com);
                com.CommandTimeout = 30000;
                com.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Configure.SendError(Utils.GetStringSql(com), ex);
            }
            finally
            {
                ComDisposable(com);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileCsv"></param>
        /// <param name="fieldterminator"></param>
        /// <param name="timeOut"></param>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void InsertBulkFomFile<T>(string fileCsv = null, string fieldterminator = ";", int timeOut = -1)
        {
            var com = ProviderFactories.GetCommand();
            com.Connection = _connect;
            if (timeOut != -1)
            {
                com.CommandTimeout = timeOut;
            }
            switch (Configure.Provider)
            {
                case ProviderName.MsSql:
                    com.CommandText = UtilsBulkMsSql.InsertFile<T>( fileCsv, fieldterminator);
                    break;
                case ProviderName.MySql:
                    com.CommandText = UtilsBulkMySql.InsertFile<T>( fileCsv, fieldterminator);
                    break;
                case ProviderName.Postgresql:
                    com.CommandText = UtilsBulkPostgres.InsertFile<T>( fileCsv, fieldterminator);
                    break;
                case ProviderName.Sqlite:
                    com.CommandText = UtilsBulkMySql.InsertFile<T>( fileCsv, fieldterminator);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            try
            {
                OpenConnectAndTransaction(com);
                com.CommandTimeout = 30000;
                com.ExecuteReader();
            }
            catch (Exception ex)
            {
                Configure.SendError(Utils.GetStringSql(com), ex);
            }
            finally
            {
                ComDisposable(com);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sql, params object[] obj)
        {
            var com = ProviderFactories.GetCommand();
            com.Connection = _connect;
            com.CommandType = CommandType.Text;
            com.CommandText = sql;
            AddParam(com, obj);
           
            try
            {
                OpenConnectAndTransaction(com);
                var res = com.ExecuteScalar();
                return res;
            }
            catch (Exception ex)
            {
                Configure.SendError(Utils.GetStringSql(com), ex);
                return null;
            }
            finally
            {
                ComDisposable(com);
            }
        }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="sql"></param>
       /// <param name="timeOut"></param>
       /// <param name="obj"></param>
       /// <returns></returns>
       public object ExecuteScalarT(string sql, int timeOut = -1, params object[] obj)
        {
            var com = ProviderFactories.GetCommand();
            com.Connection = _connect;
            com.CommandType = CommandType.Text;
            com.CommandText = sql;
            AddParam(com, obj);
            if (timeOut != -1)
            {
                com.CommandTimeout = timeOut;
            }

            try
            {
                OpenConnectAndTransaction(com);
                var res = com.ExecuteScalar();
                return res;
            }
            catch (Exception ex)
            {
                Configure.SendError(Utils.GetStringSql(com), ex);
                return null;
            }
            finally
            {
                ComDisposable(com);
            }
        }

        /// <summary>
        /// Очистка таблицы
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public int TruncateTable<T>()
        {
            var com = ProviderFactories.GetCommand();
            com.Connection = _connect;
            com.CommandType = CommandType.Text;
            if (Configure.Provider == ProviderName.Sqlite)
                com.CommandText = $"DELETE FROM {AttributesOfClass<T>.TableName};";
            else
                com.CommandText = $"TRUNCATE TABLE {AttributesOfClass<T>.TableName};";

            try
            {
                OpenConnectAndTransaction(com);
                return com.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Configure.SendError(Utils.GetStringSql(com), ex);
                return 0;
            }
            finally
            {
                ComDisposable(com);
            }
        }

        /// <summary>
        ///     Получения выражения ling to SQL
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Query<T> Querion<T>()
        {
            QueryProvider p = new DbQueryProvider<T>(this);
            return new Query<T>(p);
        }

        public IDbCommand GetCommand()
        {
            var com = ProviderFactories.GetCommand();
            com.Connection = _connect;
            return com;
        }

        public IDbConnection GetConnection()
        {
            return ProviderFactories.GetConnect();
        }

        public IDbDataAdapter GetDataAdapter()
        {
            return ProviderFactories.GetDataAdapter();
        }

        public IDataParameter GetDataParameter()
        {
            return ProviderFactories.GetParameter();
        }


        /// <summary>
        ///     Строка подключения
        /// </summary>
        /// <returns></returns>
        public string GetConnectionString()
        {
            return _connect.ConnectionString;
        }

        /// <summary>
        ///     Без возврата результата
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="obj"></param>
        public int ExecuteNonQuery(string sql, params object[] obj)
        {
            var com = ProviderFactories.GetCommand();
            com.Connection = _connect;
            com.CommandText = sql;
            AddParam(com,obj);
            try
            {
                OpenConnectAndTransaction(com);
                com.CommandTimeout = 30000;
                return com.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Configure.SendError(Utils.GetStringSql(com), ex);
                return -1;
            }
            finally
            {
                ComDisposable(com);
            }
        }

        /// <summary>
        /// Выполнение запроса с параметрами и TimeOut
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="timeOut"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int ExecuteNonQueryT(string sql, int timeOut = -1, params object[] obj)
        {

            var com = ProviderFactories.GetCommand();
            com.Connection = _connect;
            com.CommandText = sql;
            AddParam(com, obj);
            try
            {
                OpenConnectAndTransaction(com);
                com.CommandTimeout = 30000;
                if (timeOut != -1)
                {
                    com.CommandTimeout = timeOut;
                }
                return com.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Configure.SendError(Utils.GetStringSql(com), ex);
                return -1;
            }
            finally
            {
                ComDisposable(com);
            }
        }

        /// <summary>
        ///     table name from base
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public string TableName<T>()
        {
            return AttributesOfClass<T>.TableName;
        }

     


        /// <summary>
        ///     Получения списка прототипов объекта
        /// </summary>
        /// <param name="obj">прототип</param>
        /// <param name="sqlWhere">запрос Where включительно</param>
        /// <param name="isCache">использование кеша</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public IEnumerable<T> GetList<T>(T obj, string sqlWhere) where T : class
        {
            IEnumerable<T> lResult;
            var com = ProviderFactories.GetCommand();
            com.Connection = _connect;
            com.CommandText = AttributesOfClass<T>.SimpleSqlSelect + AttributesOfClass<T>.AddSqlWhere(sqlWhere);
            try
            {
                // var res1 = GetCache<T>(isCache, com);
                // if (res1 != null) return (IEnumerable<T>) res1;
                OpenConnectAndTransaction(com);
                lResult = AttributesOfClass<T>.GetEnumerableObjects(com.ExecuteReader());
                //SetCache<T>(isCache, com, lResult);
            }
            catch (Exception ex)
            {
                Configure.SendError(Utils.GetStringSql(com), ex);
                return null;
            }
            finally
            {
                ComDisposable(com);
            }

            return lResult;
        }

        /// <summary>
        ///     Возвращает лист объектов  табличной сущности
        /// </summary>
        /// <typeparam name="T">Тип класа сущности</typeparam>
        /// <param name="sqlWhere">
        ///     Запрос на выборку начиная с Where, Where включительно
        ///     для полной выборки можно указать "" или NULL
        /// </param>
        /// <returns>Лист оьектов сущности</returns>
        public IEnumerable<T> GetList<T>(string sqlWhere) where T : class
        {
            var sqlAll = AttributesOfClass<T>.SimpleSqlSelect + AttributesOfClass<T>.AddSqlWhere(sqlWhere);

            IEnumerable<T> lResul;
            var com = ProviderFactories.GetCommand();
            com.Connection = _connect;
            com.CommandText = sqlAll;
            try
            {
                // var res1 = GetCache<T>(isCache, com);
                // if (res1 != null) return (IEnumerable<T>) res1;
                OpenConnectAndTransaction(com);
                lResul = AttributesOfClass<T>.GetEnumerableObjects(com.ExecuteReader());
                //SetCache<T>(isCache, com, lResul);
            }
            catch (Exception ex)
            {
                Configure.SendError(Utils.GetStringSql(com), ex);
                return null;
            }
            finally
            {
                ComDisposable(com);
            }

            return lResul;
        }

        private int SaveNew<T>(T item) where T : class
        {
            var rez = 0;
            var com = ProviderFactories.GetCommand();
            com.Connection = _connect;

            try
            {
                if (Utils.IsPersistent(item))
                {
                    NotificBefore(item, ActionMode.Update);
                    if (Configure.Provider == ProviderName.Postgresql || Configure.Provider == ProviderName.Sqlite)
                        AttributesOfClass<T>.CreateUpdateCommandPostgres(com, item);
                    else
                        AttributesOfClass<T>.CreateUpdateCommandMysql(com, item, Configure.Provider);


                    OpenConnectAndTransaction(com);
                    rez = com.ExecuteNonQuery();
                    NotificAfter(item, ActionMode.Update);
                }
                else
                {
                    NotificBefore(item, ActionMode.Insert);
                    AttributesOfClass<T>.CreateInsetCommand(com, item);
                    OpenConnectAndTransaction(com);
                    var val = com.ExecuteScalar();
                    if (val != null)
                    {
                        AttributesOfClass<T>.RedefiningPrimaryKey(item, val);
                        rez = 1;
                    }
                    Utils.SetPersisten(item);
                    NotificAfter(item, ActionMode.Insert);
                }
            }
            catch (Exception ex)
            {
                Configure.SendError(Utils.GetStringSql(com), ex);
            }
            finally
            {
                ComDisposable(com);
            }

            return rez;
        }

        ///<summary>
        ///</summary>
        ///<param name="id"></param>
        ///<typeparam name="T"></typeparam>
        ///<returns></returns>
        ///<exception cref="Exception"></exception>
        private T GetReal<T>(object id) where T : class
        {
            var sqlAll = string.Format("{0} WHERE {1}.{2}='{3}'", AttributesOfClass<T>.SimpleSqlSelect,
                AttributesOfClass<T>.TableName, AttributesOfClass<T>.PkAttribute.ColumnName, id);
            var com = ProviderFactories.GetCommand();
            com.Connection = _connect;
            com.CommandText = sqlAll;
            try
            {
                OpenConnectAndTransaction(com);
                var res = AttributesOfClass<T>.GetEnumerableObjects(com.ExecuteReader());

                var enumerable = res as T[] ?? res.ToArray();
                return enumerable.Any() ? enumerable.First() : null;
            }
            catch (Exception ex)
            {
                Configure.SendError(Utils.GetStringSql(com), ex);

                return null;
            }
            finally
            {
                ComDisposable(com);
                //GetCache<T>(false, com);
            }
        }

        internal void ComDisposable(IDbCommand com)
        {
            try
            {
                WriteLogFile(com);
            }
            finally
            {
                if (Transactionale.IsOccupied == false)
                {
                    com.Connection.Close();

                    com.Dispose();
                }
                else
                {
                    _dbCommands.Add(com);
                }
            }
        }

        /// <summary>
        ///     Возвращает лист оъектов  табличной сущности
        /// </summary>
        /// <typeparam name="T">Тип класса сущности</typeparam>
        /// <returns>Лист оьектов сущности</returns>
        public IEnumerable<T> GetList<T>() where T : class
        {
            return GetList<T>("");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<Query<T>> QuerionAsync<T>()
        {
            return await Task.Run(() =>
            {
                QueryProvider p = new DbQueryProvider<T>(this);
                return new Query<T>(p);
            });
        }

        /// <summary>
        ///возвращает название поля для таблицы
        /// </summary>
        /// <param name="property"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string ColumnName<T>(Expression<Func<T,object>> property)
        {
            LambdaExpression lambda = property;
            MemberExpression memberExpression;

            var expression = lambda.Body as UnaryExpression;
            if (expression != null)
            {
                UnaryExpression unaryExpression = (UnaryExpression)expression;
                memberExpression = (MemberExpression)(unaryExpression.Operand);
            }
            else
            {
                memberExpression = (MemberExpression)(lambda.Body);
            }
            string name=((PropertyInfo)memberExpression.Member).Name;
            foreach (var dal in AttributesOfClass<T>.CurrentTableAttributeDall)
            {
                if (dal.PropertyName == name)
                {
                    return dal.ColumnName;
                }
            }
            if (AttributesOfClass<T>.PkAttribute.PropertyName == name)
            {
                return AttributesOfClass<T>.PkAttribute.ColumnName;
            }
            throw new Exception($"Не могу определить поле таблицы для типа {typeof(T)}");

        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public string InsertCommand<T>( T t)
        {
            switch (Configure.Provider)
            {
                case ProviderName.MsSql:
                    throw new Exception("не рализовано");
                case ProviderName.MySql:
                    throw new Exception("не рализовано");
                case ProviderName.Postgresql:
                   return CommandNativePostgres.GetInsertSql(t);
                case ProviderName.Sqlite:
                    throw new Exception("не рализовано");
                default:
                    throw new ArgumentOutOfRangeException();
            }
           
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public string DeleteCommand<T>(T t)
        {
            switch (Configure.Provider)
            {
                case ProviderName.MsSql:
                    throw new Exception("не рализовано");
                case ProviderName.MySql:
                    throw new Exception("не рализовано");
                case ProviderName.Postgresql:
                    return CommandNativePostgres.GetDeleteSql(t);
                case ProviderName.Sqlite:
                    throw new Exception("не рализовано");
                default:
                    throw new ArgumentOutOfRangeException();
            }
            // 
        }
    }
}