using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using ORM_1_21_.Linq.MsSql;
using ORM_1_21_.Linq.MySql;
using ORM_1_21_.Transaction;

namespace ORM_1_21_.Linq
{
    internal class DbQueryProvider<T> : QueryProvider, ISqlComposite
    {
        private readonly Dictionary<string, object> _paramFree = new Dictionary<string, object>();
        private readonly List<ParameterStoredPr> _paramFreeStoredPr = new List<ParameterStoredPr>();


        private readonly Dictionary<string, object> _parOut = new Dictionary<string, object>();
        private readonly Sessione _sessione;
        private IDbCommand _com;

        private Evolution _ev;

        private bool _isStoredPr;
        private List<OneComprosite> _listOne;
        private Dictionary<string, object> _param;
        private Dictionary<string, object> _paramAdd;

        public DbQueryProvider(Sessione ses)
        {
            _sessione = ses;
            Sessione = ses;
        }

        public ISession Sessione { get; }

        public List<ContainerCastExpression> ListCastExpression { get; set; } = new List<ContainerCastExpression>();

        public Transactionale Transactionale
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public IDbTransaction Transaction
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        private bool PingComposite(Evolution eval)
        {
            return _listOne.Any(a => a.Operand == eval);
        }


        public override string GetQueryText(Expression expression)
        {
            return TranslateString(expression, out _ev);
        }

        public override object Execute(Expression expression)
        {
            return null;
        }

        public object Execute<TS>(Expression expression, List<OneComprosite> comprosites,
            Dictionary<string, object> dictionary)
        {
            _param = dictionary;
            _listOne = comprosites;
            return Execute<TS>(expression);
        }

        public object ExecuteParam<TS>(Expression expression, params Parameter[] par)
        {
            if (par != null) par.ToList().ForEach(a => _paramFree.Add(a.Name, a.Value));
            return Execute<TS>(expression);
        }

        public override object ExecuteSPP<TS>(Expression expression)
        {
            var sb = new StringBuilder();
            IDataReader dataReader = null;
            var servis = (IServiceSessions) Sessione;
            _com = servis.CommandForLinq;

            _com.CommandType = CommandType.StoredProcedure;
            _com.CommandText = Translate(expression, out _ev);
            if (Configure.Provider == ProviderName.MsSql)
            {
                var mat = new Regex(@"TOP\s@p\d").Matches(_com.CommandText);
                foreach (var variable in mat)
                {
                    var st = variable.ToString().Split(' ')[1];
                    var val = _param.FirstOrDefault(a => a.Key == st).Value;
                    _com.CommandText = _com.CommandText.Replace(variable.ToString(),
                        string.Format("{1} ({0})", val, StringConst.Top));
                    _param.Remove(st);
                }
            }

            foreach (var p in _paramFreeStoredPr)
            {
                sb.Append(string.Format(CultureInfo.CurrentCulture, "{0}-{1},", p.Name, p.Value));
                IDataParameter pr = ProviderFactories.GetParameter();
                pr.Direction = p.Direction;
                pr.ParameterName = p.Name;
                pr.Value = p.Value;
                // pr.SourceColumn = p.SourceColumn;

                _com.Parameters.Add(pr);
            }

            try
            {
                _sessione.OpenConnectAndTransaction(_com);
                dataReader = _com.ExecuteReader();
                if (AttributesOfClass<TS>.IsValid)
                {
                    var lResul = AttributesOfClass<TS>.GetEnumerableObjects(dataReader);
                    foreach (var par in _com.Parameters)
                        if (((IDataParameter) par).Direction == ParameterDirection.InputOutput ||
                            ((IDataParameter) par).Direction == ParameterDirection.Output ||
                            ((IDataParameter) par).Direction == ParameterDirection.ReturnValue)
                            _parOut.Add(((IDataParameter) par).ParameterName, ((IDataParameter) par).Value);
                    return lResul;
                }
                else
                {
                    #region

                    var countcol = dataReader.FieldCount;
                    var list = new List<Type>();
                    for (var i = 0; i < countcol; i++) list.Add(dataReader.GetFieldType(i));
                    var ci = typeof(TS).GetConstructor(list.ToArray());
                    var resDis = new List<TS>();
                    while (dataReader.Read())
                        if (ci != null)
                        {
                            var par = new List<object>();
                            for (var i = 0; i < countcol; i++)
                                par.Add(dataReader[i] == DBNull.Value ? null : dataReader[i]);
                            var e = ci.Invoke(par.ToArray());
                            resDis.Add((TS) e);
                        }
                        else
                        {
                            if (countcol == 1)
                            {
                                resDis.Add((TS) Utils.Convertor<TS>(dataReader[0]));
                            }
                            else
                            {
                                dynamic employee = new ExpandoObject();
                                for (var i = 0; i < countcol; i++)
                                    ((IDictionary<string, object>) employee).Add(dataReader.GetName(i),
                                        dataReader[i] == DBNull.Value ? null : dataReader[i]);
                                resDis.Add((TS) employee);
                            }
                        }

                    dataReader.NextResult();
                    foreach (var par in _com.Parameters)
                        if (((IDataParameter) par).Direction == ParameterDirection.InputOutput ||
                            ((IDataParameter) par).Direction == ParameterDirection.Output ||
                            ((IDataParameter) par).Direction == ParameterDirection.ReturnValue)
                            _parOut.Add(((IDataParameter) par).ParameterName, ((IDataParameter) par).Value);

                    return resDis;

                    #endregion
                }
            }
            catch (Exception ex)
            {
                Configure.SendError(_com.CommandText,ex);
                return null;

            }
            finally
            {
                _sessione.ComDisposable(_com);
                if (dataReader != null)
                {
                    dataReader.Close();
                    dataReader.Dispose();
                }
            }
        }

