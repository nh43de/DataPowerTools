using System;
using System.Collections;
using System.Collections.Generic;

namespace DataPowerTools.Enumeration
{
    public class SubsetEnumerator<T> : IEnumerator<T>
    {
        public SubsetEnumerator(IEnumerator<T> baseEnumerator, int lower, int upper)
        {
            BaseEnumerator = baseEnumerator;
            Upper = upper;
            Lower = lower;
            CurrentIndex = lower - 1;

            Reset();
        }

        private IEnumerator<T> BaseEnumerator { get; }

        private int Upper { get; }
        private int Lower { get; }
        private int CurrentIndex { get; set; }

        public void Dispose()
        {
            BaseEnumerator.Dispose();
        }

        public bool MoveNext()
        {
            CurrentIndex++;
            return (CurrentIndex < Upper) && BaseEnumerator.MoveNext();
        }

        public void Reset()
        {
            BaseEnumerator.Reset();

            for (var i = 0; i < Lower - 1; i++)
                if (BaseEnumerator.MoveNext() == false)
                    throw new Exception("Invalid subset bounds");

            CurrentIndex = Lower;
        }

        public T Current => CurrentIndex < Lower ? default(T) : BaseEnumerator.Current;
        object IEnumerator.Current => Current;
    }
}