import { INode } from './INode';

export class RemovedNode implements INode {
    public get Line(): number {
        return this.OriginalNode.Line;
    }
    public get Column(): number {
        return this.OriginalNode.Column;
    }
    public OriginalNode: INode;

    constructor(originalNode: INode) {
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

    ReplaceChild(_i: number, _node: INode): void {
        throw new Error('Unsupported operation.');
    }
    HasContent(): boolean {
        return false;
    }
}
