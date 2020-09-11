using System;

namespace DataPowerTools.DataReaderExtensibility.TransformingReaders
{
    public static class DataTransformGroups
    {
        public static DataTransform None(Type dataType)
        {
            return DataTransforms.None;
        }
        //TODO: make these composible. I.e. DataTransformGroups.Default.Add(dataTranform2)
        public static DataTransform Default(Type dataType)
        {
            if (dataType == typeof(string))
                return DataTransforms.None;
            if (dataType == typeof(bool))
                return DataTransforms.TransformBoolean;
            if (dataType == typeof(decimal) || dataType == typeof(double) || dataType == typeof(float))
                return DataTransforms.TransformDecimal;
            if (dataType == typeof(int))
                return DataTransforms.TransformInt;
            if (dataType == typeof(Guid))
                return DataTransforms.TransformGuid;
            if (dataType == typeof(byte[]))
                return DataTransforms.None;
            if (dataType == typeof(DateTime))
                return DataTransforms.TransformExcelDate;

            return DataTransforms.TransformStringIsNullOrWhiteSpaceAndTrim;
        }
    }
}