﻿using System;
using System.Collections.Generic;

namespace LogicAndTrick.WikiCodeParser.Nodes
{
    /// <summary>
    /// Metadata. Has no content, only a key and value for metadata.
    /// </summary>
    public class MetadataNode : INode
    {
        public string Key { get; set; }
        public object Value { get; set; }

        public MetadataNode(string key, object value)
        {
            Key = key;
            Value = value;
        }

        public string ToHtml() => string.Empty;
        public string ToPlainText() => string.Empty;
        public IList<INode> GetChildren() => Array.Empty<INode>();
        public void ReplaceChild(int i, INode node) => throw new InvalidOperationException();
        public bool HasContent() => false;
    }
}