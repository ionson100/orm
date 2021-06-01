namespace ORM_1_21_
{
    /// <summary>
    /// Сервисный интерфейс для обслуживания базы.Для своевременного действия, с работой  в базе
    /// отнаследуйте свой табличный тип от этого интерфейса, и можете контролировать в отбработчиках
    /// интерфейса, все этапы работы с базой
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IActionDal<in T> where T : class
    {
        /// <summary>
        /// Перед вставкой
        /// </summary>
        /// <param name="item">обьект вставки</param>
        void BeforeInsert(T item);

        /// <summary>
        /// После вставки
        /// </summary>
        /// <param name="item">обьект вставки</param>
        void AfterInsert(T item);

        /// <summary>
        /// Перед обновлением
        /// </summary>
        /// <param name="item">объект обновления</param>
        void BeforeUpdate(T item);

        /// <summary>
        /// После обновления
        /// </summary>
        /// <param name="item">обьект обновления</param>
        void AfterUpdate(T item);

        /// <summary>
        /// Перед удалением
        /// </summary>
        /// <param name="item"> объект удаления</param>
        void BeforeDelete(T item);

        /// <summary>
        /// Полсле удаления
        /// </summary>
        /// <param name="item">обьект после удаления, фантом.</param>
        void AfterDelete(T item);
    }
}