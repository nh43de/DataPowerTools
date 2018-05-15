using System.Collections.Generic;
using System.Data;

namespace DataPowerTools.Extensions
{
    public static class DataSetExtensions
    {
        /// <summary>
        /// Gets a list of table names of the data table collection e.g. GetTablenames(dataSet.Tables).
        /// </summary>
        /// <param name="tables"></param>
        /// <returns></returns>
        private static IList<string> GetTablenames(this DataTableCollection tables)
        {
            var tableList = new List<string>();
            foreach (var table in tables)
                tableList.Add(table.ToString());

            return tableList;
        }

        /// <summary>
        /// Gets a list of table names of the data set.
        /// </summary>
        /// <param name="dataset"></param>
        /// <returns></returns>
        private static IList<string> GetTablenames(this DataSet dataset)
        {
            return dataset.Tables.GetTablenames();
        }
    }
}