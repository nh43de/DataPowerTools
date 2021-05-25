using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataPowerTools.Extensions;

namespace DataPowerTools.PowerTools
{
    public class InsertCommandSqlBuilder
    {
        //TODO: needs to support mapping columns

        public DatabaseEngine DatabaseEngine { get; }
        public TypeAccessorCache TypeAccessorCache { get; } = new TypeAccessorCache();


        public InsertCommandSqlBuilder(DatabaseEngine databaseEngine)
        {
            DatabaseEngine = databaseEngine;
        }

        /// <summary>
        /// The method used for escaping keywords.
        /// </summary>
        public enum KeywordEscapeMethod
        {
            /// <summary>No escape method is used.</summary>
            None = 0,
            /// <summary>Keywords are enclosed in square brackets. Used by SQL Server, SQLite.</summary>
            SquareBracket = 1,
            /// <summary>Keywords are enclosed in double quotes. Used by PostgreSQL, SQLite.</summary>
            DoubleQuote = 2,
            /// <summary>Keywords are enclosed in backticks aka grave accents (ASCII code 96). Used by MySQL, SQLite.</summary>
            Backtick = 3
        }

        private struct SqlInsertInfo
        {
            public string InsertTemplate { get; set; }
            public KeywordEscapeMethod KeywordEscapeMethod { get; set; }
        }

        /// <summary>
        /// Generates a parameterized MySQL INSERT statement from the given object and adds it to the <see cref="DbCommand" />
        /// </summary>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance to append inserts to.</param>
        /// <param name="obj">Object to generate the SQL INSERT statement from.</param>
        /// <param name="destinationTableName"></param>
        public DbCommand AppendInsert(DbCommand dbCommand, object obj, string destinationTableName)
        {
            var i = GetInsertTemplate(DatabaseEngine);

            return AppendInsertCommand(dbCommand, obj, i.InsertTemplate, destinationTableName, i.KeywordEscapeMethod);
        }

        /// <summary>
        /// Generates a parameterized MySQL INSERT statement from the given object and adds it to the <see cref="DbCommand" />
        /// </summary>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance to append inserts to.</param>
        /// <param name="dataRecord">Record to generate the SQL INSERT statement from.</param>
        /// <param name="destinationTableName"></param>
        public DbCommand AppendInsert(DbCommand dbCommand, IDataRecord dataRecord, string destinationTableName)
        {
            var i = GetInsertTemplate(DatabaseEngine);

            return AppendInsertCommand(dbCommand, dataRecord, i.InsertTemplate, destinationTableName, i.KeywordEscapeMethod);
        }

        /// <summary>
        /// Generates a parameterized SQL INSERT statement from the given object and adds it to the
        /// <see cref="DbCommand" />.
        /// </summary>
        public DbCommand AppendInsertCommand(DbCommand dbCommand, object obj, string sqlInsertStatementTemplate, string tableName, KeywordEscapeMethod keywordEscapeMethod = KeywordEscapeMethod.None)
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

        
        /// <summary>
        /// Generates a parameterized SQL INSERT statement from the given object and adds it to the
        /// <see cref="DbCommand" />.
        /// </summary>
        public DbCommand AppendInsertCommand(DbCommand dbCommand, IDictionary<string, object> columnNamesAndValues, string sqlInsertStatementTemplate, string tableName, KeywordEscapeMethod keywordEscapeMethod = KeywordEscapeMethod.None)
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

            GetEscapeStrings(keywordEscapeMethod, out var preKeywordEscapeCharacter, out var postKeywordEscapeCharacter);
            
            var linePrefix = Environment.NewLine + "\t";

            var columns = string.Empty;
            var values = string.Empty;
            
            foreach (var nameAndValue in columnNamesAndValues)
            {
                if (nameAndValue.Value == null)
                    continue;

                columns += linePrefix + preKeywordEscapeCharacter + nameAndValue.Key + postKeywordEscapeCharacter + ",";

                // Note that we are appending the ordinal parameter position as a suffix to the parameter name in order to create
                // some uniqueness for each parameter name so that this method can be called repeatedly as well as to aid in debugging.
                var parameterName = "@" + nameAndValue.Key + "_p" + dbCommand.Parameters.Count;

                values += linePrefix + parameterName + ",";

                dbCommand.AddParameter(parameterName, nameAndValue.Value);
            }

