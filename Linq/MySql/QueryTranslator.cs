using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ORM_1_21_.Linq.MsSql;

namespace ORM_1_21_.Linq.MySql
{

    internal sealed class QueryTranslator<T> : ExpressionVisitor, ITranslate
    {
        private Evolution _currentEvalytion;
        private readonly List<OneComprosite> _listOne = new List<OneComprosite>();
        private StringBuilder _sb = new StringBuilder();
        private int _paramIndex;
        private string _paramStringName = "p";

        private bool PingComposite(Evolution eval)
        {
            return _listOne.Any(a => a.Operand == eval);
        }

        public QueryTranslator()
        {
            Param = new Dictionary<string, object>();
        }

        private string ParamName
        {
            get { return string.Format("{0}{1}", string.Format("{1}{0}", ParamStringName, Utils.Prefparam), ++_paramIndex); }
        }

        private string GetColumnName(string member, Type type)
        {
            var ss = AttributesOfClass<T>.GetNameFieldForQuery(member, type);
            return ss;

        }

        private Evolution CurrentEvalytion
        {
            get { return _currentEvalytion; }

            //set
            //{
            //    if (_currentEvalytion == 0)
            //        _currentEvalytion = value;
            //}
        }

        public Dictionary<string, object> Param { get; set; }

        public List<OneComprosite> ListOne
        {
            get { return _listOne; }
        }

        private string ParamStringName
        {
            get { return _paramStringName; }
            set { _paramStringName = value; }
        }

        private StringBuilder StringB
        {
            get { return _sb; }
            set { _sb = value; }
        }


        public string Translate(Expression expression, out Evolution ev)
        {
            _currentEvalytion = 0;
            Visit(expression);
            ev = CurrentEvalytion;
            var dd = new MySqlConstructorSql().GetStringSql<T>(ListOne);//,_joinCapital
            return dd;
        }

