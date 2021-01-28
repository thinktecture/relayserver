using System.Reflection;

// ReSharper disable once CheckNamespace; (extension methods on Stream namespace)
namespace System
{
	/// <summary>
	/// Extension methods for <see cref="Type"/>.
	/// </summary>
	public static class TypeExtensions
	{
		/// <summary>
		/// <summary>Gets the simple name of the assembly. This is usually, but not necessarily, the file name of the manifest file of the assembly, minus its extension.</summary>
		/// </summary>
		/// <param name="type">The <see cref="Type"/>.</param>
		/// <returns>The simple name of the assembly.</returns>
		public static string GetAssemblySimpleName(this Type type) => type.Assembly.GetName().Name;

		/// <summary>
		/// Gets the version of the assembly either from the <see cref="AssemblyInformationalVersionAttribute"/> or <see cref="AssemblyVersionAttribute"/>.
		/// </summary>
		/// <param name="type">The <see cref="Type"/>.</param>
		/// <returns>The version of the assembly.</returns>
		public static string GetAssemblyVersion(this Type type)
		{
			var assembly = type.Assembly;
			return assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ??
				assembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version ?? "Unknown";
		}
	}
}
