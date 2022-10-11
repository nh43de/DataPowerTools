using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DataPowerTools.Extensions
{
    public static class DataRowExtensions
    {
        /// <summary>
        /// Pretty-prints a DataRow.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public static string PrintRow(this DataRow row)
        {
            var _rtn = "";

            var colVals = new List<ColNameVal>();

            foreach (DataColumn colItem in row.Table.Columns)
                colVals.Add(new ColNameVal(colItem.ColumnName, row[colItem.ColumnName].ToString()));

            var valToDisplay = new Func<string, string>(val => string.IsNullOrEmpty(val) ? "(null)" : val);

            _rtn = string.Join(", ", colVals.Select(v => $"{{\"{v.ColName}\": \"{valToDisplay(v.ColVal)}\"}}"));

            return _rtn;
        }


        /// <summary>
        /// Class representing column name-value pairs.
        /// </summary>
        private class ColNameVal
        {
            public ColNameVal(string colName, string colVal)
            {
                ColName = colName;
                ColVal = colVal;
            }

            public string ColName { get; }
            public string ColVal { get; }
        }
    }
}