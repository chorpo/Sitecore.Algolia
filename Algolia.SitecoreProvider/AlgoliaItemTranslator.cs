using Newtonsoft.Json.Linq;
using Sitecore.ContentSearch;

namespace Algolia.SitecoreProvider
{
    public class AlgoliaItemTranslator
    {
        private readonly AlgoliaItemSerializer _serializer;
        private readonly AlgoliaItemLoader _loader;

        public AlgoliaItemTranslator()
        {
            _serializer = new AlgoliaItemSerializer();
            _loader = new AlgoliaItemLoader();
        }
        
        public JObject Translate(IIndexable indexable)
        {
            var data = _loader.Load(indexable);
            return _serializer.SerializeExpandoObject(data);
        }
    }
}
