import { Parser } from '..';
import { WikiRevisionCredit } from '../Models/WikiRevisionCredit';
import { INode } from '../Nodes/INode';
import { MetadataNode } from '../Nodes/MetadataNode';
import { ParseData } from '../ParseData';
import { State } from '../State';
import { TagParseContext } from '../TagParseContext';
import { Tag } from './Tag';

export class WikiCreditTag extends Tag {
    constructor() {
        super();
        this.Token = null;
        this.Element = '';
    }

    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    public override Matches(state: State, _token: string, _context: TagParseContext): boolean {
        const peekTag = state.Peek(8);
        const pt = state.PeekTo(']');
        return peekTag == '[credit:' && pt != null && pt.length > 8 && !pt.includes('\n');
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

        const credit = new WikiRevisionCredit();
        credit.Type = WikiRevisionCredit.TypeCredit;

        const sections = str.split('|');
        for (const section of sections) {
            const spl = section.split(':');
            const key = spl[0];
            const val = spl.length > 1 ? spl.slice(1).join(':') : '';
            switch (key) {
                case 'credit':
                    credit.Description = val;
                    break;
                case 'user':
                    credit.UserID = parseInt(val, 10) || null;
                    break;
                case 'name':
                    credit.Name = val;
                    break;
                case 'url':
                    credit.Url = val;
                    break;
            }
        }

        state.SkipWhitespace();
        return new MetadataNode('WikiCredit', credit);
    }
}
