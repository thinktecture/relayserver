using System;

namespace Thinktecture.Relay.Server.Persistence.DataTransferObjects;

/// <summary>
/// An object that represents a single page of a paginated result.
/// </summary>
/// <typeparam name="T">The type of the data transfer object in this page.</typeparam>
public class Page<T>
{
	/// <summary>
	/// Gets of sets an array of the actual results for this page.
	/// </summary>
	public T[] Results { get; set; } = Array.Empty<T>();

	/// <summary>
	/// Gets or sets the total amount of data entries available.
	/// </summary>
	public int TotalCount { get; set; }

	/// <summary>
	/// Gets or sets the starting index of the <see cref="Results"/> array within all available entries.
	/// </summary>
	public int Offset { get; set; }

	/// <summary>
	/// Gets or sets the requested maximum size of the <see cref="Results"/> array.
	/// </summary>
	public int PageSize { get; set; }
}
