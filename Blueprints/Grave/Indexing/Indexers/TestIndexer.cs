﻿using System;
using System.Diagnostics.Contracts;
using Grave.Entities;

namespace Grave.Indexing.Indexers
{
    public class TestIndexer : Indexer
    {
        private readonly Test _content;

        public TestIndexer(IDocument document, Test content) : base(document)
        {
            Contract.Requires(content != null);

            _content = content;
        }

        public override void Index(string documentName)
        {
            Document.Write("Number", _content.Number);
            Document.Write("TableName", _content.Name);
            Document.Write("Day", _content.Day);
            Document.Write("Score", _content.Score);
            Document.Write("Derived",
                           string.Format("This is a derived value created on : {0} at {1}", DateTime.Now,
                                         _content.Number*2));
        }
    }
}