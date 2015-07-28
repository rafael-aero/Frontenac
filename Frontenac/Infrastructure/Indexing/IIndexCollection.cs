﻿using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Frontenac.Blueprints;

namespace Frontenac.Infrastructure.Indexing
{
    public class IntervalQueryElement : QueryElement
    {
        private object _endValue;
        private object _startValue;

        public IntervalQueryElement(string key, object startValue, object endValue)
            : base(key)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
            Contract.Requires(startValue != null);
            Contract.Requires(endValue != null);

            EndValue = endValue;
            StartValue = startValue;
        }

        public object StartValue
        {
            get
            {
                Contract.Ensures(Contract.Result<object>() != null);
                return _startValue;
            }
            private set
            {
                Contract.Requires(value != null);
                _startValue = value;
            }
        }

        public object EndValue
        {
            get
            {
                Contract.Ensures(Contract.Result<object>() != null);
                return _endValue;
            }
            private set
            {
                Contract.Requires(value != null);
                _endValue = value;
            }
        }
    }

    [ContractClass(typeof(IndexCollectionContract))]
    public interface IIndexCollection
    {
        void CreateIndex(string indexName, Parameter[] parameters);
        long DropIndex(string indexName);
        IEnumerable<string> GetIndices();
        bool HasIndex(string indexName);
        long Set(long id, string indexName, string key, object value);
        void WaitForGeneration(long generation);
        IEnumerable<long> Get(string term, object value, int hitsLimit = 1000);
        IEnumerable<long> Get(string indexName, string key, object value, int hitsLimit = 1000);
        long DeleteDocuments(long id);
        long DeleteIndex(string indexName);
        void Commit();
        void Rollback();
    }
}