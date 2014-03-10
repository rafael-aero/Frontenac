﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        public static IEnumerable<IVertex> Out(this IVertex vertex, int branchFactor, params string[] labels)
        {
            Contract.Requires(vertex != null);
            Contract.Requires(labels != null);
            Contract.Ensures(Contract.Result<IEnumerable<IVertex>>() != null);

            var finalLabels = labels.Length == 0 ? vertex.GetPropertyKeys() : labels;
            return finalLabels.SelectMany(t => vertex.Out(t).Take(branchFactor));
        }

        public static IEnumerable<IVertex> Out(this IEnumerable<IVertex> vertices, int branchFactor, params string[] labels)
        {
            Contract.Requires(vertices != null);
            Contract.Requires(labels != null);
            Contract.Ensures(Contract.Result<IEnumerable<IVertex>>() != null);

            return vertices.SelectMany(t => t.Out(branchFactor, labels));
        }

        public static IEnumerable<IVertex<TOutModel>> Out<TOutModel, TInModel>(
            this IVertex<TInModel> vertex,
            int branchFactor,
            Expression<Func<TInModel, TOutModel>> propertySelector) where TOutModel : class
        {
            Contract.Requires(vertex != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IVertex<TOutModel>>>() != null);

            return vertex.Out(propertySelector.Resolve()).As<TOutModel>();
        }

        public static IEnumerable<IVertex<TOutModel>> Out<TOutModel, TInModel>(
            this IEnumerable<IVertex<TInModel>> vertices,
            int branchFactor,
            Expression<Func<TInModel, TOutModel>> propertySelector) where TOutModel : class
        {
            Contract.Requires(vertices != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IVertex<TOutModel>>>() != null);

            return vertices.Out(branchFactor, propertySelector.Resolve()).As<TOutModel>();
        }

        public static IEnumerable<IVertex<TOutModel>> Out<TOutModel, TInModel>(
            this IEnumerable<IVertex<TInModel>> vertices,
            int branchFactor,
            Expression<Func<IVertex<TInModel>, IVertex<TOutModel>>> propertySelector) where TOutModel : class
        {
            Contract.Requires(vertices != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IVertex<TOutModel>>>() != null);

            return vertices.SelectMany(t => t.Out(branchFactor, propertySelector.Resolve())).As<TOutModel>();
        }

        public static IEnumerable<IVertex> Out(this IVertex vertex, params string[] labels)
        {
            Contract.Requires(vertex != null);
            Contract.Requires(labels != null);
            Contract.Ensures(Contract.Result<IEnumerable<IVertex>>() != null);

            return vertex.GetVertices(Direction.Out, labels);
        }

        public static IEnumerable<IVertex> Out(this IEnumerable<IVertex> vertices, params string[] labels)
        {
            Contract.Requires(vertices != null);
            Contract.Requires(labels != null);
            Contract.Ensures(Contract.Result<IEnumerable<IVertex>>() != null);

            return vertices.SelectMany(t => t.Out(labels));
        }

        public static IEnumerable<IVertex<TOutModel>> Out<TOutModel, TInModel, TEdgeModel>(
            this IVertex<TInModel> vertex,
            Expression<Func<KeyValuePair<TEdgeModel, TInModel>, TOutModel>> propertySelector) where TOutModel : class
        {
            Contract.Requires(vertex != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IVertex<TOutModel>>>() != null);

            return vertex.Out(propertySelector.Resolve()).As<TOutModel>();
        }

        public static IEnumerable<IVertex<TOutModel>> Out<TOutModel, TInModel, TEdgeModel>(
            this IVertex<TInModel> vertex,
            Expression<Func<IEnumerable<KeyValuePair<TEdgeModel, TInModel>>, TOutModel>> propertySelector) where TOutModel : class
        {
            Contract.Requires(vertex != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IVertex<TOutModel>>>() != null);

            return vertex.Out(propertySelector.Resolve()).As<TOutModel>();
        }

        public static IEnumerable<IVertex<TOutModel>> Out<TOutModel, TInModel>(
            this IEnumerable<IVertex<TInModel>> vertices,
            Expression<Func<TInModel, TOutModel>> propertySelector) where TOutModel : class
        {
            Contract.Requires(vertices != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IVertex<TOutModel>>>() != null);

            return vertices.SelectMany(t => t.Out(propertySelector.Resolve())).As<TOutModel>();
        }

        public static IEnumerable<IVertex<TOutModel>> Out<TOutModel, TInModel>(
            this IEnumerable<IVertex<TInModel>> vertices,
            Expression<Func<IVertex<TInModel>, IVertex<TOutModel>>> propertySelector) where TOutModel : class
        {
            Contract.Requires(vertices != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IVertex<TOutModel>>>() != null);

            return vertices.SelectMany(t => t.Out(propertySelector.Resolve())).As<TOutModel>();
        }

        public static IVertex<TOutModel> Out<TModel, TOutModel>(
            this IEdge<TModel> edge,
            Expression<Func<TModel, TOutModel>> edgePropertySelector) where TOutModel : class
        {
            Contract.Requires(edge != null);
            Contract.Requires(edgePropertySelector != null);
            Contract.Ensures(Contract.Result<IVertex<TOutModel>>() != null);

            var vertex = edge.GetVertex(Direction.Out);
            return new Vertex<TOutModel>(vertex, vertex.Proxy<TOutModel>());
        }

        public static IEnumerable<IVertex<TOutModel>> Out<TModel, TOutModel>(
            this IVertex<TModel> vertex,
            params Expression<Func<TModel, TOutModel>>[] edgePropertySelectors) where TOutModel : class
        {
            Contract.Requires(vertex != null);
            Contract.Requires(edgePropertySelectors != null);
            Contract.Ensures(Contract.Result<IEnumerable<IVertex<TOutModel>>>() != null);

            return vertex
                .GetVertices(Direction.Out, edgePropertySelectors.Select(Resolve).ToArray())
                .As<TOutModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> Out<TModel, TOutModel, TEdgeModel>(
            this IVertex<TModel> vertex,
            params Expression<Func<TModel, KeyValuePair<TEdgeModel, TOutModel>>>[] edgePropertySelectors) where TEdgeModel : class
        {
            Contract.Requires(vertex != null);
            Contract.Requires(edgePropertySelectors != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TEdgeModel>>>() != null);

            return vertex
                .GetEdges(Direction.Out, edgePropertySelectors.Select(Resolve).ToArray())
                .As<TEdgeModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> Out<TModel, TOutModel, TEdgeModel>(
            this IVertex<TModel> vertex,
            params Expression<Func<TModel, IEnumerable<KeyValuePair<TEdgeModel, TOutModel>>>>[] edgePropertySelectors) where TEdgeModel : class
        {
            Contract.Requires(vertex != null);
            Contract.Requires(edgePropertySelectors != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TEdgeModel>>>() != null);

            return vertex
                .GetEdges(Direction.Out, edgePropertySelectors.Select(Resolve).ToArray())
                .As<TEdgeModel>();
        }
    }
}