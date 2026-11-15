import { Position } from '../Position';
import { INode } from './INode';

export class HtmlNode implements INode {
    public Line: number = 0;
    public Column: number = 0;

    public HtmlBefore: string;
    public Content: INode;
    public HtmlAfter: string;

    public PlainBefore: string;
    public PlainAfter: string;
    public IsBlockNode: boolean;

    constructor(pos: Position, htmlBefore: string, content: INode, htmlAfter: string) {
        this.Line = pos.Line;
        this.Column = pos.Column;
        this.HtmlBefore = htmlBefore;
        this.Content = content;
        this.HtmlAfter = htmlAfter;
        this.PlainBefore = this.PlainAfter = '';
        this.IsBlockNode = false;
    }

    ToHtml(): string {
        return this.HtmlBefore + this.Content.ToHtml() + this.HtmlAfter;
    }
    ToPlainText(): string {
        return this.PlainBefore + this.Content.ToPlainText() + this.PlainAfter;
    }
    GetChildren(): INode[] {
        return [this.Content];
    }
    ReplaceChild(i: number, node: INode): void {
        if (i !== 0) throw new Error('Index out of range');
        this.Content = node;
    }
    HasContent(): boolean {
        return true;
    }
}
