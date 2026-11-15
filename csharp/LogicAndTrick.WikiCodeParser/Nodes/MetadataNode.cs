using System;
using System.Collections.Generic;

namespace LogicAndTrick.WikiCodeParser.Nodes
{
    /// <summary>
    /// Metadata. Has no content, only a key and value for metadata.
    /// </summary>
    public class MetadataNode : INode
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public string Key { get; set; }
        public object Value { get; set; }

        public MetadataNode(Position pos, string key, object value)
        {
            Line = pos.Line;
            Column = pos.Column;
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