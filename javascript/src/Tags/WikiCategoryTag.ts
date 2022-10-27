import { Parser } from '..';
import { INode } from '../Nodes/INode';
import { MetadataNode } from '../Nodes/MetadataNode';
import { ParseData } from '../ParseData';
import { State } from '../State';
import { TagParseContext } from '../TagParseContext';
import { Tag } from './Tag';

export class WikiCategoryTag extends Tag {
    constructor() {
        super();
        this.Token = null;
        this.Element = '';
    }

    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    public override Matches(state: State, _token: string, _context: TagParseContext): boolean {
        const peekTag = state.Peek(5);
        const pt = state.PeekTo(']');
        return peekTag == '[cat:' && pt != null && pt.length > 5 && !pt.includes('\n');
    }

    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    public override Parse(_parser: Parser, _data: ParseData, state: State, _scope: string, _context: TagParseContext): INode {
        const index = state.Index;
        if (state.ScanTo(':') != '[cat' || state.Next() != ':') {
            state.Seek(index, true);
            return null;
        }

        const str = state.ScanTo(']');
        if (state.Next() != ']') {
            state.Seek(index, true);
            return null;
        }

        state.SkipWhitespace();
        return new MetadataNode('WikiCategory', str.trim());
    }
}