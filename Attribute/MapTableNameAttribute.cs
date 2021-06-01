using System;

namespace ORM_1_21_.Attribute
{
    /// <summary>
    /// Атрибут для навешивания названия таблицы на класс сущности.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class MapTableNameAttribute : System.Attribute
    {
        private readonly string _tableName;
        private readonly string _sqlWhere;
        
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="tableName">Название таблицы в базе данных</param>
        public MapTableNameAttribute(string tableName)
        {
            _tableName = tableName.Trim(new []{' ','[',']','`','"'});
        }

        ///<summary>
        /// Конструктор Определяет название таблицы, условие для всех выборок
        ///</summary>
        ///<param name="tableName">Название таблицы в базе данных</param>
        ///<param name="sqlWhere">добавление критериея запроса по where пример: "id='1'"</param>
        public MapTableNameAttribute(string tableName, string sqlWhere)
        {
            _tableName = tableName.Trim(new[] { ' ', '[', ']', '`','"' });
            _sqlWhere = sqlWhere;
        }

        /// <summary>
        /// Название таблицы в базе данных
        /// </summary>
        internal string TableName
        {
            get
            {
                switch (Configure.Provider)
                {
                    case ProviderName.MsSql:
                        return $"[{_tableName}]";

                    case ProviderName.MySql:
                        return $"`{_tableName}`";
                    case ProviderName.Postgresql:
                        return $"\"{_tableName}\"";
                    case ProviderName.Sqlite:
                        return $"{_tableName}";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        internal string SqlWhere => _sqlWhere ?? "";

    }
}