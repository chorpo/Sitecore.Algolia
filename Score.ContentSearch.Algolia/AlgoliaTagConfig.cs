using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Score.ContentSearch.Algolia
{
    public class AlgoliaTagConfig
    {
        public string FieldName { get; set; }
        public bool HideField { get; set; }
        public string TagPreffix { get; set; }
    }
}
