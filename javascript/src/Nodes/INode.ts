export interface INode {
    /**
     * The index of the line (1-indexed) in the source input where this node starts.
     * If the line is 0, then the position is not set.
     */
    Line: number;
    /**
     * The index of the column (0-indexed) in the source input where this node starts.
     */
    Column: number;
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
