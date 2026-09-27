import { INode } from './INode';

export class MetadataNode implements INode {
    public Key: string;
    public Value: any;

    constructor(key: string, value: any) {
        this.Key = key;
        this.Value = value;
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
        throw new Error('Invalid operation.');
    }
    HasContent(): boolean {
        return false;
    }
}
