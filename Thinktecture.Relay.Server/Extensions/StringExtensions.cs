// ReSharper disable once CheckNamespace
namespace System
{
	internal static class StringExtensions
	{
		public static string StripQueryString(this string url)
		{
			if (url == null)
			{
				throw new ArgumentNullException(nameof(url));
			}

			var queryStart = url.IndexOf('?');

			// check if we have a malformed url without an ? before an &
			if (queryStart < 0 && url.IndexOf('&') >= 0)
			{
				queryStart = url.IndexOf('&');
			}

			return url.Substring(0, queryStart >= 0 ? queryStart : url.Length);
		}
	}
}
