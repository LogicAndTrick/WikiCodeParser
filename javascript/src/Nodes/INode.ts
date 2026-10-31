export interface INode {
    /**
     * Convert the node to HTML. The resulting output may *not* be sanitised - its possible
     * malicious input could include injection attacks. Use at your own risk.
     */
    ToHtml(): string;
    ToPlainText(): string;
    GetChildren(): INode[];
    ReplaceChild(i: number, node: INode): void;
    HasContent(): boolean;
}
