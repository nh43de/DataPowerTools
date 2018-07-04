using System.Threading;

namespace DataPowerTools.DataConnectivity.Sql
{
    public class AsyncSqlServerBulkInsertOptions : SqlServerBulkInsertOptions
    {
        public CancellationToken CancellationToken { get; set; }
    }
}