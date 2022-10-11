using System;
using System.Linq;
using System.Reflection;

namespace DataMigrationTools.Reflection
{
    public static class MethodInvoker
    {
        /// <summary>
        /// Gets the object that represents the function parameter name-value pairs.
        /// </summary>
        /// <param name="mi"></param>
        /// <returns></returns>
        public static SimpleExtendedDynamic GetMethodParamObject(MethodInfo mi)
        {
            var funcParamObject = new SimpleExtendedDynamic();

            var funcParams = mi.GetParameters();

            foreach (var fp in funcParams)
            {
                funcParamObject[fp.Name] = GetParamDefault(fp);
            }

            return funcParamObject;
        }


        /// <summary>
        /// Executes a method using a specified object context and function parameter object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="execContext"></param>
        /// <param name="method"></param>
        /// <param name="functionParamValuesObject"></param>
        /// <returns></returns>
        public static T ExecuteMethod<T>(object execContext, MethodInfo method, SimpleExtendedDynamic functionParamValuesObject)
        {
            return (T) ExecuteMethod(execContext, method, functionParamValuesObject);
        }

        /// <summary>
        /// Executes a method using a specified object context and function parameter object.
        /// </summary>
        /// <param name="execContext"></param>
        /// <param name="method"></param>
        /// <param name="functionParamValues"></param>
        /// <returns></returns>
        public static object ExecuteMethod(object execContext, MethodInfo method, SimpleExtendedDynamic methodParamObject)
        {
            var functionParams = method.GetParameters();

            var result = functionParams.Select(param =>
            {
                var paramValue = methodParamObject[param.Name];

                if (paramValue == null)
                    return null;

                if (paramValue is string)
                {
                    if (string.IsNullOrEmpty((string) paramValue))
                        return null;
                }

                return ReflectionHelpers.ChangeType(paramValue, param.ParameterType);
            }).ToArray();
            
            return method.Invoke(execContext, result);
        }
        
        private static object GetParamDefault(ParameterInfo parameterInfo)
        {
            if (parameterInfo.HasDefaultValue)
                return parameterInfo.DefaultValue;

            var paramType = parameterInfo.ParameterType;

            try
            {
                if (paramType == typeof(string))
                    return "";

                //we're just going to return strings and convert upon method execution
                return "";

                return Activator.CreateInstance(paramType);
            }
            catch (Exception ex)
            {
                //ignore
            }

            return null;
        }
    }
}