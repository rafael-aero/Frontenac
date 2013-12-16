﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using Castle.Components.DictionaryAdapter;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public static class GremlinqHelpers
    {
        private static readonly DictionaryAdapterFactory DictionaryAdapterFactory = new DictionaryAdapterFactory();

        public static IEnumerable<IElementWrapper<IVertex, TInModel>> Loop<TModel, TInModel>(
                                                this IElementWrapper<IVertex, TModel> element,
                                                Func<TModel, TInModel> startPointSelector,
                                                Func<IElementWrapper<IVertex, TInModel>, IEnumerable<IElementWrapper<IVertex, TInModel>>> loopFunction,
                                                int nbIterations)
        {
            var next = (IEnumerable<IElementWrapper<IVertex, TInModel>>)new[] { element };

            for (var i = 0; i < nbIterations; i++)
                next = next.SelectMany(loopFunction);

            return next;
        }

        public static IEnumerable<IVertex> V(this IGraph graph, string propertyName, object value)
        {
            Contract.Requires(graph != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(propertyName));

            return graph.GetVertices(propertyName, value);
        }

        public static IEnumerable<IElementWrapper<IVertex, TModel>> V<TModel, TValue>(this IGraph graph,
                                                Expression<Func<TModel, TValue>> propertySelector, TValue value)
        {
            Contract.Requires(graph != null);
            Contract.Requires(propertySelector != null);

            return graph.GetVertices(propertySelector.Resolve(), value).As<TModel>();
        }

        public static IDictionary<string, object> Map(this IElement element)
        {
            Contract.Requires(element != null);

            return element.ToDictionary(t => t.Key, t => t.Value);
        }

        public static IEnumerable<IDictionary<string, object>> Map(this IEnumerable<IElement> elements)
        {
            Contract.Requires(elements != null);

            return elements.Select(e => e.Map());
        }

        public static IEnumerable<IVertex> In(this IVertex vertex, params string[] labels)
        {
            Contract.Requires(vertex != null);

            return vertex.GetVertices(Direction.In, labels);
        }

        public static IEnumerable<IVertex> In(this IEnumerable<IVertex> vertices, params string[] labels)
        {
            Contract.Requires(vertices != null);
            Contract.Requires(labels != null);

            return vertices.SelectMany(t => t.GetVertices(Direction.In, labels));
        }

        public static IEnumerable<IElementWrapper<IVertex, TInModel>> In<TOutModel, TInModel>(
            this IElementWrapper<IVertex, TOutModel> vertex,
            Expression<Func<TOutModel, TInModel>> propertySelector)
        {
            Contract.Requires(vertex != null);
            Contract.Requires(propertySelector != null);

            return vertex.Element
                .In(propertySelector.Resolve())
                .As<TInModel>();
        }

        public static IEnumerable<IElementWrapper<IVertex, TInModel>> In<TOutModel, TInModel>(
                                    this IEnumerable<IElementWrapper<IVertex, TOutModel>> vertices,
                                    Expression<Func<TOutModel, TInModel>> propertySelector)
        {
            Contract.Requires(vertices != null);
            Contract.Requires(propertySelector != null);

            return vertices
                .SelectMany(t => t.Element.GetVertices(Direction.In, propertySelector.Resolve()))
                .As<TInModel>();
        }

        public static IEnumerable<IElementWrapper<IVertex, TInModel>> In<TOutModel, TInModel>(
                                    this IEnumerable<IElementWrapper<IVertex, TOutModel>> vertices,
                                    Expression<Func<IElementWrapper<IVertex, TOutModel>, IElementWrapper<IVertex, TInModel>>> propertySelector)
        {
            Contract.Requires(vertices != null);
            Contract.Requires(propertySelector != null);

            return vertices
                .SelectMany(t => t.Element.GetVertices(Direction.In, propertySelector.Resolve()))
                .As<TInModel>();
        }

        public static TModel Proxy<TModel>(this IElement element)
        {
            Contract.Requires(element != null);

            object typeName;
            var typeToProxy = element.TryGetValue("_type", out typeName) ? Type.GetType(typeName.ToString()) : typeof (TModel);
            var propsDesc = new PropertyDescriptor();
            propsDesc.AddBehavior(new DictionaryPropertyConverter());
            var proxy = (TModel)DictionaryAdapterFactory.GetAdapter(typeToProxy, element, propsDesc);
            return proxy;
        }

        public static IElementWrapper<IVertex, TModel> As<TModel>(this IVertex vertex)
        {
            Contract.Requires(vertex != null);

            return new ElementWrapper<IVertex, TModel>(vertex, vertex.Proxy<TModel>());
        }

        public static IElementWrapper<IEdge, TModel> As<TModel>(this IEdge edge)
        {
            Contract.Requires(edge != null);

            return new ElementWrapper<IEdge, TModel>(edge, edge.Proxy<TModel>());
        }

        public static IEnumerable<IElementWrapper<IVertex, TModel>> As<TModel>(this IEnumerable<IVertex> vertices)
        {
            Contract.Requires(vertices != null);

            return vertices.Select(t => t.As<TModel>());
        }

        public static IEnumerable<IElementWrapper<IEdge, TModel>> As<TModel>(this IEnumerable<IEdge> edges)
        {
            Contract.Requires(edges != null);

            return edges.Select(t => t.As<TModel>());
        }

        public static string Resolve(this Expression e)
        {
            Contract.Requires(e != null);

            if (e.NodeType == ExpressionType.Lambda)
                return ((LambdaExpression)e).Body.Resolve();

            if (e.NodeType == ExpressionType.MemberAccess)
                return ((MemberExpression)e).Member.Name;

            throw new InvalidOperationException("Given expression is not type MemberAccess.");
        }

        public static IElementWrapper<IVertex, TModel> AddVertex<TModel>(this IGraph graph, Action<TModel> assignMembers)
        {
            Contract.Requires(graph != null);
            Contract.Requires(assignMembers != null);

            var vertex = graph.AddVertex(null);
            var typeName = typeof (TModel).FullName;
            vertex.Add("_type", typeName);
            var proxy = vertex.Proxy<TModel>();
            assignMembers(proxy);
            return new ElementWrapper<IVertex, TModel>(vertex, proxy);
        }

        public static IEdge AddEdge<TOutModel, TInModel>(this IElementWrapper<IVertex, TOutModel> outVertex,
                                                         Expression<Func<TOutModel, TInModel>> edgePropertySelector,
                                                         IElementWrapper<IVertex, TInModel> inVertex)
        {
            Contract.Requires(outVertex != null);
            Contract.Requires(edgePropertySelector != null);
            Contract.Requires(inVertex != null);

            return outVertex.Element.AddEdge(edgePropertySelector.Resolve(), inVertex.Element);
        }

        public static IEdge AddEdge<TOutModel, TInModel>(this IElementWrapper<IVertex, TOutModel> outVertex,
                                                         Expression<Func<TOutModel, IEnumerable<TInModel>>> edgePropertySelector,
                                                         IElementWrapper<IVertex, TInModel> inVertex)
        {
            Contract.Requires(outVertex != null);
            Contract.Requires(edgePropertySelector != null);
            Contract.Requires(inVertex != null);

            return outVertex.Element.AddEdge(edgePropertySelector.Resolve(), inVertex.Element);
        }

        public static IElementWrapper<IEdge, TEdgeModel> CreateWrapper<TInModel, TOutModel, TEdgeModel>(
                                                        IElementWrapper<IVertex, TOutModel> outVertex,
                                                        Expression expression,
                                                        IElementWrapper<IVertex, TInModel> inVertex)
        {
            Contract.Requires(outVertex != null);
            Contract.Requires(expression != null);
            Contract.Requires(inVertex != null);

            var edge = outVertex.Element.AddEdge(expression.Resolve(), inVertex.Element);
            var model = edge.Proxy<TEdgeModel>();
            return new ElementWrapper<IEdge, TEdgeModel>(edge, model);
        }

        public static IElementWrapper<IEdge, TEdgeModel> CreateWrapper<TInModel, TOutModel, TEdgeModel>(
                                                        IElementWrapper<IVertex, TOutModel> outVertex,
                                                        Expression expression,
                                                        IElementWrapper<IVertex, TInModel> inVertex,
                                                        Action<TEdgeModel> assignMembers)
        {
            Contract.Requires(outVertex != null);
            Contract.Requires(expression != null);
            Contract.Requires(inVertex != null);
            Contract.Requires(assignMembers != null);

            var wrapper = CreateWrapper<TInModel, TOutModel, TEdgeModel>(outVertex, expression, inVertex);
            assignMembers(wrapper.Model);
            return wrapper;
        }

        public static IElementWrapper<IEdge, TEdgeModel> AddEdge<TOutModel, TInModel, TEdgeModel>(
                                                        this IElementWrapper<IVertex, TOutModel> outVertex,
                                                        Expression<Func<TOutModel, KeyValuePair<TEdgeModel, TInModel>>> edgePropertySelector,
                                                        IElementWrapper<IVertex, TInModel> inVertex)
        {
            Contract.Requires(outVertex != null);
            Contract.Requires(edgePropertySelector != null);
            Contract.Requires(inVertex != null);

            return CreateWrapper<TInModel, TOutModel, TEdgeModel>(outVertex, edgePropertySelector, inVertex);
        }

        public static IElementWrapper<IEdge, TEdgeModel> AddEdge<TOutModel, TInModel, TEdgeModel>(
                                                        this IElementWrapper<IVertex, TOutModel> outVertex,
                                                        Expression<Func<TOutModel, KeyValuePair<TEdgeModel, TInModel>>> edgePropertySelector,
                                                        IElementWrapper<IVertex, TInModel> inVertex,
                                                        Action<TEdgeModel> assignMembers)
        {
            Contract.Requires(outVertex != null);
            Contract.Requires(edgePropertySelector != null);
            Contract.Requires(inVertex != null);
            Contract.Requires(assignMembers != null);

            return CreateWrapper(outVertex, edgePropertySelector, inVertex, assignMembers);
        }

        public static IElementWrapper<IEdge, TEdgeModel> AddEdge<TOutModel, TInModel, TEdgeModel>(
                                                        this IElementWrapper<IVertex, TOutModel> outVertex,
                                                        Expression<Func<TOutModel, IEnumerable<KeyValuePair<TEdgeModel, TInModel>>>> edgePropertySelector,
                                                        IElementWrapper<IVertex, TInModel> inVertex,
                                                        Action<TEdgeModel> assignMembers)
        {
            Contract.Requires(outVertex != null);
            Contract.Requires(edgePropertySelector != null);
            Contract.Requires(inVertex != null);
            Contract.Requires(assignMembers != null);

            return CreateWrapper(outVertex, edgePropertySelector, inVertex, assignMembers);
        }

        public static void CreateIndex<TModel, TIndexType>(this IKeyIndexableGraph graph,
                                                           Expression<Func<TModel, TIndexType>> propertySelector,
                                                           Type indexType)
        {
            Contract.Requires(graph != null);
            Contract.Requires(propertySelector != null);
            Contract.Requires(indexType != null);

            var name = propertySelector.Resolve();
            if (!graph.GetIndexedKeys(indexType).Contains(name))
                graph.CreateKeyIndex(name, indexType);
        }

        public static void CreateVertexIndex<TModel, TResult>(this IKeyIndexableGraph graph,
                                                              Expression<Func<TModel, TResult>> propertySelector)
        {
            Contract.Requires(graph != null);
            Contract.Requires(propertySelector != null);

            graph.CreateIndex(propertySelector, typeof(IVertex));
        }

        public static void CreateEdgeIndex<TModel, TResult>(this IKeyIndexableGraph graph,
                                                            Expression<Func<TModel, TResult>> propertySelector)
        {
            Contract.Requires(graph != null);
            Contract.Requires(propertySelector != null);

            graph.CreateIndex(propertySelector, typeof(IEdge));
        }

        public static IQueryWrapper<TModel> Query<TModel>(this IGraph graph)
        {
            Contract.Requires(graph != null);

            return new QueryWrapper<TModel>(graph.Query());
        }

        public static IQueryWrapper<TModel> Has<TModel, TResult>(this IQueryWrapper<TModel> query, 
                                                                 Expression<Func<TModel, TResult>> propertySelector, 
                                                                 Compare compare, TResult value)
        {
            Contract.Requires(query != null);
            Contract.Requires(propertySelector != null);

            query.InnerQuery.Has(propertySelector.Resolve(), compare, value);
            return query;
        }

        public static IEnumerable<IElementWrapper<IEdge, TModel>> Edges<TModel>(this IQueryWrapper<TModel> query)
        {
            Contract.Requires(query != null);

            return query.InnerQuery.Edges().As<TModel>();
        }
        
        public static IElementWrapper<IVertex, TInModel> In<TModel, TInModel>(this IElementWrapper<IEdge, TModel> edge,
                                                                              Expression<Func<TModel, TInModel>> edgePropertySelector)
        {
            Contract.Requires(edge != null);
            Contract.Requires(edgePropertySelector != null);

            var vertex = edge.Element.GetVertex(Direction.In);
            return new ElementWrapper<IVertex, TInModel>(vertex, vertex.Proxy<TInModel>());
        }

        public static IElementWrapper<IVertex, TOutModel> Out<TModel, TOutModel>(this IElementWrapper<IEdge, TModel> edge,
                                                                              Expression<Func<TModel, TOutModel>> edgePropertySelector)
        {
            Contract.Requires(edge != null);
            Contract.Requires(edgePropertySelector != null);

            var vertex = edge.Element.GetVertex(Direction.Out);
            return new ElementWrapper<IVertex, TOutModel>(vertex, vertex.Proxy<TOutModel>());
        }

        public static IEnumerable<IElementWrapper<IVertex, TOutModel>> Out<TModel, TOutModel>(this IElementWrapper<IVertex, TModel> vertex,
                                                                              params Expression<Func<TModel, TOutModel>>[] edgePropertySelectors)
        {
            Contract.Requires(vertex != null);
            Contract.Requires(edgePropertySelectors != null);

            return vertex.Element.GetVertices(Direction.Out, edgePropertySelectors.Select(Resolve).ToArray()).As<TOutModel>();
        }

        public static IEnumerable<IElementWrapper<IEdge, TEdgeModel>> Out<TModel, TOutModel, TEdgeModel>(this IElementWrapper<IVertex, TModel> vertex,
                                                                              params Expression<Func<TModel, KeyValuePair<TEdgeModel, TOutModel>>>[] edgePropertySelectors)
        {
            Contract.Requires(vertex != null);
            Contract.Requires(edgePropertySelectors != null);

            return vertex.Element.GetEdges(Direction.Out, edgePropertySelectors.Select(Resolve).ToArray()).As<TEdgeModel>();
        }

        public static IEnumerable<IElementWrapper<IEdge, TEdgeModel>> Out<TModel, TOutModel, TEdgeModel>(this IElementWrapper<IVertex, TModel> vertex,
                                                                              params Expression<Func<TModel, IEnumerable<KeyValuePair<TEdgeModel, TOutModel>>>>[] edgePropertySelectors)
        {
            Contract.Requires(vertex != null);
            Contract.Requires(edgePropertySelectors != null);

            return vertex.Element.GetEdges(Direction.Out, edgePropertySelectors.Select(Resolve).ToArray()).As<TEdgeModel>();
        }

        public static IEnumerable<IElementWrapper<IVertex, TInModel>> In<TModel, TInModel>(this IElementWrapper<IVertex, TModel> vertex,
                                                                              params Expression<Func<TModel, TInModel>>[] edgePropertySelectors)
        {
            Contract.Requires(vertex != null);
            Contract.Requires(edgePropertySelectors != null);

            return vertex.Element.GetVertices(Direction.In, edgePropertySelectors.Select(Resolve).ToArray()).As<TInModel>();
        }
    }
}