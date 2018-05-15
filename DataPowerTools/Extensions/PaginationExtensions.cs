using System.Collections.Generic;
using System.Linq;
using DataPowerTools.DataStructures;

namespace DataPowerTools.Extensions
{
    //TODO: could be improvded somewhat
    /// <summary>
    ///     Used for procuring paginated lists. I.e. takes a queryable and a page index, size, total, etc. and returns the
    ///     right number of items.
    /// </summary>
    public static class PaginationExtensions
    {
        public static PaginatedList<T> ToPaginatedList<T>(this IEnumerable<T> items, int pageIndex, int pageSize,
            int total)
        {
            return new PaginatedList<T>(items, pageIndex, pageSize, total);
        }


        public static IQueryable<T> Paginate<T>(this IQueryable<T> query, int pageIndex, int pageSize)
        {
            var entities = query.Skip((pageIndex - 1)*pageSize).Take(pageSize);
            return entities;
        }
    }
}