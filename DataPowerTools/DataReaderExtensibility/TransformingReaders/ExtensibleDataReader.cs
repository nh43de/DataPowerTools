using System;
using System.Data;

namespace DataPowerTools.DataReaderExtensibility.TransformingReaders
{
    public abstract class ExtensibleDataReader<TDataReader> : IDataReader where TDataReader : IDataReader
    {
        public readonly TDataReader DataReader;


        protected ExtensibleDataReader(TDataReader dataReader)
        {
            DataReader = dataReader;
        }


        public virtual object this[int i] => DataReader[i];
        public virtual object GetValue(int i) => DataReader.GetValue(i);

        public virtual object this[string name] => DataReader[name];

        public virtual int GetValues(object[] values) => DataReader.GetValues(values);

        public virtual void Dispose()
        {
            DataReader.Dispose();
        }

        public virtual string GetName(int i) => DataReader.GetName(i);
        public virtual string GetDataTypeName(int i) => DataReader.GetDataTypeName(i);
        public virtual string GetString(int i) => DataReader.GetString(i);
        public virtual Type GetFieldType(int i) => DataReader.GetFieldType(i);
        public virtual int GetOrdinal(string name) => DataReader.GetOrdinal(name);
        public virtual bool GetBoolean(int i) => DataReader.GetBoolean(i);
        public virtual byte GetByte(int i) => DataReader.GetByte(i);

        public virtual long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
            => DataReader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);

        public virtual char GetChar(int i) => DataReader.GetChar(i);

        public virtual long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
            => DataReader.GetChars(i, fieldoffset, buffer, bufferoffset, length);

        public virtual Guid GetGuid(int i) => DataReader.GetGuid(i);
        public virtual short GetInt16(int i) => DataReader.GetInt16(i);
        public virtual int GetInt32(int i) => DataReader.GetInt32(i);
        public virtual long GetInt64(int i) => DataReader.GetInt64(i);
        public virtual float GetFloat(int i) => DataReader.GetFloat(i);
        public virtual double GetDouble(int i) => DataReader.GetDouble(i);
        public virtual decimal GetDecimal(int i) => DataReader.GetDecimal(i);
        public virtual DateTime GetDateTime(int i) => DataReader.GetDateTime(i);
        public virtual IDataReader GetData(int i) => DataReader.GetData(i);
        public virtual bool IsDBNull(int i) => DataReader.IsDBNull(i);
        public virtual int FieldCount => DataReader.FieldCount;
        public virtual void Close() => DataReader.Close();
        public virtual DataTable GetSchemaTable() => DataReader.GetSchemaTable();
        public virtual bool NextResult() => DataReader.NextResult();
        public virtual bool Read() => DataReader.Read();
        public virtual int Depth => DataReader.Depth;
        public virtual bool IsClosed => DataReader.IsClosed;
        public virtual int RecordsAffected => DataReader.RecordsAffected;
    }
}