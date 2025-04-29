using System.Linq.Expressions;

namespace Subspace.API.Helpers
{
    public static class QueryExtensions
    {
        public static IOrderedQueryable<T> ApplySort<T, TKey>(
            this IQueryable<T> baseQuery,
            IOrderedQueryable<T>? current,
            Expression<Func<T, TKey>> keySelector,
            string order)
        {
            if (current == null)
            {
                return order == "desc"
                    ? baseQuery.OrderByDescending(keySelector)
                    : baseQuery.OrderBy(keySelector);
            }
            else
            {
                return order == "desc"
                    ? current.ThenByDescending(keySelector)
                    : current.ThenBy(keySelector);
            }
        }
    }
}