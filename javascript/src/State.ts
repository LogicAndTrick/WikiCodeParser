import { Position } from './Position';
import { IndexOfAny } from './Util';

export class State {
    public readonly Text: string;
    public get Length(): number {
        return this.Text.length;
    }
    private _index: number;
    public get Index(): number {
        return this._index;
    }
    public get Done(): boolean {
        return this._index >= this.Length;
    }

    private readonly newlinePositions: Array<number>;
    private readonly startSourceLine: number;
    private readonly startSourceColumn: number;

    public constructor(startSourceLine: number, startSourceColumn: number, text: string) {
        this.Text = text;
        this._index = 0;

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
        return this.startSourceLine + this.newlinePositions.filter(x => x < this._index).length - 1; // the input line is 1-indexed too, so subtract 1 here.
    }
    public get SourceColumn(): number {
        const last = (this.newlinePositions.filter(x => x < this._index).pop() ?? 0) + 1;
        return this._index - last + (last == 0 ? this.startSourceColumn : 0);
    }
    public get SourcePosition(): Position {
        return new Position(this.SourceLine, this.SourceColumn);
    }

    public ScanTo(find: string, ignoreCase = false): string {
        let pos: number;

        if (ignoreCase) {
            const escaped = find.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
            const regex = new RegExp(escaped, 'gi');
            regex.lastIndex = this._index;
            const match = regex.exec(this.Text);
            pos = match ? match.index : this.Length;
        } else {
            pos = this.Text.indexOf(find, this._index);
            if (pos < 0) pos = this.Length;
        }
        if (pos < 0) pos = this.Length;
        const ret = this.Text.substring(this._index, pos);
        this._index = pos;
        return ret;
    }

    public ScanToAny(find: string[], ignoreCase = false): string {
        let pos: number;

        if (ignoreCase) {
            const escaped = find.map(f => f.replace(/[.*+?^${}()|[\]\\]/g, '\\$&'));
            const regex = new RegExp(escaped.join('|'), 'gi');
            regex.lastIndex = this._index;
            const match = regex.exec(this.Text);
            pos = match ? match.index : this.Length;
        } else {
            pos = IndexOfAny(this.Text, find, this._index);
            if (pos < 0) pos = this.Length;
        }
        if (pos < 0) pos = this.Length;
        const ret = this.Text.substring(this._index, pos);
        this._index = pos;
        return ret;
    }

    public ScanToEnd(): string {
        const ret = this.Text.substring(this._index, this.Length);
        this._index = this.Length;
        return ret;
    }

    public SkipWhitespace() {
        while (this._index < this.Length && this.Text[this._index].trim() == '') this._index++;
    }

    public PeekTo(find: string): string | null {
        const pos = this.Text.indexOf(find, this._index);
        if (pos < 0) return null;
        return this.Text.substring(this._index, pos);
    }

    public PeekToAny(find: string[]): string | null {
        const pos = IndexOfAny(this.Text, find, this._index);
        if (pos < 0) return null;
        return this.Text.substring(this._index, pos);
    }

    public Seek(index: number, fromStart: boolean) {
        this._index = fromStart ? index : this._index + index;
    }

    public Peek(count: number): string {
        if (count < 0) {
            const start = Math.max(0, this._index + count);
            return this.Text.substring(start, this._index);
        } else {
            if (this._index + count > this.Length) count = this.Length - this._index;
            return this.Text.substring(this._index, this._index + count);
        }
    }

    public Next(): string {
        if (this._index >= this.Length) return '\0';
        return this.Text[this._index++];
    }

    public GetToken(): string | null {
        if (this.Done || this.Text[this._index] != '[') return null;
        let found = false;
        let tok = '';
        for (let i = this._index + 1; i < Math.min(this._index + 10, this.Length); i++) {
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
