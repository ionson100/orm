using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ORM_1_21_.Linq.MsSql
{
    internal class MsSqlConstructorSql 
    {
        private List<OneComprosite> _listOne;
        // private int i = 0;

        private bool PingComposite(Evolution eval)
        {
            return _listOne.Any(a => a.Operand == eval);
        }

        public string GetStringSql<T>(List<OneComprosite> listOne) //, JoinCapital joinCapital
        {
            _listOne = listOne;

            if (PingComposite(Evolution.Update)) return AttributesOfClass<T>.CreateCommandUpdateFreeForMsSql(_listOne);

            if (PingComposite(Evolution.FreeSql)) return _listOne.Single(a => a.Operand == Evolution.FreeSql).Body;


            if (PingComposite(Evolution.All))
            {
                var sb = new StringBuilder(listOne.First(a => a.Operand == Evolution.All).Body);
                // <> - = #7#
                sb.Replace("<>", "#7#");
                // >= - <=  #1#
                sb.Replace(">=", "#1#");
                // >  -  <  #2#
                sb.Replace(">", "#2#");
                // <  - >   #3#
                sb.Replace("<", "#3#");
                // <= - >=  #4# 
                sb.Replace("<=", "#4#");
                // =  - !=  #5#
                sb.Replace("=", "#5#");
                // != - =   #6#
                sb.Replace("!=", "#6#");
                listOne.First(a => a.Operand == Evolution.All).Body = sb.ToString().Replace("#1#", "<=")
                    .Replace("#2#", "<").Replace("#3#", ">").Replace("#4#", ">=").Replace("#5#", "!=")
                    .Replace("#6#", "=").Replace("#7#", "=");
                listOne.Add(new OneComprosite
                    {Operand = Evolution.Where, Body = listOne.First(a => a.Operand == Evolution.All).Body});
            }

            var sbb = new StringBuilder();
            if (PingComposite(Evolution.Delete))
            {
                sbb.Append("DELETE ");
            }
            else
            {
                sbb.Append(StringConst.Select + " ");

                if (PingComposite(Evolution.Select))
                {
                    sbb.Append(listOne.First(a => a.Operand == Evolution.Select).Body);
                }
                else
                {
                    if (PingComposite(Evolution.Count)) sbb.Append(" COUNT(*) ");
                    if (PingComposite(Evolution.Any))
                    {
                        sbb.Clear();
                        sbb.Append(" IF  EXISTS( " + StringConst.Select);
                    }

                    if (PingComposite(Evolution.DistinctCastom))
                    {
                        sbb.Append(" DISTINCT " + listOne.First(a => a.Operand == Evolution.DistinctCastom).Body);
                    }

                    else if (_listOne.All(a => a.Operand != Evolution.Count))
                    {
                        sbb.Clear();
                        var str1 = AttributesOfClass<T>.SimpleSqlSelect;
                        sbb.Append(str1.Substring(0,
                            str1.IndexOf("FROM", StringComparison.Ordinal)));
                    }

                    var str = sbb.ToString().TrimEnd(',');
                    sbb.Length = 0;
                    sbb.Append(str);
                }
            }

            sbb.Append(AttributesOfClass<T>.SimpleSqlSelect.Substring(
                AttributesOfClass<T>.SimpleSqlSelect.IndexOf("FROM", StringComparison.Ordinal)));

            var ss = listOne.Where(a => a.Operand == Evolution.Where);
            foreach (var i in ss)
            {
                if (string.IsNullOrEmpty(i.Body)) continue;
                sbb.Append(sbb.ToString().IndexOf("WHERE", StringComparison.Ordinal) == -1 ? " WHERE " : " and ");
                sbb.Append(i.Body);
            }

            var ii = 0;

            foreach (var i in listOne.Where(a => a.Operand == Evolution.OrderBy))
            {
                if (ii == 0)
                    sbb.Append(" ORDER BY ");
                sbb.Append(" " + i.Body + ",");
                ii++;
            }

            if (PingComposite(Evolution.Reverse))
            {
                sbb.Append("  ");
                sbb.Append(listOne.Single(a => a.Operand == Evolution.Reverse).Body);
            }


            var ee = sbb.ToString().Trim(' ', ',');
            sbb.Length = 0;
            sbb.Append(ee);

            if (PingComposite(Evolution.First) || PingComposite(Evolution.FirstOrDefault))
            {
                if (PingComposite(Evolution.Single) || PingComposite(Evolution.SingleOrDefault))
                    sbb = sbb.Replace(StringConst.Select,
                        string.Format("{0} {1} (2) ", StringConst.Select, StringConst.Top));
                else
                    sbb = sbb.Replace(StringConst.Select,
                        string.Format("{0} {1} (1) ", StringConst.Select, StringConst.Top));
            }

            if (PingComposite(Evolution.Last) || PingComposite(Evolution.LastOrDefault))
                sbb = sbb.Replace(StringConst.Select,
                    $"{StringConst.Select} {StringConst.Top} (1) ");


            if (PingComposite(Evolution.All))

                sbb = new StringBuilder(StringConst.Select + " COUNT(*) " +
                                        sbb.ToString()
                                            .Substring(sbb.ToString().IndexOf("FROM", StringComparison.Ordinal)));

            if (PingComposite(Evolution.Any))
            {
                sbb.Insert(0, "IF EXISTS (");
                sbb.Append(" ) BEGIN " + StringConst.Select + " 1; END");
            }

            if (PingComposite(Evolution.Limit))
                sbb = new StringBuilder(AttributesOfClass<T>.CreateCommandLimitForMsSql(listOne, sbb.ToString()));
            if (PingComposite(Evolution.Join))
            {
                var whereSb = new StringBuilder();
                foreach (var str in _listOne.Where(a => a.Operand == Evolution.Where).Select(a => a.Body))
                {
                    var str1 = str.Replace("`", "").Replace("[", "").Replace("]", "");
                    var eew = str1;
                    var matsup = new Regex(@"[aA-zZаА-яЯ\d[_]*]*\.[aA-zZаА-яЯ\d[_]*]*").Matches(str1);
                    foreach (var s in matsup)
                        eew = str1.Replace(s.ToString(),
                            Utils.TanslycatorFieldParam1(s.ToString(), Utils.Table1AliasForJoin));
                    whereSb.Append(eew + " AND ");
                }

                if (!string.IsNullOrEmpty(whereSb.ToString()))
                    whereSb.Insert(0, " WHERE ");

                var orderBy = new StringBuilder();
                foreach (var str in _listOne.Where(a => a.Operand == Evolution.OrderBy).Select(a => a.Body))
                {
                    if (str == _listOne.First(a => a.Operand == Evolution.OrderBy).Body)
                    {
                        orderBy.AppendFormat(" ORDER BY {0},",
                            Utils.TanslycatorFieldParam1(str, Utils.Table1AliasForJoin));
                        continue;
                    }

                    orderBy.AppendFormat(" {0},", Utils.TanslycatorFieldParam1(str, Utils.Table1AliasForJoin));
                }


                var dfggf = _listOne.Single(a => a.Operand == Evolution.Join);
                sbb = new StringBuilder(dfggf.Body + whereSb.ToString().Trim("AND ".ToArray()) +
                                        orderBy.ToString().Trim(','));
            }

            return sbb.ToString().Replace("  ", " ").Replace("''", "'").Trim(' ', ',').Replace("Average", "AVG")
                .Replace("LongCount", "Count");
        }
    }
}