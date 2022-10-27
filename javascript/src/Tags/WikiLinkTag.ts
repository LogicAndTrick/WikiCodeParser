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

export class WikiLinkTag extends Tag {
    constructor() {
        super();
        this.Token = null;
    }

    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    public override Matches(state: State, _token: string, _context: TagParseContext): boolean {
        const pt = state.PeekTo(']]');
        return pt?.length > 1 && pt[1] == '[' && !pt.includes('\n')
            && pt.substring(2).match(/([^\]]*?)(?:\|([^\]]*?))?/i) != null;
    }

    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    public override Parse(_parser: Parser, _data: ParseData, state: State, _scope: string, _context: TagParseContext): INode {
        const index = state.Index;

        if (state.Next() != '[' || state.Next() != '[') {
            state.Seek(index, true);
            return null;
        }

        const str = state.ScanTo(']]');

        if (state.Next() != ']' || state.Next() != ']') {
            state.Seek(index, true);
            return null;
        }

        const match = str.match(/^([^\]]+?)(?:\|([^\]]*?))?$/i);
        if (!match) {
            state.Seek(index, true);
            return null;
        }

        let page = match[1];
        const text = match[2] ? match[2] : page;
        let hash = '';
        if (page.includes('#')) {
            const spl = page.split('#');
            page = spl[0];
            const anchor = spl.length > 1 ? spl.slice(1).join('#') : '';
            hash = '#' + anchor.replace(/[^\da-z?/:@\-._~!$&'()*+,;=]/ig, '_');
        }

        const url = HtmlHelper.AttributeEncode(`https://twhl.info/wiki/page/${WikiRevision.CreateSlug(page)}`) + hash;
        const before = `<a href="${url}">`;
        const after = '</a>';

        const content = new NodeCollection();
        content.Nodes.push(new MetadataNode('WikiLink', page));
        content.Nodes.push(new PlainTextNode(text));

        return new HtmlNode(before, content, after);
    }
}
