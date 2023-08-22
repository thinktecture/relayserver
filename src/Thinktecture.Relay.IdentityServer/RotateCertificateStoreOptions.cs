using System;

namespace Thinktecture.Relay.IdentityServer;

/// <summary>
/// The configuration options for the file-based rotating certificate store.
/// </summary>
public class RotateCertificateStoreOptions
{
	/// <summary>
	/// The default rotate interval.
	/// </summary>
	public static readonly TimeSpan DefaultRotateInterval = TimeSpan.FromDays(90);

	/// <summary>
	/// Gets or sets the path where certificates should be located.
	/// </summary>
	public string Path { get; set; } = "./certificates";

	/// <summary>
	/// Gets or sets the password that is used to protected the private keys in the certificate files.
	/// </summary>
	public string Password { get; set; } = null!;

	/// <summary>
	/// Gets or sets the interval in which certificates will be rotated.
	/// </summary>
	public TimeSpan RotateInterval { get; set; } = DefaultRotateInterval;

	private TimeSpan? _announcementPeriod;

	/// <summary>
	/// Gets or sets the period in which a new certificate will be announced before it will become the active one.
	/// </summary>
	public TimeSpan AnnouncementPeriod
	{
		get => _announcementPeriod ?? RotateInterval.Divide(3);
		set => _announcementPeriod = value;
	}
}
