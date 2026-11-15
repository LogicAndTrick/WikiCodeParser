import { Position } from './Position';

export class State {
    public readonly Text: string;
    public get Length(): number {
        return this.Text.length;
    }
    public Index: number;
    public get Done(): boolean {
        return this.Index >= this.Length;
    }

    private readonly newlinePositions: Array<number>;
    private readonly startSourceLine: number;
    private readonly startSourceColumn: number;

    public constructor(startSourceLine: number, startSourceColumn: number, text: string) {
        this.Text = text;
        this.Index = 0;

        this.startSourceLine = startSourceLine;
        this.startSourceColumn = startSourceColumn;
        // get the positions of every newline character, so we can count them
        const nlpos: number[] = [-1]; // lines are 1-indexed, we always start with a newline
        let idx = text.indexOf('\n');
        while (idx >= 0) {
            nlpos.push(idx);
            idx = text.indexOf('\n', idx + 1);
        }
        this.newlinePositions = nlpos;
    }

    public get SourceLine(): number {
        return this.startSourceLine + this.newlinePositions.filter(x => x < this.Index).length - 1; // the input line is 1-indexed too, so subtract 1 here.
    }
    public get SourceColumn(): number {
        const last = (this.newlinePositions.filter(x => x < this.Index).pop() ?? 0) + 1;
        return this.Index - last + (last == 0 ? this.startSourceColumn : 0);
    }
    public get SourcePosition(): Position {
        return new Position(this.SourceLine, this.SourceColumn);
    }

    public ScanTo(find: string, ignoreCase = false): string {
        let pos: number;

        if (ignoreCase) {
            const escaped = find.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
            const regex = new RegExp(escaped, 'gi');
            regex.lastIndex = this.Index;
            const match = regex.exec(this.Text);
            pos = match ? match.index : this.Length;
        } else {
            pos = this.Text.indexOf(find, this.Index);
            if (pos < 0) pos = this.Length;
        }
        if (pos < 0) pos = this.Length;
        const ret = this.Text.substring(this.Index, pos);
        this.Index = pos;
        return ret;
    }

    public SkipWhitespace() {
        while (this.Index < this.Length && this.Text[this.Index].trim() == '') this.Index++;
    }

    public PeekTo(find: string): string | null {
        const pos = this.Text.indexOf(find, this.Index);
        if (pos < 0) return null;
        return this.Text.substring(this.Index, pos);
    }

    public Seek(index: number, fromStart: boolean) {
        this.Index = fromStart ? index : this.Index + index;
    }

    public Peek(count: number): string {
        if (this.Index + count > this.Length) count = this.Length - this.Index;
        return this.Text.substring(this.Index, this.Index + count);
    }

    public Next(): string {
        if (this.Index >= this.Length) return '\0';
        return this.Text[this.Index++];
    }

    public GetToken(): string | null {
        if (this.Done || this.Text[this.Index] != '[') return null;
        let found = false;
        let tok = '';
        for (let i = this.Index + 1; i < Math.min(this.Index + 10, this.Length); i++) {
            const c = this.Text[i];
            if (c == ' ' || c == '=' || c == ']') {
                found = tok.length > 0;
                break;
            }

            tok += c;
        }

        return found ? tok.toLowerCase() : null;
    }
}
