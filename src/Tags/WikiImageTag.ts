import { Parser } from '..';
import { HtmlHelper } from '../HtmlHelper';
import { WikiRevision } from '../Models/WikiRevision';
import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { MetadataNode } from '../Nodes/MetadataNode';
import { NodeCollection } from '../Nodes/NodeCollection';
import { PlainTextNode } from '../Nodes/PlainTextNode';
import { ParseData } from '../ParseData';
import { State } from '../State';
import { TagParseContext } from '../TagParseContext';
import { Tag } from './Tag';

export class WikiImageTag extends Tag {
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
            if (peekTag == `[${tag}:` && pt?.length > 2 + tag.length && !pt.includes('\n')) return tag;
        }

        return null;
    }

    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    public override Matches(state: State, _token: string, _context: TagParseContext): boolean {
        const tag = WikiImageTag.GetTag(state);
        return tag != null;
    }

    public override Parse(_parser: Parser, _data: ParseData, state: State, _scope: string, context: TagParseContext): INode {
        const index = state.Index;

        const tag = WikiImageTag.GetTag(state);
        if (state.ScanTo(':') != `[${tag}` || state.Next() != ':') {
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
        const params = match[2] ? match[2].trim().split('|') : [];
        let src = image;
        if (!image.includes('/')) {
            content.Nodes.push(new MetadataNode('WikiUpload', image));
            src = `https://twhl.info/wiki/embed/${WikiRevision.CreateSlug(image)}`;
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
            else if (l.length > 4 && l.substring(0, 4) == 'url:') url = p.substring(4).trim();
            else caption = p.trim();
        }

        if (!caption || caption.trim() == '') caption = null;

        if (tag == 'img' && url != null && WikiImageTag.ValidateUrl(url)) {
            if (!url.match(/^[a-z]{2,10}:\/\//i)) {
                content.Nodes.push(new MetadataNode('WikiLink', url));
                url = `https://twhl.info/wiki/page/${WikiRevision.CreateSlug(url)}`;
            }
        }
        else {
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

        if (caption != null) {
            const cn = new HtmlNode('<span class="caption">', new PlainTextNode(caption), '</span>');
            cn.PlainAfter = '\n';
            content.Nodes.push(cn);
        }

        const before = `<${el} class="${classes.join(' ')}"` + (caption?.length > 0 ? ` title="${HtmlHelper.AttributeEncode(caption)}"` : '') + '>'
            + (url.length > 0 ? '<a href="' + HtmlHelper.AttributeEncode(url) + '">' : '')
            + '<span class="caption-panel">';
        const after = '</span>'
            + (url.length > 0 ? '</a>' : '')
            + `</${el}>`;

        const ret = new HtmlNode(before, content, after);
        ret.IsBlockNode = el == 'div';
        return ret;
    }

    private static GetEmbedObject(tag: string, url: string, caption: string, loop: boolean): INode {
        url = HtmlHelper.AttributeEncode(url);
        switch (tag) {
            case 'img':
                {
                    caption = caption ?? 'User posted image';
                    const cap = HtmlHelper.AttributeEncode(caption);
                    const ret = new HtmlNode(`<img class="caption-body" src="${url}" alt="${cap}" />`, PlainTextNode.Empty(), '');
                    ret.PlainBefore = '[Image] ';
                    return ret;
                }
            case 'video':
            case 'audio':
                {
                    let auto = '';
                    if (loop) auto = 'autoplay loop muted';
                    const ret = new HtmlNode(`<${tag} class="caption-body" src="${url}" playsinline controls ${auto}>Your browser doesn't support embedded ${tag}.</${tag}>`, PlainTextNode.Empty(), '');
                    ret.PlainBefore = tag.substring(0, 1).toUpperCase() + tag.substring(1);
                    return ret;
                }
        }

        return null;
    }

    private static ValidateUrl(url: string): boolean {
        return !url.includes('<script');
    }

    private static ValidClasses: string[] = ['large', 'medium', 'small', 'thumb', 'left', 'right', 'center', 'inline'];

    private static IsClass(param: string): boolean {
        return WikiImageTag.ValidClasses.includes(param);
    }
}