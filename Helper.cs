using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using ORM_1_21_.Linq;

namespace ORM_1_21_
{
    /// <summary>
    ///     Расширения для построения запроса
    /// </summary>
    public static class Helper
    {
        /// <summary>
        ///     Сохранение или добавление объекта в базу
        /// </summary>
        /// <param name="coll"></param>
        /// <param name="obj"></param>
        /// <typeparam name="T">сохраняемый или добавляемый обект в базу</typeparam>
        public static void SaveOrUpdate<T>(this IQueryable<T> coll, T obj) where T : class
        {
            var ses = ((ISqlComposite) coll.Provider).Sessione;
            ses.Save(obj);
        }

        public static bool LikeSql(this string s,string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            return s.StartsWith(value, StringComparison.CurrentCulture);
        }


        /// <summary>
        /// </summary>
        /// <param name="coll"></param>
        /// <param name="expression"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TRes"></typeparam>
        /// <returns></returns>
        public static TRes FreeExpression<T, TRes>(this IQueryable<T> coll, Expression expression)
        {
            return (TRes) coll.Provider.Execute<object>(coll.Expression);
        }

        /// <summary>
        /// </summary>
        /// <param name="coll"></param>
        /// <param name="exp"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<object> DistinctCore<T>(this IQueryable<T> coll, Expression<Func<T, object>> exp)
        {
            ((ISqlComposite) coll.Provider).ListCastExpression.Add(new ContainerCastExpression
                {CastomExpression = exp, TypeRevalytion = Evolution.DistinctCastom});
            return (IEnumerable<object>) coll.Provider.Execute<object>(coll.Expression);
        }

      
     


        /// <summary>
        /// Группировка по полю с  условием ключа.
        /// </summary>
        /// <param name="coll"></param>
        /// <param name="exp"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Obsolete]
        public static IEnumerable<T> GroupByCore<T>(this IQueryable<T> coll, Expression<Func<T, bool>> exp)
        {
            ((ISqlComposite) coll.Provider).ListCastExpression.Add(new ContainerCastExpression
                {CastomExpression = exp, TypeRevalytion = Evolution.GroupBy});
            return coll;
        }

        /// <summary>
        ///Группировка по полю с  анoнимным выбором
        /// </summary>
        /// <param name="coll"></param>
        /// <param name="key"></param>
        /// <param name="exp"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <returns></returns>
        [Obsolete]
        public static IEnumerable<object> GroupByCore<T, TKey>(this IQueryable<T> coll,
            Expression<Func<T, TKey>> key, Expression<Func<T, object>> exp) where TKey : class
        {
            ((ISqlComposite) coll.Provider).ListCastExpression.Add(new ContainerCastExpression
                {CastomExpression = exp, TypeRevalytion = Evolution.SelectNew});
            ((ISqlComposite) coll.Provider).ListCastExpression.Add(new ContainerCastExpression
                {CastomExpression = key, TypeRevalytion = Evolution.GroupBy});
            return (IEnumerable<object>) coll.Provider.Execute<object>(coll.Expression);
        }

        /// <summary>
        /// Удаление объекта полученого ранее из базы
        /// </summary>
        /// <param name="coll"></param>
        /// <param name="obj">Удаляемы объект, он должен быть получен из базы</param>
        /// <typeparam name="T">Тип проекции таблицы</typeparam>
        public static void Delete<T>(this IQueryable<T> coll, T obj) where T : class
        {
            var ses = ((ISqlComposite) coll.Provider).Sessione;
            ses.Delete(obj);
        }


        /// <summary>
        /// Удаление обьекта по параметрам
        /// </summary>
        /// <param name="coll"></param>
        /// <param name="exp">передикат на удаление</param>
        /// <typeparam name="T">Тип проекции таблицы</typeparam>
        /// <returns></returns>
        public static int Delete<T>(this IQueryable<T> coll, Expression<Func<T, bool>> exp) where T : class
        {
            ((ISqlComposite) coll.Provider).ListCastExpression.Add(new ContainerCastExpression
                {CastomExpression = exp, TypeRevalytion = Evolution.Delete});
            return coll.Provider.Execute<int>(coll.Expression);
        }


        /// <summary>
        ///     LIMIT всегда ставится в конце предложения LIMIT ( начало позиции с учетом нуля, количество в выборке)
        /// </summary>
        /// <param name="coll"></param>
        /// <param name="start">Начало позиции</param>
        /// <param name="length">Количество записей</param>
        /// <typeparam name="T">Тип проекции таблицы</typeparam>
        /// <returns></returns>
        public static IEnumerable<T> Limit<T>(this IQueryable<T> coll, int start, int length) where T : class
        {
            ((ISqlComposite) coll.Provider).ListCastExpression.Add(new ContainerCastExpression
                {TypeRevalytion = Evolution.Limit, ParamList = new List<object> {start, length}});
            return coll;
        }

