using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using DataPowerTools.DataConnectivity.Sql;
using DataPowerTools.DataReaderExtensibility.TransformingReaders;
using DataPowerTools.Extensions;
using DataPowerTools.PowerTools;
using DataPowerTools.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataPowerTools.Tests
{
    [TestClass]
    public class DbBulkInsertTests
    {
        [TestMethod]
        public async Task TestInsert()
        {
            var conn = new SQLiteConnection("Data Source=:memory:");
            conn.Open();

            var r = new[]
            {
                new
                {
                    Col1 = 10,
                    Col2 = 20,
                    Col3 = "abc",
                }
            }.Repeat(1000).ToArray();

            await conn.CreateTableFor(r, "DestinationTable");

            await conn.InsertRecords(r, "DestinationTable", DatabaseEngine.Sqlite);

            conn.CloseAndDispose();
        }

        
        [TestMethod]
        public async Task TestMapToType()
        {
            var conn = new SQLiteConnection("Data Source=:memory:");
            conn.Open();

            var r = new[]
            {
                new Test123
                {
                    Col1 = 10,
                    Col2 = 20,
                    Col3 = "abc",
                }
            }.Repeat(10).ToArray();

            await conn.CreateTableFor(r, "DestinationTable");
            
            var r2 = new[]
            {
                new
                {
                    Col1 = "10",
                    Col2 = "20",
                    Col3 = "abc",
                }
            }.Repeat(100).ToArray();

            var d = r2
                .ToDataReader()
                //.MapToType(typeof(Test123), DataTransformGroups.DefaultConvert)
                .Select<Test123>()
                .ToArray();

            conn.CloseAndDispose();
        }

        [TestMethod]
        public async Task TestMapToTypeWithAlias()
        {
            var conn = new SQLiteConnection("Data Source=:memory:");
            conn.Open();

            var r = new[]
            {
                new Test123
                {
                    Col1 = 10,
                    Col2 = 20,
                    Col3 = "abc",
                }
            }.Repeat(10).ToArray();

            await conn.CreateTableFor(r, "DestinationTable");

            var r2 = new[]
            {
                new
                {
                    Col1 = "10",
                    Col2 = "20",
                    NewCol3 = "abc",
                }
            }.Repeat(100).ToArray();

            var d = r2
                .ToDataReader()
                //.MapToType(typeof(Test123), DataTransformGroups.DefaultConvert)
                .Select<Test123>()
                .ToArray();

            conn.CloseAndDispose();
        }


        [TestMethod]
        public async Task TestBulkInsertUsingCsv()
        {
            //
            var conn = new SQLiteConnection("Data Source=:memory:");
            conn.Open();

            var tab = new[]
            {
                new
                {
                    Col1 = 10,
                    Col2 = 20,
                    Col3 = "abc",
                }
            }.Repeat(100).ToArray();

            var destinationtable = "DestinationTable2";

            await conn.CreateTableFor(tab, destinationtable);
            //

            var csvData = tab.ToCsvString();

            var r = Csv.ReadString(csvData);

            await r.BulkInsert(conn, destinationtable, DatabaseEngine.Sqlite, new GenericBulkCopyOptions()
            {
                BatchSize = 1
            });

            conn.CloseAndDispose();
        }

        [TestMethod]
        public async Task TestBulkInsertUsingEnumerable()
        {
            var conn = new SQLiteConnection("Data Source=:memory:");
            conn.Open();

            var r = new[]
            {
                new
                {
                    Col1 = 10,
                    Col2 = 20,
                    Col3 = "abc",
                }
            }.Repeat(100).ToArray();

            var destinationtable = "DestinationTable2";

            await conn.CreateTableFor(r, destinationtable);

            await r.BulkInsert(conn, destinationtable, DatabaseEngine.Sqlite, new GenericBulkCopyOptions()
            {
                BatchSize = 1
            });

            conn.CloseAndDispose();
        }

        [TestMethod]
        public async Task TestBulkInsertUsingReader()
        {
            var conn = new SQLiteConnection("Data Source=:memory:");
            conn.Open();

            var r = new[]
            {
                new
                {
                    Col1 = 10,
                    Col2 = 20,
                    Col3 = "abc",
                }
            }.Repeat(100).ToArray().ToDataReader();

            var destinationtable = "DestinationTable2";

            await conn.CreateTableFor(r, destinationtable);

            await r.BulkInsert(conn, destinationtable, DatabaseEngine.Sqlite);

            conn.CloseAndDispose();
        }
    }


    ///// <summary>
    ///// We're going to take the reader and insert it at the connection.
    ///// </summary>
    ///// <param name="reader"></param>
    ///// <param name="connection"></param>
    //public static void TestT(IDataReader reader, DbConnection connection)
    //{
    //    var providerFactory = connection.GetDbProviderFactory();

    //    var commandBuilder = providerFactory.CreateCommandBuilder();

    //    commandBuilder.DataAdapter
    //}

}
