using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
namespace PMS.Core.Helpers
{
    

    public static class IQueryableExtensions
    {
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query,
            QueryParameters request,
            CancellationToken cancellationToken = default)
        {
            var totalRecords = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip(request.Skip)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<T>
            {
                Items = items,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalRecords = totalRecords
            };
        }

        public static IQueryable<T> ApplySorting<T>(
            this IQueryable<T> query,
            QueryParameters request,
            Dictionary<string, Expression<Func<T, object?>>> sortOptions,
            Expression<Func<T, object?>> defaultSort)
        {
            if (string.IsNullOrWhiteSpace(request.SortBy))
                return query.OrderBy(defaultSort);

            var sortBy = request.SortBy.Trim().ToLowerInvariant();

            if (sortOptions.TryGetValue(sortBy, out var sortExpression))
            {
                return request.SortDescending
                    ? query.OrderByDescending(sortExpression)
                    : query.OrderBy(sortExpression);
            }

            return request.SortDescending
                ? query.OrderByDescending(defaultSort)
                : query.OrderBy(defaultSort);
        }
    }
}
