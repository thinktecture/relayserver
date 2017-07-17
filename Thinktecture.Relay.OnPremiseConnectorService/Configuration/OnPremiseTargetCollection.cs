using System;
using System.Configuration;

namespace Thinktecture.Relay.OnPremiseConnectorService.Configuration
{
    public class OnPremiseTargetCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override ConfigurationElement CreateNewElement(string elementName)
        {
            switch (elementName)
            {
                case "web":
                    return new OnPremiseWebTargetElement();
                case "inProc":
                    return new OnPremiseInProcTargetElement();
            }

            throw new ConfigurationErrorsException(String.Format("Invalid element name: {0}", elementName));
        }

        protected override ConfigurationElement CreateNewElement()
        {
            throw new NotImplementedException();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((OnPremiseTargetElement) element).Key;
        }

        protected override bool IsElementName(string elementName)
        {
            switch (elementName)
            {
                case "web":
                case "inProc":
                    return true;
            }

            return base.IsElementName(elementName);
        }
    }
}
