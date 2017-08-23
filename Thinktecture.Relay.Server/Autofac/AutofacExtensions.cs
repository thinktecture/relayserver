using System;
using System.Reflection;
using Autofac;
using Autofac.Builder;

namespace Thinktecture.Relay.Server.Autofac
{
	static class AutofacExtensions
	{
		public static void InjectProperties(IComponentContext context, object instance, bool overrideSetValues)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}
			if (instance == null)
			{
				throw new ArgumentNullException(nameof(instance));
			}

			foreach (var propertyInfo in instance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
			{
				var propertyType = propertyInfo.PropertyType;

				if ((!propertyType.IsValueType || propertyType.IsEnum) && (propertyInfo.GetIndexParameters().Length == 0) && context.IsRegistered(propertyType))
				{
					var accessors = propertyInfo.GetAccessors(true);
					if (((accessors.Length != 1) || !(accessors[0].ReturnType != typeof(void))) && (overrideSetValues || (accessors.Length != 2) || (propertyInfo.GetValue(instance, null) == null)))
					{
						var obj = context.Resolve(propertyType);
						propertyInfo.SetValue(instance, obj, null);
					}
				}
			}
		}

		public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InjectPropertiesAsAutowired<TLimit, TActivatorData, TRegistrationStyle>(
			this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registration)
		{
			return registration.OnActivated(args => InjectProperties(args.Context, args.Instance, true));
		}
	}
}