        /// <summary>
        ///     LIMIT всегда ставится в конце предложения LIMIT ( начало позиции с учетом нуля, количество в выборке)
        /// </summary>
        /// <param name="coll"></param>
        /// <param name="start">Начало позиции</param>
        /// <param name="length">Количество записей</param>
        /// <returns></returns>
        public static IEnumerable<decimal> Limit(this IQueryable<decimal> coll, int start, int length)
        {
            ((ISqlComposite) coll.Provider).ListCastExpression.Add(new ContainerCastExpression
                {TypeRevalytion = Evolution.Limit, ParamList = new List<object> {start, length}});
            return coll;
        }

        /// <summary>
        ///     LIMIT всегда ставится в конце предложения LIMIT ( начало позиции с учетом нуля, количество в выборке)
        /// </summary>
        /// <param name="coll"></param>
        /// <param name="start">Начало позиции</param>
        /// <param name="length">Количество записей</param>
        /// <returns></returns>
        public static IEnumerable<float> Limit(this IQueryable<float> coll, int start, int length)
        {
            ((ISqlComposite) coll.Provider).ListCastExpression.Add(new ContainerCastExpression
                {TypeRevalytion = Evolution.Limit, ParamList = new List<object> {start, length}});
            return coll;
        }

        /// <summary>
        ///     LIMIT всегда ставится в конце предложения LIMIT ( начало позиции, количество в выборке)
        /// </summary>
        /// <param name="coll"></param>
        /// <param name="start">Начало позиции</param>
        /// <param name="length">Количество записей</param>
        /// <returns></returns>
        public static IEnumerable<int> Limit(this IQueryable<int> coll, int start, int length)
        {
            ((ISqlComposite) coll.Provider).ListCastExpression.Add(new ContainerCastExpression
                {TypeRevalytion = Evolution.Limit, ParamList = new List<object> {start, length}});
            return coll;
        }

        /// <summary>
        ///     LIMIT всегда ставится в конце предложения LIMIT ( начало позиции, количество в выборке)
        /// </summary>
        /// <param name="coll"></param>
        /// <param name="start">Начало позиции</param>
        /// <param name="length">Количество записей</param>
        /// <returns></returns>
        public static IEnumerable<short> Limit(this IQueryable<short> coll, int start, int length)
        {
            ((ISqlComposite) coll.Provider).ListCastExpression.Add(new ContainerCastExpression
                {TypeRevalytion = Evolution.Limit, ParamList = new List<object> {start, length}});
            return coll;
        }

        /// <summary>
        ///     LIMIT всегда ставится в конце предложения LIMIT ( начало позиции, количество в выборке)
        /// </summary>
        /// <param name="coll"></param>
        /// <param name="start">Начало позиции</param>
        /// <param name="length">Количество записей</param>
        /// <returns></returns>
        public static IEnumerable<long> Limit(this IQueryable<long> coll, int start, int length)
        {
            ((ISqlComposite) coll.Provider).ListCastExpression.Add(new ContainerCastExpression
                {TypeRevalytion = Evolution.Limit, ParamList = new List<object> {start, length}});
            return coll;
        }

        /// <summary>
        ///     LIMIT всегда ставится в конце предложения LIMIT ( начало позиции, количество в выборке)
        /// </summary>
        /// <param name="coll"></param>
        /// <param name="start">Начало позиции</param>
        /// <param name="length">Количество записей</param>
        /// <returns></returns>
        public static IEnumerable<object> Limit(this IQueryable<object> coll, int start, int length)
        {
            ((ISqlComposite) coll.Provider).ListCastExpression.Add(new ContainerCastExpression
                {TypeRevalytion = Evolution.Limit, ParamList = new List<object> {start, length}});
            return coll;
        }

        /// <summary>
        ///LIMIT всегда ставится в конце предложения LIMIT ( от какой позиции выбираем, сколько выбираем)
        /// </summary>
        /// <param name="coll"></param>
        /// <param name="start">Начало позиции</param>
        /// <param name="length">Количество записей</param>
        /// <returns></returns>
        public static IEnumerable<double> Limit(this IQueryable<double> coll, int start, int length)
        {
            ((ISqlComposite) coll.Provider).ListCastExpression.Add(new ContainerCastExpression
                {TypeRevalytion = Evolution.Limit, ParamList = new List<object> {start, length}});
            return coll;
        }


        /// <summary>
        ///Вытаскивание обьекта по ключу
        /// </summary>
        /// <param name="coll"></param>
        /// <param name="keyValue">Значение первичного ключа</param>
        /// <typeparam name="T">Тип проекции таблицы</typeparam>
        /// <returns></returns>
        public static T Get<T>(this IQueryable<T> coll, object keyValue) where T : class
        {
            var ses = ((ISqlComposite) coll.Provider).Sessione;
            return ses.Get<T>(keyValue);
        }

        /// <summary>
        ///     Преобразование массива байт в картинку
        /// </summary>
        /// <param name="coll"></param>
        /// <returns></returns>
        public static Image GetImage(this byte[] coll)
        {
            return Utils.ImageFromByte(coll);
        }

