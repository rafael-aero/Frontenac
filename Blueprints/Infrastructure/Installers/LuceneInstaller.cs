﻿using System;
using System.IO;
using Castle.Facilities.Startable;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Frontenac.Infrastructure.Indexing;
using Frontenac.Infrastructure.Indexing.Indexers;
using Frontenac.Infrastructure.Indexing.Lucene;
using Frontenac.Infrastructure.Properties;
using Lucene.Net.Analysis;
using Lucene.Net.Contrib.Management;

namespace Frontenac.Infrastructure.Installers
{
    public class LuceneInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(

                Component.For<Indexer>()
                         .ImplementedBy<ObjectIndexer>()
                         .LifestyleTransient()
                         .DependsOn(Dependency.OnValue("maxDepth", Settings.Default.ObjectIndexerMaxDepth)),

                Component.For<IIndexerFactory>()
                         .AsFactory(),

                Component.For<Lucene.Net.Store.Directory>()
                         .UsingFactoryMethod(t =>
                         {
                             var pathProvider = t.Resolve<IDatabasePathProvider>();
                             var databasePath = pathProvider.GetPath();
                             t.ReleaseComponent(pathProvider);

                             var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Concat(databasePath, "\\Lucene"));
                             return LuceneIndexingService.CreateMMapDirectory(path);
                         }),

                Component.For<Analyzer>()
                         .ImplementedBy<KeywordAnalyzer>(),

                Component.For<NrtManager>(),

                Component.For<IIndexCollectionFactory>()
                         .ImplementedBy<IndexCollectionFactory>(),

                Component.For<IIndexCollectionFactory>()
                         .ImplementedBy<TransactionalIndexCollectionFactory>()
                         .Named("TransactionalIndexCollectionFactory"),

                Component.For<IndexingService>()
                         .Forward<LuceneIndexingService>()
                         .ImplementedBy<LuceneIndexingService>()
                         .Named("IndexingService"),

                Component.For<IndexingService>()
                         .Forward<LuceneIndexingService>()
                         .LifestyleTransient()
                         .DependsOn(Dependency.OnComponent("indexCollectionFactory", "TransactionalIndexCollectionFactory"))
                         .Named("TransactionalIndexingService"),

                Component.For<IndexingServiceComponentSelector>(),

                Component.For<TransactionaIndexingServiceComponentSelector>(),

                Component.For<IIndexingServiceFactory>()
                         .AsFactory(c => c.SelectedWith<IndexingServiceComponentSelector>())
                         .Named("IndexingServiceFactory"),

                Component.For<IIndexingServiceFactory>()
                         .AsFactory(c => c.SelectedWith<TransactionaIndexingServiceComponentSelector>())
                         .Named("TransactionalIndexingServiceFactory"),

                Component.For<NrtManagerReopener>()
                         .UsingFactoryMethod(input =>
                             new NrtManagerReopener(container.Resolve<NrtManager>(),
                                 TimeSpan.FromSeconds(LuceneIndexingServiceParameters.Default.MaxStaleSeconds),
                                 TimeSpan.FromMilliseconds(LuceneIndexingServiceParameters.Default.MinStaleMilliseconds),
                                 LuceneIndexingServiceParameters.Default.CloseTimeoutSeconds))
                         .Start()
                );
        }
    }

    public interface IDatabasePathProvider
    {
        string GetPath();
    }

    public class IndexingServiceComponentSelector : DefaultTypedFactoryComponentSelector
    {
        protected override string GetComponentName(System.Reflection.MethodInfo method, object[] arguments)
        {
            return "IndexingService";
        }
    }

    public class TransactionaIndexingServiceComponentSelector : DefaultTypedFactoryComponentSelector
    {
        protected override string GetComponentName(System.Reflection.MethodInfo method, object[] arguments)
        {
            return "TransactionalIndexingService";
        }
    }
}