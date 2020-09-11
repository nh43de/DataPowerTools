using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using DataPowerTools.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExcelDataReader.Tests
{
    [TestClass]
    public class DbMaterializationTests
    {
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
