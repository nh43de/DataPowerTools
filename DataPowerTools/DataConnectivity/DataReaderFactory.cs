using System.Data;

namespace DataPowerTools.DataConnectivity
{
    public delegate IDataReader DataReaderFactory();

    public delegate IDataReader DataReaderFactory<in T>(T item);
}