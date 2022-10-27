
export class ParseData {
    private _values : Record<string, any>;

    constructor() {
        this._values = {};
    }

    public Get<T>(key : string, defaultValue : () => T) : T {
        if (this._values[key]) return this._values[key];
        const v = defaultValue();
        this._values[key] = v;
        return v;
    }

    public Set<T>(key : string, value : T) : void {
        this._values[key] = value;
    }
}
