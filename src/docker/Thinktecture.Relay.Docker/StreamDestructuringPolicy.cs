using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Serilog.Core;
using Serilog.Events;

namespace Thinktecture.Relay.Docker
{
	public class StreamDestructuringPolicy : IDestructuringPolicy
	{
		public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory,
			[MaybeNullWhen(false)] out LogEventPropertyValue result)
		{
			if (value is Stream stream)
			{
				var destructed = new Dictionary<ScalarValue, LogEventPropertyValue>()
				{
					{ new ScalarValue("CanRead"), new ScalarValue(stream.CanRead) },
					{ new ScalarValue("CanWrite"), new ScalarValue(stream.CanWrite) },
					{ new ScalarValue("CanSeek"), new ScalarValue(stream.CanSeek) },
					{ new ScalarValue("CanTimeout"), new ScalarValue(stream.CanTimeout) }
				};

				if (stream.CanTimeout)
				{
					destructed.Add(new ScalarValue("ReadTimeout"), new ScalarValue(stream.ReadTimeout));
					destructed.Add(new ScalarValue("WriteTimeout"), new ScalarValue(stream.WriteTimeout));
				}

				if (stream.CanSeek)
				{
					destructed.Add(new ScalarValue("Length"), new ScalarValue(stream.Length));
					destructed.Add(new ScalarValue("Position"), new ScalarValue(stream.Position));
				}

				result = new DictionaryValue(destructed);
				return true;
			}

			result = null;
			return false;
		}
	}
}
