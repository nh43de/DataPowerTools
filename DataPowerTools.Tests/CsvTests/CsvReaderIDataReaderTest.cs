//	LumenWorks.Framework.Tests.Unit.IO.CSV.CsvReaderIDataReaderTest
//	Copyright (c) 2005 Sébastien Lorion
//
//	MIT license (http://en.wikipedia.org/wiki/MIT_License)
//
//	Permission is hereby granted, free of charge, to any person obtaining a copy
//	of this software and associated documentation files (the "Software"), to deal
//	in the Software without restriction, including without limitation the rights 
//	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//	of the Software, and to permit persons to whom the Software is furnished to do so, 
//	subject to the following conditions:
//
//	The above copyright notice and this permission notice shall be included in all 
//	copies or substantial portions of the Software.http://scottchacon.com/2011/08/31/github-flow.html
//
//	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
//	INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
//	PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
//	FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
//	ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Data;
using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvDataReader.Tests
{
	[TestClass]
	public class CsvReaderIDataReaderTest
	{
        #region IDataReader interface

        [TestMethod]
        public void CloseTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;

				csv.ReadNextRecord();

				reader.Close();

				Assert.IsTrue(reader.IsClosed);
				Assert.IsTrue(csv.IsDisposed);
			}
		}
        

		[TestMethod]
		public void GetSchemaTableWithHeadersTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;

				DataTable schema = reader.GetSchemaTable();

				Assert.AreEqual(CsvReaderSampleData.SampleData1RecordCount, schema.Rows.Count);

				foreach (DataColumn column in schema.Columns)
				{
					Assert.IsTrue(column.ReadOnly);
				}

				for (int index = 0; index < schema.Rows.Count; index++)
				{
					DataRow column = schema.Rows[index];

					Assert.AreEqual(int.MaxValue, column["ColumnSize"]);
					Assert.AreEqual(DBNull.Value, column["NumericPrecision"]);
					Assert.AreEqual(DBNull.Value, column["NumericScale"]);
					Assert.AreEqual(false, column["IsUnique"]);
					Assert.AreEqual(false, column["IsKey"]);
					Assert.AreEqual(string.Empty, column["BaseServerName"]);
					Assert.AreEqual(string.Empty, column["BaseCatalogName"]);
					Assert.AreEqual(string.Empty, column["BaseSchemaName"]);
					Assert.AreEqual(string.Empty, column["BaseTableName"]);
					Assert.AreEqual(typeof(string), column["DataType"]);
					Assert.AreEqual(true, column["AllowDBNull"]);
					Assert.AreEqual((int) DbType.String, column["ProviderType"]);
					Assert.AreEqual(false, column["IsAliased"]);
					Assert.AreEqual(false, column["IsExpression"]);
					Assert.AreEqual(false, column["IsAutoIncrement"]);
					Assert.AreEqual(false, column["IsRowVersion"]);
					Assert.AreEqual(false, column["IsHidden"]);
					Assert.AreEqual(false, column["IsLong"]);
					Assert.AreEqual(true, column["IsReadOnly"]);

					Assert.AreEqual(index, column["ColumnOrdinal"]);

					switch (index)
					{
						case 0:
							Assert.AreEqual(CsvReaderSampleData.SampleData1Header0, column["ColumnName"]);
							Assert.AreEqual(CsvReaderSampleData.SampleData1Header0, column["BaseColumnName"]);
							break;
						case 1:
							Assert.AreEqual(CsvReaderSampleData.SampleData1Header1, column["ColumnName"]);
							Assert.AreEqual(CsvReaderSampleData.SampleData1Header1, column["BaseColumnName"]);
							break;
						case 2:
							Assert.AreEqual(CsvReaderSampleData.SampleData1Header2, column["ColumnName"]);
							Assert.AreEqual(CsvReaderSampleData.SampleData1Header2, column["BaseColumnName"]);
							break;
						case 3:
							Assert.AreEqual(CsvReaderSampleData.SampleData1Header3, column["ColumnName"]);
							Assert.AreEqual(CsvReaderSampleData.SampleData1Header3, column["BaseColumnName"]);
							break;
						case 4:
							Assert.AreEqual(CsvReaderSampleData.SampleData1Header4, column["ColumnName"]);
							Assert.AreEqual(CsvReaderSampleData.SampleData1Header4, column["BaseColumnName"]);
							break;
						case 5:
							Assert.AreEqual(CsvReaderSampleData.SampleData1Header5, column["ColumnName"]);
							Assert.AreEqual(CsvReaderSampleData.SampleData1Header5, column["BaseColumnName"]);
							break;
						default:
							throw new IndexOutOfRangeException();
					}
				}
			}
		}

		[TestMethod]
		public void GetSchemaTableWithoutHeadersTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), false))
			{
				IDataReader reader = csv;

				DataTable schema = reader.GetSchemaTable();

				Assert.AreEqual(CsvReaderSampleData.SampleData1RecordCount, schema.Rows.Count);

				foreach (DataColumn column in schema.Columns)
				{
					Assert.IsTrue(column.ReadOnly);
				}

				for (int index = 0; index < schema.Rows.Count; index++)
				{
					DataRow column = schema.Rows[index];

					Assert.AreEqual(int.MaxValue, column["ColumnSize"]);
					Assert.AreEqual(DBNull.Value, column["NumericPrecision"]);
					Assert.AreEqual(DBNull.Value, column["NumericScale"]);
					Assert.AreEqual(false, column["IsUnique"]);
					Assert.AreEqual(false, column["IsKey"]);
					Assert.AreEqual(string.Empty, column["BaseServerName"]);
					Assert.AreEqual(string.Empty, column["BaseCatalogName"]);
					Assert.AreEqual(string.Empty, column["BaseSchemaName"]);
					Assert.AreEqual(string.Empty, column["BaseTableName"]);
					Assert.AreEqual(typeof(string), column["DataType"]);
					Assert.AreEqual(true, column["AllowDBNull"]);
					Assert.AreEqual((int) DbType.String, column["ProviderType"]);
					Assert.AreEqual(false, column["IsAliased"]);
					Assert.AreEqual(false, column["IsExpression"]);
					Assert.AreEqual(false, column["IsAutoIncrement"]);
					Assert.AreEqual(false, column["IsRowVersion"]);
					Assert.AreEqual(false, column["IsHidden"]);
					Assert.AreEqual(false, column["IsLong"]);
					Assert.AreEqual(true, column["IsReadOnly"]);

					Assert.AreEqual(index, column["ColumnOrdinal"]);

					Assert.AreEqual("Column" + index.ToString(CultureInfo.InvariantCulture), column["ColumnName"]);
					Assert.AreEqual("Column" + index.ToString(CultureInfo.InvariantCulture), column["BaseColumnName"]);
				}
			}
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void GetSchemaTableReaderClosedTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;
				csv.ReadNextRecord();
				reader.Close();

				DataTable result = reader.GetSchemaTable();
			}
		}

		[TestMethod]
		public void NextResultTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;
				Assert.IsFalse(reader.NextResult());

				csv.ReadNextRecord();
				Assert.IsFalse(reader.NextResult());
			}
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void NextResultReaderClosedTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;
				csv.ReadNextRecord();
				reader.Close();

				bool result = reader.NextResult();
			}
		}

		[TestMethod]
		public void ReadTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;

				for (int i = 0; i < CsvReaderSampleData.SampleData1RecordCount; i++)
					Assert.IsTrue(reader.Read());

				Assert.IsFalse(reader.Read());
			}
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void ReadReaderClosedTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;
				csv.ReadNextRecord();
				reader.Close();

				bool result = reader.Read();
			}
		}

		[TestMethod]
		public void DepthTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;
				Assert.AreEqual(0, reader.Depth);

				csv.ReadNextRecord();
				Assert.AreEqual(1, reader.Depth);
			}
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void DepthReaderClosedTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;
				csv.ReadNextRecord();
				reader.Close();

				int result = reader.Depth;
			}
		}

		[TestMethod]
		public void IsClosedTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;
				Assert.IsFalse(reader.IsClosed);

				csv.ReadNextRecord();
				Assert.IsFalse(reader.IsClosed);

				reader.Close();
				Assert.IsTrue(reader.IsClosed);
			}
		}

		[TestMethod]
		public void RecordsAffectedTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;
				Assert.AreEqual(-1, reader.RecordsAffected);

				csv.ReadNextRecord();
				Assert.AreEqual(-1, reader.RecordsAffected);

				reader.Close();
				Assert.AreEqual(-1, reader.RecordsAffected);
			}
		}

		#endregion

		#region IDataRecord interface

		[TestMethod]
		public void GetBooleanTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				Boolean value = true;
				while (reader.Read())
				{
					Assert.AreEqual(value, reader.GetBoolean(reader.GetOrdinal(typeof(Boolean).FullName)));
				}
			}
		}

		[TestMethod]
		public void GetByteTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				Byte value = 1;
				while (reader.Read())
				{
					Assert.AreEqual(value, reader.GetByte(reader.GetOrdinal(typeof(Byte).FullName)));
				}
			}
		}

		[TestMethod]
		public void GetBytesTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				Char[] temp = "abc".ToCharArray();
				Byte[] value = new Byte[temp.Length];

				for (int i = 0; i < temp.Length; i++)
					value[i] = Convert.ToByte(temp[i]);

				while (reader.Read())
				{
					Byte[] csvValue = new Byte[value.Length];

					long count = reader.GetBytes(reader.GetOrdinal(typeof(String).FullName), 0, csvValue, 0, value.Length);

					Assert.AreEqual(value.Length, count);
					Assert.AreEqual(value.Length, csvValue.Length);

					for (int i = 0; i < value.Length; i++)
						Assert.AreEqual(value[i], csvValue[i]);
				}
			}
		}

		[TestMethod]
		public void GetCharTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				Char value = 'a';
				while (reader.Read())
				{
					Assert.AreEqual(value, reader.GetChar(reader.GetOrdinal(typeof(Char).FullName)));
				}
			}
		}

		[TestMethod]
		public void GetCharsTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				Char[] value = "abc".ToCharArray();
				while (reader.Read())
				{
					Char[] csvValue = new Char[value.Length];

					long count = reader.GetChars(reader.GetOrdinal(typeof(String).FullName), 0, csvValue, 0, value.Length);

					Assert.AreEqual(value.Length, count);
					Assert.AreEqual(value.Length, csvValue.Length);

					for (int i = 0; i < value.Length; i++)
						Assert.AreEqual(value[i], csvValue[i]);
				}
			}
		}

		[TestMethod]
		public void GetDataTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				while (reader.Read())
				{
					Assert.AreSame(csv, reader.GetData(0));

					for (int i = 1; i < reader.FieldCount; i++)
						Assert.IsNull(reader.GetData(i));
				}
			}
		}

		[TestMethod]
		public void GetDataTypeNameTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				while (reader.Read())
				{
					for (int i = 0; i < reader.FieldCount; i++)
						Assert.AreEqual(typeof(string).FullName, reader.GetDataTypeName(i));
				}
			}
		}

		[TestMethod]
		public void GetDateTimeTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				DateTime value = new DateTime(2001, 1, 1);
				while (reader.Read())
				{
					Assert.AreEqual(value, reader.GetDateTime(reader.GetOrdinal(typeof(DateTime).FullName)));
				}
			}
		}

		[TestMethod]
		public void GetDecimalTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				Decimal value = 1;
				while (reader.Read())
				{
					Assert.AreEqual(value, reader.GetDecimal(reader.GetOrdinal(typeof(Decimal).FullName)));
				}
			}
		}

		[TestMethod]
		public void GetDoubleTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				Double value = 1;
				while (reader.Read())
				{
					Assert.AreEqual(value, reader.GetDouble(reader.GetOrdinal(typeof(Double).FullName)));
				}
			}
		}

		[TestMethod]
		public void GetFieldTypeTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				while (reader.Read())
				{
					for (int i = 0; i < reader.FieldCount; i++)
						Assert.AreEqual(typeof(string), reader.GetFieldType(i));
				}
			}
		}

		[TestMethod]
		public void GetFloatTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				Single value = 1;
				while (reader.Read())
				{
					Assert.AreEqual(value, reader.GetFloat(reader.GetOrdinal(typeof(Single).FullName)));
				}
			}
		}

		[TestMethod]
		public void GetGuidTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				Guid value = new Guid("{11111111-1111-1111-1111-111111111111}");
				while (reader.Read())
				{
					Assert.AreEqual(value, reader.GetGuid(reader.GetOrdinal(typeof(Guid).FullName)));
				}
			}
		}

		[TestMethod]
		public void GetInt16Test()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				Int16 value = 1;
				while (reader.Read())
				{
					Assert.AreEqual(value, reader.GetInt16(reader.GetOrdinal(typeof(Int16).FullName)));
				}
			}
		}

		[TestMethod]
		public void GetInt32Test()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				Int32 value = 1;
				while (reader.Read())
				{
					Assert.AreEqual(value, reader.GetInt32(reader.GetOrdinal(typeof(Int32).FullName)));
				}
			}
		}

		[TestMethod]
		public void GetInt64Test()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				Int64 value = 1;
				while (reader.Read())
				{
					Assert.AreEqual(value, reader.GetInt64(reader.GetOrdinal(typeof(Int64).FullName)));
				}
			}
		}

		[TestMethod]
		public void GetNameTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;

				while (reader.Read())
				{
					Assert.AreEqual(CsvReaderSampleData.SampleData1Header0, reader.GetName(0));
					Assert.AreEqual(CsvReaderSampleData.SampleData1Header1, reader.GetName(1));
					Assert.AreEqual(CsvReaderSampleData.SampleData1Header2, reader.GetName(2));
					Assert.AreEqual(CsvReaderSampleData.SampleData1Header3, reader.GetName(3));
					Assert.AreEqual(CsvReaderSampleData.SampleData1Header4, reader.GetName(4));
					Assert.AreEqual(CsvReaderSampleData.SampleData1Header5, reader.GetName(5));
				}
			}
		}

		[TestMethod]
		public void GetOrdinalTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;

				while (reader.Read())
				{
					Assert.AreEqual(0, reader.GetOrdinal(CsvReaderSampleData.SampleData1Header0));
					Assert.AreEqual(1, reader.GetOrdinal(CsvReaderSampleData.SampleData1Header1));
					Assert.AreEqual(2, reader.GetOrdinal(CsvReaderSampleData.SampleData1Header2));
					Assert.AreEqual(3, reader.GetOrdinal(CsvReaderSampleData.SampleData1Header3));
					Assert.AreEqual(4, reader.GetOrdinal(CsvReaderSampleData.SampleData1Header4));
					Assert.AreEqual(5, reader.GetOrdinal(CsvReaderSampleData.SampleData1Header5));
				}
			}
		}

		[TestMethod]
		public void GetStringTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				String value = "abc";
				while (reader.Read())
				{
					Assert.AreEqual(value, reader.GetString(reader.GetOrdinal(typeof(String).FullName)));
				}
			}
		}

		[TestMethod]
		public void GetValueTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;

				string[] values = new string[CsvReaderSampleData.SampleData1RecordCount];

				while (reader.Read())
				{
					for (int i = 0; i < reader.FieldCount; i++)
					{
						object value = reader.GetValue(i);

						if (string.IsNullOrEmpty(csv[i]))
							Assert.AreEqual(DBNull.Value, value);

						values[i] = value.ToString();
					}

					CsvReaderSampleData.CheckSampleData1(csv.HasHeaders, csv.CurrentRecordIndex, values);
				}
			}
		}

		[TestMethod]
		public void GetValuesTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;

				object[] objValues = new object[CsvReaderSampleData.SampleData1RecordCount];
				string[] values = new string[CsvReaderSampleData.SampleData1RecordCount];

				while (reader.Read())
				{
					Assert.AreEqual(CsvReaderSampleData.SampleData1RecordCount, reader.GetValues(objValues));

					for (int i = 0; i < reader.FieldCount; i++)
					{
						if (string.IsNullOrEmpty(csv[i]))
							Assert.AreEqual(DBNull.Value, objValues[i]);

						values[i] = objValues[i].ToString();
					}

					CsvReaderSampleData.CheckSampleData1(csv.HasHeaders, csv.CurrentRecordIndex, values);
				}
			}
		}

		[TestMethod]
		public void IsDBNullTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				while (reader.Read())
				{
					Assert.IsTrue(reader.IsDBNull(reader.GetOrdinal(typeof(DBNull).FullName)));
				}
			}
		}

		[TestMethod]
		public void FieldCountTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;

				Assert.AreEqual(CsvReaderSampleData.SampleData1RecordCount, reader.FieldCount);
			}
		}

		[TestMethod]
		public void IndexerByFieldNameTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;

				string[] values = new string[CsvReaderSampleData.SampleData1RecordCount];

				while (reader.Read())
				{
					values[0] = (string) reader[CsvReaderSampleData.SampleData1Header0];
					values[1] = (string) reader[CsvReaderSampleData.SampleData1Header1];
					values[2] = (string) reader[CsvReaderSampleData.SampleData1Header2];
					values[3] = (string) reader[CsvReaderSampleData.SampleData1Header3];
					values[4] = (string) reader[CsvReaderSampleData.SampleData1Header4];
					values[5] = (string) reader[CsvReaderSampleData.SampleData1Header5];

					CsvReaderSampleData.CheckSampleData1(csv.HasHeaders, csv.CurrentRecordIndex, values);
				}
			}
		}

		[TestMethod]
		public void IndexerByFieldIndexTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;

				string[] values = new string[CsvReaderSampleData.SampleData1RecordCount];

				while (reader.Read())
				{
					for (int i = 0; i < reader.FieldCount; i++)
						values[i] = (string) reader[i];

					CsvReaderSampleData.CheckSampleData1(csv.HasHeaders, csv.CurrentRecordIndex, values);
				}
			}
		}

		#endregion
	}
}
