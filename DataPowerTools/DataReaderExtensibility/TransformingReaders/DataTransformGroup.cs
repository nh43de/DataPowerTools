using System;

namespace DataPowerTools.DataReaderExtensibility.TransformingReaders
{
    /// <summary>
    /// A data transform group is a function that returns a DataTransform for a given Type. E.g. by default a DTG(typeof(decimal)) -> TranformDecimal().
    /// See static class DataTransformGroups.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public delegate DataTransform DataTransformGroup(Type type);
}