using System.Linq;

namespace DataPowerTools.Extensions
{
    public static class ComparisonExtensions
    {
        /// <summary>
        /// Performs a shallow comparison of two objects. No recursion is performed on reference types.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="to"></param>
        /// <param name="ignoreNonStringReferenceTypes"></param>
        /// <returns></returns>
        public static ComparisonResult[] Compare<T>(this T self, T to, bool ignoreNonStringReferenceTypes = true) where T : class
        {
            return Compare(self, to, typeof(T).GetColumnInfo(ignoreNonStringReferenceTypes));
        }

        /// <summary>
        /// Performs a shallow comparison of two objects. No recursion is performed on reference types.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="to"></param>
        /// <param name="propNames">Property names to include.</param>
        /// <returns></returns>
        public static ComparisonResult[] Compare<T>(this T self, T to, string[] propNames) where T : class
        {
            return Compare(self, to, typeof(T).GetColumnInfo(propNames));
        }

        /// <summary>
        /// Performs a shallow comparison of two objects. No recursion is performed on reference types.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="to"></param>
        /// <param name="ignoreNonStringReferenceTypes"></param>
        /// <returns></returns>
        public static bool CompareTo<T>(this T self, T to, bool ignoreNonStringReferenceTypes = true) where T : class
        {
            return CompareTo(self, to, typeof(T).GetColumnInfo(ignoreNonStringReferenceTypes));
        }

        /// <summary>
        /// Performs a shallow comparison of two objects. No recursion is performed on reference types.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="to"></param>
        /// <param name="propNames">Property names to include.</param>
        /// <returns></returns>
        public static bool CompareTo<T>(this T self, T to, string[] propNames) where T : class
        {
            return CompareTo(self, to, typeof(T).GetColumnInfo(propNames));
        }

        /// <summary>
        /// Performs a shallow comparison of two objects. No recursion is performed on reference types.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="to"></param>
        /// <param name="colInfo"></param>
        /// <returns></returns>
        private static bool CompareTo<T>(this T self, T to, CsharpTypeColumnInformation[] colInfo) where T : class
        {
            if (self != null && to != null)
            {
                var unequalProperties = self.Compare(to, colInfo).Where(m => m.IsMatch == false);
                
                return !unequalProperties.Any();
            }
            return self == to;
        }
        
        /// <summary>
        /// Performs a shallow comparison of two objects. No recursion is performed on reference types.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="to"></param>
        /// <param name="colInfo"></param>
        /// <returns></returns>
        private static ComparisonResult[] Compare<T>(this T self, T to, CsharpTypeColumnInformation[] colInfo) where T : class
        {
            if (self != null && to != null)
            {
                var compareResults = colInfo
                        .Select(p => p.PropertyInfo)
                        .Select(pi =>
                            new 
                            {
                                FieldName = pi.Name,
                                SelfValue = pi.GetValue(self, null),
                                ToValue = pi.GetValue(to, null)
                            })
                        .Select(t => new ComparisonResult
                        {
                            SelfValue = t.SelfValue,
                            ToValue = t.ToValue,
                            FieldName = t.FieldName,
                            IsMatch = !(t.SelfValue != t.ToValue
                                      &&
                                      (t.SelfValue == null
                                       || !t.SelfValue.Equals(t.ToValue)))
                        })
                        .ToArray()
                    ;

                return compareResults;
            }
            return new ComparisonResult[]{};
        }

    }
}
