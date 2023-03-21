using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.IO;
using Microsoft.Data.SqlClient.Server;

namespace DataPowerTools.Tests;

    //this is a test of using TVPs for maximum upload performance on bulk insert

    //https://stackoverflow.com/questions/25770180/how-can-i-insert-10-million-records-in-the-shortest-time-possible
    //https://www.dbdelta.com/maximizing-performance-with-table-valued-parameters/

    //[TestClass]
    //public class InsertWithStoredProcTests
    //{
    //    [TestMethod]
    //    public void TestInsertWithStoredProc()
    //    {
    //        SqlConnection _Connection = new SqlConnection("{connection string}");
    //        SqlCommand _Command = new SqlCommand("ImportData", _Connection);
    //        _Command.CommandType = CommandType.StoredProcedure;

    //        SqlParameter _TVParam = new SqlParameter();
    //        _TVParam.ParameterName = "@ImportTable";
    //        _TVParam.TypeName = "dbo.ImportStructure";
    //        _TVParam.SqlDbType = SqlDbType.Structured;
    //        _TVParam.Value = GetFileContents(); // return value of the method is streamed data
    //        _Command.Parameters.Add(_TVParam);

    //        try
    //        {
    //            _Connection.Open();

    //            _Command.ExecuteNonQuery();
    //        }
    //        finally
    //        {
    //            _Connection.Close();
    //        }

    //        return;


    //    }
        
    //    private static IEnumerable<SqlDataRecord> GetFileContents()
    //    {
    //        SqlMetaData[] _TvpSchema = new SqlMetaData[] {
    //              new SqlMetaData("Field", SqlDbType.VarChar, SqlMetaData.Max)
    //           };

    //        SqlDataRecord _DataRecord = new SqlDataRecord(_TvpSchema);
    //        StreamReader _FileReader = null;

    //        try
    //        {
    //            _FileReader = new StreamReader("{filePath}");

    //            // read a row, send a row
    //            while (!_FileReader.EndOfStream)
    //            {
    //                // You shouldn't need to call "_DataRecord = new SqlDataRecord" as
    //                // SQL Server already received the row when "yield return" was called.
    //                // Unlike BCP and BULK INSERT, you have the option here to create a string
    //                // call ReadLine() into the string, do manipulation(s) / validation(s) on
    //                // the string, then pass that string into SetString() or discard if invalid.
    //                _DataRecord.SetString(0, _FileReader.ReadLine());
    //                yield return _DataRecord;
    //            }
    //        }
    //        finally
    //        {
    //            _FileReader.Close();
    //        }
    //    }



        /*
         *
         



-- First: You need a User-Defined Table Type
CREATE TYPE ImportStructure AS TABLE (Field VARCHAR(MAX));
GO

-- Second: Use the UDTT as an input param to an import proc.
--         Hence "Tabled-Valued Parameter" (TVP)
CREATE PROCEDURE dbo.ImportData (
   @ImportTable    dbo.ImportStructure READONLY
)
AS
SET NOCOUNT ON;

-- maybe clear out the table first?
TRUNCATE TABLE dbo.DATAs;

INSERT INTO dbo.DATAs (DatasField)
    SELECT  Field
    FROM    @ImportTable;

GO
         
         *
         */