        public object ExecuteMonster<TS>(IDataReader dataReader)
        {

            var count = dataReader.FieldCount;
            var list = new List<Type>();
            for (var i = 0; i < count; i++) list.Add(dataReader.GetFieldType(i));
            var ci = typeof(TS).GetConstructor(list.ToArray());
            var resDis = new List<TS>();
            if (ci != null)
            {
                while (dataReader.Read())
                {
                    var par = new List<object>();
                    for (var i = 0; i < count; i++)
                        par.Add(dataReader[i] == DBNull.Value ? null : dataReader[i]);
                    var e = ci.Invoke(par.ToArray());
                    resDis.Add((TS)e);
                }
            }
            else if (count == 1 && typeof(TS).IsValueType || typeof(TS) == typeof(string) ||
                     typeof(TS).GetInterface("IEnumerable") != null || typeof(TS).IsGenericTypeDefinition)
            {
                while (dataReader.Read())
                    resDis.Add((TS)(dataReader[0] == DBNull.Value ? null : dataReader[0]));
            }
            else
            {
                if (typeof(TS) != typeof(object) && typeof(TS).IsClass)
                {
                    object employee = Activator.CreateInstance<TS>();
                    while (dataReader.Read())
                    {
                        for (var i = 0; i < dataReader.FieldCount; i++)
                            AttributesOfClass<TS>.SetValueFreeSql.Value[dataReader.GetName(i)]((TS)employee,
                                dataReader[i] == DBNull.Value ? null : dataReader[i]);
                        resDis.Add((TS)employee);
                    }
                }
                else
                {
                    while (dataReader.Read())
                    {
                        dynamic employee = new ExpandoObject();
                        for (var i = 0; i < count; i++)
                            ((IDictionary<string, object>)employee).Add(dataReader.GetName(i),
                                dataReader[i] == DBNull.Value ? null : dataReader[i]);
                        resDis.Add((TS)employee);
                    }
                }
            }

            return resDis;
        }


