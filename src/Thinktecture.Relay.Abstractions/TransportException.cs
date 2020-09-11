using System;

namespace Thinktecture.Relay
{
	/// <summary>
	/// Thrown when the application encounters an error when reading or writing to or from a transport.
	/// </summary>
	public class TransportException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TransportException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
		public TransportException(Exception innerException)
			: base("The transport raised an error. See inner exception for details.", innerException)
		{
		}
	}
}
