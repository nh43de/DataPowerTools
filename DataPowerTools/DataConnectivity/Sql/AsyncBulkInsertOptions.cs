using System.Threading;

namespace DataPowerTools.DataConnectivity.Sql
{
    public class AsyncBulkInsertOptions : BulkInsertOptions
    {
        public CancellationToken CancellationToken { get; set; }
    }
}