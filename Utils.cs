using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;

using Newtonsoft.Json;
using NLog.LayoutRenderers.Wrappers;


namespace ORM_1_21_
{
    public class GroupExpression<T>
    {
        public static Delegate Delegate(LambdaExpression lexp)
        {
            return Action(lexp,lexp.ReturnType);
        }
        private static Delegate Action(LambdaExpression lexp,Type type)
        {

            if (type == typeof(Guid)) { return (Func<T, Guid>)(lexp).Compile(); }
            if (type == typeof(DateTime)) { return (Func<T, DateTime>)(lexp).Compile(); }
            if (type == typeof(uint)) { return (Func<T, uint>)(lexp).Compile(); }
            if (type == typeof(ulong)) { return (Func<T, ulong>)(lexp).Compile(); }
            if (type == typeof(ushort)) { return (Func<T, ushort>)(lexp).Compile(); }
            if (type == typeof(bool)) { return (Func<T, bool>)(lexp).Compile(); }
            if (type == typeof(byte)) { return (Func<T, byte>)(lexp).Compile(); }
            if (type == typeof(char)) { return (Func<T, char>)(lexp).Compile(); }
            if (type == typeof(decimal)) { return (Func<T, decimal>)(lexp).Compile(); }
            if (type == typeof(double)) { return (Func<T, double>)(lexp).Compile(); }
            if (type == typeof(short)) { return (Func<T, short>)(lexp).Compile(); }
            if (type == typeof(int)) { return (Func<T, int>)(lexp).Compile(); }
            if (type == typeof(long)) { return (Func<T, long>)(lexp).Compile(); }
            if (type == typeof(sbyte)) { return (Func<T, sbyte>)(lexp).Compile(); }
            if (type == typeof(float)) { return (Func<T, float>)(lexp).Compile(); }
            if (type == typeof(string)) { return (Func<T, string>)(lexp).Compile(); }
            if (type == typeof(object)) { return (Func<T, object>)(lexp).Compile(); }


            if (type == typeof(Guid?)) { return (Func<T, Guid?>)(lexp).Compile(); }
            if (type == typeof(DateTime?)) { return (Func<T, DateTime?>)(lexp).Compile(); }
            if (type == typeof(uint?)) { return (Func<T, uint?>)(lexp).Compile(); }
            if (type == typeof(ulong?)) { return (Func<T, ulong?>)(lexp).Compile(); }
            if (type == typeof(ushort?)) { return (Func<T, ushort?>)(lexp).Compile(); }
            if (type == typeof(bool?)) { return (Func<T, bool?>)(lexp).Compile(); }
            if (type == typeof(byte?)) { return (Func<T, byte?>)(lexp).Compile(); }
            if (type == typeof(char?)) { return (Func<T, char?>)(lexp).Compile(); }
            if (type == typeof(decimal?)) { return (Func<T, decimal?>)(lexp).Compile(); }
            if (type == typeof(double?)) { return (Func<T, double?>)(lexp).Compile(); }
            if (type == typeof(short?)) { return (Func<T, short?>)(lexp).Compile(); }
            if (type == typeof(int?)) { return (Func<T, int?>)(lexp).Compile(); }
            if (type == typeof(long?)) { return (Func<T, long?>)(lexp).Compile(); }
            if (type == typeof(sbyte?)) { return (Func<T, sbyte?>)(lexp).Compile(); }
            if (type == typeof(float?)) { return (Func<T, float?>)(lexp).Compile(); }



            return (Func<T, object>)(lexp).Compile();


        }
    }
    /// <summary>
    ///     Утилиты
    /// </summary>
    internal static class Utils
    {
       

      
        internal const string Bungalo = "____";
        internal const string Base = "BASE", Error = "ERROR";

