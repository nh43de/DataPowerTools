using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DataPowerTools.Extensions;
using DataPowerTools.PowerTools;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExcelDataReader.Tests
{
    [TestClass]
    public class DbBulkInsertTests
    {
        [TestMethod]
        public void TestBulkInsert()
        {
            var d = new SQLiteConnection("Data Source=:memory:");
            d.Open();

            var r = new[]
            {
                new
                {
                    Col1 = 10,
                    Col2 = 20,
                    Col3 = "abc",
                }
            }.ToDataReader();

            var s = r.GetSchemaTable();

            d.Close();
            d.Dispose();
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
}
