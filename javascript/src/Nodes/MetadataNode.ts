import { Position } from '../Position';
import { INode } from './INode';

export class MetadataNode implements INode {
    public readonly Line: number = 0;
    public readonly Column: number = 0;
    public Key: string;
    public Value: any;

    constructor(pos: Position, key: string, value: any) {
        this.Line = pos.Line;
        this.Column = pos.Column;
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
