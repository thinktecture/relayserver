using Microsoft.AspNetCore.Builder;

namespace Thinktecture.Relay.Server.DependencyInjection
{
	/// <summary>
	/// An implementation of an <see cref="IApplicationBuilder"/> part to extend the application's request pipeline.
	/// </summary>
	public interface IApplicationBuilderPart
	{
		/// <summary>
		/// Adds the <see cref="IApplicationBuilderPart"/> to the application's request pipeline.
		/// </summary>
		void Use(IApplicationBuilder builder);
	}
}
