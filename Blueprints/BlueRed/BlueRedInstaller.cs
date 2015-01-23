﻿using System.Diagnostics.Contracts;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Impls.TG;
using Frontenac.Infrastructure;
using Frontenac.Infrastructure.Indexing;
using Frontenac.Infrastructure.Indexing.Indexers;
using Frontenac.Infrastructure.Indexing.Lucene;
using Frontenac.Infrastructure.Serializers;
using StackExchange.Redis;

namespace Frontenac.BlueRed
{
    public static class BlueRedInstaller
    {
        public static void SetupBlueRed(this IContainer container)
        {
            Contract.Requires(container != null);

            container.Register(LifeStyle.Singleton, typeof(ObjectIndexer), typeof(Indexer));
            container.Register(LifeStyle.Singleton, typeof(DefaultIndexerFactory), typeof(IIndexerFactory));
            container.Register(LifeStyle.Singleton, typeof(DefaultGraphFactory), typeof(IGraphFactory));

            container.Register(LifeStyle.Singleton, typeof(JsonContentSerializer), typeof(IContentSerializer));

            container.Register(LifeStyle.Transient, typeof(LuceneIndexingService), typeof(IndexingService));

            container.Register(ConnectionMultiplexer.Connect("localhost:6379"), typeof(ConnectionMultiplexer));

            //passer config par factory a la place
            container.Register(LifeStyle.Singleton, typeof(RedisGraphConfiguration), typeof(IGraphConfiguration));

            container.Register(LifeStyle.Transient, typeof(RedisGraph), typeof(IGraph), typeof(IKeyIndexableGraph), typeof(IIndexableGraph));
        }
    }
}
