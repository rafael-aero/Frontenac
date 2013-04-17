﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Frontenac.Blueprints.Util.Wrappers.Batch;

namespace Frontenac.Blueprints.Util.IO.GraphML
{
    /// <summary>
    /// GraphMLReader writes the data from a GraphML stream to a graph.
    /// </summary>
    public class GraphMLReader
    {
        readonly Graph _Graph;

        string _VertexIdKey = null;
        string _EdgeIdKey = null;
        string _EdgeLabelKey = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph">the graph to populate with the GraphML data</param>
        public GraphMLReader(Graph graph)
        {
            _Graph = graph;
        }

        /// <summary>
        /// if the id of a vertex is a &lt;data/&gt; property, fetch it from the data property.
        /// </summary>
        /// <param name="vertexIdKey"></param>
        public void SetVertexIdKey(string vertexIdKey)
        {
            _VertexIdKey = vertexIdKey;
        }

        /// <summary>
        /// if the id of an edge is a &lt;data/&gt; property, fetch it from the data property.
        /// </summary>
        /// <param name="edgeIdKey"></param>
        void SetEdgeIdKey(string edgeIdKey)
        {
            _EdgeIdKey = edgeIdKey;
        }

        /// <summary>
        /// if the label of an edge is a &lt;data/&gt; property, fetch it from the data property.
        /// </summary>
        /// <param name="edgeLabelKey"></param>
        public void SetEdgeLabelKey(string edgeLabelKey)
        {
            _EdgeLabelKey = edgeLabelKey;
        }

        /// <summary>
        /// Input the GraphML stream data into the graph.
        /// In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="graphMLInputStream">a Stream of GraphML data</param>
        void InputGraph(Stream graphMLInputStream)
        {
            GraphMLReader.InputGraph(_Graph, graphMLInputStream, 1000, _VertexIdKey, _EdgeIdKey, _EdgeLabelKey);
        }

        /// <summary>
        /// Input the GraphML stream data into the graph.
        /// In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="filename">name of a file containing GraphML data</param>
        public void InputGraph(string filename)
        {
            GraphMLReader.InputGraph(_Graph, filename, 1000, _VertexIdKey, _EdgeIdKey, _EdgeLabelKey);
        }

        /// <summary>
        /// Input the GraphML stream data into the graph.
        /// In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="graphMLInputStream">a Stream of GraphML data</param>
        /// <param name="bufferSize">the amount of elements to hold in memory before committing a transactions (only valid for TransactionalGraphs)</param>
        public void InputGraph(Stream graphMLInputStream, int bufferSize)
        {
            GraphMLReader.InputGraph(_Graph, graphMLInputStream, bufferSize, _VertexIdKey, _EdgeIdKey, _EdgeLabelKey);
        }

        /// <summary>
        /// Input the GraphML stream data into the graph.
        /// In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="filename">name of a file containing GraphML data</param>
        /// <param name="bufferSize">the amount of elements to hold in memory before committing a transactions (only valid for TransactionalGraphs)</param>
        public void InputGraph(string filename, int bufferSize)
        {
            GraphMLReader.InputGraph(_Graph, filename, bufferSize, _VertexIdKey, _EdgeIdKey, _EdgeLabelKey);
        }

        /// <summary>
        /// Input the GraphML stream data into the graph.
        /// In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="inputGraph">the graph to populate with the GraphML data</param>
        /// <param name="graphMLInputStream">a Stream of GraphML data</param>
        public static void InputGraph(Graph inputGraph, Stream graphMLInputStream)
        {
            GraphMLReader.InputGraph(inputGraph, graphMLInputStream, 1000, null, null, null);
        }

        /// <summary>
        /// Input the GraphML stream data into the graph.
        /// In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="inputGraph">the graph to populate with the GraphML data</param>
        /// <param name="filename">name of a file containing GraphML data</param>
        public static void InputGraph(Graph inputGraph, string filename)
        {
            GraphMLReader.InputGraph(inputGraph, filename, 1000, null, null, null);
        }

