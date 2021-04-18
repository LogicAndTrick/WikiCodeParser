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

            // Standard inline
            Default.Tags.Add(new Tag("b", "strong").WithScopes("inline", "excerpt"));
            Default.Tags.Add(new Tag("i", "em").WithScopes("inline", "excerpt"));
            Default.Tags.Add(new Tag("u", "span", "underline").WithScopes("inline", "excerpt"));
            Default.Tags.Add(new Tag("s", "span", "strikethrough").WithScopes("inline", "excerpt"));
            Default.Tags.Add(new Tag("green", "span", "green").WithScopes("inline", "excerpt"));
            Default.Tags.Add(new Tag("blue", "span", "blue").WithScopes("inline", "excerpt"));
            Default.Tags.Add(new Tag("red", "span", "red").WithScopes("inline", "excerpt"));
            Default.Tags.Add(new Tag("purple", "span", "purple").WithScopes("inline", "excerpt"));
            Default.Tags.Add(new Tag("yellow", "span", "yellow").WithScopes("inline", "excerpt"));

            // Standard block
            Default.Tags.Add(new PreTag());
            Default.Tags.Add(new Tag("h", "h3"));

            // Links
            Default.Tags.Add(new LinkTag().WithScopes("excerpt"));
            Default.Tags.Add(new LinkTag().WithScopes("excerpt").WithToken("email"));
            Default.Tags.Add(new QuickLinkTag());
            // Default.Tags.Add(new WikiLinkTag());
            // Default.Tags.Add(new WikiFileTag());

            // Embedded
            Default.Tags.Add(new ImageTag());
            Default.Tags.Add(new ImageTag().WithToken("simg"));
            // Default.Tags.Add(new WikiImageTag());
            // Default.Tags.Add(new YoutubeTag());
            // Default.Tags.Add(new WikiYoutubeTag());
            Default.Tags.Add(new VaultEmbedTag());

            // Custom
            Default.Tags.Add(new QuoteTag());
            Default.Tags.Add(new FontTag().WithScopes("inline", "excerpt"));
            Default.Tags.Add(new WikiCategoryTag().WithScopes("inline", "excerpt"));
            // Default.Tags.Add(new WikiBookTag().WithScopes("inline", "excerpt"));
            // Default.Tags.Add(new WikiCreditTag().WithScopes("inline", "excerpt"));
            // Default.Tags.Add(new WikiArchiveTag().WithScopes("inline", "excerpt"));
            Default.Tags.Add(new SpoilerTag().WithScopes("inline", "excerpt"));
            Default.Tags.Add(new CodeTag().WithScopes("excerpt"));

            Default.Elements.Add(new MdCodeElement());
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