        /// <summary>
        ///     Разбиение на массивы
        /// </summary>
        /// <param name="source"></param>
        /// <param name="chunkLength">количество массивов</param>
        /// <typeparam name="T">Тип проекции таблицы</typeparam>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> source, int chunkLength)
        {
            var enumerable = source as IList<T> ?? source.ToList();
            using (var enumerator = enumerable.GetEnumerator())
            {
                while (enumerator.MoveNext()) yield return InnerSplit(enumerator, chunkLength);
            }
        }

        /// <summary>
        ///разбиение на перечисления
        /// </summary>
        /// <param name="coll">исходное перечисление</param>
        /// <param name="splitSize">количество перечислений на выходе</param>
        /// <typeparam name="T">Тип проекции таблицы</typeparam>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> SplitQueryable<T>(this IQueryable<T> coll, int splitSize)
        {
            var enumerable = coll;
            using (var enumerator = enumerable.GetEnumerator())
            {
                while (enumerator.MoveNext()) yield return InnerSplit(enumerator, splitSize);
            }
        }

        private static IEnumerable<T> InnerSplit<T>(IEnumerator<T> enumerator, int splitSize)
        {
            var count = 0;
            do
            {
                count++;
                yield return enumerator.Current;
            } while (count % splitSize != 0
                     && enumerator.MoveNext());
        }


        /// <summary>
        /// Обновление таблицы без вытаскивания данных на клиента
        /// </summary>
        /// <param name="coll"></param>
        /// <param name="parametr">Словарь поле - значение</param>
        /// <typeparam name="T">Тип проекции таблицы</typeparam>
        /// <typeparam name="TKey">Свойство - поле</typeparam>
        /// <typeparam name="TValue">Значение</typeparam>
        /// <returns></returns>
        public static int Update<T, TKey, TValue>(this IQueryable<T> coll,
            Expression<Func<T, Dictionary<TKey, TValue>>> parametr) where T : class
        {
            ((ISqlComposite) coll.Provider).ListCastExpression.Add(new ContainerCastExpression
                {CastomExpression = parametr, TypeRevalytion = Evolution.Update});
            return coll.Provider.Execute<int>(coll.Expression);
        }


      


      

        /// <summary>
        ///     Выполенение произвольного запроса с параметрами
        /// </summary>
        /// <param name="ses">ISession</param>
        /// <param name="sql">Запрос</param>
        /// <param name="par">Параметры запроса</param>
        /// <typeparam name="TResult">Тип единицы Результата</typeparam>
        /// <returns>IEnumerableTResult</returns>
        public static IEnumerable<TResult> FreeSql<TResult>(this ISession ses, string sql, params Parameter[] par)
        {
            var p = new V(sql);
            Expression callExpr = Expression.Call(
                Expression.Constant(p), p.GetType().GetMethod("FreeSql"));
            if (par != null)
            {
                return (IEnumerable<TResult>) new DbQueryProvider<TResult>((Sessione) ses).ExecuteParam<TResult>(callExpr, par);
            }
            return (IEnumerable<TResult>)new DbQueryProvider<TResult>((Sessione)ses).Execute<TResult>(callExpr);
        }

        /// <summary>
        /// произвольный запрос к чужой базе
        /// </summary>
        /// <param name="ses"></param>
        /// <param name="dataReader"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public static IEnumerable<TResult> FreeSqlMonster<TResult>(this ISession ses, IDataReader dataReader)
        {
          
            return (IEnumerable<TResult>)new DbQueryProvider<TResult>((Sessione)ses).ExecuteMonster<TResult>(dataReader);
        }

        /// <summary>
        ///     Вызов хранимой процедуры
        /// </summary>
        /// <param name="ses">ISession</param>
        /// <param name="sql">Текст запроса</param>
        /// <typeparam name="TResult">Тип перечисления</typeparam>
        /// <returns>IEnumerable(TResult)</returns>
        public static IEnumerable<TResult> ProcedureCall<TResult>(this ISession ses, string sql)
        {
            var p = new V(sql);
            Expression callExpr = Expression.Call(
                Expression.Constant(p), p.GetType().GetMethod("FreeSql"));
            return new DbQueryProvider<TResult>((Sessione) ses).ExecuteCall<TResult>(callExpr);
        }

        /// <summary>
        ///     Вызов хранимой процедуры с параметрами
        /// </summary>
        /// <param name="ses">ISession</param>
        /// <param name="sql">Текст процедуры</param>
        /// <param name="par">Праметры</param>
        /// <typeparam name="TResult">Тип еденицы перчисления</typeparam>
        /// <returns>IEnumerable(TResult)</returns>
        public static IEnumerable<TResult> ProcedureCallParam<TResult>(this ISession ses, string sql,
            params ParameterStoredPr[] par)
        {
            var p = new V(sql);
            Expression callExpr = Expression.Call(
                Expression.Constant(p), p.GetType().GetMethod("FreeSql"));
            return new DbQueryProvider<TResult>((Sessione) ses).ExecuteCallParam<TResult>(callExpr, par);
        }
    }
}