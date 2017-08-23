using System;
using System.Net;
using System.Net.Http;
using Thinktecture.Relay.Server.OnPremise;
using Thinktecture.Relay.Server.Plugins;

namespace Thinktecture.Relay.PluginDemos
{
	/// <inheritdoc />
	/// <summary>
	/// Example implementation of a plugin that modifies the request method
	/// </summary>
	public class RequestMethodManipulatorDemoPlugin : IRequestMethodManipulator
	{
		public string HandleMethod(IOnPremiseConnectorRequest request, out HttpResponseMessage response)
		{
			response = null;

			// If a PUT is received, we immidiately reject and tell the user to use PATCH instead
			if (string.Equals(request.HttpMethod, HttpMethod.Put.Method, StringComparison.InvariantCultureIgnoreCase))
			{
				response = new HttpResponseMessage(HttpStatusCode.MethodNotAllowed)
				{
					ReasonPhrase = "Use PATCH instead of PUT",
				};
			}

			// If a PATCH is received, we internally change that to PUT
			if (string.Equals(request.HttpMethod, "PATCH", StringComparison.InvariantCultureIgnoreCase))
				return HttpMethod.Put.Method;

			return null;
		}
	}
}
