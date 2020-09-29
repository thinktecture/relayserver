using System;
using System.Collections.Generic;
using System.Linq;

namespace Thinktecture.Relay.Server.Helper
{
	internal class UrlParameterFilter
	{
		private readonly List<string> _queryKeys = new List<string>();

		private string _path;
		private bool _wrongQueryStart;

		private string Separator => _wrongQueryStart ? "&" : "?";

		public UrlParameterFilter(string url) => ParseUrl(url);

		private void ParseUrl(string url)
		{
			var queryStart = url.IndexOf('?');

			// check if we have a malformed url without an ? before an &
			if (queryStart <= 0 && url.IndexOf('&') >= 0)
			{
				_wrongQueryStart = true;
				queryStart = url.IndexOf('&');
			}

			_path = url.Substring(0, queryStart > 0 ? queryStart : url.Length);
			if (queryStart >= 0)
			{
				ParseQuery(url.Substring(queryStart + 1));
			}
		}

		private void ParseQuery(string queryString)
		{
			var parts = queryString.Split('&');

			foreach (var part in parts)
			{
				var element = part.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
				if (!_queryKeys.Contains(element[0]))
				{
					_queryKeys.Add(element[0]);
				}
			}
		}

		public override string ToString()
		{
			return (_queryKeys.Any())
				? $"{_path}{Separator}{String.Join("&", _queryKeys.Select(v => $"{v}=***").ToArray())}"
				: _path;
		}
	}
}
