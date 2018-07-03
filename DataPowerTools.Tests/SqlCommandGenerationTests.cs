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
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExcelDataReader.Tests
{
    [TestClass]
    public class SqlCommandGenerationTests
    {
        public class ClassWithTheLastFieldNullable
        {
            public int Ant;
            public string Baboon;
            public int? Cat;
        }

        [TestMethod]
        public void Should_Not_Produce_A_Trailing_Comma_After_The_Last_Parameter_In_The_Parameter_List()
        {
            // Arrange
            var customer = new ClassWithTheLastFieldNullable { Ant = 1, Baboon = "broom" };

            DbCommand dbCommand = new SqlCommand();

            // Act
            dbCommand = dbCommand.GenerateInsertCommand(customer, "INSERT INTO {0} ({1}) VALUES({2});");
            
            // Assert
            Assert.IsNotNull(dbCommand.CommandText);
            Assert.IsFalse(dbCommand.CommandText.Contains(",) VALUES"));
        }

        [TestMethod]
        public void Should_Not_Produce_A_Trailing_Comma_After_The_Last_Parameter_In_The_Values_List()
        {
            // Arrange
            var customer = new ClassWithTheLastFieldNullable { Ant = 1, Baboon = "broom" };

            DbCommand dbCommand = new SqlCommand();

            // Act
            dbCommand = dbCommand.GenerateInsertCommand(customer, "INSERT INTO {0} ({1}) VALUES({2});");

            // Assert
            Assert.IsNotNull(dbCommand.CommandText);
            Assert.IsFalse(dbCommand.CommandText.Contains(",);"));
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
