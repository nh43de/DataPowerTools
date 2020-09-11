using System;
using System.Data;

namespace DataPowerTools.DataReaderExtensibility.Columns
{
    public struct DbColumnInfo
    {
        public string FieldName { get; set; }
        public int Size { get; set; }
        public DbType DataType { get; set; }

        public static DbType StringToDataType(string type) //TODO: not complete
        {
            switch (type)
            {
                case "int":
                    return DbType.Int32;
                case "decimal":
                    return DbType.Decimal;
                case "datetime":
                case "smalldatetime":
                    return DbType.DateTime;
                case "guid":
                case "uniqueidentifier":
                    return DbType.Guid;
                case "xml":
                    return DbType.Xml;
                default:
                    return DbType.String;
            }
        }

        public static object StringToTypedValue(string value, DbType type)
        {
            if (string.IsNullOrEmpty(value))
                return null;
            switch (type)
            {
                case DbType.Int32:
                    return int.Parse(value);
                case DbType.Decimal:
                    return decimal.Parse(value);
                case DbType.DateTime:
                    return DateTime.Parse(value);
                case DbType.Guid:
                    return Guid.Parse(value);
                case DbType.Xml:
                    return value;
                default:
                    return value;
            }
        }

        public override string ToString()
        {
            return $"{FieldName}: {DataType}({Size})";
        }
    }
}