        public override object Execute<TS>(Expression expression)
        {
            var servis = (IServiceSessions) Sessione;
            _com = servis.CommandForLinq;
            if (_isStoredPr)
                _com.CommandType = CommandType.StoredProcedure;

            _com.CommandText = Translate(expression, out _ev).Replace("FROM", " FROM ");
            var sb = new StringBuilder();
            if (Configure.Provider == ProviderName.MsSql)
            {
                var mat = new Regex(@"TOP\s@p\d").Matches(_com.CommandText);
                foreach (var variable in mat)
                {
                    var st = variable.ToString().Split(' ')[1];
                    var val = _param.FirstOrDefault(a => a.Key == st).Value;
                    _com.CommandText = _com.CommandText.Replace(variable.ToString(),
                        string.Format("{1} ({0})", val, StringConst.Top));
                    _param.Remove(st);
                }
            }

            _com.Parameters.Clear();
            if (_paramAdd != null)
                foreach (var p in _paramAdd)
                {
                    sb.Append(string.Format(CultureInfo.CurrentCulture, "{0}-{1},", p.Key, p.Value));
                    IDataParameter pr = ProviderFactories.GetParameter();
                    pr.ParameterName = p.Key;
                    pr.Value = p.Value;
                    //  _com.Parameters.Add(pr);
                }

            foreach (var p in _param)
            {
                sb.Append(string.Format(CultureInfo.CurrentCulture, "{0}-{1},", p.Key, p.Value));
                IDataParameter pr = ProviderFactories.GetParameter();
                pr.ParameterName = p.Key;
                pr.Value = p.Value;
                _com.Parameters.Add(pr);
            }

            foreach (var p in _paramFree)
            {
                IDataParameter pr = ProviderFactories.GetParameter();
                pr.ParameterName = p.Key;
                pr.Value = p.Value;
                _com.Parameters.Add(pr);
            }

            foreach (var p in _paramFreeStoredPr)
            {
                IDataParameter pr = ProviderFactories.GetParameter();
                pr.Direction = p.Direction;
                pr.ParameterName = p.Name;
                pr.Value = p.Value;
                //  pr.SourceColumn = p.SourceColumn;
                _com.Parameters.Add(pr);
            }


            IDataReader dataReader = null;
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            try
            {
                _sessione.OpenConnectAndTransaction(_com);
                if (PingComposite(Evolution.All))
                {
                    var ee = _com.ExecuteScalar();
                    var res = ee.ToString() != "0";
                    return res;
                }

                if (PingComposite(Evolution.Count))
                {
                    var ee = _com.ExecuteScalar();
                    var res = Convert.ToInt32(ee, CultureInfo.CurrentCulture);
                    return res;
                }

                if (PingComposite(Evolution.Delete))
                {
                    var ee = _com.ExecuteNonQuery();
                    return ee;
                }

                if (PingComposite(Evolution.Update))
                {
                    var ee = _com.ExecuteNonQuery();
                    return ee;
                }

                if (PingComposite(Evolution.Any))
                {
                    var ee = _com.ExecuteScalar();
                    var res = Convert.ToInt32(ee, CultureInfo.CurrentCulture) != 0;
                    return res;
                }

                /////////////////////////////////////////////////////////////////////////////////////////////////

                #region  dataReader

                if (PingComposite(Evolution.FreeSql) &&
                    AttributesOfClass<TS>.IsValid == false)
                {
                    dataReader = _com.ExecuteReader();
                    var count = dataReader.FieldCount;
                    var list = new List<Type>();
                    for (var i = 0; i < count; i++) list.Add(dataReader.GetFieldType(i));
                    var ci = typeof(TS).GetConstructor(list.ToArray());
                    var resDis = new List<TS>();
                    if (ci != null)
                    {
                        while (dataReader.Read())
                        {
                            var par = new List<object>();
                            for (var i = 0; i < count; i++)
                                par.Add(dataReader[i] == DBNull.Value ? null : dataReader[i]);
                            var e = ci.Invoke(par.ToArray());
                            resDis.Add((TS) e);
                        }
                    }
                    else if (count == 1 && typeof(TS).IsValueType || typeof(TS) == typeof(string) ||
                             typeof(TS).GetInterface("IEnumerable") != null || typeof(TS).IsGenericTypeDefinition)
                    {
                        while (dataReader.Read())
                            resDis.Add((TS) (dataReader[0] == DBNull.Value ? null : dataReader[0]));
                    }
                    else
                    {
                        if (typeof(TS) != typeof(object) && typeof(TS).IsClass)
                        {
                            object employee = Activator.CreateInstance<TS>();
                            while (dataReader.Read())
                            {
                                for (var i = 0; i < dataReader.FieldCount; i++)
                                    AttributesOfClass<TS>.SetValueFreeSql.Value[dataReader.GetName(i)]((TS) employee,
                                        dataReader[i] == DBNull.Value ? null : dataReader[i]);
                                resDis.Add((TS) employee);
                            }
                        }
                        else
                        {
                            while (dataReader.Read())
                            {
                                dynamic employee = new ExpandoObject();
                                for (var i = 0; i < count; i++)
                                    ((IDictionary<string, object>) employee).Add(dataReader.GetName(i),
                                        dataReader[i] == DBNull.Value ? null : dataReader[i]);
                                resDis.Add((TS) employee);
                            }
                        }
                    }

                    dataReader.Dispose();
                    foreach (var par in _com.Parameters)
                        if (((IDataParameter) par).Direction == ParameterDirection.InputOutput ||
                            ((IDataParameter) par).Direction == ParameterDirection.Output ||
                            ((IDataParameter) par).Direction == ParameterDirection.ReturnValue)
                            _parOut.Add(((IDataParameter) par).ParameterName, ((IDataParameter) par).Value);
                    return resDis;
                }

                if (_listOne.Any(a => a.Operand == Evolution.Select && a.IsAgregate))
                {
                    dataReader = _com.ExecuteReader();
                    object rObj = null;
                    while (dataReader.Read())
                    {
                        rObj = dataReader[0];
                        break;
                    }

                    dataReader.Dispose();
                    var res = Utils.Convertor<TS>(rObj);
                    return res;
                }

                if (PingComposite(Evolution.Join))
                {
                    dataReader = _com.ExecuteReader();
                    var lres = new List<TS>();
                    var ss = _listOne.Single(a => a.Operand == Evolution.Join).NewConctructor;
                    if (ss == null)
                        while (dataReader.Read())
                            lres.Add((TS) dataReader[0]);
                    else
                        lres = Pizdaticus.GetListAnonymusObj<TS>(dataReader, ss);
                    return lres;
                }


                if (PingComposite(Evolution.Select) &&
                    !PingComposite(Evolution.SelectNew))
                {
                    var lres = new List<TS>();
                    dataReader = _com.ExecuteReader();
                    while (dataReader.Read()) lres.Add((TS) Utils.Convertor<TS>(dataReader[0]));
                    dataReader.Dispose();
                    bool isactive1;
                    var datasingl1 = Pizdaticus.SingleData(_listOne, lres, out isactive1);
                    return !isactive1 ? (object) lres : datasingl1;
                }

                if (PingComposite(Evolution.DistinctCastom))
                {
                    dataReader = _com.ExecuteReader();
                    var resDis = new List<object>();
                    while (dataReader.Read()) resDis.Add(dataReader[0]);
                    dataReader.Dispose();
                    return resDis;
                }

                if (PingComposite(Evolution.SelectNew))
                {
                    //todo ion100
                    var ss = _listOne.Single(a => a.Operand == Evolution.SelectNew).NewConctructor;
                    _com.CommandText = _com.CommandText.Replace(",?p", "?p");
                    dataReader = _com.ExecuteReader();
                    if (Utils.IsAnonymousType(typeof(TS)))
                    {
                        var lRes = Pizdaticus.GetListAnonymusObj<TS>(dataReader, ss);
                        bool isaActive1;
                        var dataSingl1 = Pizdaticus.SingleData(_listOne, lRes, out isaActive1);
                        var res = !isaActive1 ? (object) lRes : dataSingl1;
                        return res;
                    }
                    else
                    {
                        if (_listOne.Any(a => a.Operand == Evolution.GroupBy && a.ExpressionDelegate != null))
                        {
                            var lRes = Pizdaticus.GetListAnonymusObj<object>(dataReader, ss);
                            bool isActive1;
                            var dataSingl1 = Pizdaticus.SingleData(_listOne, lRes, out isActive1);
                            var res = !isActive1 ? lRes : dataSingl1;
                            return res;
                        }

                        throw new Exception("Не реализовано");
                    }
                }

                #endregion

                ////////////////////////////////////////////////////////////////////////////////////////////////

                dataReader = _com.ExecuteReader();
                if (_listOne.Any(a => a.Operand == Evolution.GroupBy && a.ExpressionDelegate != null))
                {
                    var lResult = AttributesOfClass<TS>.GetEnumerableObjectsGroupBy<T>(dataReader,
                        _listOne.First(a => a.Operand == Evolution.GroupBy).ExpressionDelegate);
                    return lResult;
                }

                var resd = AttributesOfClass<T>.GetEnumerableObjects(dataReader);
                bool isActive;
                var dataSingl = Pizdaticus.SingleData(_listOne, resd, out isActive);
                var ress2 = !isActive ? (object) resd : dataSingl;
                return ress2;
            }

            catch (Exception ex)
            {
                Configure.SendError(Utils.GetStringSql(_com), ex);
                return null;
            }

            finally
            {
                _sessione.ComDisposable(_com);
                if (dataReader != null)
                {
                    dataReader.Close();
                    dataReader.Dispose();
                }
            }
        }

