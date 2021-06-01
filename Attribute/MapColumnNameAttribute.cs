using System;
using System.Data;

namespace ORM_1_21_.Attribute
{
    /// <summary>
    /// Атрибуты для класса слоя, определяют название  колонки в таблице
    /// </summary>
     
    public sealed class MapColumnNameAttribute :BaseAttribute
    {
        /// <summary>
        /// поле индексируется
        /// </summary>
        public bool IsIndex { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public string TypeString { get; set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="nameColumnTable">Название колонки в таблице  базы</param>
        public MapColumnNameAttribute(string nameColumnTable):base(nameColumnTable)
        {
        }

        internal Type TypeColumn { get;  set; }

        internal String ColumnNameForReader => ColumnName.Trim('`', '\'', '[', ']','"');

        internal DbType DbType()
        {
            //if (this.TypeColumn == typeof (string) )
            //    return (DbType) System.Data.SqlDbType.VarChar;
            return DbTypeConverter.ConvertFrom(TypeColumn);
        }
    }

    /// <summary>
    /// значение по умолчанию при создании таблицы 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class MapDefaultValueAttribute :System.Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        internal  string Value { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="valueAsString"></param>
        public MapDefaultValueAttribute(string valueAsString)
        {
            Value = valueAsString;
        }
    }

    /// <summary>
    /// значение по умолчанию при создании таблицы 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class MapColumnTypeAttribute : System.Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        internal string TypeString { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public MapColumnTypeAttribute(string typeColumn)
        {
            TypeString = typeColumn;
        }
    }
}