            dbCommand.AppendCommandText(string.Format(sqlInsertStatementTemplate, tableName, columns.TrimEnd(','), values.TrimEnd(',')));

            return dbCommand;
        }

        //TODO: needs to support mapping columns
        /// <summary>
        /// Generates a parameterized SQL INSERT statement from the given object and adds it to the
        /// <see cref="DbCommand" />.
        /// </summary>
        public DbCommand AppendInsertCommand(DbCommand dbCommand, IDataRecord dataRecord, string sqlInsertStatementTemplate, string tableName, KeywordEscapeMethod keywordEscapeMethod = KeywordEscapeMethod.None)
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

            GetEscapeStrings(keywordEscapeMethod, out var preKeywordEscapeCharacter, out var postKeywordEscapeCharacter);

            var linePrefix = Environment.NewLine + "\t";

            var columns = string.Empty;
            var values = string.Empty;

            for (var i = 0; i < dataRecord.FieldCount; i++)
            {
                var columnName = dataRecord.GetName(i); //TODO: needs to support mapping columns
                var columnValue = dataRecord[i];
                
                columns += linePrefix + preKeywordEscapeCharacter + columnName + postKeywordEscapeCharacter + ",";

                // Note that we are appending the ordinal parameter position as a suffix to the parameter name in order to create
                // some uniqueness for each parameter name so that this method can be called repeatedly as well as to aid in debugging.
                var parameterName = "@" + columnName + "_p" + dbCommand.Parameters.Count;

                values += linePrefix + parameterName + ",";

                dbCommand.AddParameter(parameterName, columnValue);
            }

            dbCommand.AppendCommandText(string.Format(sqlInsertStatementTemplate, tableName, columns.TrimEnd(','), values.TrimEnd(',')));

            return dbCommand;
        }



        private static void GetEscapeStrings(KeywordEscapeMethod keywordEscapeMethod, out string preKeywordEscapeCharacter, out string postKeywordEscapeCharacter)
        {
            switch (keywordEscapeMethod)
            {
                case KeywordEscapeMethod.SquareBracket:
                    preKeywordEscapeCharacter = "[";
                    postKeywordEscapeCharacter = "]";
                    break;
                case KeywordEscapeMethod.DoubleQuote:
                    preKeywordEscapeCharacter = "\"";
                    postKeywordEscapeCharacter = "\"";
                    break;
                case KeywordEscapeMethod.Backtick:
                    preKeywordEscapeCharacter = "`";
                    postKeywordEscapeCharacter = "`";
                    break;
                case KeywordEscapeMethod.None:
                    preKeywordEscapeCharacter = "";
                    postKeywordEscapeCharacter = "";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(keywordEscapeMethod), keywordEscapeMethod, null);
            }
        }


        private static SqlInsertInfo GetInsertTemplate(DatabaseEngine databaseEngine)
        {
            switch (databaseEngine)
            {
                case DatabaseEngine.MySql:
                    return new SqlInsertInfo
                        {
                            InsertTemplate = @"
INSERT INTO {0}
({1}
)
VALUES
({2}
);
SELECT LAST_INSERT_ID() AS LastInsertedId;
",
                            KeywordEscapeMethod = KeywordEscapeMethod.Backtick
                        }
                        ;
                case DatabaseEngine.Postgre:
                    return new SqlInsertInfo
                    {
                        InsertTemplate = @"
INSERT INTO {0}
({1}
)
VALUES
({2}
);
select LastVal();
",
                        KeywordEscapeMethod = KeywordEscapeMethod.None
                    };
                case DatabaseEngine.Sqlite:
                    return new SqlInsertInfo
                    {
                        InsertTemplate = @"
INSERT INTO {0}
({1}
)
VALUES
({2}
);
SELECT last_insert_rowid() AS [LastInsertedId];
",
                        KeywordEscapeMethod = KeywordEscapeMethod.SquareBracket
                    };
                case DatabaseEngine.SqlServer:
                    return new SqlInsertInfo
                    {
                        InsertTemplate = @"
INSERT INTO {0}
({1}
)
VALUES
({2}
);
SELECT SCOPE_IDENTITY() AS [LastInsertedId];
",
                        KeywordEscapeMethod = KeywordEscapeMethod.SquareBracket
                    };
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseEngine), databaseEngine, null);
            }
        }


    }


}