        internal static string Pref
        {
            get
            {
                switch (Configure.Provider)
                {
                    case ProviderName.MsSql:
                        return "SELECT IDENT_CURRENT ('{1}');";
                    case ProviderName.MySql:
                        return "SELECT LAST_INSERT_ID();";
                    case ProviderName.Postgresql:
                        return "RETURNING {2};";
                    case ProviderName.Sqlite:
                        return ";select last_insert_rowid();";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        internal static string Prefparam
        {
            get
            {
                switch (Configure.Provider)
                {
                    case ProviderName.MsSql:
                        return "@" ;
                    case ProviderName.MySql:
                        return "?";
                    case ProviderName.Postgresql:
                        return "@";
                    case ProviderName.Sqlite:
                        return "@";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }


        private static ProviderName _providerName;

        internal static Assembly Assembler;

        /// <summary>
        /// </summary>
        internal static string Table1AliasForJoin = "tt1";


        /// <summary>
        ///     Определяем, является ли тип анонимным
        /// </summary>
        /// <param name="type">Тип для исследования</param>
        /// <returns>bool</returns>
        public static bool IsAnonymousType(Type type)
        {
            return System.Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                   && type.IsGenericType && type.Name.Contains("AnonymousType")
                   && (type.Name.StartsWith("<>", StringComparison.OrdinalIgnoreCase) ||
                       type.Name.StartsWith("VB$", StringComparison.OrdinalIgnoreCase))
                   && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }

        /// <summary>
        ///     Сериализация  объекта
        /// </summary>
        /// <param name="obj">Объект</param>
        /// <returns>byte[]</returns>
        public static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        /// <summary>
        ///     Десерелизация
        /// </summary>
        /// <param name="arrBytes">byte[]</param>
        /// <returns>Объект</returns>
        public static object ByteArrayToObject(byte[] arrBytes)
        {
            if (arrBytes == null) return null;
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                return obj;
            }
        }

        internal static string GetAsAlias(string table, string field)
        {
            return string.Concat(table.Trim(), Bungalo, field.Trim()).Replace("`", "").Replace("[", "").Replace("]", "")
                .Trim().ToLower();
        }



        /// <summary>
        ///     Получение массива байт из Image
        /// </summary>
        /// <param name="img">Image</param>
        /// <returns></returns>
        public static byte[] ImageToByte(Image img)
        {
            if (img == null) return null;
            var converter = new ImageConverter();
            return (byte[]) converter.ConvertTo(img, typeof(byte[]));
        }

        /// <summary>
        ///     Получение Image из массива байт
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static Image ImageFromByte(byte[] img)
        {
            if (img == null) return null;
            var converter = new ImageConverter();
            return (Image) converter.ConvertFrom(img);
        }


        internal static bool IsPersistent(object obj)
        {
            return TypeDescriptor.GetAttributes(obj).Contains(new PersistentAttribute());
        }


        internal static void SetPersisten(object obj)
        {
            TypeDescriptor.AddAttributes(obj, new PersistentAttribute());
        }

        internal static object Convertor<TR>(object ob)
        {
            if (ob is DBNull)
            {
                if (typeof(TR).IsGenericType) return null;

                if (!typeof(TR).IsValueType) return (TR) (object) null;
                if (typeof(TR) == typeof(DateTime))
                {
                    object o = DateTime.MinValue;
                    return (TR) o;
                }

                return Activator.CreateInstance<TR>();
            }

            if (typeof(TR) == typeof(uint)) return (TR) (object) Convert.ToUInt32(ob, CultureInfo.InvariantCulture);
            if (typeof(TR) == typeof(ulong)) return (TR) (object) Convert.ToUInt64(ob, CultureInfo.InvariantCulture);
            if (typeof(TR) == typeof(ushort)) return (TR) (object) Convert.ToUInt16(ob, CultureInfo.InvariantCulture);
            if (typeof(TR) == typeof(bool)) return (TR) (object) Convert.ToBoolean(ob, CultureInfo.InvariantCulture);
            if (typeof(TR) == typeof(byte)) return (TR) (object) Convert.ToByte(ob, CultureInfo.InvariantCulture);

            if (typeof(TR) == typeof(char)) return (TR) (object) Convert.ToChar(ob, CultureInfo.InvariantCulture);
            if (typeof(TR) == typeof(DateTime))
                return (TR) (object) Convert.ToDateTime(ob, CultureInfo.InvariantCulture);
            if (typeof(TR) == typeof(decimal)) return (TR) (object) Convert.ToDecimal(ob, CultureInfo.InvariantCulture);
            if (typeof(TR) == typeof(double)) return (TR) (object) Convert.ToDouble(ob, CultureInfo.InvariantCulture);
            if (typeof(TR) == typeof(short)) return (TR) (object) Convert.ToInt16(ob, CultureInfo.InvariantCulture);
            if (typeof(TR) == typeof(int)) return (TR) (object) Convert.ToInt32(ob, CultureInfo.InvariantCulture);
            if (typeof(TR) == typeof(long)) return (TR) (object) Convert.ToInt64(ob, CultureInfo.InvariantCulture);
            if (typeof(TR) == typeof(sbyte)) return (TR) (object) Convert.ToSByte(ob, CultureInfo.InvariantCulture);
            if (typeof(TR) == typeof(float)) return (TR) (object) Convert.ToSingle(ob, CultureInfo.InvariantCulture);
            if (typeof(TR) == typeof(string)) return (TR) (object) Convert.ToString(ob, CultureInfo.InvariantCulture);
            ////
            if (typeof(TR) == typeof(uint?)) return (TR) (object) Convert.ToUInt32(ob, CultureInfo.InvariantCulture);
            if (typeof(TR) == typeof(ulong?)) return (TR) (object) Convert.ToUInt64(ob, CultureInfo.InvariantCulture);

            if (typeof(TR) == typeof(ushort?)) return (TR) (object) Convert.ToUInt16(ob, CultureInfo.InvariantCulture);
            if (typeof(TR) == typeof(bool?)) return (TR) (object) Convert.ToBoolean(ob, CultureInfo.InvariantCulture);
            if (typeof(TR) == typeof(byte?)) return (TR) (object) Convert.ToByte(ob, CultureInfo.InvariantCulture);
            if (typeof(TR) == typeof(char?)) return (TR) (object) Convert.ToChar(ob, CultureInfo.InvariantCulture);
            if (typeof(TR) == typeof(DateTime?))
                return (TR) (object) Convert.ToDateTime(ob, CultureInfo.InvariantCulture);
            if (typeof(TR) == typeof(decimal?))
                return (TR) (object) Convert.ToDecimal(ob, CultureInfo.InvariantCulture);
            if (typeof(TR) == typeof(double?)) return (TR) (object) Convert.ToDouble(ob, CultureInfo.InvariantCulture);
            if (typeof(TR) == typeof(short?)) return (TR) (object) Convert.ToInt16(ob, CultureInfo.InvariantCulture);
            if (typeof(TR) == typeof(int?)) return (TR) (object) Convert.ToInt32(ob, CultureInfo.InvariantCulture);
            if (typeof(TR) == typeof(long?)) return (TR) (object) Convert.ToInt64(ob, CultureInfo.InvariantCulture);
            if (typeof(TR) == typeof(sbyte?)) return (TR) (object) Convert.ToSByte(ob, CultureInfo.InvariantCulture);
            if (typeof(TR) == typeof(float?)) return (TR) (object) Convert.ToSingle(ob, CultureInfo.InvariantCulture);
            if (typeof(TR) == typeof(byte[])) return (TR) ob;
            if (typeof(TR) == typeof(object)) return (TR) ob;
            //var t = typeof(TR);
            //if (typeof(TR) == typeof(Guid))
            //{
            //    return new Guid(ob.ToString());
            //}

            String message = string.Format(CultureInfo.CurrentCulture, "не могу конвертировать тип {0} as {1}",
                ob.GetType().FullName, typeof(TR));
            throw new Exception(message);
        }

        internal static object Convertor(Type t, object ob)
        {
            if (ob is DBNull)
            {
                if (t.IsGenericType) return null;

                if (!t.IsValueType) return null;
                if (t == typeof(DateTime))
                {
                    object o = DateTime.MinValue;
                    return o;
                }

                if (t == typeof(float))
                    return 0.0f;
                return 0.00;
            }

            if (t == typeof(uint)) return Convert.ToUInt32(ob, CultureInfo.InvariantCulture);
            if (t == typeof(ulong))
                return
                    Convert.ToUInt64(ob, CultureInfo.InvariantCulture);
            if (t == typeof(ushort)) return Convert.ToUInt16(ob, CultureInfo.InvariantCulture);
            if (t == typeof(bool)) return Convert.ToBoolean(ob, CultureInfo.InvariantCulture);
            if (t == typeof(byte)) return Convert.ToByte(ob, CultureInfo.InvariantCulture);
            if (t == typeof(char)) return Convert.ToChar(ob, CultureInfo.InvariantCulture);
            if (t == typeof(DateTime)) return Convert.ToDateTime(ob, CultureInfo.InvariantCulture);
            if (t == typeof(decimal)) return Convert.ToDecimal(ob, CultureInfo.InvariantCulture);
            if (t == typeof(double)) return Convert.ToDouble(ob, CultureInfo.InvariantCulture);
            if (t == typeof(short)) return Convert.ToInt16(ob, CultureInfo.InvariantCulture);
            if (t == typeof(int)) return Convert.ToInt32(ob, CultureInfo.InvariantCulture);
            if (t == typeof(long)) return Convert.ToInt64(ob, CultureInfo.InvariantCulture);
            if (t == typeof(sbyte)) return Convert.ToSByte(ob, CultureInfo.InvariantCulture);
            if (t == typeof(float)) return Convert.ToSingle(ob, CultureInfo.InvariantCulture);
            if (t == typeof(string)) return Convert.ToString(ob, CultureInfo.InvariantCulture);
            ////
            if (t == typeof(uint?)) return Convert.ToUInt32(ob, CultureInfo.InvariantCulture);
            if (t == typeof(ulong?)) return Convert.ToUInt64(ob, CultureInfo.InvariantCulture);

            if (t == typeof(ushort?)) return Convert.ToUInt16(ob, CultureInfo.InvariantCulture);
            if (t == typeof(bool?)) return Convert.ToBoolean(ob, CultureInfo.InvariantCulture);
            if (t == typeof(byte?)) return Convert.ToByte(ob, CultureInfo.InvariantCulture);
            if (t == typeof(char?)) return Convert.ToChar(ob, CultureInfo.InvariantCulture);
            if (t == typeof(DateTime?)) return Convert.ToDateTime(ob, CultureInfo.InvariantCulture);
            if (t == typeof(decimal?)) return Convert.ToDecimal(ob, CultureInfo.InvariantCulture);
            if (t == typeof(double?)) return Convert.ToDouble(ob, CultureInfo.InvariantCulture);
            if (t == typeof(short?)) return Convert.ToInt16(ob, CultureInfo.InvariantCulture);
            if (t == typeof(int?)) return Convert.ToInt32(ob, CultureInfo.InvariantCulture);
            if (t == typeof(long?)) return Convert.ToInt64(ob, CultureInfo.InvariantCulture);
            if (t == typeof(sbyte?)) return Convert.ToSByte(ob, CultureInfo.InvariantCulture);
            if (t == typeof(float?)) return Convert.ToSingle(ob, CultureInfo.InvariantCulture);
            if (t == typeof(Guid))
            {
                return new Guid(ob.ToString());
            }


            if (t == typeof(byte[])) return ob;
            if (t == typeof(object)) return ob;
            throw new Exception(string.Format(CultureInfo.CurrentCulture, "не могу конвертировать тип {0}",
                ob.GetType().FullName));
        }


       


        internal static string TanslycatorFieldParam1(string nameField, string table1)
        {
            var matsup =
                new Regex(@"[aA-zZаА-яЯ\d[_]*]*\.[aA-zZаА-яЯ\d[_]*]*").Matches(nameField.Replace("`", "")
                    .Replace("[", "").Replace("]", ""));
            var dd = matsup[0].ToString().Split('.');
            return string.Format("{0}.{1}", table1, dd[1]);
        }

        internal static object ConvertatorPrimaryKeyType(Type typeColumn, object val)
        {
            if (typeColumn == typeof(Guid))
                return (Guid) val;
            if (typeColumn == typeof(decimal))
                return val;
            if (typeColumn == typeof(short))
                return decimal.ToInt16((decimal) val);
            if (typeColumn == typeof(int))
                return decimal.ToInt32((decimal) val);
            if (typeColumn == typeof(long))
                return decimal.ToInt64((decimal) val);
            if (typeColumn == typeof(ushort))
                return decimal.ToUInt16((decimal) val);
            if (typeColumn == typeof(uint))
                return decimal.ToUInt32((decimal) val);
            if (typeColumn == typeof(ulong))
                return decimal.ToUInt64((decimal) val);

            if (typeColumn == typeof(double))
                return decimal.ToDouble((decimal) val);
            if (typeColumn == typeof(double))
                return decimal.ToSByte((sbyte) val);
            if (typeColumn == typeof(float))
                return decimal.ToSingle((sbyte) val);
            throw new Exception("Не могу найти тип для преобразования первичного ключа");
        }

        internal static bool ColumnExists(IDataReader reader, string columnName)
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i) == columnName) return true;
            }

            return false;
        }

        internal static bool IsJsonType(Type type)
        {
            if (type == typeof(byte[]))
            {
                return false;
            }
            if (type.GetInterface(typeof(ISerializableOrm).Name)!=null||type.IsArray)
            {
                return true;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)
                || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>)
                || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(HashSet<>))
                return true;

            return false;
        }

        public static string ObjectToJson(object o)
        {
            string json =JsonConvert.SerializeObject(o);
            return  json.Replace("'","''");
        }

        public static object JsonToObject(string o, Type type)
        {
            return  JsonConvert.DeserializeObject(o, type);
        }

        public static string ClearTrim(string tableName)
        {
            return tableName.Trim(' ', '`', '\'', '[', ']', '"');
        }

        public static string GetStringSql(IDbCommand command)
        {
            var stringBuilder = new StringBuilder(command.CommandText);
            if (command.Parameters.Count > 0)
            {
                stringBuilder.Append(" params: ");
                foreach (DbParameter commandParameter in command.Parameters)
                    stringBuilder.Append($" {commandParameter.ParameterName} - {commandParameter.Value} ");
            }
            

            return stringBuilder.ToString();
        }
    }
}