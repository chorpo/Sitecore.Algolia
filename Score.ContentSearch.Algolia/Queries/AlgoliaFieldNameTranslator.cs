using System;
using System.Collections.Generic;
using Sitecore.ContentSearch;

namespace Score.ContentSearch.Algolia.Queries
{
    class AlgoliaFieldNameTranslator : AbstractFieldNameTranslator
    {
        public override Dictionary<string, List<string>> MapDocumentFieldsToType(Type type, MappingTargetType target, IEnumerable<string> documentFieldNames)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<string> GetTypeFieldNames(string fieldName)
        {
            throw new NotImplementedException();
        }
    }
}
