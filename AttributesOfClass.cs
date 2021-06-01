using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using ORM_1_21_.Attribute;
using ORM_1_21_.Linq;

namespace ORM_1_21_
{
    /// <summary>
    ///     Служебный класс для генерации  данных рефлексии бывшего табличного класса
    /// </summary>
    /// <typeparam name="T">Тип  класса</typeparam>
    internal static class AttributesOfClass<T>
    {
        private static readonly Lazy<Dictionary<string, string>> ColumnName = new Lazy<Dictionary<string, string>>(() =>
        {
            var list = new List<BaseAttribute>();
            AttributeDall.Value.ToList().ForEach(a => list.AddRange(a.Value));
            PrimaryKeyLazy.Value.ToList().ForEach(a => list.AddRange(a.Value));
            return list.Any()
                ? list.Select(a => new {a.PropertyName, a.ColumnName})
                    .ToDictionary(s => s.PropertyName, d => d.ColumnName)
                : null;
        }, LazyThreadSafetyMode.PublicationOnly);

   
        private static readonly Lazy<Dictionary<Type, List<MapColumnNameAttribute>>> AttributeDall =
            new Lazy<Dictionary<Type, List<MapColumnNameAttribute>>>(ActivateDallAll,
                LazyThreadSafetyMode.PublicationOnly);

        private static readonly Lazy<Dictionary<Type, string>> TableNameAllLazy =
            new Lazy<Dictionary<Type, string>>(ActivateTableNameAll, LazyThreadSafetyMode.PublicationOnly);


        private static readonly Lazy<Dictionary<Type, string>> SqlWhereAllLazy =
            new Lazy<Dictionary<Type, string>>(ActivateSqlWhereAllLazy, LazyThreadSafetyMode.PublicationOnly);


        private static readonly Lazy<Dictionary<Type, List<MapPrimaryKeyAttribute>>> PrimaryKeyLazy =
            new Lazy<Dictionary<Type, List<MapPrimaryKeyAttribute>>>(ActivatePrimaryKeyLazy,
                LazyThreadSafetyMode.PublicationOnly);


        internal static Lazy<List<BaseAttribute>> ListBaseAttr = new Lazy<List<BaseAttribute>>(() =>
        {
            var l = new List<BaseAttribute>();
            AttributeDall.Value.ToList().ForEach(a => l.AddRange(a.Value));
            PrimaryKeyLazy.Value.ToList().ForEach(a => l.AddRange(a.Value));
            return l;
        }, LazyThreadSafetyMode.PublicationOnly);

        internal static Lazy<Dictionary<string, PropertyInfo>> PropertyInfoList =
            new Lazy<Dictionary<string, PropertyInfo>>(() =>
            {
                var d = new Dictionary<string, PropertyInfo>();
                typeof(T).GetProperties().ToList().ForEach(a => d.Add(a.Name, a));
                return d;
            }, LazyThreadSafetyMode.PublicationOnly);


        internal static readonly Lazy<Dictionary<string, Func<T, object>>> GetValue =
            new Lazy<Dictionary<string, Func<T, object>>>(
                () =>
                {
                    var pr = typeof(T).GetProperties();
                    var dictionary = new Dictionary<string, Func<T, object>>();
                    foreach (var propertyInfo in pr)
                    {
                        var instance = Expression.Parameter(typeof(T));
                        var property = Expression.Property(instance, propertyInfo);
                        var convert = Expression.TypeAs(property, typeof(object));
                        dictionary.Add(propertyInfo.Name,
                            Expression.Lambda<Func<T, object>>(convert, instance).Compile());
                    }

                    return dictionary;
                }, LazyThreadSafetyMode.PublicationOnly);


        internal static readonly Lazy<Dictionary<string, Action<T, object>>> SetValue =
            new Lazy<Dictionary<string, Action<T, object>>>(
                () =>
                {
                    var list = new Dictionary<string, Action<T, object>>();

                    var pr = typeof(T).GetProperties()
                        .Where(a => a.GetCustomAttributes(typeof(BaseAttribute), true).Any());


                    foreach (var propertyInfo in pr)
                    {
                        var instance = Expression.Parameter(propertyInfo.DeclaringType, "i");
                        var argument = Expression.Parameter(typeof(object), "a");
                        var setterCall = Expression.Call(
                            instance,
                            propertyInfo.GetSetMethod(),
                            Expression.Convert(argument, propertyInfo.PropertyType));
                        var res = (Action<T, object>) Expression.Lambda(setterCall, instance, argument)
                            .Compile();
                        list.Add(propertyInfo.Name, res);
                    }

                    return list;
                }, LazyThreadSafetyMode.PublicationOnly);


