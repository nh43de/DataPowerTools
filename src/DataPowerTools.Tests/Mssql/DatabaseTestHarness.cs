using System;
using System.Data.Common;
using System.Data.SqlClient;
using DataPowerTools.Extensions;

namespace ExcelDataReader.Tests
{
    public class DatabaseTestHarness : IDisposable
    {
        //
        public SqlConnection Connection { get; private set; }

        //
        private const string ServerName = "localhost";
        private const string DbName = "__DPT_TESTS";

        private static readonly SqlConnectionStringBuilder Builder = new SqlConnectionStringBuilder()
        {
            DataSource = ServerName,
            InitialCatalog = "master",
            IntegratedSecurity = true
        };

        public DatabaseTestHarness()
        {
            Connection = new SqlConnection(Builder.ConnectionString);
        }

        
        public void Initialize()
        {
            try
            {
                Connection.Open();
                Destruct();
                Connection.ExecuteSql($@"CREATE DATABASE [{DbName}];");
                Connection.ExecuteSql($@"USE [{DbName}];");
            }
            catch (Exception e)
            {
                throw new Exception("Unable to create test db.", e);
            }
        }

        /// <summary>
        /// Destroys testing instance
        /// </summary>
        private void Destruct()
        {
            // drop old db
            var sql = $@"
IF (EXISTS (
    SELECT *
    FROM [sys].[databases] AS [d]
    WHERE [d].[name] = N'__DPT_TESTS'
)
)
BEGIN
    USE [master];
    EXEC [msdb].[dbo].[sp_delete_database_backuphistory] @database_name = N'__DPT_TESTS';
    USE [master];
    ALTER DATABASE [__DPT_TESTS] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    USE [master];
    DROP DATABASE IF EXISTS [__DPT_TESTS];
END;
";

            try
            {
                Connection.ExecuteSql(sql);
            }
            catch (Exception e)
            {
                throw new Exception("Unable to cleanup test db.", e);
            }

        }

        public void Dispose()
        {
            Destruct();
            Connection?.Dispose();
        }
    }
}