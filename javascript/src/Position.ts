export class Position {
    public static Zero(): Position {
        return new Position(0, 0);
    }

    public Line: number;
    public Column: number;

    constructor(line: number, column: number) {
        this.Line = line;
        this.Column = column;
    }
}