        internal static readonly Lazy<Dictionary<string, Action<T, object>>> SetValueFreeSql =
            new Lazy<Dictionary<string, Action<T, object>>>(
                () =>
                {
                    var list = new Dictionary<string, Action<T, object>>();

                    var pr = typeof(T).GetProperties();


                    foreach (var propertyInfo in pr)
                    {
                        var instance = Expression.Parameter(propertyInfo.DeclaringType, "i");
                        var argument = Expression.Parameter(typeof(object), "a");
                        var setterCall = Expression.Call(
                            instance,
                            propertyInfo.GetSetMethod(),
                            Expression.Convert(argument, propertyInfo.PropertyType));
                        var res = (Action<T, object>) Expression.Lambda(setterCall, instance, argument)
                            .Compile();
                        list.Add(propertyInfo.Name, res);
                    }

                    return list;
                }, LazyThreadSafetyMode.PublicationOnly);

        public static List<MapColumnNameAttribute> CurrentTableAttributeDall => AttributeDall.Value[typeof(T)];

    
        public static string AllSqlWhereFromMap => GetSqlAll();

        public static MapPrimaryKeyAttribute PkAttribute => PrimaryKeyLazy.Value[typeof(T)].First();

        

        public static string TableName => TableNameAllLazy.Value[typeof(T)];


        private static string SqlWhere => SqlWhereAllLazy.Value[typeof(T)];

        private static string SqlWhereBase =>
            SqlWhereAllLazy.Value.ContainsKey(typeof(T).BaseType)
                ? SqlWhereAllLazy.Value[typeof(T).BaseType]
                : string.Empty;



        public static string SimpleSqlSelect => SimpleSelect();


        public static bool IsValid => typeof(T).GetCustomAttributes(typeof(MapTableNameAttribute), true).Any();

       

    

        private static Dictionary<Type, List<MapColumnNameAttribute>> ActivateDallAll()
        {
            var dictionary = new Dictionary<Type, List<MapColumnNameAttribute>> {{typeof(T), ActivateADall()}};
            return dictionary;
        }

        private static Dictionary<Type, string> ActivateTableNameAll()
        {
           
            var dictionary = new Dictionary<Type, string>
            {
                {
                    typeof(T),
                    ((MapTableNameAttribute)
                        typeof(T).GetCustomAttributes(typeof(MapTableNameAttribute), true).First()).TableName
                }
            };
            if (typeof(T).BaseType != null)
                RecursionTableName(dictionary, typeof(T).BaseType);
            return dictionary;
        }

        public static string GetTypeTable<T>()
        {
            var o = typeof(T).GetCustomAttributes(typeof(MapTypeMysqlTableAttribite), true);
            if (o.Any())
            {
                return ((MapTypeMysqlTableAttribite) o.First()).TableType;
            }

            return "";
        }

        private static void RecursionTableName(IDictionary<Type, string> dictionary, Type type)
        {
            while (true)
            {
                var d = type.GetCustomAttributes(typeof(MapTableNameAttribute), true);
                if (!d.Any()) return;
                dictionary.Add(type, ((MapTableNameAttribute) d.First()).TableName);
                if (type.BaseType != null)
                {
                    type = type.BaseType;
                    continue;
                }

                break;
            }
        }

        private static Dictionary<Type, string> ActivateSqlWhereAllLazy()
        {
            var lazy = new Dictionary<Type, string>
            {
                {
                    typeof(T),
                    ((MapTableNameAttribute)
                        typeof(T).GetCustomAttributes(typeof(MapTableNameAttribute), true).First()).SqlWhere
                }
            };
          
            return lazy;
        }


        private static Dictionary<Type, string> AtivareTypeJoinLazy()
        {
            var dictionary = new Dictionary<Type, string>();
           
            return dictionary;
        }

      

        private static Dictionary<Type, string> ActivateForeignKeyAllLazy()
        {
            var dictionary = new Dictionary<Type, string>();
            return dictionary;
        }

       

        private static Dictionary<Type, List<MapPrimaryKeyAttribute>> ActivatePrimaryKeyLazy()
        {
            var lazy = new Dictionary<Type, List<MapPrimaryKeyAttribute>>();
            RecursionPrimaryKey(lazy, typeof(T));
            return lazy;
        }

