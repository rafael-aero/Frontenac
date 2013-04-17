﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints
{
    /// <summary>
    /// A VertexQuery object defines a collection of filters and modifiers that are used to intelligently select edges from a vertex.
    /// </summary>
    public interface VertexQuery : Query
    {
        /// <summary>
        /// The direction of the edges to retrieve.
        /// </summary>
        /// <param name="direction">whether to retrieve the incoming, outgoing, or both directions</param>
        /// <returns>the modified query object</returns>
        VertexQuery Direction(Direction direction);

        /// <summary>
        /// Filter out the edge if its label is not in set of provided labels.
        /// </summary>
        /// <param name="labels">the labels to check against</param>
        /// <returns>the modified query object</returns>
        VertexQuery Labels(params string[] labels);

        /// <summary>
        /// Execute the query and return the number of edges that are unfiltered.
        /// </summary>
        /// <returns>the number of unfiltered edges</returns>
        long Count();

        /// <summary>
        /// Return the raw ids of the vertices on the other end of the edges.
        /// </summary>
        /// <returns>the raw ids of the vertices on the other end of the edges</returns>
        object VertexIds();
    }
}