        /// <summary>
        /// Input the GraphML stream data into the graph.
        /// More control over how data is streamed is provided by this method.
        /// </summary>
        /// <param name="inputGraph">the graph to populate with the GraphML data</param>
        /// <param name="filename">name of a file containing GraphML data</param>
        /// <param name="bufferSize">the amount of elements to hold in memory before committing a transactions (only valid for TransactionalGraphs)</param>
        /// <param name="vertexIdKey">if the id of a vertex is a &lt;data/&gt; property, fetch it from the data property.</param>
        /// <param name="edgeIdKey">if the id of an edge is a &lt;data/&gt; property, fetch it from the data property.</param>
        /// <param name="edgeLabelKey">if the label of an edge is a &lt;data/&gt; property, fetch it from the data property.</param>
        public static void InputGraph(Graph inputGraph, string filename, int bufferSize, string vertexIdKey, string edgeIdKey, string edgeLabelKey)
        {
            using (var fis = File.OpenRead(filename))
            {
                GraphMLReader.InputGraph(inputGraph, fis, bufferSize, vertexIdKey, edgeIdKey, edgeLabelKey);
            }
        }

        /// <summary>
        /// Input the GraphML stream data into the graph.
        /// More control over how data is streamed is provided by this method.
        /// </summary>
        /// <param name="inputGraph">the graph to populate with the GraphML data</param>
        /// <param name="graphMLInputStream">a Stream of GraphML data</param>
        /// <param name="bufferSize">the amount of elements to hold in memory before committing a transactions (only valid for TransactionalGraphs)</param>
        /// <param name="vertexIdKey">if the id of a vertex is a &lt;data/&gt; property, fetch it from the data property.</param>
        /// <param name="edgeIdKey">if the id of an edge is a &lt;data/&gt; property, fetch it from the data property.</param>
        /// <param name="edgeLabelKey">if the label of an edge is a &lt;data/&gt; property, fetch it from the data property.</param>
        public static void InputGraph(Graph inputGraph, Stream graphMLInputStream, int bufferSize, string vertexIdKey, string edgeIdKey, string edgeLabelKey)
        {
            using (XmlReader reader = XmlReader.Create(graphMLInputStream))
            {
                BatchGraph graph = BatchGraph.Wrap(inputGraph, bufferSize);

                Dictionary<string, string> keyIdMap = new Dictionary<string, string>();
                Dictionary<string, string> keyTypesMaps = new Dictionary<string, string>();
                // <Mapped ID string, ID object>

                // <Default ID string, Mapped ID string>
                Dictionary<string, string> vertexMappedIdMap = new Dictionary<string, string>();

                // Buffered Vertex Data
                string vertexId = null;
                Dictionary<string, object> vertexProps = null;
                bool inVertex = false;

                // Buffered Edge Data
                string edgeId = null;
                string edgeLabel = null;
                Vertex[] edgeEndVertices = null; //[0] = outVertex , [1] = inVertex
                Dictionary<string, object> edgeProps = null;
                bool inEdge = false;

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        string elementName = reader.Name;

                        if (elementName == GraphMLTokens.KEY)
                        {
                            string id = reader.GetAttribute(GraphMLTokens.ID);
                            string attributeName = reader.GetAttribute(GraphMLTokens.ATTR_NAME);
                            string attributeType = reader.GetAttribute(GraphMLTokens.ATTR_TYPE);
                            keyIdMap[id] = attributeName;
                            keyTypesMaps[id] = attributeType;
                        }
                        else if (elementName == GraphMLTokens.NODE)
                        {
                            vertexId = reader.GetAttribute(GraphMLTokens.ID);
                            if (vertexIdKey != null)
                                vertexMappedIdMap[vertexId] = vertexId;
                            inVertex = true;
                            vertexProps = new Dictionary<string, object>();
                        }
                        else if (elementName == GraphMLTokens.EDGE)
                        {
                            edgeId = reader.GetAttribute(GraphMLTokens.ID);
                            edgeLabel = reader.GetAttribute(GraphMLTokens.LABEL);
                            edgeLabel = edgeLabel == null ? GraphMLTokens._DEFAULT : edgeLabel;

                            string[] vertexIds = new string[2];
                            vertexIds[0] = reader.GetAttribute(GraphMLTokens.SOURCE);
                            vertexIds[1] = reader.GetAttribute(GraphMLTokens.TARGET);
                            edgeEndVertices = new Vertex[2];

                            for (int i = 0; i < 2; i++)
                            { //i=0 => outVertex, i=1 => inVertex
                                if (vertexIdKey == null)
                                {
                                    edgeEndVertices[i] = graph.GetVertex(vertexIds[i]);
                                }
                                else
                                {
                                    edgeEndVertices[i] = graph.GetVertex(vertexMappedIdMap.Get(vertexIds[i]));
                                }

                                if (null == edgeEndVertices[i])
                                {
                                    edgeEndVertices[i] = graph.AddVertex(vertexIds[i]);
                                    if (vertexIdKey != null)
                                        // Default to standard ID system (in case no mapped
                                        // ID is found later)
                                        vertexMappedIdMap[vertexIds[i]] = vertexIds[i];
                                }
                            }

                            inEdge = true;
                            edgeProps = new Dictionary<string, object>();
                        }
                        else if (elementName == GraphMLTokens.DATA)
                        {
                            string key = reader.GetAttribute(null, GraphMLTokens.KEY);
                            string attributeName = keyIdMap.Get(key);

                            if (attributeName != null)
                            {
                                string value = reader.Value;

                                if (inVertex == true)
                                {
                                    if ((vertexIdKey != null) && (key == vertexIdKey))
                                    {
                                        // Should occur at most once per Vertex
                                        // Assumes single ID prop per Vertex
                                        vertexMappedIdMap[vertexId] = value;
                                        vertexId = value;
                                    }
                                    else
                                        vertexProps[attributeName] = TypeCastValue(key, value, keyTypesMaps);
                                }
                                else if (inEdge == true)
                                {
                                    if ((edgeLabelKey != null) && (key == edgeLabelKey))
                                        edgeLabel = value;
                                    else if ((edgeIdKey != null) && (key == edgeIdKey))
                                        edgeId = value;
                                    else
                                        edgeProps[attributeName] = TypeCastValue(key, value, keyTypesMaps);
                                }
                            }
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        string elementName = reader.Name;

                        if (elementName == GraphMLTokens.NODE)
                        {
                            Vertex currentVertex = graph.GetVertex(vertexId);
                            if (currentVertex == null)
                                currentVertex = graph.AddVertex(vertexId);

                            foreach (var prop in vertexProps)
                                currentVertex.SetProperty(prop.Key, prop.Value);

                            vertexId = null;
                            vertexProps = null;
                            inVertex = false;
                        }
                        else if (elementName == GraphMLTokens.EDGE)
                        {
                            Edge currentEdge = graph.AddEdge(edgeId, edgeEndVertices[0], edgeEndVertices[1], edgeLabel);

                            foreach (var prop in edgeProps)
                                currentEdge.SetProperty(prop.Key, prop.Value);

                            edgeId = null;
                            edgeLabel = null;
                            edgeEndVertices = null;
                            edgeProps = null;
                            inEdge = false;
                        }
                    }
                }

                graph.Commit();
            }
        }

        static object TypeCastValue(string key, string value, Dictionary<string, string> keyTypes)
        {
            string type = keyTypes.Get(key);
            if (null == type || type == GraphMLTokens.STRING)
                return value;
            else if (type == GraphMLTokens.FLOAT)
                return float.Parse(value);
            else if (type == GraphMLTokens.INT)
                return int.Parse(value);
            else if (type == GraphMLTokens.DOUBLE)
                return double.Parse(value);
            else if (type == GraphMLTokens.BOOLEAN)
                return bool.Parse(value);
            else if (type == GraphMLTokens.LONG)
                return long.Parse(value);
            else
                return value;
        }
    }
}
