using System;
using NLog;

namespace ORM_1_21_
{
    public sealed partial class Configure
    {
        
        internal static void SendError(string sql, Exception exception)
        {
#if DEBUG
            _configure.OnOnErrorOrm(new ErrorOrmEventArgs { ErrorMessage = exception.ToString(), Sql = sql, InnerException = exception });
            return;
#endif
            _configure.OnOnErrorOrm(new ErrorOrmEventArgs {ErrorMessage = exception.Message,Sql = sql,InnerException = exception});
        }
        /// <summary>
        /// 
        /// </summary>
        public event ErrorEvent onErrorOrm;
        private static  Logger _log;
        private static Logger Logger => _log ?? (_log = LogManager.GetLogger("orm"));

        /// <summary>
        /// Запись в лог файл при наличии разрешения на запись при  созданом конфиге, и  определения файла куда писать.
        /// </summary>
        /// <param name="message">текст сообщения</param>
        public  static void WriteLogFile(string message)
        {
           if (LogFileName!=null)
           {
                Logger.Info(message);
           }
        }
    }

    public delegate void ErrorEvent(object sender, ErrorOrmEventArgs args);

    public class ErrorOrmEventArgs
    {
        public string Sql { get; set; }
        public string ErrorMessage { get; set; }
        public Exception InnerException { get; set; }
    }
}