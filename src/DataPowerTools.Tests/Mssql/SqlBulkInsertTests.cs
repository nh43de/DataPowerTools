using System.Data;
using System.Linq;
using System.Threading.Tasks;
using DataPowerTools.DataConnectivity.Sql;
using DataPowerTools.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataPowerTools.Tests.Mssql
{
    [TestClass]
    public class SqlBulkInsertTests
    {
        private DataTable SampleData
        {
            get
            {
                var r = new[]
                {
                    new
                    {
                        Col1 = 10,
                        Col2 = 20,
                        Col3 = "abc",
                    }
                }.Repeat(1000).ToArray().ToDataTable();

                return r;
            }
        }
        
        //[TestMethod]
        public async Task Upload()
        {
            var destinationTable = "TempTable123";

            var createTableSql = SampleData.FitToCreateTableSql(destinationTable);

            /*
            $ createTableSql >

                    CREATE TABLE [TempTable123] (
	                    [Col1] INT NOT NULL,
	                    [Col2] INT NOT NULL,
	                    [Col3] VARCHAR(3) NOT NULL
                    )
             */

            var conn = TestDb.Instance.Connection;

            //create table
            conn.ExecuteSql(createTableSql);

            var reader = SampleData.ToDataReader();

            var r = reader.MapToSqlDestination(destinationTable, conn); //DataTransformGroups.Default

            await r.BulkInsertSqlServerAsync(conn, destinationTable, new AsyncSqlServerBulkInsertOptions
            {
                RowsCopiedEventHandler = i =>
                {

                }
            });

            var dd = conn.ExecuteReader("SELECT * FROM TempTable123");

            var table = dd.ToDataTable();

            var rrr = table.Rows.Count;

            Assert.AreEqual(1000, rrr);
        }
    }
}
