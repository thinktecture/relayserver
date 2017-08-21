using System;
using System.Linq;
using System.Linq.Expressions;
using Thinktecture.Relay.Server.Dto;

namespace Thinktecture.Relay.Server.Repository
{
	// ReSharper disable InconsistentNaming
	public static class IQueryableExtensions
	// ReSharper restore InconsistentNaming
	{
		// http://stackoverflow.com/a/21177073/959687
		public static IQueryable<T> OrderByPropertyName<T>(this IQueryable<T> query, string column,
			SortDirection sortDirection)
		{
			var parameter = Expression.Parameter(typeof(T), "param");
			var property = Expression.Property(parameter, column);
			var expression = Expression.Lambda(property, parameter);
			var method = sortDirection == SortDirection.Asc ? "OrderBy" : "OrderByDescending";
			var types = new Type[] { query.ElementType, expression.Body.Type };
			var result = Expression.Call(typeof(Queryable), method, types, query.Expression, expression);
			return query.Provider.CreateQuery<T>(result);
		}

		public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, PageRequest paging)
		{
			if (paging.PageSize > 0)
			{
				query = query.Skip((paging.Page - 1) * paging.PageSize);
				query = query.Take(paging.PageSize);
			}

			return query;
		}
	}
}
