import { Element } from './Elements/Element';
import { MdCodeElement } from './Elements/MdCodeElement';
import { MdColumnsElement } from './Elements/MdColumnsElement';
import { MdHeadingElement } from './Elements/MdHeadingElement';
import { MdLineElement } from './Elements/MdLineElement';
import { MdListElement } from './Elements/MdListElement';
import { MdPanelElement } from './Elements/MdPanelElement';
import { MdQuoteElement } from './Elements/MdQuoteElement';
import { MdTableElement } from './Elements/MdTableElement';
import { PreElement } from './Elements/PreElement';
import { QuoteElement } from './Elements/QuoteElement';
import { RefElement } from './Elements/RefElement';
import { AutoLinkingProcessor } from './Processors/AutoLinkingProcessor';
import { INodeProcessor } from './Processors/INodeProcessor';
import { MarkdownTextProcessor } from './Processors/MarkdownTextProcessor';
import { NewLineProcessor } from './Processors/NewLineProcessor';
import { SmiliesProcessor } from './Processors/SmiliesProcessor';
import { TrimWhitespaceAroundBlockNodesProcessor } from './Processors/TrimWhitespaceAroundBlockNodesProcessor';
import { AlignTag } from './Tags/AlignTag';
import { CodeTag } from './Tags/CodeTag';
import { ColorTag } from './Tags/ColorTag';
import { FontTag } from './Tags/FontTag';
import { ImageTag } from './Tags/ImageTag';
import { LinkTag } from './Tags/LinkTag';
import { ListTag } from './Tags/ListTag';
import { PreTag } from './Tags/PreTag';
import { QuickLinkTag } from './Tags/QuickLinkTag';
import { SizeTag } from './Tags/SizeTag';
import { SpoilerTag } from './Tags/SpoilerTag';
import { Tag } from './Tags/Tag';
import { VaultEmbedTag } from './Tags/VaultEmbedTag';
import { WikiArchiveTag } from './Tags/WikiArchiveTag';
import { WikiBookTag } from './Tags/WikiBookTag';
import { WikiCategoryTag } from './Tags/WikiCategoryTag';
import { WikiCreditTag } from './Tags/WikiCreditTag';
import { WikiFileTag } from './Tags/WikiFileTag';
import { WikiImageTag } from './Tags/WikiImageTag';
import { WikiLinkTag } from './Tags/WikiLinkTag';
import { WikiYoutubeTag } from './Tags/WikiYoutubeTag';
import { YoutubeTag } from './Tags/YoutubeTag';

export class ParserConfiguration {

