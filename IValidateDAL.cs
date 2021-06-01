namespace ORM_1_21_
{
    /// <summary>
    ///Проверка вводимых данных, для проверки отнаследуйте свой табличный класс от этого интерфейса
    ///и проверяйте данные перед модификацией в таблице.
    /// </summary>
    /// <typeparam name="T">Тип вашего табличного класса</typeparam>
    public interface IValidateDal<in T> where T : class
    {
        /// <summary>
        /// Проверка вводимых данных, для проверки отнаследуйте свой табличный класс от этого интерфейса
        /// </summary>
        /// <param name="item">Проверяемый объект</param>
        void Validate(T item);
    }
}