        private static void RecursionPrimaryKey(IDictionary<Type, List<MapPrimaryKeyAttribute>> dictionary, Type type)
        {
            var l1 = new List<MapPrimaryKeyAttribute>();
            foreach (var f in type.GetProperties())
            {
                var oPk = f.GetCustomAttributes(typeof(MapPrimaryKeyAttribute), true);
                if (!oPk.Any()) continue;
                var pr = (MapPrimaryKeyAttribute) oPk[0];
                pr.IsBaseKey = false;
                pr.IsForeignKey = false;
                pr.PropertyName = f.Name;
                pr.DeclareType = type;
                pr.TypeColumn = f.PropertyType;
                pr.ColumnNameAlias = Utils.GetAsAlias(TableNameAllLazy.Value[type], pr.ColumnName);
                l1.Add(pr);
            }

            if (!l1.Any()) return;
            dictionary.Add(type, l1);
        }

     
        private static List<MapColumnNameAttribute> ActivateADall()
        {
            var res = new List<MapColumnNameAttribute>();
            var fields = typeof(T).GetProperties();
            foreach (var f in fields)
            {
                var o3 = f.GetCustomAttributes(typeof(MapDefaultValueAttribute), true);
                var o1 = f.GetCustomAttributes(typeof(MapColumnNameAttribute), true);
                if (!o1.Any()) continue;

                var o2 = f.GetCustomAttributes(typeof(MapIndexAttribute), true);
                ((MapColumnNameAttribute) o1.First()).IsBaseKey = false;
                ((MapColumnNameAttribute) o1.First()).IsForeignKey = false;
                    if (o2.Any()) ((MapColumnNameAttribute) o1.First()).IsIndex = true;

                ((MapColumnNameAttribute) o1.First()).PropertyName = f.Name;
                ((MapColumnNameAttribute) o1.First()).TypeColumn = f.PropertyType;
                ((MapColumnNameAttribute) o1.First()).DeclareType = typeof(T);
                if (o3.Any())
                {
                    ((MapColumnNameAttribute) o1.First()).DefaultValue = ((MapDefaultValueAttribute) o3.First()).Value;
                }

                ((MapColumnNameAttribute) o1.First()).ColumnNameAlias = Utils.GetAsAlias(
                    TableNameAllLazy.Value[typeof(T)],
                    ((MapColumnNameAttribute) o1.First())
                    .ColumnName);

                var o4 = f.GetCustomAttributes(typeof(MapColumnTypeAttribute), true);
                if (o4.Any())
                {
                    ((MapColumnNameAttribute) o1.First()).TypeString = ((MapColumnTypeAttribute) o4.First()).TypeString;
                }


                res.Add((MapColumnNameAttribute) o1.First());
            }

            return res;
        }


