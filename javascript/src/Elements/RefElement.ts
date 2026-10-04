import { Parser } from '..';
import { Lines } from '../Lines';
import { INode } from '../Nodes/INode';
import { PlainTextNode } from '../Nodes/PlainTextNode';
import { ParseData } from '../ParseData';
import { Trim, TrimEnd } from '../Util';
import { Element } from './Element';

export class RefElement extends Element {
    public Matches(lines: Lines): boolean {
        const value = Trim(lines.Value());
        return value.length > 4 && value.startsWith('[ref=') && value.match(/\[ref=[a-z0-9 ]+\]/i) != null;
    }
    public Consume(parser: Parser, data: ParseData, lines: Lines, scope: string): INode | null {
        const current = lines.Current();
        const arr: string[] = [];

        let line = Trim(lines.Value());
        const res = line.match(/\[ref=([a-z0-9 ]+)\]/i);
        if (!res) {
            lines.SetCurrent(current);
            return null;
        }

        line = line.substring(res[0].length);

        const name = res[1];

        if (line.endsWith('[/ref]')) {
            arr.push(line.substring(0, line.length - 6));
        } else {
            if (line.length > 0) arr.push(line);
            let found = false;
            while (lines.Next()) {
                const value = TrimEnd(lines.Value());
                if (value.endsWith('[/ref]')) {
                    const lastLine = value.substring(0, value.length - 6);
                    arr.push(lastLine);
                    found = true;
                    break;
                } else {
                    arr.push(value);
                }
            }

            if (!found || arr.length == 0) {
                lines.SetCurrent(current);
                return null;
            }
        }

        // Store the ref node
        const node = parser.ParseElements(data, Trim(arr.join('\n')), scope);
        data.Set(`Ref::${name}`, node);

        // Return nothing
        return PlainTextNode.Empty();
    }
}
