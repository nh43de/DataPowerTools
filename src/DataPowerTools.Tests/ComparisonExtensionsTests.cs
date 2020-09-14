using System.Linq;
using DataPowerTools.Extensions;
using DataPowerTools.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataPowerTools.Tests
{
    [TestClass]
    public class ComparisonExtensionsTests
    {
        [TestMethod]
        public void TestCompareWithAnonymousMethod3()
        {
            var d = new MonthlyAccountingBasedSummary
            {
                ClientId = "TestClient",
                FundingExpense = 100
            };

            var d2 = new MonthlyAccountingBasedSummary
            {
                ClientId = "TestClient2"
            };

            var failResults = d.Compare(d2,
                    new[]
                    {
                        nameof(MonthlyAccountingBasedSummary.ClientId),
                        nameof(MonthlyAccountingBasedSummary.TotalRealizedIncomeBasis)
                    })
                .Where(r => r.IsMatch == false).ToArray();
            
            Assert.AreEqual(failResults.Length, 1);
            Assert.AreEqual(failResults[0].FieldName, "ClientId");
            Assert.AreEqual(failResults[0].ToValue, "TestClient2");
        }

        [TestMethod]
        public void TestCompareWithAnonymousMethod2()
        {
            var d = new MonthlyAccountingBasedSummary
            {
                ClientId = "TestClient"
            };

            var d2 = new MonthlyAccountingBasedSummary
            {
                ClientId = "TestClient2"
            };

            var failResults = d.Compare(d2,
                    new[]
                    {
                        nameof(MonthlyAccountingBasedSummary.InterestIncome),
                        nameof(MonthlyAccountingBasedSummary.TotalRealizedIncomeBasis)
                    })
                .Where(r => r.IsMatch == false).ToArray();

            Assert.AreEqual(failResults.Length, 0);
        }

        [TestMethod]
        public void TestCompareWithAnonymousMethod()
        {
            var d = new MonthlyAccountingBasedSummary
            {
                ClientId = "TestClient"
            };

            var d2 = new MonthlyAccountingBasedSummary
            {
                ClientId = "TestClient2"
            };

            var failResults = d.Compare(d2).Where(r => r.IsMatch == false).ToArray();

            Assert.AreEqual(failResults.Length, 1);
            Assert.AreEqual(failResults[0].FieldName, "ClientId");
            Assert.AreEqual(failResults[0].ToValue, "TestClient2");
        }

        [TestMethod]
        public void TestCompareToWithAnonymousMethod5()
        {
            var d = new MonthlyAccountingBasedSummary
            {
                ClientId = "TestClient"
            };

            var d2 = new MonthlyAccountingBasedSummary
            {
                ClientId = "TestClient2"
            };

            var compareSucceeded = d.CompareTo(d2,
                new[]
                {
                    nameof(MonthlyAccountingBasedSummary.HedgeInstrumentGainLoss),
                    nameof(MonthlyAccountingBasedSummary.TotalIncome)
                });

            Assert.AreEqual(compareSucceeded, true);
        }

        [TestMethod]
        public void TestCompareToWithAnonymousMethod4()
        {
            var d = new MonthlyAccountingBasedSummary
            {
                ClientId = "TestClient"
            };

            var d2 = new MonthlyAccountingBasedSummary
            {
                ClientId = "TestClient2"
            };

            Assert.AreEqual(d.CompareTo(d2), false);
        }
        
        [TestMethod]
        public void TestCompareToWithAnonymousMethod3()
        {
            var d = new MonthlyAccountingBasedSummary
            {
                ClientId = "TestClient"
            };

            var d2 = new MonthlyAccountingBasedSummary
            {
                ClientId = "TestClient"
            };

            Assert.AreEqual(d.CompareTo(d2), true);
        }
        
        [TestMethod]
        public void TestCompareToWithAnonymousMethod2()
        {
            var d = new { Test1 = "s", Test2 = 1, Test3 = (int?)null }
                .CompareTo(new
                {
                    Test1 = "test",
                    Test2 = 1,
                    Test3 = (int?)null
                });

            Assert.AreEqual(d, false);
        }

        [TestMethod]
        public void TestCompareToWithAnonymousMethod()
        {
            var d = new {Test1 = "s", Test2 = 1, Test3 = (int?) null}
                .CompareTo(new
                {
                    Test1 = "s",
                    Test2 = 1,
                    Test3 = (int?) null
                });

            Assert.AreEqual(d, true);
        }
    }
}