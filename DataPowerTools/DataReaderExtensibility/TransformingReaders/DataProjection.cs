namespace DataPowerTools.DataReaderExtensibility.TransformingReaders
{
    /// <summary>
    ///     A data projection takes an array of objects and returns a single object.
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public delegate object DataProjection(object[] vector);
}