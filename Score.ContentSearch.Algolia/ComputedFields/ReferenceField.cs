using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.ComputedFields;

namespace Score.ContentSearch.Algolia.ComputedFields
{
    public class ReferenceField: IComputedIndexField
    {
        public object ComputeFieldValue(IIndexable indexable)
        {
            throw new NotImplementedException();
        }

        public string FieldName { get; set; }
        public string ReturnType { get; set; }
    }
}
