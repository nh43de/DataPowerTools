using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataPowerTools.DataReaderExtensibility.Columns;

namespace DataPowerTools.DataReaderExtensibility.Readers
{
    public sealed class DictionaryDataReader<T> : IDataReader
    {
        private readonly IList<DbColumnInfo> m_schema;
        private readonly IDictionary<string, int> m_schemaMapping;
        private readonly Func<T, string, object> m_selector;
        private IEnumerator<T> m_dataEnumerator;

        public DictionaryDataReader(IList<DbColumnInfo> schema, IEnumerable<T> data, Func<T, string, object> selector)
        {
            m_schema = schema;
            m_schemaMapping = m_schema.Select((x, i) => new {x.FieldName, Index = i})
                .ToDictionary(x => x.FieldName, x => x.Index);
            m_selector = selector;
            m_dataEnumerator = data.GetEnumerator();
        }

        #region IDataReader Members

        public void Close()
        {
            Dispose();
        }

        public int Depth => 1;

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public bool IsClosed => m_dataEnumerator == null;

        public bool NextResult()
        {
            return false;
        }

        public bool Read()
        {
            if (IsClosed)
                throw new ObjectDisposedException(GetType().Name);
            return m_dataEnumerator.MoveNext();
        }

        public int RecordsAffected => -1;

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
                if (m_dataEnumerator != null)
                {
                    m_dataEnumerator.Dispose();
                    m_dataEnumerator = null;
                }
        }

        #endregion

        #region IDataRecord Members

        public int FieldCount => m_schema.Count;

        public int GetOrdinal(string name)
        {
            int ordinal;
            if (!m_schemaMapping.TryGetValue(name, out ordinal))
                throw new InvalidOperationException("Unknown parameter name: " + name);
            return ordinal;
        }

        public object GetValue(int i)
        {
            if (m_dataEnumerator == null)
                throw new ObjectDisposedException(GetType().Name);

            var value = m_selector(m_dataEnumerator.Current, m_schema[i].FieldName);

            if (value == null)
                return DBNull.Value;

            var strValue = value as string;
            if (strValue != null)
            {
                if ((strValue.Length > m_schema[i].Size) && (m_schema[i].Size > 0))
                    strValue = strValue.Substring(0, m_schema[i].Size);
                if (m_schema[i].DataType == DbType.String)
                    return strValue;
                return DbColumnInfo.StringToTypedValue(strValue, m_schema[i].DataType) ?? DBNull.Value;
            }

            return value;
        }

        #region Not Implemented Members

        public bool GetBoolean(int i)
        {
            throw new NotImplementedException();
        }

        public byte GetByte(int i)
        {
            throw new NotImplementedException();
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            throw new NotImplementedException();
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            throw new NotImplementedException();
        }

        public decimal GetDecimal(int i)
        {
            throw new NotImplementedException();
        }

        public double GetDouble(int i)
        {
            throw new NotImplementedException();
        }

        public Type GetFieldType(int i)
        {
            throw new NotImplementedException();
        }

        public float GetFloat(int i)
        {
            throw new NotImplementedException();
        }

        public Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        public short GetInt16(int i)
        {
            throw new NotImplementedException();
        }

        public int GetInt32(int i)
        {
            throw new NotImplementedException();
        }

        public long GetInt64(int i)
        {
            throw new NotImplementedException();
        }

        public string GetName(int i)
        {
            throw new NotImplementedException();
        }

        public string GetString(int i)
        {
            throw new NotImplementedException();
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            throw new NotImplementedException();
        }

        public object this[string name]
        {
            get { throw new NotImplementedException(); }
        }

        public object this[int i]
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #endregion
    }
}