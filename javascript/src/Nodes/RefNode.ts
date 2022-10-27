import { ParseData } from '../ParseData';
import { INode } from './INode';
import { UnprocessablePlainTextNode } from './UnprocessablePlainTextNode';

export class RefNode implements INode {
    public Data : ParseData;
    public Name : string;

    constructor(data : ParseData, name : string) {
        this.Data = data;
        this.Name = name;
    }

    private GetNode() : INode {
        return this.Data.Get(`Ref::${this.Name}`, UnprocessablePlainTextNode.Empty);
    }

    ToHtml(): string {
        return this.GetNode().ToHtml();
    }
    ToPlainText(): string {
        return this.GetNode().ToPlainText();
    }
    GetChildren(): INode[] {
        return [this.GetNode() ];
    }
    ReplaceChild(i: number, node: INode): void {
        if (i != 0) throw new Error('Index out of range');
        this.Data.Set(`Ref::${this.Name}`, node);
    }
    HasContent(): boolean {
        return this.GetNode().HasContent();
    }
}
