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

        public static DataTransform ConvertEnum(Type destEnumType)
        {
            return value =>
            {
                try
                {
                    // ReSharper disable once PossibleNullReferenceException // Because an enum and a nullable enum are both value types, it's actually not possible to reach the next line of code when the value variable is null
                    var r = Enum.Parse(destEnumType, value.ToString(), true);
                    return r;
                }
                catch (Exception exception)
                {
                    throw new TypeConversionException(
                        $"An error occurred while attempting to convert the value '{value}' to an enum of type '{destEnumType}' in a data transform group", exception);
                }
            };
        }
        
        /// <summary>
        /// The default transform group uses a minimum set of robust transforms for transforming CSV and Excel data / dates.
        /// </summary>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public static DataTransform Default(Type destinationType)
        {
            if (destinationType.IsNullableGenericType())
            {
                destinationType = destinationType.GetNonNullableType();

                var destinationTypeConverter = DefaultValueTypeConvert(destinationType);

                return o =>
                {
                    o = DataTransforms.TransformNull(o);

                    if (o == null)
                        return null;

                    o = destinationTypeConverter(o);
                    return o;
                };
            }

            return DefaultValueTypeConvert(destinationType);
        }

        /// <summary>
        /// Converts value types using a robust set of transforms. Does not handle nulls.
        /// </summary>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public static DataTransform DefaultValueTypeConvert(Type destinationType)
        {
            if (destinationType.IsEnum)
                return ConvertEnum(destinationType);
            
            if (destinationType == typeof(string))
                return DataTransforms.None;

            if (destinationType == typeof(bool))
                return DataTransforms.TransformBoolean;

            if (destinationType == typeof(decimal) || destinationType == typeof(double) || destinationType == typeof(float))
                return o => Convert.ChangeType(DataTransforms.TransformDecimal(o), destinationType);

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

        public static DataTransform TypeConvert(Type destinationType)
        {
            return o => Convert.ChangeType(o, destinationType);
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