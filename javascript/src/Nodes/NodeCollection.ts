import { INode } from './INode';

export class NodeCollection implements INode {
    public get Line(): number {
        return this.Nodes.length == 0 ? 0 : this.Nodes[0].Line;
    }
    public get Column(): number {
        return this.Nodes.length == 0 ? 0 : this.Nodes[0].Column;
    }
    public Nodes: INode[];

    constructor(...nodes: INode[]) {
        this.Nodes = Array.from(nodes);
    }

    ToHtml(): string {
        return this.Nodes.map(x => x.ToHtml()).join('');
    }
    ToPlainText(): string {
        return this.Nodes.map(x => x.ToPlainText()).join('');
    }
    GetChildren(): INode[] {
        return this.Nodes;
    }
    ReplaceChild(i: number, node: INode): void {
        this.Nodes[i] = node;
    }
    HasContent(): boolean {
        return this.Nodes.some(x => x.HasContent());
    }
}
