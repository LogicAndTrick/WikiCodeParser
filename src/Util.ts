
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
