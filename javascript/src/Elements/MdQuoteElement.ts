import { Parser } from '..';
import { Lines } from '../Lines';
import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { ParseData } from '../ParseData';
import { Trim } from '../Util';
import { Element } from './Element';

export class MdQuoteElement extends Element {
    public Matches(lines: Lines): boolean {
        const value = lines.Value();
        return value.length > 0 && value.startsWith('>');
    }
    public Consume(parser: Parser, data: ParseData, lines: Lines, scope: string): INode | null {
        let value = lines.Value();
        const arr = [Trim(value.substring(1))];
        while (lines.Next()) {
            value = Trim(lines.Value());
            if (value.length == 0 || value[0] != '>') {
                lines.Back();
                break;
            }
            arr.push(Trim(value.substring(1)));
        }

        const text = Trim(arr.join('\n'));
        const ret = new HtmlNode('<blockquote>', parser.ParseElements(data, text, scope), '</blockquote>');
        ret.PlainBefore = '[quote]\n';
        ret.PlainAfter = '\n[/quote]';
        return ret;
    }
}
