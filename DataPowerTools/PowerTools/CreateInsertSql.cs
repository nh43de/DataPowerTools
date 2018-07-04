using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataPowerTools.Extensions;

namespace DataPowerTools.PowerTools
{
    public static class CreateInsertSql
    {
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

        
        /// <summary>
        /// Generates a parameterized MySQL INSERT statement from the given object and adds it to the <see cref="DbCommand" />
        /// .
        /// <para>
        /// Note that the generated query also selects the last inserted id using MySQL's SELECT LAST_INSERT_ID() function.
        /// </para>
        /// </summary>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance.</param>
        /// <param name="obj">Object to generate the SQL INSERT statement from.</param>
        /// <param name="tableName">
        /// Optional table name to insert into. If none is supplied, it will use the type name. Note that this parameter is
        /// required when passing in an anonymous object or an <see cref="ArgumentNullException" /> will be thrown.
        /// </param>
        /// <returns>The given <see cref="DbCommand" /> instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// The value of 'tableName' cannot be null when the object passed is an anonymous
        /// type.
        /// </exception>
        public static DbCommand AppendInsertForMySql(DbCommand dbCommand, object obj, string tableName = null)
        {
            const string mySqlInsertStatementTemplate = @"
INSERT INTO {0}
({1}
)
VALUES
({2}
);
SELECT LAST_INSERT_ID() AS LastInsertedId;
"; // Intentional line break for readability of multiple inserts

            return AppendInsertCommand(dbCommand, obj, mySqlInsertStatementTemplate, tableName, CreateInsertSql.KeywordEscapeMethod.Backtick);
        }

        /// <summary>
        /// Generates a parameterized PostgreSQL INSERT statement from the given object and adds it to the <see cref="DbCommand" />
        /// .
        /// <para>
        /// Note that the generated query also selects the last inserted id using PostgreSQL's LastVal() function.
        /// </para>
        /// </summary>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance.</param>
        /// <param name="obj">Object to generate the SQL INSERT statement from.</param>
        /// <param name="tableName">
        /// Optional table name to insert into. If none is supplied, it will use the type name. Note that this parameter is
        /// required when passing in an anonymous object or an <see cref="ArgumentNullException" /> will be thrown.
        /// </param>
        /// <returns>The given <see cref="DbCommand" /> instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// The value of 'tableName' cannot be null when the object passed is an anonymous
        /// type.
        /// </exception>
        public static DbCommand AppendInsertForPostgreSql(DbCommand dbCommand, object obj, string tableName = null)
        {
            const string postgreSqlInsertStatementTemplate = @"
INSERT INTO {0}
({1}
)
VALUES
({2}
);
select LastVal();
";

            return AppendInsertCommand(dbCommand, obj, postgreSqlInsertStatementTemplate, tableName, CreateInsertSql.KeywordEscapeMethod.None);
        }

        /// <summary>
        /// Generates a parameterized SQLite INSERT statement from the given object and adds it to the <see cref="DbCommand" />
        /// .
        /// <para>
        /// Note that the generated query also selects the last inserted id using SQLite's SELECT last_insert_rowid() function.
        /// </para>
        /// </summary>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance.</param>
        /// <param name="obj">Object to generate the SQL INSERT statement from.</param>
        /// <param name="tableName">
        /// Optional table name to insert into. If none is supplied, it will use the type name. Note that this parameter is
        /// required when passing in an anonymous object or an <see cref="ArgumentNullException" /> will be thrown.
        /// </param>
        /// <returns>The given <see cref="DbCommand" /> instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// The value of 'tableName' cannot be null when the object passed is an anonymous
        /// type.
        /// </exception>
        // ReSharper disable once InconsistentNaming
        public static DbCommand AppendInsertForSQLite(DbCommand dbCommand, object obj, string tableName = null)
        {
            const string sqliteInsertStatementTemplate = @"
INSERT INTO {0}
({1}
)
VALUES
({2}
);
SELECT last_insert_rowid() AS [LastInsertedId];
"; // Intentional line break for readability of multiple inserts

            return AppendInsertCommand(dbCommand, obj, sqliteInsertStatementTemplate, tableName, CreateInsertSql.KeywordEscapeMethod.SquareBracket);
        }
        
        /// <summary>
        /// Generates a parameterized SQL Server INSERT statement from the given object and adds it to the
        /// <see cref="DbCommand" />.
        /// <para>
        /// Note that the generated query also selects the last inserted id using SQL Server's SELECT SCOPE_IDENTITY() function.
        /// </para>
        /// </summary>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance.</param>
        /// <param name="obj">Object to generate the SQL INSERT statement from.</param>
        /// <param name="tableName">
        /// Optional table name to insert into. If none is supplied, it will use the type name. Note that this parameter is
        /// required when passing in an anonymous object or an <see cref="ArgumentNullException" /> will be thrown.
        /// </param>
        /// <returns>The given <see cref="DbCommand" /> instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// The value of 'tableName' cannot be null when the object passed is an anonymous
        /// type.
        /// </exception>
        public static DbCommand AppendInsertForSqlServer(DbCommand dbCommand, object obj, string tableName = null)
        {
            const string sqlServerInsertStatementTemplate = @"
INSERT INTO {0}
({1}
)
VALUES
({2}
);
SELECT SCOPE_IDENTITY() AS [LastInsertedId];
"; // Intentional line break for readability of multiple inserts

            return AppendInsertCommand(dbCommand, obj, sqlServerInsertStatementTemplate, tableName, CreateInsertSql.KeywordEscapeMethod.SquareBracket);
        }
        
        /// <summary>
        /// Generates a parameterized SQL INSERT statement from the given object and adds it to the
        /// <see cref="DbCommand" />.
        /// </summary>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance.</param>
        /// <param name="obj">Object to generate the SQL INSERT statement from.</param>
        /// <param name="sqlInsertStatementTemplate">
        /// SQL INSERT statement template where argument 0 is the table name, argument 1 is the comma delimited list of columns,
        /// and argument 2 is the comma delimited list of values.
        /// <para>Example: INSERT INTO {0} ({1}) VALUES({2});</para>
        /// </param>
        /// <param name="tableName">
        /// Optional table name to insert into. If none is supplied, it will use the type name. Note that this parameter is
        /// required when passing in an anonymous object or an <see cref="ArgumentNullException" /> will be thrown.
        /// </param>
        /// <param name="keywordEscapeMethod">The method used for escaping keywords.</param>
        /// <returns>The given <see cref="DbCommand" /> instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// The value of 'tableName' cannot be null when the object passed is an anonymous
        /// type.
        /// </exception>
        public static DbCommand AppendInsertCommand(DbCommand dbCommand, object obj, string sqlInsertStatementTemplate, string tableName = null, KeywordEscapeMethod keywordEscapeMethod = KeywordEscapeMethod.None)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
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

            if (tableName == null && obj.IsAnonymousType())
            {
                throw new ArgumentNullException(nameof(tableName), "The 'tableName' parameter must be provided when the object supplied is an anonymous type.");
            }

            var preKeywordEscapeCharacter = "";

            var postKeywordEscapeCharacter = "";

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
            }

            if (tableName == null)
            {
                tableName = preKeywordEscapeCharacter + obj.GetType().Name + postKeywordEscapeCharacter;
            }

            var linePrefix = Environment.NewLine + "\t";

            var columns = string.Empty;

            var values = string.Empty;

            var namesAndValues = obj.GetPropertyAndFieldNamesAndValues();

            foreach (var nameAndValue in namesAndValues)
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


    }
}
