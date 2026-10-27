export function OrderBy<T>(array: T[], selector: (arg: T) => any) {
    const copy = Array.from(array);
    return copy.sort((a, b) => {
        const ka = selector(a);
        const kb = selector(b);
        if (ka < kb) return -1;
        if (ka > kb) return 1;
        return 0;
    });
}

export function OrderByDescending<T>(array: T[], selector: (arg: T) => any) {
    const copy = OrderBy(array, selector);
    return copy.reverse();
}

export function IndexOfAny(str: string, searchStrings: Iterable<string>, position = 0) {
    let min = -1;
    for (const searchString of searchStrings) {
        const idx = str.indexOf(searchString, position);
        if (idx >= 0) min = min < 0 ? idx : Math.min(min, idx);
    }
    return min;
}

export function Template(template_string: string, obj: any) {
    return template_string.replace(/\{(.*?)\}/gi, function (_match, name) {
        return obj[name] || '';
    });
}

// this was a regex before, but performance was terrible.
const TrimChars = new Set<string>([' ', '\t', '\n', '\r', '\0', '\x0B']);

export function TrimStart(str: string) {
    // find the index of the first char that isn't trimmable
    let i = 0;
    while (i < str.length && TrimChars.has(str[i])) i++;
    // slice off everything before that
    return str.slice(i);
}

export function TrimEnd(str: string) {
    // as above, backwards
    let i = str.length;
    while (i > 0 && TrimChars.has(str[i - 1])) i--;
    return str.slice(0, i);
}

export function Trim(str: string) {
    return TrimEnd(TrimStart(str));
}

export function ParseIntStrict(str: string): number | null {
    const match = /^-?[0-9]+$/.exec(str);
    if (!match) return null;
    const num = parseInt(str, 10);
    return isNaN(num) ? null : num;
}
