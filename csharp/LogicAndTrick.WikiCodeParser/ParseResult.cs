using System;
using System.Collections.Generic;
using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser
{
    public class ParseResult
    {
        public INode Content { get; set; }

        public ParseResult()
        {
            Content = new NodeCollection();
        }

        public List<KeyValuePair<string, object>> GetMetadata()
        {
            var list = new List<KeyValuePair<string, object>>();
            Content.Walk(n =>
            {
                if (n is MetadataNode md) list.Add(new KeyValuePair<string, object>(md.Key, md.Value));
            });
            return list;
        }

        /// <summary>
        /// Convert the result to HTML and sanitise it with a given callback.
        /// Supplying a valid sanitiser is the responsibility of the caller.
        /// </summary>
        public string ToHtml(Func<string, string> sanitiser) => sanitiser(ToHtmlUnsanitised());

        /// <summary>
        /// Convert the result to HTML without a sanitiser callback.
        /// Its possible malicious input could include injection attacks. Use at your own risk.
        /// </summary>
        public string ToHtmlUnsanitised() => Content.ToHtml();

        /// <summary>
        /// Convert the result to plain text.
        /// </summary>
        public string ToPlainText() => Content.ToPlainText();
    }
}