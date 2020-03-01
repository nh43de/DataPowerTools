using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using DataPowerTools.Extensions;

//TODO: move to abstract.lib (or something like that)

//original author: Nathan Hollis

namespace DataPowerTools.DataStructures
{
    /// <summary>
    /// A BidirectionalMap is a one-to-one dictionary (in math terms, this is a bijective mapping).
    /// For example {{1,10}, {2,10}} is a valid dictionary but an invalid BidirectionalMap.
    /// </summary>
    /// <typeparam name="TLeft"></typeparam>
    /// <typeparam name="TRight"></typeparam>
    internal class BidirectionalMap<TLeft, TRight> : ICollection<Tuple<TLeft, TRight>>
    {
        private readonly ObservableCollection<Tuple<TLeft, TRight>> Mappings;
        private readonly Dictionary<TLeft, TRight> _leftValues;
        private readonly Dictionary<TRight, TLeft> _rightValues;

        /// <summary>
        ///     Default constructor. Takes in a list of tuples that represent the mappings.
        /// </summary>
        /// <param name="mappings"></param>
        /// <param name="leftComparer"></param>
        public BidirectionalMap(IEnumerable<Tuple<TLeft, TRight>> mappings, IEqualityComparer<TLeft> leftComparer = null)
        {
            _leftValues = leftComparer == null
                ? new Dictionary<TLeft, TRight>()
                : new Dictionary<TLeft, TRight>(leftComparer);
            _rightValues = new Dictionary<TRight, TLeft>();
            Mappings = mappings.ToObservableCollection();
            UpdateMappings();
        }

        public BidirectionalMap()
        {
            _leftValues = new Dictionary<TLeft, TRight>();
            _rightValues = new Dictionary<TRight, TLeft>();
            Mappings = new ObservableCollection<Tuple<TLeft, TRight>>();
            Mappings.CollectionChanged += (sender, args) => UpdateMappings();
        }


        public TLeft this[TRight i] => GetLeft(i);

        public TRight this[TLeft i] => GetRight(i);

        /// <summary>
        ///     Update mappings.
        /// </summary>
        private void UpdateMappings()
        {
            Tuple<TLeft, TRight> mappingItem = null;
            try
            {
                foreach (var mapItem in Mappings)
                {
                    mappingItem = mapItem;
                    _leftValues.Add(mapItem.Item1, mapItem.Item2);
                    _rightValues.Add(mapItem.Item2, mapItem.Item1);
                }
            }
            catch (Exception)
            {
                if (mappingItem == null)
                    throw new ItemsNotOneToOneException();

                throw new ItemsNotOneToOneException($"[{mappingItem.Item1},{mappingItem.Item2}]");
            }
        }

        /// <summary>
        ///     Returns true if right contains key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsRightKey(TRight key)
        {
            return _rightValues.ContainsKey(key);
        }

        /// <summary>
        ///     Returns true if left contains key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsLeftKey(TLeft key)
        {
            return _leftValues.ContainsKey(key);
        }

        /// <summary>
        ///     Get left value from right value. Returns false if key does not exist.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetLeft(TRight key, out TLeft value)
        {
            try
            {
                return _rightValues.TryGetValue(key, out value);
            }
            catch
            {
                value = default(TLeft);
                return false;
            }
        }

        /// <summary>
        ///     Get right value from left value. Returns false if key does not exist.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetRight(TLeft key, out TRight value)
        {
            try
            {
                return _leftValues.TryGetValue(key, out value);
            }
            catch
            {
                value = default(TRight);
                return false;
            }
        }

        /// <summary>
        ///     Get left value from right value. (Will throw exception on fail)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TLeft GetLeft(TRight key)
        {
            try
            {
                return _rightValues[key];
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving left. Key: <{key}>. {ex.Message}");
            }
        }

        /// <summary>
        ///     Get right value from left value. (Will throw exception on fail)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TRight GetRight(TLeft key)
        {
            try
            {
                return _leftValues[key];
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving right. Key: <{key}>. {ex.Message}");
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Mappings.GetEnumerator();
        }

        public IEnumerator<Tuple<TLeft, TRight>> GetEnumerator()
        {
            return Mappings.AsEnumerable().GetEnumerator();
        }

        public void Add(Tuple<TLeft, TRight> item)
        {
            Mappings.Add(item);
        }

        public void Clear()
        {
            Mappings.Clear();
        }

        public bool Contains(Tuple<TLeft, TRight> item)
        {
            return Mappings.Contains(item);
        }

        public void CopyTo(Tuple<TLeft, TRight>[] array, int arrayIndex)
        {
            Mappings.CopyTo(array, arrayIndex);
        }

        public bool Remove(Tuple<TLeft, TRight> item)
        {
            return Mappings.Remove(item);
        }

        public int Count => Mappings.Count;
        public bool IsReadOnly => false;

        public void Add(TLeft left, TRight right)
        {
            this.Mappings.Add(new Tuple<TLeft, TRight>(left,right));
        }
    }
}