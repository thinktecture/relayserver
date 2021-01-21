using System;

namespace Thinktecture.Relay.Server.Diagnostics
{
	/// <summary>
	/// Used to set the verbosity of the <see cref="IRelayRequestLogger{TRequest,TResponse}"/>.
	/// </summary>
	[Flags]
	public enum RelayRequestLoggerLevel
	{
		/// <summary>
		/// Log no requests.
		/// </summary>
		None = 0,

		/// <summary>
		/// Log succeeded requests.
		/// </summary>
		Succeeded = 1,

		/// <summary>
		/// Log aborted requests.
		/// </summary>
		Aborted = 2,

		/// <summary>
		/// Log failed requests.
		/// </summary>
		Failed = 4,

		/// <summary>
		/// Log expired requests.
		/// </summary>
		Expired = 8,

		/// <summary>
		/// Log errored requests.
		/// </summary>
		Errored = 16,

		/// <summary>
		/// Log all requests.
		/// </summary>
		All = Succeeded | Aborted | Failed | Expired | Errored
	}

	/// <summary>
	/// Extension methods for the <see cref="RelayRequestLoggerLevel"/>.
	/// </summary>
	public static class RelayRequestLoggerLevelExtensions
	{
		/// <summary>
		/// Checks the <see cref="RelayRequestLoggerLevel"/> for succeeded.
		/// </summary>
		/// <param name="level">A <see cref="RelayRequestLoggerLevel"/>.</param>
		/// <returns>true, if the level includes <see cref="RelayRequestLoggerLevel.Succeeded"/>; otherwise, false.</returns>
		public static bool LogSucceeded(this RelayRequestLoggerLevel level) => (level & RelayRequestLoggerLevel.Succeeded) != 0;

		/// <summary>
		/// Checks the <see cref="RelayRequestLoggerLevel"/> for aborted.
		/// </summary>
		/// <param name="level">A <see cref="RelayRequestLoggerLevel"/>.</param>
		/// <returns>true, if the level includes <see cref="RelayRequestLoggerLevel.Aborted"/>; otherwise, false.</returns>
		public static bool LogAborted(this RelayRequestLoggerLevel level) => (level & RelayRequestLoggerLevel.Aborted) != 0;

		/// <summary>
		/// Checks the <see cref="RelayRequestLoggerLevel"/> for failed.
		/// </summary>
		/// <param name="level">A <see cref="RelayRequestLoggerLevel"/>.</param>
		/// <returns>true, if the level includes <see cref="RelayRequestLoggerLevel.Failed"/>; otherwise, false.</returns>
		public static bool LogFailed(this RelayRequestLoggerLevel level) => (level & RelayRequestLoggerLevel.Failed) != 0;

		/// <summary>
		/// Checks the <see cref="RelayRequestLoggerLevel"/> for expired.
		/// </summary>
		/// <param name="level">A <see cref="RelayRequestLoggerLevel"/>.</param>
		/// <returns>true, if the level includes <see cref="RelayRequestLoggerLevel.Expired"/>; otherwise, false.</returns>
		public static bool LogExpired(this RelayRequestLoggerLevel level) => (level & RelayRequestLoggerLevel.Expired) != 0;

		/// <summary>
		/// Checks the <see cref="RelayRequestLoggerLevel"/> for errored.
		/// </summary>
		/// <param name="level">A <see cref="RelayRequestLoggerLevel"/>.</param>
		/// <returns>true, if the level includes <see cref="RelayRequestLoggerLevel.Errored"/>; otherwise, false.</returns>
		public static bool LogErrored(this RelayRequestLoggerLevel level) => (level & RelayRequestLoggerLevel.Errored) != 0;
	}
}
