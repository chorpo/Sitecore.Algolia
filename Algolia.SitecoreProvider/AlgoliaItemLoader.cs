using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.ContentSearch;
using Sitecore.Data.Items;

namespace Algolia.SitecoreProvider
{
    public class AlgoliaItemLoader
    {
        public dynamic Load(IIndexable indexable)
        {
            var item = (Item) (indexable as SitecoreIndexableItem);

            //var tags = new List<string>();
            //tags.Add("image");

            //if (!string.IsNullOrEmpty(item.GpId))
            //    tags.Add("gpId_" + item.GpId);

            //tags.Add("owner_" + item.UserId);
            //tags.Add("tripId_" + item.TripId);
            //foreach (var connectionId in item.UsersCanSee)
            //{
            //    tags.Add("connection_" + connectionId);
            //}

            dynamic w = new ExpandoObject();

            w.name = item.Name;
            w.path = item.Paths.Path;
            w.id = item.ID.ToGuid().ToString();

            return w;
        }
    }
}
