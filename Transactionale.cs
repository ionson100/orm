using System;
using System.Collections.Generic;
using System.Data;

namespace ORM_1_21_.Transaction
{
    /// <summary>
    /// Тип контейнер,содержит данные для трансакции,
    /// занятость трансакции, фабрику для транакций
    /// </summary>
    internal class Transactionale : ITransaction
    {
        bool _occupied;
        // List<Transactionale> _ltr = new List<Transactionale>();
        IDbConnection _connection;
        List<IDisposable> _listDispose = new List<IDisposable>();

        public List<IDisposable> ListDispose
        {
            get { return _listDispose; }
            set { _listDispose = value; }
        }

        public IDbConnection Connection
        {
            //get { return _connection; }
            set { _connection = value; }
        }

        public Transactionale()
        {
            IsolationLevel = IsolationLevel.RepeatableRead;
        }

        public IsolationLevel IsolationLevel { get; set; }

        public IDbTransaction Transaction { get; set; }

        public bool IsOccupied
        {
            get { return _occupied; }
            set { _occupied = value; }
        }

        #region ITransaction Members

        public void Commit()
        {
            Transaction?.Commit();
            InnerTransaction();
        }

        public void Rollback()
        {
            Transaction?.Rollback();
            InnerTransaction();
        }

        #endregion ITransaction Members

        private void InnerTransaction()
        {
            _occupied = false;
            if (_connection?.State == ConnectionState.Open)
            {
                _connection.Close();
            }
            _listDispose.ForEach(a => a.Dispose());
            _listDispose.Clear();
        }
    }
}