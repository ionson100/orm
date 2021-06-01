using System;

namespace ORM_1_21_.Attribute
{
    /// <summary>
    /// Атрибут для класса слоя, определяет тип и название колонки.Применяется только при транформацию листа в таблицу
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class CurrentTableAtribute : System.Attribute
    {
        readonly Type _typeS;
        readonly String _nameColumn;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="type">Тип колонки в создаваемой таблице</param>
        /// <param name="nameColumn">Название колонки в создаваемой таблице</param>
        public CurrentTableAtribute(Type type, String nameColumn)
        {
            _typeS = type;
            _nameColumn = nameColumn;
        }

        /// <summary>
        /// Тип колонки в создаваемой таблице
        /// </summary>
        public Type GetTypeColumn
        {
            get { return _typeS; }
        }

        /// <summary>
        /// Название колонки в создаваемой таблице
        /// </summary>
        public String GetNameColumn
        {
            get { return _nameColumn; }
        }
    }
}