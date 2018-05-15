using System;
using System.Collections;
using System.Collections.Generic;

namespace DataPowerTools.Enumeration
{
    /// <summary>
    /// TODO: not tested
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EnumerableSubset<T> : IEnumerable<T>
    {
        public EnumerableSubset(IEnumerable<T> enumerable, int lower, int maxcount)
        {
            Enumerable = enumerable;
            Lower = lower;
            Upper = maxcount;
        }

        public IEnumerable<T> Enumerable { get; }
        private int Lower { get; }
        private int Upper { get; }

        public IEnumerator<T> GetEnumerator()
        {
            var e = Enumerable.GetEnumerator();
            return new SubsetEnumerator<T>(e, Lower, Upper);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}