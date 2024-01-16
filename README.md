# DataPowerTools
ADO.NET Power Tools for In-Memory Data Processing with minimal dependencies.

## Take a chainsaw to your data!

<img src="assets/DptLogo1.png" width="240">

### Overview

DataPowerTools are tools for dealing with changing between IEnumerable, IDataReader, and ADO.NET DataTable/DataSet. 

It also provides a powerful API for filtering and object materialization.

### Why DataPowerTools?

DataPowerTools is not dependent on any other frameworks or libraries such as Entity Framework or any other ORM. It has virtually no dependencies except for some core .NET / ADO.NET libraries. Nor does it try to be an ORM. It is simply a set of tools built around the ADO.NET IDataReader interface that allow you to manipulate the reader, apply transformations, and send this data somewhere else. For example, if you want to read a CSV, transform some column names, trim or truncate some columns, and map it to a SQL server destination and bulk insert, this allows you do do so in a very high performant way and with a low memory footprint. Check out the unit tests for examples.

### Getting Started

1. Install DataPowerTools nuget.
2. Use methods and extensions.


### Understanding IDataReader

IDataReader is a fundamental interface for reading data, whether it be CSV, Excel, SQL database, or other format in a streaming fashion. It has a few main operations, 1) Read() - initializes the reader and reads the first record into memory, or reads the next record. It returns true if there is a next record available, false otherwise 2) GetValue(at i or name) - gets the C# object value at the specified column index or name, 3) GetDataSchema() - gets the column schema information about the data stream, including but not limited to name, index, data type, length, keys, etc. Note that this isn't implemented in all implementations nor is standard across implementations. Also it is non-contractual and the returned types from GetValue could be different or the data could otherwise violate the schema.

### Main Features

At the core there are a few key features that really make life easier when dealing with large amounts in different storage and record formats. It is basically LINQ for IDataReaders plus a lot more.

1. Extending IDataReader with extensible DataReaders and special DataReaders

There are primarily two abstract classes that are used to extend IDataReader: ExtensibleDataReader and ExtensibleDataReaderExplicit. By inheriting from one of these classes you can create a new DataReader class that takes in another IDataReader as a constructor parameter, and extends the one that was passed in.

ExtensibleDataReader - Implements IDataReader and an IDataReader member, if not overridden, will by default route to the passed in IDataReader.

ctor

ex

ExtensibleDataReaderExplicit - Implements IDataReader and an IDataReader member, if not overridden, will by default throw a NotImplementedException.


ctor	

ex

2. LINQ-like extensions for IDataReader

It features standard extensions methods for operating on IDataReader interfaces to avoid tedious boilerplate. Some examples are: 

	1. AddColumn() - adds a new column to the IDataReader output with the specified source, e.g. when you want to add a "ModifiedDate" column
	2. ApplyTransformation() - transforms a column in an IDataReader into another (e.g. parsing a date, number)
	3. SelectRows() - loops over all records in a while loop calling .Read(), and allows you to project rows into another IDataReader or return some new C# object
	4. RenameColumn() - changes the name of an IDataReader column
	5. Where() - filters the IDataReader so that only rows that match the predicate are returned
	6. Do() - perform a side-effecting action on each row
	7. ToDataTable() - not that people really need to use DataTables anymore, but this option is there to materialize to a DataTable
	8. PrintData() - prints all of the current record data as a string
	9. ToList() - materialize an IDataReader to a list using a projection
	10. AsEnumerable() - returns the IDataReader as an IEnumerable
	11. Union() - returns a new IDataReader that is the concatenation of IDataReader 1 and 2
	12. AsCsv() - writes the IDataReader to CSV
	13. ApplyHeaders() - applies header information to the reader
	14. NotifyOn() - every n rows, executes a callback
	15. ToArray() - materialize an IDataReader to a list using a projection
	16. ToDictionary() - materializes an IDataReader to a dictionary using a projection
	17. FitToCreateTableSql() - generates create table SQL based on the input data
	18. Take() - like the LINQ take, only fetches a certain number of rows then .Read() returns false
	18. Diagnostic extensions - can be called on the IDataReader to assist with debugging

These are achieved using derived classes that inherit from one of the base classes (for example see UnionDataReader.cs).

3. Fitting data to table

We like having data in this format. It means we don't have to do any reflection or materialize C# objects at any point in time (why waste compute?). Also most ORMs want data to be nicely formatted from the source, and are often tied to the source. For example, I can't use Dapper to query CSV or Access, and if my data is formatted poorly then I have to do a lot of different cleaning steps which can vary from source framework to source framework. DataPowerTools is completely agnostic: once the data is in IDataReader land, you can push anywhere. Also almost every library uses the IDataReader inferface for reading and writing (e.g. CSV reader, SQL reader, SQL bulk copy, Excel reader).

Having data in this format also means we can be a little smarter with it. For example, if we have some random CSV files, one of the challenges is mapping and uploading this data to a database table. We may have thousands of these files, and we have no idea the schema, or column names and formats could vary from copy to copy. The file may be terabytes in size so we may not be able to afford to waste a lot of resources loading the entire set. We may also be putting this data somewhere that does need the data to be suffiently cleansed for loading. The API structure for DataPowerTools allows the end user to programmatically compose transformations in an intuitive way based on our inputs.

 The IDataReader.FitToCreateTableSql extension scans the entire IDataReader and maps the values to a best-fit SQL table definition.

3.b Sending Data to a Destination

One of the challenges with data is ensuring that it has been sufficiently sanitized before sending to a database. For example, SQL server can error out if inserting a value like '12E-12' into a column that isn't defined as REAL, and C# parsing doesn't handle this for you by default.

Enter the DataTransform. A DataTransform maps a destination type to an (object) -> (object) transformation. So if the destination is a decimal type, by default the system use a transformation that tries to parse the source data as a decimal, etc. The extension methods IDataReader.MapToType and IDataReader.MapToSqlDestination return a new IDataReader that has the transformed values. DataPowerTools uses a defeault transform group that handles a lot of this out-of-the-box (DataTransformGroups.Default). So calling Csv.Read(<file>).MapToSqlDestination(<dest sql server>).BulkInsertSqlServer(<dest sql server>) will do just that, no additional code needed.

3.c Other destinations:

FitToCSharpClass - fits the data to a CSharp class defintion
ReadToJson - reads the data to unstructured JSON

4. Some Examples (coming soon)

DataPowerTools has been used to load tens of terabytes worth of unstructured data into a SQL Server instance in no time at all and production workloads rely on it.

-- read all csv files in a directory, union them, and upload to a sql database
-- stream results of SQL statement into another table
-- stream results of SQL statement to a csv file
-- Warehousing - copy data from one database to another

### Other Features

- Robust date string parsing
- SqlBulkCopy wrapper / map to SQL destination
- Fit table/CSV to SQL table
- SQLite Bulk operations
- Minimal dependencies
- Easily convert between IEnumerable <-> IDataReader <-> DataTable
- Json to SQL Insert Statements
- Unpivot DataTable or IDataReader