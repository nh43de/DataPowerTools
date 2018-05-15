using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DataPowerTools.DataStructures
{
    /// <summary>
    /// Fillboxes are sweet! They are basically a list with a defined capacity- when the list reaches capacity, it "dumps" the items by invoking a callback with the items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FillBox<T>
    {
        private readonly BoxFilledEventHandler BoxFilled;

        public delegate void BoxFilledEventHandler(IEnumerable<T> shippedItems);

        public int? Capacity { get; set; } = 300;

        private ConcurrentBag<T> _items;

        public FillBox(int? capacity, BoxFilledEventHandler callback)
        {
            Capacity = capacity;
            _items = new ConcurrentBag<T>();
            BoxFilled = callback;
        }

        public void Add(T item)
        {
            _items.Add(item);
            if (Capacity != null && _items.Count >= Capacity)
            {
                Dump();
            }
        }

        public int Count => _items.Count;

        public void Dump()
        {
            ConcurrentBag<T> a;

            lock (_items)
            {
                a = _items;
                _items = new ConcurrentBag<T>();
            }

            BoxFilled?.Invoke(a);
        }
    }
}