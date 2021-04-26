using System;
using System.Collections.Generic;

namespace WikiCodeParser.Nodes
{
    /// <summary>
    /// Html content. Has HTML before and after, and a node containing the content.
    /// </summary>
    public class HtmlNode : INode
    {
        public string HtmlBefore { get; }
        public INode Content { get; set; }
        public string HtmlAfter { get; }

        public string PlainBefore { get; set; }
        public string PlainAfter { get; set; }

        public HtmlNode(string htmlBefore, INode content, string htmlAfter)
        {
            HtmlBefore = htmlBefore;
            Content = content;
            HtmlAfter = htmlAfter;
            PlainBefore = PlainAfter = String.Empty;
        }

        public string ToHtml()
        {
            return HtmlBefore + Content.ToHtml() + HtmlAfter;
        }

        public string ToPlainText()
        {
            return PlainBefore + Content.ToPlainText() + PlainAfter;
        }

        public IList<INode> GetChildren()
        {
            return new[] {Content};
        }

        public void ReplaceChild(int i, INode node)
        {
            if (i != 0) throw new ArgumentOutOfRangeException();
            Content = node;
        }
    }
}