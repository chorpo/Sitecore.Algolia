using System;
using System.Collections.Generic;
using Sitecore.ContentSearch;

namespace Score.ContentSearch.Algolia.Queries
{
    class AlgoliaFieldNameTranslator : AbstractFieldNameTranslator
    {
        public override IEnumerable<string> GetTypeFieldNames(string fieldName)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, List<string>> MapFieldsToType(IEnumerable<string> fieldNames, Type type, MappingTargetType target)
        {
            throw new NotImplementedException();
        }
    }
}
