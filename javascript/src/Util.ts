
export function OrderBy<T>(array : T[], selector : (arg : T) => any) {
    const copy = Array.from(array);
    return copy.sort((a, b) => {
        const ka = selector(a);
        const kb = selector(b);
        if (ka < kb) return -1;
        if (ka > kb) return 1;
        return 0;
    });
}

export function OrderByDescending<T>(array : T[], selector : (arg : T) => any) {
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
    return template_string.replace(/\{(.*?)\}/ig, function (_match, name) {
        return obj[name] || '';
    });
}
