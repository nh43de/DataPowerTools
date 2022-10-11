using System.Data;
using System.Linq;
using System.Threading.Tasks;
using DataPowerTools.DataConnectivity.Sql;
using DataPowerTools.Extensions;
using DataPowerTools.PowerTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataPowerTools.Tests.Mssql
{
    [TestClass]
    public class SqlSchemaTests
    {

        //TODO: broken
        //[TestMethod]
        public void TestGetSchema()
        {
            TestDb.Instance.Connection.ExecuteSql(@"CREATE TABLE [dbo].[Rates](
	[RateId] [INT] IDENTITY(1,1) NOT NULL,
	[ClientId] [UNIQUEIDENTIFIER] NULL,
	[Coupon] [MONEY] NOT NULL,
	[D100] [MONEY] NULL,
	[D200] [MONEY] NULL,
	[D300] [MONEY] NULL,
	[D400] [MONEY] NULL,
	[Group] [NVARCHAR](16) NULL,
	[LegacyClientId] [INT] NULL,
	[Price] [MONEY] NULL,
	[Type] [NVARCHAR](255) NULL,
	[U100] [MONEY] NULL,
	[U200] [MONEY] NULL,
	[U300] [MONEY] NULL,
	[U400] [MONEY] NULL,
 CONSTRAINT [PK_Rates] PRIMARY KEY CLUSTERED 
(
	[RateId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];");

            var d = new MsSqlDatabaseConnection(TestDb.Instance.Connection);

            var s = d.GetTableSchema("Rates");


            var ss = Database.GetTableColumns("Rates", TestDb.Instance.Connection);



        }
    }
}
