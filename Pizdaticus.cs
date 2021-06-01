using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using ORM_1_21_.Linq;

namespace ORM_1_21_
{
    internal static class Pizdaticus
    {
        public static IEnumerable<TObj> GetRiderToList<TObj>(IDataReader reader)
        {
            bool? fied = null;
            var res = new List<TObj>();
            while (reader.Read())
            {
                var d = (TObj) FormatterServices.GetSafeUninitializedObject(typeof(TObj));
                foreach (var s in AttributesOfClass<TObj>.ListBaseAttr.Value)
                {
                    try
                    {
                        if (fied == null)
                            fied = Utils.ColumnExists(reader, s.ColumnNameAlias);

                        object e;
                        if (fied == true)
                            e = reader[s.ColumnNameAlias];
                        else
                            e = reader[Utils.ClearTrim(s.ColumnName)];

                        var pr = AttributesOfClass<TObj>.PropertyInfoList.Value[s.PropertyName];
                        

                        if (Utils.IsJsonType(pr.PropertyType))
                        {
                           
                                var o = Utils.JsonToObject(e.ToString(), pr.PropertyType);
                                AttributesOfClass<TObj>.SetValue.Value[pr.Name](d,
                                    e == DBNull.Value
                                        ? null
                                        : o);
                           
                           
                        }
                        else if (pr.PropertyType == typeof(Image))
                        {
                            AttributesOfClass<TObj>.SetValue.Value[pr.Name](d,
                                e == DBNull.Value ? null : Utils.ImageFromByte((byte[])e));
                        }
                        else if (pr.PropertyType == typeof(bool))
                        {
                            if (e == DBNull.Value)
                            {
                                AttributesOfClass<TObj>.SetValue.Value[pr.Name](d, null);
                            }
                            else
                            {
                                var b = Convert.ToBoolean(e);
                                AttributesOfClass<TObj>.SetValue.Value[pr.Name](d, b);
                            }
                        }
                        else if (pr.PropertyType == typeof(DateTime))
                        {
                           
                            if (Configure.Provider == ProviderName.Sqlite)
                            {
                                DateTime dateTime = DateTime.Parse(e.ToString());
                                AttributesOfClass<TObj>.SetValue.Value[pr.Name](d, dateTime);

                            }
                            else
                            {
                                AttributesOfClass<TObj>.SetValue.Value[pr.Name](d, e);
                            }
                            
                        }
                        else if (pr.PropertyType == typeof(DateTime?))
                        {
                            var tt = e.GetType();
                            AttributesOfClass<TObj>.SetValue.Value[pr.Name](d, e == DBNull.Value ? null : e);
                        }
                        else if (pr.PropertyType.BaseType == typeof(Enum))
                        {
                            var eres = Enum.Parse(pr.PropertyType, e.ToString());
                            AttributesOfClass<TObj>.SetValue.Value[pr.Name](d, e == DBNull.Value ? null : eres);
                        }
                        else if (pr.PropertyType == typeof(Guid))
                        {
                            var eres = new Guid(e.ToString());
                            AttributesOfClass<TObj>.SetValue.Value[pr.Name](d, e == DBNull.Value ? Guid.Empty : eres);
                        }
                        else if (pr.PropertyType == typeof(float))
                        {
                            if (e == DBNull.Value)
                                AttributesOfClass<TObj>.SetValue.Value[pr.Name](d, 0f);
                            else
                                AttributesOfClass<TObj>.SetValue.Value[pr.Name](d, Convert.ToSingle(e));
                        }
                        else if (pr.PropertyType == typeof(float?))
                        {
                            if (e == DBNull.Value)
                                AttributesOfClass<TObj>.SetValue.Value[pr.Name](d, null);
                            else
                                AttributesOfClass<TObj>.SetValue.Value[pr.Name](d, (float?)Convert.ToSingle(e));
                        }
                        else if (pr.PropertyType == typeof(double))
                        {
                            if (e == DBNull.Value)
                                AttributesOfClass<TObj>.SetValue.Value[pr.Name](d, 0);
                            else
                                AttributesOfClass<TObj>.SetValue.Value[pr.Name](d, Convert.ToDouble(e));
                        }
                        else if (pr.PropertyType == typeof(double?))
                        {
                            if (e == DBNull.Value)
                                AttributesOfClass<TObj>.SetValue.Value[pr.Name](d, null);
                            else
                                AttributesOfClass<TObj>.SetValue.Value[pr.Name](d, (double?)Convert.ToDouble(e));
                        }
                        else if (pr.PropertyType == typeof(decimal))
                        {
                            if (e == DBNull.Value)
                                AttributesOfClass<TObj>.SetValue.Value[pr.Name](d, 0);
                            else
                                AttributesOfClass<TObj>.SetValue.Value[pr.Name](d, Convert.ToDecimal(e));
                        }
                        else if (pr.PropertyType == typeof(decimal?))
                        {
                            if (e == DBNull.Value)
                                AttributesOfClass<TObj>.SetValue.Value[pr.Name](d, null);
                            else
                                AttributesOfClass<TObj>.SetValue.Value[pr.Name](d, (decimal?)Convert.ToDecimal(e));
                        }

                        else if (pr.PropertyType == typeof(int))
                        {
                            if (e == DBNull.Value)
                                AttributesOfClass<TObj>.SetValue.Value[pr.Name](d, 0);
                            else
                                AttributesOfClass<TObj>.SetValue.Value[pr.Name](d, Convert.ToInt32(e));
                        }
                        else if (pr.PropertyType == typeof(int?))
                        {
                            if (e == DBNull.Value)
                                AttributesOfClass<TObj>.SetValue.Value[pr.Name](d, null);
                            else
                                AttributesOfClass<TObj>.SetValue.Value[pr.Name](d, (int?)Convert.ToInt32(e));
                        }
                        else
                        {
                            var tt = e.GetType();
                            var tte = pr.PropertyType;
                            try
                            {
                               
                                AttributesOfClass<TObj>.SetValue.Value[pr.Name](d, e == DBNull.Value ? null : e);
                            }
                            catch (Exception exception)
                            {
                                Console.WriteLine(exception);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Configure.SendError("create item "+typeof(TObj),e);
                    }
                   
                }

                Utils.SetPersisten(d);
                res.Add(d);
            }

            reader.Dispose();
            return res;
        }

        private static object CastTo(object value, Type targetType)
        {
            var t = value.GetType();
            if (targetType == typeof(int)) return (int) value;

            if (targetType == typeof(float) && t == typeof(double)) return Convert.ToSingle(value);

            return value;
        }

        public static List<T> GetListAnonymusObj<T>(IDataReader reader, object ss)
        {
            var ctor = ((NewExpression) ss).Constructor;
            var lres = new List<T>();
            var d = new object[reader.FieldCount]; //////////////////////////////
            while (reader.Read())
            {
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    var o = reader[i];
                    var t = ((NewExpression) ss).Arguments[i].Type;
                    var res = Utils.Convertor(t, o);
                    d[i] = res;
                }

                var ee = ctor
                    .Invoke(d); //Activator.CreateInstance(typeof(T),d)); //Creator<T>.GetActivator((NewExpression)ss).Invoke(d);

                lres.Add((T) ee);
            }

            reader.Dispose();
            //todo ion100
            return lres;
        }

        public static Delegate GetDelegateForGroupBy(LambdaExpression lambda)
        {
            return lambda.Compile();
        }

        public static T SingleData<T>(IEnumerable<OneComprosite> listOne, IEnumerable<T> lResul, out bool isActive)
        {
            if (listOne == null) throw new ArgumentException("listOne == null ");
            if (listOne.Any(a => a.Operand == Evolution.SingleOrDefault && a.IsAgregate))
            {
                if (!lResul.Any())
                {
                    isActive = true;
                    return default(T);
                }

                if (lResul.Count() == 1)
                {
                    isActive = true;
                    return lResul.First();
                }

                throw new Exception("Последовательност содержит больше чем одни элемент -" + lResul.Count());
            }

            if (listOne.Any(a => a.Operand == Evolution.Single && a.IsAgregate))
            {
                if (lResul.Count() == 1)
                {
                    isActive = true;
                    return lResul.First();
                }

                throw new Exception("Последовательност содержит больше чем одни элемент или пустая count -" +
                                    lResul.Count());
            }

            if (listOne.Any(a => a.Operand == Evolution.First))
            {
                isActive = true;
                return lResul.First();
            }

            if (listOne.Any(a => a.Operand == Evolution.FirstOrDefault))
            {
                isActive = true;
                return !lResul.Any() ? default(T) : lResul.First();
            }

            if (listOne.Any(a => a.Operand == Evolution.LastOrDefault))
            {
                isActive = true;
                return !lResul.Any() ? default(T) : lResul.First();
            }

            if (listOne.Any(a => a.Operand == Evolution.Last))
            {
                isActive = true;
                return lResul.Last();
            }

            if (listOne.Any(a => a.Operand == Evolution.ElementAt))
            {
                isActive = true;
                return lResul.First();
            }

            if (listOne.Any(a => a.Operand == Evolution.ElementAtOrDefault))
            {
                isActive = true;
                return lResul.Any() ? lResul.First() : default(T);
            }

            isActive = false;
            return default(T);
        }

        public static object Carding(Type type)
        {
            if (type == typeof(uint)) return (uint) 1;
            if (type == typeof(ulong)) return (ulong) 1;
            if (type == typeof(ushort)) return (ushort) 1;
            if (type == typeof(bool)) return true;
            if (type == typeof(byte)) return (byte) 1;

            if (type == typeof(char)) return (char) 1;
            if (type == typeof(DateTime)) return DateTime.Now;
            if (type == typeof(decimal)) return (decimal) 1;
            if (type == typeof(double)) return (double) 1;
            if (type == typeof(short)) return (short) 1;
            if (type == typeof(int)) return 1;
            if (type == typeof(long)) return (long) 1;
            if (type == typeof(sbyte)) return (sbyte) 1;
            if (type == typeof(float)) return (float) 1;
            if (type == typeof(string)) return string.Empty;
            ////
            if (type == typeof(uint?)) return (uint?) 1;
            if (type == typeof(ulong?)) return (ulong?) 1;
            if (type == typeof(ushort?)) return (ushort?) 2;
            if (type == typeof(bool?)) return (bool?) true;
            if (type == typeof(byte?)) return (byte?) 1;
            if (type == typeof(char?)) return (char?) 1;
            if (type == typeof(DateTime?)) return null;
            if (type == typeof(decimal?)) return (decimal?) 1;
            if (type == typeof(double?)) return (double?) 1;
            if (type == typeof(short?)) return (short?) 1;
            if (type == typeof(int?)) return (int?) 1;

            if (type == typeof(long?)) return (long?) 1;
            if (type == typeof(sbyte?)) return (sbyte?) 1;
            if (type == typeof(float?)) return (float?) 1;
            return null;
        }

        public static IEnumerable<TObj> GetRiderToList2<TObj>(IDataReader reader)
        {
            bool? fied = null;
            var res = new List<TObj>();
            using (var read = reader)
            {
                while (read.Read())
                {
                    var d = (TObj) FormatterServices.GetSafeUninitializedObject(typeof(TObj));
                    foreach (var s in AttributesOfClass<TObj>.ListBaseAttr.Value)
                    {
                        if (fied == null)
                            fied = Utils.ColumnExists(reader, s.ColumnNameAlias);

                        object e;
                        if (fied == true)
                            e = reader[s.ColumnNameAlias];
                        else
                            e =
                                reader[
                                    s.ColumnName.Replace("`", string.Empty)
                                        .Replace("[", string.Empty)
                                        .Replace("]", string.Empty)];

                        var pr = AttributesOfClass<TObj>.PropertyInfoList.Value[s.PropertyName];
                        {
                            if (Utils.IsJsonType(pr.PropertyType))
                            {
                                AttributesOfClass<TObj>.SetValue.Value[pr.Name](d,
                                    e == DBNull.Value
                                        ? null
                                        : Utils.JsonToObject(e.ToString(), pr.PropertyType));
                            }
                            else if (pr.PropertyType == typeof(Image))
                            {
                                AttributesOfClass<TObj>.SetValue.Value[pr.Name](d,
                                    e == DBNull.Value ? null : Utils.ImageFromByte((byte[]) e));
                            }
                            else if (pr.PropertyType == typeof(bool))
                            {
                                if (e == DBNull.Value)
                                {
                                    AttributesOfClass<TObj>.SetValue.Value[pr.Name](d, null);
                                }
                                else
                                {
                                    var b = Convert.ToBoolean(e);
                                    AttributesOfClass<TObj>.SetValue.Value[pr.Name](d, b);
                                }
                            }
                            else if (pr.PropertyType == typeof(DateTime))
                            {
                                var tt = e.GetType();
                                AttributesOfClass<TObj>.SetValue.Value[pr.Name](d, e == DBNull.Value ? null : e);
                            }
                            else if (pr.PropertyType.BaseType == typeof(Enum))
                            {
                                var eres = Enum.Parse(pr.PropertyType, e.ToString());
                                AttributesOfClass<TObj>.SetValue.Value[pr.Name](d, e == DBNull.Value ? null : eres);
                            }
                            else if (pr.PropertyType == typeof(Guid))
                            {
                                var eres = new Guid(e.ToString());
                                AttributesOfClass<TObj>.SetValue.Value[pr.Name](d,
                                    e == DBNull.Value ? Guid.Empty : eres);
                            }
                            else
                            {
                                var tt = e.GetType();
                                var tte = pr.PropertyType;

                                AttributesOfClass<TObj>.SetValue.Value[pr.Name](d, e == DBNull.Value ? null : e);
                            }
                        }
                    }

                    Utils.SetPersisten(d);
                    yield return d;
                }
            }
        }
    }

