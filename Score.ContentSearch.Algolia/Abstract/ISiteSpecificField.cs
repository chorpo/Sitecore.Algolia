using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Score.ContentSearch.Algolia.Abstract
{
    public interface ISiteSpecificField
    {
        string Site { get; set; }
    }
}
