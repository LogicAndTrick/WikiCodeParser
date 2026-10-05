import { INode } from '../Nodes/INode';
import { MetadataNode } from '../Nodes/MetadataNode';
import { ParseData } from '../ParseData';
import { Parser } from '../Parser';
import { State } from '../State';
import { TagParseContext } from '../TagParseContext';
import { Trim } from '../Util';
import { Tag } from './Tag';

export class WikiCategoryTag extends Tag {
    constructor() {
        super();
        this.Token = null;
        this.Element = '';
    }

    public override Matches(state: State, _token: string | null, _context: TagParseContext): boolean {
        const peekTag = state.Peek(5);
        const pt = state.PeekTo(']');
        return peekTag == '[cat:' && pt != null && pt.length > 5 && !pt.includes('\n');
    }

    public override Parse(_parser: Parser, _data: ParseData, state: State, _scope: string, _context: TagParseContext): INode | null {
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
        return new MetadataNode('WikiCategory', Trim(str));
    }
}
