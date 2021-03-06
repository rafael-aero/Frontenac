﻿using System;
using System.Collections;
using System.Diagnostics.Contracts;
using System.Reflection;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq.RelationAccessors
{
    internal abstract class RelationAccessor
    {
        protected readonly Type ModelType;
        public readonly bool IsEnumerable;
        protected readonly bool IsWrapped;
        public readonly bool IsCollection;

        protected delegate object ConvertDelegate(IEnumerable elements, bool isWrapped, bool isEnumerable);
        protected readonly ConvertDelegate ConvertMethod;

        internal delegate object CreateCollectionDelegate(IElement element, string key, RelationAccessor accessor);
        internal readonly CreateCollectionDelegate CreateCollectionMethod;

        protected RelationAccessor(Type modelType, bool isEnumerable, bool isCollection, bool isWrapped, Type[] models)
        {
            ModelType = modelType;
            IsEnumerable = isEnumerable;
            IsWrapped = isWrapped;
            IsCollection = isCollection;

            ConvertMethod = (ConvertDelegate)CreateMagicMethod("Convert", typeof(ConvertDelegate), models);
            CreateCollectionMethod = (CreateCollectionDelegate)CreateMagicMethod("CreateCollection", typeof(CreateCollectionDelegate), models);
        }

        //To boost reflected method invocation speed
        protected Delegate CreateMagicMethod(string delegateName, Type delegateType, params Type[] modelTypes)
        {
            Contract.Requires(delegateName != null);

            var method = GetType().GetMethod(delegateName, BindingFlags.Static | BindingFlags.NonPublic);
            var genericMethod = method.MakeGenericMethod(modelTypes);
            return Delegate.CreateDelegate(delegateType, genericMethod);
        }

        public abstract object GetRelations(IElement element, string key);

        public static Direction DirectionFromKey(string key, out string newKey)
        {
            Contract.Requires(!string.IsNullOrEmpty(key));

            Direction result;
            var minKey = key.ToLowerInvariant();
            
            if (minKey.StartsWith("in"))
            {
                result = Direction.In;
                newKey = key.Substring(2);
            }
            else if (minKey.StartsWith("out"))
            {
                result = Direction.Out;
                newKey = key.Substring(3);
            }
            else if (minKey.StartsWith("both"))
            {
                result = Direction.Both;
                newKey = key.Substring(4);
            }
            else
            {
                newKey = key;
                result = Direction.Out;
            }
            
            return result;
        }
    }
}