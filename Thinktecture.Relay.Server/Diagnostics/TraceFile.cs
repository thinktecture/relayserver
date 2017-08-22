using System;
using System.Collections.Generic;
using System.Linq;

namespace Thinktecture.Relay.Server.Diagnostics
{
	public class TraceFile
	{
		private static readonly IEnumerable<string> _viewableContentTypes = new[]
		{
			"text/",
			"application/json"
		};

		private IDictionary<string, string> _caseInsensitiveHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		public string HeaderFileName { get; set; }
		public string ContentFileName { get; set; }

		public IDictionary<string, string> Headers
		{
			get => _caseInsensitiveHeaders;
			set => _caseInsensitiveHeaders = new Dictionary<string, string>(value, StringComparer.OrdinalIgnoreCase);
		}

		public byte[] Content { get; set; }

		public bool IsViewable => ContentIsViewable();

		public bool IsContentAvailable => CheckIfContentIsAvailable();

		private bool CheckIfContentIsAvailable()
		{
			if (!Headers.TryGetValue("content-length", out var tmp))
				return false;

			if (!Int64.TryParse(tmp, out var contentLength))
				return false;

			return contentLength > 0;
		}

		private bool ContentIsViewable()
		{
			if (!Headers.TryGetValue("content-type", out var contentType))
				return false;

			return _viewableContentTypes.Any(viewableContentType => contentType.Contains(viewableContentType.ToLowerInvariant()));
		}
	}
}
