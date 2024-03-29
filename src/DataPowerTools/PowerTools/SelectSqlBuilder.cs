﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using DataPowerTools.Extensions;
using DataPowerTools.Extensions.Objects;

namespace DataPowerTools.PowerTools
{
    public class SelectSqlBuilder
    {
        private readonly string _joinString;
        private readonly bool _colNamesFirstRowOnly;
        
        private readonly string _preKeywordEscapeCharacter;
        private readonly string _postKeywordEscapeCharacter;
        private readonly List<string> _selectStatements = new List<string>();

        private readonly string _linePrefix;
        private bool _firstRow = true;

        public DatabaseEngine DatabaseEngine { get; }

        private readonly SqlInsertInfo _i;

        public SelectSqlBuilder(DatabaseEngine databaseEngine, string joinString = "UNION ALL", bool colNamesFirstRowOnly = true, bool insertNewLines = true)
        {
            _joinString = joinString;
            _colNamesFirstRowOnly = colNamesFirstRowOnly;
            DatabaseEngine = databaseEngine;

            _linePrefix = insertNewLines ? Environment.NewLine : string.Empty;

            _i = GetInsertTemplate(DatabaseEngine);
            
            InsertCommandSqlBuilder.GetEscapeStrings(_i.KeywordEscapeMethod, out _preKeywordEscapeCharacter, out _postKeywordEscapeCharacter);
        }

        public string WriteString()
        {
            var r = _selectStatements.JoinStr(_joinString + _linePrefix);

            return r;
        }

        /// <summary>
        /// Generates a parameterized SQL INSERT statement for the given object and it's properties.
        /// <see cref="DbCommand" />.
        /// </summary>
        public void AppendObject(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            
            var typeAccessor = TypeAccessorCache.GetTypeAccessor(obj);

            var namesAndValues = obj.GetPropertyAndFieldNamesAndValuesDictionary(typeAccessor);

            AppendFromValuesDictionary(namesAndValues);
        }

        private string EscapeValueString(string valueString)
        {
            return valueString.Replace("'", "''");
        }

        /// <summary>
        /// Generates a parameterized SQL INSERT statement from the given object and adds it to the
        /// <see cref="DbCommand" />.
        /// </summary>
        public void AppendFromValuesDictionary(IDictionary<string, object> columnNamesAndValues)
        {
            var sqlInsertStatementTemplate = _i.InsertTemplate;

            if (columnNamesAndValues == null)
            {
                throw new ArgumentNullException(nameof(columnNamesAndValues));
            }
            
            if (string.IsNullOrWhiteSpace(sqlInsertStatementTemplate))
            {
                throw new ArgumentNullException(nameof(sqlInsertStatementTemplate), "The 'sqlInsertStatementTemplate' parameter must not be null, empty, or whitespace.");
            }

            if (sqlInsertStatementTemplate.Contains("{0}") == false || sqlInsertStatementTemplate.Contains("{1}") == false || sqlInsertStatementTemplate.Contains("{2}") == false)
            {
                throw new Exception("The 'sqlInsertStatementTemplate' parameter does not conform to the template requirements of containing three string.Format arguments. A valid example is: INSERT INTO {0} ({1}) VALUES({2});");
            }

            var values = new List<string>();

            foreach (var nameAndValue in columnNamesAndValues)
            {
                if (nameAndValue.Value == null)
                    continue;

                var columnValue = nameAndValue.Value;
                var columnName = _preKeywordEscapeCharacter + nameAndValue.Key + _postKeywordEscapeCharacter;

                values.Add(BuildValue(columnName, columnValue));
            }

            var ss = string.Format(sqlInsertStatementTemplate, values.JoinStr(", "));

            AddRow(ss);
        }

        public void AppendDataRecord(IDataRecord dataRecord)
        {
            var sqlInsertStatementTemplate = _i.InsertTemplate;
            
            if (dataRecord == null)
            {
                throw new ArgumentNullException(nameof(dataRecord));
            }
            
            if (string.IsNullOrWhiteSpace(sqlInsertStatementTemplate))
            {
                throw new ArgumentNullException(nameof(sqlInsertStatementTemplate), "The 'sqlInsertStatementTemplate' parameter must not be null, empty, or whitespace.");
            }

            if (sqlInsertStatementTemplate.Contains("{0}") == false)
            {
                throw new Exception("The 'sqlInsertStatementTemplate' parameter does not conform to the template requirements of containing one string.Format arguments. A valid example is: INSERT INTO {0} ({1}) VALUES({2});");
            }

            var values = new List<string>();

            for (var i = 0; i < dataRecord.FieldCount; i++)
            {
                var columnName = dataRecord.GetName(i);
                var columnValue = dataRecord[i];

                values.Add(BuildValue(columnName, columnValue));
            }

            var ss = string.Format(sqlInsertStatementTemplate, values.JoinStr(", "));

            AddRow(ss);
        }


        private void AddRow(string value)
        {
            if (_firstRow)
                _firstRow = false;

            _selectStatements.Add(value);
        }

        private string BuildValue(string columnName, object columnValue)
        {
            var colName = _preKeywordEscapeCharacter + columnName + _postKeywordEscapeCharacter;
            
            var escapedValue = EscapeValueString(columnValue.ToString());

            if (!_colNamesFirstRowOnly || (_firstRow && _colNamesFirstRowOnly))
            {
                return $"'{escapedValue}' as {colName}";
            }
            else
            {
                return $"'{escapedValue}'";
            }
        }

        private static SqlInsertInfo GetInsertTemplate(DatabaseEngine databaseEngine)
        {
            switch (databaseEngine)
            {
                case DatabaseEngine.MySql:
                    return new SqlInsertInfo
                        {
                            InsertTemplate = @"SELECT {0}" + "\r\n",
                            KeywordEscapeMethod = KeywordEscapeMethod.Backtick
                        };
                case DatabaseEngine.Postgre:
                    return new SqlInsertInfo
                    {
                        InsertTemplate = @"SELECT {0}" + "\r\n",
                        KeywordEscapeMethod = KeywordEscapeMethod.None
                    };
                case DatabaseEngine.Sqlite:
                    return new SqlInsertInfo
                    {
                        InsertTemplate = @"SELECT {0}" + "\r\n",
                        KeywordEscapeMethod = KeywordEscapeMethod.SquareBracket
                    };
                case DatabaseEngine.SqlServer:
                    return new SqlInsertInfo
                    {
                        InsertTemplate = @"SELECT {0}" + "\r\n",
                        KeywordEscapeMethod = KeywordEscapeMethod.SquareBracket
                    };
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseEngine), databaseEngine, null);
            }
        }

    }
}
