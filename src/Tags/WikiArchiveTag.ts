import { Parser } from '..';
import { WikiRevisionCredit } from '../Models/WikiRevisionCredit';
import { INode } from '../Nodes/INode';
import { MetadataNode } from '../Nodes/MetadataNode';
import { ParseData } from '../ParseData';
import { State } from '../State';
import { TagParseContext } from '../TagParseContext';
import { Tag } from './Tag';

export class WikiArchiveTag extends Tag {
    constructor() {
        super();
        this.Token = null;
        this.Element = '';
    }

    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    public override Matches(state: State, _token: string, _context: TagParseContext): boolean {
        const peekTag = state.Peek(9);
        const pt = state.PeekTo(']');
        return peekTag == '[archive:' && pt != null && pt.length > 8 && !pt.includes('\n');
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
        credit.Type = WikiRevisionCredit.TypeArchive;

        const sections = str.split('|');
        for (const section of sections) {
            const spl = section.split(':');
            const key = spl[0];
            const val = spl.length > 1 ? spl.slice(1).join(':') : '';
            switch (key) {
                case 'archive':
                    credit.Name = val;
                    break;
                case 'description':
                    credit.Description = val;
                    break;
                case 'url':
                    credit.Url = val;
                    break;
                case 'wayback':
                    credit.WaybackUrl = val;
                    break;
                case 'full':
                    credit.Type = WikiRevisionCredit.TypeFull;
                    break;
            }
        }
        if (credit.WaybackUrl != null && credit.Url != null && !credit.WaybackUrl.startsWith('http://') && !credit.WaybackUrl.startsWith('https://') && parseInt(credit.WaybackUrl, 10)) {
            credit.WaybackUrl = `https://web.archive.org/web/${credit.WaybackUrl}/${credit.Url}`;
        }

        state.SkipWhitespace();
        return new MetadataNode('WikiCredit', credit);
    }
}
