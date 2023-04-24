using DataPowerTools.Extensions;
using DataPowerTools.PowerTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataPowerTools.Tests;

[TestClass]
public class InsertSqlBuilderTests
{
    [TestMethod]
    public void TestCreateInserts()
    {
        var csv = @"Col1	Col2	Col 3
AAA	AA'C	''
AAB	AA""C	''
AAC	AB""D""'	''
";


        var dd = csv.ReadCsvString('\t', true);

        var r = dd.AsSqlInsertStatements("MyTable", DatabaseEngine.SqlServer);

        Assert.AreEqual(@"INSERT INTO MyTable ([Col1],[Col2],[Col 3]) SELECT 'AAA' as [Col1],'AA''C' as [Col2],'''''' as [Col 3];
INSERT INTO MyTable ([Col1],[Col2],[Col 3]) SELECT 'AAB' as [Col1],'AA""C' as [Col2],'''''' as [Col 3];
INSERT INTO MyTable ([Col1],[Col2],[Col 3]) SELECT 'AAC' as [Col1],'AB""D""''' as [Col2],'''''' as [Col 3];
", r);

    }


    [TestMethod]
    public void TestCreateSelects()
    {
        var csv = @"Col1	Col2	Col 3
AAA	AA'C	''
AAB	AA""C	''
AAC	AB""D""'	''
";
        
        var dd = csv.ReadCsvString('\t', true);
        var r = dd.AsSqlSelectStatements( DatabaseEngine.SqlServer, "UNION ALL", true);

        Assert.AreEqual(@"SELECT 'AAA' as [Col1], 'AA''C' as [Col2], ''''''  as [Col3]
UNION ALL
SELECT 'AAB', 'AA""C', ''''''
UNION ALL
SELECT 'AAC', 'AB""D""''', ''''''
", r);

        var dd2 = csv.ReadCsvString('\t', true);
        var r2 = dd2.AsSqlSelectStatements(DatabaseEngine.SqlServer, "UNION ALL", false);

        Assert.AreEqual(@"SELECT 'AAA' as [Col1], 'AA''C' as [Col2], '''''' as [Col 3]
UNION ALL
SELECT 'AAB' as [Col1], 'AA""C' as [Col2], '''''' as [Col 3]
UNION ALL
SELECT 'AAC' as [Col1], 'AB""D""''' as [Col2], '''''' as [Col 3]
", r2);

    }
}