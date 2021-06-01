using System;
using System.Linq.Expressions;
using System.Threading;

namespace ORM_1_21_.Linq
{
    delegate T ObjectActivator<out T>(params object[] args);
    internal static class Creator<T>
    {
        static readonly object o = new object();
        public static ObjectActivator<T> Activator;


        public static ObjectActivator<T> GetActivator(NewExpression m)
        {
            if (typeof (T) != typeof (object))
            {
                if (Activator == null)
                {
                    lock (o)
                    {
                            Activator = InnerGetActivator(m);
                    }

                }
                return Activator;
            }
            return  InnerGetActivator(m);
        }

        static ObjectActivator<T> InnerGetActivator(NewExpression m)
        {
            var paramsInfo = m.Constructor.GetParameters();
            var param = Expression.Parameter(typeof(object[]), "args");
            var argsExp = new Expression[paramsInfo.Length];

            for (var i = 0; i < paramsInfo.Length; i++)
            {
                Expression index = Expression.Constant(i);
                Type paramType = paramsInfo[i].ParameterType;
                Expression paramAccessorExp = Expression.ArrayIndex(param, index);
                Expression paramCastExp = Expression.Convert(paramAccessorExp, paramType);
                argsExp[i] = paramCastExp;
            }

            var newExp = Expression.New(m.Constructor, argsExp);
            var lambda = Expression.Lambda(typeof(ObjectActivator<T>), newExp, param);
            var compiled = (ObjectActivator<T>)lambda.Compile();
            return compiled;
        }
    }
}
