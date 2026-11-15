import { Lines } from '../Lines';
import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { PlainTextNode } from '../Nodes/PlainTextNode';
import { ParseData } from '../ParseData';
import { Parser } from '../Parser';
import { Position } from '../Position';
import { TrimEnd } from '../Util';
import { Element } from './Element';

export class MdLineElement extends Element {
    public Matches(lines: Lines): boolean {
        const value = TrimEnd(lines.Value());
        return value.length >= 3 && value == '-'.repeat(value.length);
    }

    public Consume(_parser: Parser, _data: ParseData, _lines: Lines, _scope: string): INode | null {
        const ret = new HtmlNode(Position.Zero(), '<hr />', PlainTextNode.Empty(Position.Zero()), '');
        ret.PlainBefore = '---';
        return ret;
    }
}
