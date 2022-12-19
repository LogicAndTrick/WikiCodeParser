using System.Collections.Generic;
using LogicAndTrick.WikiCodeParser.Elements;
using LogicAndTrick.WikiCodeParser.Processors;
using LogicAndTrick.WikiCodeParser.Tags;

namespace LogicAndTrick.WikiCodeParser
{
    public class ParserConfiguration
    {
        /// <summary>
        /// Create an instance of configuration that matches the configuration of the TWHL website.
        /// </summary>
        /// <returns>Default configuration</returns>
        public static ParserConfiguration Twhl()
        {
            var conf = new ParserConfiguration();

            // Standard inline
            conf.Tags.Add(new Tag("b", "strong").WithScopes("inline", "excerpt"));
            conf.Tags.Add(new Tag("i", "em").WithScopes("inline", "excerpt"));
            conf.Tags.Add(new Tag("u", "span", "underline").WithScopes("inline", "excerpt"));
            conf.Tags.Add(new Tag("s", "span", "strikethrough").WithScopes("inline", "excerpt"));
            conf.Tags.Add(new Tag("green", "span", "green").WithScopes("inline", "excerpt"));
            conf.Tags.Add(new Tag("blue", "span", "blue").WithScopes("inline", "excerpt"));
            conf.Tags.Add(new Tag("red", "span", "red").WithScopes("inline", "excerpt"));
            conf.Tags.Add(new Tag("purple", "span", "purple").WithScopes("inline", "excerpt"));
            conf.Tags.Add(new Tag("yellow", "span", "yellow").WithScopes("inline", "excerpt"));

            // Standard block
            conf.Tags.Add(new PreTag());
            conf.Tags.Add(new Tag("h", "h3").WithBlock(true));

            // Links
            conf.Tags.Add(new LinkTag().WithScopes("excerpt"));
            conf.Tags.Add(new LinkTag().WithScopes("excerpt").WithToken("email"));
            conf.Tags.Add(new QuickLinkTag());
            conf.Tags.Add(new WikiLinkTag());
            conf.Tags.Add(new WikiFileTag());

            // Embedded
            conf.Tags.Add(new ImageTag());
            conf.Tags.Add(new ImageTag().WithToken("simg").WithBlock(false));
            conf.Tags.Add(new WikiImageTag());
            conf.Tags.Add(new YoutubeTag());
            conf.Tags.Add(new WikiYoutubeTag());
            conf.Tags.Add(new VaultEmbedTag());

            // Custom
            conf.Tags.Add(new QuoteTag());
            conf.Tags.Add(new FontTag().WithScopes("inline", "excerpt"));
            conf.Tags.Add(new WikiCategoryTag().WithScopes("inline", "excerpt"));
            conf.Tags.Add(new WikiBookTag().WithScopes("inline", "excerpt"));
            conf.Tags.Add(new WikiCreditTag().WithScopes("inline", "excerpt"));
            conf.Tags.Add(new WikiArchiveTag().WithScopes("inline", "excerpt"));
            conf.Tags.Add(new SpoilerTag().WithScopes("inline", "excerpt"));
            conf.Tags.Add(new CodeTag().WithScopes("excerpt"));

            // Elements
            conf.Elements.Add(new MdCodeElement());
            conf.Elements.Add(new PreElement());
            conf.Elements.Add(new MdHeadingElement());
            conf.Elements.Add(new MdLineElement());
            conf.Elements.Add(new MdQuoteElement());
            conf.Elements.Add(new MdListElement());
            conf.Elements.Add(new MdTableElement());
            conf.Elements.Add(new MdPanelElement());
            conf.Elements.Add(new MdColumnsElement());
            conf.Elements.Add(new RefElement());
            
            // Processors
            conf.Processors.Add(new MarkdownTextProcessor());
            conf.Processors.Add(new AutoLinkingProcessor());
            conf.Processors.Add(new SmiliesProcessor().AddTwhl());
            conf.Processors.Add(new TrimWhitespaceAroundBlockNodesProcessor());
            conf.Processors.Add(new NewLineProcessor());

            return conf;
        }

        /// <summary>
        /// Create an instance of configuration that matches the configuration of the Snarkpit website.
        /// </summary>
        /// <returns>Default configuration</returns>
        public static ParserConfiguration Snarkpit()
        {
            var conf = new ParserConfiguration();

            // Standard inline
            conf.Tags.Add(new Tag("b", "strong").WithScopes("inline", "excerpt"));
            conf.Tags.Add(new Tag("i", "em").WithScopes("inline", "excerpt"));
            conf.Tags.Add(new Tag("u", "span", "underline").WithScopes("inline", "excerpt"));
            conf.Tags.Add(new Tag("s", "span", "strikethrough").WithScopes("inline", "excerpt"));

            // Standard block
            conf.Tags.Add(new PreTag());
            conf.Tags.Add(new Tag("center", "div", "text-center").WithBlock(true));
            // aligntag
            // listtag
            
            // Links
            conf.Tags.Add(new LinkTag().WithScopes("excerpt"));
            conf.Tags.Add(new LinkTag().WithScopes("excerpt").WithToken("email"));
            conf.Tags.Add(new QuickLinkTag());

            // Embedded
            conf.Tags.Add(new ImageTag());
            conf.Tags.Add(new ImageTag().WithToken("simg").WithBlock(false));
            conf.Tags.Add(new WikiImageTag());

            conf.Tags.Add(new YoutubeTag());
            conf.Tags.Add(new WikiYoutubeTag());

            // Custom
            conf.Tags.Add(new QuoteTag());
            // color
            // size
            conf.Tags.Add(new SpoilerTag().WithScopes("inline", "excerpt"));

            // Elements
            conf.Elements.Add(new MdCodeElement());
            conf.Elements.Add(new PreElement { Token = "code" });
            conf.Elements.Add(new MdHeadingElement());
            conf.Elements.Add(new MdLineElement());
            conf.Elements.Add(new MdQuoteElement());
            conf.Elements.Add(new MdListElement());
            conf.Elements.Add(new MdTableElement());
            conf.Elements.Add(new MdPanelElement());
            conf.Elements.Add(new MdColumnsElement());
            conf.Elements.Add(new RefElement());

            // Processors
            conf.Processors.Add(new MarkdownTextProcessor());
            conf.Processors.Add(new AutoLinkingProcessor());
            conf.Processors.Add(new SmiliesProcessor().AddSnarkpit());
            conf.Processors.Add(new TrimWhitespaceAroundBlockNodesProcessor());
            conf.Processors.Add(new NewLineProcessor());

            return conf;
        }

        public List<Element> Elements { get; }
        public List<Tag> Tags { get; }
        public List<INodeProcessor> Processors { get; }

        public ParserConfiguration()
        {
            Elements = new List<Element>();
            Tags = new List<Tag>();
            Processors = new List<INodeProcessor>();
        }
    }
}
