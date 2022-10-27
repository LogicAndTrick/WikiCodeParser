import { Parser } from '..';
import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { PlainTextNode } from '../Nodes/PlainTextNode';
import { UnprocessablePlainTextNode } from '../Nodes/UnprocessablePlainTextNode';
import { ParseData } from '../ParseData';
import { INodeProcessor } from './INodeProcessor';

export class NewLineProcessor implements INodeProcessor {
    public Priority = 1;

    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    ShouldProcess(node: INode, _scope: string): boolean {
        return node instanceof PlainTextNode && (node.Text.includes('\n') || node.Text.includes('<br>'));
    }

    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    Process(parser: Parser, data: ParseData, node: INode, _scope: string): INode[] {
        let text = (node as PlainTextNode).Text;
        text = text.replace(/ *<br> */g, '\n');

        const ret : INode[] = [];
        const lines = text.split('\n');
        for (let i = 0; i < lines.length; i++)
        {
            const line = lines[i];
            ret.push(new PlainTextNode(line));
            // Don't emit a line break after the final line of the text as it did not end with a newline
            if (i < lines.length - 1) ret.push(new HtmlNode('<br/>', UnprocessablePlainTextNode.NewLine(), ''));
        }
        return ret;
    }
}
