import { HtmlHelper } from '../HtmlHelper';
import { WikiRevision } from '../Models/WikiRevision';
import { HtmlNodeBuilder } from '../Nodes/HtmlNodeBuilder';
import { INode } from '../Nodes/INode';
import { MetadataNode } from '../Nodes/MetadataNode';
import { NodeCollection } from '../Nodes/NodeCollection';
import { PlainTextNode } from '../Nodes/PlainTextNode';
import { UnprocessablePlainTextNode } from '../Nodes/UnprocessablePlainTextNode';
import { ParseData } from '../ParseData';
import { Parser } from '../Parser';
import { State } from '../State';
import { TagParseContext } from '../TagParseContext';
import { Trim } from '../Util';
import { Tag } from './Tag';
export class WikiImageTag extends Tag {
    public TwhlBehaviour = false;

    constructor() {
        super();
        this.Token = null;
        this.Element = 'img';
    }

    private static Tags = ['img', 'video', 'audio'];

    private static GetTag(state: State): string | null {
        for (const tag of this.Tags) {
            const peekTag = state.Peek(2 + tag.length);
            const pt = state.PeekTo(']');
            if (peekTag == `[${tag}:` && pt && pt.length > 2 + tag.length && !pt.includes('\n')) return tag;
        }

        return null;
    }

    public override Matches(state: State, _token: string | null, _context: TagParseContext): boolean {
        const tag = WikiImageTag.GetTag(state);
        return tag != null;
    }

    public override Parse(_parser: Parser, _data: ParseData, state: State, _scope: string, context: TagParseContext): INode | null {
        const index = state.Index;

        const tag = WikiImageTag.GetTag(state);
        if (!tag || state.ScanTo(':') != `[${tag}` || state.Next() != ':') {
            state.Seek(index, true);
            return null;
        }

        const str = state.ScanTo(']');

        if (state.Next() != ']') {
            state.Seek(index, true);
            return null;
        }

        const match = str.match(/^([^|\]]*?)(?:\|([^\]]*?))?$/i);

        if (!match) {
            state.Seek(index, true);
            return null;
        }

        const content = new NodeCollection();

        const image = match[1];
        const params = match[2] ? Trim(match[2]).split('|') : [];
        let src = image;
        if (!image.includes('/')) {
            if (this.TwhlBehaviour) {
                content.Nodes.push(new MetadataNode('WikiUpload', image));
                src = `https://twhl.info/wiki/embed/${WikiRevision.CreateSlug(image)}`;
            } else {
                state.Seek(index, true);
                return null;
            }
        } else {
            src = HtmlHelper.StripControlCharacters(src);
            if (!HtmlHelper.ValidateUrl(src)) {
                state.Seek(index, true);
                return null;
            }
        }

        let url: string | null = null;
        let caption: string | null = null;
        let loop = false;

        const classes = ['embedded', 'image'];
        if (this.ElementClass != null) classes.push(this.ElementClass);

        for (const p of params) {
            const l = p.toLowerCase();
            if (WikiImageTag.IsClass(l)) classes.push(l);
            else if (l == 'loop') loop = true;
            else if (l.length > 4 && l.substring(0, 4) == 'url:') url = Trim(p.substring(4));
            else caption = Trim(p);
        }

        if (!caption || caption.trim() == '') caption = null;

        if (url != null) url = HtmlHelper.StripControlCharacters(url);
        if (tag == 'img' && url != null && HtmlHelper.ValidateUrl(url)) {
            if (this.TwhlBehaviour && HtmlHelper.GetUrlScheme(url) == null) {
                content.Nodes.push(new MetadataNode('WikiLink', url));
                url = `https://twhl.info/wiki/page/${WikiRevision.CreateSlug(url)}`;
            }
        } else {
            url = '';
        }

        let el = 'span';

        // Force inline if we are in an inline context
        if (context == TagParseContext.Inline && !classes.includes('inline')) classes.push('inline');

        // Non-inline images should eat any whitespace after them
        if (!classes.includes('inline')) {
            state.SkipWhitespace();
            el = 'div';
        }

        const embed = WikiImageTag.GetEmbedObject(tag, src, caption, loop);
        if (embed != null) content.Nodes.push(embed);
        if (caption != null) content.Nodes.push(HtmlNodeBuilder.make('span').addClass('caption').add(new PlainTextNode(caption)).toNode({ plainBefore: ' ' }));

        const panel = HtmlNodeBuilder.make('span').addClass('caption-panel').add(content).toNode();
        const wrapper = !url || url.length == 0 ? panel : HtmlNodeBuilder.make('a').attr('href', url).add(panel).toNode();

        return HtmlNodeBuilder.make(el)
            .addClasses(classes)
            .if(!!caption && caption.length > 0, x => x.attr('title', caption))
            .add(wrapper)
            .toNode({ isBlockNode: el == 'div' });
    }

    private static GetEmbedObject(tag: string, url: string, caption: string | null, loop: boolean): INode | null {
        switch (tag) {
            case 'img':
                return HtmlNodeBuilder.make('img')
                    .addClass('caption-body')
                    .attr('src', url)
                    .attr('alt', caption ?? 'User posted image')
                    .toNode({ plainBefore: '[Image]', selfClosing: true });
            case 'video':
            case 'audio':
                return HtmlNodeBuilder.make(tag)
                    .addClass('caption-body')
                    .attr('src', url)
                    .prop('playsinline')
                    .prop('controls')
                    .if(loop, x => x.prop('autoplay').prop('loop').prop('muted'))
                    .add(new UnprocessablePlainTextNode(`Your browser doesn't support embedded ${tag}.`))
                    .toNode({ plainBefore: tag.substring(0, 1).toUpperCase() + tag.substring(1), selfClosing: true });
            default:
                return null;
        }
    }

    private static ValidClasses: string[] = ['large', 'medium', 'small', 'thumb', 'left', 'right', 'center', 'inline'];

    private static IsClass(param: string): boolean {
        return WikiImageTag.ValidClasses.includes(param);
    }
}
