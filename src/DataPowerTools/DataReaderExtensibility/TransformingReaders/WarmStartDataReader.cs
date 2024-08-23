using System.Data;

namespace DataPowerTools.DataReaderExtensibility.TransformingReaders;

/// <summary>
/// Skips the first read on the data reader and returns the specified first read value instead.
/// </summary>
/// <typeparam name="TDataReader"></typeparam>
public class WarmStartDataReader<TDataReader> : ExtensibleDataReader<TDataReader> where TDataReader : IDataReader
{
    private readonly bool _firstReadValue;
    private bool _isFirstRead = true;

    public WarmStartDataReader(TDataReader dataReader, bool firstReadValue) : base(dataReader)
    {
        _firstReadValue = firstReadValue;
    }

    public override bool Read()
    {
        if (_isFirstRead)
        {
            _isFirstRead = false;
            return _firstReadValue;
        }

        return base.Read();
    }
}