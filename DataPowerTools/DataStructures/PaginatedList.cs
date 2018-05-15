using System;
using System.Collections.Generic;

namespace DataPowerTools.DataStructures
{
    public class PaginatedList<T> : List<T>
    {
        public PaginatedList(IEnumerable<T> source, int pageIndex, int pageSize, int totalCount)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            AddRange(source);
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = totalCount;
            TotalPageCount = (int) Math.Ceiling(totalCount/(double) pageSize);
        }

        public int PageIndex { get; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
        public int TotalPageCount { get; }

        public bool HasPreviousPage => PageIndex > 1;

        public bool HasNextPage => PageIndex < TotalPageCount;
    }
}