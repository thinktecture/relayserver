using System.Linq;
using Autofac;
using Autofac.Core;
using NLog;

namespace Thinktecture.Relay.Server.Logging
{
	public class LoggingModule : Autofac.Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			base.Load(builder);

			builder.RegisterInstance(LogManager.GetLogger("GenericLogger"))
				.As<ILogger>().SingleInstance();
		}

		protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry,
			IComponentRegistration registration)
		{
			// Handle constructor parameters.
			registration.Preparing += OnComponentPreparing;
		}

		private static void OnComponentPreparing(object sender, PreparingEventArgs e)
		{
			e.Parameters = e.Parameters.Union(
				new[]
				{
					new ResolvedParameter(
						(p, i) => p.ParameterType == typeof(ILogger),
						(p, i) => LogManager.GetLogger(p.Member.DeclaringType.Name)
					),
				});
		}
	}
}
