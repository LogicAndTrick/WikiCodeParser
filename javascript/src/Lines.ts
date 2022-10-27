
export class Lines {
    public Content : string[];
    public Index : number;

    constructor(content : string) {
        this.Content = content.split('\n');
        this.Index = -1;
    }

    public Back() {
        this.Index--;
    }
    public Next() : boolean {
        return ++this.Index < this.Content.length;
    }
    public Value() : string {
        return this.Content[this.Index];
    }
    public Current() : number {
        return this.Index;
    }
    public SetCurrent(index : number) {
        this.Index = index;
    }
}
