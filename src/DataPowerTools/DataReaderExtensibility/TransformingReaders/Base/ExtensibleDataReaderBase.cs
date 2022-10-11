using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DataPowerTools.Extensions.DataConversionExtensions;
using DataPowerTools.Extensions.Objects;

namespace DataPowerTools.DataReaderExtensibility.TransformingReaders
{
    /// <summary>
    /// (i) This is the derived data reader class.
    /// </summary>
    /// <typeparam name="TDataReader"></typeparam>
    public abstract class ExtensibleDataReaderBase<TDataReader> : IDataReader where TDataReader : IDataReader
    {
        //(j) this is the child data reader
        public readonly TDataReader DataReader;


        protected ExtensibleDataReaderBase(TDataReader dataReader)
        {
            DataReader = dataReader;
        }


        /// <summary>
        /// How many fields the data reader has.
        /// The cardinality of (i) column indexes.
        /// </summary>
        public abstract int FieldCount { get; }

        /// <summary>
        /// Mapping from (i) column ordinal -> returns name of the column.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public abstract string GetName(int i);

        /// <summary>
        /// Mapping from column name -> returns column ordinal (i).
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public abstract int GetOrdinal(string name);

        /// <summary>
        /// Gets the index of the child datareader (j) from the index (i) of this data reader.
        /// (i) -> (j) mapping.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public abstract int GetChildOrdinal(int i);

        /// <summary>
        /// Gets the value of the 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public abstract object GetValue(int i);


        // get values
        //
        public virtual object this[int i] => GetValue(i);

        public virtual object this[string name] => GetValue(GetOrdinal(name));

        //
        //
        public virtual string GetDataTypeName(int i) => DataReader.GetDataTypeName(GetChildOrdinal(i));
        public virtual string GetString(int i) => DataReader.GetString(GetChildOrdinal(i));
        public virtual Type GetFieldType(int i) => DataReader.GetFieldType(GetChildOrdinal(i));
        public virtual bool GetBoolean(int i) => DataReader.GetBoolean(GetChildOrdinal(i));
        public virtual byte GetByte(int i) => DataReader.GetByte(GetChildOrdinal(i));

        public virtual long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
            => DataReader.GetBytes(GetChildOrdinal(i), fieldOffset, buffer, bufferoffset, length);

        public virtual char GetChar(int i) => DataReader.GetChar(GetChildOrdinal(i));

        public virtual long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
            => DataReader.GetChars(GetChildOrdinal(i), fieldoffset, buffer, bufferoffset, length);

        public virtual Guid GetGuid(int i) => DataReader.GetGuid(GetChildOrdinal(i));
        public virtual short GetInt16(int i) => DataReader.GetInt16(GetChildOrdinal(i));
        public virtual int GetInt32(int i) => DataReader.GetInt32(GetChildOrdinal(i));
        public virtual long GetInt64(int i) => DataReader.GetInt64(GetChildOrdinal(i));
        public virtual float GetFloat(int i) => DataReader.GetFloat(GetChildOrdinal(i));
        public virtual double GetDouble(int i) => DataReader.GetDouble(GetChildOrdinal(i));
        public virtual decimal GetDecimal(int i) => DataReader.GetDecimal(GetChildOrdinal(i));
        public virtual DateTime GetDateTime(int i) => DataReader.GetDateTime(GetChildOrdinal(i));
        public virtual IDataReader GetData(int i) => DataReader.GetData(GetChildOrdinal(i));
        public virtual bool IsDBNull(int i) => DataReader.IsDBNull(GetChildOrdinal(i));

        //
        //
        public virtual void Close() => DataReader.Close();

        public virtual DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
            //var childSchemaTable = DataReader.GetSchemaTable();

            //if (childSchemaTable == null)
            //    return null;

            //var newSchemaTable = childSchemaTable.Clone();

            //newSchemaTable.Rows.Clear();

            //var i = 0;
            //for (; i < FieldCount; i++)
            //{
            //    foreach (DataRow stRow in childSchemaTable.Rows)
            //    {
            //        var ordinal = stRow[SchemaTableColumn.ColumnOrdinal].ToString().ToNullableInt();
            //        var colName = stRow[SchemaTableColumn.ColumnName].ToString();

            //        if (ordinal == i)
            //            newSchemaTable.Rows.Add();
            //    }
            //}

            //return newSchemaTable;
        }

        public virtual bool NextResult() => DataReader.NextResult();
        public virtual bool Read() => DataReader.Read();
        public virtual int Depth => DataReader.Depth;
        public virtual bool IsClosed => DataReader.IsClosed;
        public virtual int RecordsAffected => DataReader.RecordsAffected;

        public virtual int GetValues(object[] values)
        {
            var i = 0;
            for (; i < FieldCount; i++)
            {
                if (values.Length <= i)
                    return i;
                values[i] = GetValue(i);
            }
            return i;
        }

        public virtual void Dispose()
        {
            DataReader.Dispose();
        }
    }
}