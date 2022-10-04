using System;

namespace Thinktecture.Relay.Server.Management.Models;

/// <summary>
/// Represents a result that carries an Id.
/// </summary>
public class IdResult
{
	/// <summary>
	/// Gets or sets an Id.
	/// </summary>
	public Guid Id { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="IdResult"/> class.
	/// </summary>
	public IdResult() { }

	/// <summary>
	/// Initializes a new instance of the <see cref="IdResult"/> class.
	/// </summary>
	/// <param name="id">An initial value for the <see cref="Id"/> property.</param>
	public IdResult(Guid id)
	{
		Id = id;
	}
}
