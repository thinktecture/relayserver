using System;
using System.Reflection;
using Autofac;

namespace Thinktecture.Relay.Server.DependencyInjection
{
	internal interface ICustomCodeAssemblyLoader
	{
		Assembly Assembly { get; }
		void RegisterModule(ContainerBuilder builder);
		Type GetType(Type type);
		Type[] GetTypes(Type type);
	}
}
