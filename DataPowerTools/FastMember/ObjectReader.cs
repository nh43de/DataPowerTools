using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using DataPowerTools.Extensions;

namespace DataPowerTools.FastMember
{
    /// <summary>
    ///     Provides a means of reading a sequence of objects as a data-reader, for example
    ///     for use with SqlBulkCopy or other data-base oriented code
    /// </summary>
    public class ObjectReader : DbDataReader
    {
        /// <summary>
        /// The source type of the underlying enumerable.
        /// </summary>
        public Type SourceType => _type;

        private Type _type;
        private readonly IEnumerable _enumerableSource;
        private readonly Func<Type, string[]> _typeMemberSelector;
        private string[] _memberNames;

        private TypeAccessor accessor;
        private BitArray allowNull;
        private Type[] effectiveTypes;
        private bool active = true;
        private object current;
        private IEnumerator source;


        /// <summary>
        ///     Creates a new ObjectReader instance for reading the supplied data
        /// </summary>
        /// <param name="type">The expected Type of the information to be read</param>
        /// <param name="enumerableSource"></param>
        /// <param name="members"></param>
        public ObjectReader(IEnumerable enumerableSource, Type type, params string[] members) : this(enumerableSource, members)
        {
            if (type != null)
            {
                _type = type;
                Initialize();
            }
        }
        
        /// <summary>
        ///     Creates a new ObjectReader instance for reading the supplied data
        /// </summary>
        /// <param name="enumerableSource"></param>
        /// <param name="members">The members that should be exposed to the reader</param>
        public ObjectReader(IEnumerable enumerableSource, params string[] members)
        {
            _enumerableSource = enumerableSource;
            source = _enumerableSource.GetEnumerator();
            _memberNames = (string[]) members?.Clone();
            if (enumerableSource == null) throw new ArgumentOutOfRangeException(nameof(enumerableSource));
        }
        
        /// <summary>
        /// Defers type column generation until first one is read.
        /// </summary>
        /// <param name="enumerableSource"></param>
        /// <param name="typeMemberSelector"></param>
        public ObjectReader(IEnumerable enumerableSource, Func<Type, string[]> typeMemberSelector)
        {
            _enumerableSource = enumerableSource;
            _typeMemberSelector = typeMemberSelector;
            source = _enumerableSource.GetEnumerator();
            if (enumerableSource == null) throw new ArgumentOutOfRangeException(nameof(enumerableSource));
        }
        
        /// <summary>
        ///     Creates a new ObjectReader instance for reading the supplied data
        /// </summary>
        /// <param name="source">The sequence of objects to represent</param>
        /// <param name="ignoreNonStringReferenceTypes"></param>
        public static ObjectReader Create<T>(IEnumerable<T> source, bool ignoreNonStringReferenceTypes)
        {
            if (typeof(T) != typeof(object))
            {
                return new ObjectReader(source, typeof(T), typeof(T).GetColumnMemberNames(ignoreNonStringReferenceTypes));
            }
            return new ObjectReader(source, t => t.GetColumnMemberNames(ignoreNonStringReferenceTypes));
        }

        /// <summary>
        ///     Creates a new ObjectReader instance for reading the supplied data
        /// </summary>
        /// <param name="source">The sequence of objects to represent</param>
        /// <param name="members">The members that should be exposed to the reader</param>
        public static ObjectReader Create<T>(IEnumerable<T> source, params string[] members)
        {
            return new ObjectReader(source, members);
        }

        private void Initialize()
        {
            if (_type == null || (current != null && current.GetType() != _type))
            {
                _type = current.GetType();
            }

            if (_typeMemberSelector != null)
            {
                _memberNames = _typeMemberSelector(_type);
            }

            var allMembers = _memberNames == null || _memberNames.Length == 0;

            accessor = TypeAccessor.Create(_type);
            if (accessor.GetMembersSupported)
            {
                var typeMembers = accessor.GetMembers();

                if (allMembers)
                {
                    _memberNames = new string[typeMembers.Count];
                    for (var i = 0; i < _memberNames.Length; i++)
                    {
                        _memberNames[i] = typeMembers[i].Name;
                    }
                }

                allowNull = new BitArray(_memberNames.Length);
                effectiveTypes = new Type[_memberNames.Length];
                for (var i = 0; i < _memberNames.Length; i++)
                {
                    Type memberType = null;
                    var iAllowNull = true;
                    var hunt = _memberNames[i];
                    foreach (var member in typeMembers)
                    {
                        if (member.Name == hunt)
                        {
                            if (memberType == null)
                            {
                                var tmp = member.Type;
                                memberType = Nullable.GetUnderlyingType(tmp) ?? tmp;

                                iAllowNull = !(TypeHelpers._IsValueType(memberType) && memberType == tmp);

                                // but keep checking, in case of duplicates
                            }
                            else
                            {
                                memberType = null; // duplicate found; say nothing
                                break;
                            }
                        }
                    }
                    allowNull[i] = iAllowNull;
                    effectiveTypes[i] = memberType ?? typeof(object);
                }
            }
            else if (allMembers)
            {
                throw new InvalidOperationException(
                    "Member information is not available for this type; the required members must be specified explicitly");
            }

            //current = null;
            _isInitialized = true;
        }

