using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using DataPowerTools.Extensions;
using DataPowerTools.Extensions.Objects;

namespace DataPowerTools.PowerTools
{
    public class InsertSqlBuilder
    {
        private readonly bool _insertNewLines;

        private readonly bool _appendInsertedCols;
        //TODO: needs to support mapping columns

        public DatabaseEngine DatabaseEngine { get; }
        public TypeAccessorCache TypeAccessorCache { get; } = new TypeAccessorCache();


        public InsertSqlBuilder(DatabaseEngine databaseEngine, bool insertNewLines = false, bool appendInsertedCols = false)
        {
            _insertNewLines = insertNewLines;
            _appendInsertedCols = appendInsertedCols;
            DatabaseEngine = databaseEngine;
        }
        
        /// <summary>
        /// Generates a parameterized MySQL INSERT statement from the given object and adds it to the <see cref="DbCommand" />
        /// </summary>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance to append inserts to.</param>
        /// <param name="obj">Object to generate the SQL INSERT statement from.</param>
        /// <param name="destinationTableName"></param>
        public StringBuilder AppendInsert(StringBuilder dbCommand, object obj, string destinationTableName)
        {
            var i = GetInsertTemplate(DatabaseEngine, _appendInsertedCols);

            return AppendInsertCommand(dbCommand, obj, i.InsertTemplate, destinationTableName, i.KeywordEscapeMethod);
        }

        /// <summary>
        /// Generates a parameterized MySQL INSERT statement from the given object and adds it to the <see cref="DbCommand" />
        /// </summary>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance to append inserts to.</param>
        /// <param name="dataRecord">Record to generate the SQL INSERT statement from.</param>
        /// <param name="destinationTableName"></param>
        public StringBuilder AppendInsert(StringBuilder dbCommand, IDataRecord dataRecord, string destinationTableName)
        {
            var i = GetInsertTemplate(DatabaseEngine, _appendInsertedCols);

            return AppendInsertCommand(dbCommand, dataRecord, i.InsertTemplate, destinationTableName, i.KeywordEscapeMethod);
        }

        /// <summary>
        /// Generates a parameterized SQL INSERT statement from the given object and adds it to the
        /// <see cref="DbCommand" />.
        /// </summary>
        private StringBuilder AppendInsertCommand(StringBuilder dbCommand, object obj, string sqlInsertStatementTemplate, string tableName, KeywordEscapeMethod keywordEscapeMethod = KeywordEscapeMethod.None)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            
            var typeAccessor = TypeAccessorCache.GetTypeAccessor(obj);

            var namesAndValues = obj.GetPropertyAndFieldNamesAndValuesDictionary(typeAccessor);

            return AppendInsertCommand(dbCommand, namesAndValues, sqlInsertStatementTemplate, tableName,
                keywordEscapeMethod);
        }

        private string EscapeValueString(string valueString)
        {
            return valueString.Replace("'", "''");
        }

        /// <summary>
        /// Generates a parameterized SQL INSERT statement from the given object and adds it to the
        /// <see cref="DbCommand" />.
        /// </summary>
        private StringBuilder AppendInsertCommand(StringBuilder dbCommand, IDictionary<string, object> columnNamesAndValues, string sqlInsertStatementTemplate, string tableName, KeywordEscapeMethod keywordEscapeMethod = KeywordEscapeMethod.None)
        {
            //TODO: performance optimization: would have better performance if parameter values were changed instead of rebuilding the command every time (https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/bulk-insert)
            //TODO: do we really want this a dictionary?? seems like a waste to do all that hashing when building it

            if (columnNamesAndValues == null)
            {
                throw new ArgumentNullException(nameof(columnNamesAndValues));
            }

            if (sqlInsertStatementTemplate == null)
            {
                throw new ArgumentNullException(nameof(sqlInsertStatementTemplate));
            }

            if (string.IsNullOrWhiteSpace(sqlInsertStatementTemplate))
            {
                throw new ArgumentNullException(nameof(sqlInsertStatementTemplate), "The 'sqlInsertStatementTemplate' parameter must not be null, empty, or whitespace.");
            }

            if (sqlInsertStatementTemplate.Contains("{0}") == false || sqlInsertStatementTemplate.Contains("{1}") == false || sqlInsertStatementTemplate.Contains("{2}") == false)
            {
                throw new Exception("The 'sqlInsertStatementTemplate' parameter does not conform to the template requirements of containing three string.Format arguments. A valid example is: INSERT INTO {0} ({1}) VALUES({2});");
            }

            InsertCommandSqlBuilder.GetEscapeStrings(keywordEscapeMethod, out var preKeywordEscapeCharacter, out var postKeywordEscapeCharacter);
            
            var linePrefix = _insertNewLines ? Environment.NewLine + "\t" : string.Empty;

            var columns = string.Empty;
            var values = string.Empty;
            
            foreach (var nameAndValue in columnNamesAndValues)
            {
                if (nameAndValue.Value == null)
                    continue;

                var colName = preKeywordEscapeCharacter + nameAndValue.Key + postKeywordEscapeCharacter;

                columns += linePrefix + colName + ",";

                var escapedValue = EscapeValueString(nameAndValue.Value.ToString());

                values += $"'{escapedValue}' as {colName},";
            }

            dbCommand.Append(string.Format(sqlInsertStatementTemplate, tableName, columns.TrimEnd(','), values.TrimEnd(',')));

            return dbCommand;
        }

