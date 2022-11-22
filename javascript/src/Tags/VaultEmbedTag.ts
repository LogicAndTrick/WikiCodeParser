import { Parser } from '..';
import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { PlainTextNode } from '../Nodes/PlainTextNode';
import { ParseData } from '../ParseData';
import { State } from '../State';
import { TagParseContext } from '../TagParseContext';
import { Tag } from './Tag';

export class VaultEmbedTag extends Tag {
    constructor() {
        super();
        this.Element = 'div';
        this.MainOption = 'id';
        this.Options = ['id'];
    }

    public override Matches(state: State, _token: string, context: TagParseContext): boolean {
        const peekTag = state.Peek(7);
        const pt = state.PeekTo(']');
        return context == TagParseContext.Block && peekTag == '[vault:' && pt?.length > 7 && !pt.includes('\n');
    }

    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    public override Parse(_parser: Parser, _data: ParseData, state: State, _scope: string, _context: TagParseContext): INode {
        const index = state.Index;

        if (state.ScanTo(':') != '[vault' || state.Next() != ':') {
            state.Seek(index, true);
            return null;
        }

        const str = state.ScanTo(']');
        if (state.Next() != ']') {
            state.Seek(index, true);
            return null;
        }

        const id = parseInt(str, 10);
        if (!id) {
            state.Seek(index, true);
            return null;
        }

        const classes = ['embedded', 'vault'];
        if (this.ElementClass != null) classes.push(this.ElementClass);

        state.SkipWhitespace();

        const before = `<div class="${classes.join(' ')}">` +
            '<div class="embed-container">' +
            '<div class="embed-content">' +
            `<div class="uninitialised" data-embed-type="vault" data-vault-id="${id}">` +
            `Loading embedded content: Vault Item #${id}`;
        const after = '</div></div></div></div>';
        const ret = new HtmlNode(before, PlainTextNode.Empty(), after);
        ret.PlainBefore = `[TWHL vault item #${id}]`;
        ret.PlainAfter = '\n';
        ret.IsBlockNode = true;
        return ret;
    }
}