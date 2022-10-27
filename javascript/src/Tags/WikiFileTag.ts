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

export class WikiFileTag extends Tag {
    constructor() {
        super();

    }
    private static GetTag(state: State): string {
        const peekTag = state.Peek(6);
        const pt = state.PeekTo(']');
        if (peekTag == '[file:' && pt?.length > 6 && !pt.includes('\n')) return 'file';
        return null;
    }

    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    public override Matches(state: State, _token: string, _context: TagParseContext): boolean {
        const tag = WikiFileTag.GetTag(state);
        return tag != null;
    }

    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    public override Parse(_parser: Parser, _data: ParseData, state: State, _scope: string, _context: TagParseContext): INode {
        const index = state.Index;

        const tag = WikiFileTag.GetTag(state);
        if (state.ScanTo(':') != `[${tag}` || state.Next() != ':') {
            state.Seek(index, true);
            return null;
        }

        const str = state.ScanTo(']');

        if (state.Next() != ']') {
            state.Seek(index, true);
            return null;
        }

        const match = str.match(/^([^#\]]+?)(?:\|([^\]]*?))?$/i);

        if (!match) {
            state.Seek(index, true);
            return null;
        }

        const page = match[1];
        const text = match[2]?.length > 0 ? match[2] : page;
        const slug = WikiRevision.CreateSlug(page);
        const url = HtmlHelper.AttributeEncode(`https://twhl.info/wiki/embed/${slug}`);
        const infoUrl = HtmlHelper.AttributeEncode(`https://twhl.info/wiki/embed-info/${slug}`);

        const before = `<span class="embedded-inline download" data-info="${infoUrl}"><a href="${url}"><span class="fa fa-download"></span> `;
        const after = '</a></span>';

        const content = new NodeCollection();
        content.Nodes.push(new MetadataNode('WikiUpload', page));
        content.Nodes.push(new PlainTextNode(text));

        return new HtmlNode(before, content, after);
    }
}
