import { Parser } from '..';
import { HtmlHelper } from '../HtmlHelper';
import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { UnprocessablePlainTextNode } from '../Nodes/UnprocessablePlainTextNode';
import { ParseData } from '../ParseData';
import { State } from '../State';
import { TagParseContext } from '../TagParseContext';
import { Tag } from './Tag';

export class QuickLinkTag extends Tag {
    constructor() {
        super();
        this.Token = null;
        this.Element = 'a';
        this.MainOption = 'url';
        this.Options = ['url'];
    }

    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    public override Matches(state: State, _token: string, _context: TagParseContext): boolean {
        let pt = state.PeekTo(']');
        if (!pt || pt == '') return false;

        pt = pt.substring(1);
        return pt.length > 0 && !pt.includes('\n') && pt.match(/^([a-z]{2,10}:\/\/[^\]]*?)(?:\|([^\]]*?))?/i) != null;
    }

    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    public override Parse(_parser: Parser, _data: ParseData, state: State, _scope: string, _context: TagParseContext): INode {
        const index = state.Index;

        if (state.Next() != '[') {
            state.Seek(index, true);
            return null;
        }

        const str = state.ScanTo(']');
        if (state.Next() != ']') {
            state.Seek(index, true);
            return null;
        }

        const match = str.match(/^([a-z]{2,10}:\/\/[^\]]*?)(?:\|([^\]]*?))?$/i);
        if (!match) {
            state.Seek(index, true);
            return null;
        }

        let url = match[1];
        const text = match[2]?.length > 0 ? match[2] : url;
        const options = { url };
        if (!this.Validate(options, text)) {
            state.Seek(index, true);
            return null;
        }

        url = HtmlHelper.AttributeEncode(url);
        const before = `<${this.Element} href="${url}">`;
        const after = `</${this.Element}>`;

        const content = new UnprocessablePlainTextNode(text);
        const ret = new HtmlNode(before, content, after);
        ret.PlainAfter = match[2]?.length > 0 ? ` (${url})` : '';
        return ret;
    }

    public override Validate(options: Record<string, string>, text: string): boolean {
        let url = text;
        if (options['url']) url = options['url'];
        return !url.includes('<script') && url.match(/^([a-z]{2,10}:\/\/)?([^\]""\n ]+?)/i) != null;
    }
}