    internal static class CallExp<TRes, TElement>
    {
        public static IEnumerable<TRes> GetTrechForGroupBy(IEnumerable<TElement> list, Delegate @delegate, Type type)
        {
            if (type == typeof(Guid)) return (IEnumerable<TRes>)list.GroupBy((Func<TElement, Guid>)@delegate);
            if (type == typeof(uint)) return (IEnumerable<TRes>) list.GroupBy((Func<TElement, uint>) @delegate);
            if (type == typeof(ulong)) return (IEnumerable<TRes>) list.GroupBy((Func<TElement, ulong>) @delegate);
            if (type == typeof(ushort)) return (IEnumerable<TRes>) list.GroupBy((Func<TElement, ushort>) @delegate);
            if (type == typeof(bool)) return (IEnumerable<TRes>) list.GroupBy((Func<TElement, bool>) @delegate);
            if (type == typeof(byte)) return (IEnumerable<TRes>) list.GroupBy((Func<TElement, byte>) @delegate);
            if (type == typeof(char)) return (IEnumerable<TRes>) list.GroupBy((Func<TElement, char>) @delegate);
            if (type == typeof(DateTime)) return (IEnumerable<TRes>) list.GroupBy((Func<TElement, DateTime>) @delegate);
            if (type == typeof(decimal)) return (IEnumerable<TRes>) list.GroupBy((Func<TElement, decimal>) @delegate);
            if (type == typeof(double)) return (IEnumerable<TRes>) list.GroupBy((Func<TElement, double>) @delegate);
            if (type == typeof(short)) return (IEnumerable<TRes>) list.GroupBy((Func<TElement, short>) @delegate);
            if (type == typeof(int)) return (IEnumerable<TRes>) list.GroupBy((Func<TElement, int>) @delegate);
            if (type == typeof(long)) return (IEnumerable<TRes>) list.GroupBy((Func<TElement, long>) @delegate);
            if (type == typeof(sbyte)) return (IEnumerable<TRes>) list.GroupBy((Func<TElement, sbyte>) @delegate);
            if (type == typeof(float)) return (IEnumerable<TRes>) list.GroupBy((Func<TElement, float>) @delegate);
            if (type == typeof(string)) return (IEnumerable<TRes>) list.GroupBy((Func<TElement, string>) @delegate);
            if (type == typeof(uint?)) return (IEnumerable<TRes>) list.GroupBy((Func<TElement, uint?>) @delegate);
            if (type == typeof(ulong?)) return (IEnumerable<TRes>) list.GroupBy((Func<TElement, ulong?>) @delegate);
            if (type == typeof(ushort?)) return (IEnumerable<TRes>) list.GroupBy((Func<TElement, ushort?>) @delegate);
            if (type == typeof(bool?)) return (IEnumerable<TRes>) list.GroupBy((Func<TElement, bool?>) @delegate);
            if (type == typeof(byte?)) return (IEnumerable<TRes>) list.GroupBy((Func<TElement, byte?>) @delegate);
            if (type == typeof(char?)) return (IEnumerable<TRes>) list.GroupBy((Func<TElement, char?>) @delegate);
            if (type == typeof(DateTime?))
                return (IEnumerable<TRes>) list.GroupBy((Func<TElement, DateTime?>) @delegate);
            if (type == typeof(decimal?)) return (IEnumerable<TRes>) list.GroupBy((Func<TElement, decimal?>) @delegate);
            if (type == typeof(double?)) return (IEnumerable<TRes>) list.GroupBy((Func<TElement, double?>) @delegate);
            if (type == typeof(short?)) return (IEnumerable<TRes>) list.GroupBy((Func<TElement, short?>) @delegate);
            if (type == typeof(int?)) return (IEnumerable<TRes>) list.GroupBy((Func<TElement, int?>) @delegate);
            if (type == typeof(long?)) return (IEnumerable<TRes>) list.GroupBy((Func<TElement, long?>) @delegate);
            if (type == typeof(sbyte?)) return (IEnumerable<TRes>) list.GroupBy((Func<TElement, sbyte?>) @delegate);
            if (type == typeof(float?)) return (IEnumerable<TRes>) list.GroupBy((Func<TElement, float?>) @delegate);
            if (type == typeof(byte[])) return (IEnumerable<TRes>) list.GroupBy((Func<TElement, byte?>) @delegate);
            if (type == typeof(object)) return (IEnumerable<TRes>) list.GroupBy((Func<TElement, object>) @delegate);
            throw new Exception(string.Format(CultureInfo.CurrentCulture, "не могу конвертировать тип {0} as {1}",
                type.FullName));
        }
    }
}