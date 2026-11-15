import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { PlainTextNode } from '../Nodes/PlainTextNode';
import { UnprocessablePlainTextNode } from '../Nodes/UnprocessablePlainTextNode';
import { ParseData } from '../ParseData';
import { Parser } from '../Parser';
import { Position } from '../Position';
import { Trim, TrimEnd, TrimStart } from '../Util';
import { INodeProcessor } from './INodeProcessor';

export class NewLineProcessor implements INodeProcessor {
    public Priority = 1;

    ShouldProcess(node: INode, _scope: string): boolean {
        return node instanceof PlainTextNode && (node.Text.includes('\n') || node.Text.includes('<br>'));
    }

    Process(_parser: Parser, _data: ParseData, parentNode: INode | null, node: INode, _scope: string): INode[] {
        const text = (node as PlainTextNode).Text;

        const lines: string[] = [];
        for (const line of text.split('\n')) {
            const breaks = line.split('<br>');
            for (let i = 0; i < breaks.length; i++) {
                let s = breaks[i];
                if (i > 0) s = TrimStart(s);
                if (i < breaks.length - 1) s = TrimEnd(s);
                lines.push(s);
            }
        }

        // track the number of sequential newlines, to collapse 3+ newlines into 2 newlines
        let numSequentialBreaks = 0;

        const ret: INode[] = [];
        for (let i = 0; i < lines.length; i++) {
            const line = lines[i];
            if (line && Trim(line) !== '') {
                ret.push(new PlainTextNode(Position.Zero(), line));
                numSequentialBreaks = 0;
            }

            // Don't emit a line break after the final line of the text as it did not end with a newline
            if (i >= lines.length - 1) continue;

            numSequentialBreaks++;
            if (numSequentialBreaks > 2) continue;

            ret.push(new HtmlNode(Position.Zero(), '<br/>', UnprocessablePlainTextNode.NewLine(Position.Zero()), ''));
        }
        return ret;
    }
}
