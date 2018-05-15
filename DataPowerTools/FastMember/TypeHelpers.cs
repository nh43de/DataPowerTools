using System;
using System.Reflection;
using System.Reflection.Emit;

namespace DataPowerTools.FastMember
{
    internal static class TypeHelpers
    {
        public static readonly Type[] EmptyTypes = Type.EmptyTypes;

        public static bool _IsValueType(Type type)
        {
            return type.GetTypeInfo().IsValueType;
        }

        public static bool _IsPublic(Type type)
        {
            return type.IsPublic;
        }

        public static bool _IsNestedPublic(Type type)
        {
            return type.IsNestedPublic;
        }

        public static bool _IsClass(Type type)
        {
            return type.IsClass;
        }

        public static bool _IsAbstract(Type type)
        {
            return type.IsAbstract;
        }

        public static Type _CreateType(TypeBuilder type)
        {
            return type.CreateTypeInfo().AsType();
        }

        public static int Min(int x, int y)
        {
            return x < y ? x : y;
        }
    }
}