using System;
using System.Configuration;

namespace Thinktecture.Relay.OnPremiseConnectorService.Configuration
{
	public class OnPremiseTargetCollection : ConfigurationElementCollection
	{
		public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.BasicMap;

		protected override ConfigurationElement CreateNewElement(string elementName)
		{
			switch (elementName)
			{
				case "web":
					return new OnPremiseWebTargetElement();
				case "inProc":
					return new OnPremiseInProcTargetElement();
				default:
					throw new ConfigurationErrorsException($"Invalid element name: {elementName}");
			}
		}

		protected override ConfigurationElement CreateNewElement()
		{
			throw new NotSupportedException();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((OnPremiseTargetElement)element).Key;
		}

		protected override bool IsElementName(string elementName)
		{
			switch (elementName)
			{
				case "web":
				case "inProc":
					return true;
				default:
					return base.IsElementName(elementName);
			}
		}
	}
}
