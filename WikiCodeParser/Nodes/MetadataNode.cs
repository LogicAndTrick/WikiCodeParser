using System.Collections.Generic;

namespace WikiCodeParser.Nodes
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
        public IEnumerable<INode> GetChildren() => new INode[0];
    }
}