//using EntityFramework.MappingAPI.Extensions;

namespace DataPowerTools.Extensions
{
    public static class SqlBulkCopyExtensions
    {
        //public static DataTable PrepareDataTable<T>(this SqlBulkCopy sqlBulkCopy, DbContext context, IEnumerable<T> items, string connectionString) where T : class
        //{
        //    return PrepareDataTable(sqlBulkCopy, context.Db<T>().TableName, items, connectionString);
        //}

        //public static DataTable PrepareDataTable<T>(this SqlBulkCopy sqlBulkCopy, string tableName, IEnumerable<T> items, string connectionString) where T : class
        //{
        //    var dataTable = ConvertToDataTable(items, tableName);
        //    var columnNames = GetOrderedColumnNames(tableName, connectionString);
        //    SortColumns(dataTable, columnNames);
        //    RemoveNotExistingColumns(dataTable, columnNames);
        //    return dataTable;
        //}


        //private static IEnumerable<string> GetOrderedColumnNames(string tableName, string connectionString)
        //{
        //    using (var connection = new SqlConnection(connectionString))
        //    {
        //        return connection
        //            .Query<string>(
        //                $@"SELECT  COLUMN_NAME AS ColumnName
        //                FROM    INFORMATION_SCHEMA.COLUMNS
        //                WHERE   TABLE_NAME = '{tableName}'
        //                ORDER BY ORDINAL_POSITION")
        //            .ToList();
        //    }
        //}

    }
}