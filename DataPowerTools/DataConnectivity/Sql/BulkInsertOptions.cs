using System.Data.SqlClient;
using DataPowerTools.DataReaderExtensibility.Columns;

namespace DataPowerTools.DataConnectivity.Sql
{
    public class BulkInsertOptions
    {
        public RowsCopiedEventHandler RowsCopiedEventHandler { get; set; }
        public int BatchSize { get; set; } = 5000;
        public bool UseOrdinals { get; set; } = false;
        public int BulkCopyTimeout { get; set; } = 0;
        public SqlBulkCopyOptions SqlBulkCopyOptions { get; set; } = SqlBulkCopyOptions.TableLock |
                                                                     SqlBulkCopyOptions.FireTriggers;
        public SqlTransaction SqlTransaction { get; set; }
        
    }
}