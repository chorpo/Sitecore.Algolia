# Sitecore search provider for Algolia

Algolia is a Hosted Search API that delivers instant and relevant results. Search Provider implements crawling part of index integration.

## Why another provider?

Sitecore supports Lucene and Solr, other vendors have their engines like Coveo. So, why anybody wants to build one more provider?
Becasue it is different:

* Algolia is cool, blazing fast, requires no maintenance and easily configurable
* Algolia includes modern [instantsearch.js](https://community.algolia.com/instantsearch.js) UX Framefork that simplifies construction of custom Search pages   
* Search Provider designed for Domain search and Multitenancy


## Is there anything you cannot do with this provider?

Algolia is a razor comparing to swiss army knife. Index designed for speed and easy use and do not implement all the features that generic search engine has.
As a result Algolia cannot replace default Sitecore indexes.

