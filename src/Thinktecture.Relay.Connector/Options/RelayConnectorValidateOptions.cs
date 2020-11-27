using System;
using System.Linq;
using Microsoft.Extensions.Options;

namespace Thinktecture.Relay.Connector.Options
{
	internal class RelayConnectorValidateOptions : IValidateOptions<RelayConnectorOptions>
	{
		public ValidateOptionsResult Validate(string name, RelayConnectorOptions options)
		{
			if (options.Targets == null || options.Targets.Count == 0)
			{
				return ValidateOptionsResult.Success;
			}

			var missingType = options.Targets.Where(kvp => !kvp.Value.ContainsKey(Constants.RelayConnectorOptionsTargetType))
				.Select(kvp => kvp.Key).ToArray();
			if (missingType.Length != 0)
			{
				return ValidateOptionsResult.Fail(
					$"The following targets have no \"{Constants.RelayConnectorOptionsTargetType}\" provided: \"{string.Join("\", \"", missingType)}\"");
			}

			var unknownType = options.Targets.Where(kvp => Type.GetType(kvp.Value[Constants.RelayConnectorOptionsTargetType]) == null)
				.Select(kvp => kvp.Key).ToArray();
			if (unknownType.Length != 0)
			{
				return ValidateOptionsResult.Fail(
					$"The following targets have an invalid type provided: \"{string.Join("\", \"", missingType)}\"");
			}

			return ValidateOptionsResult.Success;
		}
	}
}
