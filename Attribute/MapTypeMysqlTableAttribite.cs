using System;

namespace ORM_1_21_.Attribute
{
    /// <summary>
    /// Информация для создания определенного типа таблиц, только для MySql пример: ENGINE=InnoDB  DEFAULT CHARSET=UTF8;
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MapTypeMysqlTableAttribite:System.Attribute
    {
        internal string TableType { get; }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="tableType">строка, которая будет участвовать при создании таблиы</param>
        public MapTypeMysqlTableAttribite(string tableType)
        {
            TableType = tableType;
        }
    }
}