        private string Translate(Expression expression, out Evolution ev1) //update
        {
            ITranslate sq;
            switch (Configure.Provider)
            {
                case ProviderName.MySql:
                    sq = new QueryTranslator<T>();
                    break;
                case ProviderName.MsSql:
                    sq = new QueryTranslatorMsSql<T>();
                    break;
                case ProviderName.Postgresql:
                    sq = new QueryTranslator<T>();
                    break;
                case ProviderName.Sqlite:
                    sq = new QueryTranslator<T>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (sq != null)
            {
                _listOne = sq.ListOne;
                _param = sq.Param;
                //todo ion100
                ListCastExpression.ForEach(a => sq.Translate(a.CastomExpression, a.TypeRevalytion, a.ParamList));

                var sql = sq.Translate(expression, out ev1);
                return sql;
            }

            throw new Exception("sq == null");
        }

        private string TranslateString(Expression expression, out Evolution ev1)
        {
            ITranslate sq;
            switch (Configure.Provider)
            {
                case ProviderName.MySql:
                    sq = new QueryTranslator<T>();
                    break;
                case ProviderName.MsSql:
                    sq = new QueryTranslatorMsSql<T>();
                    break;
                case ProviderName.Postgresql:
                    sq = new QueryTranslator<T>();
                    break;
                case ProviderName.Sqlite:
                    sq = new QueryTranslator<T>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _listOne = sq.ListOne;
            _param = sq.Param;
            ListCastExpression.ForEach(a => sq.Translate(a.CastomExpression, a.TypeRevalytion, a.ParamList));
            var eee = sq.Translate(expression, out ev1);
            return eee;
        }

        private string TranslateString(Expression expression, List<OneComprosite> comprosites,
            IDictionary<string, object> dictionary, string parstr)
        {
            ITranslate sq;
            switch (Configure.Provider)
            {
                case ProviderName.MySql:
                    sq = new QueryTranslator<T>();
                    break;
                case ProviderName.MsSql:
                    sq = new QueryTranslatorMsSql<T>();
                    break;
                case ProviderName.Postgresql:
                    sq = new QueryTranslator<T>();
                    break;
                case ProviderName.Sqlite:
                    sq = new QueryTranslator<T>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _listOne = sq.ListOne;
            _param = sq.Param;
            ListCastExpression.ForEach(a => sq.Translate(a.CastomExpression, a.TypeRevalytion, a.ParamList));
            Evolution ev1;
            var eee = sq.Translate(expression, out ev1, parstr);
            comprosites.AddRange(_listOne);
            if (_param != null && _param.Any())
                foreach (var d in _param)
                    dictionary.Add(d.Key, d.Value);
            return eee;
        }

        internal override string GetQueryTextForJoin(Expression expression, List<OneComprosite> comprosite,
            Dictionary<string, object> dictionary, string parStr)
        {
            return TranslateString(expression, comprosite, dictionary, parStr);
        }

        public IEnumerable<TS> ExecuteCall<TS>(Expression callExpr)
        {
            _isStoredPr = true;
            return (IEnumerable<TS>) Execute<TS>(callExpr);
        }

        public IEnumerable<TS> ExecuteCallParam<TS>(Expression callExpr, params ParameterStoredPr[] par)
        {
            _isStoredPr = true;
            if (par != null) _paramFreeStoredPr.AddRange(par);
            var res = (IEnumerable<TS>) ExecuteSPP<TS>(callExpr);

            foreach (var re in _parOut)
            {
                if (par == null) continue;
                var p = par.FirstOrDefault(a => a.Name == re.Key);
                if (p != null) p.Value = re.Value;
            }

            return res;
        }
    }
}