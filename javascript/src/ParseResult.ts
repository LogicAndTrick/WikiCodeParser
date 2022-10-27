import { INode } from './Nodes/INode';
import { MetadataNode } from './Nodes/MetadataNode';
import { NodeCollection } from './Nodes/NodeCollection';
import { NodeExtensions } from './Nodes/NodeExtensions';

export class ParseResult {
    public Content : INode;

    constructor() {
        this.Content = new NodeCollection();
    }

    public GetMetadata() : Array<{ Key: string, Value: any }> {
        const list : Array<{ Key: string, Value: any }> = [];
        NodeExtensions.Walk(this.Content, n =>
        {
            if (n instanceof MetadataNode) list.push({ Key: n.Key, Value: n.Value });
            return true;
        });
        return list;
    }

    public ToHtml() : string {
        return this.Content.ToHtml();
    }

    public ToPlainText() : string {
        return this.Content.ToPlainText();
    }
}