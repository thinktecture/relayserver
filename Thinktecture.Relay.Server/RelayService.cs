using System;
using Microsoft.Owin.Hosting;

namespace Thinktecture.Relay.Server
{
	internal class RelayService
	{
	    private readonly string _hostName;
	    private readonly int _port;
	    private readonly bool _allowHttp;

	    private IDisposable _host;

		public RelayService(string hostName, int port, bool allowHttp)
		{
		    _hostName = hostName;
		    _port = port;
		    _allowHttp = allowHttp;
		}

	    public void Start()
	    {
	        var address = String.Format("{0}://{1}:{2}", _allowHttp ? "http" : "https", _hostName, _port);

#if DEBUG
            Console.WriteLine("Listing on: {0}", address);
#endif

            _host = WebApp.Start<Startup>(address);
		}

		public void Stop()
		{
			if (_host != null)
			{
				_host.Dispose();
				_host = null;
			}
		}
	}
}
