using System.Data;

namespace DataPowerTools.DataReaderExtensibility.TransformingReaders
{
    /// <summary>
    /// Basically f(IDataReader) => T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="row"></param>
    /// <returns></returns>
    public delegate T RowProjection<out T>(IDataReader row);
}

