using Microsoft.Data.SqlClient;
using DataPowerTools.DataReaderExtensibility.Columns;

namespace DataPowerTools.DataConnectivity.Sql
{
    public class SqlServerBulkInsertOptions : GenericBulkCopyOptions
    {
        public SqlBulkCopyOptions SqlBulkCopyOptions { get; set; } = SqlBulkCopyOptions.TableLock |
                                                                     SqlBulkCopyOptions.FireTriggers;
        public SqlTransaction SqlTransaction { get; set; }

        public new int BatchSize { get; set; } = 2000;
    }
}