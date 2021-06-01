using System;
using System.Data;

namespace ORM_1_21_.Attribute
{
    /// <summary>
    /// Атрибут для указания первичного ключа для таблицы ( составные ключи не работают)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class MapPrimaryKeyAttribute : BaseAttribute
    {
        readonly Generator _p;

        /// <summary>
        /// Аттрибут первичного ключа
        /// </summary>
        /// <param name="columnName">Название поля</param>
        /// <param name="generator">Тип генерации зачения на ключ, native-автоинремент,assigned-назначенный в рукопашную</param>
        public MapPrimaryKeyAttribute(string columnName, Generator generator):base(columnName)
        {
            _p = generator;
        }

        /// <summary>
        /// Название колонки первичного 
        /// </summary>
        internal String ColumnNameForRider => Utils.ClearTrim(ColumnName);


        /// <summary>
        /// Тип колонки первичного ключа
        /// </summary>
        internal Type TypeColumn { get; set; }

        /// <summary>
        /// Тип генератора первичного ключа в базе
        /// </summary>
        internal Generator Generator => _p;

        /// <summary>
        /// Тип поля в базе данных
        /// </summary>
        internal DbType DbType()
        {
            return DbTypeConverter.ConvertFrom(TypeColumn);
        }
    }
}