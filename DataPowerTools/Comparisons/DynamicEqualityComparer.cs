using System;
using System.Collections.Generic;

namespace DataPowerTools.Extensions
{
    public sealed class DynamicEqualityComparer<TLeft, TRight> : IEqualityComparer<object>
        where TLeft : class where TRight : class
    {
        private readonly Func<TLeft, TRight, bool> _func;

        public DynamicEqualityComparer(Func<TLeft, TRight, bool> func)
        {
            _func = func;
        }

        public new bool Equals(object x, object y)
        {
            return _func(x as TLeft, y as TRight);
        }

        public int GetHashCode(object obj) => 0; // force Equals
    }

    public sealed class DynamicEqualityComparer<T> : IEqualityComparer<T>
        where T : class
    {
        private readonly Func<T, T, bool> _func;

        public DynamicEqualityComparer(Func<T, T, bool> func)
        {
            _func = func;
        }

        public bool Equals(T x, T y) => _func(x, y);

        public int GetHashCode(T obj) => 0; // force Equals
    }
}