using System;

namespace Thinktecture.Relay.IdentityServer.Extensions;

/// <summary>
/// Defines the boundary of a Date Time comparison check.
/// </summary>
internal enum BetweenBoundary
{
	/// <summary>
	/// Includes both boundaries ( start &lt;= value &lt;= end ).
	/// </summary>
	Inclusive,

	/// <summary>
	/// Excludes both boundaries ( start &lt; value &lt; end ).
	/// </summary>
	Exclusive,

	/// <summary>
	/// Excludes the start and includes the end ( start &lt; value &lt;= end ).
	/// </summary>
	ExclusiveStart,

	/// <summary>
	/// Includes the start and excludes the end ( start &lt;= value &lt; end ).
	/// </summary>
	ExclusiveEnd,
}

/// <summary>
/// Provides extension methods for the <see cref="DateTime"/> type.
/// </summary>
internal static class DateTimeExtensions
{
	/// <summary>
	/// Checks whether a <see cref="DateTime"/> value is in between two boundaries.
	/// </summary>
	/// <param name="instant">The value to check.</param>
	/// <param name="start">The lower boundary to check against.</param>
	/// <param name="end">The upper boundary to check against.</param>
	/// <param name="boundaryCheckType">The way to check against the boundaries (inclusive or exclusive).</param>
	/// <returns>True, id specified instant is in between the boundaries; otherwise, false.</returns>
	public static bool IsBetween(this DateTime instant, DateTime start, DateTime end,
		BetweenBoundary boundaryCheckType = BetweenBoundary.Inclusive)
	{
		if (start >= end) throw new ArgumentException($"{nameof(start)} must not be after {nameof(end)}");

		// try to accomodate for comparison
		instant = instant.ToUniversalTime();
		start = start.ToUniversalTime();
		end = end.ToUniversalTime();

		return boundaryCheckType switch
		{
			BetweenBoundary.Inclusive => instant >= start && instant <= end,
			BetweenBoundary.Exclusive => instant > start && instant < end,
			BetweenBoundary.ExclusiveStart => instant > start && instant <= end,
			BetweenBoundary.ExclusiveEnd => instant >= start && instant < end,
			_ => false
		};
	}
}
