using System;
using System.Collections.Generic;
using System.Linq;

namespace Thinktecture.Relay.Server.Diagnostics
{
    public class TraceFile
    {
        private static readonly IEnumerable<string> ViewableContentTypes = new[]
        {
            "text/",
            "application/json"
        };

        private IDictionary<string, string> _caseInsensitiveHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public string HeaderFileName { get; set; }
        public string ContentFileName { get; set; }

        public IDictionary<string, string> Headers
        {
            get { return _caseInsensitiveHeaders; }
            set
            {
                _caseInsensitiveHeaders = new Dictionary<string, string>(value, StringComparer.OrdinalIgnoreCase);
            }
        }

        public byte[] Content { get; set; }

        public bool IsViewable
        {
            get { return ContentIsViewable(); }
        }

        public bool IsContentAvailable
        {
            get { return CheckIfContentIsAvailable(); }
        }

        private bool CheckIfContentIsAvailable()
        {
            string tmp;

            if (!Headers.TryGetValue("content-length", out tmp))
            {
                return false;
            }

            long contentLength;

            if (!long.TryParse(tmp, out contentLength))
            {
                return false;
            }

            return contentLength > 0;
        }

        private bool ContentIsViewable()
        {
            string contentType;
            if (!Headers.TryGetValue("content-type", out contentType))
            {
                return false;
            }

            return ViewableContentTypes.Any(viewableContentType => contentType.Contains(viewableContentType.ToLowerInvariant()));
        }
    }
}