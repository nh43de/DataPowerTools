﻿//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.SQLite;
//using System.Linq;
//using DataPowerTools.DataReaderExtensibility.TransformingReaders;
//using DataPowerTools.Extensions;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//
//namespace ExcelDataReader.Tests
//{
//    [TestClass]
//    public class DataPowerToolsTests
//    {
//        [TestInitialize]
//        public void TestInitialize()
//        {
//
//        }
//
//        [TestMethod]
//        public void TestDiagnostics()
//        {
//            var d = new SQLiteConnection("Data Source=:memory:");
//            d.Open();
//
//            d.ExecuteSql("CREATE TABLE Test123 ( Col1 int, Col2 decimal(3,4), Col3 varchar(255) )");
//            d.ExecuteSql("INSERT INTO Test123 VALUES ( 1, 2.344, 'this is a test')");
//
//            var sr = d.CreateSqlCommand("SELECT * FROM Test123").ExecuteReader()
//                .MapToSqlDestination("Test123", d, DataTransformGroups.Default);
//
//            sr.Read();
//
//            var rr = sr.PrintDiagnostics();
//
//            Assert.AreEqual("0. [Col1] (Int32) '1' -> 0. [Col1] (Int32) '1'\r\n1. [Col2] (Decimal) '2.344' -> 1. [Col2] (Decimal) '2.344'", rr);
//
//            d.Close();
//            d.Dispose();
//        }
//        
//        [TestMethod]
//        public void TestDiagnosticsWithError()
//        {
//            var d = new SQLiteConnection("Data Source=:memory:");
//            d.Open();
//
//            d.ExecuteSql("CREATE TABLE Test123 ( Col1 int, Col2 decimal(3,4), Col3 varchar(255) )");
//            d.ExecuteSql("INSERT INTO Test123 VALUES ( 1, 2.344, 'this is a test')");
//            
//            var sr = new[]
//                {
//                    new
//                    {
//                        Col3 = 3,
//                        Col1 = "abc",
//                        Col2 = 1
//                    }
//                }
//                .ToDataReader()
//                .MapToSqlDestination("Test123", d, DataTransformGroups.Default);
//
//            sr.Read();
//
//            var rr = sr.PrintDiagnostics();
//
//            Assert.AreEqual("1. [Col1] (String) 'abc' -> 0. [Col1] (Int32) '<none>' (ERROR - Input string was not in a correct format.)\r\n2. [Col2] (Int32) '1' -> 1. [Col2] (Decimal) '1'", rr);
//
//            d.Close();
//            d.Dispose();
//        }
//
//
//
//        [TestMethod]
//        public void Test123()
//        {
//            var d = new[] {true, true, false};
//
//            var t = d.Invert();
//
//            var tt = t.Count(x => x);
//            var tf = t.Count(x => !x);
//
//            Assert.AreEqual(tt, 1);
//            Assert.AreEqual(tf, 2);
//        }
//
//
//        [TestMethod]
//        public void TestMultiSheet()
//        {
//            var d = new[]
//            {
//                new
//                {
//                    Test1 = "1",
//                    Test2 = 1
//                },
//                new
//                {
//                    Test1 = "2",
//                    Test2 = 2
//                }
//            };
//            
//            var dt1 = d.ToDataTable(false, null, true, "Sheet Test1");
//            
//            var dd = new[]
//            {
//                new TestClass
//                {
//                    Name = "test1",
//                    TestIntP = 1
//                },
//                new TestClass
//                {
//                    Name = "test2",
//                    TestIntP = 2
//                }
//            };
//
//            var dt2 = dd.ToDataTable(false, null, true, "Sheet Test2");
//
//        }
//                
//        [TestMethod]
//        public void GetFieldNamesBeforeInitializingForExplicitClass()
//        {
//            var item = new Tuple<string, IEnumerable<MonthlyAccountingBasedSummary>>("test", new[]
//            {
//                new MonthlyAccountingBasedSummary(),
//                new MonthlyAccountingBasedSummary()
//            }.ToReadOnlyCollection());
//
//            var dr = item.Item2.ToDataReader();
//
//            try
//            {
//                dr.GetFieldNames();
//            }
//            catch (Exception e)
//            {
//                Assert.Fail("Should have failed");
//            }
//        }
//        
//        [TestMethod]
//        public void GetFieldNamesBeforeInitializingForObject()
//        {
//            var item = new Tuple<string, IEnumerable<object>>("test", new[]
//                {
//                    new MonthlyAccountingBasedSummary(),
//                    new MonthlyAccountingBasedSummary()
//                }.ToReadOnlyCollection());
//
//            var dr = item.Item2.ToDataReader();
//
//            //a non initialized object may be passed in saving an empty grid to excel
//            //instead of failing this should return an empty array
//
//            Assert.IsTrue(dr.GetFieldNames().Length == 0);
//
//            //try
//            //{
//            //    dr.GetFieldNames();
//            //}
//            //catch (Exception e)
//            //{
//            //    return;
//            //}
//
//            //Assert.Fail("Should have failed");
//        }
//
//        [TestMethod]
//        public void TestEmptyClass()
//        {
//            IEnumerable<Test> test = new[]
//            {
//                new Test(), 
//            };
//
//            var dr = test.ToDataReader();
//
//            var drt = dr.ToDataTable();
//
//            Assert.AreEqual(drt.Columns.Count, 0);
//
//            Assert.AreEqual(dr.FieldCount, 0);
//        }
//
//        [TestMethod]
//        public void TestDuplicateColumnName2()
//        {
//            var excelReader = DataReaderFactories.Default("ConsumerLoans.xlsx", Helper.GetTestWorkbook("ConsumerLoans"), false);
//
//            var names = excelReader.GetFieldNames();
//        }
//
//        [TestMethod]
//        public void TestDuplicateColumnName()
//        {
//            var excelReader = DataReaderFactories.Default("ConsumerLoans.xlsx", Helper.GetTestWorkbook("ConsumerLoans"), true);
//
//            var names = excelReader.GetFieldNames();
//        }
//
//        [TestMethod]
//        public void TestWriteHeaders()
//        {
//            var headers = new[]
//            {
//                "header1",
//                "header2",
//                "header3"
//            };
//
//            headers.WriteExcelTemplate("headers.xlsx");
//
//            var r = DataReaderFactories.Default("headers.xlsx");
//
//            var dt = r.ToDataTable();
//
//            Assert.AreEqual(dt.Columns[0].ColumnName, "header1");
//            Assert.AreEqual(dt.Columns[1].ColumnName, "header2");
//            Assert.AreEqual(dt.Columns[2].ColumnName, "header3");
//        }
//
//        [TestMethod]
//        public void TestSelectNonStrict()
//        {
//            var excelReader = DataReaderFactories.Default("test.xlsx", Helper.GetTestWorkbook("TestSelectNonStrict"));
//
//            var r = excelReader.SelectNonStrict<CoFicoLtv>().ToArray().FirstOrDefault();
//
//            Assert.AreEqual(r.CoFicoLtvId, default(short));
//            Assert.AreEqual(r.ValCoFicoLtv, 1.22d);
//            Assert.AreEqual(r.CatCoFicoLtv, "BBBBB1");
//        }
//
//        [TestMethod]
//        public void TestSelectNonStrict2()
//        {
//            var excelReader = DataReaderFactories.Default("test.xlsx", Helper.GetTestWorkbook("TestSelectNonStrict"));
//
//            var r = excelReader.SelectNonStrict(typeof(CoFicoLtv)).OfType<CoFicoLtv>().ToArray().FirstOrDefault();
//
//            Assert.AreEqual(r.CoFicoLtvId, default(short));
//            Assert.AreEqual(r.ValCoFicoLtv, 1.22d);
//            Assert.AreEqual(r.CatCoFicoLtv, "BBBBB1");
//        }
//
//        [TestMethod]
//        public void TestDataReaderSelect2()
//        {
//            var dt = new DataTable();
//
//            dt.Columns.Add("CoFicoLtvId", typeof(short));
//            dt.Columns.Add("CatCoFicoLtv", typeof(string));
//            dt.Columns.Add("ValCoFicoLtv", typeof(double));
//
//            dt.Rows.Add(DBNull.Value, "BBBBB1", 1.22d);
//
//            var r = dt.ToDataReader().SelectNonStrict<CoFicoLtv>(new [] { nameof(CoFicoLtv.ValCoFicoLtv) }).ToArray().FirstOrDefault();
//
//            Assert.AreEqual(r.CoFicoLtvId, default(short));
//            Assert.AreEqual(r.ValCoFicoLtv, 1.22d);
//            Assert.AreEqual(r.CatCoFicoLtv, default(string));
//        }
//
//        [TestMethod]
//        public void TestDataReaderSelect()
//        {
//            var dt = new DataTable();
//
//            dt.Columns.Add("CoFicoLtvId", typeof(short));
//            dt.Columns.Add("CatCoFicoLtv", typeof(string));
//            dt.Columns.Add("ValCoFicoLtv", typeof(double));
//
//            dt.Rows.Add(DBNull.Value, "BBBBB1", 1.22d);
//
//            var r = dt.ToDataReader().SelectNonStrict<CoFicoLtv>().ToArray().FirstOrDefault();
//
//            Assert.AreEqual(r.CoFicoLtvId, default(short));
//            Assert.AreEqual(r.ValCoFicoLtv, 1.22d);
//            Assert.AreEqual(r.CatCoFicoLtv, "BBBBB1");
//        }
//
//        public class TestNumerics
//        {
//            public string Column1 { get; set; }
//            public decimal? ColumnDecimal { get; set; }
//            public float? ColumnFloat { get; set; }
//            public double? ColumnDouble { get; set; }
//        }
//
//        [TestMethod]
//        public void TestAnonymous()
//        {
//            var d = new[]
//            {
//                new
//                {
//                    Test1 = "1",
//                    Test2 = 1
//                },
//                new
//                {
//                    Test1 = "2",
//                    Test2 = 2
//                }
//            };
//
//            d.WriteExcel("test.xlsx");
//
//            var dr = DataReaderFactories.Default("test.xlsx");
//
//            var dt = dr.ToDataTable();
//
//            Assert.AreEqual("'Test1': '1', 'Test2': '1'\r\n'Test1': '2', 'Test2': '2'\r\n", dt.PrintData());
//        }
//
//        [TestMethod]
//        public void TestAFewThings()
//        {
//            var fields = new[]
//            {
//                nameof(TestNumerics.Column1), nameof(TestNumerics.ColumnDecimal), nameof(TestNumerics.ColumnDouble),
//                nameof(TestNumerics.ColumnFloat)
//            };
//
//            var d = new TestNumerics
//            {
//                Column1 = "Test1",
//                ColumnDecimal = (decimal) 2.03,
//                ColumnFloat = (float) 2.03,
//                ColumnDouble = (double) 2.03
//            };
//
//            IEnumerable<object> da = new[] { d };
//
//            da.ToDataTable().WriteExcel("testAfewthings.xlsx");
//
//            var results = DataReaderFactories
//                .Default("testAfewthings.xlsx")
//                .SelectNonStrict<TestNumerics>(fields)
//                .ToArray();
//
//            var r1 = results[0];
//
//            var cr = r1.Compare(r1);
//
//            Assert.IsTrue(cr.All(c => c.IsMatch));
//
//            dynamic r = results[0].ToDynamicDictionary();
//
//            Assert.AreEqual(Math.Round((decimal)r["ColumnDecimal"], 3), (decimal) 2.03);
//            Assert.IsTrue(Math.Abs(Math.Round((float)r["ColumnFloat"], 3) - (float)2.03) < 0.01);
//            Assert.AreEqual(Math.Round((double)r["ColumnDouble"], 3), (double)2.03);
//        }
//
//        [TestMethod]
//        public void TestToDataTableCovariance()
//        {
//            /*var list = new[]
//            {
//                new Tuple<string, IEnumerable<object>>("test", new[]
//                {
//                    new MonthlyAccountingBasedSummary()
//                }.ToReadOnlyCollection().Where(d => d.RecordDate.Date == new DateTime(2017, 6, 1))),
//            };
//
//            foreach (var tuple in list)
//            {
//                var dt = tuple.Item2.ToDataTable();
//            }*/
//
//            var ds = new DataSet();
//
//            var path = "temp123255.xlsx";
//            Tuple<string, IEnumerable<object>>[] items =
//            {
//                new Tuple<string, IEnumerable<object>>("test", new[]
//                {
//                    new MonthlyAccountingBasedSummary(),
//                    new MonthlyAccountingBasedSummary()
//                }.ToReadOnlyCollection())
//            };
//
//            foreach (var enumerable in items)
//            {
//                var dt = enumerable.Item2.ToDataTable();
//                dt.TableName = enumerable.Item1;
//                ds.Tables.Add(dt);
//            }
//
//            ds.WriteExcel(path);
//        }
//
//        [TestMethod]
//        public void EnumerableWriteToExcel()
//        {
//            IEnumerable<MonthlyAccountingBasedSummary> test = new[]
//            {
//                new MonthlyAccountingBasedSummary(),
//                new MonthlyAccountingBasedSummary()
//            };
//
//            test.WriteExcel("test555668.xlsx");
//        }
//
//        [TestMethod]
//        public void TestToDataTable()
//        {
//            var dd = new[]
//            {
//                new TestClass
//                {
//                    Name = "test1",
//                    TestIntP = 1
//                },
//                new TestClass
//                {
//                    Name = "test2",
//                    TestIntP = 2
//                }
//            };
//
//            var dt = dd.ToDataTable();
//
//            Assert.AreEqual(dt.Rows[0][0], "test1");
//            Assert.AreEqual(dt.Rows[0][1], 1);
//            Assert.AreEqual(dt.Rows[1][0], "test2");
//            Assert.AreEqual(dt.Rows[1][1], 2);
//        }
//
//    }
//}