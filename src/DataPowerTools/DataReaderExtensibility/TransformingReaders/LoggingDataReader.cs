using System;
using System.Collections.Generic;
using System.Data;

namespace DataPowerTools.DataReaderExtensibility.TransformingReaders
{
    public class LoggingDataReader<TDataReader> : ExtensibleDataReader<TDataReader> where TDataReader : IDataReader
    {
        private readonly long? _maxErrorsBeforeThrowing;
        private readonly List<DataReaderLogEvent> _logEvents = new List<DataReaderLogEvent>();

        public LoggingDataReader(TDataReader dataReader, long? maxErrorsBeforeThrowing) : base(dataReader)
        {
            _maxErrorsBeforeThrowing = maxErrorsBeforeThrowing;
        }

        public List<DataReaderLogEvent> LogEvents
        {
            get
            {
                if (_logEvents.Count > _maxErrorsBeforeThrowing)
                {
                    throw new Exception("Max errors reached. Aborting.");
                }
                return _logEvents;
            }
        }
        
        public override object this[int i]
        {
            get{
                try
                {
                    return base[i];
                }
                catch (Exception e)
                {
                    Log(e);
                    return null;
                }
            }
        }

        private void Log(Exception e)
        {
            LogEvents.Add(new DataReaderLogEvent
            {
                ErrorMessage = e.Message,
                Row = Depth
            });
        }

        public override object this[string name]
        {
            get
            {
                try
                {
                    return base[name];
                }
                catch (Exception e)
                {
                    Log(e);
                    return null;
                }
            }
        }
        
        public override object GetValue(int i)
        {
            try
            {
                return base.GetValue(i);
            }
            catch (Exception e)
            {
                Log(e);
                return null;
            }
        }
    }
}