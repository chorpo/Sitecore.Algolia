﻿using System;
using NUnit.Framework;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Maintenance;
using Sitecore.Data;
using Sitecore.FakeDb;

namespace Score.ContentSearch.Algolia.Tests.Integrations
{
    [TestFixture]
    [Category("Integrated")]
    public class AddItemsIntegrations
    {
        private DbItem _source;

        private Guid testItemId = Guid.Parse("764A2503-59CA-4825-983C-A19000F44797");

        [SetUp]
        public void SetUp()
        {
            _source = new DbItem("source", new ID(testItemId)) { new DbItem("child") };
        }

        [Test, Ignore]
        public void AddItemsIntegration()  
        {
            // arrange
            using (var db = new Db {_source})
            {
                var item = db.GetItem("/sitecore/content/source");
                var indexable = new SitecoreIndexableItem(item);
                var index = new AlgoliaSearchIndex("algolia_master_index", "3Q92VD0BCR", "8ae3d3950e531a4be7d32a3e58bb2eea", "test");
                var context = index.CreateUpdateContext();
                
                var operations = new AlgoliaIndexOperations(index);

                //Act
                operations.Update(indexable, context, new ProviderIndexConfiguration());
                context.Commit();
                //Assert
            }
        }
    }
}
