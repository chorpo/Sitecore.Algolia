# Sitecore search provider for Algolia

Algolia is a Hosted Search API that delivers instant and relevant results. This project implements Search Provider for Algolia in Sitecore.

## How to setup

Find right Nuget package for your Sitecore version. Currenly project supports all Sitecore versions from 7.0 to 8.1 you can find versions matrix below:

* [Algolia.Sitecore.8.2](https://www.nuget.org/packages/Algolia.Sitecore.8.2/) for Sitecore 8.2
* [Algolia.Sitecore.8.1](https://www.nuget.org/packages/Algolia.Sitecore.8.1/) for Sitecore 8.1
* [Algolia.Sitecore.8](https://www.nuget.org/packages/Algolia.Sitecore.8/) for Sitecore 8.0
* [Algolia.Sitecore.7](https://www.nuget.org/packages/Algolia.Sitecore.7/) for Sitecore 7.0-7.2

Add Nuget package to your Visual Studio web project for Sitecore. Package will add all required DLLs. Config files should be implemented manually.

Below you can find instructions for configs setup applicable for Sitecore 8.1. Different Sitecore versions may requre slightly different configuration. Use default Lucene search config files to identify these variations.

Test project includes sample configuration files:
* [Index Configuration](https://github.com/dharnitski/Sitecore.Algolia/blob/develop/Score.ContentSearch.Algolia.Tests/SampleConfiguration/AlgoliaTestConfiguration.Sc81.config) 
* [Index Definition](https://github.com/dharnitski/Sitecore.Algolia/blob/develop/Score.ContentSearch.Algolia.Tests/SampleConfiguration/AlgoliaTestIndex.Sc8.config) 

Copy these files into *site-root/App_Config/Include* folder. Now we need to make some changes to connect index with your Algolia account and filter data to include only relevant content.

### Setup Index Configuration

Find commented *include* section 

    <!--<include hint="list:AddIncludedTemplate">
        <HomePage>{9CAAECFD-3BEB-44B1-9BE5-F7E30811EF2D}</HomePage>
        <ContentPage>{520A275F-6104-4690-8BCD-36B86BAD8D4E}</ContentPage>
    </include>-->

Uncomment it and replace Ids in that section with Ids of templates you want to index. Typically that will be templates for pages.

### Setup Index Definition

Index has some parameters required for Algolia connection:

* applicationId - Application ID
* fullApiKey - Admin API Key
* indexName - Index Name.

All these values defined in [Algolia Api Keys](https://www.algolia.com/api-keys) page.

    <Site>website</Site>

Site parameter stores name of your site. That value will be used to define some site specific values like *targetHostName*.

    <strategies hint="list:AddStrategy">
        <strategy ref="contentSearch/indexConfigurations/indexUpdateStrategies/syncMaster" />
    </strategies>

Strategy is used to control what causes by Sitecore to update data in search index. This is standard Sitecore config and you can find more information about that in [John West's blog post](http://www.sitecore.net/learn/blogs/technical-blogs/john-west-sitecore-blog/posts/2013/04/sitecore-7-index-update-strategies.aspx) 

    <locations hint="list:AddCrawler">
        <crawler type="Sitecore.ContentSearch.SitecoreItemCrawler, Sitecore.ContentSearch">
            <Database>master</Database>
            <Root>/sitecore/content/Home</Root>
        </crawler>
    </locations>

Crawler is a class that reads data from Sitecore database before it will be added to the index. This is also standard Sitecore configuration, although we need to do some changes here.
* Database - align this with strategy. *syncMaster* should work with *master* database and *onPublishEndAsync* should work with *web* or whatever is your publishing target database.
* Root - Crawling root item. Typically that will be homepage of your site  

## Why another provider?

Sitecore supports Lucene and Solr, other vendors have their engines like Coveo. So, why anybody wants to build one more provider?
Because it is different:

* Algolia is cool, blazing fast, requires no maintenance and easily configurable
* Algolia includes modern [instantsearch.js](https://community.algolia.com/instantsearch.js) UX Framefork that simplifies construction of custom Search pages   
* Search Provider designed for Domain search and Multitenancy


## Is there anything you cannot do with this provider?

Algolia is a razor comparing to swiss army knife. Index designed for speed and easy use and do not implement all the features that generic search engine has.
As a result Algolia cannot replace default Sitecore indexes.

This provider includes only crawling part (code that sends data to the index). Linq to Search is not included and not on the roadmap. 
To make it fast, search has to be run on client side. Check Algloa JS UI [instantsearch.js](https://community.algolia.com/instantsearch.js) or join me in [Algolia simple UI for Sitecore](https://github.com/dharnitski/Algolia.Sitecore.UI)  

