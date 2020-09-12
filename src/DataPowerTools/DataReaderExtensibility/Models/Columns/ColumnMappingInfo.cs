using System;
using System.Collections.Generic;
using System.Linq;
using DataPowerTools.Extensions;

namespace DataPowerTools.DataReaderExtensibility.Columns
{
    /// <summary>
    ///     Constructs a class that has quick info about mappings between source and destination column info
    /// </summary>
    public class ColumnMappingInfo
    {
        //TODO: needs cleanup
        public ColumnMappingInfo(
            TypedDataColumnInfo[] destinationColumns,
            BasicDataColumnInfo[] sourceColumns)
        {
            SourceColumns = sourceColumns;
            DestinationColumns = destinationColumns;

            //check for uniqueness on both sides
            var destNames = destinationColumns.Select(dc => dc.ColumnName).ToHashSet();
            var srcNames = sourceColumns.Select(dc => dc.ColumnName).ToHashSet();

            if (destNames.Count != destinationColumns.Length)
                throw new Exception("Duplicate column name defined in destination column set");

            if (srcNames.Count != sourceColumns.Length)
                throw new Exception("Duplicate column name defined in source column set");
            ////

            // (Source) BasicDataColumnInfo -> (Destination) BasicDataColumnInfo
            Mappings = sourceColumns.GroupJoin( //join source columns to destination column information
                        destinationColumns,
                        sourceField => sourceField.ColumnName,
                        destinationField => destinationField.ColumnName,
                        (sourceField, groupDestinationFields) => new BasicColumnMapping
                        {
                            SourceField = sourceField,
                            DestinationField = groupDestinationFields.FirstOrDefault()
                        })
                    .ToArray()
                ;

            // int[] --- v[srcOrd] = destOrd
            SourceOrdinalToDestinationOrdinal = Mappings.Select(
                        sourceField =>
                                sourceField.DestinationField?.Ordinal
                    )
                    .ToArray()
                ;

            // dictionary[srcName] = destOrdinal
            SourceColumnNameToDestinationOrdinal = Mappings.ToDictionary(
                readerField => readerField.SourceField.ColumnName,
                readerField => readerField.DestinationField?.Ordinal
            );

            // bool[] --- v[srcOrd] = dest data type is string?
            SourceOrdinalDestinationIsString =
                Mappings
                    .Select(p => (p.DestinationField == null) || (p.DestinationField.DataType == typeof(string)))
                    .ToArray();

            // list of source ordinals that have non-string destinations.
            NonStringDestinationSourceOrdinals =
                Mappings
                    .Where(d => (d.DestinationField != null) && (d.DestinationField.DataType != typeof(string)))
                    .Select(m => m.SourceField.Ordinal)
                    .ToArray();
            ;
        }

        /// <summary>
        ///     Mappings
        ///     Returns [ Source: BasicDataColumnInfo, Destination: BasicDataColumnInfo ]
        /// </summary>
        public BasicColumnMapping[] Mappings { get; }

        public BasicDataColumnInfo[] SourceColumns { get; private set; }

        public TypedDataColumnInfo[] DestinationColumns { get; private set; }

        public string PrintMappings() => Mappings.Select(m => m.ToString()).JoinStr("\r\n");

        public override string ToString() => PrintMappings();

        #region MappingResults 

        /// <summary>
        ///     ( SourceOrdinal ) -> ( InfoOrdinal )
        ///     int[]
        ///     ie. v[srcOrd] = destOrd
        /// </summary>
        public int?[] SourceOrdinalToDestinationOrdinal { get; private set; }


        /// <summary>
        ///     Returns a list of source ordinals that have non-string destinations.
        /// </summary>
        public int[] NonStringDestinationSourceOrdinals { get; private set; }

        /// <summary>
        ///     ( Source name ) -> ( InfoOrdinal )?
        /// </summary>
        public Dictionary<string, int?> SourceColumnNameToDestinationOrdinal { get; private set; }

        /// <summary>
        ///     ( Source ordinal ) -> ( use transform? )
        /// </summary>
        public bool[] SourceOrdinalDestinationIsString { get; private set; }

        #endregion
    }
}