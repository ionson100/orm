using System.Data;

namespace ORM_1_21_
{
    /// <summary>
    /// Еденица параметра, для запроса с параметрами
    /// </summary>
    public sealed class Parameter
    {
        /// <summary>
        /// Имя парметра
        /// </summary>
        public string Name { get; private  set; }
        /// <summary>
        /// Значение пераметра
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="name">Имя парметра</param>
        /// <param name="value">Имя парметра</param>
        public Parameter(string name, object value)
        {
            Name = name.Replace("?",string.Empty).Replace("@",string.Empty);
            Value = value;
        }
    }

    /// <summary>
    /// Еденица параметра, для запроса с параметрами
    /// </summary>
    public sealed class ParameterStoredPr
    {
        //private readonly string _sourceColumn;

        /// <summary>
        /// Название колонки таблицы
        /// </summary>
       // public string SourceColumn { get; private set; }

        /// <summary>
        /// Имя парметра
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Значение пераметра
        /// </summary>
        public object Value { get;  set; }

        /// <summary>
        /// Перечисление типа параметра
        /// </summary>
        public ParameterDirection Direction { get; set; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="name">Имя парметра</param>
        /// <param name="value">Имя парметра</param>
        /// <param name="direction">ParameterDirection </param>
       
        public ParameterStoredPr(string name, object value,ParameterDirection direction)//,string sourceColumn
        {
           // SourceColumn = sourceColumn;
            Name = name.Replace("?", string.Empty).Replace("@", string.Empty);
            Value = value;
            Direction = direction;
        }
    }
}