using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace ORM_1_21_
{
    /// <summary>
    ///     Базовый класс для конфигурации
    /// </summary>
    public sealed partial class Configure
    {
        //internal static bool UsageCache;

        internal static string ConnectionString;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetConnectionString()
        {
            lock (_locker)  
            {
                return ConnectionString;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static string LogFileName { get; private set; }


        private static Configure _configure;

        private static readonly object _locker = new object();

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="connectionString">Строка соединения с базой</param>
        /// <param name="provider">Провайдер соединения с базой</param>
        /// <param name="logFileName">Путь и название файла, куда будем писать логи, его отсутствие (null) отменяет запиь в файл.</param>
        public Configure(string connectionString, ProviderName provider, string logFileName)
        {
            ConnectionString = connectionString;
            Provider = provider;
            LogFileName = logFileName;
            LogFileName = logFileName;
            ActivateLogger(logFileName);
            if (provider == ProviderName.MySql)
                Utils.Assembler = AppDomain.CurrentDomain.Load("Mysql.Data");//Npgsql.dll
            if (provider == ProviderName.Postgresql)
            {
                Utils.Assembler = AppDomain.CurrentDomain.Load("Npgsql");//Npgsql.dll
            }

            if (Provider == ProviderName.Sqlite)
            {
                
                Utils.Assembler = AppDomain.CurrentDomain.Load("System.Data.SQLite");//Npgsql.dll
              
            }
            _configure = this;
        }

        /// <summary>
        /// Провайдер, которы использует орм в текущий момент
        /// </summary>
        public static ProviderName Provider { get; private set; }

        /// <summary>
        ///Получение сессии
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static ISession GetSession()
        {
            if (_configure == null)
            {
                SendError(null, new Exception("ISession GetInnerSession error _configure==null"));
                return null;
            }
               
            return _configure.GetInnerSession();
        }

      // public static IEnumerable<IEnumerable<T>> Batch<T>(
      //     IEnumerable<T> source, int batchSize)
      // {
      //     using (var enumerator = source.GetEnumerator())
      //     {
      //         while (enumerator.MoveNext())
      //             yield return YieldBatchElements(enumerator, batchSize - 1);
      //     }
      // }
      //
      // private static IEnumerable<T> YieldBatchElements<T>(
      //     IEnumerator<T> source, int batchSize)
      // {
      //     yield return source.Current;
      //     for (var i = 0; i < batchSize && source.MoveNext(); i++)
      //         yield return source.Current;
      // }


        private static void ActivateLogger(string fileNameLogFile)
        {
            if (Configure.LogFileName==null) return;
            if (!File.Exists(fileNameLogFile))
                using (File.Create(fileNameLogFile))
                {
                }

            var config = new LoggingConfiguration();
            using (var myTarget = new FileTarget())
            {
                config.AddTarget("file", myTarget);
                myTarget.FileName = fileNameLogFile;
                myTarget.Layout = "${message}";
                myTarget.ArchiveAboveSize = 5242880;
                myTarget.ArchiveNumbering = ArchiveNumberingMode.Rolling;
                var rule = new LoggingRule("*", LogLevel.Debug, myTarget);
                config.LoggingRules.Add(rule);
                LogManager.Configuration = config;
            }
        }

        private ISession GetInnerSession()
        {
            lock (_locker)
            {
                return new Sessione(ConnectionString);
            }
        }


        private void OnOnErrorOrm(ErrorOrmEventArgs args)
        {
            if (onErrorOrm == null)
            {
                throw new Exception(args.ErrorMessage+Environment.NewLine+args.Sql);
            }

            onErrorOrm.Invoke(this, args);

        }

      
    }
}