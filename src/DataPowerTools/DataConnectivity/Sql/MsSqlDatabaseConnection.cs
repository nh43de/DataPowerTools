using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace DataPowerTools.DataConnectivity.Sql
{
    //TODO: LOTS OF DUPLICATED CODE

    /// <summary>
    /// Do not use.
    /// </summary>
    [Obsolete]
    public class MsSqlDatabaseConnection : IDataConnection, IDisposable
    {
        public MsSqlDatabaseConnection(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public SqlConnection Connection { get; set; }

        public string ConnectionString { get; }

        public bool IsConnected
            =>
            (Connection != null) && (Connection.State != ConnectionState.Closed) &&
            (Connection.State != ConnectionState.Broken);


        public void Dispose()
        {
            if (IsConnected) return;
            Disconnect();

            Connection.Dispose();
            Connection = null;
        }

        public void Connect()
        {
            if (IsConnected)
                Disconnect();

            Connection = new SqlConnection(ConnectionString);

            Connection.Open();
        }

        public void Disconnect()
        {
            Connection.Close();
        }

        public DataSet ExecuteDataSet(string sql)
        {
            try
            {
                var sqlCommand = new SqlCommand(sql, Connection);
                var dataAdapter = new SqlDataAdapter(sqlCommand);

                var dataSet = new DataSet();
                dataAdapter.Fill(dataSet);

                return dataSet;
            }
            catch (Exception exception)
            {
                throw new Exception("Error executing to dataset: " + exception.Message);
            }
        }

        public IDataReader GetReader(string sql)
        {
            try
            {
                var sqlCommand = new SqlCommand(sql, Connection);

                return sqlCommand.ExecuteReader();
            }
            catch (Exception exception)
            {
                throw new Exception("Error executing to reader: " + exception.Message);
            }
        }


        //TODO: need this for execute scalar

        //TODO: this doesn't close the connection

        public int ExecuteSql(string sql)
        {
            try
            {
                var sqlCommand = new SqlCommand(sql, Connection);

                return sqlCommand.ExecuteNonQuery();
            }
            catch (Exception exception)
            {
                throw new Exception("Error executing sql: " + exception.Message);
            }
        }

        public DataTable GetTableSchema(string tableName)
        {
            var noncomputedColumns = GetNonComputedColumns(tableName);

            var cols = "[" + string.Join("],[", noncomputedColumns) + "]";

            try
            {
                var sc = new SqlCommand(@"SELECT TOP 0 " + cols + " from " + tableName, Connection);
                var sa = new SqlDataAdapter(sc);
                var dt = new DataTable();

                sa.FillSchema(dt, SchemaType.Source);

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Could not get data schema for table [{tableName}]. Column names: '{cols}': {ex.Message}");
            }
        }

        public TResult ExecuteScalar<TResult>(string sql)
        {
            try
            {
                var sqlCommand = new SqlCommand(sql, Connection);

                return (TResult) sqlCommand.ExecuteScalar();
            }
            catch (Exception exception)
            {
                throw new Exception("Error executing sql: " + exception.Message);
            }
        }


        public Task BulkInsertDataTable(string destinationTable, DataTable data, bool useOrdinals = false)
        {
            return BulkInsertDataTableAsync(destinationTable, data, useOrdinals);
        }

        public async Task BulkInsertDataTableAsync(string destinationTable, DataTable data, bool useOrdinals = false)
        {
            var bulkCopy = new SqlBulkCopy(Connection,
                    SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.UseInternalTransaction, null)
                {BulkCopyTimeout = 0};

            if (!useOrdinals)
            {
                //try using explicit naming
                foreach (DataColumn colItem in data.Columns)
                    bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(colItem.ColumnName, colItem.ColumnName));
            }
            else
            {
                //clear and try using ordinals
                bulkCopy.ColumnMappings.Clear();

                var i = 0;
                foreach (DataColumn colItem in data.Columns)
                {
                    bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(i, i));
                    i++;
                }
            }

            bulkCopy.DestinationTableName = destinationTable;

            //bulkCopy.WriteToServer(data);
            await bulkCopy.WriteToServerAsync(data);
        }


        public IEnumerable<string> GetNonComputedColumns(string tableName)
        {
            var dt = new DataTable();

            if (tableName.StartsWith("[") && tableName.EndsWith("]"))
                tableName = tableName.Substring(1, tableName.Length - 2);

            try
            {
                var sc = new SqlCommand(@"select name, column_id from sys.all_columns a
                                                    where a.object_id = OBJECT_ID(@table_name)
                                                    except
                                                    select name, column_id from sys.computed_columns a
                                                    where a.object_id = OBJECT_ID(@table_name)
                                                    order by column_id", Connection);

                sc.Parameters.Add(new SqlParameter("@table_name", tableName));

                using (var sa = new SqlDataAdapter(sc))
                {
                    sa.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Could not get list of computed columns for table [{tableName}]: {ex.Message}");
            }

            return
                dt.Rows.Cast<DataRow>()
                    .Select(row => row["name"].ToString())
                    .Where(colname => !string.IsNullOrWhiteSpace(colname));
        }
    }
}