import { Parser } from '..';
import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { PlainTextNode } from '../Nodes/PlainTextNode';
import { ParseData } from '../ParseData';
import { State } from '../State';
import { TagParseContext } from '../TagParseContext';
import { Tag } from './Tag';


export class WikiYoutubeTag extends Tag {
    constructor() {
        super();
        this.Token = null;
        this.Element = 'div';
        this.MainOption = 'id';
        this.Options = ['id'];
    }

    public override Matches(state: State, _token: string, context: TagParseContext): boolean {
        const peekTag = state.Peek(9);
        const pt = state.PeekTo(']');
        return context == TagParseContext.Block && peekTag == '[youtube:' && pt != null && pt.length > 9 && !pt.includes('\n');
    }

    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    public override Parse(_parser: Parser, _data: ParseData, state: State, _scope: string, _context: TagParseContext): INode {
        const index = state.Index;
        if (state.ScanTo(':') != '[youtube' || state.Next() != ':') {
            state.Seek(index, true);
            return null;
        }

        const str = state.ScanTo(']');
        if (state.Next() != ']') {
            state.Seek(index, true);
            return null;
        }

        const regs = str.match(/^([^|\]]*?)(?:\|([^\]]*?))?$/i);
        if (!regs) {
            state.Seek(index, true);
            return null;
        }

        const id = regs[1];
        const params = regs[2]?.trim().split('|') ?? [];

        if (!WikiYoutubeTag.ValidateID(id)) {
            state.Seek(index, true);
            return null;
        }

        state.SkipWhitespace();

        let caption: string | null = null;
        const classes = ['embedded', 'video'];
        if (this.ElementClass != null) classes.push(this.ElementClass);
        for (const p of params) {
            const l = p.toLowerCase();
            if (WikiYoutubeTag.IsClass(l)) classes.push(l);
            else caption = p.trim();
        }

        if (!caption || caption.trim() == '') caption = null;

        const captionNode = new HtmlNode(
            caption != null ? '<span class="caption">' : '',
            new PlainTextNode(caption ?? ''),
            caption != null ? '</span>' : ''
        );
        captionNode.PlainBefore = '[YouTube video] ';
        captionNode.PlainAfter = '\n';

        const before = `<div class="${classes.join(' ')}">` +
            '<div class="caption-panel">' +
            '<div class="video-container caption-body">' +
            '<div class="video-content">' +
            `<div class="uninitialised" data-youtube-id="${id}" style="background-image: url('https://i.ytimg.com/vi/${id}/hqdefault.jpg');"></div>` +
            '</div>' +
            '</div>';
        const after = '</div></div>';
        const ret = new HtmlNode(before, captionNode, after);
        ret.IsBlockNode = true;
        return ret;
    }

    private static ValidateID(id: string): boolean {
        return id.match(/^[a-zA-Z0-9_-]{6,11}$/i) != null;
    }

    private static ValidClasses: string[] = ['large', 'medium', 'small', 'left', 'right', 'center'];

    private static IsClass(param: string): boolean {
        return WikiYoutubeTag.ValidClasses.includes(param);
    }
}
