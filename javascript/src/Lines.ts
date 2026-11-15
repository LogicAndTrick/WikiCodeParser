import { Util } from '.';

/**
 * Line-based state tracker for a multi-line string
 */
export class Lines {
    public Content: string[];
    public Index: number;

    private readonly startSourceLine: number;
    public get SourceLine(): number {
        return this.startSourceLine + Math.max(0, this.Index);
    }

    constructor(startSourceLine: number, content: string) {
        this.Content = content.split('\n');
        this.Index = -1;
        this.startSourceLine = startSourceLine;
    }

    public Back() {
        this.Index--;
    }
    public Next(): boolean {
        return ++this.Index < this.Content.length;
    }
    public Value(): string {
        return this.Content[this.Index];
    }
    public Current(): number {
        return this.Index;
    }
    public SetCurrent(index: number) {
        this.Index = index;
    }
    public IsEmpty(): boolean {
        return Util.Trim(this.Value()).length === 0;
    }
}
