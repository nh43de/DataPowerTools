namespace DataPowerTools.DataConnectivity.Sql
{
    public class GenericBulkCopyOptions
    {
        public RowsCopiedEventHandler RowsCopiedEventHandler { get; set; }
        /// <summary>
        /// Batch data is chunked into when inserting. The default size is 5,000 records per batch.
        /// </summary>
        public int BatchSize { get; set; } = 1; //SQLite in-memory works best with this batch size. 
        public bool UseOrdinals { get; set; } = false;

        /// <summary>
        /// The time in seconds to wait for a batch to load. The default is 30 seconds.
        /// </summary>
        public int BulkCopyTimeout { get; set; } = 0;
    }
}