using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;

namespace ORM_1_21_
{
    internal static class DbTypeConverter
    {
        //case TypeCode.DBNull:return DbType.Object;
        //          return DbType.Object;

        public static DbType ConvertFrom(Type type)
        {

            if (type == null)
                throw new ArgumentNullException("type");

            if (type == typeof(Char[]))
            {
                return DbType.AnsiStringFixedLength;
            }
            if (type == typeof(DateTimeOffset))
            {
                return DbType.DateTimeOffset;
            }
            if (type == typeof(Image))
            {
                return DbType.Binary;
            }

            if (type == typeof(Byte[]))
            {
                return DbType.Binary;
            }
            if (type == typeof(Boolean))
            {
                return DbType.Boolean;
            }

            if (type == typeof(Byte))
            {
                return DbType.Byte;
            }
            if (type == typeof(Char))
            {
                return DbType.String;
            }
            if (type == typeof(DateTime))
            {
                return DbType.DateTime;
            }
            if (type == typeof(decimal))
            {
                return DbType.Decimal;
            }
            if (type == typeof(double))
            {
                return DbType.Double;
            }
            if (type == typeof(Int16))
            {
                return DbType.Int16;
            }
            if (type == typeof(Int32))
            {
                return DbType.Int32;
            }
            if (type == typeof(Int64))
            {
                return DbType.Int64;
            }
            if (type == typeof(SByte))
            {
                return DbType.SByte;
            }
            if (type == typeof(Single))
            {
                return DbType.Single;
            }
            if (type == typeof(String))
            {
                return DbType.String;
            }
            if (type == typeof(UInt16))
            {
                return DbType.UInt16;
            }
            if (type == typeof(UInt32))
            {
                return DbType.UInt32;
            }
            if (type == typeof(UInt64))
            {
                return DbType.UInt64;
            }

            if (type == typeof(Guid))
            {
                return DbType.Guid;
            }
            if (type == typeof(byte[]))
            {
                return DbType.Binary;
            }
            if (type.BaseType == typeof(Enum))
            {
                return DbType.String;
            }
            if (type == typeof(DateTimeOffset))
            {
                return DbType.DateTimeOffset;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return ConvertFrom(type.GetGenericArguments()[0]);
            }
            if (Utils.IsJsonType(type))
            {
                return DbType.String;
            }

       
            throw new NotSupportedException(string.Format("Unable to convert {0} to a DbType enum value.", type.FullName));
        }

        public static string ConvertSqlType(Type type)
        {
            if (type == typeof(Char[]))
            {
                return "char(100)";
            }
            if (type == typeof(float))
            {
                return "float ";
            }
            if (type == typeof(Byte))
            {
                return "varbinary";
            }
            if (type == typeof(Char))
            {
                return "char ";
            }

            if (type == typeof(decimal))
            {
                return "decimal";
            }
            if (type == typeof(double))
            {
                return "float";
            }
            if (type == typeof(Int16))
            {
                return "int";
            }
            if (type == typeof(Int32))
            {
                return "int";
            }
            if (type == typeof(Int64))
            {
                return "int";
            }
            if (type == typeof(SByte))
            {
                return "int";
            }
            if (type == typeof(Single))
            {
                return "float ";
            }
            if (type == typeof(String))
            {
                return "ntext ";
            }
            if (type == typeof(UInt16))
            {
                return "int";
            }
            if (type == typeof(UInt32))
            {
                return "int";
            }
            if (type == typeof(UInt64))
            {
                return "int";
            }

            if (type == typeof(Guid))
            {
                return "ntext";
            }
            if (type == typeof(byte[]))
            {
                return "varbinary";
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return ConvertSqlType(type.GetGenericArguments()[0]);
            }
            throw new NotSupportedException(string.Format("Unable to convert {0} to a SqlType enum value.", type.FullName));
        }
    }
}
