using System;
using System.Data;
using System.Threading.Tasks;

namespace DataPowerTools.DataConnectivity
{
    [Obsolete]
    public interface IDataConnection
    {
        bool IsConnected { get; }
        void Dispose();
        void Connect();
        void Disconnect();

        IDataReader GetReader(string sql); //TODO: remove param for SQL

        DataSet ExecuteDataSet(string sql); //TODO: remove - no need for this. Can just call GetReader().ToDataSet()


        TResult ExecuteScalar<TResult>(string sql);
        int ExecuteSql(string sql);


        DataTable GetTableSchema(string tableName); //TODO: standardize this

        Task BulkInsertDataTable(string destinationTable, DataTable data, bool useOrdinals = false);
        Task BulkInsertDataTableAsync(string destinationTable, DataTable data, bool useOrdinals = false);
    }
}