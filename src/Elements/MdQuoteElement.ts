import { Parser } from '..';
import { Lines } from '../Lines';
import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { ParseData } from '../ParseData';
import { Element } from './Element';

export class MdQuoteElement extends Element {
    public Matches(lines: Lines): boolean {
        const value = lines.Value();
        return value.length > 0 && value.startsWith('>');
    }
    public Consume(parser: Parser, data: ParseData, lines: Lines, scope: string): INode {
        let value = lines.Value();
        const arr = [value.substring(1).trim()];
        while (lines.Next()) {
            value = lines.Value().trim();
            if (value.length == 0 || value[0] != '>') {
                lines.Back();
                break;
            }
            arr.push(value.substring(1).trim());
        }

        const text = arr.join('\n');
        return new HtmlNode('<blockquote>', parser.ParseElements(data, text, scope), '</blockquote>');
    }
}
