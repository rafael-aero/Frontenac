Frontenac
=========

A .NET port of the [Tinkerpop Stack](http://www.tinkerpop.com/).

## News
* 2015-07-30
 * Frontenac.Blueprints 2.4 released.
 * New GraphJson format support.
 * Geographic objects added to store and query GeoPoints.
 * IVertex.AddEdge now takes an optional id parameter.
 * New Redis Graph Library
 * New ElasticSearch Indexing Service

* 2014-05-10
 * Frontenac.Blueprints 2.3.10 released.
 * GraphBackedTypeProvider now supports multiple graph instances correctly.
 * The default column name for type information vertices is now __g_type__, due to a name clash.
 * New Gremlinq AddVertex overloads that allow to specify the id.
 * NuGet package now only includes .NET 4.0 assemblies. A new package for .NET 4.5 will be released soon.
 * Grave Transactions now serialize write access to LuceneIndex.
 * Introduction of GraveInstance class for better separation of concerns.

* 2014-03-29
 * Frontenac.Blueprints 2.3.9 released.
 * Gremlinq ORM now supports relations. You can user IEnumerable&lt;T&gt; to read, and ICollection&lt;T&gt; to add/remove edges.
 * Frontenac.Gremlinq.dll is now automatically references when installing nuget package.

* 2014-03-22
 * Frontenac.Blueprints 2.3.8 released.
 * Half of [Gremlinq documentation](https://github.com/Loupi/Frontenac/wiki/Gremlinq) has been written.
 * Breaking change: IElement has a new Graph property that returns the element's graph.
 * IElementTypeProvider was renamed to ITypeProvider.
 * Introduction of IProxyFactory to separate proxy creation from type type provider.
 * Introduction of DictionaryAdapterProxy factory, which uses Castle DictionaryAdapter to create proxies.
 * TinkerGraph is now thread safe (uses ConcurrentDictionary for all it's collections).
 * GremlinqContext is now automatically created with GraphTypeProvider and DictionaryAdapterProxyFactory.
 * IGremlinqContextFactory, ITypePovider and IProxyFactory interfaces are used to customize context creation. 
 * GraphBackedTypeProvider automatically adjusts itself to the current graph.
 * New Gremlinq OfType override.
 
* 2014-03-10
 * Frontenac.Blueprints 2.3.7 released.
 * Gremlinq proxy types are now stored in a more lightweight manner
 * Introduces the IElementTypeProvider interface to instantiate proxies and get/store types
 * Added a DictionaryElementTypeProvider that stores proxy types as integers on elements. It is initialized with a Dictionary<int, Type> on which you have control.
 * Added a GraphBackedElementTypeProvider that stores types as simple ids on elements, and also store the type names in the graph.
 * Corrected a missing Nuget dependency to Castle.Core.1
 
* 2014-03-09
 * Frontenac.Blueprints 2.3.6 released.
 * Gremling (alpha) library now included in Nuget Package.
 * Available operators are: Both, In, InE, Loop, Map, Out, OutE, V.
 * Added proxy helpers to Gremlinq to ease mapping of entitites to edges and vertices.
 
* 2013-12-09
 * Frontenac.Blueprints 2.3.5 released.
 * IElement now inherits from IDictionary<string, object> interface.
 * Added DictionaryElement class to ease implementation of IElement.

* 2013-09-09 
 * Frontenac.Blueprints 2.3.4 released.
 * Blueprint interfaces are now enforced with code contracts.
 
* 2013-08-25 
 * Frontenac.Blueprints 2.3.3 released.
 * Initial commit for Grave, a graph database backed by Microsoft ESENT and Lucent.NET.

* 2013-07-29 
 * [VelocityDB](http://velocitydb.com/) now supports Frontenac through it's [VelocityGraph](https://github.com/VelocityDB/VelocityGraph) library. Available on NuGet.

## What is inside
For now, blueprints-core 2.3.0 has been ported. It is not production ready yet. It includes
* [Property Graph Model](https://github.com/tinkerpop/blueprints/wiki/Property-Graph-Model)
* Implementations
  * [TinkerGraph](https://github.com/tinkerpop/blueprints/wiki/TinkerGraph)
* Utilities
  * Import/Export
     * [GML Reader and Writer Library](https://github.com/tinkerpop/blueprints/wiki/GML-Reader-and-Writer-Library)
     * [GraphML Reader and Writer Library](https://github.com/tinkerpop/blueprints/wiki/GraphML-Reader-and-Writer-Library)
     * [GraphSON Reader and Writer Library](https://github.com/tinkerpop/blueprints/wiki/GraphSON-Reader-and-Writer-Library)
  * Wrappers
     * [Batch Implementation](https://github.com/tinkerpop/blueprints/wiki/Batch-Implementation)
     * [ReadOnly Implementation](https://github.com/tinkerpop/blueprints/wiki/ReadOnly-Implementation)
     * [Event Implementation](https://github.com/tinkerpop/blueprints/wiki/Event-Implementation)
     * [Partition Implementation](https://github.com/tinkerpop/blueprints/wiki/Partition-Implementation)
     * [Id Implementation](https://github.com/tinkerpop/blueprints/wiki/Id-Implementation)

The [Property Graph Model Test Suite](https://github.com/tinkerpop/blueprints/wiki/Property-Graph-Model-Test-Suite) is currently being ported. As of now, 357/357 tests pass. 

## What's Next?
There is still a lot more work that must be done in order to get a full stack implemented in .NET.
Making sure it's root component is solid is fundamental. This is why, before proceeding further on with the port of the other APIs, the test suite must be complete and more than the TinkerGraph implementation must pass through it.  

## Why that name?
This is in reference to the [Château Frontenac](http://en.wikipedia.org/wiki/Chateau_Frontenac) in Quebec City, Canada, because writing softwares is a bit like building castles.

## Credits
Special thanks to the authors and contributors of the original library:
* [alexaverbuch](http://www.github.com/alexaverbuch/)(Alex Averbuch)
* [bdeggleston](http://www.github.com/bdeggleston/)(Blake Eggleston)
* [BrynCooke](http://www.github.com/BrynCooke/)(Bryn Cooke)
* [espeed](http://www.github.com/espeed/)(James Thornton)
* [joshsh](http://www.github.com/joshsh/)(Joshua Shinavier)
* [mbroecheler](http://www.github.com/mbroecheler/)(Matthias Broecheler)
* [okram](http://www.github.com/okram/)(Marko A. Rodriguez)
* [pangloss](http://www.github.com/pangloss/)(Darrick Wiebe)
* [peterneubauer](http://www.github.com/peterneubauer/)(Peter Neubauer)
* [pierredewilde](http://www.github.com/pierredewilde/)(Pierre De Wilde)
* [spmallette](http://www.github.com/spmallette/)(stephen mallette)
* [xedin](http://www.github.com/xedin/)(Pavel Yaskevich)
* and others :)
