using System.Data.Common;
using DataPowerTools.PowerTools;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataPowerTools.Tests
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
            dbCommand = new InsertCommandSqlBuilder(DatabaseEngine.SqlServer).AppendInsert(dbCommand, customer, "INSERT INTO {0} ({1}) VALUES({2});");
            
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
            dbCommand = new InsertCommandSqlBuilder(DatabaseEngine.SqlServer).AppendInsert(dbCommand, customer, "INSERT INTO {0} ({1}) VALUES({2});");

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
