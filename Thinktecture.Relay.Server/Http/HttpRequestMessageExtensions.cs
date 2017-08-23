using System.Net.Http;
using System.ServiceModel.Channels;
using System.Web;
using Microsoft.Owin;

namespace Thinktecture.Relay.Server.Http
{
	internal static class HttpRequestMessageExtensions
	{
		public static string GetClientIp(this HttpRequestMessage request)
		{
			if (request.Properties.ContainsKey("MS_OwinContext"))
			{
				return ((OwinContext)request.Properties["MS_OwinContext"]).Request.RemoteIpAddress;
			}

			if (request.Properties.ContainsKey("MS_HttpContext"))
			{
				return ((HttpContextWrapper)request.Properties["MS_HttpContext"]).Request.UserHostAddress;
			}

			if (request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
			{
				RemoteEndpointMessageProperty prop = (RemoteEndpointMessageProperty)request.Properties[RemoteEndpointMessageProperty.Name];
				return prop.Address;
			}

			if (HttpContext.Current != null)
			{
				return HttpContext.Current.Request.UserHostAddress;
			}

			return null;
		}
	}
}
