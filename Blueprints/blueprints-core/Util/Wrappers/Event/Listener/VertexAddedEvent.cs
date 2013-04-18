﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    public class VertexAddedEvent : Event
    {
        readonly Vertex _vertex;

        public VertexAddedEvent(Vertex vertex)
        {
            _vertex = vertex;
        }

        public void fireEvent(IEnumerator<GraphChangedListener> eventListeners)
        {
            while (eventListeners.MoveNext())
            {
                eventListeners.Current.vertexAdded(_vertex);
            }
        }
    }
}
