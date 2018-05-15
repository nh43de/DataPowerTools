using System;
using System.Data;
using System.Data.Common;
using System.Globalization;

namespace DataPowerTools.DataReaderExtensibility
{
    public static class Helpers
    {
        public static DataTable GetSchemaTable(string[] columnNames)
        {
            var schema = new DataTable("SchemaTable")
            {
                Locale = CultureInfo.InvariantCulture,
                MinimumCapacity = columnNames.Length
            };

            schema.Columns.Add(SchemaTableColumn.AllowDBNull, typeof(bool)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.BaseColumnName, typeof(string)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.BaseSchemaName, typeof(string)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.BaseTableName, typeof(string)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.ColumnName, typeof(string)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.ColumnOrdinal, typeof(int)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.ColumnSize, typeof(int)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.DataType, typeof(object)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.IsAliased, typeof(bool)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.IsExpression, typeof(bool)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.IsKey, typeof(bool)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.IsLong, typeof(bool)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.IsUnique, typeof(bool)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.NumericPrecision, typeof(short)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.NumericScale, typeof(short)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.ProviderType, typeof(int)).ReadOnly = true;

            schema.Columns.Add(SchemaTableOptionalColumn.BaseCatalogName, typeof(string)).ReadOnly = true;
            schema.Columns.Add(SchemaTableOptionalColumn.BaseServerName, typeof(string)).ReadOnly = true;
            schema.Columns.Add(SchemaTableOptionalColumn.IsAutoIncrement, typeof(bool)).ReadOnly = true;
            schema.Columns.Add(SchemaTableOptionalColumn.IsHidden, typeof(bool)).ReadOnly = true;
            schema.Columns.Add(SchemaTableOptionalColumn.IsReadOnly, typeof(bool)).ReadOnly = true;
            schema.Columns.Add(SchemaTableOptionalColumn.IsRowVersion, typeof(bool)).ReadOnly = true;

            // null marks columns that will change for each row
            object[] schemaRow =
            {
                true, // 00- AllowDBNull
                null, // 01- BaseColumnName
                string.Empty, // 02- BaseSchemaName
                string.Empty, // 03- BaseTableName
                null, // 04- ColumnName
                null, // 05- ColumnOrdinal
                int.MaxValue, // 06- ColumnSize
                typeof(string), // 07- DataType
                false, // 08- IsAliased
                false, // 09- IsExpression
                false, // 10- IsKey
                false, // 11- IsLong
                false, // 12- IsUnique
                DBNull.Value, // 13- NumericPrecision
                DBNull.Value, // 14- NumericScale
                (int) DbType.String, // 15- ProviderType

                string.Empty, // 16- BaseCatalogName
                string.Empty, // 17- BaseServerName
                false, // 18- IsAutoIncrement
                false, // 19- IsHidden
                true, // 20- IsReadOnly
                false // 21- IsRowVersion
            };

            for (var i = 0; i < columnNames.Length; i++)
            {
                schemaRow[1] = columnNames[i]; // Base column name
                schemaRow[4] = columnNames[i]; // Column name
                schemaRow[5] = i; // Column ordinal

                schema.Rows.Add(schemaRow);
            }

            return schema;
        }
    }
}