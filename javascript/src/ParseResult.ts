import { INode } from './Nodes/INode';
import { MetadataNode } from './Nodes/MetadataNode';
import { NodeCollection } from './Nodes/NodeCollection';
import { NodeExtensions } from './Nodes/NodeExtensions';

export class ParseResult {
    public Content: INode;

    constructor() {
        this.Content = new NodeCollection();
    }

    public GetMetadata(): Array<{ Key: string; Value: any }> {
        const list: Array<{ Key: string; Value: any }> = [];
        NodeExtensions.Walk(this.Content, n => {
            if (n instanceof MetadataNode) list.push({ Key: n.Key, Value: n.Value });
            return true;
        });
        return list;
    }

    /**
     * Convert the result to HTML and sanitise it with a given callback.
     * Supplying a valid sanitiser is the responsibility of the caller.
     */
    public ToHtml(sanitiser: (x: string) => string): string {
        return sanitiser(this.Content.ToHtml());
    }

    /**
     * Convert the result to HTML without a sanitiser callback.
     * Its possible malicious input could include injection attacks. Use at your own risk.
     */
    public ToHtmlUnsanitised(): string {
        return this.Content.ToHtml();
    }

    /**
     * Convert the result to plain text.
     */
    public ToPlainText(): string {
        return this.Content.ToPlainText();
    }
}