        public void Translate(Expression expression, Evolution ev, List<object> paramList)
        {
            _currentEvalytion = ev;
            Visit(expression);
            //if (ev == Evolution.FindLikeEndsWith || ev == Evolution.FindLikeStartsWith || ev == Evolution.FindLikeContains)
            //{
            //    _listOne.Add(new OneComprosite { Operand = Evolution.Where, Body = _sb.ToString() });
            //}
            if (ev == Evolution.Delete)
            {
                _listOne.Add(new OneComprosite { Operand = Evolution.Delete });
                _listOne.Add(new OneComprosite { Operand = Evolution.Where, Body = _sb.ToString() });
            }
            if (ev == Evolution.DistinctCastom)
            {
                ListOne.Add(new OneComprosite { Operand = Evolution.DistinctCastom, Body = _sb.ToString() });
            }

            if (ev == Evolution.GroupBy)
            {
                ListOne.Add(new OneComprosite { Operand = Evolution.GroupBy, Body = _sb.ToString() });
            }
            if (ev == Evolution.Limit)
            {
                if (paramList != null && paramList.Count == 2)
                    if (Configure.Provider == ProviderName.Postgresql|| Configure.Provider == ProviderName.Sqlite)
                    {
                        ListOne.Add(new OneComprosite { Operand = Evolution.Limit, Body = string.Format(" Limit {1} OFFSET {0}", paramList[0], paramList[1]) });
                    }
                    else
                    {
                        ListOne.Add(new OneComprosite { Operand = Evolution.Limit, Body = string.Format(" Limit {0},{1}", paramList[0], paramList[1]) });
                    }
                   
            }
            if (ev == Evolution.Update)
            {
                ListOne.Add(new OneComprosite { Operand = Evolution.Update, Body = _sb.ToString() });
            }
            if (ev == Evolution.OverCache)
            {
                ListOne.Add(new OneComprosite { Operand = Evolution.OverCache });
            }
            if (ev == Evolution.Join)
            {
                if (paramList != null)
                {
                    foreach (var d in (Dictionary<string, Object>)paramList[0])
                    {
                        Param.Add(d.Key, d.Value);
                    }

                    ListOne.Add(new OneComprosite { Operand = Evolution.Join, Body = paramList[1].ToString(), NewConctructor = paramList[2] });
                }
                else
                {
                    throw new Exception("paramList pizda");
                }
            }





            _sb.Length = 0;
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {

            if (m.Method.DeclaringType == typeof(V)
                && m.Method.Name == "FreeSql")
            {
                // CurrentEvalytion = Evolution.FreeSql;

                var constantExpression = (ConstantExpression)m.Object;
                if (constantExpression == null) return m;
                var val = m.Method.Invoke(constantExpression.Value, null);

                _listOne.Add(new OneComprosite { Body = val.ToString(), Operand = Evolution.FreeSql });
                return m;
            }


            if (m.Method.DeclaringType == typeof(Queryable)
                   && m.Method.Name == "ElementAt")
            {
                //  CurrentEvalytion = Evolution.ElementAt;
                Visit(m.Arguments[0]);
                StringB.Clear();
                Visit(m.Arguments[1]);
                var o = new OneComprosite
                {
                    Operand = Evolution.ElementAt,
                    Body = StringB.ToString()
                };
                ListOne.Add(o);
                StringB.Clear();


                return m;
            }

            if (m.Method.DeclaringType == typeof(Queryable)
                && m.Method.Name == "Contains")
            {
                var o = new OneComprosite
                {
                    Operand = Evolution.Contains,

                };
                ListOne.Add(o);
                Visit(m.Arguments[0]);
                StringB.Clear();
                Visit(m.Arguments[1]);

                var o1 = new OneComprosite
                {
                    Operand = Evolution.Where,
                    Body = StringB.ToString()
                };
                ListOne.Add(o1);
                var o2 = new OneComprosite
                {
                    Operand = Evolution.Count,
                };
                ListOne.Add(o2);
                var o3 = new OneComprosite
                {
                    Operand = Evolution.All,
                };
                ListOne.Add(o3);

                StringB.Clear();
                return m;
            }



            if (m.Method.DeclaringType == typeof(Queryable)
                 && m.Method.Name == "ElementAtOrDefault")
            {
                //  CurrentEvalytion = Evolution.ElementAtOrDefault;
                Visit(m.Arguments[0]);
                StringB.Clear();
                Visit(m.Arguments[1]);
                var o = new OneComprosite
                {
                    Operand = Evolution.ElementAtOrDefault,
                    Body = StringB.ToString()
                };
                ListOne.Add(o);
                StringB.Clear();


                return m;
            }

            if (m.Method.DeclaringType == typeof(Queryable)
                   && m.Method.Name == "SelectMany")
            {
                if (m.Arguments.Count == 2)
                {
                    return BindSelectMany(m, m.Arguments[0], GetLambda(m.Arguments[1]), null);
                }
                if (m.Arguments.Count == 3)
                {
                    return BindSelectMany(m, m.Arguments[0], GetLambda(m.Arguments[1]), GetLambda(m.Arguments[2]));
                }
            }

            if ((m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Join") ||
                (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "GroupJoin"))
            {
                // _currentTypeCode = Evolution.Join;
                //   _joinCapital.Join= VisitorJoin.LeftJoin;

                for (var i = 0; i < m.Arguments.Count; i++)
                {
                    StringB.Length = 0;
                    if (i == 0)
                    {
                        Visit(m.Arguments[0]);
                    }

                    if (i == 1)
                    {
                        Visit(m.Arguments[1]);
                    }

                    if (i == 2)
                    {

                        StringB.Length = 0;
                        Visit(m.Arguments[2]);
                    }
                    if (i == 3)
                    {

                        StringB.Length = 0;
                        Visit(m.Arguments[3]);
                    }

                    if (i == 4)
                    {
                        StringB.Length = 0;
                        Visit(m.Arguments[4]);
                    }

                }

                return m;
            }








            #region mysql








            if (m.Method.DeclaringType == typeof(DateTime))
            {
                switch (m.Method.Name)
                {
                    case "op_Subtract":
                        if (m.Arguments[1].Type == typeof(DateTime))
                        {
                            StringB.Append("DATEDIFF(");
                            Visit(m.Arguments[0]);
                            StringB.Append(", ");
                            Visit(m.Arguments[1]);
                            StringB.Append(")");
                            return m;
                        }
                        break;
                    case "AddYears":
                        StringB.Append("DATE_ADD(");
                        Visit(m.Object);
                        StringB.Append(", INTERVAL ");
                        Visit(m.Arguments[0]);
                        StringB.Append(" YEAR)");
                        return m;
                    case "AddMonths":
                        StringB.Append("DATE_ADD(");
                        Visit(m.Object);
                        StringB.Append(", INTERVAL ");
                        Visit(m.Arguments[0]);
                        StringB.Append(" MONTH)");
                        return m;
                    case "AddDays":
                        StringB.Append("DATE_ADD(");
                        Visit(m.Object);
                        StringB.Append(", INTERVAL ");
                        Visit(m.Arguments[0]);
                        StringB.Append(" DAY)");
                        return m;
                    case "AddHours":
                        StringB.Append("DATE_ADD(");
                        Visit(m.Object);
                        StringB.Append(", INTERVAL ");
                        Visit(m.Arguments[0]);
                        StringB.Append(" HOUR)");
                        return m;
                    case "AddMinutes":
                        StringB.Append("DATE_ADD(");
                        Visit(m.Object);
                        StringB.Append(", INTERVAL ");
                        Visit(m.Arguments[0]);
                        StringB.Append(" MINUTE)");
                        return m;
                    case "AddSeconds":
                        StringB.Append("DATE_ADD(");
                        Visit(m.Object);
                        StringB.Append(", INTERVAL ");
                        Visit(m.Arguments[0]);
                        StringB.Append(" SECOND)");
                        return m;
                    case "AddMilliseconds":
                        StringB.Append("DATE_ADD(");
                        Visit(m.Object);
                        StringB.Append(", INTERVAL (");
                        Visit(m.Arguments[0]);
                        StringB.Append("* 1000) MICROSECOND)");
                        return m;
                }
            }
            if (m.Method.ReturnType == typeof(char[]) && m.Method.Name == "ToArray")
            {
                Visit(m.Arguments[0]);//Trin("ddas".ToArray())
            }
            if (m.Method.DeclaringType == typeof(string) || m.Method.DeclaringType == typeof(Enumerable))
            {
                switch (m.Method.Name)
                {
                    case "StartsWith":
                        StringB.Append("(");
                        Visit(m.Object);
                        if (Configure.Provider == ProviderName.Sqlite)
                        {
                            StringB.AppendFormat(" {0} ", StringConst.Like);
                            Visit(m.Arguments[0]);
                            StringB.Append(")");
                        }
                        else
                        {
                            StringB.AppendFormat(" {0} CONCAT(", StringConst.Like);
                            Visit(m.Arguments[0]);
                            StringB.Append(",'%'))");
                        }

                       
                        return m;
                    case "LikeSql":
                    {
                        return m;
                    }
                    case "EndsWith":
                        if (Configure.Provider == ProviderName.Sqlite)
                        {
                            StringB.Append("(");
                            Visit(m.Object);
                            StringB.AppendFormat(" {0} ", StringConst.Like);
                            Visit(m.Arguments[0]);
                            StringB.Append(")");
                        }
                        else
                        {
                            StringB.Append("(");
                            Visit(m.Object);
                            StringB.AppendFormat(" {0} CONCAT('%',", StringConst.Like);
                            Visit(m.Arguments[0]);
                            StringB.Append("))");
                        }
                        return m;
                    case "Contains":
                        if (Configure.Provider == ProviderName.Sqlite)
                        {
                            StringB.Append("(");
                            Visit(m.Object);
                            StringB.AppendFormat(" {0} ", StringConst.Like);
                            Visit(m.Arguments[0]);
                            StringB.Append(")");
                        }
                        else
                        {
                            StringB.Append("(");
                            Visit(m.Object);
                            StringB.AppendFormat(" {0} CONCAT('%',", StringConst.Like);
                            Visit(m.Arguments[0]);
                            StringB.Append(",'%'))");
                        }
                        return m;
                    case "Concat":
                        IList<Expression> args = m.Arguments;
                        if (args.Count == 1 && args[0].NodeType == ExpressionType.NewArrayInit)
                        {
                            args = ((NewArrayExpression)args[0]).Expressions;
                        }
                        StringB.Append("CONCAT(");
                        for (int i = 0, n = args.Count; i < n; i++)
                        {
                            if (i > 0) StringB.Append(", ");
                            Visit(args[i]);
                        }
                        StringB.Append(")");
                        return m;
                    case "IsNullOrEmpty":
                        StringB.Append("(");
                        Visit(m.Arguments[0]);
                        StringB.Append(" IS NULL OR ");
                        Visit(m.Arguments[0]);
                        StringB.Append(" = '')");
                        return m;
                    case "ToUpper":
                        StringB.Append("UPPER(");
                        Visit(m.Object);
                        StringB.Append(")");
                        return m;
                    case "ToLower":
                        StringB.Append("LOWER(");
                        Visit(m.Object);
                        StringB.Append(")");
                        return m;
                    case "Replace":
                        StringB.Append("REPLACE(");
                        Visit(m.Object);
                        StringB.Append(", ");
                        Visit(m.Arguments[0]);
                        StringB.Append(", ");
                        Visit(m.Arguments[1]);
                        StringB.Append(")");
                        return m;
                    case "Substring":
                        StringB.Append("SUBSTRING(");
                        Visit(m.Object);
                        StringB.Append(", ");
                        Visit(m.Arguments[0]);
                        StringB.Append(" + 1");
                        if (m.Arguments.Count == 2)
                        {
                            StringB.Append(", ");
                            Visit(m.Arguments[1]);
                        }
                        StringB.Append(")");
                        return m;
                    case "Remove":
                        if (m.Arguments.Count == 1)
                        {
                            StringB.Append("LEFT(");
                            Visit(m.Object);
                            StringB.Append(", ");
                            Visit(m.Arguments[0]);
                            StringB.Append(")");
                        }
                        else
                        {
                            StringB.Append("CONCAT(");
                            StringB.Append("LEFT(");
                            Visit(m.Object);
                            StringB.Append(", ");
                            Visit(m.Arguments[0]);
                            StringB.Append("), SUBSTRING(");
                            Visit(m.Object);
                            StringB.Append(", ");
                            Visit(m.Arguments[0]);
                            StringB.Append(" + ");
                            Visit(m.Arguments[1]);
                            StringB.Append("))");
                        }
                        return m;
                    case "IndexOf":
                        StringB.Append("(LOCATE(");
                        Visit(m.Arguments[0]);
                        StringB.Append(", ");
                        Visit(m.Object);
                        if (m.Arguments.Count == 2 && m.Arguments[1].Type == typeof(int))
                        {
                            StringB.Append(", ");
                            Visit(m.Arguments[1]);
                            StringB.Append(" + 1");
                        }
                        StringB.Append(") - 1)");
                        return m;
                    case "Trim":
                        if (m.Arguments.Count == 0)
                        {
                            StringB.Append("TRIM(");
                            Visit(m.Object);
                            StringB.Append(")");
                        }
                        else if (m.Arguments.Count == 1)
                        {
                            StringB.Append("TRIM(LEADING ");
                            Visit(m.Arguments[0]);
                            StringB = StringB.Replace(",", "");
                            StringB.Append(" FROM ");
                            Visit(m.Object);
                            StringB.Append(")");

                        }
                        return m;
                    case "TrimEnd":
                        StringB.Append("RTRIM(");
                        Visit(m.Object);
                        StringB.Append(")");
                        return m;
                    case "TrimStart":
                        StringB.Append("LTRIM(");
                        Visit(m.Object);
                        StringB.Append(")");
                        return m;
                }
            }
            if (m.Method.DeclaringType == typeof(Decimal))
            {
                switch (m.Method.Name)
                {
                    case "Add":
                    case "Subtract":
                    case "Multiply":
                    case "Divide":
                    case "Remainder":
                        StringB.Append("(");
                        VisitValue(m.Arguments[0]);
                        StringB.Append(" ");
                        StringB.Append(GetOperator(m.Method.Name));
                        StringB.Append(" ");
                        VisitValue(m.Arguments[1]);
                        StringB.Append(")");
                        return m;
                    case "Negate":
                        StringB.Append("-");
                        Visit(m.Arguments[0]);
                        StringB.Append("");
                        return m;
                    case "Ceiling":
                    case "Floor":
                        StringB.Append(m.Method.Name.ToUpper());
                        StringB.Append("(");
                        Visit(m.Arguments[0]);
                        StringB.Append(")");
                        return m;
                    case "Round":
                        if (m.Arguments.Count == 1)
                        {
                            StringB.Append("ROUND(");
                            Visit(m.Arguments[0]);
                            StringB.Append(")");
                            return m;
                        }
                        if (m.Arguments.Count == 2 && m.Arguments[1].Type == typeof(int))
                        {
                            StringB.Append("ROUND(");
                            Visit(m.Arguments[0]);
                            StringB.Append(", ");
                            Visit(m.Arguments[1]);
                            StringB.Append(")");
                            return m;
                        }
                        break;
                    case "Truncate":
                        StringB.Append("TRUNCATE(");
                        Visit(m.Arguments[0]);
                        StringB.Append(",0)");
                        return m;
                }
            }
            else if (m.Method.DeclaringType == typeof(Math))
            {
                switch (m.Method.Name)
                {
                    case "Abs":
                    case "Acos":
                    case "Asin":
                    case "Atan":
                    case "Atan2":
                    case "Cos":
                    case "Exp":
                    case "Log10":
                    case "Sin":
                    case "Tan":
                    case "Sqrt":
                    case "Sign":
                    case "Ceiling":
                    case "Floor":
                        StringB.Append(m.Method.Name.ToUpper());
                        StringB.Append("(");
                        Visit(m.Arguments[0]);
                        StringB.Append(")");
                        return m;
                    case "Log":
                        if (m.Arguments.Count == 1)
                        {
                            goto case "Log10";
                        }
                        break;
                    case "Pow":
                        StringB.Append("POWER(");
                        Visit(m.Arguments[0]);
                        StringB.Append(", ");
                        Visit(m.Arguments[1]);
                        StringB.Append(")");
                        return m;
                    case "Round":
                        if (m.Arguments.Count == 1)
                        {
                            StringB.Append("ROUND(");
                            Visit(m.Arguments[0]);
                            StringB.Append(")");
                            return m;
                        }
                        if (m.Arguments.Count == 2 && m.Arguments[1].Type == typeof(int))
                        {
                            StringB.Append("ROUND(");
                            Visit(m.Arguments[0]);
                            StringB.Append(", ");
                            Visit(m.Arguments[1]);
                            StringB.Append(")");
                            return m;
                        }
                        break;
                    case "Truncate":
                        StringB.Append("TRUNCATE(");
                        Visit(m.Arguments[0]);
                        StringB.Append(",0)");
                        return m;
                }
            }

            #endregion








            if (m.Method.DeclaringType == typeof(Queryable)
                && m.Method.Name == "Where")
            {
                //if(_currentTypeCode!= Evolution.Join)
                //_currentTypeCode= Evolution.Where;
                var o = new OneComprosite { Operand = Evolution.Where };
                Visit(m.Arguments[0]);
                var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);

                Visit(lambda.Body);
                o.Body = StringB.ToString();
                ListOne.Add(o);
                StringB.Length = 0;


                return m;
            }

            if (m.Method.DeclaringType == typeof(Queryable)
                && m.Method.Name == "GroupBy")
            {
                Visit(m.Arguments[0]);
                var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                //var type = ((MemberExpression)lambda.Body).Expression.Type;
                Delegate tt;






                var typew = ((MemberExpression)lambda.Body).Expression.Type;
                if (typew != typeof(T) && Utils.IsAnonymousType(typew) && ListOne.Any(a => a.Operand == Evolution.SelectNew))
                {
                   //var trtyetr= typeof (T);
                   // throw new Exception("не реализовано");
                    //tt = Pizdaticus.GetDelegateForGroupBy((LambdaExpression)StripQuotes(m.Arguments[1]));
                   tt= ((LambdaExpression)StripQuotes(m.Arguments[1])).Compile();
                }
                else
                {
                    Expression exp = StripQuotes(m.Arguments[1]);
                    LambdaExpression lexp = (LambdaExpression) exp;
                   tt= GroupExpression<T>.Delegate(lexp);
                    // if (lexp.ReturnType == typeof(Guid))
                    // {
                    //     tt = (Func<T, Guid>)(lexp).Compile();
                    // }
                    // else if (lexp.ReturnType == typeof(DateTime))
                    // {
                    //     tt = (Func<T, DateTime>)(lexp).Compile();
                    // }
                    ////else if (lexp.ReturnType.BaseType == typeof(Enum))
                    ////{
                    ////    tt = (Func<T, Enum>)(lexp).Compile();
                    ////}
                    // else
                    // {
                    //     tt = (Func<T, object>)(lexp).Compile();
                    //
                    // }


                }
                Visit(lambda.Body);



                var o = new OneComprosite
                            {
                                Operand = Evolution.GroupBy,
                                Body = StringB.ToString(),
                                ExpressionDelegate = tt

                            };

                ListOne.Add(o);
                StringB.Length = 0;

                return m;
            }


            if (m.Method.DeclaringType == typeof(Queryable)
                && (m.Method.Name == "OrderBy" || m.Method.Name == "ThenBy"))
            {
                Visit(m.Arguments[0]);
                var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                Visit(lambda.Body);
                var o = new OneComprosite { Operand = Evolution.OrderBy, Body = StringB.ToString().Trim(' ', ',') };
                ListOne.Add(o);
                StringB.Length = 0;
                return m;
            }

            if (m.Method.DeclaringType == typeof(Queryable)
               && m.Method.Name == "Reverse")
            {
                Visit(m.Arguments[0]);
                var sb = new StringBuilder();
                if (ListOne.Any(a => a.Operand == Evolution.OrderBy))
                {
                    ListOne.Where(a => a.Operand == Evolution.OrderBy).ToList().ForEach(a =>
                                                                                           {
                                                                                               sb.AppendFormat(" {0},", a.Body);
                                                                                               ListOne.Remove(a);
                                                                                           });

                }
                else
                {
                    sb.AppendFormat("{0}.{1}", AttributesOfClass<T>.TableName, AttributesOfClass<T>.PkAttribute.ColumnName);
                }


                var o = new OneComprosite { Operand = Evolution.Reverse, Body = string.Format("ORDER BY {0} DESC ", sb.ToString().TrimEnd(',')) };
                ListOne.Add(o);
                StringB.Length = 0;
                return m;
            }

            if (m.Method.DeclaringType == typeof(Queryable)
               && m.Method.Name == "OrderByDescending" || m.Method.Name == "ThenByDescending")
            {
                Visit(m.Arguments[0]);
                var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                Visit(lambda.Body);
                var o = new OneComprosite { Operand = Evolution.OrderBy, Body = StringB.ToString().Trim(' ', ',') + " DESC " };
                //if (m.Method.Name == "OrderByDescending" && m.Arguments.Count == 3)
                //{
                //    //var tt = Expression.Lambda<Func<T, int>>(m.Arguments[1], true, m.Arguments[1]).Compile(); ;
                //    var tt = new List<T>().OrderByDescending((Func<T, object>)((LambdaExpression)StripQuotes(m.Arguments[1])).Compile());
                //    var ree = m.Arguments[1];//.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
                //}
                ListOne.Add(o);
                StringB.Length = 0;
                return m;
            }


            if (m.Method.DeclaringType == typeof(Queryable)
                && m.Method.Name == "Select")
            {
                Visit(m.Arguments[0]);
                var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                Visit(lambda.Body);
                var o = new OneComprosite { Operand = Evolution.Select, Body = _sb.ToString().Trim(new[] { ' ', ',' }) };
                if (!string.IsNullOrEmpty(StringB.ToString()))
                {
                    _listOne.Add(o);
                }
                StringB.Length = 0;
                return m;
            }
            if (m.Method.DeclaringType == typeof(Queryable)
                && m.Method.Name == "First")
            {
                // CurrentEvalytion = (Evolution)Enum.Parse(typeof(Evolution), m.Method.Name);
                Visit(m.Arguments[0]);
                if (m.Arguments.Count == 2)
                {
                    var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                    Visit(lambda.Body);

                    var o1 = new OneComprosite { Operand = Evolution.Where, Body = StringB.ToString() };
                    ListOne.Add(o1);
                    StringB.Length = 0;
                    var o2 = new OneComprosite { Operand = Evolution.First };
                    ListOne.Add(o2);
                    StringB.Length = 0;
                    return m;
                }
                var o = new OneComprosite { Operand = Evolution.First, Body = StringB.ToString() };
                ListOne.Add(o);
                StringB.Length = 0;

                return m;
            }

            if (m.Method.DeclaringType == typeof(Queryable)
                && m.Method.Name == "FirstOrDefault")
            {
                //CurrentEvalytion = (Evolution)Enum.Parse(typeof(Evolution), m.Method.Name);
                Visit(m.Arguments[0]);
                if (m.Arguments.Count == 2)
                {
                    var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                    Visit(lambda.Body);

                    var o1 = new OneComprosite { Operand = Evolution.Where, Body = StringB.ToString() };
                    ListOne.Add(o1);
                    StringB.Length = 0;
                    var o2 = new OneComprosite { Operand = Evolution.FirstOrDefault };
                    ListOne.Add(o2);
                    StringB.Length = 0;
                    return m;
                }
                var o = new OneComprosite { Operand = Evolution.FirstOrDefault, Body = StringB.ToString() };
                ListOne.Add(o);
                StringB.Length = 0;

                return m;
            }

            if (m.Method.DeclaringType == typeof(Queryable)
                && (m.Method.Name == "All"))
            {
                //CurrentEvalytion = Evolution.All;
                Visit(m.Arguments[0]);
                var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                Visit(lambda.Body);
                var o1 = new OneComprosite { Operand = Evolution.All, Body = StringB.ToString() };
                ListOne.Add(o1);



                return m;

            }






            if (m.Method.DeclaringType == typeof(Queryable)
                && (m.Method.Name == "Any"))
            {
                //CurrentEvalytion = Evolution.Any;
                Visit(m.Arguments[0]);
                if (m.Arguments.Count == 2)
                {
                    var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                    Visit(lambda.Body);
                    var o1 = new OneComprosite { Operand = Evolution.Where, Body = StringB.ToString() };
                    ListOne.Add(o1);
                    StringB.Length = 0;
                    var o2 = new OneComprosite { Operand = Evolution.Any };
                    ListOne.Add(o2);
                    StringB.Length = 0;
                    return m;
                }
                var o = new OneComprosite { Operand = Evolution.Where, Body = StringB.ToString() };
                ListOne.Add(o);
                ListOne.Add(new OneComprosite { Operand = Evolution.Any, });
                StringB.Length = 0;

                return m;
            }

            if (m.Method.DeclaringType == typeof(Queryable)
                && m.Method.Name == "Last" || m.Method.Name == "LastOrDefault")
            {
                var ee = (Evolution)Enum.Parse(typeof(Evolution), m.Method.Name);
                Visit(m.Arguments[0]);

                if (m.Arguments.Count == 2)
                {
                    var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                    Visit(lambda.Body);
                    var o1 = new OneComprosite
                                 {
                                     Operand = Evolution.Where,
                                     Body = StringB.ToString()
                                 };
                    ListOne.Add(o1);
                    StringB.Length = 0;

                }
                if (ListOne.Any(a => a.Operand == Evolution.OrderBy))
                {
                    foreach (var body in ListOne.Where(a => a.Operand == Evolution.OrderBy))
                    {
                        if (body.Body.IndexOf("DESC", StringComparison.Ordinal) != -1)
                        {
                            body.Body = body.Body.Replace("DESC", string.Empty);
                        }
                        else
                        {
                            body.Body = body.Body + " DESC";
                        }
                    }
                    ListOne.Last(a => a.Operand == Evolution.OrderBy).Body += " LIMIT 1";

                }
                else
                {
                    var o = new OneComprosite
                            {
                                Operand = Evolution.OrderBy,
                                Body = string.Format(" {0}.{1} DESC LIMIT 1", AttributesOfClass<T>.TableName, AttributesOfClass<T>.PkAttribute.ColumnName)
                            };
                    ListOne.Add(o);
                }
                var os = new OneComprosite
                {
                    Operand = ee,

                };
                ListOne.Add(os);

                return m;
            }

            if (m.Method.DeclaringType == typeof(Queryable)
                && (m.Method.Name == "Count" || m.Method.Name == "LongCount"))
            {
                //  var dd = m.Arguments[0];
                //CurrentEvalytion = Evolution.Count;
                Visit(m.Arguments[0]);
                if (m.Arguments.Count == 2)
                {
                    var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                    Visit(lambda.Body);
                    var o1 = new OneComprosite { Operand = Evolution.Where, Body = StringB.ToString() };
                    ListOne.Add(o1);
                    StringB.Length = 0;
                    var o2 = new OneComprosite { Operand = Evolution.Count };
                    ListOne.Add(o2);
                    StringB.Length = 0;
                    return m;
                }
                var o = new OneComprosite { Operand = Evolution.Count, Body = StringB.ToString() };//|| m.Method.Name == "SingleOrDefault"
                ListOne.Add(o);
                StringB.Length = 0;
                return m;
            }


            if (m.Method.DeclaringType == typeof(Queryable)
              && m.Method.Name == "Single")
            {
                ListOne.Add(new OneComprosite { Operand = Evolution.Single, Body = "", IsAgregate = true });

                Visit(m.Arguments[0]);

                if (m.Arguments.Count == 2)
                {
                    var o = new OneComprosite { Operand = Evolution.Where };
                    Visit(m.Arguments[0]);
                    var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                    Visit(lambda.Body);
                    o.Body = StringB.ToString();
                    ListOne.Add(o);
                    StringB.Length = 0;
                }


                ListOne.Add(new OneComprosite { Operand = Evolution.First, Body = "" });

                return m;
            }

            if (m.Method.DeclaringType == typeof(Queryable)
             && m.Method.Name == "SingleOrDefault")
            {
                ListOne.Add(new OneComprosite { Operand = Evolution.SingleOrDefault, Body = "", IsAgregate = true });
                ListOne.Add(new OneComprosite { Operand = Evolution.First, Body = "" });
                Visit(m.Arguments[0]);

                if (m.Arguments.Count == 2)
                {

                    var o = new OneComprosite { Operand = Evolution.Where };

                    var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                    Visit(lambda.Body);
                    o.Body = StringB.ToString();
                    ListOne.Add(o);

                    StringB.Length = 0;
                }
                return m;
            }



            if (m.Method.MemberType == MemberTypes.Method)
            {
                var mcs = m;
                if (mcs.Method.DeclaringType == typeof(Queryable)
                    || mcs.Method.DeclaringType == typeof(Enumerable))
                {

                    switch (mcs.Method.Name)
                    {
                        case "Count":
                        case "LongCount":
                        case "Sum":
                        case "Min":
                        case "Max":
                        case "Average":
                            {
                                // var e = mcs.Arguments;
                                Visit(mcs.Arguments[0]);
                                var lambda = (LambdaExpression)StripQuotes(mcs.Arguments[1]);
                                StringB.Length = 0;
                                StringB.Append(mcs.Method.Name + "(");
                                Visit(lambda.Body);
                                StringB.Append(")");
                                ListOne.Add(new OneComprosite { Operand = Evolution.Select, Body = StringB.ToString(), IsAgregate = true });

                                break;
                            }
                    }
                    return m;
                }

                if (m.Method.MemberType == MemberTypes.Field)
                {

                    var df = m.Arguments.Select(a => a.GetType().GetProperty("Value").GetValue(a, null));
                    var values = m.Method.Invoke(m, df.ToArray());
                    AddParametr(values);
                    return m;
                }

                if (m.Method.MemberType == MemberTypes.Property)
                {

                    var df = m.Arguments.Select(a => a.GetType().GetProperty("Value").GetValue(a, null));
                    var value = m.Method.Invoke(m, df.ToArray());
                    AddParametr(value);
                    return m;
                }





                if (m.Method.Name == "Querion")
                {
                    return m;
                }

                if (m.Method.Name == "get_Item")
                {
                    if (m.Object != null)
                    {
                        var value = Expression.Lambda<Func<object>>(m.Object).Compile()();
                        var dddd = m.Arguments[0].GetType().GetProperty("Value").GetValue(m.Arguments[0], null);
                        var tt = m.Method.Invoke(value, new[] { dddd });
                        AddParametr(tt);
                    }
                    return m;
                }



                var la = new List<object>();
                foreach (var i in m.Arguments)
                {
                    if (i.GetType().GetProperty("Value") == null)//System.Linq.Expressions.InstanceMethodCallExpressionN
                    {
                        var value = Expression.Lambda<Func<object>>(i).Compile()();
                        la.Add(value);
                    }
                    else
                    {
                        la.Add(i.GetType().GetProperty("Value").GetValue(i, null));

                    }
                }
                var value3 = m.Method.Invoke(m, la.ToArray());
                AddParametr(value3);
                return m;

            }

            throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture,
                "The method '{0}' is not supported", m.Method.Name));

        }



