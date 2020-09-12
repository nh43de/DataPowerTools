using System;
using System.Data;

namespace DataPowerTools.DataReaderExtensibility.TransformingReaders
{
    public abstract class ExtensibleDataReaderExplicit<TDataReader> : IDataReader where TDataReader: IDataReader
    {
        public readonly TDataReader DataReader;


        protected ExtensibleDataReaderExplicit(TDataReader dataReader)
        {
            DataReader = dataReader;
        }


        public virtual object this[int i]
        {
            get { throw new NotSupportedException(); }
        }


        public virtual object GetValue(int i)
        {
            throw new NotSupportedException();
        }


        public virtual object this[string name]
        {
            get { throw new NotSupportedException(); }
        }

        public virtual int GetValues(object[] values)
        {
            throw new NotSupportedException();
        }

        public virtual void Dispose()
        {
            DataReader.Dispose();
        }

        public virtual string GetName(int i)
        {
            throw new NotSupportedException();
        }

        public virtual string GetDataTypeName(int i)
        {
            throw new NotSupportedException();
        }

        public virtual string GetString(int i)
        {
            throw new NotSupportedException();
        }

        public virtual Type GetFieldType(int i)
        {
            throw new NotSupportedException();
        }

        public virtual int GetOrdinal(string name)
        {
            throw new NotSupportedException();
        }

        public virtual bool GetBoolean(int i)
        {
            throw new NotSupportedException();
        }

        public virtual byte GetByte(int i)
        {
            throw new NotSupportedException();
        }

        public virtual long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotSupportedException();
        }

        public virtual char GetChar(int i)
        {
            throw new NotSupportedException();
        }

        public virtual long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotSupportedException();
        }

        public virtual Guid GetGuid(int i)
        {
            throw new NotSupportedException();
        }

        public virtual short GetInt16(int i)
        {
            throw new NotSupportedException();
        }

        public virtual int GetInt32(int i)
        {
            throw new NotSupportedException();
        }

        public virtual long GetInt64(int i)
        {
            throw new NotSupportedException();
        }

        public virtual float GetFloat(int i)
        {
            throw new NotSupportedException();
        }

        public virtual double GetDouble(int i)
        {
            throw new NotSupportedException();
        }

        public virtual decimal GetDecimal(int i)
        {
            throw new NotSupportedException();
        }

        public virtual DateTime GetDateTime(int i)
        {
            throw new NotSupportedException();
        }

        public virtual IDataReader GetData(int i)
        {
            throw new NotSupportedException();
        }

        public virtual bool IsDBNull(int i)
        {
            throw new NotSupportedException();
        }

        public virtual int FieldCount
        {
            get { throw new NotSupportedException(); }
        }

        public virtual void Close()
        {
            throw new NotSupportedException();
        }

        public virtual DataTable GetSchemaTable()
        {
            throw new NotSupportedException();
        }

        public virtual bool NextResult()
        {
            throw new NotSupportedException();
        }

        public virtual bool Read()
        {
            throw new NotSupportedException();
        }

        public virtual int Depth
        {
            get { throw new NotSupportedException(); }
        }

        public virtual bool IsClosed
        {
            get { throw new NotSupportedException(); }
        }

        public virtual int RecordsAffected
        {
            get { throw new NotSupportedException(); }
        }
    }
}