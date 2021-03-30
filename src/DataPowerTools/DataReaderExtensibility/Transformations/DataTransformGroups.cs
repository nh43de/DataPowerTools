using System;
using DataPowerTools.Extensions;

namespace DataPowerTools.DataReaderExtensibility.TransformingReaders
{
    public static class DataTransformGroups
    {
        /// <summary>
        /// Applies no transformation.
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public static DataTransform None(Type dataType)
        {
            return DataTransforms.None;
        }

        //TODO: make these composible. I.e. DataTransformGroups.Default.Add(dataTranform2)
        

        /// <summary>
        /// The default transform group uses a minimum set of robust transforms for transforming CSV and Excel data / dates.
        /// </summary>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public static DataTransform Default(Type destinationType)
        {
            if (destinationType == typeof(string))
                return DataTransforms.None;
            if (destinationType == typeof(bool))
                return DataTransforms.TransformBoolean;
            if (destinationType == typeof(decimal) || destinationType == typeof(double) || destinationType == typeof(float))
                return DataTransforms.TransformDecimal;
            if (destinationType == typeof(int))
                return DataTransforms.TransformInt;
            if (destinationType == typeof(Guid))
                return DataTransforms.TransformGuid;
            if (destinationType == typeof(byte[]))
                return DataTransforms.None;
            if (destinationType == typeof(DateTime))
                return DataTransforms.TransformExcelDate;

            return DataTransforms.TransformStringIsNullOrWhiteSpaceAndTrim;
        }

        /// <summary>
        /// Uses some checking and MS default converters for C# types using .ConvertType
        /// </summary>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public static DataTransform DefaultConvert(Type destinationType)
        {
            return o => o.ConvertTo(destinationType);
        }
    }
}