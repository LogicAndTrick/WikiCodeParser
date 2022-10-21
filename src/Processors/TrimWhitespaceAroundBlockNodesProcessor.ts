import { Parser } from '..';
import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { NodeCollection } from '../Nodes/NodeCollection';
import { PlainTextNode } from '../Nodes/PlainTextNode';
import { UnprocessablePlainTextNode } from '../Nodes/UnprocessablePlainTextNode';
import { ParseData } from '../ParseData';
import { INodeProcessor } from './INodeProcessor';

export class TrimWhitespaceAroundBlockNodesProcessor implements INodeProcessor {
    Priority = 20;

    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    ShouldProcess(node: INode, _scope: string): boolean {
        return node instanceof NodeCollection;
    }

    Process(parser: Parser, data: ParseData, node: INode, scope: string): INode[] {
        const coll = node as NodeCollection;

        const ret = [];

        let trimStart = false;
        for (let i = 0; i < coll.Nodes.length; i++) {
            let child = coll.Nodes[i];
            const next = i < coll.Nodes.length - 1 ? coll.Nodes[i + 1] : null;
            if (child instanceof PlainTextNode) {
                let text = child.Text;
                if (trimStart) text = text.trimStart();
                if (next instanceof HtmlNode && next.IsBlockNode) text = text.trimEnd();
                child.Text = text;
            }

            child = parser.RunProcessor(child, this, data, scope);

            if (child instanceof HtmlNode && child.IsBlockNode) {
                trimStart = true;
                ret.push(UnprocessablePlainTextNode.NewLine());
                ret.push(child);
                ret.push(UnprocessablePlainTextNode.NewLine());
            }
            else {
                trimStart = false;
                ret.push(child);
            }
        }

        return ret;
    }
}
