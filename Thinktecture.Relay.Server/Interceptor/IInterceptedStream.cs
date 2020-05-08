using System.Collections.Generic;
using System.IO;

namespace Thinktecture.Relay.Server.Interceptor
{
	internal interface IInterceptedStream
	{
		Stream Stream { get; set; }

		byte[] Body { get; set; }

		long ContentLength { get; set; }

		IReadOnlyDictionary<string, string> HttpHeaders { get; set; }
	}
}
