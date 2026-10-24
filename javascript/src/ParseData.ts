export class ParseData {
    private _values: Record<string, any>;
    public CurrentElementDepth: number;
    public CurrentTagDepth: number;

    constructor() {
        this._values = {};
        this.CurrentElementDepth = 0;
        this.CurrentTagDepth = 0;
    }

    public Get<T>(key: string, defaultValue: () => T): T {
        if (this._values[key]) return this._values[key] as T;
        const v = defaultValue();
        this._values[key] = v;
        return v;
    }

    public Set<T>(key: string, value: T): void {
        this._values[key] = value;
    }
}
