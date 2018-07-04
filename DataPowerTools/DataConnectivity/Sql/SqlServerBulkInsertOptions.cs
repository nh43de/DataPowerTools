using System.Data.SqlClient;
using DataPowerTools.DataReaderExtensibility.Columns;

namespace DataPowerTools.DataConnectivity.Sql
{
    public class SqlServerBulkInsertOptions : GenericBulkCopyOptions
    {
        public SqlBulkCopyOptions SqlBulkCopyOptions { get; set; } = SqlBulkCopyOptions.TableLock |
                                                                     SqlBulkCopyOptions.FireTriggers;
        public SqlTransaction SqlTransaction { get; set; }
    }


    public class GenericBulkCopyOptions
    {
        public RowsCopiedEventHandler RowsCopiedEventHandler { get; set; }
        /// <summary>
        /// Batch data is chunked into when inserting. The default size is 5,000 records per batch.
        /// </summary>
        public int BatchSize { get; set; } = 5000;
        public bool UseOrdinals { get; set; } = false;

        /// <summary>
        /// The time in seconds to wait for a batch to load. The default is 30 seconds.
        /// </summary>
        public int BulkCopyTimeout { get; set; } = 0;
    }
}