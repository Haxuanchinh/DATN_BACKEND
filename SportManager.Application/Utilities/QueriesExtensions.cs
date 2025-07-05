using System.Linq.Expressions;

namespace SportManager.Application.Utilities
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Expression<Func<T, bool>> predicate)
        {
            if (!condition)
            {
                return query;
            }

            return query.Where(predicate);
        }

        public static IQueryable<T> OrderByDescendingOrReverseIf<T, TKey>(this IQueryable<T> query, bool condition, Expression<Func<T, TKey>> predicate)
        {
            if (!condition)
            {
                return query.OrderBy(predicate);
            }

            return query.OrderByDescending(predicate);
        }

        public static IQueryable<T> TakeIf<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> orderBy, bool condition, int limit, bool orderByDescending = true)
        {
            query = (orderByDescending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy));
            if (!condition)
            {
                return query;
            }

            return query.Take(limit);
        }

        public static IQueryable<T> TakePage<T>(this IQueryable<T> query, int page, int pageSize)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }

            if (page <= 0)
            {
                page = 1;
            }

            return query.Skip((page - 1) * pageSize).Take(pageSize);
        }
    }
}
