using System;
using System.Reflection;
using Serilog;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;

namespace Thinktecture.Relay.OnPremiseConnector.SignalR
{
	public class RelayServerConnectionConfig
	{
		public RelayServerConnectionConfig(Assembly versionAssembly, String userName, String password, Uri relayServerUri, TimeSpan requestTimeout, TimeSpan tokenRefreshWindow, Int32 minConnectWaitTimeInSeconds, Int32 maxConnectWaitTimeInSeconds)
		{
			VersionAssembly = versionAssembly;
			UserName = userName;
			Password = password;
			RelayServerUri = relayServerUri;
			RequestTimeout = requestTimeout;
			TokenRefreshWindow = tokenRefreshWindow;
			MinConnectWaitTimeInSeconds = minConnectWaitTimeInSeconds;
			MaxConnectWaitTimeInSeconds = maxConnectWaitTimeInSeconds;
		}

		public Assembly VersionAssembly { get; private set; }
		public String UserName { get; private set; }
		public String Password { get; private set; }
		public Uri RelayServerUri { get; private set; }
		public TimeSpan RequestTimeout { get; private set; }
		public TimeSpan TokenRefreshWindow { get; private set; }
		public int MinConnectWaitTimeInSeconds;
		public int MaxConnectWaitTimeInSeconds;
	}
}