        //TODO: needs to support mapping columns
        /// <summary>
        /// Generates a parameterized SQL INSERT statement from the given object and adds it to the
        /// <see cref="DbCommand" />.
        /// </summary>
        private StringBuilder AppendInsertCommand(StringBuilder dbCommand, IDataRecord dataRecord, string sqlInsertStatementTemplate, string tableName, KeywordEscapeMethod keywordEscapeMethod = KeywordEscapeMethod.None)
        {
            if (dataRecord == null)
            {
                throw new ArgumentNullException(nameof(dataRecord));
            }

            if (sqlInsertStatementTemplate == null)
            {
                throw new ArgumentNullException(nameof(sqlInsertStatementTemplate));
            }

            if (string.IsNullOrWhiteSpace(sqlInsertStatementTemplate))
            {
                throw new ArgumentNullException(nameof(sqlInsertStatementTemplate), "The 'sqlInsertStatementTemplate' parameter must not be null, empty, or whitespace.");
            }

            if (sqlInsertStatementTemplate.Contains("{0}") == false || sqlInsertStatementTemplate.Contains("{1}") == false || sqlInsertStatementTemplate.Contains("{2}") == false)
            {
                throw new Exception("The 'sqlInsertStatementTemplate' parameter does not conform to the template requirements of containing three string.Format arguments. A valid example is: INSERT INTO {0} ({1}) VALUES({2});");
            }

            InsertCommandSqlBuilder.GetEscapeStrings(keywordEscapeMethod, out var preKeywordEscapeCharacter, out var postKeywordEscapeCharacter);

            var linePrefix = _insertNewLines ? Environment.NewLine + "\t" : string.Empty;

            var columns = string.Empty;
            var values = string.Empty;

            for (var i = 0; i < dataRecord.FieldCount; i++)
            {
                var columnName = dataRecord.GetName(i); //TODO: needs to support mapping columns //TODO: this is called with every insert command built - this should be cached
                var columnValue = dataRecord[i];

                var colName = preKeywordEscapeCharacter + columnName + postKeywordEscapeCharacter;

                columns += linePrefix + colName + ",";

                var escapedValue = EscapeValueString(columnValue.ToString());

                values += $"'{escapedValue}' as {colName},";
            }

            var ss = string.Format(sqlInsertStatementTemplate, tableName, columns.TrimEnd(','), values.TrimEnd(','));

            dbCommand.Append(ss);

            return dbCommand;
        }
        
        private static SqlInsertInfo GetInsertTemplate(DatabaseEngine databaseEngine, bool appendLastInserted)
        {
            switch (databaseEngine)
            {
                case DatabaseEngine.MySql:
                    return new SqlInsertInfo
                        {
                            InsertTemplate = @"INSERT INTO {0} ({1}) SELECT {2};" + (appendLastInserted ? " SELECT LAST_INSERT_ID() AS LastInsertedId;" : "") + "\r\n",
                            KeywordEscapeMethod = KeywordEscapeMethod.Backtick
                        }
                        ;
                case DatabaseEngine.Postgre:
                    return new SqlInsertInfo
                    {
                        InsertTemplate = @"INSERT INTO {0} ({1}) SELECT {2};"+ (appendLastInserted ? " select LastVal();" : "") + "\r\n",
                        KeywordEscapeMethod = KeywordEscapeMethod.None
                    };
                case DatabaseEngine.Sqlite:
                    return new SqlInsertInfo
                    {
                        InsertTemplate = @"INSERT INTO {0} ({1}) SELECT {2};" + (appendLastInserted ? " SELECT last_insert_rowid() AS [LastInsertedId];" : "") + "\r\n",
                        KeywordEscapeMethod = KeywordEscapeMethod.SquareBracket
                    };
                case DatabaseEngine.SqlServer:
                    return new SqlInsertInfo
                    {
                        InsertTemplate = @"INSERT INTO {0} ({1}) SELECT {2};" + (appendLastInserted ? " SELECT SCOPE_IDENTITY() AS [LastInsertedId];" : "") + "\r\n",
                        KeywordEscapeMethod = KeywordEscapeMethod.SquareBracket
                    };
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseEngine), databaseEngine, null);
            }
        }

    }
}
