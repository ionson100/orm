namespace ORM_1_21_
{
    class V
    {
        public V(string sql)
        {
            _sql = sql;
           
        }
        private readonly string _sql;
        public string FreeSql()
        {
            return _sql;
        }
    }
}