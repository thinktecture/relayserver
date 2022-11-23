using IdentityServer4.Stores;
using Microsoft.Extensions.Configuration;
using Thinktecture.Relay.IdentityServer.Stores;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for the <see cref="IIdentityServerBuilder"/> type.
/// </summary>
public static class IdentityServerBuilderExtensions
{
	/// <summary>
	/// Adds support for rotating signing keys stored in files.
	/// </summary>
	/// <param name="builder">An instance of an <see cref="IIdentityServerBuilder"/>.</param>
	/// <param name="configuration">An instance of an <see cref="IConfigurationSection"/>.</param>
	/// <returns>The <see cref="IIdentityServerBuilder"/> provided.</returns>
	public static IIdentityServerBuilder AddRotatingFileStore(this IIdentityServerBuilder builder,
		IConfigurationSection configuration)
	{
		builder.Services.AddRotatingFileStore(configuration);

		return builder;
	}

	/// <summary>
	/// Adds support for rotating signing keys stored in files.
	/// </summary>
	/// <param name="services">An instance of an <see cref="IServiceCollection"/>.</param>
	/// <param name="configuration">An instance of an <see cref="IConfigurationSection"/>.</param>
	/// <returns>The <see cref="IServiceCollection"/> provided.</returns>
	public static IServiceCollection AddRotatingFileStore(this IServiceCollection services,
		IConfigurationSection configuration)
	{
		return services
			.Configure<RotateCertificateStoreOptions>(configuration)
			.AddSingleton<RotateCertificateFileStore>()
			.AddSingleton<ISigningCredentialStore, RotateSigningCredentialFileStore>()
			.AddSingleton<IValidationKeysStore, RotateValidationKeysFileStore>();
	}
}
