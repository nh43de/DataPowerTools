using System;
using System.Data;
using System.Linq;

namespace DataPowerTools.DataReaderExtensibility.TransformingReaders
{
    public class FilteringDataReader<TDataReader> : ExtensibleDataReaderExplicit<TDataReader> where TDataReader : IDataReader
    {
        private object[] CurrentRow;

        public FilteringDataReader(
            TDataReader dataReader,
            RowInclusionCondition[] rowInclusionConditions
        ) : base(dataReader)
        {
            RowInclusionConditions = rowInclusionConditions ?? new RowInclusionCondition[] {};
        }

        private RowInclusionCondition[] RowInclusionConditions { get; }

        public override int Depth { get; } = 0;

        public override int FieldCount => DataReader.FieldCount;


        //these need to be modified
        public override object this[int i] => CurrentRow[i];
        public override object this[string name] => CurrentRow[DataReader.GetOrdinal(name)];


        private void SetCurrentRow()
        {
            CurrentRow = new object[DataReader.FieldCount];
            DataReader.GetValues(CurrentRow);
        }

        public override bool Read() //like readnext
        {
            //var hasRows = CurrentRow != null;
            //We have to read ahead here because of the way dataReader.Read() works
            //DataReader.Read() returns true if more rows are available, and false otherwise.
            
            while (true)
            {
                var hasRecords = DataReader.Read(); //get next record

                if (hasRecords == false)
                    //we have reached the end of the stream and there are no more records, return false
                    return false;

                var recordIsValid = RowInclusionConditions.All(p => p.Invoke(DataReader));
                //determine if this record is valid

                if (!recordIsValid) continue;

                SetCurrentRow();
                return true;
            }
        }

        //TODO: not supported yet
        public override DataTable GetSchemaTable()
        {
            throw new NotSupportedException();
        }

        public override string GetName(int i) => DataReader.GetName(i);
        public override int GetOrdinal(string name) => DataReader.GetOrdinal(name);
        public override object GetValue(int i) => CurrentRow[i];
        public override Type GetFieldType(int i) => DataReader.GetFieldType(i);


        //these are sketchy
        public override bool IsDBNull(int i) => Convert.IsDBNull(CurrentRow[i]);


        public override int GetValues(object[] values)
        {
            var i = 0;
            for (; i < FieldCount; i++)
            {
                if (values.Length <= i)
                    return i;
                values[i] = GetValue(i);
            }
            return i;
        }
    }
}