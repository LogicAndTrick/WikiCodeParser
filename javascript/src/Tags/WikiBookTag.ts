import { WikiRevisionBook } from '../Models/WikiRevisionBook';
import { INode } from '../Nodes/INode';
import { MetadataNode } from '../Nodes/MetadataNode';
import { ParseData } from '../ParseData';
import { Parser } from '../Parser';
import { State } from '../State';
import { TagParseContext } from '../TagParseContext';
import { ParseIntStrict } from '../Util';
import { Tag } from './Tag';

export class WikiBookTag extends Tag {
    constructor() {
        super();
        this.Token = null;
        this.Element = '';
    }

    public override Matches(state: State, _token: string | null, _context: TagParseContext): boolean {
        const peekTag = state.Peek(6);
        const pt = state.PeekTo(']');
        return peekTag == '[book:' && pt != null && pt.length > 6 && !pt.includes('\n');
    }

    public override Parse(_parser: Parser, _data: ParseData, state: State, _scope: string, _context: TagParseContext): INode | null {
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

        const book = new WikiRevisionBook();

        const sections = str.split('|');
        for (const section of sections) {
            const spl = section.split(':');
            const key = spl[0];
            const val = spl.length > 1 ? spl.slice(1).join(':') : '';
            switch (key) {
                case 'book':
                    book.BookName = val;
                    break;
                case 'chapter':
                    book.ChapterName = val;
                    break;
                case 'chapternumber':
                    const cn = ParseIntStrict(val);
                    if (cn !== null && cn > 0) {
                        book.ChapterNumber = cn;
                    } else {
                        state.Seek(index, true);
                        return null;
                    }
                    break;
                case 'pagenumber':
                    const pn = ParseIntStrict(val);
                    if (pn !== null && pn > 0) {
                        book.PageNumber = pn;
                    } else {
                        state.Seek(index, true);
                        return null;
                    }
                    break;
            }
        }

        state.SkipWhitespace();
        return new MetadataNode('WikiBook', book);
    }
}
