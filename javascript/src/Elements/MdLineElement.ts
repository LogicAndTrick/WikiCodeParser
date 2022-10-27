import { Parser } from '..';
import { Lines } from '../Lines';
import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { PlainTextNode } from '../Nodes/PlainTextNode';
import { ParseData } from '../ParseData';
import { Element } from './Element';

export class MdLineElement extends Element {
    public Matches(lines: Lines): boolean {
        const value = lines.Value().trimEnd();
        return value.length >= 3 && value == '-'.repeat(value.length);
    }
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    public Consume(_parser: Parser, _data: ParseData, _lines: Lines, _scope: string): INode {
        return new HtmlNode('<hr />', PlainTextNode.Empty(), '');
    }
}
