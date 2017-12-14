using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Thinktecture.Relay.OnPremiseConnector;

namespace Thinktecture.Relay.Server
{
	public static class AssemblyExtensions
	{
		public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
		{
			try
			{
				return assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException e)
			{
				return e.Types.Where(t => t != null);
			}
		}

		public static bool HasAttribute<T>(this MethodInfo method) where T : Attribute
		{
			return method.GetCustomAttributes(typeof(T), false).Any();
		}

		public static IEnumerable<string> GetAsyncVoidMethods(this Assembly assembly, string[] ignoredMethods)
		{
			return assembly.GetLoadableTypes()
				.SelectMany(type => type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
				.Where(method => method.HasAttribute<AsyncStateMachineAttribute>() && method.ReturnType == typeof(void))
				.Select(method => $"{method.DeclaringType.Name}.{method.Name}")
				.Where(method => ignoredMethods.All(name => name != method));
		}
	}

	[TestClass]
	public class NoAsyncVoidTest
	{
		[TestMethod]
		public void Ensure_RelayServer_assembly_has_no_async_void_methods()
		{
			AssertNoAsyncVoidMethods(typeof(RelayService).Assembly);
		}

		[TestMethod]
		public void Ensure_OnPremiseConnector_assembly_has_no_async_void_methods()
		{
			AssertNoAsyncVoidMethods(typeof(RelayServerConnector).Assembly, "RelayServerConnection.OnMessageReceived");
		}

		private static void AssertNoAsyncVoidMethods(Assembly assembly, params string[] ignoredMethods)
		{
			var messages = assembly
				.GetAsyncVoidMethods(ignoredMethods)
				.Select(method => $"'{method}' is an async void method.")
				.ToList();
			Assert.IsFalse(messages.Any(), "Async void methods found!" + Environment.NewLine + String.Join(Environment.NewLine, messages));
		}
	}
}
