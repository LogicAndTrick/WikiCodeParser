import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { PlainTextNode } from '../Nodes/PlainTextNode';
import { UnprocessablePlainTextNode } from '../Nodes/UnprocessablePlainTextNode';
import { ParseData } from '../ParseData';
import { Parser } from '../Parser';
import { TrimEnd, TrimStart } from '../Util';
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

        const ret: INode[] = [];
        for (let i = 0; i < lines.length; i++) {
            const line = lines[i];
            ret.push(new PlainTextNode(line));
            // Don't emit a line break after the final line of the text as it did not end with a newline
            if (i < lines.length - 1) ret.push(new HtmlNode('<br/>', UnprocessablePlainTextNode.NewLine(), ''));
        }
        return ret;
    }
}
