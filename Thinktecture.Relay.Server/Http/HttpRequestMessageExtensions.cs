using System;
using System.Net;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Threading;
using System.Web;
using Microsoft.Owin;

namespace Thinktecture.Relay.Server.Http
{
	internal static class HttpRequestMessageExtensions
	{
		private const string MS_OwinContext = "MS_OwinContext";

		public static IPAddress GetRemoteIpAddress(this HttpRequestMessage request)
		{
			string ip = null;

			if (request.Properties.ContainsKey(MS_OwinContext))
			{
				ip = ((OwinContext)request.Properties[MS_OwinContext]).Request.RemoteIpAddress;
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

		public static CancellationToken GetCallCancelled(this HttpRequestMessage request)
		{
			return request.Properties.TryGetValue(MS_OwinContext, out object owinContext)
				? ((OwinContext)owinContext).Request.CallCancelled
				: CancellationToken.None;
		}
	}
}
