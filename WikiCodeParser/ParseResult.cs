using System.Collections.Generic;
using WikiCodeParser.Nodes;

namespace WikiCodeParser
{
    public class ParseResult
    {
        public INode Content { get; set; }

        public ParseResult()
        {
            Content = new NodeCollection();
        }

        public List<KeyValuePair<string, string>> GetMetadata()
        {
            var list = new List<KeyValuePair<string, string>>();
            Content.Walk(n =>
            {
                if (n is MetadataNode md) list.Add(new KeyValuePair<string, string>(md.Key, md.Value));
            });
            return list;
        }

        public string ToHtml() => Content.ToHtml();
        public string ToPlainText() => Content.ToPlainText();
    }
}