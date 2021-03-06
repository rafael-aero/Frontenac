﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Event
{
    /// <summary>
    ///     An index that wraps graph elements in the "evented" way. This class does not directly raise graph events, but
    ///     passes the GraphChangedListener to the edges and vertices returned from indices so that they may raise graph
    ///     events.
    /// </summary>
    public class EventIndex : IIndex
    {
        protected readonly IIndex RawIndex;
        private readonly EventGraph _eventGraph;

        public EventIndex(IIndex rawIndex, EventGraph eventGraph)
        {
            Contract.Requires(rawIndex != null);
            Contract.Requires(eventGraph != null);

            RawIndex = rawIndex;
            _eventGraph = eventGraph;
        }

        public void Remove(string key, object value, IElement element)
        {
            var eventElement = element as EventElement;
            if (eventElement != null)
                RawIndex.Remove(key, value, eventElement.GetBaseElement());
        }

        public void Put(string key, object value, IElement element)
        {
            var eventElement = element as EventElement;
            if (eventElement != null) RawIndex.Put(key, value, eventElement.GetBaseElement());
        }

        public IEnumerable<IElement> Get(string key, object value)
        {
            if (typeof (IVertex).IsAssignableFrom(Type))
                return new EventVertexIterable((IEnumerable<IVertex>) RawIndex.Get(key, value), _eventGraph);
            return new EventEdgeIterable((IEnumerable<IEdge>) RawIndex.Get(key, value), _eventGraph);
        }

        public IEnumerable<IElement> Query(string key, object value)
        {
            if (typeof (IVertex).IsAssignableFrom(Type))
                return new EventVertexIterable((IEnumerable<IVertex>) RawIndex.Query(key, value), _eventGraph);
            return new EventEdgeIterable((IEnumerable<IEdge>) RawIndex.Query(key, value), _eventGraph);
        }

        public long Count(string key, object value)
        {
            return RawIndex.Count(key, value);
        }

        public string Name
        {
            get { return RawIndex.Name; }
        }

        public Type Type
        {
            get { return RawIndex.Type; }
        }

        public override string ToString()
        {
            return this.IndexString();
        }
    }
}