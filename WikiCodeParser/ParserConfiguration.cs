using System.Collections.Generic;
using WikiCodeParser.Elements;
using WikiCodeParser.Tags;

namespace WikiCodeParser
{
    public class ParserConfiguration
    {
        public static readonly ParserConfiguration Default;

        static ParserConfiguration()
        {
            Default = new ParserConfiguration();
            Default.Elements.Add(new MdCodeElement());

            Default.Tags.Add(new CodeTag());
            Default.Tags.Add(new FontTag());
            Default.Tags.Add(new ImageTag());
            Default.Tags.Add(new LinkTag());
            Default.Tags.Add(new PreTag());
            Default.Tags.Add(new QuickLinkTag());
            Default.Tags.Add(new QuoteTag());
            Default.Tags.Add(new WikiCategoryTag());
        }

        public List<Element> Elements { get; }
        public List<Tag> Tags { get; }

        public ParserConfiguration()
        {
            Elements = new List<Element>();
            Tags = new List<Tag>();
        }
    }
}
