namespace ORM_1_21_.Transaction
{
    /// <summary>
    /// Транзакциая
    /// </summary>
   public interface ITransaction
    {
       /// <summary>
       /// Завершить транзакцию
       /// </summary>
        void Commit();
       /// <summary>
       /// Откатить транзакцию
       /// </summary>
        void Rollback();
    }
}
