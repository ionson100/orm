using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ORM_1_21_.Linq
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class QueryProvider : IQueryProvider
    {
        IQueryable<TS> IQueryProvider.CreateQuery<TS>(Expression expression) 
        {
            return new Query<TS>(this, expression);
        }
        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
                var elementType = TypeSystem.GetElementType(expression.Type);
                try
                {
                    return (
                        IQueryable) Activator.CreateInstance(typeof (Query<>).MakeGenericType(
                            elementType), new object[] {this, expression});
                }
                catch (TargetInvocationException tie)
                {
                    throw tie.InnerException;
                }
        }

        TS IQueryProvider.Execute<TS>(Expression expression) 
        {
            return (TS) Execute<TS>(expression);
        }

        object IQueryProvider.Execute(Expression expression)
        {
            return Execute(expression);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public abstract string GetQueryText(Expression expression);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <typeparam name="TS"></typeparam>
        /// <returns></returns>
        public abstract object Execute<TS>(Expression expression) ;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <typeparam name="TS"></typeparam>
        /// <returns></returns>
        public  abstract object ExecuteSPP<TS>(Expression expression);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public abstract object Execute(Expression expression);

        internal abstract string GetQueryTextForJoin(Expression expression,List<OneComprosite> comprosite,Dictionary<string,object> dictionary ,string parStr );

    }

   

   



   









   
}






