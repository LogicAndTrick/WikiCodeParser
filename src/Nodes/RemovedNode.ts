import { INode } from './INode';

export class RemovedNode implements INode {
    public OriginalNode : INode;

    constructor(originalNode : INode) {
        this.OriginalNode = originalNode;
    }

    ToHtml(): string {
        return '';
    }
    ToPlainText(): string {
        return '';
    }
    GetChildren(): INode[] {
        return [];
    }
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    ReplaceChild(_i: number, _node: INode): void {
        throw new Error('Unsupported operation.');
    }
    HasContent(): boolean {
        return false;
    }
}