        private static LambdaExpression GetLambda(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            if (e.NodeType == ExpressionType.Constant)
            {
                return ((ConstantExpression)e).Value as LambdaExpression;
            }
            return e as LambdaExpression;
        }



        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    StringB.Append(" NOT ");
                    Visit(u.Operand);
                    break;

                case ExpressionType.Convert:
                    Visit(u.Operand);
                    break;

                case ExpressionType.Quote:
                    Visit(u.Operand);
                    break;

                default:
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture,
                        "The unary operator '{0}' is not supported", u.NodeType));
            }
            return u;
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            StringB.Append("(");
            Visit(b.Left);
            switch (b.NodeType)
            {
                case ExpressionType.AndAlso:

                case ExpressionType.And:
                    StringB.Append(" and ");
                    break;
                case ExpressionType.Or:
                    StringB.Append(" or ");
                    break;

                case ExpressionType.Add:
                    StringB.Append("+");
                    break;

                case ExpressionType.OrElse:
                    StringB.Append(" or ");
                    break;



                case ExpressionType.Equal:


                    if (b.Right.ToString() == "null")
                    {
                        StringB.Append(" is ");
                        break;
                    }
                    if (b.Right is UnaryExpression && ((UnaryExpression)b.Right).Operand.ToString() == "null")
                    {
                        StringB.Append(" is ");
                        break;
                    }
                    //if (_listOne.Any(a=>a.Operand == Evolution.FindLikeContains))
                    //{
                    //    StringB.Append(" LIKE CONCAT('%',");
                    //    Visit(b.Right);
                    //    StringB.Append(",'%')) ");

                    //    return b;
                    //}

                    //if (_listOne.Any(a=>a.Operand == Evolution.FindLikeStartsWith))
                    //{
                    //    StringB.Append(" LIKE CONCAT(");
                    //    Visit(b.Right);
                    //    StringB.Append(",'%')) ");

                    //    return b;
                    //}

                    //if (_listOne.Any(a=>a.Operand == Evolution.FindLikeEndsWith))
                    //{
                    //    StringB.Append(" LIKE CONCAT('%',");
                    //    Visit(b.Right);
                    //    StringB.Append(")) ");

                    //    return b;
                    //}
                    StringB.Append(" = ");
                    break;
                case ExpressionType.NotEqual:

                    if (b.Right.ToString() == "null")
                    {
                        StringB.Append(" is not  ");
                        break;
                    }
                    if (b.Right is UnaryExpression && ((UnaryExpression)b.Right).Operand.ToString() == "null")
                    {
                        StringB.Append(" is not  ");
                        break;
                    }


                    StringB.Append(" <> ");
                    break;
                case ExpressionType.LessThan:
                    StringB.Append(" < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    StringB.Append(" <= ");
                    break;
                case ExpressionType.GreaterThan:
                    StringB.Append(" > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    StringB.Append(" >= ");
                    break;
                case ExpressionType.Divide:
                    StringB.Append(" / ");
                    break;
                case ExpressionType.Modulo:
                    StringB.Append("%");
                    break;
                case ExpressionType.ExclusiveOr:
                    StringB.Append("^");
                    break;
                case ExpressionType.LeftShift:
                    StringB.Append("<<");
                    break;
                case ExpressionType.RightShift:
                    StringB.Append(">>");
                    break;

                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    StringB.Append("-");
                    break;
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    StringB.Append("*");
                    break;
                default:
                    throw new NotSupportedException(
                        string.Format(CultureInfo.CurrentCulture,
                            "The binary operator '{0}' is not supported", b.NodeType));
            }
            Visit(b.Right);
            StringB.Append(")");
            return b;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {

            var q = c.Value as IQueryable;

            if (q != null)
            {
                return c;
            }
            if (c.Value == null)
            {
                StringB.Append("null");
            }
            else
            {

                switch (Type.GetTypeCode(c.Value.GetType()))
                {
                    case TypeCode.Boolean:
                        if (Configure.Provider == ProviderName.Postgresql)
                        {
                            StringB.Append(((bool)c.Value));
                        }
                        else
                        {
                            StringB.Append(((bool)c.Value) ? 1 : 0);

                        }
                        
                        break;
                    case TypeCode.String:

                        var p = ParamName;
                        StringB.Append(p);
                        Param.Add(p, c.Value);

                        break;
                    case TypeCode.Object:
                        {
                            if (c.Value is T && PingComposite(Evolution.Contains))
                            {
                                var o = (T)c.Value;
                                var propertyname = AttributesOfClass<T>.PkAttribute.PropertyName;
                                var value = AttributesOfClass<T>.GetValue.Value[propertyname](o);
                                var tablenane = AttributesOfClass<T>.TableName;
                                var key = AttributesOfClass<T>.PkAttribute.ColumnName;
                                StringB.Append(string.Format("({0}.{1} = '{2}')", tablenane, key, value));
                                break;
                            }

                            throw new NotSupportedException(
                                string.Format(CultureInfo.CurrentCulture,
                                    "The constant for '{0}' is not supported", c.Value));
                        }
                    default:
                        AddParametr(c.Value);
                        break;
                }
            }
            return c;
        }


        protected override Expression VisitMemberAccess(MemberExpression m)
        {

            if (m.Expression != null
                && m.Expression.NodeType == ExpressionType.Parameter)
            {


                if (m.Expression.Type != typeof(T))
                {
                    if (Utils.IsAnonymousType(m.Expression.Type))
                    {
                        return m;
                    }
                }
                else
                {
                    StringB.Append(GetColumnName(m.Member.Name, m.Expression.Type));
                }



                return m;
            }
            if (m.Expression != null
                && m.Expression.NodeType == ExpressionType.New)
            {
                var alias = m.Expression.GetType().GetProperty("Name").GetValue(m.Expression, null);
                StringB.Append(GetColumnName(m.Member.Name, m.Expression.Type));
                return m;
            }

            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Constant)
            {
                VisitorMemberAccess(m);
                return m;
            }
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.MemberAccess)
            {
                VisitorMemberAccess(m);
                return m;
            }


            if (m.Expression != null && m.NodeType == ExpressionType.MemberAccess)
            {
                VisitorMemberAccess(m);
                return m;
            }

            if (m.Expression == null && m.NodeType == ExpressionType.MemberAccess)
            {
                var ee = m.Type;
                if (m.Type == typeof(int))
                {
                    var value = Expression.Lambda<Func<int>>(m).Compile()();
                    AddParametr(value);
                    return m;
                }

                if (m.Type == typeof(long))
                {
                    var value = Expression.Lambda<Func<long>>(m).Compile()();
                    AddParametr(value);
                    return m;
                }
                if (m.Member.ReflectedType == typeof(DateTime))
                {
                    var value = Expression.Lambda<Func<DateTime>>(m).Compile()();
                    AddParametr(value);
                    return m;
                }
                if (m.Type == typeof(Guid))
                {
                    var value = Expression.Lambda<Func<Guid>>(m).Compile()();
                    AddParametr(value);
                    return m;
                }

                var rr = m.Type;
                var str = Expression.Lambda<Func<object>>(m).Compile().Invoke();
                AddParametr(str);
                return m;
            }

            throw new NotSupportedException(
                string.Format(CultureInfo.CurrentCulture, "The member '{0}' is not supported", m.Member.Name));
        }

        private void VisitorMemberAccess(MemberExpression m)
        {
            object value;
            if (m.Member.DeclaringType == typeof(string))
            {
                switch (m.Member.Name)
                {
                    case "Length":
                        StringB.Append("CHAR_LENGTH(");
                        Visit(m.Expression);
                        StringB.Append(")");
                        return;
                }
            }

            if (m.Member.ReflectedType == typeof(DateTime))
            {

                if (m.Member.DeclaringType == typeof(DateTimeOffset))
                {
                    throw new Exception("m.Member.DeclaringType == typeof(DateTimeOffset)");
                }


                switch (m.Member.Name)
                {
                    case "Day":
                        StringB.Append("DAY(");
                        Visit(m.Expression);
                        StringB.Append(")");
                        return;
                    case "Month":
                        StringB.Append("MONTH(");
                        Visit(m.Expression);
                        StringB.Append(")");
                        return;
                    case "Year":
                        StringB.Append("YEAR(");
                        Visit(m.Expression);
                        StringB.Append(")");
                        return;
                    case "Hour":
                        StringB.Append("HOUR(");
                        Visit(m.Expression);
                        StringB.Append(")");
                        return;
                    case "Minute":
                        StringB.Append("MINUTE(");
                        Visit(m.Expression);
                        StringB.Append(")");
                        return;
                    case "Second":
                        StringB.Append("SECOND(");
                        Visit(m.Expression);
                        StringB.Append(")");
                        return;
                    case "Millisecond":
                        StringB.Append("(MICROSECOND(");
                        Visit(m.Expression);
                        StringB.Append(")/1000)");
                        return;
                    case "DayOfWeek":
                        StringB.Append("(DAYOFWEEK(");
                        Visit(m.Expression);
                        StringB.Append(") - 1)");
                        return;
                    case "DayOfYear":
                        StringB.Append("(DAYOFYEAR(");
                        Visit(m.Expression);
                        StringB.Append(") - 1)");
                        return;
                }
                var str1 = Expression.Lambda<Func<DateTime>>(m.Expression).Compile()();
                var ss = str1.GetType().GetProperty(m.Member.Name);
                value = ss.GetValue(str1, null);

                AddParametr(value);

                return;
            }

            var strs = new JoinAlias().GetAlias(m.Expression);
            if (strs != null && strs.IndexOf("TransparentIdentifier", StringComparison.Ordinal) != -1)
            {
                var eee = m.Expression.GetType().GetProperty("Member").GetValue(m.Expression, null);
                var name = eee.GetType().GetProperty("Name").GetValue(eee, null);
                StringB.Append(GetColumnName(m.Member.Name, m.Expression.Type));
                return;
                //m.Member.DeclaringType
            }

            var str = Expression.Lambda<Func<object>>(m.Expression).Compile()();
            value = null;
            if (m.Member.MemberType == MemberTypes.Field)
            {
                var fieldInfo = str.GetType().GetField(m.Member.Name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                if (fieldInfo != null)

                    value = fieldInfo.GetValue(str);
            }
            if (m.Member.MemberType == MemberTypes.Property)
            {
                var ass = str.GetType().GetProperty(m.Member.Name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                value = ass.GetValue(str, null);
            }
            AddParametr(value);
        }

        private void AddParametr(object value)
        {
            var p1 = ParamName;
            StringB.Append(p1);
            Param.Add(p1, value);
        }


        private Expression VisitValue(Expression expr)
        {
            if (IsPredicate(expr))
            {
                StringB.Append("CASE WHEN (");
                Visit(expr);
                StringB.Append(") THEN 1 ELSE 0 END");
                return expr;
            }
            return Visit(expr);
        }

        private static bool IsPredicate(Expression expr)
        {
            switch (expr.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return IsBoolean(expr.Type);
                case ExpressionType.Not:
                    return IsBoolean(expr.Type);
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                    return true;
                case ExpressionType.Call:
                    return IsBoolean(expr.Type);
                default:
                    return false;
            }
        }

        private static bool IsBoolean(Type type)
        {
            return type == typeof(bool) || type == typeof(bool?);
        }

        private static string GetOperator(string methodName)
        {
            switch (methodName)
            {
                case "Add": return "+";
                case "Subtract": return "-";
                case "Multiply": return "*";
                case "Divide": return "/";
                case "Negate": return "-";
                case "Remainder": return "%";
                default: return null;
            }
        }
        protected override ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            List<Expression> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                StringB.Append(" ,");
                Expression p = Visit(original[i]);
                if (list != null)
                {
                    list.Add(p);
                }
                else if (p != original[i])
                {
                    list = new List<Expression>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(p);
                }
            }
            if (list != null)
            {
                return list.AsReadOnly();
            }
            return original;
        }

        protected override NewExpression VisitNew(NewExpression nex)
        {

            IEnumerable<Expression> args = VisitExpressionList(nex.Arguments);
            if (args != nex.Arguments)
            {

                if (nex.Members != null)
                {
                    _listOne.Add(new OneComprosite { Operand = Evolution.SelectNew, NewConctructor = Expression.New(nex.Constructor, args, nex.Members) });
                    return Expression.New(nex.Constructor, args, nex.Members);
                }

                else
                {
                    _listOne.Add(new OneComprosite { Operand = Evolution.SelectNew, NewConctructor = nex });
                    return Expression.New(nex.Constructor, args);
                }

            }
            //todo ion100
            _listOne.Add(new OneComprosite { Operand = Evolution.SelectNew, NewConctructor = nex });
            return nex;
        }

        private Expression BindSelectMany(Expression exp, Expression source, LambdaExpression collectionSelector, LambdaExpression resultSelector)
        {
            throw new Exception("Не реализовано");
        }

        public string Translate(Expression expression, out Evolution ev1, string par)
        {
            ParamStringName = par;
            return Translate(expression, out ev1);
        }
    }

}