        public override int Depth => 0;

        public override bool HasRows => active;

        public override int RecordsAffected => 0;

        public override int FieldCount => _memberNames?.Length ?? 0;

        public override bool IsClosed => source == null;

        public override object this[string name] => accessor[current, name] ?? DBNull.Value;

        /// <summary>
        ///     Gets the value of the current object in the member specified
        /// </summary>
        public override object this[int i] => accessor[current, _memberNames[i]] ?? DBNull.Value;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing) Shutdown();
        }

        public override bool GetBoolean(int i)
        {
            return (bool) this[i];
        }

        public override byte GetByte(int i)
        {
            return (byte) this[i];
        }

        public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            var s = (byte[]) this[i];
            var available = s.Length - (int) fieldOffset;
            if (available <= 0) return 0;

            var count = TypeHelpers.Min(length, available);
            Buffer.BlockCopy(s, (int) fieldOffset, buffer, bufferoffset, count);
            return count;
        }

        public override char GetChar(int i)
        {
            return (char) this[i];
        }

        public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            var s = (string) this[i];
            var available = s.Length - (int) fieldoffset;
            if (available <= 0) return 0;

            var count = TypeHelpers.Min(length, available);
            s.CopyTo((int) fieldoffset, buffer, bufferoffset, count);
            return count;
        }

        public override string GetDataTypeName(int i)
        {
            return (effectiveTypes == null ? typeof(object) : effectiveTypes[i]).Name;
        }

        public override DateTime GetDateTime(int i)
        {
            return (DateTime) this[i];
        }

        protected override DbDataReader GetDbDataReader(int i)
        {
            throw new NotSupportedException();
        }

        public override decimal GetDecimal(int i)
        {
            return (decimal) this[i];
        }

        public override double GetDouble(int i)
        {
            return (double) this[i];
        }

        public override IEnumerator GetEnumerator()
        {
#if COREFX
            throw new NotImplementedException(); // https://github.com/dotnet/corefx/issues/4646
#else
            return new DbEnumerator(this);
#endif
        }

        public override Type GetFieldType(int i)
        {
            return effectiveTypes == null ? typeof(object) : effectiveTypes[i];
        }

        public override float GetFloat(int i)
        {
            return (float) this[i];
        }

        public override Guid GetGuid(int i)
        {
            return (Guid) this[i];
        }

        public override short GetInt16(int i)
        {
            return (short) this[i];
        }

        public override int GetInt32(int i)
        {
            return (int) this[i];
        }

        public override long GetInt64(int i)
        {
            return (long) this[i];
        }

        public override string GetName(int i)
        {
            return _memberNames[i];
        }

        public override int GetOrdinal(string name)
        {
            return Array.IndexOf(_memberNames, name);
        }

        public override string GetString(int i)
        {
            return (string) this[i];
        }

        public override object GetValue(int i)
        {
            return this[i];
        }

        public override int GetValues(object[] values)
        {
            // duplicate the key fields on the stack
            var members = _memberNames;
            var current = this.current;
            var accessor = this.accessor;

            var count = TypeHelpers.Min(values.Length, members.Length);
            for (var i = 0; i < count; i++) values[i] = accessor[current, members[i]] ?? DBNull.Value;
            return count;
        }

        public override bool IsDBNull(int i)
        {
            return this[i] is DBNull;
        }

        public override bool NextResult()
        {
            active = false;
            return false;
        }

        private bool _isInitialized = false;
        
        public override bool Read()
        {
            if (active)
            {
                var tmp = source;
                
                if (tmp != null && tmp.MoveNext())
                {
                    //initialize to get enumerated type (first one only)
                    current = tmp.Current;

                    if (_isInitialized == false)
                    {
                        Initialize();
                    }

                    return true;
                }

                active = false;
            }
            current = null;
            return false;
        }

        private void Shutdown()
        {
            active = false;
            current = null;
            var tmp = source as IDisposable;
            source = null;
            if (tmp != null) tmp.Dispose();
        }

#if !COREFX
        public override DataTable GetSchemaTable()
        {
            // these are the columns used by DataTable load
            var table = new DataTable
            {
                Columns =
                {
                    {"ColumnOrdinal", typeof(int)},
                    {"ColumnName", typeof(string)},
                    {"DataType", typeof(Type)},
                    {"ColumnSize", typeof(int)},
                    {"AllowDBNull", typeof(bool)}
                }
            };
            var rowData = new object[5];
            for (var i = 0; i < _memberNames.Length; i++)
            {
                rowData[0] = i;
                rowData[1] = _memberNames[i];
                rowData[2] = effectiveTypes == null ? typeof(object) : effectiveTypes[i];
                rowData[3] = -1;
                rowData[4] = allowNull == null ? true : allowNull[i];
                table.Rows.Add(rowData);
            }
            return table;
        }

        public override void Close()
        {
            Shutdown();
        }
#endif
    }
}