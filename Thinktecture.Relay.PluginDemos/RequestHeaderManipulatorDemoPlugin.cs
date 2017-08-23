using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using NLog;
using Thinktecture.Relay.Server.OnPremise;
using Thinktecture.Relay.Server.Plugins;

namespace Thinktecture.Relay.PluginDemos
{
	/// <inheritdoc />
	/// <summary>
	/// Example implementation of a plugin that modifies the request headers
	/// </summary>
	public class RequestHeaderManipulatorDemoPlugin : IRequestHeaderManipulator
	{
		private readonly ILogger _logger;

		/// <summary>
		/// Creates a new instance of the <see cref="RequestHeaderManipulatorDemoPlugin"/>
		/// </summary>
		/// <param name="logger">An instance of an <see cref="ILogger"/>, that will be injected by Autofac when this plugin is created</param>
		public RequestHeaderManipulatorDemoPlugin(ILogger logger)
		{
			// You can also have the DI inject different custom dependencies, as long as
			// they are all registered in your plugins Autofac module
			_logger = logger;
		}

		public IDictionary<string, string> HandleHeaders(IOnPremiseConnectorRequest onPremiseConnectorRequest, out HttpResponseMessage response)
		{
			// set the response to null
			response = null;
			_logger.Debug("RequestHeaderManipulator is called. Adding a header.");

			var result = onPremiseConnectorRequest.HttpHeaders.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			result.Add("X-ThinkectureRelay-Example", "Modified by plugin");

			return result;
		}
	}
}
