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

        var r = dd.AsInsertStatements("MyTable", DatabaseEngine.SqlServer);

    }



}