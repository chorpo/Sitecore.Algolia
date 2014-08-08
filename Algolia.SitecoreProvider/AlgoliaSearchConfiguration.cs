using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Sitecore.ContentSearch;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using Sitecore.Extensions;

namespace Algolia.SitecoreProvider
{
    /// <summary>
    /// Sitecore configuration for Algolia Index
    /// </summary>
    public class AlgoliaSearchConfiguration : ContentSearchConfiguration
    {
        public virtual void AddIndex(ISearchIndex index)
        {
            Assert.ArgumentNotNull((object)index, "index");
            this.Indexes[index.Name] = index;
            if (index.Configuration == null)
            {
                XmlNode configNode = this.factory.GetConfigNode(this.DefaultIndexConfigurationPath);
                if (configNode == null)
                    throw new ConfigurationException("Index must have a ProviderIndexConfiguration associated with it. Please check your config.");
                ProviderIndexConfiguration @object = this.factory.CreateObject<ProviderIndexConfiguration>(configNode);
                if (@object == null)
                    throw new ConfigurationException("Unable to create configuration object from path specified in setting 'ContentSearch.DefaultIndexConfigurationPath'. Please check your config.");
                index.Configuration = @object;
            }
            if (!index.Configuration.InitializeOnAdd)
                return;
            index.Initialize();
        }
    }
}
