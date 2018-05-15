using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DataPowerTools.DataReaderExtensibility.Readers
{
    /// <summary>
    /// Use fastmember instead.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    [Obsolete] 
    public sealed class ObjectDataReader<TData> : IDataReader
    {
        private static readonly Lazy<PropertyAccessor> SPropertyAccessorCache = new Lazy<PropertyAccessor>(() =>
        {
            var propertyAccessors = typeof(TData)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead)
                .Select((p, i) => new
                {
                    Index = i,
                    Property = p,
                    Accessor = CreatePropertyAccessor(p)
                })
                .ToArray();

            return new PropertyAccessor
            {
                Accessors = propertyAccessors.Select(p => p.Accessor).ToList(),
                Lookup =
                    propertyAccessors.ToDictionary(p => p.Property.Name, p => p.Index, StringComparer.OrdinalIgnoreCase)
            };
        });

        private IEnumerator<TData> _mDataEnumerator;

        public ObjectDataReader(IEnumerable<TData> data)
        {
            _mDataEnumerator = data.GetEnumerator();
        }

        private static Func<TData, object> CreatePropertyAccessor(PropertyInfo p)
        {
            var parameter = Expression.Parameter(typeof(TData), "input");
            var propertyAccess = Expression.Property(parameter, p.GetGetMethod());
            var castAsObject = Expression.TypeAs(propertyAccess, typeof(object));
            var lamda = Expression.Lambda<Func<TData, object>>(castAsObject, parameter);
            return lamda.Compile();
        }

        private class PropertyAccessor
        {
            public List<Func<TData, object>> Accessors { get; set; }
            public Dictionary<string, int> Lookup { get; set; }
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

        public bool IsClosed => _mDataEnumerator == null;

        public bool NextResult()
        {
            return false;
        }

        public bool Read()
        {
            if (IsClosed)
                throw new ObjectDisposedException(GetType().Name);
            return _mDataEnumerator.MoveNext();
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
                if (_mDataEnumerator != null)
                {
                    _mDataEnumerator.Dispose();
                    _mDataEnumerator = null;
                }
        }

        #endregion

        #region IDataRecord Members

        public int GetOrdinal(string name)
        {
            int ordinal;
            if (!SPropertyAccessorCache.Value.Lookup.TryGetValue(name, out ordinal))
                throw new InvalidOperationException("Unknown parameter name: " + name);
            return ordinal;
        }

        public object GetValue(int i)
        {
            if (_mDataEnumerator == null)
                throw new ObjectDisposedException(GetType().Name);
            return SPropertyAccessorCache.Value.Accessors[i](_mDataEnumerator.Current);
        }

        public int FieldCount => SPropertyAccessorCache.Value.Accessors.Count;

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