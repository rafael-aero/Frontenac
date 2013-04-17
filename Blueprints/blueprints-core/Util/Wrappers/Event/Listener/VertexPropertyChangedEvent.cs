﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    public class VertexPropertyChangedEvent : VertexPropertyEvent
    {
        public VertexPropertyChangedEvent(Vertex vertex, string key, object oldValue, object newValue)
            : base(vertex, key, oldValue, newValue)
        {

        }

        protected override void Fire(GraphChangedListener listener, Vertex vertex, string key, object oldValue, object newValue)
        {
            listener.VertexPropertyChanged(vertex, key, oldValue, newValue);
        }
    }
}