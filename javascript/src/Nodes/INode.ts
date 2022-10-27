
export interface INode {
    ToHtml() : string;
    ToPlainText() : string;
    GetChildren() : INode[];
    ReplaceChild(i : number, node : INode) : void;
    HasContent() : boolean;
}
