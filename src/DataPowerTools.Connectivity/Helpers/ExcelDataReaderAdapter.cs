using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDataReader;

namespace DataPowerTools.Connectivity.Helpers
{
    public class ExcelDataReaderAdapter
    {
        public ExcelDataReaderAdapter(IExcelDataReader reader)
        {
            
        }
        
        /// <summary>
        /// Turns this into an excel data reader
        /// </summary>
        /// <param name="self"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private static IDataReader AsDataReader(IExcelDataReader self, ExcelDataTableConfiguration configuration)
        {
            var result = (IDataReader) null; //

            //var result = new DataTable { TableName = self.Name };
            //result.ExtendedProperties.Add("visiblestate", self.VisibleState);
            var first = true;
            var emptyRows = 0;
            var columnIndices = new List<int>();
            while (self.Read())
            {
                if (first)
                {
                    if (configuration.UseHeaderRow && configuration.ReadHeaderRow != null)
                    {
                        configuration.ReadHeaderRow(self);
                    }

                    for (var i = 0; i < self.FieldCount; i++)
                    {
                        if (configuration.FilterColumn != null && !configuration.FilterColumn(self, i))
                        {
                            continue;
                        }

                        //
                        var name = configuration.UseHeaderRow
                            ? Convert.ToString(self.GetValue(i))
                            : null;

                        if (string.IsNullOrEmpty(name))
                        {
                            name = configuration.EmptyColumnNamePrefix + i;
                        }

                        // if a column already exists with the name append _i to the duplicates
                        var columnName = GetUniqueColumnName(result, name);
                        var column = new DataColumn(columnName, typeof(object)) { Caption = name };
                        result.Columns.Add(column);
                        columnIndices.Add(i);
                    }

                    result.BeginLoadData();
                    first = false;

                    if (configuration.UseHeaderRow)
                    {
                        continue;
                    }
                }

                if (configuration.FilterRow != null && !configuration.FilterRow(self))
                {
                    continue;
                }

                if (IsEmptyRow(self))
                {
                    emptyRows++;
                    continue;
                }

                for (var i = 0; i < emptyRows; i++)
                {
                    result.Rows.Add(result.NewRow());
                }

                emptyRows = 0;

                var row = result.NewRow();

                for (var i = 0; i < columnIndices.Count; i++)
                {
                    var columnIndex = columnIndices[i];
                    var value = self.GetValue(columnIndex);
                    row[i] = value;
                }

                result.Rows.Add(row);
            }

            result.EndLoadData();
            return result;
        }



        private static bool IsEmptyRow(IExcelDataReader reader)
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetValue(i) != null)
                    return false;
            }

            return true;
        }

        private static string GetUniqueColumnName(DataTable table, string name)
        {
            var columnName = name;
            var i = 1;
            while (table.Columns[columnName] != null)
            {
                columnName = string.Format("{0}_{1}", name, i);
                i++;
            }

            return columnName;
        }

    }
}
