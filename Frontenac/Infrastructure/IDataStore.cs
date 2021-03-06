﻿using System.Collections.Generic;
using Frontenac.Blueprints;

namespace Frontenac.Infrastructure
{
    public interface IDataStore
    {
        IVertex AddVertex(object id);
        IVertex GetVertex(object id);
        void RemoveVertex(IVertex vertex);
        IEdge AddEdge(object id, IVertex outVertex, IVertex inVertex, string label);
        void RemoveEdge(IEdge edge);
        IEnumerable<IVertex> GetVertices();
    }
}