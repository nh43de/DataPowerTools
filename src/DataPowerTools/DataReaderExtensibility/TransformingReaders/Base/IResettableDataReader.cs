using System.Data;

namespace DataPowerTools.Connectivity.Helpers
{
    public interface IResettableDataReader : IDataReader
    {
        void Reset();
    }
}