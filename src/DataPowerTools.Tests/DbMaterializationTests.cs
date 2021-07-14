using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using DataPowerTools.DataReaderExtensibility.TransformingReaders;
using DataPowerTools.Extensions;
using DataPowerTools.PowerTools;
using DataPowerTools.Tests.Models;
using DataPowerTools.Tests.ReaderTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataPowerTools.Tests
{
    [TestClass]
    public class DbMaterializationTests
    {
        [TestMethod]
        public async Task TestInsertBulkWithNavigationProperty()
        {
            var conn = new SQLiteConnection("Data Source=:memory:");
            conn.Open();

            var r = new[]
            {
                new Test123WithId
                {
                    Test123Id = 1,
                    Col1 = 10,
                    Col2 = 20.1m,
                    Col3 = "abc"
                }
            };

            await conn.CreateTableFor(r.ToDataReader(), "DestinationTable");

            var rr = r
                .ToDataReader()
                .SelectRows<Test123WithRef>(DataTransformGroups.Default)
                .ToArray();


            //await r
            //    .ToDataReader()
            //    .Select<Test123WithRef>(DataTransformGroups.Default)
            //    .BulkInsert(
            //        conn,
            //        "DestinationTable",
            //        DatabaseEngine.Sqlite,
            //        new GenericBulkCopyOptions
            //        {
            //            BatchSize = 1
            //        });

            conn.CloseAndDispose();
        }


        [TestMethod]
        public async Task TestExecuteReader()
        {
            var d = new SQLiteConnection("Data Source=:memory:");
            d.Open();

            d.ExecuteSql("CREATE TABLE Test123 ( Col1 int, Col2 decimal(3,4), Col3 varchar(255) )");

            var dr = TestDataHelpers.GetSampleDataReader(DataReaderSource.ObjectReader, 3);

            await dr.BulkInsert(d, "Test123", DatabaseEngine.Sqlite);

            var dr2 = d.ExecuteReader("SELECT * FROM Test123");



            var rr = dr2.SelectStrict<Test123>().ToArray();

            Assert.AreEqual(3, rr.Length);

            d.Close();
            d.Dispose();
        }
        
        [TestMethod]
        public void TestExecuteObject()
        {
            var d = new SQLiteConnection("Data Source=:memory:");
            d.Open();

            d.ExecuteSql("CREATE TABLE Test123 ( Col1 int, Col2 decimal(3,4), Col3 varchar(255) )");

            d.ExecuteSql("INSERT INTO Test123 VALUES ( 1, 2.344, 'this is a test')");

            var rr = d.ExecuteObject<Test123>("SELECT * FROM Test123");

            object o;
            

            Assert.AreEqual(rr.Col1, 1);
            Assert.AreEqual((double)rr.Col2, (double)2.344);
            Assert.AreEqual(rr.Col3, "this is a test");
            
            d.Close();
            d.Dispose();
        }
        
        [TestMethod]
        public void TestExecuteScalar()
        {
            var d = new SQLiteConnection("Data Source=:memory:");
            d.Open();

            d.ExecuteSql("CREATE TABLE Test123 ( Col1 int, Col2 decimal(3,4), Col3 varchar(255) )");

            d.ExecuteSql("INSERT INTO Test123 VALUES ( 1, 2.344, 'this is a test')");

            var rr = d.ExecuteScalar<int>("SELECT Col1 FROM Test123");

            Assert.AreEqual(rr, 1);


            d.Close();
            d.Dispose();
        }
        
        [TestMethod]
        public void TestExecuteList()
        {
            var d = new SQLiteConnection("Data Source=:memory:");
            d.Open();

            d.ExecuteSql("CREATE TABLE Test123 ( Col1 int, Col2 decimal(3,4), Col3 varchar(255) )");

            d.ExecuteSql("INSERT INTO Test123 VALUES ( 1, 2.344, 'this is a test')");
            d.ExecuteSql("INSERT INTO Test123 VALUES ( 1, 2.344, 'this is a test')");
            d.ExecuteSql("INSERT INTO Test123 VALUES ( 1, 2.344, 'this is a test')");
            d.ExecuteSql("INSERT INTO Test123 VALUES ( 1, 2.344, 'this is a test')");
            d.ExecuteSql("INSERT INTO Test123 VALUES ( 1, 2.344, 'this is a test')");

            List<Test123> r;
            using (var cmd = d.CreateSqlCommand("SELECT * FROM Test123"))
            {
                 r = cmd.ExecuteToEnumerable<Test123>().ToList();
            }
            
            Assert.AreEqual(r.Count, 5);

            d.Close();
            d.Dispose();
        }

    }
}