        private static string GetSqlAll()
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(SqlWhere)) sb.Append(SqlWhere);
            if (!string.IsNullOrEmpty(SqlWhere) && !string.IsNullOrEmpty(SqlWhereBase))
                sb.AppendFormat(" AND {0}", SqlWhereBase);
            if (string.IsNullOrEmpty(SqlWhere) && !string.IsNullOrEmpty(SqlWhereBase))
                sb.AppendFormat("{0}", SqlWhereBase);
            return sb.ToString();
        }


        public static string AddSqlWhere(string sqlWhere)
        {
            sqlWhere = sqlWhere.Trim();
            if (!string.IsNullOrEmpty(sqlWhere) && string.IsNullOrEmpty(AllSqlWhereFromMap))
                return " WHERE " + sqlWhere;
            if (string.IsNullOrEmpty(sqlWhere) && !string.IsNullOrEmpty(AllSqlWhereFromMap))
                return " WHERE " + AllSqlWhereFromMap;
            if (!string.IsNullOrEmpty(sqlWhere) && !string.IsNullOrEmpty(AllSqlWhereFromMap))
                return " WHERE " + AllSqlWhereFromMap + " AND " + sqlWhere;
            return string.Empty;
        }


        public static IEnumerable<T> GetEnumerableObjects(IDataReader reader)
        {
            if (reader == null) return null;
            var res = Pizdaticus.GetRiderToList<T>(reader);

            return res;
        }



        public static IEnumerable<T> GetEnumerableObjectsGroupBy<TT>(IDataReader reader, Delegate expDelegate)
        {
            if (reader == null) return null;
            var rr = typeof(T).GetGenericArguments();
            var r = Pizdaticus.GetRiderToList<TT>(reader);
            var res = CallExp<T, TT>.GetTrechForGroupBy(r, expDelegate, rr[0]);
            return res;
        }


        public static string SimpleSelect()
        {
            var sb = new StringBuilder();
            sb.Append(StringConst.Select + " ");
            foreach (var type in AttributeDall.Value.Keys.Reverse())
            {
                if (Configure.Provider == ProviderName.Postgresql)
                {
                    foreach (var a in AttributeDall.Value[type])
                        sb.AppendFormat($" {a.ColumnName}," );
                           
                    foreach (var pk in PrimaryKeyLazy.Value[type])
                        sb.AppendFormat($" {pk.ColumnName},");
                }
                else
                {
                    foreach (var a in AttributeDall.Value[type])
                        sb.AppendFormat(" {0}.{1} AS {2},",
                            TableNameAllLazy.Value[type],
                            a.ColumnName,
                            Utils.GetAsAlias(TableNameAllLazy.Value[type], a.ColumnName));
                    foreach (var pk in PrimaryKeyLazy.Value[type])
                        sb.AppendFormat(" {0}.{1} AS {2},",
                            TableNameAllLazy.Value[type],
                            pk.ColumnName,
                            Utils.GetAsAlias(TableNameAllLazy.Value[type], pk.ColumnName));
                }
               
            }


            sb = new StringBuilder(sb.ToString().Trim(','));
            sb.AppendFormat(" FROM {0}", TableName);

            return sb.ToString();
        }


       



        public static void CreateUpdateCommandMysql(IDbCommand command, T item, ProviderName provider)
        {
            var listType = TableNameAllLazy.Value.Keys.Reverse();
            var allSql = new StringBuilder();
            switch (provider)
            {
                case ProviderName.MsSql:
                    //allSql.Append("DECLARE @T1 VARCHAR(20);BEGIN TRAN T1;");
                    break;
                case ProviderName.MySql:
                    allSql.Append(""); //START TRANSACTION;
                    break;
                case ProviderName.Postgresql:
                    allSql.Append("");
                    break;
                case ProviderName.Sqlite:
                    allSql.Append("");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(provider), provider, null);
            }

            var i = 0;
            var sb = new StringBuilder();

            var enumerable = listType as Type[] ?? listType.ToArray();

            foreach (var type in enumerable)
            {
                sb.Clear();

                var par = new StringBuilder();
                foreach (var pra in AttributeDall.Value[type]
                    .Where(pra => !pra.IsBaseKey && !pra.IsForeignKey ))
                {
                    par.AppendFormat(" {0}.{1} = {3}p{2},", TableNameAllLazy.Value[type], pra.ColumnName, ++i,
                        Utils.Prefparam);

                    IDataParameter pr = ProviderFactories.GetParameter();
                    pr.ParameterName = string.Format("{1}p{0}", i, Utils.Prefparam);
                    var prcore = item.GetType().GetProperties().First(a => a.Name == pra.PropertyName);
                    object vall;
                    if (prcore.PropertyType == typeof(Image))
                        vall = Utils.ImageToByte((Image) GetValue.Value[prcore.Name](item));
                    else if (Utils.IsJsonType(prcore.PropertyType))
                        vall = Utils.ObjectToJson(GetValue.Value[prcore.Name](item));
                    else if (prcore.PropertyType.BaseType == typeof(Enum))
                        vall = (int) GetValue.Value[prcore.Name](item);
                    else
                        vall = GetValue.Value[prcore.Name](item);

                    pr.Value = vall ?? DBNull.Value;
                    pr.DbType = pra.DbType();
                    command.Parameters.Add(pr);
                }


                sb.AppendFormat("UPDATE {0} SET {1} WHERE {0}.{2} = {4}p{3}; ", TableNameAllLazy.Value[type],
                    par.ToString().Trim(','),
                    PrimaryKeyLazy.Value[type].First().ColumnName, ++i, Utils.Prefparam);

                IDataParameter pr1 = ProviderFactories.GetParameter();
                pr1.ParameterName = string.Format("{1}p{0}", i, Utils.Prefparam);
                var cname = PrimaryKeyLazy.Value[type].First();
                var sxs = item.GetType().GetProperties().First(a => a.Name == cname.PropertyName);


                var val1 = GetValue.Value[sxs.Name](item);
                //  var val1 = sxs.GetValue(item, null);
                pr1.Value = val1 ?? DBNull.Value;
                pr1.DbType = PrimaryKeyLazy.Value[type].First().DbType();
                command.Parameters.Add(pr1);
                allSql.Append(sb);
            }


            switch (provider)
            {
                case ProviderName.MsSql:
                   // allSql.Append("COMMIT TRAN T1;");
                    break;
                case ProviderName.MySql:
                    allSql.Append(""); //COMMIT;
                    break;
                case ProviderName.Postgresql:
                    allSql.Append("");
                    break;
                case ProviderName.Sqlite:
                    allSql.Append("");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(provider), provider, null);
            }

            command.CommandText = allSql.ToString();
        }


        public static void CreateUpdateCommandPostgres(IDbCommand command, T item)
        {
            var listType = TableNameAllLazy.Value.Keys.Reverse();
            var allSql = new StringBuilder();
           
            var sb = new StringBuilder();
            var i = 0;
            var enumerable = listType as Type[] ?? listType.ToArray();

            foreach (var type in enumerable)
            {
                sb.Clear();

                var par = new StringBuilder();
                foreach (var pra in AttributeDall.Value[type]
                    .Where(pra => !pra.IsBaseKey && !pra.IsForeignKey ))
                {
                    par.AppendFormat(" {0} = {2}p{1},", pra.ColumnName, ++i, Utils.Prefparam);

                    IDataParameter pr = ProviderFactories.GetParameter();
                    pr.ParameterName = string.Format("{1}p{0}", i, Utils.Prefparam);
                    var prcore =
                        item.GetType()
                            .GetProperties()
                            .First(a => a.Name == pra.PropertyName);
                    object vall;
                    if (prcore.PropertyType == typeof(Image))
                        vall = Utils.ImageToByte((Image)GetValue.Value[prcore.Name](item));
                   
                    else if (Utils.IsJsonType(prcore.PropertyType))
                        vall = Utils.ObjectToJson(GetValue.Value[prcore.Name](item));
                    else if (prcore.PropertyType.BaseType == typeof(Enum))
                        vall = (int)GetValue.Value[prcore.Name](item);
                    else
                        vall = GetValue.Value[prcore.Name](item);

                    pr.Value = vall ?? DBNull.Value;
                    if (prcore.PropertyType.BaseType == typeof(Enum))
                    {
                        pr.DbType = DbTypeConverter.ConvertFrom(typeof(int));
                    }
                    else
                    {
                         pr.DbType = pra.DbType();
                    }

                    if (prcore.PropertyType == typeof(Guid) && Configure.Provider == ProviderName.Sqlite)
                    {
                        pr.DbType= DbTypeConverter.ConvertFrom(typeof(string));
                        pr.Value = GetValue.Value[prcore.Name](item).ToString();
                    }



                    command.Parameters.Add(pr);
                }


                sb.AppendFormat("UPDATE {0} SET {1} WHERE {2} = {4}p{3}; ", TableNameAllLazy.Value[type],
                    par.ToString().Trim(','),
                    PrimaryKeyLazy.Value[type].First().ColumnName, ++i, Utils.Prefparam);
                
                IDataParameter pr1 = ProviderFactories.GetParameter();
                pr1.ParameterName = string.Format("{1}p{0}", i, Utils.Prefparam);
                var cname = PrimaryKeyLazy.Value[type].First();
                var sxs = item.GetType().GetProperties().First(a => a.Name == cname.PropertyName);
                var val1 = GetValue.Value[sxs.Name](item);
                pr1.Value = val1 ?? DBNull.Value;
                pr1.DbType = PrimaryKeyLazy.Value[type].First().DbType();
                command.Parameters.Add(pr1);
                allSql.Append(sb);
            }
            command.CommandText = allSql.ToString();
        }

        public static string CreateCommandLimitForMySql(List<OneComprosite> listOne)
        {
            // "UPDATE {1} INNER JOIN {0} ON ({0}.id_body = {1}.id ) SET {2}  {3};",

            var si = SimpleSqlSelect;


            var sb = new StringBuilder();
            foreach (var oneComprosite in listOne.Where(a => a.Operand == Evolution.Update))
            {
                var ee = oneComprosite.Body.Trim().Trim(',').Split(',');
                var eee = ee.ToList().Split(2);
                foreach (var s in eee)
                {
                    var enumerable = s as string[] ?? s.ToArray();
                    if (enumerable.Any())
                        sb.AppendFormat(" {0} = {1},", enumerable.First(), enumerable.Last());
                }
            }

            var where = new StringBuilder();

            foreach (var v in listOne.Where(a => a.Operand == Evolution.Where)) where.AppendFormat(" {0} AND", v.Body);
            if (!string.IsNullOrEmpty(where.ToString()))
                where = new StringBuilder(" WHERE " + where.ToString().Trim("AND".ToArray()));
            var from = si.Substring(si.IndexOf("FROM", StringComparison.Ordinal) + 4);

            if (Configure.Provider == ProviderName.Postgresql|| Configure.Provider == ProviderName.Sqlite)
            {
                string str= string.Format("UPDATE {0} SET {1}  {2};", from, sb.ToString().Trim(','), where.ToString().Trim(','));
                return str.Replace($"{TableName}.", "");
            }
           
            return string.Format("UPDATE {0} SET {1}  {2};", from, sb.ToString().Trim(','), where.ToString().Trim(','));
        }

        public static string CreateCommandUpdateFreeForMsSql(List<OneComprosite> listOne)
        {
            var si = SimpleSqlSelect;


            var r = new StringBuilder();
            foreach (var oneComprosite in listOne.Where(a => a.Operand == Evolution.Update))
            {
                var ee = oneComprosite.Body.Trim().Trim(',').Split(',');
                //  var eee = ee.Split(2).ToList();
                foreach (var s in ee.Split(2))
                {
                    var enumerable = s as string[] ?? s.ToArray();
                    if (enumerable.ToList().Any())
                        r.AppendFormat(" {0} = {1},", enumerable.First(), enumerable.Last());
                }
            }

            var where = new StringBuilder();

            foreach (var v in listOne.Where(a => a.Operand == Evolution.Where)) where.AppendFormat(" {0} AND", v.Body);
            if (!string.IsNullOrEmpty(where.ToString()))
                where = new StringBuilder(" WHERE " + where.ToString().Trim("AND".ToArray()));


            var from = si.Substring(si.IndexOf("FROM", StringComparison.Ordinal));


            return string.Format(" UPDATE {0} SET {1} {2} {3}", TableNameAllLazy.Value[typeof(T)],
                r.ToString().Trim(','), from, where.ToString().Trim(','));
        }

        public static string CreateCommandLimitForMsSql(List<OneComprosite> listOne, string doSql)
        {
            const string table = "tt1";

            var dd = listOne.Single(a => a.Operand == Evolution.Limit).Body.Replace("LIMIT", "").Trim(' ').Split(',');

            var start = int.Parse(dd[0]);
            var count = int.Parse(dd[1]);


            var where = listOne.Where(a => a.Operand == Evolution.Where);
            var sbwhere = new StringBuilder();
            var sbOrderBy = new StringBuilder();
            var oneComprosites = where as OneComprosite[] ?? where.ToArray();
            foreach (var oneComprosite in oneComprosites)
            {
                if (oneComprosite == oneComprosites.First())
                {
                    sbwhere.AppendFormat(" {0} ", oneComprosite.Body);
                    continue;
                }

                sbwhere.AppendFormat("AND  {0} ", oneComprosite.Body);
            }

            var ordrby = listOne.Where(a => a.Operand == Evolution.OrderBy);
            foreach (var oneComprosite in ordrby) sbOrderBy.AppendFormat("{0},", oneComprosite.Body);
            var ss = SimpleSqlSelect.Replace(StringConst.Select, "") + AddSqlWhere(sbwhere.ToString());
            var mat4 = new Regex(@"AS[^,]*").Matches(doSql.Substring(0, doSql.IndexOf("FROM", StringComparison.Ordinal))
                                                         .Replace(StringConst.Select, "") + ",");
            var d = new StringBuilder();
            if (mat4.Count > 0)
            {
                foreach (var v in mat4) d.Append(v.ToString().Replace("AS", "") + " ,");
            }
            else
            {
                var sss =
                    doSql.Substring(0, doSql.IndexOf("FROM", StringComparison.Ordinal)).Replace(StringConst.Select, "")
                        .Split(',');
                foreach (var s in sss)
                    d.Append(s.Replace("]", "").Replace("[", "").Replace(".", Utils.Bungalo).ToLower() + " ,");
            }


            if (string.IsNullOrEmpty(sbOrderBy.ToString()))
                sbOrderBy.AppendFormat(" {0}.{1} ", TableNameAllLazy.Value[typeof(T)],
                    PrimaryKeyLazy.Value[typeof(T)].First().ColumnName);


            var ff = string.Format("SELECT {0} FROM (SELECT ROW_NUMBER() " +
                                   "OVER(ORDER BY {3} ) AS rownum, {1} ) " +
                                   "AS {2} WHERE rownum BETWEEN {4} AND {5}",
                d.ToString().Trim(','),
                ss,
                table,
                sbOrderBy.ToString().Trim(','),
                start,
                count);
            return ff;
        }

        public static void CreateUpdateCommand(IDbCommand command, T item)
        {
            var listType = TableNameAllLazy.Value.Keys.Reverse();
            var allSb = new StringBuilder();

            const string par = "p";
            var i = 0;

            var enumerable = listType as Type[] ?? listType.ToArray();
            foreach (var type in enumerable)
            {
                var sb = new StringBuilder();
                var sbvalues = new StringBuilder("");
                sb.AppendFormat("UPDATE {0} SET ", TableNameAllLazy.Value[type]);
                foreach (var pk in PrimaryKeyLazy.Value[type])
                {
                    if (pk.Generator == Generator.Native) continue;
                    if (pk.Generator == Generator.Assigned) continue;
                    sbvalues.AppendFormat(" {0}.{1}={2}{3}{4},", TableNameAllLazy.Value[type], pk.ColumnName,
                        Utils.Prefparam, par, ++i);
                    IDataParameter pr = ProviderFactories.GetParameter();
                    pr.ParameterName = string.Format("{0}{1}{2}", Utils.Prefparam, par, i);
                    var val = GetValue.Value[pr.ParameterName](item);
                    pr.Value = val ?? DBNull.Value;
                    pr.DbType = pk.DbType();
                    command.Parameters.Add(pr);
                }

                foreach (var rtp in AttributeDall.Value[type])
                {
                    sbvalues.AppendFormat(" {0}.{1}={2}{3}{4},", TableNameAllLazy.Value[type], rtp.ColumnName,
                        Utils.Prefparam, par, ++i);
                    IDataParameter pr = ProviderFactories.GetParameter();
                    pr.ParameterName = string.Format("{0}{1}{2}", Utils.Prefparam, par, i);
                    var val = GetValue.Value[rtp.PropertyName](item);
                    pr.Value = val ?? DBNull.Value;
                    pr.DbType = rtp.DbType();
                    command.Parameters.Add(pr);
                }

                sb.Append(sbvalues.ToString().Trim(',') + " ");
                sb.Append(SimpleSelect().Substring(SimpleSelect().IndexOf("FROM", StringComparison.Ordinal)));
                sb.Append(";");
                allSb.Append(sb);
            }

            command.CommandText = allSb.ToString();
        }

        public static void RedefiningPrimaryKey(T item, object val)
        {
            var e = PrimaryKeyLazy.Value[typeof(T)].First();
            if (e.Generator != Generator.Native) return;
            var valCore = Utils.ConvertatorPrimaryKeyType(e.TypeColumn, Convert.ToDecimal(val));
            item.GetType().GetProperty(e.PropertyName).SetValue(item, valCore, null);
        }

        public static void CreateInsetCommand(IDbCommand command, T obj)
        {
            var listType = TableNameAllLazy.Value.Keys.Reverse();
            var allSb = new StringBuilder();
            var declare = new StringBuilder();
            const string par = "p";
            var i = 0;

            var enumerable = listType as Type[] ?? listType.ToArray();
            foreach (var type in enumerable)
            {
                var sb = new StringBuilder();
                var sbvalues = new StringBuilder(" VALUES (");
                sb.AppendFormat("INSERT INTO {0} (", TableNameAllLazy.Value[type]);
                foreach (var pk in PrimaryKeyLazy.Value[type])
                {
                    
                    if (pk.Generator != Generator.Assigned) continue;

                    if (Configure.Provider == ProviderName.Postgresql|| Configure.Provider == ProviderName.Sqlite)
                    {
                        sb.AppendFormat($"{pk.ColumnName}, ");
                    }
                    else
                    {
                        sb.AppendFormat("{0}.{1}, ", TableNameAllLazy.Value[type], pk.ColumnName);
                    }
                    if (pk.IsForeignKey)
                    {
                        sbvalues.Append(" @basekey, ");
                    }
                    else
                    {
                        sbvalues.AppendFormat("{0}{1}{2},", Utils.Prefparam, par, ++i);
                        IDataParameter pr = ProviderFactories.GetParameter();
                        pr.ParameterName = string.Format("{0}{1}{2}", Utils.Prefparam, par, i);
                        // var val = obj.GetType().GetProperty(pk.PropertyName).GetValue(obj, null);
                        var val = GetValue.Value[pk.PropertyName](obj);
                       
                        pr.Value = val ?? DBNull.Value;
                        pr.DbType = pk.DbType();
                        command.Parameters.Add(pr);
                    }
                }

             
                foreach (var rtp in AttributeDall.Value[type])
                {
                   
                    if (Configure.Provider == ProviderName.Postgresql|| Configure.Provider == ProviderName.Sqlite)
                    {
                        sb.AppendFormat($"{ rtp.ColumnName}, ");
                    }
                    else
                    {
                        sb.AppendFormat("{0}.{1},", TableNameAllLazy.Value[type], rtp.ColumnName);
                    }
                   
                    if (rtp.IsForeignKey)
                    {
                        sbvalues.Append(" @basekey, ");
                    }
                    else
                    {
                        bool isEnum = false;
                        sbvalues.AppendFormat("{0}{1}{2},", Utils.Prefparam, par, ++i);
                        IDataParameter pr = ProviderFactories.GetParameter();
                        pr.ParameterName = $"{Utils.Prefparam}{par}{i}";
                        var prcore = obj.GetType().GetProperty(rtp.PropertyName);
                        object vall;
                        if (prcore.PropertyType == typeof(Image))
                            vall = Utils.ImageToByte((Image) GetValue.Value[prcore.Name](obj));
                        if (prcore.PropertyType.BaseType == typeof(Enum))
                        {
                            vall = (int)GetValue.Value[prcore.Name](obj);
                            isEnum = true;
                        }
                        else if (Utils.IsJsonType(prcore.PropertyType))
                            vall = Utils.ObjectToJson(GetValue.Value[prcore.Name](obj));
                        else
                            vall = GetValue.Value[prcore.Name](obj);

                        if (prcore.PropertyType == typeof(Guid)&&Configure.Provider==ProviderName.Sqlite)
                        {
                            pr.Value = vall.ToString();
                            pr.DbType = DbTypeConverter.ConvertFrom(typeof(string));
                        }
                        else
                        {
                           pr.Value = vall ?? DBNull.Value;
                           pr.DbType = isEnum ? DbTypeConverter.ConvertFrom(typeof(int)) : rtp.DbType(); 
                        }
                        
                       
                        command.Parameters.Add(pr);
                    }
                }

                sb = new StringBuilder(sb.ToString().Trim(new []{' ',','}) + ")");
                sb.Append(sbvalues.ToString().Trim(new []{' ',','}) + ");");
                allSb.Append(sb);
               
            }

            

            if (Configure.Provider == ProviderName.MsSql)
                allSb.Insert(0, declare);
            {
                if (Configure.Provider == ProviderName.Postgresql|| Configure.Provider == ProviderName.Sqlite)
                {
                    var s = allSb.ToString().Trim(new[] {' ', ';'});
                    allSb.Length = 0;
                    allSb.Append(s).Append(" ").Append(Utils.Pref.Replace("{1}", TableNameAllLazy.Value[typeof(T)])
                        .Replace("{2}", PkAttribute.ColumnName));
                }
                else
                {
                    allSb.Append(Utils.Pref.Replace("{1}", TableNameAllLazy.Value[typeof(T)]).Replace("{2}", PkAttribute.ColumnName));
                }
            }
            command.CommandText = allSb.ToString();
        }

        public static void CreateDeleteCommand(IDbCommand command, T obj)
        {
            command.CommandText = string.Format(" DELETE FROM {0} WHERE {0}.{1} = {2}p1",
                TableNameAllLazy.Value[typeof(T)],
                PrimaryKeyLazy.Value[typeof(T)].First().ColumnName, Utils.Prefparam);
            IDataParameter pr = ProviderFactories.GetParameter();
            pr.ParameterName = string.Format("{0}p1", Utils.Prefparam);
            var prr = obj.GetType().GetProperty(PrimaryKeyLazy.Value[typeof(T)].First().PropertyName);
            var val = GetValue.Value[prr.Name](obj);

            // .GetValue(obj, null);
            pr.Value = val ?? DBNull.Value;
            pr.DbType = PrimaryKeyLazy.Value[typeof(T)].First().DbType();
            command.Parameters.Add(pr);
        }

        public static string GetNameFieldForQuery(string member, Type type)
        {
            return TableNameAllLazy.Value[type] + "." + ColumnName.Value[member];
        }
    }
}