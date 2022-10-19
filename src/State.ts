
export class State {

    public readonly Text: string;
    public get Length(): number {
        return this.Text.length;
    }
    public Index: number;
    public get Done(): boolean {
        return this.Index >= this.Length;
    }

    public constructor(text: string) {
        this.Text = text;
        this.Index = 0;
    }

    public ScanTo(find: string, ignoreCase = false): string {
        let pos = ignoreCase
            ? this.Text.toLowerCase().indexOf(find.toLowerCase(), this.Index)
            : this.Text.indexOf(find, this.Index);
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

    public GetToken(): string {
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
