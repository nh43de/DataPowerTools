using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataPowerTools.DataReaderExtensibility.TransformingReaders;
using DataPowerTools.DataStructures;

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
        /// Gets or sets a value indicating whether to use a row from the data as column names.
        /// </summary>
        public bool UseHeaderRow { get; set; } = false;

        /// <summary>
        /// Gets or sets a callback to determine which row is the header row. Only called when UseHeaderRow = true.
        /// </summary>
        public Action<IDataReader> ReadHeaderRow { get; set; }

        /// <summary>
        /// Gets or sets a callback to determine whether to include the current row in the DataTable.
        /// </summary>
        public Func<IDataReader, bool> FilterRow { get; set; }

        /// <summary>
        /// Gets or sets a callback to determine whether to include the specific column in the DataTable. Called once per column after reading the headers.
        /// </summary>
        public Func<IDataReader, int, bool> FilterColumn { get; set; }
    }

    /// <summary>
    /// Reads for headers in a data reader and applies column aliases. 
    /// </summary>
    public class HeaderReaderAdapter : ExtensibleDataReaderBase<IDataReader>
    {
        public override int FieldCount => _columns.Value.Length;


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
        
        private readonly Lazy<SimpleColumnInfo[]> _columns;

        private readonly Lazy<BidirectionalMap<string, int>> _columnNamesToIndex;



        public HeaderReaderAdapter(IDataReader dataReader, HeaderReaderConfiguration configuration) : base(dataReader)
        {
            //build col info
            _columns = new Lazy<SimpleColumnInfo[]>(() =>
            {
                var d = GetHeaderInformation(configuration).ToArray();

                return d;
            });

            //build col names index
            _columnNamesToIndex = new Lazy<BidirectionalMap<string, int>>(() =>
            {
                var cols = _columns.Value
                    .Select(c => new KeyValuePair<string, int>(c.ColumnName, c.Index))
                    .ToArray();

                var r = new BidirectionalMap<string, int>(cols);

                return r;
            });
        }
        

        /// <summary>
        /// Turns this into an excel data reader
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private IEnumerable<SimpleColumnInfo> GetHeaderInformation(HeaderReaderConfiguration configuration)
        {
            var first = true;

            var cols = new List<SimpleColumnInfo>();

            while (DataReader.Read())
            {
                if (first)
                {
                    if (configuration.UseHeaderRow && configuration.ReadHeaderRow != null)
                    {
                        configuration.ReadHeaderRow(DataReader);
                    }

                    for (var i = 0; i < DataReader.FieldCount; i++)
                    {
                        if (configuration.FilterColumn != null && !configuration.FilterColumn(DataReader, i))
                        {
                            continue;
                        }

                        //
                        var name = configuration.UseHeaderRow
                            ? Convert.ToString(DataReader.GetValue(i))
                            : null;

                        if (string.IsNullOrEmpty(name))
                        {
                            name = configuration.EmptyColumnNamePrefix + i;
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

                    first = false;

                    if (configuration.UseHeaderRow)
                    {
                        continue;
                    }
                }

                if (configuration.FilterRow != null && !configuration.FilterRow(DataReader))
                {
                    continue;
                }

                if (IsEmptyRow(DataReader))
                {
                    continue;
                }

                break;
            }

            return cols;
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
