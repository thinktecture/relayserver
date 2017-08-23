using System;
using System.Net;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Web;
using Microsoft.Owin;

namespace Thinktecture.Relay.Server.Http
{
	internal static class HttpRequestMessageExtensions
	{
		public static IPAddress GetRemoteIpAddress(this HttpRequestMessage request)
		{
			string ip = null;

			if (request.Properties.ContainsKey("MS_OwinContext"))
			{
				ip = ((OwinContext)request.Properties["MS_OwinContext"]).Request.RemoteIpAddress;
			}
			else if (request.Properties.ContainsKey("MS_HttpContext"))
			{
				ip = ((HttpContextWrapper)request.Properties["MS_HttpContext"]).Request.UserHostAddress;
			}
			else if (request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
			{
				var prop = (RemoteEndpointMessageProperty)request.Properties[RemoteEndpointMessageProperty.Name];
				ip = prop.Address;
			}
			else if (HttpContext.Current != null)
			{
				ip = HttpContext.Current.Request.UserHostAddress;
			}

			return String.IsNullOrWhiteSpace(ip) ? null : IPAddress.Parse(ip);
		}
	}
}
