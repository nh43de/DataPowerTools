namespace DataPowerTools.PowerTools
{
    public enum DatabaseEngine
    {
        MySql,
        Postgre,
        /// <summary>
        /// Recommended batch size is 1 for SQLITE bulk inserts
        /// </summary>
        Sqlite,
        SqlServer
    }
}