    public static Twhl(): ParserConfiguration {
        const conf = new ParserConfiguration();

        // // Standard inline
        conf.Tags.push(new Tag('b', 'strong').WithScopes('inline', 'excerpt'));
        conf.Tags.push(new Tag('i', 'em').WithScopes('inline', 'excerpt'));
        conf.Tags.push(new Tag('u', 'span', 'underline').WithScopes('inline', 'excerpt'));
        conf.Tags.push(new Tag('s', 'span', 'strikethrough').WithScopes('inline', 'excerpt'));
        conf.Tags.push(new Tag('green', 'span', 'green').WithScopes('inline', 'excerpt'));
        conf.Tags.push(new Tag('blue', 'span', 'blue').WithScopes('inline', 'excerpt'));
        conf.Tags.push(new Tag('red', 'span', 'red').WithScopes('inline', 'excerpt'));
        conf.Tags.push(new Tag('purple', 'span', 'purple').WithScopes('inline', 'excerpt'));
        conf.Tags.push(new Tag('yellow', 'span', 'yellow').WithScopes('inline', 'excerpt'));

        // Standard block
        conf.Tags.push(new PreTag());
        conf.Tags.push(new Tag('h', 'h3').WithBlock(true));

        // Links
        conf.Tags.push(new LinkTag().WithScopes('excerpt'));
        conf.Tags.push(new LinkTag().WithScopes('excerpt').WithToken('email'));
        conf.Tags.push(new QuickLinkTag());
        conf.Tags.push(new WikiLinkTag());
        conf.Tags.push(new WikiFileTag());

        // Embedded
        conf.Tags.push(new ImageTag());
        conf.Tags.push(new ImageTag().WithToken('simg').WithBlock(false));
        const wikiImageTag = new WikiImageTag();
        wikiImageTag.TwhlBehaviour = true;
        conf.Tags.push(wikiImageTag);
        conf.Tags.push(new YoutubeTag());
        conf.Tags.push(new WikiYoutubeTag());
        conf.Tags.push(new VaultEmbedTag());

        // Custom
        conf.Tags.push(new FontTag().WithScopes('inline', 'excerpt'));
        conf.Tags.push(new WikiCategoryTag().WithScopes('inline', 'excerpt'));
        conf.Tags.push(new WikiBookTag().WithScopes('inline', 'excerpt'));
        conf.Tags.push(new WikiCreditTag().WithScopes('inline', 'excerpt'));
        conf.Tags.push(new WikiArchiveTag().WithScopes('inline', 'excerpt'));
        conf.Tags.push(new SpoilerTag().WithScopes('inline', 'excerpt'));
        conf.Tags.push(new CodeTag().WithScopes('excerpt'));

        // Elements
        conf.Elements.push(new MdCodeElement());
        conf.Elements.push(new PreElement());
        conf.Elements.push(new MdHeadingElement());
        conf.Elements.push(new MdLineElement());
        conf.Elements.push(new MdQuoteElement());
        conf.Elements.push(new MdListElement());
        conf.Elements.push(new MdTableElement());
        conf.Elements.push(new MdPanelElement());
        conf.Elements.push(new MdColumnsElement());
        conf.Elements.push(new RefElement());
        conf.Elements.push(new QuoteElement());

        // Processors
        conf.Processors.push(new MarkdownTextProcessor());
        conf.Processors.push(new AutoLinkingProcessor());
        conf.Processors.push(new SmiliesProcessor('https://twhl.info/images/smilies/{0}.png').AddTwhl());
        conf.Processors.push(new TrimWhitespaceAroundBlockNodesProcessor());
        conf.Processors.push(new NewLineProcessor());

        return conf;
    }
    
    public static Snarkpit(): ParserConfiguration {
        const conf = new ParserConfiguration();

        // Standard inline
        conf.Tags.push(new Tag('b', 'strong').WithScopes('inline', 'excerpt'));
        conf.Tags.push(new Tag('i', 'em').WithScopes('inline', 'excerpt'));
        conf.Tags.push(new Tag('u', 'span', 'underline').WithScopes('inline', 'excerpt'));
        conf.Tags.push(new Tag('s', 'span', 'strikethrough').WithScopes('inline', 'excerpt'));

        // Standard block
        conf.Tags.push(new PreTag());
        conf.Tags.push(new Tag('center', 'div', 'text-center').WithBlock(true));
        conf.Tags.push(new AlignTag());
        conf.Tags.push(new ListTag());
        
        // Links
        conf.Tags.push(new LinkTag().WithScopes('excerpt'));
        conf.Tags.push(new LinkTag().WithScopes('excerpt').WithToken('email'));
        conf.Tags.push(new QuickLinkTag());

        // Embedded
        conf.Tags.push(new ImageTag());
        conf.Tags.push(new ImageTag().WithToken('simg').WithBlock(false));
        conf.Tags.push(new WikiImageTag());

        conf.Tags.push(new YoutubeTag());
        conf.Tags.push(new WikiYoutubeTag());

        // Custom
        conf.Tags.push(new ColorTag());
        conf.Tags.push(new SizeTag());
        conf.Tags.push(new SpoilerTag().WithScopes('inline', 'excerpt'));

        // Elements
        conf.Elements.push(new MdCodeElement());
        const preElement = new PreElement();
        preElement.Token = 'code';
        conf.Elements.push(preElement);
        conf.Elements.push(new MdHeadingElement());
        conf.Elements.push(new MdLineElement());
        conf.Elements.push(new MdQuoteElement());
        conf.Elements.push(new MdListElement());
        conf.Elements.push(new MdTableElement());
        conf.Elements.push(new MdPanelElement());
        conf.Elements.push(new MdColumnsElement());
        conf.Elements.push(new RefElement());
        conf.Elements.push(new QuoteElement());

        // Processors
        conf.Processors.push(new MarkdownTextProcessor());
        conf.Processors.push(new AutoLinkingProcessor());
        conf.Processors.push(new SmiliesProcessor('https://snarkpit.net/images/smilies/{0}.gif').AddSnarkpit());
        conf.Processors.push(new TrimWhitespaceAroundBlockNodesProcessor());
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