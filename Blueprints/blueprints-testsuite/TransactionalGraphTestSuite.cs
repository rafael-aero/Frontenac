﻿using Frontenac.Blueprints.Impls;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Frontenac.Blueprints
{
    public abstract class TransactionalGraphTestSuite : TestSuite
    {
        protected TransactionalGraphTestSuite(GraphTest graphTest)
            : base("TransactionalGraphTestSuite", graphTest)
        {
        }

        [Test]
        public void TestRepeatedTransactionStopException()
        {
            var graph = (ITransactionalGraph)GraphTest.GenerateGraph();
            graph.Commit();
            graph.Rollback();
            graph.Commit();
            graph.Shutdown();
        }

        [Test]
        public void TestAutoStartTransaction()
        {
            var graph = (ITransactionalGraph)GraphTest.GenerateGraph();
            IVertex v1 = graph.AddVertex(null);
            VertexCount(graph, 1);
            Assert.AreEqual(v1.GetId(), graph.GetVertex(v1.GetId()).GetId());
            graph.Commit();
            VertexCount(graph, 1);
            Assert.AreEqual(v1.GetId(), graph.GetVertex(v1.GetId()).GetId());
            graph.Shutdown();
        }

        [Test]
        public void TestTransactionsForVertices()
        {
            var graph = (ITransactionalGraph)GraphTest.GenerateGraph();
            var vin = new List<IVertex>();
            var vout = new List<IVertex>();
            vin.Add(graph.AddVertex(null));
            graph.Commit();
            VertexCount(graph, 1);
            ContainsVertices(graph, vin);

            StopWatch();
            vout.Add(graph.AddVertex(null));
            VertexCount(graph, 2);
            ContainsVertices(graph, vin);
            ContainsVertices(graph, vout);
            graph.Rollback();

            ContainsVertices(graph, vin);
            VertexCount(graph, 1);
            PrintPerformance(graph.ToString(), 1, "vertex not added in failed transaction", StopWatch());

            StopWatch();
            vin.Add(graph.AddVertex(null));
            VertexCount(graph, 2);
            ContainsVertices(graph, vin);
            graph.Commit();
            PrintPerformance(graph.ToString(), 1, "vertex added in successful transaction", StopWatch());
            VertexCount(graph, 2);
            ContainsVertices(graph, vin);

            graph.Shutdown();
        }

        [Test]
        public void TestBasicVertexEdgeTransactions()
        {
            var graph = (ITransactionalGraph)GraphTest.GenerateGraph();
            IVertex v = graph.AddVertex(null);
            graph.AddEdge(null, v, v, ConvertId(graph, "self"));
            Assert.AreEqual(Count(v.GetEdges(Direction.In)), 1);
            Assert.AreEqual(Count(v.GetEdges(Direction.Out)), 1);
            Assert.AreEqual(v.GetEdges(Direction.In).First(), v.GetEdges(Direction.Out).First());
            graph.Commit();
            v = graph.GetVertex(v.GetId());
            Assert.AreEqual(Count(v.GetEdges(Direction.In)), 1);
            Assert.AreEqual(Count(v.GetEdges(Direction.Out)), 1);
            Assert.AreEqual(v.GetEdges(Direction.In).First(), v.GetEdges(Direction.Out).First());
            graph.Commit();
            v = graph.GetVertex(v.GetId());
            Assert.AreEqual(Count(v.GetVertices(Direction.In)), 1);
            Assert.AreEqual(Count(v.GetVertices(Direction.Out)), 1);
            Assert.AreEqual(v.GetVertices(Direction.In).First(), v.GetVertices(Direction.Out).First());
            graph.Commit();
            graph.Shutdown();
        }

        [Test]
        public void TestBruteVertexTransactions()
        {
            var graph = (ITransactionalGraph)GraphTest.GenerateGraph();
            List<IVertex> vin = new List<IVertex>(), vout = new List<IVertex>();
            StopWatch();
            for (int i = 0; i < 100; i++)
            {
                vin.Add(graph.AddVertex(null));
                graph.Commit();
            }
            PrintPerformance(graph.ToString(), 100, "vertices added in 100 successful transactions", StopWatch());
            VertexCount(graph, 100);
            ContainsVertices(graph, vin);

            StopWatch();
            for (int i = 0; i < 100; i++)
            {
                vout.Add(graph.AddVertex(null));
                graph.Rollback();
            }
            PrintPerformance(graph.ToString(), 100, "vertices not added in 100 failed transactions", StopWatch());

            VertexCount(graph, 100);
            ContainsVertices(graph, vin);
            graph.Rollback();
            VertexCount(graph, 100);
            ContainsVertices(graph, vin);


            StopWatch();
            for (int i = 0; i < 100; i++)
            {
                vin.Add(graph.AddVertex(null));
            }
            VertexCount(graph, 200);
            ContainsVertices(graph, vin);
            graph.Commit();
            PrintPerformance(graph.ToString(), 100, "vertices added in 1 successful transactions", StopWatch());
            VertexCount(graph, 200);
            ContainsVertices(graph, vin);

            StopWatch();
            for (int i = 0; i < 100; i++)
            {
                vout.Add(graph.AddVertex(null));
            }
            VertexCount(graph, 300);
            ContainsVertices(graph, vin);
            ContainsVertices(graph, vout.GetRange(100, 100));
            graph.Rollback();
            PrintPerformance(graph.ToString(), 100, "vertices not added in 1 failed transactions", StopWatch());
            VertexCount(graph, 200);
            ContainsVertices(graph, vin);
            graph.Shutdown();
        }

        [Test]
        public void TestTransactionsForEdges()
        {
            var graph = (ITransactionalGraph)GraphTest.GenerateGraph();

            IVertex v = graph.AddVertex(null);
            IVertex u = graph.AddVertex(null);
            graph.Commit();

            StopWatch();
            IEdge e = graph.AddEdge(null, graph.GetVertex(v.GetId()), graph.GetVertex(u.GetId()), ConvertId(graph, "test"));


            Assert.AreEqual(graph.GetVertex(v.GetId()), v);
            Assert.AreEqual(graph.GetVertex(u.GetId()), u);

            if (graph.GetFeatures().SupportsEdgeRetrieval)
                Assert.AreEqual(graph.GetEdge(e.GetId()), e);

            VertexCount(graph, 2);
            EdgeCount(graph, 1);

            graph.Rollback();
            PrintPerformance(graph.ToString(), 1, "edge not added in failed transaction (w/ iteration)", StopWatch());

            Assert.AreEqual(graph.GetVertex(v.GetId()), v);
            Assert.AreEqual(graph.GetVertex(u.GetId()), u);

            if (graph.GetFeatures().SupportsEdgeRetrieval)
                Assert.Null(graph.GetEdge(e.GetId()));

            if (graph.GetFeatures().SupportsVertexIteration)
                Assert.AreEqual(Count(graph.GetVertices()), 2);

            if (graph.GetFeatures().SupportsEdgeIteration)
                Assert.AreEqual(Count(graph.GetEdges()), 0);

            StopWatch();

            e = graph.AddEdge(null, graph.GetVertex(u.GetId()), graph.GetVertex(v.GetId()), ConvertId(graph, "test"));

            Assert.AreEqual(graph.GetVertex(v.GetId()), v);
            Assert.AreEqual(graph.GetVertex(u.GetId()), u);

            if (graph.GetFeatures().SupportsEdgeRetrieval)
                Assert.AreEqual(graph.GetEdge(e.GetId()), e);

            if (graph.GetFeatures().SupportsVertexIteration)
                Assert.AreEqual(Count(graph.GetVertices()), 2);

            if (graph.GetFeatures().SupportsEdgeIteration)
                Assert.AreEqual(Count(graph.GetEdges()), 1);

            Assert.AreEqual(e, GetOnlyElement(graph.GetVertex(u.GetId()).GetEdges(Direction.Out)));
            graph.Commit();
            PrintPerformance(graph.ToString(), 1, "edge added in successful transaction (w/ iteration)", StopWatch());

            if (graph.GetFeatures().SupportsVertexIteration)
                Assert.AreEqual(Count(graph.GetVertices()), 2);

            if (graph.GetFeatures().SupportsEdgeIteration)
                Assert.AreEqual(Count(graph.GetEdges()), 1);

            Assert.AreEqual(graph.GetVertex(v.GetId()), v);
            Assert.AreEqual(graph.GetVertex(u.GetId()), u);
            if (graph.GetFeatures().SupportsEdgeRetrieval)
                Assert.AreEqual(graph.GetEdge(e.GetId()), e);
            Assert.AreEqual(e, GetOnlyElement(graph.GetVertex(u.GetId()).GetEdges(Direction.Out)));

            graph.Shutdown();
        }

        [Test]
        public void TestBruteEdgeTransactions()
        {
            var graph = (ITransactionalGraph)GraphTest.GenerateGraph();
            StopWatch();
            for (int i = 0; i < 100; i++)
            {
                IVertex v = graph.AddVertex(null);
                IVertex u = graph.AddVertex(null);
                graph.AddEdge(null, v, u, ConvertId(graph, "test"));
                graph.Commit();
            }
            PrintPerformance(graph.ToString(), 100, "edges added in 100 successful transactions (2 vertices added for each edge)", StopWatch());
            VertexCount(graph, 200);
            EdgeCount(graph, 100);

            StopWatch();
            for (int i = 0; i < 100; i++)
            {
                IVertex v = graph.AddVertex(null);
                IVertex u = graph.AddVertex(null);
                graph.AddEdge(null, v, u, ConvertId(graph, "test"));
                graph.Rollback();
            }
            PrintPerformance(graph.ToString(), 100, "edges not added in 100 failed transactions (2 vertices added for each edge)", StopWatch());
            VertexCount(graph, 200);
            EdgeCount(graph, 100);

            StopWatch();
            for (int i = 0; i < 100; i++)
            {
                IVertex v = graph.AddVertex(null);
                IVertex u = graph.AddVertex(null);
                graph.AddEdge(null, v, u, ConvertId(graph, "test"));
            }
            VertexCount(graph, 400);
            EdgeCount(graph, 200);
            graph.Commit();
            PrintPerformance(graph.ToString(), 100, "edges added in 1 successful transactions (2 vertices added for each edge)", StopWatch());
            VertexCount(graph, 400);
            EdgeCount(graph, 200);

            StopWatch();
            for (int i = 0; i < 100; i++)
            {
                IVertex v = graph.AddVertex(null);
                IVertex u = graph.AddVertex(null);
                graph.AddEdge(null, v, u, ConvertId(graph, "test"));
            }
            VertexCount(graph, 600);
            EdgeCount(graph, 300);

            graph.Rollback();
            PrintPerformance(graph.ToString(), 100, "edges not added in 1 failed transactions (2 vertices added for each edge)", StopWatch());
            VertexCount(graph, 400);
            EdgeCount(graph, 200);


            graph.Shutdown();
        }

        [Test]
        public void TestPropertyTransactions()
        {
            var graph = (ITransactionalGraph)GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsElementProperties())
            {
                StopWatch();
                IVertex v = graph.AddVertex(null);
                Object id = v.GetId();
                v.SetProperty("name", "marko");
                graph.Commit();
                PrintPerformance(graph.ToString(), 1, "vertex added with string property in a successful transaction", StopWatch());


                StopWatch();
                v = graph.GetVertex(id);
                Assert.NotNull(v);
                Assert.AreEqual(v.GetProperty("name"), "marko");
                v.SetProperty("age", 30);
                Assert.AreEqual(v.GetProperty("age"), 30);
                graph.Rollback();
                PrintPerformance(graph.ToString(), 1, "integer property not added in a failed transaction", StopWatch());

                StopWatch();
                v = graph.GetVertex(id);
                Assert.NotNull(v);
                Assert.AreEqual(v.GetProperty("name"), "marko");
                Assert.Null(v.GetProperty("age"));
                PrintPerformance(graph.ToString(), 2, "vertex properties checked in a successful transaction", StopWatch());

                EdgeCount(graph, 1);
                graph.Commit();
                EdgeCount(graph, 1);
                IEdge edge = GetOnlyElement(graph.GetVertex(v.GetId()).GetEdges(Direction.Out));
                Assert.NotNull(edge);

                StopWatch();
                edge.SetProperty("transaction-1", "success");
                Assert.AreEqual(edge.GetProperty("transaction-1"), "success");
                graph.Commit();
                PrintPerformance(graph.ToString(), 1, "edge property added and checked in a successful transaction", StopWatch());
                edge = GetOnlyElement(graph.GetVertex(v.GetId()).GetEdges(Direction.Out));
                Assert.AreEqual(edge.GetProperty("transaction-1"), "success");

                StopWatch();
                edge.SetProperty("transaction-2", "failure");
                Assert.AreEqual(edge.GetProperty("transaction-1"), "success");
                Assert.AreEqual(edge.GetProperty("transaction-2"), "failure");
                graph.Rollback();
                PrintPerformance(graph.ToString(), 1, "edge property added and checked in a failed transaction", StopWatch());
                edge = GetOnlyElement(graph.GetVertex(v.GetId()).GetEdges(Direction.Out));
                Assert.AreEqual(edge.GetProperty("transaction-1"), "success");
                Assert.Null(edge.GetProperty("transaction-2"));
            }
            graph.Shutdown();
        }

        [Test]
        public void TestIndexTransactions()
        {
            var graph = (ITransactionalGraph)GraphTest.GenerateGraph();

            if (graph.GetFeatures().SupportsVertexIndex)
            {
                StopWatch();
                IIndex index = ((IIndexableGraph)graph).CreateIndex("txIdx", typeof(IVertex));
                IVertex v = graph.AddVertex(null);
                Object id = v.GetId();
                v.SetProperty("name", "marko");
                index.Put("name", "marko", v);
                VertexCount(graph, 1);
                v = (IVertex)GetOnlyElement(((IIndexableGraph)graph).GetIndex("txIdx", typeof(IVertex)).Get("name", "marko"));
                Assert.AreEqual(v.GetId(), id);
                Assert.AreEqual(v.GetProperty("name"), "marko");
                graph.Commit();
                PrintPerformance(graph.ToString(), 1, "vertex added and retrieved from index in a successful transaction", StopWatch());


                StopWatch();
                VertexCount(graph, 1);
                v = (IVertex)GetOnlyElement(((IIndexableGraph)graph).GetIndex("txIdx", typeof(IVertex)).Get("name", "marko"));
                Assert.AreEqual(v.GetId(), id);
                Assert.AreEqual(v.GetProperty("name"), "marko");
                PrintPerformance(graph.ToString(), 1, "vertex retrieved from index outside successful transaction", StopWatch());


                StopWatch();
                v = graph.AddVertex(null);
                v.SetProperty("name", "pavel");
                index.Put("name", "pavel", v);
                VertexCount(graph, 2);
                v = (IVertex)GetOnlyElement(((IIndexableGraph)graph).GetIndex("txIdx", typeof(IVertex)).Get("name", "marko"));
                Assert.AreEqual(v.GetProperty("name"), "marko");
                v = (IVertex)GetOnlyElement(((IIndexableGraph)graph).GetIndex("txIdx", typeof(IVertex)).Get("name", "pavel"));
                Assert.AreEqual(v.GetProperty("name"), "pavel");
                graph.Rollback();
                PrintPerformance(graph.ToString(), 1, "vertex not added in a failed transaction", StopWatch());

                StopWatch();
                VertexCount(graph, 1);
                Assert.AreEqual(Count(((IIndexableGraph)graph).GetIndex("txIdx", typeof(IVertex)).Get("name", "pavel")), 0);
                PrintPerformance(graph.ToString(), 1, "vertex not retrieved in a successful transaction", StopWatch());
                v = (IVertex)GetOnlyElement(((IIndexableGraph)graph).GetIndex("txIdx", typeof(IVertex)).Get("name", "marko"));
                Assert.AreEqual(v.GetProperty("name"), "marko");
            }
            graph.Shutdown();
        }

        [Test]
        public void TestAutomaticSuccessfulTransactionOnShutdown()
        {
            var graph = (ITransactionalGraph)GraphTest.GenerateGraph();

            if (graph.GetFeatures().IsPersistent && graph.GetFeatures().SupportsVertexProperties)
            {
                IVertex v = graph.AddVertex(null);
                Object id = v.GetId();
                v.SetProperty("count", "1");
                v.SetProperty("count", "2");
                graph.Shutdown();
                graph = (ITransactionalGraph)GraphTest.GenerateGraph();
                IVertex reloadedV = graph.GetVertex(id);
                Assert.AreEqual("2", reloadedV.GetProperty("count"));

            }
            graph.Shutdown();
        }

        [Test]
        public void TestVertexCountOnPreTransactionCommit()
        {
            var graph = (ITransactionalGraph)GraphTest.GenerateGraph();
            IVertex v1 = graph.AddVertex(null);
            graph.Commit();

            VertexCount(graph, 1);

            IVertex v2 = graph.AddVertex(null);
            v1 = graph.GetVertex(v1.GetId());
            graph.AddEdge(null, v1, v2, ConvertId(graph, "friend"));

            VertexCount(graph, 2);

            graph.Commit();

            VertexCount(graph, 2);
            graph.Shutdown();
        }

        [Test]
        public void TestBulkTransactionsOnEdges()
        {
            var graph = (ITransactionalGraph)GraphTest.GenerateGraph();
            for (int i = 0; i < 5; i++)
            {
                graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), ConvertId(graph, "test"));
            }
            EdgeCount(graph, 5);
            graph.Rollback();
            EdgeCount(graph, 0);

            for (int i = 0; i < 4; i++)
            {
                graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), ConvertId(graph, "test"));
            }
            EdgeCount(graph, 4);
            graph.Rollback();
            EdgeCount(graph, 0);


            for (int i = 0; i < 3; i++)
            {
                graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), ConvertId(graph, "test"));
            }
            EdgeCount(graph, 3);
            graph.Commit();
            EdgeCount(graph, 3);

            graph.Shutdown();
        }

        [Test]
        public void TestCompetingThreads()
        {
            var graph = (ITransactionalGraph)GraphTest.GenerateGraph();
            const int totalThreads = 250;
            long vertices = 0;
            long edges = 0;
            long completedThreads = 0;

            Parallel.For(0, totalThreads, i =>
            {
                var random = new Random();
                if (random.Next() % 2 == 0)
                {
                    IVertex a = graph.AddVertex(null);
                    IVertex b = graph.AddVertex(null);
                    IEdge e = graph.AddEdge(null, a, b, ConvertId(graph, "friend"));

                    if (graph.GetFeatures().SupportsElementProperties())
                    {
                        a.SetProperty("test", Task.CurrentId);
                        b.SetProperty("blah", random.NextDouble());
                        e.SetProperty("bloop", random.Next());
                    }
                    Interlocked.Add(ref vertices, 2);
                    Interlocked.Increment(ref edges);
                    graph.Commit();
                }
                else
                {
                    IVertex a = graph.AddVertex(null);
                    IVertex b = graph.AddVertex(null);
                    IEdge e = graph.AddEdge(null, a, b, ConvertId(graph, "friend"));
                    if (graph.GetFeatures().SupportsElementProperties())
                    {
                        a.SetProperty("test", Task.CurrentId);
                        b.SetProperty("blah", random.NextDouble());
                        e.SetProperty("bloop", random.Next());
                    }
                    if (random.Next() % 2 == 0)
                    {
                        graph.Commit();
                        Interlocked.Add(ref vertices, 2);
                        Interlocked.Increment(ref edges);
                    }
                    else
                    {
                        graph.Rollback();
                    }
                }

                Interlocked.Increment(ref completedThreads);
            });

            Assert.AreEqual(completedThreads, 250);
            EdgeCount(graph, (int)edges);
            VertexCount(graph, (int)vertices);
            graph.Shutdown();
        }

        [Test]
        public void TestCompetingThreadsOnMultipleDbInstances()
        {
            // the idea behind this test is to simulate a rexster environment where two graphs of the same type
            // are being mutated by multiple threads. the test itself surfaced issues with OrientDB in such
            // an environment and remains relevant for any graph that might be exposed through rexster.

            var graph1 = (ITransactionalGraph)GraphTest.GenerateGraph("first");
            var graph2 = (ITransactionalGraph)GraphTest.GenerateGraph("second");

            if (!graph1.GetFeatures().IsRdfModel)
            {
                Task threadModFirstGraph = Task.Factory.StartNew(() =>
                {
                    IVertex v = graph1.AddVertex(null);
                    v.SetProperty("name", "stephen");
                    graph1.Commit();
                });

                
                Task threadReadBothGraphs = Task.Factory.StartNew(() =>
                {
                    int counter = graph1.GetVertices().Count();

                    Assert.AreEqual(1, counter);

                    counter = graph2.GetVertices().Count();

                    Assert.AreEqual(0, counter);
                });

                Task.WaitAll(threadModFirstGraph, threadReadBothGraphs);
            }

            graph1.Shutdown();
            graph2.Shutdown();
        }

        [Test]
        public void UntestTransactionIsolationWithSeparateThreads()
        {
            // the purpose of this test is to simulate rexster access to a graph instance, where one thread modifies
            // the graph and a separate thread reads before the transaction is committed. the expectation is that
            // the changes in the transaction are isolated to the thread that made the change and the second thread
            // should not see the change until commit() in the first thread.
            var graph = (ITransactionalGraph)GraphTest.GenerateGraph();

            if (!graph.GetFeatures().IsRdfModel)
            {
                var latchCommit = new CountdownEvent(1);
                var latchFirstRead = new CountdownEvent(1);
                var latchSecondRead = new CountdownEvent(1);

                Task threadMod = Task.Factory.StartNew(() =>
                {
                    IVertex v = graph.AddVertex(null);
                    v.SetProperty("name", "stephen");

                    Console.WriteLine("added vertex");

                    latchFirstRead.AddCount();
                    latchCommit.Wait();
                    graph.Commit();

                    Console.WriteLine("committed vertex");

                    latchSecondRead.AddCount();
                });

                Task threadRead = Task.Factory.StartNew(() =>
                {

                    latchFirstRead.Wait();

                    Console.WriteLine("reading vertex before tx");
                    Assert.False(graph.GetVertices().Any());
                    Console.WriteLine("read vertex before tx");

                    latchCommit.AddCount();
                    latchSecondRead.Wait();

                    Console.WriteLine("reading vertex after tx");
                    Assert.True(graph.GetVertices().Any());
                    Console.WriteLine("read vertex after tx");
                });

                Task.WaitAll(threadMod, threadRead);
            }

            graph.Shutdown();
        }

        [Test]
        public void TestTransactionIsolationCommitCheck()
        {
            // the purpose of this test is to simulate rexster access to a graph instance, where one thread modifies
            // the graph and a separate thread cannot affect the transaction of the first
            var graph = (ITransactionalGraph)GraphTest.GenerateGraph();

            if (!graph.GetFeatures().IsRdfModel)
            {
                var latchCommittedInOtherThread = new CountdownEvent(1);
                var latchCommitInOtherThread = new CountdownEvent(1);

                // this thread starts a transaction then waits while the second thread tries to commit it.
                Task threadTxStarter = Task.Factory.StartNew(() =>
                {
                    IVertex v = graph.AddVertex(null);
                    v.SetProperty("name", "stephen");

                    Console.WriteLine("added vertex");

                    latchCommitInOtherThread.AddCount();
                    latchCommittedInOtherThread.Wait();
                    graph.Rollback();

                    // there should be no vertices here
                    Console.WriteLine("reading vertex before tx");
                    Assert.False(graph.GetVertices().Any());
                    Console.WriteLine("read vertex before tx");
                });

                // this thread tries to commit the transaction started in the first thread above.
                Task threadTryCommitTx = Task.Factory.StartNew(() =>
                {
                    latchCommitInOtherThread.Wait();

                    // try to commit the other transaction
                    graph.Commit();

                    latchCommittedInOtherThread.AddCount();
                });

                Task.WaitAll(threadTxStarter, threadTryCommitTx);
            }

            graph.Shutdown();
        }

        [Test]
        public void TestRemoveInTransaction()
        {
            var graph = (ITransactionalGraph)GraphTest.GenerateGraph();
            EdgeCount(graph, 0);

            IVertex v1 = graph.AddVertex(null);
            Object v1Id = v1.GetId();
            IVertex v2 = graph.AddVertex(null);
            graph.AddEdge(null, v1, v2, ConvertId(graph, "test-edge"));
            graph.Commit();

            EdgeCount(graph, 1);
            IEdge e1 = GetOnlyElement(graph.GetVertex(v1Id).GetEdges(Direction.Out));
            Assert.NotNull(e1);
            graph.RemoveEdge(e1);
            EdgeCount(graph, 0);
            Assert.Null(GetOnlyElement(graph.GetVertex(v1Id).GetEdges(Direction.Out)));
            graph.Rollback();

            EdgeCount(graph, 1);
            e1 = GetOnlyElement(graph.GetVertex(v1Id).GetEdges(Direction.Out));
            Assert.NotNull(e1);

            graph.RemoveEdge(e1);
            graph.Commit();

            EdgeCount(graph, 0);
            Assert.Null(GetOnlyElement(graph.GetVertex(v1Id).GetEdges(Direction.Out)));
            graph.Shutdown();
        }
    }
}