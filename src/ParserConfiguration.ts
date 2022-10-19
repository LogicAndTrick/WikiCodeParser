import { Element } from './Elements/Element';
import { MdHeadingElement } from './Elements/MdHeadingElement';
import { PreElement } from './Elements/PreElement';
import { RefElement } from './Elements/RefElement';
import { INodeProcessor } from './Processors/INodeProcessor';
import { NewLineProcessor } from './Processors/NewLineProcessor';
import { Tag } from './Tags/Tag';

export class ParserConfiguration {

    public static Default(): ParserConfiguration {
        const conf = new ParserConfiguration();

        // // Standard inline
        // conf.Tags.push(new Tag("b", "strong").WithScopes("inline", "excerpt"));
        // conf.Tags.push(new Tag("i", "em").WithScopes("inline", "excerpt"));
        // conf.Tags.push(new Tag("u", "span", "underline").WithScopes("inline", "excerpt"));
        // conf.Tags.push(new Tag("s", "span", "strikethrough").WithScopes("inline", "excerpt"));
        // conf.Tags.push(new Tag("green", "span", "green").WithScopes("inline", "excerpt"));
        // conf.Tags.push(new Tag("blue", "span", "blue").WithScopes("inline", "excerpt"));
        // conf.Tags.push(new Tag("red", "span", "red").WithScopes("inline", "excerpt"));
        // conf.Tags.push(new Tag("purple", "span", "purple").WithScopes("inline", "excerpt"));
        // conf.Tags.push(new Tag("yellow", "span", "yellow").WithScopes("inline", "excerpt"));

        // // Standard block
        // conf.Tags.push(new PreTag());
        // conf.Tags.push(new Tag("h", "h3"));

        // // Links
        // conf.Tags.push(new LinkTag().WithScopes("excerpt"));
        // conf.Tags.push(new LinkTag().WithScopes("excerpt").WithToken("email"));
        // conf.Tags.push(new QuickLinkTag());
        // conf.Tags.push(new WikiLinkTag());
        // conf.Tags.push(new WikiFileTag());

        // // Embedded
        // conf.Tags.push(new ImageTag());
        // conf.Tags.push(new ImageTag().WithToken("simg").WithBlock(false));
        // conf.Tags.push(new WikiImageTag());
        // conf.Tags.push(new YoutubeTag());
        // conf.Tags.push(new WikiYoutubeTag());
        // conf.Tags.push(new VaultEmbedTag());

        // // Custom
        // conf.Tags.push(new QuoteTag());
        // conf.Tags.push(new FontTag().WithScopes("inline", "excerpt"));
        // conf.Tags.push(new WikiCategoryTag().WithScopes("inline", "excerpt"));
        // conf.Tags.push(new WikiBookTag().WithScopes("inline", "excerpt"));
        // conf.Tags.push(new WikiCreditTag().WithScopes("inline", "excerpt"));
        // conf.Tags.push(new WikiArchiveTag().WithScopes("inline", "excerpt"));
        // conf.Tags.push(new SpoilerTag().WithScopes("inline", "excerpt"));
        // conf.Tags.push(new CodeTag().WithScopes("excerpt"));

        // Elements
        //conf.Elements.push(new MdCodeElement());
        conf.Elements.push(new PreElement());
        conf.Elements.push(new MdHeadingElement());
        //conf.Elements.push(new MdLineElement());
        //conf.Elements.push(new MdQuoteElement());
        //conf.Elements.push(new MdListElement());
        //conf.Elements.push(new MdTableElement());
        //conf.Elements.push(new MdPanelElement());
        //conf.Elements.push(new MdColumnsElement());
        conf.Elements.push(new RefElement());

        // // Processors
        // conf.Processors.push(new MarkdownTextProcessor());
        // conf.Processors.push(new AutoLinkingProcessor());
        // conf.Processors.push(new SmiliesProcessor().pushDefault());
        // conf.Processors.push(new TrimWhitespaceAroundBlockNodesProcessor());
        conf.Processors.push(new NewLineProcessor());

        return conf;
    }

    public Elements: Element[];
    public Tags: Tag[];
    public Processors: INodeProcessor[];

    public constructor() {
        this.Elements = [];
        this.Tags = [];
        this.Processors = [];
    }
}