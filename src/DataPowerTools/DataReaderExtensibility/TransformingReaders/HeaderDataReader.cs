using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataPowerTools.DataReaderExtensibility.TransformingReaders;
using DataPowerTools.DataStructures;
using DataPowerTools.Extensions;

namespace DataPowerTools.Connectivity.Helpers
{
    /// <summary>
    /// Processing configuration options and callbacks for AsDataTable().
    /// </summary>
    public class HeaderReaderConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating the prefix of generated column names.
        /// </summary>
        public string EmptyColumnNamePrefix { get; set; } = "Column";

        /// <summary>
        /// Defaults to true. Whether to use headers at all
        /// </summary>
        public bool UseHeaderRow { get; set; } = true;

        /// <summary>
        /// Gets or sets a callback to determine which row is the header row. Only called when UseHeaderRow = true.
        /// </summary>
        public Action<IDataReader> ReadHeaderRow { get; set; }
    }

    /// <summary>
    /// Reads for headers in a data reader and applies column aliases. 
    /// </summary>
    public class HeaderDataReader : ExtensibleDataReaderBase<IDataReader>
    {
        private readonly HeaderReaderConfiguration _configuration;
        public override int FieldCount => _columnsLazy.Value.Length;

        private bool _firstRead = true;

        public override string GetName(int i)
        {
            var r = _columnNamesToIndex.Value.GetLeft(i);

            return r;
        }

        public override int GetOrdinal(string name)
        {
            var r = _columnNamesToIndex.Value.GetRight(name);

            return r;
        }

        public override int GetChildOrdinal(int i)
        {
            return i;
        }

        public override object GetValue(int i)
        {
            return DataReader.GetValue(i);
        }

        public override bool Read()
        {
            if (_firstRead)
                HeaderInformation();

            return DataReader.Read();
        }

        private SimpleColumnInfo[] _cols;

        private readonly Lazy<SimpleColumnInfo[]> _columnsLazy;

        private readonly Lazy<BidirectionalMap<string, int>> _columnNamesToIndex;


        public override DataTable GetSchemaTable() => throw new NotImplementedException();

        public HeaderDataReader(IDataReader dataReader, HeaderReaderConfiguration configuration) : base(dataReader)
        {
            this._configuration = configuration;

            //build col info
            _columnsLazy = new Lazy<SimpleColumnInfo[]>(() =>
            {
                var d = HeaderInformation();

                return d;
            });

            //build col names index
            _columnNamesToIndex = new Lazy<BidirectionalMap<string, int>>(() =>
            {
                var cols = _columnsLazy.Value
                    .Select(c => new KeyValuePair<string, int>(c.ColumnName, c.Index))
                    .ToArray();

                var r = new BidirectionalMap<string, int>(cols);

                return r;
            });
        }
        

        /// <summary>
        /// Turns this into an excel data reader
        /// </summary>
        /// <returns></returns>
        private SimpleColumnInfo[] HeaderInformation()
        {
            if (_firstRead == false)
                return _cols;

            _firstRead = false;

            var first = true;

            var cols = new List<SimpleColumnInfo>();

            //if not using header rows then just generate new names, don't bother reading from the reader.
            if (_configuration.UseHeaderRow == false)
            {
                for (var i = 0; i < DataReader.FieldCount; i++)
                {
                    var name =  _configuration.EmptyColumnNamePrefix + i;

                    // if a column already exists with the name append _i to the duplicates
                    var columnName = GetUniqueColumnName(cols, name);

                    var column = new SimpleColumnInfo
                    {
                        ColumnName = columnName,
                        ColumnDescription = name,
                        Index = i
                    };

                    cols.Add(column);
                }
            }
            else
            {
                //otherwise yes we are finding a header row to apply
                if (_configuration.UseHeaderRow && _configuration.ReadHeaderRow != null)
                {
                    _configuration.ReadHeaderRow(DataReader);
                }

                for (var i = 0; i < DataReader.FieldCount; i++)
                {
                    var name = Convert.ToString(DataReader.GetValue(i));

                    if (string.IsNullOrEmpty(name))
                    {
                        name = _configuration.EmptyColumnNamePrefix + i;
                    }

                    // if a column already exists with the name append _i to the duplicates
                    var columnName = GetUniqueColumnName(cols, name);

                    var column = new SimpleColumnInfo
                    {
                        ColumnName = columnName,
                        ColumnDescription = name,
                        Index = i
                    };

                    cols.Add(column);
                }
            }

            _cols = cols.ToArray();
            return _cols;
        }

        private static bool IsEmptyRow(IDataReader reader)
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetValue(i) != null)
                    return false;
            }

            return true;
        }

        private static string GetUniqueColumnName(List<SimpleColumnInfo> table, string name)
        {
            var columnName = name;
            var i = 1;
            while (table.Any(t => t.ColumnName == columnName))
            {
                columnName = string.Format("{0}_{1}", name, i);
                i++;
            }

            return columnName;
        }

    }
}
